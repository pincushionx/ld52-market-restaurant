using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pincushion.LD52
{
    public class InventoryData
    {

        // String is a combination of <Ingredient Name>__<Quality>
        private Dictionary<string, InventoryIngredientData> _IngredientLookups = new Dictionary<string, InventoryIngredientData>();
        public InventoryIngredientData[] Ingredients
        {
            get
            {
                InventoryIngredientData[] data = new InventoryIngredientData[_IngredientLookups.Count];
                _IngredientLookups.Values.CopyTo(data, 0);
                return data;
            }
        }

        public InventoryIngredientData GetIngredient(IngredientData ingredient, int quality)
        {
            string code = GetIngredientLookupCode(ingredient.Name, quality);
            if (_IngredientLookups.ContainsKey(code))
            {
                return _IngredientLookups[code];
            }
            return null;
        }
        public void SetIngredient(IngredientData ingredient, int quantity, int quality)
        {
            InventoryIngredientData data = new InventoryIngredientData()
            {
                Ingredient = ingredient,
                Quantity = quantity,
                Quality = quality
            };
            SetIngredient(data);
        }
        public void SetIngredient(InventoryIngredientData ingredientInventory)
        {
            string code = GetIngredientLookupCode(ingredientInventory);
            if (_IngredientLookups.ContainsKey(code))
            {
                _IngredientLookups[code] = ingredientInventory;
            }
            else
            {
                _IngredientLookups.Add(code, ingredientInventory);
            }
        }

        public void RemoveIngredient(InventoryIngredientData ingredientInventory)
        {
            if (_IngredientLookups.ContainsKey(ingredientInventory.LookupCode))
            {
                _IngredientLookups.Remove(ingredientInventory.LookupCode);
            }
        }

        public void SetIngredients(InventoryIngredientData[] ingredients)
        {
            _IngredientLookups.Clear();

            foreach (InventoryIngredientData ingredient in ingredients)
            {
                SetIngredient(ingredient);
            }
        }

        private string GetIngredientLookupCode(InventoryIngredientData ingredientInventory)
        {
            return ingredientInventory.LookupCode;
        }
        private string GetIngredientLookupCode(string name, int quality)
        {
            return InventoryIngredientData.GetLookupCode(name, quality);
        }

        public void ReduceIngredientQuality(int amount)
        {
            Dictionary<string, InventoryIngredientData> updatedIngredientLookups = new Dictionary<string, InventoryIngredientData>();

            foreach (KeyValuePair<string, InventoryIngredientData> kv in _IngredientLookups)
            {
                kv.Value.Quality -= amount;

                // Resave the ingredient with the new lookup code
                updatedIngredientLookups.Add(kv.Value.LookupCode, kv.Value);
            }

            // the dictionary needs to be replaced, since the Quality is part of the unique key
            _IngredientLookups = updatedIngredientLookups;
        }
    }
}