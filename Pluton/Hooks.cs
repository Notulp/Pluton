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

        public static Subject<BuildingHurtEvent> OnBuildingPartAttacked = new Subject<BuildingHurtEvent>();

        public static Subject<BuildingHurtEvent> OnBuildingPartDestroyed = new Subject<BuildingHurtEvent>();

        public static Subject<FrameDeployedEvent> OnBuildingFrameDeployed = new Subject<FrameDeployedEvent>();

        public static Subject<BuildingPart> OnBuildingComplete = new Subject<BuildingPart>();

        public static Subject<BuildingEvent> OnBuildingUpdate = new Subject<BuildingEvent>();

        public static Subject<ChatEvent> OnChat = new Subject<ChatEvent>();

        public static Subject<AuthEvent> OnClientAuth = new Subject<AuthEvent>();

        public static Subject<ClientConsoleEvent> OnClientConsole = new Subject<ClientConsoleEvent>();

        public static Subject<CommandEvent> OnCommand = new Subject<CommandEvent>();

        public static Subject<CommandPermissionEvent> OnCommandPermission = new Subject<CommandPermissionEvent>();

        public static Subject<CorpseHurtEvent> OnCorpseAttacked = new Subject<CorpseHurtEvent>();

        public static Subject<CorpseInitEvent> OnCorpseDropped = new Subject<CorpseInitEvent>();

        public static Subject<DoorCodeEvent> OnDoorCode = new Subject<DoorCodeEvent>();

        public static Subject<NPCDeathEvent> OnNPCDied = new Subject<NPCDeathEvent>();

        public static Subject<NPCHurtEvent> OnNPCHurt = new Subject<NPCHurtEvent>();

        public static Subject<Player> OnPlayerConnected = new Subject<Player>();

        public static Subject<Player> OnPlayerDisconnected = new Subject<Player>();

        public static Subject<PlayerDeathEvent> OnPlayerDied = new Subject<PlayerDeathEvent>();

        public static Subject<PlayerHurtEvent> OnPlayerHurt = new Subject<PlayerHurtEvent>();

        public static Subject<PlayerTakedmgEvent> OnPlayerTakeDamage = new Subject<PlayerTakedmgEvent>();

        public static Subject<PlayerTakeRadsEvent> OnPlayerTakeRads = new Subject<PlayerTakeRadsEvent>();

        public static Subject<MetabolismTickEvent> OnMetTick = new Subject<MetabolismTickEvent>();

        public static Subject<MetabolismDamageEvent> OnMetDamage = new Subject<MetabolismDamageEvent>();

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
        // ConnectionAuth.Approve()
        public static void ClientAuth(ConnectionAuth ca, Connection connection)
        {
            var ae = new Events.AuthEvent(connection);

            OnClientAuth.OnNext(ae);

            ca.m_AuthConnection.Remove(connection);
            if (!ae.approved)
                ConnectionAuth.Reject(connection, ae._reason);

            Approval instance = new Approval();
            instance.level = Application.loadedLevelName;
            instance.levelSeed = global::World.Seed;
            instance.levelSize = global::World.Size;
            instance.hostname = server.hostname;
            Net.sv.Approve(connection, Approval.SerializeToBytes(instance));
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
                        ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(pChat.BroadcastName) + " " + StringExtensions.QuoteSafe(pChat.FinalText) + " 1.0");
                    } else {
                        float num = Mathf.Pow(50, 2);
                        foreach (Connection current in Net.sv.connections) {
                            if (!(current.player == null)) {
                                float sqrMagnitude = (current.player.transform.position - basePlayer.transform.position).sqrMagnitude;
                                if (sqrMagnitude <= num) {
                                    ConsoleSystem.SendClientCommand(current, "chat.add " + StringExtensions.QuoteSafe(pChat.BroadcastName) + " " + StringExtensions.QuoteSafe(pChat.FinalText) + " " + Mathf.Clamp01(num - sqrMagnitude + 0.2f).ToString("F").Replace(',', '.'));
                                }
                            }
                        }
                    }
                }
            }
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

        public static void DoorCode(CodeLock doorLock, BaseEntity.RPCMessage rpc)
        {
            if (!doorLock.IsLocked())
                return;

            DoorCodeEvent dc = new DoorCodeEvent(doorLock, rpc);
            OnDoorCode.OnNext(dc);
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

            ResourceDispenser dispenser = res.GetComponent<ResourceDispenser>();
            if (dispenser != null) {
                dispenser.OnAttacked(info);
            }
            float num = info.damageTypes.Total() * info.resourceGatherProficiency;
            res.health -= num;
            if (res.health <= 0) {
                res.Kill(EntityDestroy.Mode.None, 0, 0, default(Vector3));
                return;
            }
            res.CallMethod("UpdateNetworkStage", 0,1);
        }

        /*// BaseAnimal.OnAttacked()
        public static void NPCHurt(BaseAnimal animal, HitInfo info)
        {
            var npc = new NPC(animal);

            if (info.Initiator != null) {
                Server.GetPlayer(info.Initiator as BasePlayer)
                    .Stats.AddDamageTo(info.damageAmount, false, true, false);
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
        }*/

        // BaseAnimal.Die()
        public static void NPCDied(BaseAnimal animal, HitInfo info)
        {

            if (info.Initiator != null) {
                Server.GetPlayer(info.Initiator as BasePlayer).Stats.AddKill(false, true);
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
            if (Config.GetBoolValue("Config", "welcomeMessage", true)) {
                p.Message("Welcome " + p.Name + "!");
                p.Message(String.Format("This server is powered by Pluton[v{0}]!", Bootstrap.Version));
                p.Message("Visit pluton-team.org for more information or to report bugs!");
            }
        }

        // BasePlayer.Die()
        public static void PlayerDied(BasePlayer player, HitInfo info)
        {
            if (info == null) {
                info = new HitInfo();
                info.AddDamage(Rust.DamageType.LAST, 100f);
                info.Initiator = player as BaseEntity;
            }

            Player victim = Server.GetPlayer(player);

            if (info.Initiator != null) {
                PlayerStats statsV = victim.Stats;

                if (info.Initiator is BasePlayer) {
                    Server.GetPlayer(info.Initiator as BasePlayer).Stats.AddKill(true, false);

                    victim.Stats.AddDeath(true, false);
                } else if (info.Initiator is BaseAnimal) {
                    victim.Stats.AddDeath(false, true);
                }
            }

            Events.PlayerDeathEvent pde = new Events.PlayerDeathEvent(victim, info);
            OnPlayerDied.OnNext(pde);

            if (!pde.dropLoot)
                player.inventory.Strip();
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

        // BasePlayer.OnAttacked()
        /*public static void PlayerHurt(BasePlayer player, HitInfo info)
        {
            var p = Server.GetPlayer(player);

            if (info == null) { // it should never accour, but just in case
                info = new HitInfo();
                info.AddDamage(Rust.DamageType.LAST, 0f);
                info.Initiator = player as BaseEntity;
            }

            if (!player.TestAttack(info) || !Realm.Server() || (info.damageAmount <= 0.0f))
                return;

            if (!player.IsDead())
                OnPlayerHurt.OnNext(new Events.PlayerHurtEvent(p, info));

            if (info.damageAmount <= 0.0f)
                return;

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

            player.metabolism.bleeding.Add(Mathf.InverseLerp(0.0f, 100f, info.damageAmount));
            player.metabolism.SubtractHealth(info.damageAmount);
            player.TakeDamageIndicator(info.damageAmount, player.transform.position - info.PointStart);
            player.CheckDeathCondition(info);

            player.SendEffect("takedamage_hit");
        }*/

        // BasePlayer.TakeDamage()
        public static void PlayerTakeDamage(BasePlayer player, float dmgAmount, Rust.DamageType dmgType)
        {
            var ptd = new PlayerTakedmgEvent(Server.GetPlayer(player), dmgAmount, dmgType);
            OnPlayerTakeDamage.OnNext(ptd);
        }

        public static void PlayerTakeDamageOverload(BasePlayer player, float dmgAmount)
        {
            PlayerTakeDamage(player, dmgAmount, Rust.DamageType.Generic);
        }

        // BasePlayer.TakeRadiation()
        public static void PlayerTakeRadiation(BasePlayer player, float dmgAmount)
        {
            var ptr = new PlayerTakeRadsEvent(Server.GetPlayer(player), dmgAmount);
            OnPlayerTakeRads.OnNext(ptr);
        }
        

        /*public static void RunMetabolism(PlayerMetabolism pm, float delta, BasePlayer owner)
        {
            delta = delta * World.GetWorld().MetabolismDeltaMultiplier;
            MetabolismTickEvent Evt = new MetabolismTickEvent(owner);

            float temperature = owner.GetTemperature();
            Evt.CurrentTemperature = temperature;

            Evt.FutureTemperature = Mathf.MoveTowards(pm.temperature.value, temperature - ((float)pm.CallMethod("DeltaWet") * 34f) + (pm.heartrate.value * 10f), delta * 2f);

            if ((double)pm.calories.value > 75.0)
                Evt.CaloriesHealthChange = (float)((double)Mathf.InverseLerp(75f, 100f, pm.calories.value) * (double)delta * 0.05);

            if ((double)pm.hydration.value > 75.0)
                Evt.HydrationHealthChange = (float)((double)Mathf.InverseLerp(75f, 100f, pm.hydration.value) * (double)delta * 0.01);

            Evt.CaloriesChange = (float)((double)pm.heartrate.value * (double)delta * -0.20);

            Evt.HydrationChange = (float)((double)pm.heartrate.value * (double)delta * -0.10);


            if ((double)pm.temperature.value > 40.0)
                Evt.HydrationChange += (float)((double)Mathf.InverseLerp(40f, 200f, pm.temperature.value) * (double)delta * -1.0);

            if ((double)Evt.FutureTemperature < 10.0)
            {
                float t = Mathf.InverseLerp(20f, -100f, Evt.FutureTemperature);
                Evt.HeartrateValue = Mathf.Clamp(Mathf.MoveTowards(pm.heartrate.value, Mathf.Lerp(0.5f, 10f, t), delta * 1f * t), pm.heartrate.min, pm.heartrate.max);

                // Final "update" processed after cold damage, same as normal;
                Evt.HeartrateValue = Mathf.Clamp(Mathf.MoveTowards(Evt.HeartrateValue, 0.0f, delta * 0.01666667f), pm.heartrate.min, pm.heartrate.max);
            }
            else
            {
                // Update per normal
                Evt.HeartrateValue = Mathf.Clamp(Mathf.MoveTowards(pm.heartrate.value, 0.0f, delta * 0.01666667f), pm.heartrate.min, pm.heartrate.max);
            }

            double waterLevel = System.Convert.ToDouble(owner.GetFieldValue("waterLevel"));

            if (waterLevel > 0.75)
                Evt.OxygenValue = Mathf.Clamp(Mathf.MoveTowards(pm.oxygen.value, 0.0f, delta * 0.1f), pm.oxygen.min, pm.oxygen.max);
            else
                Evt.OxygenValue = Mathf.Clamp(Mathf.MoveTowards(pm.oxygen.value, 1.0f, delta * 1.0f), pm.oxygen.min, pm.oxygen.max);


            if (waterLevel >= (double)pm.wetness.value)
                Evt.WetnessValue = (float)waterLevel;
            else
                Evt.WetnessValue = Mathf.Clamp(Mathf.MoveTowards(pm.wetness.value, 0.0f, delta * 0.2f * Mathf.InverseLerp(0.0f, 100f, temperature)), pm.wetness.min, pm.wetness.max);

            Evt.BleedingValue = Mathf.Clamp(Mathf.MoveTowards(pm.bleeding.value, 0.0f, delta * 0.008333334f), pm.bleeding.min, pm.bleeding.max); ;
            Evt.PoisonValue = Mathf.Clamp(Mathf.MoveTowards(pm.poison.value, 0.0f, delta * 0.5555556f), pm.poison.min, pm.poison.max); ;
            Evt.RadiationValue = Mathf.Clamp(Mathf.MoveTowards(pm.radiation.value, 0.0f, delta * 1.666667f), pm.radiation.min, pm.radiation.max); ;


            // Evt Push
            OnMetTick.OnNext(Evt);

            // Pushing updates here:
            pm.temperature.value = Mathf.Clamp(Evt.FutureTemperature, pm.temperature.min, pm.temperature.max);

            pm.health.Add(Evt.CaloriesHealthChange);
            pm.health.Add(Evt.HydrationHealthChange);
            pm.calories.Add(Evt.CaloriesChange);
            pm.hydration.Add(Evt.HydrationChange);
            pm.wetness.value = Mathf.Clamp(Evt.WetnessValue, pm.wetness.min, pm.wetness.max);
            pm.heartrate.value = Mathf.Clamp(Evt.HeartrateValue, pm.heartrate.min, pm.heartrate.max);
            pm.oxygen.value = Mathf.Clamp(Evt.OxygenValue, pm.oxygen.min, pm.oxygen.max);
            pm.bleeding.value = Mathf.Clamp(Evt.BleedingValue, pm.bleeding.min, pm.bleeding.max);
            pm.poison.value = Mathf.Clamp(Evt.PoisonValue, pm.poison.min, pm.poison.max);
            pm.radiation.value = Mathf.Clamp(Evt.RadiationValue, pm.oxygen.min, pm.radiation.max);

            if (Evt.PreventDamage)
            {
                return; // No need to process damage event.
            }

            MetabolismDamageEvent Dmg = new MetabolismDamageEvent(owner);

            if ((double)pm.calories.value <= 25.0)
                Dmg.HungerDamage = (float)((double)Mathf.InverseLerp(25f, 0.0f, pm.calories.value) * (double)delta * -0.1);

            if ((double)pm.hydration.value <= 25.0)
                Dmg.ThirstDamage = (float)((double)Mathf.InverseLerp(25f, 0.0f, pm.hydration.value) * (double)delta * -0.1);

            if ((double)pm.temperature.value < 1.0)
                Dmg.ColdDamage = (float)((double)Mathf.InverseLerp(1f, -50f, pm.temperature.value) * (double)delta * -5.0);

            if ((double)pm.temperature.value > 60.0)
                Dmg.HeatDamage = (float)((double)Mathf.InverseLerp(60f, 200f, pm.temperature.value) * (double)delta * -10.0);

            if ((double)pm.oxygen.value < 0.5)
                Dmg.DrownedDamage = (float)((double)Mathf.InverseLerp(0.5f, 0.0f, pm.oxygen.value) * (double)delta * -10.0);

            if ((double)pm.bleeding.value > 0.0)
                Dmg.BleedingDamage = (float)((double)pm.bleeding.value * (double)delta * -5.0);

            if ((double)pm.poison.value > 0.0)
                Dmg.PoisonDamage = (float)((double)pm.poison.value * (double)delta * -0.100000001490116);

            if ((double)pm.radiation.value > 0.0)
                Dmg.RadiationDamage = (float)((double)pm.radiation.value * (double)delta * -0.0199999995529652);


            if (Dmg.HungerDamage == 0.0f && Dmg.ThirstDamage == 0.0f && Dmg.ColdDamage == 0.0f && Dmg.HeatDamage == 0.0f && Dmg.DrownedDamage == 0.0f && Dmg.BleedingDamage == 0.0f && Dmg.PoisonDamage == 0.0f && Dmg.RadiationDamage == 0.0f)
                return; // We don't need to call the event;

            OnMetDamage.OnNext(Dmg);

            if ((float) Dmg.HungerDamage > 0.0f)
            {
                pm.health.Add(Dmg.HungerDamage);
                pm.lastDamage = Rust.DamageType.Hunger;
            }
            if (Dmg.ThirstDamage != 0.0f)
            {
                pm.health.Add(Dmg.ThirstDamage);
                pm.lastDamage = Rust.DamageType.Thirst;
            }
            if (Dmg.ColdDamage != 0.0f)
            {
                pm.health.Add(Dmg.ColdDamage);
                pm.lastDamage = Rust.DamageType.Thirst;
            }
            if (Dmg.HeatDamage != 0.0f)
            {
                pm.health.Add(Dmg.HeatDamage);
                pm.lastDamage = Rust.DamageType.Heat;
            }
            if (Dmg.DrownedDamage != 0.0f)
            {
                pm.health.Add(Dmg.DrownedDamage);
                pm.lastDamage = Rust.DamageType.Drowned;
            }
            if (Dmg.BleedingDamage != 0.0f)
            {
                pm.health.Add(Dmg.BleedingDamage);
                pm.lastDamage = Rust.DamageType.Bleeding;
            }
            if (Dmg.PoisonDamage != 0.0f)
            {
                pm.health.Add(Dmg.PoisonDamage);
                pm.lastDamage = Rust.DamageType.Poison;
            }
            if (Dmg.RadiationDamage != 0.0f)
            {
                pm.health.Add(Dmg.RadiationDamage);
                pm.lastDamage = Rust.DamageType.Radiation;
            }
            return;
        }*/





        /*// BuildingBlock.OnAttacked()
        public static void EntityAttacked(BuildingBlock bb, HitInfo info)
        {
            if (info.Initiator != null) {
                Server.GetPlayer(info.Initiator as BasePlayer)
                    .Stats.AddDamageTo(info.damageAmount, false, false, true);
            }

            var bp = new BuildingPart(bb);
            // if entity will be destroyed call the method below
            if ((bb.blockHealth - info.damageAmount) <= 0.0f) {
                BuildingPartDestroyed(bp, info);
                if ((bb.blockHealth - info.damageAmount) <= 0.0f)
                    return;
            }
            OnBuildingPartAttacked.OnNext(new BuildingHurtEvent(bp, info));
        }*/

        public static void BuildingPartDestroyed(BuildingPart bp, HitInfo info)
        {
            OnBuildingPartDestroyed.OnNext(new BuildingHurtEvent(bp, info));
        }

        // BuildingBlock.BecomeFrame()
        public static void EntityFrameDeployed(Item.Modules.Planner planner, Item item, BasePlayer p, GameObject obj)
        {
            if (obj == null)
                return;

            FrameDeployedEvent fde = new FrameDeployedEvent(planner, item, p, obj);
            OnBuildingFrameDeployed.OnNext(fde);
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
            BuildingPart bp = new BuildingPart(bb);

            BuildingEvent ebe = new Events.BuildingEvent(bp, info);
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
        public static void StartLootingEntity(PlayerLoot playerLoot)
        {
            BasePlayer looter = playerLoot.GetComponent<BasePlayer>();
            var ele = new Events.EntityLootEvent(playerLoot, Server.GetPlayer(looter), new Entity(playerLoot.entitySource));
            OnLootingEntity.OnNext(ele);
        }

        // PlayerLoot.StartLootingPlayer()
        public static void StartLootingPlayer(PlayerLoot playerLoot)
        {
            BasePlayer looter = playerLoot.GetComponent<BasePlayer>();
            var ple = new Events.PlayerLootEvent(playerLoot, Server.GetPlayer(looter), Server.GetPlayer(playerLoot.entitySource as BasePlayer));
            OnLootingPlayer.OnNext(ple);
        }

        // PlayerLoot.StartLootingItem()
        public static void StartLootingItem(PlayerLoot playerLoot)
        {
            BasePlayer looter = playerLoot.GetComponent<BasePlayer>();
            var ile = new Events.ItemLootEvent(playerLoot, Server.GetPlayer(looter), playerLoot.itemSource);
            OnLootingItem.OnNext(ile);
        }

        public static void ServerInit()
        {
            float craft = Single.Parse(Config.GetValue("Config", "craftTimescale", "1.0").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
            Server.GetServer().CraftingTimeScale = craft;
            double resource = Double.Parse(Config.GetValue("Config", "resourceGatherMultiplier", "1.0").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
            World.GetWorld().ResourceGatherMultiplier = resource;
            float metabolismdelta = float.Parse(Config.GetValue("Config", "metabolismModifier", "1.0").Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture) / 10;
            World.GetWorld().MetabolismDeltaMultiplier = metabolismdelta;
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
            player.SetPlayerFlag (BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.StopSpectating ();
            player.UpdateNetworkGroup ();
            player.UpdatePlayerCollider (true, false);
            player.StartSleeping ();
            player.metabolism.Reset ();
            if (re.GiveDefault)
                player.inventory.GiveDefaultItems();

            player.SendFullSnapshot();

            if (re.WakeUp)
                player.EndSleeping();
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

