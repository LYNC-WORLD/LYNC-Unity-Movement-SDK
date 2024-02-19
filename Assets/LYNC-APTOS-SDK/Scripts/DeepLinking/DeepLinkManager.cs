using System.Collections;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using LYNC.DeepLink;

namespace LYNC.Wallet
{
    public class DeepLinkManager : MonoBehaviour
    {
        // Windows configuration
        private readonly string launcherPath = (Application.streamingAssetsPath + "/Executables/Launcher.exe").Replace("/", "\\");
        private readonly string registerPath = (Application.streamingAssetsPath + "/Executables/register.reg").Replace("/", "\\");
        private readonly string sharedFilePath = @"C:\ProgramData\launcherdata.txt";

        public static DeepLinkManager Instance { private set; get; }

        private Coroutine runningCoroutine = null;

        private MessageHandler messageHandler = new MessageHandler();

        private void Start()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                RegisterCustomProtocol();
                ClearSharedFile();
                OpenLauncher();
            }
            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(gameObject);
        }

        // Invokable 
        public void StartBrowserProcess(string url)
        {
            // Open auth page for standalone and mobile
            Application.OpenURL(url);

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                ClearSharedFile();
                OpenLauncher();
                runningCoroutine = StartCoroutine(ListenForBrowserMessageWindows());
            }
        }

        private void OnDeepLinkActivated(string url)
        {
            // Handle the message
            messageHandler.HandleMessage(url);
        }

        #region Windows platform methods
        private IEnumerator ListenForBrowserMessageWindows()
        {
            string text = File.ReadAllText(sharedFilePath);
            if (text.IndexOf(DeepLinkRegistration.DeepLinkUrl.ToLower()) > -1)
            {
                string url = text.Replace(Process.GetCurrentProcess().Id.ToString(), "").Trim();
                ClearSharedFile();
                StopCoroutine(runningCoroutine);

                // Handle the message
                messageHandler.HandleMessage(url);

                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            runningCoroutine = StartCoroutine(ListenForBrowserMessageWindows());

        }

        private void OpenLauncher()
        {
            string processId = Process.GetCurrentProcess().Id.ToString();
            Process p = Process.Start(launcherPath, "processid" + processId);
        }

        private void ClearSharedFile()
        {
            using (StreamWriter writer = File.CreateText(sharedFilePath))
            {
                writer.Write("");
            }
        }

        private void RegisterCustomProtocol()
        {
            string tempFilePath = registerPath.Replace("register.reg", "temp.reg");
            string temp = File.ReadAllText(registerPath);
            temp = temp.Replace("%APP_NAME%", DeepLinkRegistration.DeepLinkUrl);
            temp = temp.Replace("%LAUNCHER_PATH%", launcherPath.Replace(@"\", @"\\"));

            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                writer.Write(temp);
            }

            Process regeditProcess = new Process();
            regeditProcess.StartInfo.FileName = "reg.exe";
            regeditProcess.StartInfo.Arguments = "import \"" + tempFilePath + "\"";
            regeditProcess.StartInfo.UseShellExecute = false;
            regeditProcess.Start();
            regeditProcess.WaitForExit();

            File.Delete(tempFilePath);
        }
        #endregion
    }
}