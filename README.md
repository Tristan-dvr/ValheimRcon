# ValheimRcon
 
This plugin adds RCON protocol support to your Valheim server.

- Introduces several commands for interacting with the server remotely
- Supports sending command execution results to Discord
- Provides an extensible RCON command interface that can be used by third-party mods
- Plugin is protected against most potential threats, preventing unauthorized access to the server via the RCON protocol

# Usage
1. Install the plugin to your Valheim server
2. Start the server to generate the configuration file
3. Configure the plugin in `BepInEx/config/org.tristan.rcon.cfg`:
	- Set the RCON port (default is server port + 2)
	- Change the RCON password. **Do not leave the default password, as it is insecure!**
	- Provide a Discord webhook URL to see command results in your Discord (optional but recommended)
4. Restart the server to apply the changes
5. Connect to the server using an RCON client (web or desktop application)
	- For example, you can use this website https://cmsminecraftshop.com/en/console/.
Select Conan Exiles (it has similar RCON packet structure), then enter your server IP, RCON port and password
	- Or use desktop RCON client like mcrcon -> https://sourceforge.net/projects/mcrcon/
6. After connecting you will be able to execute commands in the console.

# Command List
View all available commands:
```
list
```

[All commands](https://github.com/Tristan-dvr/ValheimRcon/blob/master/commands.md) - detailed description with examples.

### Notice
- Player management commands (kick, ban, heal, damage) should be used carefully
- Object deletion commands are irreversible

If result of command execution is too long, it will be truncated in the RCON client, but the full result will be sent to Discord if you have configured a webhook URL in the plugin settings.
For example if you execute findObjects command, it could return a lot of objects, and the RCON client will show only few of them. But the full result will be sent to Discord.

### Examples
Gives player **Ragnar** level 4 Blackmetal Sword:
```
give Ragnar SwordBlackmetal 4 1
```
Spawns 4 tamed level 3 **Boars** at coordinates x:90 y:31 z:90:
```
spawn Boar 90 31 90 -count 4 -level 3 -tamed
```
Sends a message `Hello everyone!` to the global chat:
```
say Hello everyone!
```

## Custom commands
If you are a modder and want to add your own RCON commands for your server, read [this documentation](https://github.com/Tristan-dvr/ValheimRcon/blob/master/add-custom-command.md).

# Contacts
If you have any questions / bug reports / suggestions for improvement or found incompatibility with another mod, feel free to contact me in discord `typedeff` or on GitHub 