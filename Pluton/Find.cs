using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton
{
    public class Find
    {
        static Find instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Find Instance {
            get {
                if (instance == null)
                    instance = new Find();
                return instance;
            }
        }

        /// <summary>
        /// Gets all the Entities.
        /// </summary>
        public List<Entity> Entities()
        {
            return (from ent in BaseNetworkable.serverEntities.All()
                select new Entity(ent as BaseEntity)).ToList<Entity>();
        }

        /// <summary>
        /// Gets all the entity that matches the specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public List<Entity> Entities(string name)
        {
            return (from ent in BaseNetworkable.serverEntities.All()
                where ent.LookupPrefabName().Contains(name) ||
                ent.LookupShortPrefabName().Contains(name) ||
                ent.name.Contains(name)
                select new Entity(ent as BaseEntity)).ToList<Entity>();
        }

        /// <summary>
        /// Gets a list of all the NPCs.
        /// </summary>
        /// <returns>All the NPCs.</returns>
        public List<NPC> NPCs()
        {
            return (from npc in UnityEngine.Object.FindObjectsOfType<BaseNPC>()
                select new NPC(npc)).ToList<NPC>();
        }

        /// <summary>
        /// Gets all the npc that matches the specified name.
        /// </summary>
        /// <returns>The cs.</returns>
        /// <param name="name">Name.</param>
        public List<NPC> NPCs(string name)
        {
            return (from npc in BaseNPC.list
                where npc.name.Contains(name) 
                select new NPC(npc)).ToList<NPC>();
        }

        /// <summary>
        /// Find a Player by the specified nameorIPorID.
        /// </summary>
        /// <param name="nameorIPorID">Name or IP or ID.</param>
        public Player Player(string nameorIPorID)
        {
            return (from player in Server.GetServer().Players.Values
                where player.Name == nameorIPorID ||
                player.IP == nameorIPorID ||
                player.SteamID == nameorIPorID
                select player).FirstOrDefault<Player>();
        }

        /// <summary>
        /// Find Players by part of their name or their IP.
        /// </summary>
        /// <param name="nameorIP">Name or IP.</param>
        public List<Player> Players(string nameorIP)
        {
            return (from player in Server.GetServer().Players.Values
                where player.Name.Contains(nameorIP) ||
                player.IP.Contains(nameorIP)
                select player).ToList<Player>();
        }
    }
}

