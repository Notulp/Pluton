using System;

namespace Pluton
{
    public class LoadOutItem
    {
        public readonly int Amount;
        public readonly string Name;

        public LoadOutItem(string name)
        {
            Amount = 1;
            Name = name;
        }

        public LoadOutItem(string name, int amount)
        {
            Amount = amount;
            Name = name;
        }

        public InvItem invItem {
            get {
                return new InvItem(Name, Amount);
            }
        }
    }
}

