using System.Collections;
using UnityEngine;
using System.IO;
using LYNC.DeepLink;

namespace LYNC.Wallet
{
    public class DeepLinkManager
    {
        // Windows configuration
        private readonly string launcherPath = (Application.streamingAssetsPath + "/Executables/Launcher.exe").Replace("/", "\\");
        private readonly string registerPath = (Application.streamingAssetsPath + "/Executables/register.reg").Replace("/", "\\");
        private readonly string sharedFilePath = @"C:\ProgramData\launcherdata.txt";

        public static DeepLinkManager Instance { private set; get; } = null;

        private Coroutine runningCoroutine = null;

        private MessageHandler messageHandler = new MessageHandler();

        public DeepLinkManager()
        {
            if (Instance != null) return;

            Instance = this;

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                RegisterCustomProtocol();
                ClearSharedFile();
                OpenLauncher();
            }
            Application.deepLinkActivated += OnDeepLinkActivated;
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
                runningCoroutine = LyncManager.Instance.StartCoroutine(ListenForBrowserMessageWindows());
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
                string url = text.Replace(System.Diagnostics.Process.GetCurrentProcess().Id.ToString(), "").Trim();
                ClearSharedFile();
                LyncManager.Instance.StopCoroutine(runningCoroutine);

                // Handle the message
                messageHandler.HandleMessage(url);

                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            runningCoroutine = LyncManager.Instance.StartCoroutine(ListenForBrowserMessageWindows());

        }

        private void OpenLauncher()
        {
            string processId = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(launcherPath, "processid" + processId);
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

            System.Diagnostics.Process regeditProcess = new System.Diagnostics.Process();
            regeditProcess.StartInfo.FileName = "C:\\Windows\\System32\\reg.exe";
            regeditProcess.StartInfo.Arguments = "import \"" + tempFilePath + "\"";
            regeditProcess.StartInfo.UseShellExecute = false;
            regeditProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            regeditProcess.Start();
            regeditProcess.WaitForExit();

            File.Delete(tempFilePath);
        }
        #endregion
    }
}