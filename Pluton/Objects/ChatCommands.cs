﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton
{
    public class ChatCommand
    {
        public delegate void CallbackDelegate(string[] args, Player player);

        public string _command;
        public string _description;
        public string _usage;
        public BasePlugin plugin;
        public CallbackDelegate callback;

        public ChatCommand setCallback(CallbackDelegate function)
        {
            callback = function;
            return this;
        }
        public ChatCommand setCallback(string function)
        {
            callback = new CallbackDelegate((cmd, player) => plugin.Invoke(function, new[] {
                (object)cmd,
                player
            }));
            return this;
        }

        public ChatCommand setUsage(string usage)
        {
            _usage = usage;
            return this;
        }

        public ChatCommand setDescription(string description)
        {
            _description = description;
            return this;
        }

        public ChatCommand setCommand(string command)
        {
            _command = command;
            return this;
        }

        public ChatCommand(string command)
        {
            _command = command;
        }

        public ChatCommand()
        {
            _command = "";
        }
    }

    public class ChatCommands
    {
        public BasePlugin plugin;

        public Dictionary<int, ChatCommand> Commands = new Dictionary<int, ChatCommand>();

        public ChatCommands(BasePlugin pl)
        {
            plugin = pl;
        }

        public ChatCommand Register(string command)
        {
            if (String.IsNullOrEmpty(command))
                return (ChatCommand)null;

            var c = new ChatCommand(command);
            c.plugin = plugin;

            return Register(c);
        }
        
        public List<ChatCommand> RegisterWithMultipleNames(string[] commands, string callback, string usage, string description)
        {
            var chatCommands = new List<ChatCommand>();
            foreach (string command in commands)
            {
                ChatCommand chatCommand = Register(command).setCallback(callback).setUsage(usage).setDescription(description);
                chatCommands.Add(chatCommand);
            }
            return chatCommands;
        }

        public ChatCommand Register(ChatCommand command)
        {
            Commands.Add(Commands.Count, command);
            return command;
        }

        public List<string> getCommands()
        {
            return (from c in Commands.Values
                             select c._command).ToList<string>();
        }

        public ChatCommand[] getChatCommands(string command)
        {
            return (from c in Commands.Values
                             where c._command == command
                             select c).ToArray<ChatCommand>();
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

