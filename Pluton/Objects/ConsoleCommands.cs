using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton
{
    public class ConsoleCommand
    {
        public delegate void CallbackDelegate(string[] args);

        public string _command;
        public string _description;
        public string _usage;
        public BasePlugin plugin;
        public CallbackDelegate callback;

        public ConsoleCommand setCallback(CallbackDelegate function)
        {
            callback = function;
            return this;
        }

        public ConsoleCommand setCallback(string function)
        {
            callback = new CallbackDelegate(cmd => {
                try {
                    plugin.Invoke(function, (object)cmd);
                } catch (Exception ex) {
                    Logger.Log("there");
                    Logger.Log(ex.ToString());
                }
            });
            return this;
        }

        public ConsoleCommand setUsage(string usage)
        {
            _usage = usage;
            return this;
        }

        public ConsoleCommand setDescription(string description)
        {
            _description = description;
            return this;
        }

        public ConsoleCommand setCommand(string command)
        {
            _command = command;
            return this;
        }

        public ConsoleCommand(string command)
        {
            _command = command;
        }

        public ConsoleCommand()
        {
            _command = "";
        }
    }

    public class ConsoleCommands
    {
        public BasePlugin plugin;

        public Dictionary<int, ConsoleCommand> Commands = new Dictionary<int, ConsoleCommand>();

        public ConsoleCommands(BasePlugin pl)
        {
            plugin = pl;
        }

        public ConsoleCommand Register(string command)
        {
            if (String.IsNullOrEmpty(command))
                return (ConsoleCommand)null;

            ConsoleCommand c = new ConsoleCommand(command);
            c.plugin = plugin;

            return Register(c);
        }

        public ConsoleCommand Register(ConsoleCommand command)
        {
            Commands.Add(Commands.Count, command);
            return command;
        }

        public List<string> getCommands()
        {
            return (from c in Commands.Values
                             select c._command).ToList<string>();
        }

        public ConsoleCommand[] getConsoleCommands(string command)
        {
            return (from c in Commands.Values
                             where c._command == command
                             select c).ToArray<ConsoleCommand>();
        }

        public string[] getDescriptions(string command)
        {
            return (from c in Commands.Values
                             where c._command == command
                             select c._description).ToArray<string>();
        }

        public string[] getUsages(string command)
        {
            return (from c in Commands.Values
                             where c._command == command
                             select c._usage).ToArray<string>();
        }
    }
}

