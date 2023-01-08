using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD52
{
    public class CameraController : MonoBehaviour
    {
        public SceneController Scene;
        public Camera Camera;

        public GameObject MarketCamera;
        public GameObject RestaurantCamera;

        private void Awake()
        {
            Camera = Camera.main;
        }
        public void Init(SceneController scene)
        {
            Scene = scene;
        }


        public void MoveCameraToStall(StallController stall)
        {
            foreach (KeyValuePair<string, StallController> kv in Scene.Market.StallControllers)
            {
                if (kv.Value == stall)
                {
                    kv.Value.VirtualCamera.SetActive(true);
                }
                else
                {
                    kv.Value.VirtualCamera.SetActive(false);
                }
            }
        }

        public void MoveCameraToMarket()
        {
            MarketCamera.SetActive(true);
            RestaurantCamera.SetActive(false);

            MoveCameraToStall(null);

            // in the future, the market camera may be enabled/disabled
        }

        public void MoveCameraToRestaurant()
        {
            RestaurantCamera.SetActive(true);
            MarketCamera.SetActive(false);
        }
    }
}