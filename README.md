# ValheimRcon
 
This plugin adds RCON protocol support to your Valheim server.

- Introduces several commands for interacting with the server remotely
- Supports sending command execution results to Discord
- Provides an extensible RCON command interface that can be used by third-party mods


# Command List
### Chat
Sends a message to the global chat
```
say {text}
```
Displays a message in the center of the screen for all players
```
showMessage {text}
```
Displays a ping at the specified coordinates
```
ping {x} {y} {z}
```

### Player Interaction
Deals damage to the player in the specified amount
```
damage {steam id or nickname} {damage amount}
```
Heals the player by the specified amount of HP
```
heal {steam id or nickname} {heal amount}
```
Creates an item at the player's coordinates with the given amount and level. *If the player is not moving, the item should be added to their inventory immediately*
```
give -> GiveItem
```
Teleports the player to the specified coordinates
```
teleport {steam id or nickname} {x} {y} {z}
```

### Utility Commands
Creates an object (any available in the game) at the specified coordinates
```
spawn -> SpawnObject
```
Adds an admin
```
addAdmin {steam id}
```
Removes an admin
```
removeAdmin {steam id}
```
Adds a player to the whitelist
```
addPermitted {steam id}
```
Removes a player from the whitelist
```
removePermitted {steam id}
```
Displays the list of admins
```
adminlist
```
Displays the list of banned players
```
banlist
```
Displays the list of players in the whitelist
```
permitted
```
Disconnects a player from the server
```
kick {steam id or nickname}
```
Bans a player by nickname or steam ID
```
ban {steam id or nickname}
```
Bans a player by steam ID
```
banSteamId {steam id}
```
Unbans a player by name or steam ID
```
unban {steam id or nickname}
```
Displays the list of online players
```
players
```
Displays a set of server stats (FPS, load, etc.)
```
serverStats
```
Shows the last few lines of the server logs. The full server log file is also sent to Discord
```
logs
```
Saves the world
```
save
```

# Adding custom rcon commands