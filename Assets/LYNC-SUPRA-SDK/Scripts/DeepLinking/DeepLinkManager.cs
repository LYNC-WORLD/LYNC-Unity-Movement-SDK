using System.Collections;
using UnityEngine;
using System.IO;
using LYNC.DeepLink;

namespace LYNC.Wallet
{
    public class DeepLinkManager
    {
        public static DeepLinkManager Instance { private set; get; } = null;
        
        private MessageHandler messageHandler = new MessageHandler();
        public static string gameObjectName { get; private set; }

        public static event BrowserProcessStartedCallback browserProcessStarted;
        public delegate void BrowserProcessStartedCallback(string url);

        public DeepLinkManager(string gameObjectName)
        {
            if (Instance != null) return;

            Instance = this;
            DeepLinkManager.gameObjectName = gameObjectName;


            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        public void StartBrowserProcess(string url)
        {
            browserProcessStarted?.Invoke(url);
        }

        private void OnDeepLinkActivated(string url)
        {
            // Handle the message
            HandleMessage(url);
            // Debug.Log(url);
        }

        public void HandleMessage(string message) => messageHandler.HandleMessage(message);
    }
}