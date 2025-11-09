### 1.4.1
- Added container inventory management commands:
  - `showContainer <id:userid>` - display inventory contents with item indices `[index]` for each item
  - `addItemToContainer <id:userid> <item_name> [options]` - add items to container
    - options: `-count <count>` `-quality <quality>` `-variant <variant>` `-data <key> <value>`
    - Default durability is set to maximum for the specified quality level`-nocrafter` `-durability <durability>` `-force`
  - `removeItemFromContainer <id:userid> [options]` - remove items from container
    - options: `-index <index>` (recommended) or `-item <name>`, `-count <count>`, `-force`
    - Use `-index` to precisely target items when multiple items with the same name exist
  - `clearContainer <id:userid> [options]` - clear container inventory
    - options: `-force`
- Added `-durability <durability>` option to `give` command
  - Default durability is set to maximum for the specified quality level
  - Use `-durability` to override the default value

### 1.4.0
- Changed custom object tag ZDO key to avoid conflict with portal tag (breaking: existing tagged objects created with older versions will not be found by tag)
- Removed `findObjectsNear` command; use `findObjects -near <x> <y> <z> <radius>` instead
- Expanded `findObjects` with `-near` filter to search within a radius around a position
- Added `-tag-old <tag>` to `findObjects` to search objects that use the old tag key
- Added `modifyObject` command to modify properties of persistent objects
  - Supports changing position, rotation, health and custom tag
  - Will not modify non-persistent objects or objects owned by online players unless `-force` is used to bypass safety checks
  - Note: changes to position, rotation or health of objects currently owned by players may not be immediately visible to those players
- Added `disconnectAll` command to disconnect all connected players from the server
- Added Random Events commands:
  - `eventsList` - list all available random events
  - `startEvent <event_name> <x> <y> <z>` - start a random event at the specified position
  - `stopEvent` - stop the currently active random event
  - `currentEvent` - show the currently active random event and position

### 1.3.0
- deleteObjects: added `-force` flag to bypass safety checks; use with extreme caution as it can delete critical game objects (zones, dungeons, player models)
- Simplified object id to `id:userid` format

### 1.2.2
- Fixed rcon packet size limit issue (huge thanks to **ourbob** for finding the issue)

### 1.2.1
- Improved give command

### 1.2.0
- Improved command execution result delivery to RCON clients and Discord
- Fixed errors when RCON clients disconnect (huge thanks to **ourbob** for finding and fixing the issue)
- Added deleteObjects command for removing objects by selected criteria
- Improved interface of some commands, added optional arguments

### 1.1.0
- Added new commands
    - to manage global keys
    - to show server time
    - to find objects by name and creator id and get their information
    - to execute Valheim console commands on the server
    - to get player information
    - to show all available commands

### 1.0.1 
- Fixed updating server time while no players online

### 1.0.0 
Public release