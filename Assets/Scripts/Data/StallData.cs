using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD52
{
    public class StallData
    {
        public string Name { get; set; }
        public InventoryData Inventory { get; set; }
        public int BaseRotSpeed { get; set; }
        public int RotSpeed
        {
            get
            {
                return BaseRotSpeed;
            }
        }

        public string Prefab { get; set; }

        public PossibleStallIngredientData[] PossibleIngredients {get;set;}
        private WeightedRandomizer<string> _PossibleIngredientRandomizer;

        public string GetRandomIngredientName()
        {
            if (_PossibleIngredientRandomizer == null)
            {
                _PossibleIngredientRandomizer = new WeightedRandomizer<string>();
                foreach (PossibleStallIngredientData ingredient in PossibleIngredients)
                {
                    _PossibleIngredientRandomizer.AddSegment(ingredient.IngredientName, ingredient.ProbabilityWeight);
                }
            }

            return _PossibleIngredientRandomizer.GetRandomEntry();
        }


    }
}