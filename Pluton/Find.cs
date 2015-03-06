using System;
using System.Linq;
using System.Collections.Generic;

namespace Pluton
{
    public class Find : Singleton<Find>, ISingleton
    {
        public void Initialize(){}

        public List<BuildingPart> BuildingParts()
        {
            return (from block in UnityEngine.Object.FindObjectsOfType<BuildingBlock>()
                select new BuildingPart(block)).ToList<BuildingPart>();
        }

        public List<BuildingPart> BuildingPartsByName(string name)
        {
            return (from block in UnityEngine.Object.FindObjectsOfType<BuildingBlock>()
                where block.LookupPrefabName() == name ||
                block.LookupShortPrefabName() == name ||
                block.blockDefinition.fullname == name
                select new BuildingPart(block)).ToList<BuildingPart>();
        }

        public List<BuildingPart> BuildingPartsByGrade(BuildingGrade.Enum grade)
        {
            return (from block in UnityEngine.Object.FindObjectsOfType<BuildingBlock>()
                where block.grade == grade
                select new BuildingPart(block)).ToList<BuildingPart>();
        }

        public List<Entity> CupBoards()
        {
            return (from board in UnityEngine.Object.FindObjectsOfType<BuildingPrivlidge>()
                select new Entity(board)).ToList<Entity>();
        }

        // not tested...
        /*public List<Entity> CupBoards(ulong steamid)
        {
            return (from board in UnityEngine.Object.FindObjectsOfType<BuildingPrivlidge>()
                where board.authorizedPlayers.Where(x => x.userid == steamid).Count() != 0
                select new Entity(board)).ToList<Entity>();
        }*/

        public List<Entity> Doors()
        {
            return (from door in UnityEngine.Object.FindObjectsOfType<Door>()
                select new Entity(door as BaseEntity)).ToList<Entity>();
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

        public List<Entity> Locks()
        {
            return (from _lock in UnityEngine.Object.FindObjectsOfType<BaseLock>()
                select new Entity(_lock as BaseEntity)).ToList<Entity>();
        }

        public List<Entity> LocksByAuthorizedUserID(ulong steamid)
        {
            return (from _lock in UnityEngine.Object.FindObjectsOfType<CodeLock>()
                where ((List<ulong>)_lock.GetFieldValue("whitelistPlayers")).Contains(steamid)
                select new Entity(_lock as BaseEntity)).ToList<Entity>();
        }

        public List<Entity> Loot()
        {
            return (from loot in UnityEngine.Object.FindObjectsOfType<LootContainer>()
                select new Entity(loot as BaseEntity)).ToList<Entity>();
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
            return (from npc in UnityEngine.Object.FindObjectsOfType<BaseNPC>()
                where npc.name.Contains(name) 
                select new NPC(npc)).ToList<NPC>();
        }

        /// <summary>
        /// Find a Player by the specified nameorIPorID.
        /// </summary>
        /// <param name="nameorIPorID">Name or IP or ID.</param>
        public Player Player(string nameorIPorID)
        {
            return (from player in Server.GetInstance().Players.Values
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
            return (from player in Server.GetInstance().Players.Values
                where player.Name.Contains(nameorIP) ||
                player.IP.Contains(nameorIP)
                select player).ToList<Player>();
        }

        public List<Entity> Storage()
        {
            return (from storage in UnityEngine.Object.FindObjectsOfType<StorageContainer>()
                select new Entity(storage as BaseEntity)).ToList<Entity>();
        }
    }
}

