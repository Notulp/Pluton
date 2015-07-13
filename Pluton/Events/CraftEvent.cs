namespace Pluton.Events
{
    public class CraftEvent : CountedInstance
    {
        /// <summary>
        /// The Player who started crafting.
        /// </summary>
        public Player Crafter;

        /// <summary>
        /// Item.Definition of the target item.
        /// </summary>
        public ItemDefinition Target;

        /// <summary>
        /// The ItemCrafter object.
        /// </summary>
        public ItemCrafter itemCrafter;

        /// <summary>
        /// The blueprint.
        /// </summary>
        public ItemBlueprint bluePrint;

        /// <summary>
        /// Amount of crafting item.
        /// </summary>
        public int Amount;

        /// <summary>
        /// Cancels teh event.
        /// </summary>
        public bool Cancel;

        /// <summary>
        /// Notify the Crafter about the action.
        /// </summary>
        public string cancelReason = "A plugin stops you from crafting that!";

        public CraftEvent(ItemCrafter self, ItemBlueprint bp, BasePlayer owner, ProtoBuf.Item.InstanceData instanceData, int amount)
        {
            Crafter = Server.GetPlayer(owner);
            Target = bp.targetItem;
            itemCrafter = self;
            Amount = amount;
            bluePrint = bp;
        }

        public void Stop(string reason = "A plugin stops you from crafting that!")
        {
            cancelReason = reason;
            Cancel = true;
        }

        /// <summary>
        /// Gets or sets the time needed to craft this item. NOTE: this is saved for each blueprint, so CraftTime /= 2; will make instacraft eventually. Be careful!
        /// </summary>
        /// <value>The craft time.</value>
        public float CraftTime {
            get {
                return bluePrint.time;
            }
            set {
                bluePrint.time = value;
            }
        }

        /// <summary>
        /// Freecraft.
        /// </summary>
        /// <value><c>true</c> for free craft; otherwise, <c>false</c>.</value>
        public bool FreeCraft {
            get {
                return itemCrafter.freeCraftingCheat;
            }
            set {
                itemCrafter.freeCraftingCheat = value;
            }
        }
    }
}

