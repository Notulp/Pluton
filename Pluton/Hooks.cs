using System;
using Network;
using ProtoBuf;
using UnityEngine;
using Pluton.Events;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace Pluton
{
    public class Hooks
    {

        #region Events

        public static Subject<BuildingHurtEvent> OnBuildingPartAttacked = new Subject<BuildingHurtEvent>();

        public static Subject<BuildingHurtEvent> OnBuildingPartDestroyed = new Subject<BuildingHurtEvent>();

        public static Subject<BuildingPart> OnBuildingFrameDeployed = new Subject<BuildingPart>();

        public static Subject<BuildingPart> OnBuildingComplete = new Subject<BuildingPart>();

        public static Subject<BuildingEvent> OnBuildingUpdate = new Subject<BuildingEvent>();

        public static Subject<Chat> OnChat = new Subject<Pluton.Chat>();

        public static Subject<AuthEvent> OnClientAuth = new Subject<AuthEvent>();

        public static Subject<Pluton.Command> OnCommand = new Subject<Pluton.Command>();

        public static Subject<CorpseHurtEvent> OnCorpseAttacked = new Subject<CorpseHurtEvent>();

        public static Subject<CorpseInitEvent> OnCorpseDropped = new Subject<CorpseInitEvent>();

        public static Subject<NPCDeathEvent> OnNPCDied = new Subject<NPCDeathEvent>();

        public static Subject<NPCHurtEvent> OnNPCHurt = new Subject<NPCHurtEvent>();

        public static Subject<Player> OnPlayerConnected = new Subject<Player>();

        public static Subject<Player> OnPlayerDisconnected = new Subject<Player>();

        public static Subject<PlayerDeathEvent> OnPlayerDied = new Subject<PlayerDeathEvent>();

        public static Subject<PlayerHurtEvent> OnPlayerHurt = new Subject<PlayerHurtEvent>();

        public static Subject<PlayerTakedmgEvent> OnPlayerTakeDamage = new Subject<PlayerTakedmgEvent>();

        public static Subject<PlayerTakeRadsEvent> OnPlayerTakeRads = new Subject<PlayerTakeRadsEvent>();

        public static Subject<GatherEvent> OnGathering = new Subject<GatherEvent>();

        public static Subject<EntityLootEvent> OnLootingEntity = new Subject<EntityLootEvent>();

        public static Subject<PlayerLootEvent> OnLootingPlayer = new Subject<PlayerLootEvent>();

        public static Subject<ItemLootEvent> OnLootingItem = new Subject<ItemLootEvent>();

        public static Subject<string> OnServerShutdown = new Subject<string>();

        public static Subject<RespawnEvent> OnRespawn = new Subject<RespawnEvent>();

        #endregion

        #region Handlers
        public static bool loaded = false;
        // ConnectionAuth.Approve()
        public static void ClientAuth(ConnectionAuth ca, Connection connection)
        {
            if (!loaded) {
                double resource = double.Parse(Config.PlutonConfig.GetSetting("Config", "resourceGatherMultiplier", "1.0").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
                World.GetWorld().ResourceGatherMultiplier = resource;
            
                Console.WriteLine("Svr Timescale:  " + Server.GetServer().CraftingTimeScale);
                float time = float.Parse(Config.PlutonConfig.GetSetting("Config", "permanentTime", "-1").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
                if (time != -1) {
                    World.GetWorld().Time = time;
                    World.GetWorld().FreezeTime();
                } else {
                    World.GetWorld().Timescale = float.Parse(Config.PlutonConfig.GetSetting("Config", "timescale", "30").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
                }
                loaded = true;
            }
            var ae = new Events.AuthEvent(connection);

            OnClientAuth.OnNext(ae);

            ca.m_AuthConnection.Remove(connection);
            if (!ae.approved)
                ConnectionAuth.Reject(connection, ae._reason);

            Approval instance = new Approval();
            instance.level = Application.loadedLevelName;
            instance.levelSeed = TerrainGenerator.Seed;
            instance.hostname = server.hostname;
            Net.sv.Approve(connection, Approval.SerializeToBytes(instance));
        }

        public static bool blueprintsLoaded = false;
        public static void CraftingTime(ItemBlueprint ibp) {
            if (!blueprintsLoaded)
                Server.GetServer().blueprints.Add(ibp);
        }

        // chat.say().Hooks.Chat()
        public static void Command(ConsoleSystem.Arg arg)
        {
            Player player = new Player(arg.Player());
            string[] args = arg.ArgsStr.Substring(2, arg.ArgsStr.Length - 3).Replace("\\", "").Split(new string[]{" "}, StringSplitOptions.None);

            Command cmd = new Command(player, args);

            if (cmd.cmd == "")
                return;

            if (Config.GetBoolValue("Commands", "enabled", true)) {
                if (cmd.cmd == Config.GetValue("Commands", "ShowMyStats", "mystats")) {
                    PlayerStats stats = player.Stats;
                    player.Message(String.Format("You have {0} kills and {1} deaths!", stats.Kills, stats.Deaths));
                    player.Message(String.Format("You have taken {0} dmg, and caused {1} in total!", stats.TotalDamageTaken, stats.TotalDamageDone));
                    return;
                }
                if (cmd.cmd == Config.GetValue("Commands", "ShowStatsOther", "statsof")) {
                    Player pOther = Player.Find(String.Join(" ", cmd.args));
                    if (pOther != null) {
                        PlayerStats stats2 = pOther.Stats;
                        player.Message(String.Format(pOther.Name + " has {0} kills and {1} deaths!", stats2.Kills, stats2.Deaths));
                        player.Message(String.Format(pOther.Name + " has taken {0} dmg, and caused {1} in total!", stats2.TotalDamageTaken, stats2.TotalDamageDone));
                        return;
                    }
                    player.Message("Can't find player: " + String.Join(" ", cmd.args));
                    return;
                }
                if (cmd.cmd == Config.GetValue("Commands", "ShowLocation", "whereami")) {
                    player.Message(player.Location.ToString());
                    return;
                }
                if (cmd.cmd == Config.GetValue("Commands", "ShowOnlinePlayers", "players")) {
                    string msg = Server.GetServer().Players.Count == 1 ? "You are alone!" : String.Format("There are: {0} players online!", Server.GetServer().Players.Count) ;
                    player.Message(msg);
                    return;
                }
				if (cmd.cmd == Config.GetValue("Commands", "Help", "help")) {
					foreach (string key in Config.PlutonConfig.EnumSection("HelpMessage")) {
						player.Message(Config.GetValue("HelpMessage", key));
					}
				}
                if (cmd.cmd == Config.GetValue("Commands", "Commands", "commands")) {
                    player.Message(String.Join(", ", PluginCommands.GetInstance().getCommands().ToArray()));
                }
                // TODO: command description, usage
            }
            OnCommand.OnNext(cmd);

            if (cmd.ReplyWith != "")
                arg.ReplyWith(cmd.ReplyWith);

        }

        // chat.say()
        public static void Chat(ConsoleSystem.Arg arg)
        {
            if (arg.ArgsStr.StartsWith("\"/") && !arg.ArgsStr.StartsWith("\"/ ")) {
                Command(arg);
                return;
            }

            if (!chat.enabled) {
                arg.ReplyWith("Chat is disabled.");
            } else {
                if (arg.ArgsStr == "\"\"") {
                    return;
                }

                BasePlayer basePlayer = ArgExtension.Player(arg);
                if (!(bool) ((UnityEngine.Object) basePlayer))
                    return;

                Chat pChat = new Chat(new Player(basePlayer), arg);

                string str = arg.GetString(0, "text");

                if (str.Length > 128)
                    str = str.Substring(0, 128);

                if (chat.serverlog)
                    Debug.Log((object)(basePlayer.displayName + ": " + str));

                OnChat.OnNext(pChat);

                if (pChat.FinalText != "") {
                    Logger.ChatLog(pChat.BroadcastName, pChat.FinalText);
                    ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(pChat.BroadcastName) + " " + StringExtensions.QuoteSafe(pChat.FinalText));
                    arg.ReplyWith(pChat.ReplyWith);
                }
            }
        }

        // In future create an Event, allow people to adjust certain resources to give certain amounts!
        public static void ResourceGatherMultiplier(int amount, BasePlayer player, ItemAmount itemAmt)
        {
            int newAmt = (int)((double)amount * World.GetWorld().ResourceGatherMultiplier);
            player.inventory.GiveItem(itemAmt.itemid, newAmt, true);
        }

        // BaseResource.OnAttacked()
        public static void Gathering(BaseResource res, HitInfo info)
        {
            if (!Realm.Server())
                return;

            OnGathering.OnNext(new Events.GatherEvent(res, info));

            res.health -= info.damageAmount * info.resourceGatherProficiency;
            if ((double) res.health <= 0.0)
                res.Kill(ProtoBuf.EntityDestroy.Mode.None, 0, 0.0f, new Vector3());
            else
                res.Invoke("UpdateNetworkStage", 0.1f);
        }

        // BaseAnimal.OnAttacked()
        public static void NPCHurt(BaseAnimal animal, HitInfo info)
        {
            var npc = new NPC(animal);

            if (info.Initiator != null) {
                Player p = new Player(info.Initiator as BasePlayer);
                PlayerStats stats = new PlayerStats(p.SteamID);
                stats.AddDamageTo(info.damageAmount, false, true, false);
                p.Stats = stats;
            }

            if (Realm.Server()) {
                if (animal.myHealth <= 0) {
                    return;
                }

                if ((animal.myHealth - info.damageAmount) > 0.0f)
                    OnNPCHurt.OnNext(new Events.NPCHurtEvent(npc, info));

                animal.myHealth -= info.damageAmount;
                if (animal.myHealth <= 0)
                    animal.Die(info);
            }
        }

        // BaseAnimal.Die()
        public static void NPCDied(BaseAnimal animal, HitInfo info)
        {
           
            if (info.Initiator != null) {
                Player p = new Player(info.Initiator as BasePlayer);
                PlayerStats stats = new PlayerStats(p.SteamID);
                stats.AddKill(false, true);
                p.Stats = stats;
            }

            var npc = new NPC(animal);
            OnNPCDied.OnNext(new Events.NPCDeathEvent(npc, info));
        }

        // BasePlayer.PlayerInit()
        public static void PlayerConnected(Network.Connection connection)
        {
            var player = connection.player as BasePlayer;
            var p = new Player(player);
            if (Server.GetServer().OfflinePlayers.ContainsKey(player.userID))
                Server.GetServer().OfflinePlayers.Remove(player.userID);
            if (!Server.GetServer().Players.ContainsKey(player.userID))
                Server.GetServer().Players.Add(player.userID, p);

            OnPlayerConnected.OnNext(p);
            p.Message("Welcome " + p.Name + "!");
            p.Message(String.Format("This server is powered by Pluton[v{0}]!", Bootstrap.Version));
            p.Message("Visit fougerite.com for more information or to report bugs!");
        }

        // BasePlayer.Die()
        public static void PlayerDied(BasePlayer player, HitInfo info)
        {
            if (info == null) {
                info = new HitInfo();
                info.damageType = player.metabolism.lastDamage;
                info.Initiator = player as BaseEntity;
            }

            Player victim = new Player(player);

            if (info.Initiator != null) {
                PlayerStats statsV = new PlayerStats(victim.SteamID);

                if (info.Initiator is BasePlayer) {
                    Player p = new Player(info.Initiator as BasePlayer);
                    PlayerStats stats = new PlayerStats(p.SteamID);
                    stats.AddKill(true, false);
                    p.Stats = stats;

                    statsV.AddDeath(true, false);
                } else if (info.Initiator is BaseAnimal) {
                    statsV.AddDeath(false, true);
                }
                victim.Stats = statsV;
            }

            Events.PlayerDeathEvent pde = new Events.PlayerDeathEvent(victim, info);
            OnPlayerDied.OnNext(pde);

            if (!pde.dropLoot)
                player.inventory.Strip();
        }

        // BasePlayer.OnDisconnected()
        public static void PlayerDisconnected(BasePlayer player)
        {
            var p = new Player(player);

            if (Server.GetServer().serverData.ContainsKey("OfflinePlayers", p.SteamID)) {
                OfflinePlayer op = (Server.GetServer().serverData.Get("OfflinePlayers", p.SteamID) as OfflinePlayer);
                op.Update(p);
                Server.GetServer().OfflinePlayers[player.userID] = op;
            } else {
                OfflinePlayer op = new OfflinePlayer(p);
                Server.GetServer().OfflinePlayers.Add(player.userID, op);
            }

            if (Server.GetServer().Players.ContainsKey(player.userID))
                Server.GetServer().Players.Remove(player.userID);

            OnPlayerDisconnected.OnNext(p);
        }

        // BasePlayer.OnAttacked()
        public static void PlayerHurt(BasePlayer player, HitInfo info)
        {
            var p = new Player(player);

            if (info == null) { // it should never accour, but just in case
                info = new HitInfo();
                info.damageAmount = 0.0f;
                info.damageType = player.metabolism.lastDamage;
                info.Initiator = player as BaseEntity;
            }

            bool fromPlayer = (info.Initiator is BasePlayer);
            PlayerStats statV = p.Stats;
            statV.AddDamageFrom(info.damageAmount, fromPlayer, !fromPlayer, false);
            p.Stats = statV;

            if (fromPlayer) {
                string sid = info.Initiator.ToPlayer().userID.ToString();
                PlayerStats statA = new PlayerStats(sid);
                statA.AddDamageTo(info.damageAmount, true, false, false);
                Server.GetServer().serverData.Add("PlayerStats", sid, statA);
            }

            if (!player.TestAttack(info) || !Realm.Server() || (info.damageAmount <= 0.0f))
                return;
            player.metabolism.bleeding.Add(Mathf.InverseLerp(0.0f, 100f, info.damageAmount));
            player.metabolism.SubtractHealth(info.damageAmount);
            player.TakeDamageIndicator(info.damageAmount, player.transform.position - info.PointStart);
            player.CheckDeathCondition(info);

            if (!player.IsDead())
                OnPlayerHurt.OnNext(new Events.PlayerHurtEvent(p, info));

            player.SendEffect("takedamage_hit");
        }

        // BasePlayer.TakeDamage()
        public static void PlayerTakeDamage(BasePlayer player, float dmgAmount, Rust.DamageType dmgType)
        {
            var ptd = new PlayerTakedmgEvent(new Player(player), dmgAmount, dmgType);
            OnPlayerTakeDamage.OnNext(ptd);
        }

        public static void PlayerTakeDamageOverload(BasePlayer player, float dmgAmount)
        {
            PlayerTakeDamage(player, dmgAmount, Rust.DamageType.Generic);
        }

        // BasePlayer.TakeRadiation()
        public static void PlayerTakeRadiation(BasePlayer player, float dmgAmount)
        {
            var ptr = new PlayerTakeRadsEvent(new Player (player), dmgAmount);
            OnPlayerTakeRads.OnNext(ptr);
        }

        // BuildingBlock.OnAttacked()
        public static void EntityAttacked(BuildingBlock bb, HitInfo info)
        {
            if (info.Initiator != null) {
                Player p = new Player(info.Initiator as BasePlayer);
                PlayerStats stats = new PlayerStats(p.SteamID);
                stats.AddDamageTo(info.damageAmount, false, false, true);
                p.Stats = stats;
            }

            var bp = new BuildingPart(bb);
            // if entity will be destroyed call the method below
            if ((bb.health - info.damageAmount) <= 0.0f) {
                BuildingPartDestroyed(bp, info);
                if ((bb.health - info.damageAmount) <= 0.0f)
                    return;
            }
            OnBuildingPartAttacked.OnNext(new BuildingHurtEvent(bp, info));
        }

        public static void BuildingPartDestroyed(BuildingPart bp, HitInfo info)
        {
            OnBuildingPartDestroyed.OnNext(new BuildingHurtEvent(bp, info));
        }

        // BuildingBlock.BecomeFrame()
        public static void EntityFrameDeployed(BuildingBlock bb)
        {
            // blockDefinition is null in this hook

            var bp = new BuildingPart(bb);
            OnBuildingFrameDeployed.OnNext(bp);
        }

        // BuildingBlock.BecomeBuilt()
        public static void EntityBuilt(BuildingBlock bb)
        {
            var bp = new BuildingPart(bb);
            OnBuildingComplete.OnNext(bp);
        }

        // BuildingBlock.DoBuild()
        public static void EntityBuildingUpdate(BuildingBlock bb, HitInfo info)
        {
            // hammer prof = 1
            // called anytime you hit a building block with a constructor item (hammer)
            BasePlayer player = info.Initiator as BasePlayer;
            float proficiency = info.resourceGatherProficiency;

            var bp = new BuildingPart(bb);
            var p = new Player(player);
            var ebe = new Events.BuildingEvent(bp, p, proficiency);
            OnBuildingUpdate.OnNext(ebe);
        }

        // BaseCorpse.InitCorpse()
        public static void CorpseInit(BaseCorpse corpse, BaseEntity parent)
        {
            OnCorpseDropped.OnNext(new CorpseInitEvent(corpse, parent));
        }

        // BaseCorpse.OnAttacked()
        public static void CorpseHit(BaseCorpse corpse, HitInfo info)
        {
            CorpseHurtEvent che = new CorpseHurtEvent(corpse, info);
            OnCorpseAttacked.OnNext(che);
        }

        // PlayerLoot.StartLootingEntity()
        public static void StartLootingEntity(PlayerLoot playerLoot, BasePlayer looter, BaseEntity entity)
        {
            var ele = new Events.EntityLootEvent(playerLoot, new Player(looter), new Entity(entity));

            OnLootingEntity.OnNext(ele);
        }

        // PlayerLoot.StartLootingPlayer()
        public static void StartLootingPlayer(PlayerLoot playerLoot, BasePlayer looter, BasePlayer looted)
        {
            // not tested

            var ple = new Events.PlayerLootEvent(playerLoot, new Player(looter), new Player(looted));

            OnLootingPlayer.OnNext(ple);
        }

        // PlayerLoot.StartLootingItem()
        public static void StartLootingItem(PlayerLoot playerLoot, BasePlayer looter, Item item)
        {
            var ile = new Events.ItemLootEvent(playerLoot, new Player(looter), item);

            OnLootingItem.OnNext(ile);
        }

        public static void ServerShutdown()
        {
            Bootstrap.timers.Dispose();
            OnServerShutdown.OnNext("");
            Bootstrap.SaveAll();
        }

        public static void Respawn(BasePlayer player, bool newPos)
        {
            Player p = new Player(player);
            RespawnEvent re = new RespawnEvent(p);
            OnRespawn.OnNext(re);

            ++ServerPerformance.spawns;
            if (newPos) {
                BasePlayer.SpawnPoint spawnPoint = ServerMgr.FindSpawnPoint();
                player.transform.position = spawnPoint.pos;
                player.transform.rotation = spawnPoint.rot;
            }
            if (re.ChangePos && re.SpawnPos != Vector3.zero) {
                player.transform.position = re.SpawnPos;
            }
            player.supressSnapshots = true;
            player.StopSpectating();
            player.UpdateNetworkGroup();
            player.UpdatePlayerCollider(true, false);
            player.StartSleeping();
            player.metabolism.Reset();
            if (re.GiveDefault)
                player.inventory.GiveDefaultItems();

            player.SendFullSnapshot();
        }

        public static void SetModded()
        {
            try {
                if (pluton.enabled) {
                    string pchGameTags = String.Format("mp{0},cp{1},v{2},modded",
                        new object[] { server.maxplayers, BasePlayer.activePlayerList.Count, Rust.Protocol.network });

                    Steamworks.SteamGameServer.SetGameTags(pchGameTags);
                }
            } catch (Exception ex) {
                Debug.Log("[Hooks] Error while setting the server modded.");
                Logger.LogException(ex);
            }
        }

        #endregion

        public static void Advertise()
        {
            foreach (string arg in Config.PlutonConfig.EnumSection("BroadcastMessages"))
            {
                Server.GetServer().Broadcast(Config.GetValue("BroadcastMessages", arg));
            }
        }

        public Hooks()
        {
        }
    }
}

