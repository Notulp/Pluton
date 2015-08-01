using System;

namespace Pluton.Events
{
    public class CommandPermissionEvent : CommandEvent
    {
        public bool Blocked = false;
        [Obsolete("CommandPermissionEvent.blocked is obsolete and will be removed, please use CommandPermissionEvent.Blocked", true)]
        public bool blocked {
            get { return Blocked; }
        }

        public readonly ChatCommand ChatCommand;
        [Obsolete("CommandPermissionEvent.chatCommand is obsolete and will be removed, please use CommandPermissionEvent.ChatCommand", true)]
        public ChatCommand chatCommand {
            get { return ChatCommand; }
        }

        public CommandPermissionEvent(Player player, string[] command, ChatCommand chatCmd)
            : base(player, command)
        {
            ChatCommand = chatCmd;
        }

        public void BlockCommand(string reason)
        {
            Reply = reason;
            Blocked = true;
        }

        public string PluginName
        {
            get { return ChatCommand.plugin.Name; }
        }
    }
}

