using Piuncushion.LD52;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD52
{
    public class RestaurantController : MonoBehaviour
    {
        public SceneController Scene;

        public List<ComboData> RequestedCombos = new List<ComboData> ();
        public List<ComboData> CurrentCombos = new List<ComboData>();
        public List<ComboData> NextCombos = new List<ComboData>();


        public int MaxCombosPerSection = 3;

        public int MinBonusMultiplier = 2;
        public int MaxBonusMultiplier = 4;
        public int MinComboHarvests = 1;
        public int MaxComboHarvests = 5;
        //public int MinComboIngredients = 2;
        //public int MaxComboIngredients = 4;


        private WeightedRandomizer<int> RandomComboIngredients = new WeightedRandomizer<int>();

        public void Init(SceneController scene)
        {
            Scene = scene;

            RandomComboIngredients.AddSegment(2, 7);
            RandomComboIngredients.AddSegment(3, 2);
            RandomComboIngredients.AddSegment(4, 1);
        }

        /// <summary>
        /// Update combos for every harvest. They are randomly generated
        /// </summary>
        public void UpdateCombos()
        {
            // First update the existing combos
            for (int comboIndex = RequestedCombos.Count - 1; comboIndex >= 0; comboIndex--)
            {
                ComboData combo = RequestedCombos[comboIndex];

                combo.Harvests -= 1;

                if (combo.Harvests == 0)
                {
                    RequestedCombos.RemoveAt(comboIndex);
                }
            }
            for (int comboIndex = CurrentCombos.Count - 1; comboIndex >= 0; comboIndex--)
            {
                ComboData combo = CurrentCombos[comboIndex];

                combo.Harvests -= 1;

                if (combo.Harvests == 0)
                {
                    CurrentCombos.RemoveAt(comboIndex);
                }
            }

            // Next, fill the current with next
            if (CurrentCombos.Count < MaxCombosPerSection)
            {
                for (int comboIndex = CurrentCombos.Count; comboIndex < MaxCombosPerSection; comboIndex++)
                {
                    if (NextCombos.Count == 0)
                    {
                        CreateNewNextCombo();
                    }

                    ComboData combo = NextCombos[0];
                    CurrentCombos.Add(combo);
                    NextCombos.Remove(combo);
                }
            }

            // Fill the next
            if (NextCombos.Count < MaxCombosPerSection)
            {
                for (int comboIndex = NextCombos.Count; comboIndex < MaxCombosPerSection; comboIndex++)
                {
                    CreateNewNextCombo();
                }
            }
        }

        public void CreateNewNextCombo()
        {
            // Modes include
            // - using stuff in the player's inventory
            // - using stuff available this harvest
            // - using completely random ingredients

            int ingredientsInCombo = RandomComboIngredients.GetRandomEntry();// Random.Range(MinComboIngredients, MaxComboIngredients);
            List<IngredientData> ingredientDatas = new List<IngredientData>(ingredientsInCombo);

            while (ingredientDatas.Count < ingredientsInCombo)
            {
                // Look through player's stock for an ingredient
                InventoryIngredientData[] playerInventory = Scene.PlayerInventory.Ingredients;
                if (playerInventory.Length > 0)
                {
                    int playerInventoryIndex = Random.Range(0, playerInventory.Length);

                    if (!ingredientDatas.Contains(playerInventory[playerInventoryIndex].Ingredient))
                    {
                        ingredientDatas.Add(playerInventory[playerInventoryIndex].Ingredient);
                    }
                }

                // Look through the market stock for an ingredient
                if (ingredientDatas.Count < ingredientsInCombo)
                {
                    List<IngredientData> ingredients = new List<IngredientData>();
                    foreach (KeyValuePair<string, StallController> kv in Scene.Market.StallControllers)
                    {
                        StallController stallController = kv.Value;
                        foreach (StallIngredientController ingredientController in stallController.IngredientControllers)
                        {
                            InventoryIngredientData marketInventory = ingredientController.Data;
                            if (marketInventory.Quantity > 0 && !ingredientDatas.Contains(marketInventory.Ingredient))
                            {
                                ingredients.Add(marketInventory.Ingredient);
                            }
                        }
                    }

                    if (ingredients.Count > 0)
                    {
                        int marketInventoryIndex = Random.Range(0, ingredients.Count);
                        ingredientDatas.Add(ingredients[marketInventoryIndex]);
                    }
                }

                // Completely randomize
                if (ingredientDatas.Count < ingredientsInCombo)
                {
                    List<IngredientData> ingredients = new List<IngredientData>();

                    IngredientData[] allIngredients = Scene.Market.Ingredients;
                    if (allIngredients.Length > 0)
                    {
                        int allIngredientIndex = Random.Range(0, allIngredients.Length);

                        if (!ingredientDatas.Contains(allIngredients[allIngredientIndex]))
                        {
                            ingredientDatas.Add(allIngredients[allIngredientIndex]);
                        }
                    }
                }
            }

            // Build the combo
            ComboData combo = new ComboData()
            {
                Ingredients = ingredientDatas.ToArray(),
                BonusMultiplier = Random.Range(MinBonusMultiplier, MaxBonusMultiplier),
                Harvests = Random.Range(MinComboHarvests, MaxComboHarvests)
            };

            // Add the combo
            NextCombos.Add(combo);
        }

        /// <summary>
        /// Removes a current combo after it was used
        /// </summary>
        /// <param name="combos"></param>
        public void RemoveCombos(List<ComboData> combos)
        {
            for (int comboIndex = CurrentCombos.Count - 1; comboIndex >= 0; comboIndex--)
            {
                ComboData combo = CurrentCombos[comboIndex];

                if (combos.Contains(combo))
                {
                    CurrentCombos.RemoveAt(comboIndex);
                }
            }
        }

        public int CalculatePoints(InventoryIngredientData[] ingredients, ref List<ComboData> combos)
        {
            float points = 0;
            Dictionary<string, InventoryIngredientData> ingredientLookup = new Dictionary<string, InventoryIngredientData>();

            foreach (InventoryIngredientData data in ingredients)
            {
                if (!ingredientLookup.ContainsKey(data.Ingredient.Name))
                {
                    // Since there could be two ingredients of different qualities, only store one here, but get the points for both
                    ingredientLookup.Add(data.Ingredient.Name, data);
                }

                points += GetBasePoints(data);
            }

            // calculate multiplier
            float multiplier = 1;

            GetCombos(combos, RequestedCombos, ingredientLookup);
            GetCombos(combos, CurrentCombos, ingredientLookup);
            //GetCombos(combos, NextCombos, ingredientLookup);

            foreach (ComboData combo in combos)
            {
                multiplier *= combo.BonusMultiplier;
            }

            // Calculate total points
            points *= multiplier;

            return Mathf.FloorToInt(points);
        }

        private int GetBasePoints(InventoryIngredientData ingredient)
        {
            return ingredient.BasePoints;
        }

        private float GetComboMultiplier(List<ComboData> combos, Dictionary<string, InventoryIngredientData> ingredientLookup)
        {
            bool isCombo;
            float multiplier = 1;
            foreach (ComboData combo in combos)
            {
                isCombo = true;
                foreach (IngredientData comboIngredient in combo.Ingredients)
                {
                    if (!ingredientLookup.ContainsKey(comboIngredient.Name))
                    {
                        // not this combo
                        isCombo = false;
                        break;
                    }
                }

                if (isCombo)
                {
                    multiplier *= combo.BonusMultiplier;
                }
            }
            return multiplier;
        }
        private void GetCombos(List<ComboData> matchedCombos, List<ComboData> combos, Dictionary<string, InventoryIngredientData> ingredientLookup)
        {
            bool isCombo;
            float multiplier = 1;
            foreach (ComboData combo in combos)
            {
                isCombo = true;
                foreach (IngredientData comboIngredient in combo.Ingredients)
                {
                    if (!ingredientLookup.ContainsKey(comboIngredient.Name))
                    {
                        // not this combo
                        isCombo = false;
                        break;
                    }
                }

                if (isCombo)
                {
                    matchedCombos.Add(combo);
                }
            }
        }
    }
}