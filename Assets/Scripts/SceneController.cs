using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD52
{
    public class SceneController : MonoBehaviour
    {
        public PlayerController Player;
        public SoundController Sound;
        public OverlayController Overlay;
        public MarketController Market;
        public RestaurantController Restaurant;
        public CameraController Camera;

        [HideInInspector]
        public InventoryData PlayerInventory = new InventoryData();

        public int Points = 0; // Similar to money, but it doesn't go down
        public int Money = 10;
        public int Combos = 0;
        public int Upkeep = 0;

        public int RotSpeed = 1;


        public int TotalIngredientsPurchased = 0;
        public int TotalIngredientsUsed = 0;
        public int TotalMoneySpent = 0;



        // Turn stats
        public int Turn = 1;
        public int TurnIngredientsPurchased = 0;
        public int TurnIngredientsUsed = 0;
        public int TurnPoints = 0;
        public int TurnMoney = 0;
        public int TurnMoneySpent = 0;
        public int TurnCombos = 0;


        public GameMode GameMode;
        public MarketMode MarketMode;

        private bool paused = false;
        public bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
            }
        }

        private float time;
        public float Time { get { return time; } }

        private float deltaTime;
        public float DeltaTime { get { return deltaTime; } }



        private void Awake()
        {
            Player = GetComponent<PlayerController>();
            Camera = GetComponent<CameraController>();

            Market.Init(this);
            Player.Init(this);
            Camera.Init(this);
            Overlay.Init(this);
            Restaurant.Init(this);
        }

        public void Start()
        {
            VisitMarket();

            Restaurant.UpdateCombos();
            Overlay.UpdateCombos();

            Overlay.ShowTutorial();
        }

        private void Update()
        {
            // Pause
            // TODO add a better pause key. Esc is bad for webgl
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Paused)
                {
                    Overlay.ShowPauseMenu(true);
                }
                Paused = !Paused;
            }

            if (!Paused)
            {
                time += UnityEngine.Time.deltaTime;
                deltaTime = UnityEngine.Time.deltaTime;
            }
            else
            {
                deltaTime = 0;
            }
        }


        public void VisitMarket()
        {
            GameMode = GameMode.Market;
            MarketMode = MarketMode.Market;
            Overlay.UpdateOverlayMode();
            Camera.MoveCameraToMarket();
        }

        public void VisitStall(StallController stall)
        {
            GameMode = GameMode.Market;
            MarketMode = MarketMode.Stall;
            Overlay.UpdateOverlayMode();
            Camera.MoveCameraToStall(stall);

            Overlay.SetChatTitle(stall.Data.Name);
            Overlay.SetChatText("Hi there!");

            //Overlay.SetChatOptions(new string[] { "test1", "OK!" });
        }

        public void VisitRestaurant()
        {
            GameMode = GameMode.Restaurant;
            Overlay.UpdateOverlayMode();
            Camera.MoveCameraToRestaurant();
        }

        /// <summary>
        /// Assumes that we're in the stall market mode
        /// </summary>
        /// <param name="inventory"></param>
        public void ShowTransactionWindow(InventoryIngredientData inventory)
        {
            Overlay.ShowTransactionWindow(inventory);
        }

        public void EndTurn()
        {
            // Update stalls
            Market.UpdateStalls();

            // Reduce food quality in inventory
            PlayerInventory.ReduceIngredientQuality(RotSpeed);
            Overlay.UpdateInventory();

            // Update combos
            Restaurant.UpdateCombos();
            Overlay.UpdateCombos();

            // Charge upkeep
            Money -= Upkeep;
            Upkeep = 10 * Turn;

            // This needs to be done before updating turn stats
            // since the end turn window needs the old stats
            if (Money < 0)
            {
                LoseCondition();
            }
            else
            {
                // Show the end turn window
                Overlay.ShowEndTurnWindow();
            }

            // Update the turn stats
            Turn++;
            TurnIngredientsPurchased = 0;
            TurnIngredientsUsed = 0;
            TurnPoints = 0;
            TurnMoney = 0;
            TurnMoneySpent = 0;
            TurnCombos = 0;

            Overlay.UpdatePoints();
        }

        public void LoseCondition()
        {
            Paused = true;
            Overlay.LoseConditionMessage();
        }
    }
}