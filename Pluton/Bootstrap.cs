using System;
using System.IO;
using UnityEngine;

namespace Pluton
{
    public class Bootstrap : MonoBehaviour
    {

        public static string Version = "0.9.2";

        public static void AttachBootstrap()
        {
            try {
                Config.Init();
                if (!pluton.enabled)
                {
                    Debug.Log("[Bootstrap] Pluton is disabled!");
                    return;
                }

                Init();
                Console.WriteLine("Pluton Loaded!");
            } catch (Exception ex) {
                Debug.LogException(ex);
                Debug.Log("[Bootstarp] Error while loading Pluton!");
            }
        }

        public static void SaveAll(object x = null)
        {
            try {
                Server.GetServer().OnShutdown();
                DataStore.GetInstance().Save();
                Logger.LogDebug("[Bootstrap] Server saved successfully!");
            } catch (Exception ex) {
                Logger.LogDebug("[Bootstrap] Failed to save the server!");
                Logger.LogException(ex);
            }
        }

        public static void Init()
        {
            if (!Directory.Exists(Util.GetPublicFolder()))
                Directory.CreateDirectory(Util.GetPublicFolder());

            Logger.Init();
            Server.GetServer();
            PluginLoader.GetInstance().Init();

            server.official = false;

            if (!server.hostname.ToLower().Contains("pluton"))
                server.hostname = String.Format("{0} [Pluton v.{1}]", server.hostname, Version);
        }
    }
}

