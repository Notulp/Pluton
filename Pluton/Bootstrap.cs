﻿using System;
using System.IO;
using System.Timers;
using UnityEngine;

namespace Pluton
{
    public class Bootstrap : MonoBehaviour
    {

        public static string Version { get; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static ServerTimers timers;

        public static bool PlutonLoaded = false;

        public static void AttachBootstrap()
        {
            try {
                DirectoryConfig.GetInstance();
                CoreConfig.GetInstance();
                Config.GetInstance();

                if (!pluton.enabled) {
                    Debug.Log("[Bootstrap] Pluton is disabled!");
                    return;
                }

                Init();

                PlutonLoaded = true;
                Console.WriteLine("Pluton Loaded!");
            } catch (Exception ex) {
                Debug.LogException(ex);
                Debug.Log("[Bootstarp] Error while loading Pluton!");
            }
        }

        public static void SaveAll(object x = null)
        {
            try {
                Server.GetInstance().Save();
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

            var saver = Config.GetInstance().GetValue("Config", "saveInterval", "180000");
            var broadcast = Config.GetInstance().GetValue("Config", "broadcastInterval", "600000");
            if (saver != null && broadcast != null) {
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
            CryptoExtensions.Init();
            DataStore.GetInstance().Load();
            Server.GetInstance();

            CSharpPluginLoader.GetInstance();
            CSScriptPluginLoader.GetInstance();
            JSPluginLoader.GetInstance();
            PYPluginLoader.GetInstance();

            InstallThreadedOutput();

            ReloadTimers();
            ConVar.Server.official = false;
        }

        public static void InstallThreadedOutput()
        {
            Application.logMessageReceivedThreaded += new Application.LogCallback(delegate(string condition, string stackTrace, LogType type) {
                Logger.ThreadedLogRecieved(condition, stackTrace, type);
            });
            Application.logMessageReceived += new Application.LogCallback(delegate(string condition, string stackTrace, LogType type) {
                Logger.LogRecieved(condition, stackTrace, type);
            });
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

