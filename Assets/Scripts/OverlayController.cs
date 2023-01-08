using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Pincushion.LD52
{
    public class OverlayController : MonoBehaviour
    {
        public SceneController Scene;

        private UIDocument uiDocument;
        private VisualElement root;

        public VisualTreeAsset tutAsset;



        // Working variables (for open dialogues
        public InventoryIngredientData WorkingStallInventory; // for transactions
        private List<InventoryIngredientData> WorkingCookpotIngredients = new List<InventoryIngredientData>(); // for cooking

        private EventCallback<ClickEvent> _VisitRestaurantCallback;

        // Pause Menu
        private VisualElement pauseMenu;
        private Slider masterVolume;

        // Chat


        // Messages
        private TextElement messageText;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;


            root.Q("ToMarketButton").RegisterCallback<ClickEvent>(e => Scene.VisitMarket());

            _VisitRestaurantCallback = e => Scene.VisitRestaurant();
            root.Q("ToRestaurantButton").RegisterCallback<ClickEvent>(_VisitRestaurantCallback);

            root.Q("EndTurnButton").RegisterCallback<ClickEvent>(e => Scene.EndTurn());

            // Transaction
            root.Q("QuantityLessButton").RegisterCallback<ClickEvent>(e => TransactionIngredientLess());
            root.Q("QuantityMoreButton").RegisterCallback<ClickEvent>(e => TransactionIngredientMore());

            root.Q("BuyButton").RegisterCallback<ClickEvent>(e => HideTransactionWindowWithPurchase());
            root.Q("CancelButton").RegisterCallback<ClickEvent>(e => HideTransactionWindow());

            // Cooking
            root.Q("CookButton").RegisterCallback<ClickEvent>(e => Cook());

            // End turn
            root.Q("TurnSummaryContinueButton").RegisterCallback<ClickEvent>(e => HideEndTurnWindow());

            // Tutorial
            root.Q("SkipTutorialButton").RegisterCallback<ClickEvent>(e => EndTutorial());
            root.Q("EndTutorialButton").RegisterCallback<ClickEvent>(e => EndTutorial());
        }

        public void Init(SceneController scene)
        {
            Scene = scene;
        }

        private void Start()
        {
            HideTransactionWindow();
            UpdateOverlayMode();
            UpdatePoints();
        }
        private void Update()
        {
            if (!Scene.Paused)
            {
                
            }
        }

        public void UpdateOverlayMode()
        {
            if (Scene.GameMode == GameMode.Market)
            {
                root.Q<VisualElement>("RestaurantContainer").style.display = DisplayStyle.None;
                root.Q<VisualElement>("MarketContainer").style.display = DisplayStyle.Flex;

                if (Scene.MarketMode == MarketMode.Stall)
                {
                    root.Q<VisualElement>("Chat").style.display = DisplayStyle.Flex;
                }
                else if (Scene.MarketMode == MarketMode.Market)
                {
                    root.Q<VisualElement>("Chat").style.display = DisplayStyle.None;
                }
            }
            else
            {
                root.Q<VisualElement>("MarketContainer").style.display = DisplayStyle.None;
                root.Q<VisualElement>("RestaurantContainer").style.display = DisplayStyle.Flex;

                // Update inventory and other restaurant stuff
                UpdateRestaurantInventory();
                ResetCookpot();
                UpdateCookButtonVisibility();
            }
        }

        public void UpdatePoints()
        {
            SetElementText("Turn", Scene.Turn.ToString());
            SetElementText("Upkeep", Scene.Upkeep.ToString());
            SetElementText("Points", Scene.Points.ToString());
            SetElementText("Money", Scene.Money.ToString());
            //SetElementText("RotSpeed", Scene.RotSpeed.ToString());
        }

        public void SetChatTitle(string title)
        {
            TextElement label = root.Q<TextElement>("ChatTitle");
            label.text = title;
        }

        public void SetChatText(string text)
        {
            TextElement label = root.Q<TextElement>("ChatText");
            label.text = text;
        }

        public void SetChatOptions(string[] options)
        {
            VisualTreeAsset chatItemTemplate = Resources.Load<VisualTreeAsset>("UI/ChatSelectItem");

            VisualElement container = root.Q<VisualElement>("ChatItems");

            container.Clear();

            VisualElement chatItem = chatItemTemplate.Instantiate();

            Button chatItemButton = chatItem.Q<Button>("ChatItemText");
            chatItemButton.text = options[0];

            chatItemButton.RegisterCallback<ClickEvent>(e => Scene.VisitMarket());
            container.Add(chatItem);
        }

        public void SetInventoryItems(InventoryIngredientData[] inventory)
        {
            VisualTreeAsset inventoryItemTemplate = Resources.Load<VisualTreeAsset>("UI/InventoryItem");

            VisualElement container = root.Q<VisualElement>("InventoryItems");

            container.Clear();
            
            foreach (InventoryIngredientData data in inventory)
            {
                VisualElement item = inventoryItemTemplate.Instantiate();

                TextElement textElement = item.Q<TextElement>("Text");
                textElement.text = data.Ingredient.Name + "\nQuality: " + data.Quality + "\nTaste: " + data.BasePoints + "\nQuantity: " + data.Quantity;

                VisualElement imageElement = item.Q<VisualElement>("Image");
                Texture2D image = Resources.Load<Texture2D>(data.Ingredient.Image);
                StyleBackground styleBackground = new StyleBackground(image);
                imageElement.style.backgroundImage = styleBackground;


                Toggle toggle = item.Q<Toggle>("Toggle");
                toggle.RegisterCallback<ChangeEvent<bool>>(e => InventoryItemToggled(e, data));


                container.Add(item);
            }
        }
        public void UpdateRestaurantInventory()
        {
            InventoryIngredientData[] playerInventory = Scene.PlayerInventory.Ingredients;
            SetInventoryItems(playerInventory);
        }

        public void UpdateInventory()
        {
            UpdatePlayerMarketModeInventory();
            UpdateRestaurantInventory();
        }


        public void InventoryItemToggled(ChangeEvent<bool> e, InventoryIngredientData data)
        {
            e.StopPropagation();
            VisualElement container = root.Q<VisualElement>("CookIngredients");

            if (e.newValue)
            {
                // add to the cookpot
                if (WorkingCookpotIngredients.Count < 4)
                {
                    WorkingCookpotIngredients.Add(data);

                    VisualTreeAsset cookItemTemplate = Resources.Load<VisualTreeAsset>("UI/CookItem");
                    VisualElement item = cookItemTemplate.Instantiate();

                    TextElement textElement = item.Q<TextElement>("Text");
                    textElement.text = data.Ingredient.Name + "\nQuality: " + data.Quality + "\nTaste: " + data.BasePoints;

                    VisualElement imageElement = item.Q<VisualElement>("Image");
                    Texture2D image = Resources.Load<Texture2D>(data.Ingredient.Image);
                    StyleBackground styleBackground = new StyleBackground(image);
                    imageElement.style.backgroundImage = styleBackground;

                    container.Add(item);

                    //TODO allow the player to remove items from the cookpot directly
                }
                else
                {
                    // unset the value
                    ((Toggle)e.target).value = false;
                }
            }
            else if(WorkingCookpotIngredients.Contains(data))
            {
                // remove from the cookpot

                for (int i = 0; i < WorkingCookpotIngredients.Count; i++)
                {
                    InventoryIngredientData cookpotIngredient = WorkingCookpotIngredients[i];
                    if (cookpotIngredient == data)
                    {
                        container.RemoveAt(i);
                        WorkingCookpotIngredients.Remove(data);
                        break;
                    }
                }

            }
            UpdateCookpotMoney();
            UpdateCookButtonVisibility();
        }

        private void UpdateCookButtonVisibility()
        {
            root.Q<VisualElement>("CookButton").style.display = WorkingCookpotIngredients.Count > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateCookpotMoney()
        {
            if (WorkingCookpotIngredients.Count > 1)
            {
                List<ComboData> combos = new List<ComboData>();

                // Add points
                int points = Scene.Restaurant.CalculatePoints(WorkingCookpotIngredients.ToArray(), ref combos);

                string comboText = ".";
                if (combos.Count > 0)
                {
                    float multiplier = 1;
                    foreach (ComboData combo in combos)
                    {
                        multiplier *= combo.BonusMultiplier;
                    }
                    comboText = " with a " + multiplier + "x bonus from combos!";
                }

                SetElementText("CooktopMoney", "This recipe will make " + points + " money" + comboText);
            }
            else
            {
                SetElementText("CooktopMoney", "");
            }
        }

        private void Cook()
        {
            if (WorkingCookpotIngredients.Count > 1)
            {
                List<ComboData> combos = new List<ComboData>();

                // Add points
                int points = Scene.Restaurant.CalculatePoints(WorkingCookpotIngredients.ToArray(), ref combos);
                Scene.Points += points;
                Scene.Money += points;
                Scene.TotalIngredientsUsed += WorkingCookpotIngredients.Count;
                Scene.Combos += combos.Count;

                Scene.TurnPoints += points;
                Scene.TurnMoney += points;
                Scene.TurnIngredientsUsed += WorkingCookpotIngredients.Count;
                Scene.TurnCombos += combos.Count;

                UpdatePoints();
                Scene.Restaurant.RemoveCombos(combos);
                UpdateCombos();

                // Remove the items from the player's inventory
                foreach (InventoryIngredientData ingredient in WorkingCookpotIngredients)
                {
                    ingredient.Quantity -= 1;

                    // Remove the ingredient from the player's inventory, if there are no more
                    if (ingredient.Quantity < 1)
                    {
                        Scene.PlayerInventory.RemoveIngredient(ingredient);
                    }
                }

                // Give the player feedback - play a sound
                Scene.Sound.PlaySound("Payment");

                // Clear the cookpot
                ResetCookpot();

                // Refresh the inventory
                SetInventoryItems(Scene.PlayerInventory.Ingredients.ToArray());
            }
            else
            {
                //TODO give player feedback if there aren't enough ingredients OR disable the button until there are enough
            }
        }

        private void ResetCookpot()
        {
            InventoryIngredientData[] inventoryIngredientData = Scene.PlayerInventory.Ingredients;
            VisualElement inventoryContainer = root.Q<VisualElement>("InventoryItems");

            // untoggle all inventory items
            for (int i = 0; i < inventoryIngredientData.Length; i++)
            {
                InventoryIngredientData inventoryIngredient = inventoryIngredientData[i];
                if (WorkingCookpotIngredients.Contains(inventoryIngredient))
                {
                    inventoryContainer[i].Q<Toggle>("Toggle").value = false;
                }
            }

            // Clear the ingredients
            WorkingCookpotIngredients.Clear();

            VisualElement container = root.Q<VisualElement>("CookIngredients");
            container.Clear();

            UpdateCookpotMoney();
        }
        /*
        private void UpdateInventoryItems()
        {
            InventoryIngredientData[] inventoryIngredientData = Scene.PlayerInventory.Ingredients;
            VisualElement inventoryContainer = root.Q<VisualElement>("InventoryItems");


            // Ensure that all cookpot ingredients are selected, and their similar ingredients are disabled.
            for (int i = 0; i < inventoryIngredientData.Length; i++)
            {
                InventoryIngredientData inventoryIngredient = inventoryIngredientData[i];
                if (WorkingCookpotIngredients.Contains(inventoryIngredient))
                {
                    inventoryContainer[i].Q<Toggle>("Toggle").value = true;
                }
            }
        }*/

        public void UpdateCombos()
        {
            // Current Combos
            List<ComboData> currentCombos = Scene.Restaurant.CurrentCombos;
            VisualElement container = root.Q<VisualElement>("CurrentCombos");
            container.Clear();

            for (int i = 0; i < currentCombos.Count; i++)
            {
                ComboData combo = currentCombos[i];

                VisualTreeAsset itemTemplate = Resources.Load<VisualTreeAsset>("UI/ComboItem");
                VisualElement item = itemTemplate.Instantiate();


                string moreTurn = combo.Harvests > 1 ? " turns left" : " turn left";
                TextElement textElement = item.Q<TextElement>("Description");
                textElement.text = combo.BonusMultiplier + "x bonus\n" + combo.Harvests + moreTurn;

                int ingredientNum = 1;
                foreach (IngredientData ingredient in combo.Ingredients)
                {
                    VisualElement imageElement = item.Q<VisualElement>("Ingredient" + ingredientNum++);
                    Texture2D image = Resources.Load<Texture2D>(ingredient.Image);
                    StyleBackground styleBackground = new StyleBackground(image);
                    imageElement.style.backgroundImage = styleBackground;
                }

                container.Add(item);
            }

            // Next Combos
            List<ComboData> nextCombos = Scene.Restaurant.NextCombos;
            container = root.Q<VisualElement>("ComingCombos");
            container.Clear();

            for (int i = 0; i < nextCombos.Count; i++)
            {
                ComboData combo = nextCombos[i];

                VisualTreeAsset itemTemplate = Resources.Load<VisualTreeAsset>("UI/ComboItem");
                VisualElement item = itemTemplate.Instantiate();

                TextElement textElement = item.Q<TextElement>("Description");
                textElement.text = combo.BonusMultiplier + "x bonus";

                int ingredientNum = 1;
                foreach (IngredientData ingredient in combo.Ingredients)
                {
                    VisualElement imageElement = item.Q<VisualElement>("Ingredient" + ingredientNum++);
                    Texture2D image = Resources.Load<Texture2D>(ingredient.Image);
                    StyleBackground styleBackground = new StyleBackground(image);
                    imageElement.style.backgroundImage = styleBackground;
                }

                container.Add(item);
            }
        }

        public void TransactionIngredientLess()
        {
            TextField field = root.Q<TextField>("QuantityInput");
            int currentAmount = int.Parse(field.value);
            if (currentAmount > 0)
            {
                field.value = (currentAmount - 1).ToString();
            }
            UpdateTransactionPurchasePrice();
        }
        public void TransactionIngredientMore()
        {
            TextField field = root.Q<TextField>("QuantityInput");
            int currentAmount = int.Parse(field.value);
            if (currentAmount < WorkingStallInventory.Quantity)
            {
                field.value = (currentAmount + 1).ToString();
            }
            UpdateTransactionPurchasePrice();
        }

        public void UpdateTransactionPurchasePrice()
        {
            int quantiyToPurchase = int.Parse(root.Q<TextField>("QuantityInput").value);
            SetElementText("TransactionTotalCostText", Scene.Market.GetPurchasePrice(WorkingStallInventory, quantiyToPurchase).ToString());
        }

        public void ShowTransactionWindow(InventoryIngredientData inventory)
        {
            Scene.Paused = true;

            root.Q<VisualElement>("TransactionContainer").style.display = DisplayStyle.Flex;

            WorkingStallInventory = inventory;
            SetElementText("TransactionItemNameText", inventory.Ingredient.Name);
            SetElementText("TransactionQuantityAvailableText", inventory.Quantity.ToString());
            SetElementText("TransactionQualityText", inventory.Quality.ToString() + " / 10");
            SetElementText("TransactionYumText", inventory.BasePoints.ToString());
            SetElementText("TransactionItemCostText", Scene.Market.GetPurchasePrice(inventory, 1).ToString());


            VisualElement imageElement = root.Q<VisualElement>("TransactionProductImage");
            Texture2D image = Resources.Load<Texture2D>(inventory.Ingredient.Image);
            StyleBackground styleBackground = new StyleBackground(image);
            imageElement.style.backgroundImage = styleBackground;


            // set default quantity
            root.Q<TextField>("QuantityInput").value = inventory.Quantity == 0? "0" : "1";

            UpdateTransactionPurchasePrice();


            if (inventory.Quality <= 6)
            {
                SetChatText("They're ripe and they're cheap! They won't be around tomorrow! How many would you like?");
            }
            else if (inventory.Quality > 9)
            {
                SetChatText("Newly harvested. We sell only the finest ingredients!");
            }
            else if (inventory.Quality > 6)
            {
                SetChatText("How many would you like?");
            }
            
        }

        public void HideTransactionWindowWithPurchase()
        {
            // Add ingredients to inventory
            int quantiyToPurchase = int.Parse(root.Q<TextField>("QuantityInput").value);
            int costToPurchase = Scene.Market.GetPurchasePrice(WorkingStallInventory, quantiyToPurchase);

            if (quantiyToPurchase <= 0 || quantiyToPurchase > WorkingStallInventory.Quantity)
            {
                //TODO give feedback for not enough quantity

                if (quantiyToPurchase > WorkingStallInventory.Quantity)
                {
                    SetChatText("Sorry, we don't have that many.");
                    Scene.Sound.PlaySound("No");
                }
            }
            else if (Scene.Money < costToPurchase)
            {
                //TODO give feedback for not enough money
                SetChatText("You don't have enough money. Come back when you do!");
                Scene.Sound.PlaySound("No");
            }
            else
            {
                // perform the transaction
                WorkingStallInventory.Quantity -= quantiyToPurchase;

                InventoryIngredientData playerInventory = Scene.PlayerInventory.GetIngredient(WorkingStallInventory.Ingredient, WorkingStallInventory.Quality);

                if (playerInventory == null)
                {
                    Scene.PlayerInventory.SetIngredient(WorkingStallInventory.Ingredient, quantiyToPurchase, WorkingStallInventory.Quality);
                }
                else
                {
                    playerInventory.Quantity += quantiyToPurchase;
                }

                // Charge the player
                Scene.Money -= costToPurchase;

                // Update the turn stats
                Scene.TurnIngredientsPurchased += quantiyToPurchase;
                Scene.TurnMoneySpent += costToPurchase;

                Scene.TotalMoneySpent += costToPurchase;
                Scene.TotalIngredientsPurchased += quantiyToPurchase;

                UpdatePoints();

                SetChatText("Thank you for your business.");
                Scene.Sound.PlaySound("Payment");
            }

            // Hide the transaction window
            // Not calling HideTransactionWindow because it changes the chat
            root.Q<VisualElement>("TransactionContainer").style.display = DisplayStyle.None;
            WorkingStallInventory = null;
            Scene.Paused = false;
        }

        public void HideTransactionWindow()
        {
            SetChatText("Have a nice day!");

            root.Q<VisualElement>("TransactionContainer").style.display = DisplayStyle.None;
            WorkingStallInventory = null;
            Scene.Paused = false;
        }


        public void ShowEndTurnWindow()
        {
            Scene.Paused = true;

            root.Q<VisualElement>("TurnSummaryContainer").style.display = DisplayStyle.Flex;

            SetElementText("TurnSummaryTurn", Scene.Turn.ToString());
            SetElementText("TurnSummaryIngredientsPurchased", Scene.TurnIngredientsPurchased.ToString());
            SetElementText("TurnSummaryIngredientsUsed", Scene.TurnIngredientsUsed.ToString());
            SetElementText("TurnSummaryYum", Scene.TurnPoints.ToString());
            SetElementText("TurnSummaryMoney", Scene.TurnMoney.ToString());
            SetElementText("TurnSummaryMoneySpent", Scene.TurnMoneySpent.ToString());
        }
        public void HideEndTurnWindow()
        {
            root.Q<VisualElement>("TurnSummaryContainer").style.display = DisplayStyle.None;
            Scene.Paused = false;
        }

        public void UpdatePlayerMarketModeInventory()
        {
            //TODO
        }

        private void SetElementText(string id, string text)
        {
            root.Q<TextElement>(id).text = text;
        }

        public void VolumeChanged(ChangeEvent<float> e)
        {
            AudioListener.volume = e.newValue;
        }
        /*public void MusicVolumeChanged(float value)
        {
            AudioSource music = overlay.scene.Sound.GetAudioSource("Music");
            music.volume = musicVolumeSlider.value;
        }*/

        public void ResumeClicked(ClickEvent e)
        {
            ShowPauseMenu(false);
        }
        public void ExitClicked(ClickEvent e)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


        public void ShowPauseMenu(bool show)
        {
            if (show)
            {
                root.Q("PauseMenu").style.display = DisplayStyle.Flex;
                Scene.Paused = true;
            }
            else
            {
                root.Q("PauseMenu").style.display = DisplayStyle.None;
                Scene.Paused = false;
            }
        }

        public void ShowTutorial()
        {
            root.Q("Tutorial").style.display = DisplayStyle.Flex;
            root.Q("Tut0").style.display = DisplayStyle.Flex;
            root.Q("Tut1").style.display = DisplayStyle.None;
            Scene.Paused = true;

            // swap callbacks for the tutorial
            root.Q("ToRestaurantButton").UnregisterCallback(_VisitRestaurantCallback);

            _VisitRestaurantCallback = e => RestaurantTutorial();
            root.Q("ToRestaurantButton").RegisterCallback<ClickEvent>(_VisitRestaurantCallback);
        }

        public void RestaurantTutorial()
        {
            root.Q("Tutorial").style.display = DisplayStyle.Flex;
            root.Q("Tut0").style.display = DisplayStyle.None;
            root.Q("Tut1").style.display = DisplayStyle.Flex;

            Scene.VisitRestaurant();
        }

        public void EndTutorial()
        {
            root.Q("Tutorial").style.display = DisplayStyle.None;
            root.Q("Tut0").style.display = DisplayStyle.None;
            root.Q("Tut1").style.display = DisplayStyle.None;

            // reset the Restaurant handler
            root.Q("ToRestaurantButton").UnregisterCallback(_VisitRestaurantCallback);
            root.Q("ToRestaurantButton").RegisterCallback<ClickEvent>(e => Scene.VisitRestaurant());

            Scene.Paused = false;
        }

        public void MessageOkClicked(ClickEvent e, VisualElement panel)
        {
            if (e.propagationPhase != PropagationPhase.AtTarget)
            {
                return;
            }
            VisualElement element = panel;// e.target as VisualElement;

            if (element.name == "Tut0")
            {
                element.style.display = DisplayStyle.None;
                root.Q("Controls").style.display = DisplayStyle.Flex;
            }
            else // messages
            {
                element.style.display = DisplayStyle.None;
                Scene.Paused = false;
            }
        }


        public void LoseConditionMessage()
        {
            root.Q<VisualElement>("LoseConditionContainer").style.display = DisplayStyle.Flex;
            Scene.Paused = true;

            SetElementText("TotalSummaryTurns", Scene.Turn.ToString());
            SetElementText("TotalSummaryIngredientsPurchased", Scene.TotalIngredientsPurchased.ToString());
            SetElementText("TotalSummaryIngredientsUsed", Scene.TotalIngredientsUsed.ToString());
            SetElementText("TotalSummaryYum", Scene.Points.ToString());
            SetElementText("TotalSummaryMoney", Scene.Money.ToString());
            SetElementText("TotalSummaryMoneySpent", Scene.TotalMoneySpent.ToString());
        }
    }
}