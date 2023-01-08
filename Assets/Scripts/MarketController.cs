using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pincushion.LD52
{
    public class MarketController : MonoBehaviour
    {
        [HideInInspector]
        public SceneController Scene;

        [SerializeField]
        [InspectorName("Stall positions")]
        private GameObject[] _StallPositionGos;


        private int RotThreshold = 5;
        private int MaxIngredientsPerStall = 3;

        private int MinIngredientPerSpot = 1;
        private int MaxIngredientPerSpot = 3;

        private int MinQuality = 6;
        private int MaxQuality = 10;

        private Dictionary<string, GameObject> _StallPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<IngredientData, GameObject> _IngredientPrefabs = new Dictionary<IngredientData, GameObject>();

        private StallData[] _Stalls =
        {
            new StallData() {
                Name = "Vita Farms",
                Prefab = "Prefab/Stall",
                BaseRotSpeed = 2,
                Inventory = new InventoryData(),
                PossibleIngredients = new PossibleStallIngredientData[] {
                    new PossibleStallIngredientData("Apple", 2),
                    new PossibleStallIngredientData("Eggplant", 2),
                    new PossibleStallIngredientData("Tomato", 3),
                    new PossibleStallIngredientData("Potato", 2),
                }
            },
            new StallData() {
                Name = "Chris's Creamery",
                Prefab = "Prefab/Stall",
                BaseRotSpeed = 2,
                Inventory = new InventoryData(),
                PossibleIngredients = new PossibleStallIngredientData[] {
                    new PossibleStallIngredientData("Cream", 2),
                    new PossibleStallIngredientData("Cheese", 3),
                    new PossibleStallIngredientData("Apple", 1),
                }
            },
            new StallData() {
                Name = "Ricky's Ranch",
                Prefab = "Prefab/Stall",
                BaseRotSpeed = 2,
                Inventory = new InventoryData(),
                PossibleIngredients = new PossibleStallIngredientData[] {
                    new PossibleStallIngredientData("Cream", 2),
                    new PossibleStallIngredientData("Cheese", 1),
                    new PossibleStallIngredientData("Meat", 3),
                    new PossibleStallIngredientData("Potato", 2),
                }
            },
            new StallData() {
                Name = "Gilda's Granary",
                Prefab = "Prefab/Stall",
                BaseRotSpeed = 2,
                Inventory = new InventoryData(),
                PossibleIngredients = new PossibleStallIngredientData[] {
                    new PossibleStallIngredientData("Potato", 1),
                    new PossibleStallIngredientData("Corn", 2),
                    new PossibleStallIngredientData("Wheat", 3),
                }
            },
        };
        public Dictionary<string, StallData> StallLookup = new Dictionary<string, StallData>();
        public Dictionary<string, StallController> StallControllers = new Dictionary<string, StallController>();

        public IngredientData[] Ingredients = {
            new IngredientData() {
                Name = "Apple",
                BasePoints = 5,
                BasePrice = 10,
                Prefab = "Prefab/Ingredients/Apple",
                Image = "Images/Apple"
            },
            new IngredientData() {
                Name = "Cheese",
                BasePoints = 22,
                BasePrice = 50,
                Prefab = "Prefab/Ingredients/Cheese",
                Image = "Images/Cheese"
            },
            new IngredientData() {
                Name = "Cream",
                BasePoints = 8,
                BasePrice = 20,
                Prefab = "Prefab/Ingredients/Cream",
                Image = "Images/Cream"
            },
            new IngredientData() {
                Name = "Corn",
                BasePoints = 5,
                BasePrice = 10,
                Prefab = "Prefab/Ingredients/Corn",
                Image = "Images/Corn"
            },
            new IngredientData() {
                Name = "Eggplant",
                BasePoints = 6,
                BasePrice = 10,
                Prefab = "Prefab/Ingredients/Eggplant",
                Image = "Images/Eggplant"
            },
            new IngredientData() {
                Name = "Meat",
                BasePoints = 30,
                BasePrice = 70,
                Prefab = "Prefab/Ingredients/Meat",
                Image = "Images/Meat"
            },
            new IngredientData() {
                Name = "Potato",
                BasePoints = 5,
                BasePrice = 10,
                Prefab = "Prefab/Ingredients/Potato",
                Image = "Images/Potato"
            },
            new IngredientData() {
                Name = "Tomato",
                BasePoints = 10,
                BasePrice = 20,
                Prefab = "Prefab/Ingredients/Tomato",
                Image = "Images/tomato"
            },
            new IngredientData() {
                Name = "Wheat",
                BasePoints = 5,
                BasePrice = 10,
                Prefab = "Prefab/Ingredients/Wheat",
                Image = "Images/Wheat"
            },
        };
        public Dictionary<string, IngredientData> IngredientLookup = new Dictionary<string, IngredientData>();

        private void Awake()
        {
            InitializeIngredientPrefabsAndLookups();
            InitializeStalls();

            // Populate their ingredients
            UpdateStalls();
        }

        public void UpdateStalls()
        {
            // for each stall
            //   Degrade their fresh unsold ingredients
            //   for the empty slots
            //     Select 3 ingredients from their possible ingredient list (randomly, with modifiers)

            List<InventoryIngredientData> newIngredients = new List<InventoryIngredientData>();

            foreach (KeyValuePair<string, StallController> kv in StallControllers)
            {
                string stallName = kv.Key;
                StallController stallController = kv.Value;
                StallData stallData = stallController.Data;
                InventoryData inventory = stallData.Inventory;
                InventoryIngredientData[] ingredients = inventory.Ingredients;

                newIngredients.Clear();

                foreach (InventoryIngredientData ingredient in ingredients)
                {
                    if (ingredient.Quantity > 0) {
                        ingredient.Quality -= stallData.RotSpeed;

                        if (ingredient.Quality >= RotThreshold)
                        {
                            newIngredients.Add(ingredient);
                        }
                    }
                }

                // Fill in the missing spots
                for (int i = newIngredients.Count; i < MaxIngredientsPerStall; i++)
                {
                    InventoryIngredientData newIngredient = GetNewIngredientForStall(stallName);
                    newIngredients.Add(newIngredient);
                }

                inventory.SetIngredients(newIngredients.ToArray());
                stallController.UpdateData(stallData);
            }
        }

        private InventoryIngredientData GetNewIngredientForStall(string stallName)
        {
            StallData stall = StallLookup[stallName];
            string ingredientName = stall.GetRandomIngredientName();

            return new InventoryIngredientData() {
                Ingredient = IngredientLookup[ingredientName],
                Quantity = Random.Range(MinIngredientPerSpot, MaxIngredientPerSpot+1),
                Quality = Random.Range(MinQuality, MaxQuality + 1)
            };
        }

        public void SetStall(int stallIndex, StallData stall)
        {
            GameObject stallPosition1 = _StallPositionGos[stallIndex];

            GameObject prefab = _StallPrefabs[stall.Name];
            GameObject stallGo = Instantiate(prefab);
            stallGo.transform.SetParent(stallPosition1.transform, false);
            stallGo.name = "Stall (" +stall.Name+ ")";

            StallController controller = stallGo.GetComponent<StallController>();
            controller.Init(this, stall);

            StallControllers.Add(stall.Name, controller);
        }

        private void Start()
        {
            Assert.IsNotNull(Scene, "Scene is required");
        }

        public void Init(SceneController scene)
        {
            Scene = scene;
        }

        public GameObject GetIngredientPrefab(IngredientData ingredient)
        {
            return _IngredientPrefabs[ingredient];
        }

        private void InitializeStalls()
        {
            int i = 0;
            foreach (StallData stall in _Stalls)
            {
                GameObject prefab = Resources.Load<GameObject>(stall.Prefab);
                _StallPrefabs.Add(stall.Name, prefab);

                StallLookup.Add(stall.Name, stall);

                SetStall(i++, stall);
            }
        }
        private void InitializeIngredientPrefabsAndLookups()
        {
            foreach (IngredientData ingredient in Ingredients)
            {
                GameObject prefab = Resources.Load<GameObject>(ingredient.Prefab);
                _IngredientPrefabs.Add(ingredient, prefab);

                IngredientLookup.Add(ingredient.Name, ingredient);
            }
        }



        public int GetPurchasePrice(InventoryIngredientData ingredient, int quantity)
        {
            return Mathf.CeilToInt(ingredient.Ingredient.BasePrice * ingredient.Quality / 10f) * quantity;
        }
    }
}