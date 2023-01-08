using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD52
{
    public class PossibleStallIngredientData
    {
        public string IngredientName;
        public int ProbabilityWeight;

        public PossibleStallIngredientData()
        {
        }
        public PossibleStallIngredientData(string ingredientName, int probabilityWeight)
        {
            IngredientName = ingredientName;
            ProbabilityWeight = probabilityWeight;
        }   
    }
}