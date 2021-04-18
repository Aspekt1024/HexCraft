using System;
using UnityEngine;

namespace Aspekt.Hex
{
    /// <summary>
    /// Enables testing of the game scene, bypassing the lobby
    /// </summary>
    public class TestHandler : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private bool connectAsHost;
        [SerializeField] private NetworkManagerHex networkManager;
#pragma warning restore 649
        
        public bool loadAssetsFromBundles = true;

        public static TestHandler Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindObjectOfType<TestHandler>();
                return instance;
            }
        }

        private static TestHandler instance;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Multiple Test Handlers found! :(");
                Destroy(gameObject);
                return;
            }

            instance = this;
            
            var room = FindObjectOfType<NetworkManagerHex>();
            if (room == null)
            {
                SetupTestServer();
            }
        }

        private void SetupTestServer()
        {
            var room = Instantiate(networkManager);
            room.ConnectToTestServer(connectAsHost);
        } 
    }
}