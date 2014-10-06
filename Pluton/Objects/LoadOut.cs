using System;
using System.IO;
using System.Collections.Generic;

namespace Pluton
{
    public class LoadOut
    {
        public Dictionary<int, LoadOutItem> items;
        public readonly string path;
        public readonly string Name;
        public int itemCount;

        public bool ModeratorUse;
        public bool OwnerUse;
        public bool NormalUse;

        public LoadOut(string name)
        {
            path = Path.Combine(Util.GetLoadoutFolder(), name + ".ini");
            bool nu = false;
            if (!File.Exists(path)) {
                File.AppendAllText(path, "");
                nu = true;
            }
            var ini = new IniParser(path);
            Name = ini.Name;
            if (!nu) {
                itemCount = Int32.Parse(ini.GetSetting("Def", "itemCount"));
                OwnerUse = ini.GetBoolSetting("Def", "ownerCanUse");
                ModeratorUse = ini.GetBoolSetting("Def", "modCanUse");
                NormalUse = ini.GetBoolSetting("Def", "normalCanUse");
            } else {
                itemCount = 0;
                OwnerUse = true;
                NormalUse = true;
                ModeratorUse = true;
            }
            items = new Dictionary<int, LoadOutItem>(30);

            if (itemCount != 0) {
                LoadOutItem current;
                for (var i = 0; i < itemCount; i++) {
                    string namee = ini.GetSetting(i.ToString(), "Name");
                    int amount = Int32.Parse(ini.GetSetting(i.ToString(), "Amount"));
                    current = new LoadOutItem(namee, amount);
                    items.Add(i, current);
                }
            }
            ini = null;
            if (Server.GetServer().LoadOuts.ContainsKey(Name))
                Server.GetServer().LoadOuts.Remove(Name);
            Server.GetServer().LoadOuts.Add(Name, this);
        }

        public bool Add(InvItem item)
        {
            return Add(new LoadOutItem(item.Name, item.Quantity));
        }

        public bool Add(LoadOutItem item)
        {
            if (itemCount >= 30) {
                Logger.LogDebug("[LoadOut] You may not add more then 30 items to one loadout.");
                return false;
            }
            items.Add(itemCount, item);
            itemCount++;

            if (Server.GetServer().LoadOuts.ContainsKey(Name))
                Server.GetServer().LoadOuts.Remove(Name);
            Server.GetServer().LoadOuts.Add(Name, this);
            return true;
        }

        public bool Remove(int item)
        {
            if (items.ContainsKey(item)) {
                items.Remove(item);
                itemCount--;
                Reorganize();
                return true;
            }
            return false;
        }

        public void Reorganize()
        {
            var count = 0;
            var items2 = items;
            items = new Dictionary<int, LoadOutItem>(30);
            foreach (LoadOutItem item in items2.Values) {
                if (item != null) {
                    items.Add(count, item);
                    count++;
                }
            }
            count++;
            if (count != itemCount) {
                Logger.LogDebug("[LoadOut] An error accoured while reorganizing the items?");
                itemCount = count;
            }
            if (Server.GetServer().LoadOuts.ContainsKey(Name))
                Server.GetServer().LoadOuts.Remove(Name);
            Server.GetServer().LoadOuts.Add(Name, this);
        }

        public void ToIni()
        {
            var ini = new IniParser(path);
            ini.AddSetting("Def", "itemCount", itemCount.ToString());
            ini.AddSetting("Def", "ownerCanUse", OwnerUse.ToString());
            ini.AddSetting("Def", "modCanUse", ModeratorUse.ToString());
            ini.AddSetting("Def", "normalCanUse", NormalUse.ToString());
            for (var i = 0; i < itemCount; i++) {
                ini.AddSetting(i.ToString(), "Name", items[i].Name);
                ini.AddSetting(i.ToString(), "Amount", items[i].Amount.ToString());
            }
            ini.Save();
        }

        public void ToInv(Inv inv, bool notify = true)
        {
            try {
                bool perms = false;
                if (NormalUse)
                    perms = true;
                else if (ModeratorUse && inv.owner.Moderator)
                    perms = true;
                else if (OwnerUse && inv.owner.Owner)
                    perms = true;

                if (perms)
                    foreach (LoadOutItem item in items.Values)
                    {
                        if (notify)
                            inv.Notice(item);
                        inv.Add(item.invItem);
                    }
				
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
        }
    }
}

