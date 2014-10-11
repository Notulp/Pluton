using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Pluton
{
    public static class Logger
    {
        struct Writer
        {
            public StreamWriter LogWriter;
            public string DateTime;
        }

        private static string LogsFolder;
        private static Writer LogWriter;
        private static Writer ChatWriter;
        private static bool showChat = false;
        private static bool showDebug = false;
        private static bool showErrors = false;
        private static bool showException = false;
        private static bool logChat = false;
        private static bool logDebug = false;
        private static bool logErrors = false;
        private static bool logException = false;

        public static void Init()
        {
            try {
                logChat = Config.GetBoolValue("Logging", "chatInLog", true);
                logDebug = Config.GetBoolValue("Logging", "debugInLog", true);
                logErrors = Config.GetBoolValue("Logging", "errorInLog", true);
                logException = Config.GetBoolValue("Logging", "exceptionInLog", true);

                showChat = Config.GetBoolValue("Logging", "chatInConsole", true);
                showDebug = Config.GetBoolValue("Logging", "debugInConsole", true);
                showErrors = Config.GetBoolValue("Logging", "errorInConsole", true);
                showException = Config.GetBoolValue("Logging", "exceptionInConsole", true);
            } catch (Exception ex) {
                Debug.LogException(ex);
            }

            try {
                Debug.Log("Public folder: " + Util.GetPublicFolder());
                LogsFolder = Path.Combine(Util.GetPublicFolder(), "Logs");
                if (!Directory.Exists(LogsFolder))
                    Directory.CreateDirectory(LogsFolder);
                
                LogWriterInit();
                ChatWriterInit();
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static void LogWriterInit()
        {
            try {
                if (LogWriter.LogWriter != null)
                    LogWriter.LogWriter.Close();

                LogWriter.DateTime = DateTime.Now.ToString("dd_MM_yyyy");
                LogWriter.LogWriter = new StreamWriter(Path.Combine(LogsFolder, "Log " + LogWriter.DateTime + ".txt"), true);
                LogWriter.LogWriter.AutoFlush = true;
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static void ChatWriterInit()
        {
            try {
                if (ChatWriter.LogWriter != null)
                    ChatWriter.LogWriter.Close();

                ChatWriter.DateTime = DateTime.Now.ToString("dd_MM_yyyy");
                ChatWriter.LogWriter = new StreamWriter(Path.Combine(LogsFolder, "Chat " + ChatWriter.DateTime + ".txt"), true);
                ChatWriter.LogWriter.AutoFlush = true;
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static string LogFormat(string Text)
        {
            return String.Format("[{0}] {1}" , DateTime.Now, Text);
        }

        private static void WriteLog(string Message)
        {
            try {
                if (LogWriter.DateTime != DateTime.Now.ToString("dd_MM_yyyy"))
                    LogWriterInit();
                LogWriter.LogWriter.WriteLine(LogFormat(Message));
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        private static void WriteChat(string Message)
        {
            try {
                if (ChatWriter.DateTime != DateTime.Now.ToString("dd_MM_yyyy"))
                    ChatWriterInit();
                ChatWriter.LogWriter.WriteLine(LogFormat(Message));
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
        }

        // verbose?
        public static void Log(string Message, UnityEngine.Object Context = null)
        {
            Message = "[Console] " + Message;
            Debug.Log(Message, Context);
            WriteLog(Message);
        }

        public static void LogWarning(string Message, UnityEngine.Object Context = null)
        {
            Message = "[Warning] " + Message;
            Debug.LogWarning(Message, Context);
            WriteLog(Message);
        }

        public static void LogError(string Message, UnityEngine.Object Context = null)
        {
            Message = "[Error] " + Message;
            if (showErrors)
                Debug.LogError(Message, Context);

            if (!logErrors)
                return;

            WriteLog(Message);
        }

        public static void LogException(Exception Ex, UnityEngine.Object Context = null)
        {
            if (showException)
                Debug.LogException(Ex, Context);

            if (!logException)
                return;

            string Trace = "";
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 1; i < stackTrace.FrameCount; i++)
                Trace += stackTrace.GetFrame(i).GetMethod().DeclaringType.Name + "->" + stackTrace.GetFrame(i).GetMethod().Name + " | ";

            string Message = "[Exception] [ " + Trace + "]\r\n" + (Ex == null ? "(null) exception" : Ex.ToString());
            WriteLog(Message);
        }

        public static void LogDebug(string Message, UnityEngine.Object Context = null)
        {
            Message = "[Debug] " + Message;
            if (showDebug)
                Debug.Log(Message, Context);

            if (!logDebug)
                return;

            WriteLog(Message);
        }

        public static void ChatLog(string Sender, string Msg)
        {
            Msg = "[CHAT] " + Sender + ": " + Msg;
            if (showChat)
                Debug.Log(Msg);
            if (!logChat)
                return;

            WriteChat(Msg);
        }
    }
}