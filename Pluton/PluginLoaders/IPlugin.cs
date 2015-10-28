using System;
using Pluton.Events;

namespace Pluton
{
    public interface IPlugin
    {
        string FormatException(Exception ex);

        object Invoke(string method, params object[] args);

        void Load(string code = "");
    }

    public enum PluginState : sbyte
    {
        FailedToLoad = -1,
        NotLoaded = 0,
        Loaded = 1,
        HashNotFound = 2
    }

    public enum PluginType
    {
        Undefined,
        Python,
        JavaScript,
        CSharp,
        CSScript,
        Lua
    }
}

