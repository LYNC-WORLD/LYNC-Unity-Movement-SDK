using UnityEngine;

namespace LYNC.Wallet
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class testReg : MonoBehaviour
    {
        public string deepLinkUrl = "lync";
        public static string DeepLinkUrl { private set; get; }

        private void Awake()
        {
            Debug.Log("Starting...");
            testReg.DeepLinkUrl = deepLinkUrl.ToLower();

            if (!string.IsNullOrEmpty(deepLinkUrl))
            {
#if UNITY_EDITOR_OSX
                UnityEditor.PlayerSettings.macOS.urlSchemes = new string[] { deepLinkUrl.ToLower() };
#endif
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(deepLinkUrl))
            {
#if UNITY_EDITOR_OSX
                UnityEditor.PlayerSettings.macOS.urlSchemes = new string[] { deepLinkUrl.ToLower() };
#endif

                UnityEditor.PlayerSettings.iOS.iOSUrlSchemes = new string[] { deepLinkUrl.ToLower() };
            }
            else
                Debug.LogError("DeepLink URL is empty or null, set its value in LYNC prefab.");

            testReg.DeepLinkUrl = deepLinkUrl.ToLower();        }
#endif
    }
}