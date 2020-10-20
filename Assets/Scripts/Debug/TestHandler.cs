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
        
        private void Awake()
        {
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