using System;
using System.IO;
using System.Timers;
using UnityEngine;

namespace Pluton
{
    public class Bootstrap : MonoBehaviour
    {

        public static string Version = "0.9.3";

        public static ServerTimers timers;

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
                Server.GetServer().Save();
                DataStore.GetInstance().Save();
                Logger.LogDebug("[Bootstrap] Server saved successfully!");
            } catch (Exception ex) {
                Logger.LogDebug("[Bootstrap] Failed to save the server!");
                Logger.LogException(ex);
            }
        }

        public static void ReloadTimers()
        {
            if (timers != null)
                timers.Dispose();

            double save = Double.Parse(Config.GetValue("Config", "saveInterval"));
            double ads = Double.Parse(Config.GetValue("Config", "broadcastInterval"));

            timers = new ServerTimers(save, ads);
            timers.Start();
        }

        public static void Init()
        {
            if (!Directory.Exists(Util.GetPublicFolder()))
                Directory.CreateDirectory(Util.GetPublicFolder());
            
            Logger.Init();
            Server.GetServer();
            PluginLoader.GetInstance().Init();
            PluginCommands.GetInstance().Init();
            ReloadTimers();

            server.official = false;

            if (!server.hostname.ToLower().Contains("pluton"))
                server.hostname = String.Format("{0} [Pluton v.{1}]", server.hostname, Version);
        }

        public class ServerTimers
        {
            public readonly Timer _savetimer;
            public readonly Timer _adstimer;

            public ServerTimers(double save, double ads)
            {
                _savetimer = new Timer(save);
                _adstimer = new Timer(ads);
                _savetimer.Elapsed += new ElapsedEventHandler(this._savetimer_Elapsed);
                _adstimer.Elapsed += new ElapsedEventHandler(this._adstimer_Elapsed);
            }

            public void Dispose()
            {
                Stop();
                _savetimer.Dispose();
                _adstimer.Dispose();
            }

            public void Start()
            {
                _savetimer.Start();
                _adstimer.Start();
            }

            public void Stop()
            {
                _savetimer.Stop();
                _adstimer.Stop();
            }

            private void _adstimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                Hooks.Advertise();
            }

            private void _savetimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                Bootstrap.SaveAll();
            }
        }
    }
}

