using System;
using System.IO;
using System.Timers;
using UnityEngine;

namespace Pluton
{
    public class Bootstrap : MonoBehaviour
    {

        public static string Version = "0.9.5";

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

            var saver = Config.GetValue("Config", "saveInterval", "180000");
            var broadcast = Config.GetValue("Config", "broadcastInterval", "600000");
            if(saver != null && broadcast != null) {
                double save = Double.Parse(saver);
                double ads = Double.Parse(broadcast);

                timers = new ServerTimers(save, ads);
                timers.Start();
            }
        }

        public static void Init()
        {
            if (!Directory.Exists(Util.GetPublicFolder()))
                Directory.CreateDirectory(Util.GetPublicFolder());

            Logger.Init();
            DataStore.GetInstance().Load();
            Server.GetServer();
            PluginLoader.GetInstance().Init();
            PluginCommands.GetInstance().Init();
            ReloadTimers();
            server.official = false;
            nameChangeTimer = new Timer(60000);
            nameChangeTimer.Elapsed += new ElapsedEventHandler(nameChangeTimer_Elapsed);
            nameChangeTimer.Start();
        }

        private static void nameChangeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            /*
             *  PLEASE READ: This is only temporary until a 1.0 release version.
             * 
             *  It helps Pluton by getting the name out there, which in return
             *  should bring in more plugin developers and users to pluton community.
             *  
             *  The below code will add [Pluton v0.9.x] to the start of your server name
             *  We would really appreciate you not changing this temporary addition.
             * */
            string start = String.Format("[Pluton v.{1}] {0}", server.hostname, Version);
            if (!server.hostname.StartsWith("[Pluton v")) {
                server.hostname = start;
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

