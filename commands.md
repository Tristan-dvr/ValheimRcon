# ValheimRcon - Available Commands

This document contains a complete description of all available RCON commands for the ValheimRcon mod.

---

## Player Management Commands

### kick
Kicks a player from the server  
**Usage:** `kick <playername or steamid>`  
```
kick PlayerName
kick 76561198000000000
```

### banSteamId
Bans a player by their Steam ID  
**Usage:** `banSteamId <steamid>`  
```
banSteamId 76561198000000000
```

### ban
Bans a player by their name or Steam ID  
**Usage:** `ban <playername or steamid>`  
```
ban PlayerName
ban 76561198000000000
```

### unban
Unbans a player from the server  
**Usage:** `unban <playername or steamid>`  
```
unban PlayerName
```

### addAdmin
Adds a player to the admin list  
**Usage:** `addAdmin <steamid>`  
```
addAdmin 76561198000000000
```

### removeAdmin
Removes a player from the admin list  
**Usage:** `removeAdmin <steamid>`  
```
removeAdmin 76561198000000000
```

### addPermitted
Adds a player to the permitted list  
**Usage:** `addPermitted <steamid>`  
```
addPermitted 76561198000000000
```

### removePermitted
Removes a player from the permitted list  
**Usage:** `removePermitted <steamid>`  
```
removePermitted 76561198000000000
```

### adminlist
Shows the list of server administrators  
**Usage:** `adminlist`

### banlist
Shows the list of banned players  
**Usage:** `banlist`

### permitted
Shows the list of permitted players  
**Usage:** `permitted`

---

## Player Interaction Commands

### give
Gives an item to a player  
**Usage:** `give <steamid> <item_name> [options]`

**Options:**
- `-count <count>` - number of items to give (default: 1)
- `-quality <quality>` - item quality level (default: 1)
- `-variant <variant>` - item variant (default: 0)
- `-data <key> <value>` - custom data for the item
- `-nocrafter` - removes crafter information from the item

```
give 76561198000000000 SwordBlackmetal
give 76561198000000000 SwordBlackmetal -count 5 -quality 4
give 76561198000000000 ArrowWood -count 100 -variant 1
give 76561198000000000 OnionSoup -count 10 -data org.bepinex.plugins.cooking#Cooking.Cooking+CookingSkill 099 // gives 10 Onion Soups with 99 Cooking Skill
give 76561198000000000 SwordIron -nocrafter // gives item without crafter information
```

### heal
Heals a player to a specified health value  
**Usage:** `heal <steamid> <value>`  
```
heal 76561198000000000 100
```

### damage
Damages a player by a specified amount  
**Usage:** `damage <steamid> <amount>`  
```
damage 76561198000000000 50
```

### teleport
Teleports a player to a specified position  
**Usage:** `teleport <steamid> <x> <y> <z>`  
```
teleport 76561198000000000 100 50 200
```

### findPlayer
Finds a player and shows their details  
**Usage:** `findPlayer <playername or steamid>`  
```
findPlayer PlayerName
```

### players
Shows all online players with their positions and zones  
**Usage:** `players`

---

## Object Management Commands

### spawn
Creates the specified number of objects at the given position  
**Usage:** `spawn <prefabName> <x> <y> <z> [options]`

**Options:**
- `-count` or `-c <count>` - number of objects to spawn (default: 1)
- `-level` or `-l <level>` - object level (default: 0)
- `-tag` or `-t <tag>` - tag for the object
- `-rotation` or `-rot <x> <y> <z>` - object rotation
- `-radius` or `-rad <radius>` - spawn radius for random placement
- `-tamed` - tame spawned creatures

```
spawn Boar 90 31 90 -count 3 -level 4 -tamed
spawn Rock 100 50 200 -count 5 -radius 10
```

### findObjects
Finds objects matching the provided search criteria (at least one criteria must be provided)  
**Usage:** `findObjects [options]`

**Options:**
- `-near <x> <y> <z> <radius>` - limit search to a radius around position
- `-prefab <prefab>` - search by object type
- `-creator <creator_id>` - search by creator ID (character ID, not Steam ID)
- `-id <id:userid>` - search by specific object ID
- `-tag <tag>` - search by tag
- `-tag-old <tag>` - search by old tag key (before 1.3.1)

```
findObjects -prefab Boar
findObjects -creator 193029
findObjects -tag my_tag
findObjects -near 100 50 200 50 -prefab Boar
findObjects -tag-old battle_boars
```

### deleteObjects
Deletes objects matching the provided search criteria (at least one criteria must be provided)  
**Usage:** `deleteObjects [options]`

**Options:**
- `-creator <creator_id>` - delete by creator ID
- `-id <id:userid>` - delete by specific object ID
- `-tag <tag>` - delete by tag
 - `-force` - bypass safety checks and force deletion

```
deleteObjects -creator 193029
deleteObjects -id 12345:67890
deleteObjects -tag temp_objects
deleteObjects -id 12345:67890 -force
```

### modifyObject
Modifies properties of a persistent object.  
**Usage:** `modifyObject <id:userid> [options]`

**Options:**
- `-position <x> <y> <z>` - set object position
- `-rotation <x> <y> <z>` - set object rotation
- `-health <value>` - set object health (if applicable)
- `-tag <tag>` - set custom tag for the object
- `-force` - bypass ownership and modification safety checks

Notes:
- The command will refuse to modify non-persistent objects (completely controlled by players).
- By default the command will not modify objects owned by online players or protected by the server; use `-force` to override these checks.
- Changes to position, rotation or health of objects currently owned by players may not be immediately visible to those players.

```
modifyObject 12345:67890 -position 100 50 200 -rotation 0 90 0
modifyObject 12345:67890 -health 250
modifyObject 12345:67890 -tag my_new_tag
modifyObject 12345:67890 -position 100 50 200 -force
```

---

## Chat and Message Commands

### say
Sends a message to the chat as a shout  
**Usage:** `say <message>`  
```
say Hello everyone!
```

### showMessage
Displays a message in the center of the screen for all players  
**Usage:** `showMessage <message>`  
```
showMessage Server restart in 5 minutes!
```

### ping
Sends a ping message to all players at the specified coordinates  
**Usage:** `ping <x> <y> <z>`  
```
ping 100 50 200
```

---

## Server Commands

### serverStats
Shows server statistics including player count, FPS, memory usage, and world information  
**Usage:** `serverStats`

### time
Shows current server time and day  
**Usage:** `time`

### save
Saves the current world state  
**Usage:** `save`

### logs
Gets server logs (last 5 lines)  
**Usage:** `logs`

### consoleCommand
Executes a console command on the server  
**Usage:** `consoleCommand <command>`  

---

## Global Key Commands

### addGlobalKey
Adds a global key to the server  
**Usage:** `addGlobalKey <key>`  
```
addGlobalKey defeated_bonemass
```

### removeGlobalKey
Removes a global key from the server  
**Usage:** `removeGlobalKey <key>`  
```
removeGlobalKey defeated_bonemass
```

### globalKeys
Shows all global keys and their values  
**Usage:** `globalKeys`

---

## Notes

1. **Steam ID** - unique player identifier in Steam (64-bit number)
2. **Creator ID** - unique character identifier on the server (not Steam ID), visible on objects built by the player
3. **Prefabs** - object names in the game (e.g., Boar, Rock, SwordBlackmetal). See [complete list of Valheim prefabs](https://valheim-modding.github.io/Jotunn/data/prefabs/overview.html)
4. **Specific Object ID** - consists of `id:userid` (colon-separated). `userid` can be negative. Visible on objects found through `findObjects`
5. **Coordinates** - position in Valheim world (x, y, z)
6. **Tags** - custom labels for objects
7. **Global Keys** - game system flags (e.g., defeated_bonemass)