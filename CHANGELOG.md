### 1.4.0
- Changed custom object tag ZDO key to avoid conflict with portal tag (breaking: existing tagged objects created with older versions will not be found by tag)
- Removed `findObjectsNear` command; use `findObjects -near <x> <y> <z> <radius>` instead
- Expanded `findObjects` with `-near` filter to search within a radius around a position
- Added `-tag-old <tag>` to `findObjects` to search objects that use the old tag key
- Added `modifyObject` command to modify properties of persistent objects
  - Supports changing position, rotation, health and custom tag
  - Will not modify non-persistent objects or objects owned by online players unless `-force` is used to bypass safety checks
  - Note: changes to position, rotation or health of objects currently owned by players may not be immediately visible to those players

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