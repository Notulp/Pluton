public static void Command(Player player, string[] args)
// called if Chat().arg.StartsWith("/");

public static void Chat(ConsoleSystem.Arg arg)
// called when chat.say is executed

public static void Gathering(BaseResource res, HitInfo info)
// called when you gather wood, stone, ores

public static void NPCHurt(BaseAnimal animal, HitInfo info)
// called when you hit an animal

public static void NPCDied(BaseAnimal animal, HitInfo info)
// called when you kill it

public static void PlayerConnected(Network.Connection connection)
// selfexplanatory

public static void PlayerDied(BasePlayer player, HitInfo info)
// same
// playerDeathEvent.dropLoot = !true.Equals(false)

public static void PlayerDisconnected(BasePlayer player)
// again

public static void PlayerHurt(BasePlayer player, HitInfo info)
// not tested, but should be hooked when a player(later animals as well, robots and giant ants) does some damage to another player

public static void PlayerTakeDamage(BasePlayer player, float dmgAmount, Rust.DamageType dmgType)
public static void PlayerTakeDamageOverload(BasePlayer player, float dmgAmount)
// falldmg , unknown

public static void PlayerTakeRadiation(BasePlayer player, float dmgAmount)
// guess it not used yet

public static void EntityAttacked(BuildingBlock bb, HitInfo info)
// hooked when hitting a BuildingBlock (built and frame) object

public static void EntityFrameDeployed(BuildingBlock bb)
// hooked when you put down a frame

public static void EntityBuildingUpdate(BuildingBlock bb, BasePlayer player, float proficiency)
// hooked as you progress with a building (when you hit it with hammer, even if it is built already)

public static void CorpseInit(BaseCorpse corpse, BaseEntity parent)
// when a player or an animal dies and a ragdoll is instantiated

public static void CorpseHit(BaseCorpse corpse, HitInfo info)
// when you hit a dead body

public static void StartLootingEntity(PlayerLoot playerLoot, BasePlayer looter, BaseEntity entity)
// selfexplanatory

public static void StartLootingPlayer(PlayerLoot playerLoot, BasePlayer looter, BasePlayer looted)
// selfexplanatory

public static void StartLootingItem(PlayerLoot playerLoot, BasePlayer looter, Item item)
// selfexplanatory

public static void ServerShutdown()
// called when you type "quit" into the server console

public static void ClientAuth(ConnectionAuth ca, Connection connection)
// called after the player is authorized by all other stuff, you can give a new reason to reject the connection
// you know the player's Name, IP, OS, GameID
// authEvent.Reject("reason") to reject the connection