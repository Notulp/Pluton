using System;
using System.IO;
using System.Timers;
using UnityEngine;

namespace Pluton
{
    public class Bootstrap : MonoBehaviour
    {

        public static string Version = "0.9.4";

        public static ServerTimers timers;
        public static Timer nameChangeTimer;


        public static void AttachBootstrap()
        {
            try {
                Config.Init();
                if (!pluton.enabled)
                {
                    Debug.Log("[Bootstrap] Pluton is disabled\t!");
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

            var saver = Config.GetValue("Config", "saveInterval");
            var broadcast = Config.GetValue("Config", "broadcastInterval");
            if(saver != null && broadcast != null) {
                double save = Double.Parse(saver);
                double ads = Double.Parse(Config.GetValue("Config", "broadcastInterval"));

                timers = new ServerTimers(save, ads);
                timers.Start();
            }
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
            nameChangeTimer = new Timer(60000);
            nameChangeTimer.Elapsed += new ElapsedEventHandler(nameChangeTimer_Elapsed);
            nameChangeTimer.Start();

            if (!server.hostname.StartsWith("[Pluton v"))
                server.hostname = String.Format("[Pluton v.{1}] {0}", server.hostname, Version);
        }

        private static void nameChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string start = String.Format("[Pluton v.{1}] {0}", server.hostname, Version);
            if (!server.hostname.StartsWith("[Pluton v")) {
                Console.WriteLine("setting");
                server.hostname = start;
                Console.WriteLine("set: " + server.hostname);
            }
        }

        public class ServerTimers
        {
            public readonly Timer _savetimer;
            public readonly Timer _adstimer;

            public ServerTimers(double save, double ads)
            {
                _savetimer = new Timer(save);
                _adstimer = new Timer(ads);
               
                Debug.Log("Server timers started!");
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

