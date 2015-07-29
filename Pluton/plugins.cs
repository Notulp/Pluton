using System;

namespace Pluton
{
    [ConsoleSystem.Factory("plugins")]
    public class plugins : ConsoleSystem
    {
        [ConsoleSystem.Admin, ConsoleSystem.Help("Prints out plugin statistics!")]
        public static void Loaded(ConsoleSystem.Arg args)
        {
            int count = PluginLoader.GetInstance().Plugins.Count;
            string result = String.Format("Loaded plugins({0}):\r\n", count);
            foreach (BasePlugin plugin in PluginLoader.GetInstance().Plugins.Values) {
                result += String.Format("    {0, -22} [{1, -10}], timers: {2, 8}, parallel: {3, 8}\r\n", plugin.Name, plugin.Type, plugin.Timers.Count + plugin.ParallelTimers.Count, plugin.ParallelTimers.Count);
                result += String.Format("Author: {0}, about: {1}, version: {2}\r\n\r\n", plugin.Author, plugin.About, plugin.Version);
            }
            args.ReplyWith(result);
        }
    }
}

