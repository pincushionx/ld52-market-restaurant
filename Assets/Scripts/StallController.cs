using Piuncushion.LD52;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pincushion.LD52
{
    public class StallController : MonoBehaviour
    {
        public GameObject VirtualCamera;
        
        [HideInInspector]
        public MarketController Market;

        public StallData Data;
        public StallIngredientController[] IngredientControllers;

        public TextMeshPro Banner;

        public int MaxIngredients { get { return IngredientPlaceGos.Length; } }

        [SerializeField]
        [InspectorName("Ingredient Places")]
        private GameObject[] IngredientPlaceGos;

        private void Start()
        {
            Assert.IsNotNull(Market);
        }

        public void Init(MarketController market, StallData data)
        {
            Market = market;
            UpdateData(data);

            Banner.text = data.Name;
        }

        public void UpdateData(StallData data)
        {
            Data = data;

            PlaceIngredients(data.Inventory.Ingredients);
        }

        private void PlaceIngredients(InventoryIngredientData[] ingredients)
        {
            // Destroy existing ingredients, if any
            for (int i = 0; i < IngredientPlaceGos.Length; i++)
            {
                GameObject ingredientPlace = IngredientPlaceGos[i];
                if (ingredientPlace != null)
                {
                    for(int childIndex = ingredientPlace.transform.childCount-1; childIndex >= 0; childIndex--)
                    {
                        Transform childTransform = ingredientPlace.transform.GetChild(childIndex);
                        Destroy(childTransform.gameObject);
                    }
                }
            }

            IngredientControllers = new StallIngredientController[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                GameObject prefab = Market.GetIngredientPrefab(ingredients[i].Ingredient);
                GameObject ingredientGo = Instantiate(prefab);
                ingredientGo.transform.SetParent(IngredientPlaceGos[i].transform, false);

                StallIngredientController ingredientController = ingredientGo.GetComponent<StallIngredientController>();
                ingredientController.Data = ingredients[i];

                IngredientControllers[i] = ingredientController;
            }
        }

        public InventoryIngredientData[] GetIngredientInventory()
        {
            List<InventoryIngredientData> inventoryIngredients = new List<InventoryIngredientData>();
            foreach (StallIngredientController controller in IngredientControllers)
            {
                inventoryIngredients.Add(controller.Data);
            }
            return inventoryIngredients.ToArray();
        }
    }
}