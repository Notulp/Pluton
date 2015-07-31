using System;
using Network;
using UnityEngine;
using Pluton.Events;
using System.Reactive.Subjects;
using System.Collections.Generic;

namespace Pluton
{
    public class Hooks
    {

        #region Events

        public static Subject<HammerEvent> OnBeingHammered = new Subject<HammerEvent>();

        public static Subject<BuildingPart> OnBuildingComplete = new Subject<BuildingPart>();

        public static Subject<BuildingPartDemolishedEvent> OnBuildingPartDemolished = new Subject<BuildingPartDemolishedEvent>();

        public static Subject<BuildingPartDestroyedEvent> OnBuildingPartDestroyed = new Subject<BuildingPartDestroyedEvent>();
        
        public static Subject<ChatEvent> OnChat = new Subject<ChatEvent>();

        public static Subject<AuthEvent> OnClientAuth = new Subject<AuthEvent>();

        public static Subject<ClientConsoleEvent> OnClientConsole = new Subject<ClientConsoleEvent>();

        public static Subject<CombatEntityHurtEvent> OnCombatEntityHurt = new Subject<CombatEntityHurtEvent>();
        
        public static Subject<CommandEvent> OnCommand = new Subject<CommandEvent>();

        public static Subject<CommandPermissionEvent> OnCommandPermission = new Subject<CommandPermissionEvent>();
        
        public static Subject<ConsumeFuelEvent> OnConsumeFuel = new Subject<ConsumeFuelEvent>();

        public static Subject<CorpseHurtEvent> OnCorpseHurt = new Subject<CorpseHurtEvent>();

        public static Subject<DoorCodeEvent> OnDoorCode = new Subject<DoorCodeEvent>();

        public static Subject<DoorUseEvent> OnDoorUse = new Subject<DoorUseEvent>();

        public static Subject<InventoryModEvent> OnItemAdded = new Subject<InventoryModEvent>();

        public static Subject<ItemConditionEvent> OnItemLoseCondition = new Subject<ItemConditionEvent>();

        public static Subject<ItemPickupEvent> OnItemPickup = new Subject<ItemPickupEvent>();

        public static Subject<InventoryModEvent> OnItemRemoved = new Subject<InventoryModEvent>();

        public static Subject<ItemRepairEvent> OnItemRepaired = new Subject<ItemRepairEvent>();

        public static Subject<ItemUsedEvent> OnItemUsed = new Subject<ItemUsedEvent>();

        public static Subject<Landmine> OnLandmineArmed = new Subject<Landmine>();

        public static Subject<Landmine> OnLandmineExploded = new Subject<Landmine>();

        public static Subject<EntityLootEvent> OnLootingEntity = new Subject<EntityLootEvent>();

        public static Subject<ItemLootEvent> OnLootingItem = new Subject<ItemLootEvent>();

        public static Subject<PlayerLootEvent> OnLootingPlayer = new Subject<PlayerLootEvent>();

        public static Subject<MiningQuarry> OnMining = new Subject<MiningQuarry>();

        public static Subject<BaseNetworkable> OnNetworkableKill = new Subject<BaseNetworkable>();

        public static Subject<NPCHurtEvent> OnNPCHurt = new Subject<NPCHurtEvent>();

        public static Subject<NPCDeathEvent> OnNPCDied = new Subject<NPCDeathEvent>();

        public static Subject<BuildingEvent> OnPlacement = new Subject<BuildingEvent>();

        public static Subject<Player> OnPlayerAssisted = new Subject<Player>();

        public static Subject<PlayerClothingEvent> OnPlayerClothingChanged = new Subject<PlayerClothingEvent>();

        public static Subject<Player> OnPlayerConnected = new Subject<Player>();

        public static Subject<PlayerDeathEvent> OnPlayerDied = new Subject<PlayerDeathEvent>();

        public static Subject<Player> OnPlayerDisconnected = new Subject<Player>();

        public static Subject<GatherEvent> OnGathering = new Subject<GatherEvent>();

        public static Subject<PlayerHurtEvent> OnPlayerHurt = new Subject<PlayerHurtEvent>();

        public static Subject<Player> OnPlayerLoaded = new Subject<Player>();

        public static Subject<Player> OnPlayerSleep = new Subject<Player>();

        public static Subject<CraftEvent> OnPlayerStartCrafting = new Subject<CraftEvent>();

        public static Subject<SyringeUseEvent> OnPlayerSyringeOther = new Subject<SyringeUseEvent>();

        public static Subject<SyringeUseEvent> OnPlayerSyringeSelf = new Subject<SyringeUseEvent>();

        public static Subject<PlayerTakeRadsEvent> OnPlayerTakeRads = new Subject<PlayerTakeRadsEvent>();

        public static Subject<Player> OnPlayerWakeUp = new Subject<Player>();

        public static Subject<Player> OnPlayerWounded = new Subject<Player>();

        public static Subject<RespawnEvent> OnRespawn = new Subject<RespawnEvent>();

        public static Subject<RocketShootEvent> OnRocketShooting = new Subject<RocketShootEvent>();

        public static Subject<ShootEvent> OnShooting = new Subject<ShootEvent>();

        public static Subject<ServerConsoleEvent> OnServerConsole = new Subject<ServerConsoleEvent>();

        public static Subject<string> OnServerInit = new Subject<string>();

        public static Subject<string> OnServerSaved = new Subject<string>();

        public static Subject<string> OnServerShutdown = new Subject<string>();

        public static Subject<WeaponThrowEvent> OnWeaponThrow = new Subject<WeaponThrowEvent>();

        #endregion

        #region Handlers

        // chat.say()
        public static void Chat(ConsoleSystem.Arg arg)
        {
            if (arg.ArgsStr.StartsWith("\"/") && !arg.ArgsStr.StartsWith("\"/ ")) {
                Command(arg);
                return;
            }

            if (!ConVar.Chat.enabled) {
                arg.ReplyWith("Chat is disabled.");
            } else {
                if (arg.ArgsStr == "\"\"") {
                    return;
                }

                BasePlayer basePlayer = arg.Player();
                if (!basePlayer) {
                    return;
                }

                ChatEvent pChat = new ChatEvent(Server.GetPlayer(basePlayer), arg);

                string str = arg.GetString(0, "text");

                if (str.Length > 128)
                    str = str.Substring(0, 128);

                if (str.Length <= 0)
                    return;


                if (ConVar.Chat.serverlog) {
                    ServerConsole.PrintColoured(new object[] {
                        ConsoleColor.DarkYellow,
                        basePlayer.displayName + ": ",
                        ConsoleColor.DarkGreen,
                        str
                    });
                    ConVar.Server.Log("Log.Chat.txt", string.Format("{0}/{1}: {2}\r\n", basePlayer.userID, basePlayer.displayName, str));
                    Debug.Log(string.Format("[CHAT] {0}: {1}", basePlayer.displayName, str));
                }

                string arg2 = "#5af";
                if (basePlayer.IsAdmin()) {
                    arg2 = "#af5";
                }

                if (DeveloperList.IsDeveloper(basePlayer)) {
                    arg2 = "#fa5";
                }

                OnChat.OnNext(pChat);

                string text2 = string.Format("<color={2}>{0}</color>: {1}", basePlayer.displayName.Replace('<', '[').Replace('>', ']'), pChat.FinalText, arg2);

                if (pChat.FinalText != "") {
                    Logger.ChatLog(pChat.BroadcastName, pChat.OriginalText);
                    arg.ReplyWith(pChat.Reply);

                    if (ConVar.Server.globalchat) {
                        ConsoleSystem.Broadcast("chat.add", basePlayer.userID, text2, 1);
                    } else {
                        float num = 2500;
                        foreach (Connection current in Net.sv.connections) {
                            if (current.player != null) {
                                float sqrMagnitude = (current.player.transform.position - basePlayer.transform.position).sqrMagnitude;
                                if (sqrMagnitude <= num) {
                                    ConsoleSystem.SendClientCommand(current, "chat.add", basePlayer.userID, text2, Mathf.Clamp01(num - sqrMagnitude + 0.2f));
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
            SingletonComponent<ServerMgr>.Instance.ConnectionApproved(connection);
        }

        // ConsoleSystem.OnClientCommand()
        public static void ClientConsoleCommand(ConsoleSystem.Arg arg, String rconCmd)
        {
            ClientConsoleEvent ce = new ClientConsoleEvent(arg, rconCmd);
            if (arg.connection != null) {
                OnClientConsole.OnNext(ce);

                if (arg.Invalid) {
                    if (!Net.sv.IsConnected()) {
                        return;
                    }
                    Net.sv.write.Start();
                    Net.sv.write.PacketID(Message.Type.ConsoleMessage);
                    Net.sv.write.String(ce.Reply);
                    Net.sv.write.Send(new SendInfo(arg.connection));
                } else {
                    arg.ReplyWith(ce.Reply);
                }
            }
        }

        // chat.say().Hooks.Chat()
        public static void Command(ConsoleSystem.Arg arg)
        {
            Player player = Server.GetPlayer(arg.Player());
            string[] args = arg.ArgsStr.Substring(2, arg.ArgsStr.Length - 3).Replace("\\", "").Split(new string[]{ " " }, StringSplitOptions.None);

            CommandEvent cmd = new CommandEvent(player, args);

            if (cmd.cmd == "")
                return;

            foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
                ChatCommand[] commands = pl.Value.chatCommands.getChatCommands(cmd.cmd);
                foreach (ChatCommand chatCmd in commands) {
                    if (chatCmd.callback == null)
                        continue;

                    CommandPermissionEvent permission = new CommandPermissionEvent(player, args, chatCmd);
                    OnCommandPermission.OnNext(permission);
                    if (permission.blocked) {
                        player.Message(permission.Reply);
                        continue;
                    }

                    try {
                        chatCmd.callback(cmd.args, player);
                    } catch (Exception ex) {
                        Logger.LogError(chatCmd.plugin.FormatException(ex));
                    }
                }
            }
            OnCommand.OnNext(cmd);

            if (cmd.Reply != "")
                arg.ReplyWith(cmd.Reply);

        }

        public static void OnShoot(BaseProjectile baseProjectile, BaseEntity.RPCMessage msg)
        {
            OnShooting.OnNext(new ShootEvent(baseProjectile, msg));
        }

        public static void ItemUsed(Item item, int amountToConsume)
        {
            OnItemUsed.OnNext(new ItemUsedEvent(item, amountToConsume));
        }

        public static void ProcessResources(MiningQuarry miningQuarry)
        {
            OnMining.OnNext(miningQuarry);
        }

        public static void DoThrow(ThrownWeapon thrownWeapon, BaseEntity.RPCMessage msg)
        {
            OnWeaponThrow.OnNext(new WeaponThrowEvent(thrownWeapon, msg));
        }

        public static void OnRocketShoot(BaseLauncher baseLauncher, BaseEntity.RPCMessage msg, BaseEntity baseEntity)
        {
            OnRocketShooting.OnNext(new RocketShootEvent(baseLauncher, msg, baseEntity));
        }

        public static void Pickup(CollectibleEntity ce, BaseEntity.RPCMessage msg, Item i)
        {
            OnItemPickup.OnNext(new ItemPickupEvent(ce, msg, i));
        }

        public static void ConsumeFuel(BaseOven bo, Item fuel, ItemModBurnable burn)
        {
            OnConsumeFuel.OnNext(new ConsumeFuelEvent(bo, fuel, burn));
        }

        public static void PlayerSleep(BasePlayer bp)
        {
            OnPlayerSleep.OnNext(new Player(bp));
        }

        public static void PlayerWakeUp(BasePlayer bp)
        {
            OnPlayerWakeUp.OnNext(new Player(bp));
        }

        public static void PlayerLoaded(BasePlayer bp)
        {
            OnPlayerLoaded.OnNext(new Player(bp));
        }

        public static void PlayerWounded(BasePlayer bp)
        {
            OnPlayerWounded.OnNext(new Player(bp));
        }

        public static void PlayerAssisted(BasePlayer bp)
        {
            OnPlayerAssisted.OnNext(new Player(bp));
        }

        public static void ItemRepaired(RepairBench rb, BaseEntity.RPCMessage msg)
        {
            OnItemRepaired.OnNext(new ItemRepairEvent(rb, msg));
        }

        public static void PlayerSyringeSelf(SyringeWeapon sw, BaseEntity.RPCMessage msg)
        {
            OnPlayerSyringeSelf.OnNext(new SyringeUseEvent(sw, msg, true));
        }

        public static void PlayerSyringeOther(SyringeWeapon sw, BaseEntity.RPCMessage msg)
        {
            OnPlayerSyringeOther.OnNext(new SyringeUseEvent(sw, msg, false));
        }

        public static void ItemAdded(ItemContainer ic, Item i)
        {
            OnItemAdded.OnNext(new InventoryModEvent(ic, i));
        }
        
        public static void ItemRemoved(ItemContainer ic, Item i)
        {
            OnItemRemoved.OnNext(new InventoryModEvent(ic, i));
        }

        public static void ItemLoseCondition(Item i, float f)
        {
            OnItemLoseCondition.OnNext(new ItemConditionEvent(i, f));
        }

        public static void PlayerClothingChanged(PlayerInventory pi, Item i)
        {
            OnPlayerClothingChanged.OnNext(new PlayerClothingEvent(pi, i));
        }

        public static void BuildingPartDemolished(BuildingBlock bb, BaseEntity.RPCMessage msg)
        {
            OnBuildingPartDemolished.OnNext(new BuildingPartDemolishedEvent(bb, msg.player));
        }

        public static void NetworkableKill(BaseNetworkable bn)
        {
            OnNetworkableKill.OnNext(bn);
        }

        public static void BeingHammered(HitInfo info, BasePlayer ownerPlayer)
        {
            OnBeingHammered.OnNext(new HammerEvent(info, ownerPlayer));
        }

        public static void CombatEntityHurt(BaseCombatEntity combatEnt, HitInfo info)
        {
            try {
                Assert.Test(combatEnt.isServer, "This should be called serverside only");
                if (combatEnt.IsDead()) {
                    return;
                }

                BaseNPC npc = combatEnt.GetComponent<BaseNPC>();
                BaseCorpse corpse = combatEnt.GetComponent<BaseCorpse>();
                BasePlayer player = combatEnt.GetComponent<BasePlayer>();

                if (!SkinnedMeshCollider.ScaleDamage(info)) {
                    if (combatEnt.baseProtection != null) {
                        combatEnt.baseProtection.Scale(info.damageTypes);
                        if (ConVar.Global.developer > 1) {
                            Debug.Log("BaseProtection Scaling for entity :" + combatEnt.name);
                        }
                    }
                }
                else if (ConVar.Global.developer > 1) {
                    Debug.Log("SMC scaling damage for entity :" + combatEnt.name);
                }
                HurtEvent he;
                if (player != null) {
                    Player p = Server.GetPlayer(player);
                    if (p.Teleporting) {
                        for (int i = 0; i < info.damageTypes.types.Length; i++) {
                            info.damageTypes.types[i] = 0f;
                        }
                    }

                    he = new PlayerHurtEvent(p, info);
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
                if (ConVar.Vis.attack) {
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
                        float num = info.damageTypes.types[i];
                        if (num != 0) {
                            string text2 = text;
                            text = String.Concat(new string[] {
                                text2, " ", ((Rust.DamageType)i).ToString().PadRight(10), num.ToString("0.00"), "\r\n"
                            });
                        }
                    }
                    string text3 = String.Concat(new object[] {
                        "<color=lightblue>Damage:</color>".PadRight(10),
                        info.damageTypes.Total().ToString("0.00"),
                        "\r\n<color=lightblue>Health:</color>".PadRight(10),
                        combatEnt.health.ToString("0.00"), " / ",
                        (combatEnt.health - info.damageTypes.Total() > 0) ? "<color=green>" : "<color=red>",
                        (combatEnt.health - info.damageTypes.Total()).ToString("0.00"), "</color>",
                        "\r\n<color=lightblue>Hit Ent:</color>".PadRight(10), combatEnt,
                        "\r\n<color=lightblue>Attacker:</color>".PadRight(10), info.Initiator,
                        "\r\n<color=lightblue>Weapon:</color>".PadRight(10), info.Weapon,
                        "\r\n<color=lightblue>Damages:</color>\r\n", text
                    });
                    ConsoleSystem.Broadcast("ddraw.text", new object[] {
                        60, Color.white, info.HitPositionWorld, text3
                    });
                }
                if (combatEnt.skeletonProperties) {
                    combatEnt.skeletonProperties.ScaleDamage(info);
                }
                if (info.PointStart != Vector3.zero) {
                    DirectionProperties[] directionProperties = (DirectionProperties[])combatEnt.GetFieldValue("propDirection");
                    for (int i = 0; i < directionProperties.Length; i++) {
                        if (!(directionProperties[i].extraProtection == null)) {
                            if (directionProperties[i].IsPointWithinRadius(combatEnt.transform, info.PointStart)) {
                                directionProperties[i].extraProtection.Scale(info.damageTypes);
                            }
                        }
                    }
                }
                combatEnt.health -= info.damageTypes.Total();
                combatEnt.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
                if (ConVar.Global.developer > 1) {
                    Debug.Log(string.Concat(new object[]
		            {
			            "[Combat]".PadRight(10),
			            combatEnt.gameObject.name,
			            " hurt ",
			            info.damageTypes.GetMajorityDamageType(),
			            "/",
			            info.damageTypes.Total(),
			            " - ",
			            combatEnt.health.ToString("0"),
			            " health left"
		            }));
                }
                combatEnt.lastDamage = info.damageTypes.GetMajorityDamageType();
                combatEnt.lastAttacker = info.Initiator;
                if (combatEnt.health <= 0f) {
                    combatEnt.Die(info);
                    BuildingBlock bb = combatEnt.GetComponent<BuildingBlock>();
                    if (bb != null)
                    {
                        OnBuildingPartDestroyed.OnNext(new BuildingPartDestroyedEvent(bb, info));
                    }
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
            string a = rpc.read.String();
            DoorCodeEvent dc = new DoorCodeEvent(doorLock, rpc.player, a);
            OnDoorCode.OnNext(dc);
            if ((!dc.IsCorrect() || !dc.allowed) && !dc.forceAllow)
            {
                Effect.server.Run(doorLock.effectDenied, doorLock, 0u, Vector3.zero, Vector3.forward);
                rpc.player.Hurt(1f, Rust.DamageType.ElectricShock, doorLock, true);
                return;
            }
            Effect.server.Run(doorLock.effectUnlocked, doorLock, 0u, Vector3.zero, Vector3.forward);
            doorLock.SetFlag(BaseEntity.Flags.Locked, false);
            doorLock.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);
            List<ulong> whitelist = new List<ulong>();
            whitelist = (List<ulong>)doorLock.GetFieldValue("whitelistPlayers");
            if (!whitelist.Contains(rpc.player.userID))
            {
                whitelist.Add(rpc.player.userID);
                doorLock.SetFieldValue("whitelistPlayers", whitelist);
            }
        }

        public static void LandmineArmed(Landmine l)
        {
            OnLandmineArmed.OnNext(l);
        }

        public static void LandmineExploded(Landmine l)
        {
            OnLandmineExploded.OnNext(l);
        }

        // Door.RPC_CloseDoor()/RPC_OpenDoor()
        public static void DoorUse(Door door, BaseEntity.RPCMessage rpc, bool open)
        {
            DoorUseEvent due = new DoorUseEvent(new Entity(door), Server.GetPlayer(rpc.player), open);
            OnDoorUse.OnNext(due);

            BaseLock baseLock = door.GetSlot(BaseEntity.Slot.Lock) as BaseLock;
            if (baseLock != null && !due.IgnoreLock) {
                bool TryCloseOpen = open ? !baseLock.OnTryToOpen(rpc.player) : !baseLock.OnTryToClose(rpc.player);
                if (TryCloseOpen)
                    return;
            }

            door.SetFlag(BaseEntity.Flags.Open, due.Open);
            door.Invoke("UpdateLayer", 0f);
            door.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);

            if (due.DenyReason != "")
                rpc.player.SendConsoleCommand("chat.add", 0, String.Format("{0}: {1}", Server.server_message_name.ColorText("fa5"), due.DenyReason));
        }

        // Construiction.Common.CreateConstruction()
        public static BaseEntity DoPlacement(Construction construction, Construction.Target target, bool bNeedsValidPlacement)
        {
            try {
                GameObject gameObject = GameManager.server.CreatePrefab(construction.fullName, default(Vector3), default(Quaternion), true);
                BuildingBlock component = gameObject.GetComponent<BuildingBlock>();

                bool flag = construction.UpdatePlacement(gameObject.transform, construction, target);
                if (bNeedsValidPlacement && !flag) {
                    UnityEngine.Object.Destroy(gameObject);
                    return null;
                }

                BuildingEvent be = null;
                if (component != null) {
                    be = new BuildingEvent(construction, target, component, bNeedsValidPlacement);
                    OnPlacement.OnNext(be);
                }

                if (be != null && be.DoDestroy) {
                    be.Builder.Message(be.DestroyReason);
                    UnityEngine.Object.Destroy(gameObject);
                    return null;
                }

                return gameObject.GetComponent<BaseEntity>();
            } catch (Exception ex) {
                Logger.LogException(ex);
                return null;
            }
        }

        public static void Gathering(ResourceDispenser dispenser, BaseEntity to, ItemAmount itemAmt, int amount)
        {
            itemAmt.amount += amount;
            BaseEntity from = (BaseEntity)dispenser.GetFieldValue("baseEntity");
            GatherEvent ge = new GatherEvent(dispenser, from, to, itemAmt, amount);
            OnGathering.OnNext(ge);
 
            if (ge.Amount > 0) {
 
                if (amount > 0) {
 
                    itemAmt.amount -= amount;
                    if (itemAmt.amount < 0)
                        itemAmt.amount = 0;
 
                    Item item = ItemManager.CreateByItemID(itemAmt.itemid, ge.Amount, false);
                    if (item == null) {
                        return;
                    }
                    to.GiveItem(item);
                }
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

        // ItemCrafter.CraftItem()
        public static bool PlayerStartCrafting(ItemCrafter self, ItemBlueprint bp, BasePlayer owner, ProtoBuf.Item.InstanceData instanceData = null, int amount = 1)
        {
            /*ItemBlueprint bpcopy = new ItemBlueprint();
            bpcopy.amountToCreate = bp.amountToCreate;
            bpcopy.defaultBlueprint = bp.defaultBlueprint;
            bpcopy.ingredients = bp.ingredients;
            bpcopy.rarity = bp.rarity;
            bpcopy.targetItem = bp.targetItem;
            bpcopy.time = bp.time / Server.GetInstance().CraftingTimeScale;
            bpcopy.userCraftable = bp.userCraftable;*/
            CraftEvent ce = new CraftEvent(self, bp, owner, instanceData, amount);
            OnPlayerStartCrafting.OnNext(ce);
            if (!self.CanCraft(bp, 1)) {
                return false;
            }
            if (ce.Cancel) {
                if (ce.cancelReason != "")
                    owner.SendConsoleCommand("chat.add", 0, String.Format("{0}: {1}", Server.server_message_name.ColorText("fa5"), ce.cancelReason));
                return false;
            }
 
            self.taskUID++;
            ItemCraftTask itemCraftTask = new ItemCraftTask();
            itemCraftTask.blueprint = bp;
            if (!ce.FreeCraft) {
                List<Item> list = new List<Item>();
                foreach (ItemAmount current in bp.ingredients) {
                    int amount2 = (int)current.amount * amount;
                    foreach (ItemContainer current2 in self.containers) {
                        amount2 -= current2.Take(list, current.itemid, amount2);
                    }
                }
                foreach (Item current2 in list) {
                    current2.Remove(0f);
                }
            }
            itemCraftTask.endTime = 0;
            itemCraftTask.taskUID = self.taskUID;
            itemCraftTask.owner = owner;
            itemCraftTask.instanceData = instanceData;
            itemCraftTask.amount = amount;
            self.queue.Enqueue(itemCraftTask);
            if (itemCraftTask.owner != null) {
                itemCraftTask.owner.Command("note.craft_add", new object[] {
                    itemCraftTask.taskUID,
                    itemCraftTask.blueprint.targetItem.itemid,
                    amount
                });
            }
            return true;
        }

        // BasePlayer.Die()
        public static void PlayerDied(BasePlayer player, HitInfo info)
        {
            if (info == null) {
                info = new HitInfo();
                info.damageTypes.Add(player.lastDamage, Single.MaxValue);
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
            if (Server.GetInstance().OfflinePlayers.ContainsKey(player.userID))
                Server.GetInstance().OfflinePlayers.Remove(player.userID);
            if (!Server.GetInstance().Players.ContainsKey(player.userID))
                Server.GetInstance().Players.Add(player.userID, p);

            OnPlayerConnected.OnNext(p);
        }

        // BasePlayer.OnDisconnected()
        public static void PlayerDisconnected(BasePlayer player)
        {
            var p = Server.GetPlayer(player);

            if (Server.GetInstance().serverData.ContainsKey("OfflinePlayers", p.SteamID)) {
                OfflinePlayer op = (Server.GetInstance().serverData.Get("OfflinePlayers", p.SteamID) as OfflinePlayer);
                op.Update(p);
                Server.GetInstance().OfflinePlayers[player.userID] = op;
            } else {
                OfflinePlayer op = new OfflinePlayer(p);
                Server.GetInstance().OfflinePlayers.Add(player.userID, op);
            }

            if (Server.GetInstance().Players.ContainsKey(player.userID))
                Server.GetInstance().Players.Remove(player.userID);

            OnPlayerDisconnected.OnNext(p);
        }

        // BasePlayer.UpdateRadiation()
        public static void PlayerTakeRadiation(BasePlayer player, float radAmount)
        {
            var ptr = new PlayerTakeRadsEvent(Server.GetPlayer(player), player.metabolism.radiation_level.value, radAmount);
            OnPlayerTakeRads.OnNext(ptr);
            player.metabolism.radiation_level.value = ptr.Next;
        }
        /*
        // In future create an Event, allow people to adjust certain resources to give certain amounts!
        public static void ResourceGatherMultiplier(int amount, BaseEntity receiver, ItemAmount itemAmt)
        {
            int newAmt = amount;
            if (receiver.ToPlayer() != null)
                newAmt = (int)((double)amount * World.GetInstance().ResourceGatherMultiplier);
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
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Wounded, false);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.SetFieldValue("lastTickTime", 0f);
            player.CancelInvoke("DieFromWounds");
            player.StopSpectating();
            player.UpdateNetworkGroup();
            player.UpdatePlayerCollider(true, false);
            player.StartSleeping();
            player.Invoke("LifeStoryStart", 0f);
            player.metabolism.Reset();

            if (re.StartHealth < Single.Epsilon) {
                player.InitializeHealth(player.StartHealth(), player.StartMaxHealth());
            } else {
                player.InitializeHealth(re.StartHealth, player.StartMaxHealth());
            }

            if (re.GiveDefault)
                player.inventory.GiveDefaultItems();

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
                looter.SendConsoleCommand("chat.add", 0, String.Format("{0}: {1}", Server.server_message_name.ColorText("fa5"), ele.cancelReason));
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
                looter.SendConsoleCommand("chat.add", 0, String.Format("{0}: {1}", Server.server_message_name.ColorText("fa5"), ple.cancelReason));
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
                looter.SendConsoleCommand("chat.add", 0, String.Format("{0}: {1}", Server.server_message_name.ColorText("fa5"), ile.cancelReason));
            }
        }

        // Facepunch.ConsoleSystem.SystemRealm.Normal
        public static void ServerConsoleCommand(ConsoleSystem.Arg arg, string cmd)
        {
            try {
                if (!Bootstrap.PlutonLoaded)
                    return;

                ServerConsoleEvent ssc = new ServerConsoleEvent(arg, cmd);

                foreach (KeyValuePair<string, BasePlugin> pl in PluginLoader.GetInstance().Plugins) {
                    ConsoleCommand[] commands = pl.Value.consoleCommands.getConsoleCommands(ssc.cmd);
                    foreach (ConsoleCommand cc in commands) {
                        if (cc.callback == null)
                            continue;
                        try {
                            cc.callback(arg.ArgsStr.Split(' '));
                        } catch (Exception ex) {
                            Logger.LogError(cc.plugin.FormatException(ex));
                        }
                    }
                }

                OnServerConsole.OnNext(ssc);
                if (arg.Invalid) {
                    Debug.Log(ssc.Reply);
                }
            } catch (Exception ex) {
                Logger.LogException(ex);
            }
        }

        public static void ServerInit()
        {
            Server.GetInstance().SendCommand("plugins.loaded");
            if (Server.GetInstance().Loaded)
                return;

            Server.GetInstance().Loaded = true;
            OnServerInit.OnNext("");
        }

        public static void ServerSaved()
        {
            OnServerSaved.OnNext("");
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
                    string pchGameTags = String.Format("mp{0},cp{1},v{2},procsd{3},procsz{4},modded",
                                             new object[] {
                            ConVar.Server.maxplayers,
                            BasePlayer.activePlayerList.Count,
                            Rust.Protocol.network,
                            global::World.Seed,
                            global::World.Size
                        });

                    Steamworks.SteamGameServer.SetGameTags(pchGameTags);
                }
            } catch (Exception ex) {
                Logger.LogError("[Hooks] Error while setting the server modded.");
                Logger.LogException(ex);
            }
        }

        #endregion
    }
}
