using Piuncushion.LD52;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pincushion.LD52 {
    public class PlayerController : MonoBehaviour
    {
        public SceneController Scene;

        public void Init(SceneController scene)
        {
            Scene = scene;
        }

        private void Start()
        {
            Assert.IsNotNull(Scene, "Scene is required");
        }

        void Update()
        {
            if (Scene.Paused)
            {
                return;
            }

            if (Scene.GameMode == GameMode.Market)
            {
                if (Scene.MarketMode == MarketMode.Market)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        LayerMask selectableMask = LayerMask.GetMask("Selectable");

                        RaycastHit hit;
                        Ray ray = Scene.Camera.Camera.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray, out hit, float.MaxValue, selectableMask))
                        {
                            StallController stall = hit.transform.gameObject.GetComponent<StallController>();
                            if (stall != null)
                            {
                                Scene.VisitStall(stall);
                            }
                        }
                    }
                }
                else if (Scene.MarketMode == MarketMode.Stall)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        LayerMask selectableMask = LayerMask.GetMask("Selectable");

                        RaycastHit hit;
                        Ray ray = Scene.Camera.Camera.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray, out hit, float.MaxValue, selectableMask))
                        {
                            StallIngredientController ingredient = hit.transform.gameObject.GetComponent<StallIngredientController>();
                            if (ingredient != null)
                            {
                                Scene.ShowTransactionWindow(ingredient.Data);
                            }
                        }
                    }
                }
            }
        }
    }
}