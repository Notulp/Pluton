using System;
using System.Collections.Generic;

namespace Pluton.Events
{
    public class ServerConsoleEvent : CountedInstance
    {
        public readonly String cmd;
        public readonly List<String> Args;
        public readonly bool wantFeedback;

        public ServerConsoleEvent(String rconCmd, bool feedback)
        {
            this.wantFeedback = feedback;
            this.Args = new List<String>();

            if(String.IsNullOrEmpty(rconCmd))
                return;

            foreach (String str in rconCmd.Split(' '))
                Args.Add(str);
            
            this.cmd = Args[0];
            Args.RemoveAt(0);
        }
    }
}
