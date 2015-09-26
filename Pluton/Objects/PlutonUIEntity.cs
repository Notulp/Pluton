using System.Collections.Generic;

namespace Pluton
{
    public class PlutonUIEntity
    {
        public List<Network.Connection> connections;
        public JSON.Array panels = new JSON.Array();

        public PlutonUIEntity(IEnumerable<Network.Connection> cons = null)
        {
            if (cons != null)
                connections = cons as List<Network.Connection>;
        }

        public PlutonUIEntity(Network.Connection con = null)
        {
            if (con != null)
                connections = new List<Network.Connection>() { con };
        }

        public PlutonUIPanel AddPanel(string name = null, string parent = null, float? fadeout = null)
        {
            PlutonUIPanel panel = new PlutonUIPanel(name, parent, fadeout);
            panels.Add(panel.obj);
            return panel;
        }

        public JSON.Array CreateUI()
        {
            if (connections.Count == 0)
                return null;
            
            CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connections = connections }, null, "AddUI", panels.ToString());
            return panels;
        }

        public void DestroyUI()
        {
            if (connections.Count == 0)
                return;
            foreach (var panel in panels) {
                CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connections = connections }, null, "DestroyPanel", panel.Obj.GetString("name"));
            }
        }

        public PlutonUIPanel GetPanel(string name)
        {
            for (int i = 0; i < panels.Length; i++) {
                if (panels[i].Obj.GetString(name) == name) {
                    return  new PlutonUIPanel() { obj = panels[i].Obj };
                }
            }
            return null;
        }

        public bool RemovePanel(string name)
        {
            
            for (int i = 0; i < panels.Length; i++) {
                if (panels[i].Obj.GetString(name) == name) {
                    panels.Remove(i);
                    return true;
                }
            }
            return false;
        }
    }
}

