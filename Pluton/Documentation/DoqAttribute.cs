using System;
using System.Collections.Generic;

namespace Pluton.Documentation
{
    public class DoqAttribute : Attribute
    {
        public object category;

        public object doqType;

        public string description = "none";

        public Type[] args;

        public DoqAttribute(params Type[] Args){
            args = Args;
        }
    }
}

