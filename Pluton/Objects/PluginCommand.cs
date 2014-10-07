using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton
{
    public class PluginCommand
    {
        public string _command;
        public string _description;
        public string _usage;

        public PluginCommand(string command, string description, string usage)
        {
            _command = command;
            _usage = usage;
            _description = description;
        }

        public PluginCommand()
        {
            _command = "";
            _usage = "_not specified_";
            _description = "_not specified_";
        }
    }

    public class PluginCommands
    {
        public static PluginCommands instance;

        public static Dictionary<int, PluginCommand> Commands = new Dictionary<int, PluginCommand>();

        public static PluginCommands GetInstance()
        {
            return (PluginCommands)instance;
        }

        public void RegisterCommand(string command, string usage, string description)
        {
            if (String.IsNullOrEmpty(command))
                return;
            PluginCommand c = new PluginCommand();
            c._description = description;
            c._command = command;
            c._usage = usage;
            RegisterCommand(c);
        }

        public void RegisterCommand(PluginCommand command)
        {
            Commands.Add(Commands.Count, command);
        }

        public PluginCommands()
        {
            if (instance == null)
                instance = this;
        }

        public List<string> getCommands()
        {
            return (from c in Commands.Values
                    select c._command).ToList();
        }

        public List<string> getDescriptions(string command)
        {
            return (from c in Commands.Values
                where c._command == command
                select c._description).ToList();
        }

        public List<string> getUsages(string command)
        {
            return (from c in Commands.Values
                where c._command == command
                select c._usage).ToList();
        }
    }
}

