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
            string result = String.Format("Loaded plugins({0}):" + Environment.NewLine, count);
            foreach (BasePlugin plugin in PluginLoader.GetInstance().Plugins.Values) {
                result += String.Format("    {0, -22} [{1, -10}], timers: {2, 8}, parallel: {3, 8}\r\n", plugin.Name, plugin.Type, plugin.Timers.Count + plugin.ParallelTimers.Count, plugin.ParallelTimers.Count);
                result += String.Format("Author: {0}, about: {1}, version: {2}" + Environment.NewLine + Environment.NewLine, plugin.Author, plugin.About, plugin.Version);
            }
            args.ReplyWith(result);
        }

        [ConsoleSystem.Admin, ConsoleSystem.Help("Prints out hooks statistics!")]
        public static void Hooks(ConsoleSystem.Arg args)
        {
            Dictionary<string, List<string>> hooks = new Dictionary<string, List<string>>();
            PluginLoader.GetInstance().Plugins.Values.ToList().ForEach(
                p => p.Globals.ToList().ForEach(
                    g => {
                        if (g.StartsWith("On_"))
                            AddPluginToHookListInDict(hooks, g, p.Name);
                    }));
            
            string result = "The registered hooks are:" + Environment.NewLine;

            hooks.Keys.ToList().ForEach(k => {
                result += k + ": " + String.Join(", ", hooks[k].ToArray()) + Environment.NewLine;
            });

            args.ReplyWith(result);
        }

        private static void AddPluginToHookListInDict(Dictionary<string, List<string>> hooks, string key, string value)
        {
            if (hooks.ContainsKey(key))
                hooks[key].Add(value);
            else
                hooks.Add(key, new List<string>() { value } );
        }
    }
}

