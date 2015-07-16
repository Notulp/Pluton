namespace Pluton.Events
{
    public class CommandPermissionEvent : CommandEvent
    {
        public bool blocked = false;
        public readonly ChatCommand chatCommand;

        public CommandPermissionEvent(Player player, string[] command, ChatCommand chatCmd)
            : base(player, command)
        {
            chatCommand = chatCmd;
        }

        public void BlockCommand(string reason)
        {
            Reply = reason;
            blocked = true;
        }

        public string PluginName
        {
            get { return chatCommand.plugin.Name; }
        }
    }
}

