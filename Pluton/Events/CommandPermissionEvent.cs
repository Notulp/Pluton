namespace Pluton.Events
{
    public class CommandPermissionEvent : CommandEvent
    {
        public bool blocked = false;
        public readonly ChatCommand chatCommand;

        public CommandPermissionEvent(Player player, string[] command, ChatCommand chatCmd)
            : base(player, command)
        {
            this.chatCommand = chatCmd;
        }

        public void BlockCommand(string reason)
        {
            this.Reply = reason;
            this.blocked = true;
        }

        public string PluginName
        {
            get { return this.chatCommand.plugin.Name; }
        }
    }
}

