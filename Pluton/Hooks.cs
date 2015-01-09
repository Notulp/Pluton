using System;
using Network;
using ProtoBuf;
using UnityEngine;
using Pluton.Events;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Pluton
{
    public class Hooks
    {

        #region Events

        public static Subject<FrameDeployedEvent> OnBuildingFrameDeployed = new Subject<FrameDeployedEvent>();

        public static Subject<BuildingPart> OnBuildingComplete = new Subject<BuildingPart>();

        public static Subject<BuildingEvent> OnPlacement = new Subject<BuildingEvent>();

        public static Subject<ChatEvent> OnChat = new Subject<ChatEvent>();

        public static Subject<AuthEvent> OnClientAuth = new Subject<AuthEvent>();

        public static Subject<ClientConsoleEvent> OnClientConsole = new Subject<ClientConsoleEvent>();

        public static Subject<CombatEntityHurtEvent> OnCombatEntityHurt = new Subject<CombatEntityHurtEvent>();

        public static Subject<CommandEvent> OnCommand = new Subject<CommandEvent>();

        public static Subject<CommandPermissionEvent> OnCommandPermission = new Subject<CommandPermissionEvent>();

        public static Subject<CorpseHurtEvent> OnCorpseHurt = new Subject<CorpseHurtEvent>();

        public static Subject<CorpseInitEvent> OnCorpseDropped = new Subject<CorpseInitEvent>();

        public static Subject<DoorCodeEvent> OnDoorCode = new Subject<DoorCodeEvent>();

        public static Subject<DoorUseEvent> OnDoorUse = new Subject<DoorUseEvent>();

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

        public static Subject<ServerConsoleEvent> OnServerConsole = new Subject<ServerConsoleEvent>();

        public static Subject<string> OnServerInit = new Subject<string>();

        public static Subject<string> OnServerShutdown = new Subject<string>();

        public static Subject<RespawnEvent> OnRespawn = new Subject<RespawnEvent>();

        #endregion



        #region Handlers

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

                BasePlayer basePlayer = arg.Player ();
                if (!basePlayer) {
                    return;
                }

                ChatEvent pChat = new ChatEvent(Server.GetPlayer(basePlayer), arg);

                string str = arg.GetString(0, "text");

                if (str.Length > 128)
                    str = str.Substring(0, 128);

                if (chat.serverlog)
                    Debug.Log(basePlayer.displayName + ": " + str);

                OnChat.OnNext(pChat);

                if (pChat.FinalText != "") {
                    Logger.ChatLog(pChat.BroadcastName, pChat.FinalText);
                    arg.ReplyWith(pChat.Reply);

                    if (server.globalchat) {
                        ConsoleSystem.Broadcast("chat.add " + pChat.BroadcastName.QuoteSafe() + " " + pChat.FinalText.QuoteSafe() + " 1.0");
                    } else {
                        float num = 2500;
                        foreach (Connection current in Net.sv.connections) {
                            if (current.player != null) {
                                float sqrMagnitude = (current.player.transform.position - basePlayer.transform.position).sqrMagnitude;
                                if (sqrMagnitude <= num) {
                                    ConsoleSystem.SendClientCommand(current, "chat.add " + pChat.BroadcastName.QuoteSafe() + " " + pChat.FinalText.QuoteSafe() + " " + Mathf.Clamp01(num - sqrMagnitude + 0.2f).ToString("F").Replace(',', '.'));
                                }
                            }
                        }
                    }
                }
            }
        }

        // ConnectionAuth.Approve()
        public static void ClientAuth(ConnectionAuth ca, Connection connection)
        {
            var ae = new Events.AuthEvent(connection);

            OnClientAuth.OnNext(ae);

            ca.m_AuthConnection.Remove(connection);
            if (!ae.approved) {
                ConnectionAuth.Reject(connection, ae._reason);
                return;
            }

            Approval instance = new Approval();
            instance.level = Application.loadedLevelName;
            instance.levelSeed = global::World.Seed;
            instance.levelSize = global::World.Size;
            instance.hostname = server.hostname;
            Net.sv.Approve(connection, Approval.SerializeToBytes(instance));
        }

        //FacePunch.ConsoleSystem.OnClientCommand
        public static void ClientConsoleCommand(ConsoleSystem.Arg arg, String rconCmd)
        {
            ClientConsoleEvent ce = new ClientConsoleEvent(arg, rconCmd);
            if (arg.connection != null) {
                OnClientConsole.OnNext(ce);

                arg.ReplyWith(ce.Reply);
            }
        }

        public static bool blueprintsLoaded = false;
        public static void CraftingTime(ItemBlueprint ibp) {
            if (!blueprintsLoaded) {
                ibp.ingredients.Clear();
                Server.GetServer().blueprints.Add(ibp);
            }
        }

        // chat.say().Hooks.Chat()
        public static void Command(ConsoleSystem.Arg arg)
        {
            Player player = Server.GetPlayer(arg.Player());
            string[] args = arg.ArgsStr.Substring(2, arg.ArgsStr.Length - 3).Replace("\\", "").Split(new string[]{" "}, StringSplitOptions.None);

            CommandEvent cmd = new CommandEvent(player, args);

            if (cmd.cmd == "")
                return;

            foreach(KeyValuePair<string, Plugin> pl in PluginLoader.Plugins) {
                ChatCommand[] commands = pl.Value.chatCommands.getChatCommands(cmd.cmd);
                foreach (ChatCommand chatCmd in commands) {
                    if (chatCmd.callback == null)
                        continue;

                    CommandPermissionEvent permission = new CommandPermissionEvent(player, args, chatCmd);
                    OnCommandPermission.OnNext(permission);
                    if(permission.blocked) {
                        player.Message(permission.Reply);
                        continue;
                    }

                    try {
                        chatCmd.callback(cmd.args, player);
                    } catch (Exception ex) {
                        Logger.LogError(chatCmd.plugin.FormatExeption(ex));
                    }
                }
            }

            if (Config.GetBoolValue("Commands", "enabled", true)) {
                // TODO: make a plugin from these, no need to be in the core

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

                List<ChatCommands> cc = new List<ChatCommands>();
                foreach (KeyValuePair<string, Plugin> pl in PluginLoader.Plugins) {
                    cc.Add(pl.Value.chatCommands);
                }
                if (cmd.cmd == Config.GetValue("Commands", "Commands", "commands")) {
                    List<string> list = new List<string>();
                    foreach (ChatCommands cm in cc) {
                        list.AddRange(cm.getCommands());
                    }
                    player.Message(String.Join(", ", list.ToArray()));
                }
                if (cmd.cmd == Config.GetValue("Commands", "Description", "whatis")) {
                    if (cmd.args.Length < 1)
                        player.Message("You must provide a command name");
                    else {
                        List<string> list = new List<string>();
                        foreach (ChatCommands cm in cc) {
                            list.AddRange(cm.getDescriptions(cmd.args[0]));
                        }
                        if(list.Count > 0)
                            player.Message(String.Join("\r\n", list.ToArray()));
                    }
                }
                if (cmd.cmd == Config.GetValue("Commands", "Usage", "howto")) {
                    if (cmd.args.Length < 1)
                        player.Message("You must provide a command name");
                    else {
                        List<string> list = new List<string>();
                        foreach (ChatCommands cm in cc) {
                            list.AddRange(cm.getUsages(cmd.args[0]));
                        }
                        foreach (var item in list) {
                            player.Message(String.Format("/{0} {1}", cmd.args[0], item));
                        }
                    }
                }
            }
            OnCommand.OnNext(cmd);

            if (cmd.Reply != "")
                arg.ReplyWith(cmd.Reply);

        }

        public static void CombatEntityHurt(BaseCombatEntity combatEnt, HitInfo info)
        {
            try {
                Assert.Test (combatEnt.isServer, "This should be called serverside only");
                if (combatEnt.IsDead()) {
                    return;
                }

                BaseNPC npc = combatEnt.GetComponent<BaseNPC>();
                BaseCorpse corpse = combatEnt.GetComponent<BaseCorpse>();
                BasePlayer player = combatEnt.GetComponent<BasePlayer>();

                if (!SkinnedMeshCollider.ScaleDamage (info)) {
                    if (combatEnt.baseProtection != null) {
                        combatEnt.baseProtection.Scale (info.damageTypes);
                        if (ConsoleGlobal.developer > 1) {
                            Debug.Log ("BaseProtection Scaling for entity :" + combatEnt.name);
                        }
                    }
                }
                else {
                    if (ConsoleGlobal.developer > 1) {
                        Debug.Log ("SMC scaling damage for entity :" + combatEnt.name);
                    }
                }

                HurtEvent he;
                if (player != null) {
                    he = new PlayerHurtEvent(Server.GetPlayer(player), info);
                    OnPlayerHurt.OnNext(he as PlayerHurtEvent);
                } else if (npc != null) {
                    he = new NPCHurtEvent(new NPC(npc), info);
                    OnNPCHurt.OnNext(he as NPCHurtEvent);
                } else if (corpse != null) {
                    he = new CorpseHurtEvent(corpse, info);
                    OnCorpseHurt.OnNext(he as CorpseHurtEvent);
                } else {
                    he = new CombatEntityHurtEvent(combatEnt, info);
                    OnCombatEntityHurt.OnNext(he as CombatEntityHurtEvent);
                }

                // the DebugHurt() method
                if (vis.attack) {
                    if (info.PointStart != info.PointEnd) {
                        ConsoleSystem.Broadcast("ddraw.arrow", new object[] {
                            60, Color.cyan, info.PointStart, info.PointEnd, 0.1
                        });
                        ConsoleSystem.Broadcast("ddraw.sphere", new object[] {
                            60, Color.cyan, info.HitPositionWorld, 0.05
                        });
                    }
                    string text = String.Empty;
                    for (int i = 0; i < info.damageTypes.types.Length; i++) {
                        float num = info.damageTypes.types [i];
                        if (num != 0) {
                            string text2 = text;
                            text = String.Concat (new string[] {
                                text2, " ", ((Rust.DamageType)i).ToString().PadRight(10), num.ToString("0.00"), "\r\n"
                            });
                        }
                    }
                    string text3 = String.Concat(new object[] {
                        "<color=lightblue>Damage:</color>".PadRight(10),
                        info.damageTypes.Total().ToString("0.00"),
                        "\r\n<color=lightblue>Health:</color>".PadRight(10),
                        combatEnt.health.ToString ("0.00"), " / ",
                        (combatEnt.health - info.damageTypes.Total () > 0) ? "<color=green>" : "<color=red>",
                        (combatEnt.health - info.damageTypes.Total ()).ToString ("0.00"), "</color>",
                        "\r\n<color=lightblue>Hit Ent:</color>".PadRight(10), combatEnt,
                        "\r\n<color=lightblue>Attacker:</color>".PadRight(10), info.Initiator,
                        "\r\n<color=lightblue>Weapon:</color>".PadRight(10), info.Weapon,
                        "\r\n<color=lightblue>Damages:</color>\r\n", text
                    });
                    ConsoleSystem.Broadcast("ddraw.text", new object[] {
                        60, Color.white, info.HitPositionWorld, text3
                    });
                }

                combatEnt.health -= info.damageTypes.Total();
                if (ConsoleGlobal.developer > 1) {
                    Debug.Log("[Combat]".PadRight(10) +
                        combatEnt.gameObject.name + " hurt " +
                        info.damageTypes.GetMajorityDamageType() + "/" +
                        info.damageTypes.Total() + " - " +
                        combatEnt.health.ToString("0") + " health left"
                    );
                }
                combatEnt.lastDamage = info.damageTypes.GetMajorityDamageType();
                if (combatEnt.health <= 0) {
                    combatEnt.Die(info);
                }

            } catch (Exception ex) {
                Logger.LogError("[Hooks] Error in CombatEntityHurt hook.");
                Logger.LogException(ex);
            }
        }

        public static void DoorCode(CodeLock doorLock, BaseEntity.RPCMessage rpc)
        {
            if (!doorLock.IsLocked())
                return;

            DoorCodeEvent dc = new DoorCodeEvent(doorLock, rpc);
            OnDoorCode.OnNext(dc);
        }

        // Door.RPC_CloseDoor()/RPC_OpenDoor()
        public static void DoorUse(Door door, BaseEntity.RPCMessage rpc, bool open)
        {
            BaseLock baseLock = door.GetSlot(BaseEntity.Slot.Lock) as BaseLock;
            if (baseLock != null) {
                bool TryCloseOpen = open ? !baseLock.OnTryToOpen(rpc.player) : !baseLock.OnTryToClose(rpc.player);
                if (TryCloseOpen)
                    return;
            }

            DoorUseEvent due = new DoorUseEvent(new Entity(door), Server.GetPlayer(rpc.player), open);
            OnDoorUse.OnNext(due);

            door.SetFlag(BaseEntity.Flags.Open, due.Open);
            door.Invoke("UpdateLayer", 0f);
            door.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);

            if (due.DenyReason != "")
                rpc.player.SendConsoleCommand("chat.add " + StringExtensions.QuoteSafe(Server.server_message_name) + " " + StringExtensions.QuoteSafe(due.DenyReason));
        }

        // Construiction.Common.CreateConstruction()
        public static BuildingBlock DoPlacement(Construction.Common common, Construction.Target target, bool bNeedsValidPlacement)
        {
            try {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(common.prefab);
                BuildingBlock component = gameObject.GetComponent<BuildingBlock>();

                BuildingEvent be = new BuildingEvent(common, target, component, bNeedsValidPlacement);
                OnPlacement.OnNext(be);

                bool flag = Construction.UpdatePlacement(gameObject.transform, common, target);
                if (bNeedsValidPlacement && !flag) {
                    UnityEngine.Object.Destroy(gameObject);
                    return null;
                }

                if (be.DoDestroy) {
                    be.Builder.Message(be.DestroyReason);
                    UnityEngine.Object.Destroy(gameObject);
                    return null;
                }

                return component;
            } catch (Exception ex) {
                Logger.LogException(ex);
                return null;
            }
        }

        // BaseResource.OnAttacked()
        public static void GatheringBR(BaseResource res, HitInfo info)
        {
            if (!Realm.Server())
                return;

            OnGathering.OnNext(new Events.GatherEvent(res, info));

            ResourceDispenser dispenser = res.GetComponent<ResourceDispenser>();
            if (dispenser != null) {
                dispenser.OnAttacked(info);
            }
            float num = info.damageTypes.Total() * info.resourceGatherProficiency;
            res.health -= num;
            if (res.health <= 0) {
                res.Kill(EntityDestroy.Mode.None, 0, 0, Vector3.zero);
                return;
            }
            res.Invoke("UpdateNetworkStage", 0.1f);
        }

        // TreeEntity.OnAttacked()
        public static void GatheringTree(TreeEntity tree, HitInfo info)
        {
            if (!Realm.Server())
                return;

            OnGathering.OnNext(new Events.GatherEvent(tree, info));

            ResourceDispenser dispenser = tree.GetComponent<ResourceDispenser>();
            if (dispenser != null) {
                dispenser.OnAttacked(info);
            }
            float num = info.damageTypes.Total() * info.resourceGatherProficiency;
            tree.health -= num;
            if (tree.health <= 0) {
                tree.Kill(EntityDestroy.Mode.None, 0, 0, Vector3.zero);
                return;
            }
        }

        // BaseAnimal.Die()
        public static void NPCDied(BaseNPC bnpc, HitInfo info)
        {
            if (info.Initiator != null && info.Initiator.ToPlayer() != null) {
                Server.GetPlayer(info.Initiator as BasePlayer).Stats.AddKill(false, true);
            }

            var npc = new NPC(bnpc);
            OnNPCDied.OnNext(new Events.NPCDeathEvent(npc, info));
        }

        // BasePlayer.Die()
        public static void PlayerDied(BasePlayer player, HitInfo info)
        {
            if (info == null) {
                info = new HitInfo();
                info.AddDamage(player.lastDamage, Single.MaxValue);
                info.Initiator = player as BaseEntity;
            }

            Player victim = Server.GetPlayer(player);

            if (info.Initiator != null) {
                PlayerStats statsV = victim.Stats;

                if (info.Initiator is BasePlayer) {
                    Server.GetPlayer(info.Initiator as BasePlayer).Stats.AddKill(true, false);
                    victim.Stats.AddDeath(true, false);
                } else if (info.Initiator is BaseNPC) {
                    victim.Stats.AddDeath(false, true);
                }
            }

            Events.PlayerDeathEvent pde = new Events.PlayerDeathEvent(victim, info);
            OnPlayerDied.OnNext(pde);

            if (!pde.dropLoot)
                player.inventory.Strip();
        }


        public static void PlayerConnected(Network.Connection connection)
        {
            var player = connection.player as BasePlayer;
            var p = new Player(player);
            if (Server.GetServer().OfflinePlayers.ContainsKey(player.userID))
                Server.GetServer().OfflinePlayers.Remove(player.userID);
            if (!Server.GetServer().Players.ContainsKey(player.userID))
                Server.GetServer().Players.Add(player.userID, p);

            OnPlayerConnected.OnNext(p);
            if (Config.GetBoolValue("Config", "welcomeMessage", true)) {
                p.Message("Welcome " + p.Name + "!");
                p.Message(String.Format("This server is powered by Pluton[v{0}]!", Bootstrap.Version));
                p.Message("Visit pluton-team.org for more information or to report bugs!");
            }
        }

        // BasePlayer.OnDisconnected()
        public static void PlayerDisconnected(BasePlayer player)
        {
            var p = Server.GetPlayer(player);

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

        // BasePlayer.UpdateRadiation()
        public static void PlayerTakeRadiation(BasePlayer player, float radAmount)
        {
            var ptr = new PlayerTakeRadsEvent(Server.GetPlayer(player), (float)player.metabolism.radiation.GetFieldValue("value"), radAmount);
            OnPlayerTakeRads.OnNext(ptr);
            player.metabolism.radiation.SetFieldValue("value", ptr.Next);
        }
        /*
        // In future create an Event, allow people to adjust certain resources to give certain amounts!
        public static void ResourceGatherMultiplier(int amount, BaseEntity receiver, ItemAmount itemAmt)
        {
            int newAmt = amount;
            if (receiver.ToPlayer() != null)
                newAmt = (int)((double)amount * World.GetWorld().ResourceGatherMultiplier);

            Item item = ItemManager.CreateByItemID(itemAmt.itemid, newAmt);
            receiver.GiveItem(item);
        }*/

        public static void Respawn(BasePlayer player, bool newPos)
        {
            Player p = Server.GetPlayer(player);
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
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.StopSpectating();
            player.UpdateNetworkGroup();
            player.UpdatePlayerCollider(true, false);
            player.StartSleeping();
            player.metabolism.Reset();

            if (re.StartHealth < Single.Epsilon) {
                player.InitializeHealth(player.StartHealth(), player.StartMaxHealth());
            } else {
                player.InitializeHealth(re.StartHealth, player.StartMaxHealth());
            }

            if (re.GiveDefault)
                player.inventory.GiveDefaultItems();

            player.SendFullSnapshot();

            if (re.WakeUp)
                player.EndSleeping();
        }

        // PlayerLoot.StartLootingEntity()
        public static void StartLootingEntity(PlayerLoot playerLoot)
        {
            BasePlayer looter = playerLoot.GetComponent<BasePlayer>();
            var ele = new Events.EntityLootEvent(playerLoot, Server.GetPlayer(looter), new Entity(playerLoot.entitySource));
            OnLootingEntity.OnNext(ele);

            if (ele.Cancel) {
                playerLoot.Clear();
                looter.SendConsoleCommand("chat.add " + StringExtensions.QuoteSafe(Server.server_message_name) + " " + StringExtensions.QuoteSafe(ele.cancelReason));
            }
        }

        // PlayerLoot.StartLootingPlayer()
        public static void StartLootingPlayer(PlayerLoot playerLoot)
        {
            BasePlayer looter = playerLoot.GetComponent<BasePlayer>();
            var ple = new Events.PlayerLootEvent(playerLoot, Server.GetPlayer(looter), Server.GetPlayer(playerLoot.entitySource as BasePlayer));
            OnLootingPlayer.OnNext(ple);

            if (ple.Cancel) {
                playerLoot.Clear();
                looter.SendConsoleCommand("chat.add " + StringExtensions.QuoteSafe(Server.server_message_name) + " " + StringExtensions.QuoteSafe(ple.cancelReason));
            }
        }

        // PlayerLoot.StartLootingItem()
        public static void StartLootingItem(PlayerLoot playerLoot)
        {
            BasePlayer looter = playerLoot.GetComponent<BasePlayer>();
            var ile = new Events.ItemLootEvent(playerLoot, Server.GetPlayer(looter), playerLoot.itemSource);
            OnLootingItem.OnNext(ile);

            if (ile.Cancel) {
                playerLoot.Clear();
                looter.SendConsoleCommand("chat.add " + StringExtensions.QuoteSafe(Server.server_message_name) + " " + StringExtensions.QuoteSafe(ile.cancelReason));
            }
        }

        //FacePunch.ConsoleSystem.Run
        public static void ServerConsoleCommand(String rconCmd, bool wantFeedback)
        {
            if (!String.IsNullOrEmpty(rconCmd)) {
                ServerConsoleEvent ce = new ServerConsoleEvent(rconCmd, wantFeedback);

                foreach(KeyValuePair<string, Plugin> pl in PluginLoader.Plugins) {
                    ConsoleCommand[] commands = pl.Value.consoleCommands.getConsoleCommands(ce.cmd);
                    foreach (ConsoleCommand cmd in commands) {
                        if (cmd.callback == null)
                            continue;
                        try {
                            cmd.callback(ce.Args.ToArray());
                        } catch (Exception ex) {
                            Logger.LogError(cmd.plugin.FormatExeption(ex));
                        }
                    }
                }

                OnServerConsole.OnNext(ce);

                if(wantFeedback)
                    Debug.Log(String.Format("{0} was executed from console!", rconCmd));
            }
        }

        public static void ServerInit()
        {
            float craft = Single.Parse(Config.GetValue("Config", "craftTimescale", "1.0").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
            Server.GetServer().CraftingTimeScale = craft;
            double resource = Double.Parse(Config.GetValue("Config", "resourceGatherMultiplier", "1.0").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
            World.GetWorld().ResourceGatherMultiplier = resource;
            float time = Single.Parse(Config.GetValue("Config", "permanentTime", "-1").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture);
            if (time != -1) {
                World.GetWorld().Time = time;
                World.GetWorld().FreezeTime();
            } else {
                World.GetWorld().Timescale = Single.Parse(Config.GetValue("Config", "timescale", "30").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture);
            }
            Server.GetServer().Loaded = true;
            OnServerInit.OnNext("");
        }

        public static void ServerShutdown()
        {
            Bootstrap.timers.Dispose();
            OnServerShutdown.OnNext("");

            PluginLoader.GetInstance().UnloadPlugins();

            Bootstrap.SaveAll();
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
                Logger.LogError("[Hooks] Error while setting the server modded.");
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
    }
}
