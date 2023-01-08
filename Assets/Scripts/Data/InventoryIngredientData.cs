using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD52
{
    public class InventoryIngredientData
    {
        public IngredientData Ingredient;
        public int Quantity;
        public int Quality; // 0 to 10 for 0-5 stars / 0-10 half stars

        public int BasePoints
        {
            get
            {
                return Mathf.FloorToInt(Ingredient.BasePoints * Quality / 10f);
            }
        }

        public string LookupCode
        {
            get
            {
                return GetLookupCode(Ingredient.Name, Quality);
            }
        }
        public static string GetLookupCode(string name, int quality)
        {
            return name + "__" + quality.ToString();
        }


    }
}