# ValheimRcon
 
This plugin adds RCON protocol support to your Valheim server.

- Introduces several commands for interacting with the server remotely
- Supports sending command execution results to Discord
- Provides an extensible RCON command interface that can be used by third-party mods
- Advanced security features:
	- IP address filtering with CIDR mask support (whitelist/blacklist)
	- Security incident logging to Discord (failed logins, unauthorized access attempts, IP filter rejections)
	- Empty password protection (plugin disabled if password is empty)
	- Automatic disconnection of unauthorized clients

# Usage
1. Install the plugin to your Valheim server
2. Start the server to generate the configuration file
3. Configure the plugin in `BepInEx/config/org.tristan.rcon.cfg`:
	- Set the RCON port (default is server port + 2)
	- Change the RCON password. Empty password will disable the plugin.
	- Configure IP filtering (optional):
		- Whitelist: comma-separated list of allowed IP addresses or CIDR masks (e.g., `192.168.1.0/24, 10.0.0.1`)
		- Blacklist: comma-separated list of blocked IP addresses or CIDR masks
		- If whitelist is empty, all IPs are allowed (except blacklisted)
	- Configure Discord webhook to see command results in your Discord (optional but recommended)
	- Configure security Discord webhook for security incident reports (optional but recommended)
4. Restart the server to apply the changes
5. Connect to the server using an RCON client (web or desktop application)
	- For example, you can use this website https://cmsminecraftshop.com/en/console/.
Select Conan Exiles (it has similar RCON packet structure), then enter your server IP, RCON port and password
	- Or use desktop RCON client like mcrcon -> https://sourceforge.net/projects/mcrcon/
6. After connecting you will be able to execute commands in the console.

## IP Address Filtering with CIDR

The plugin supports CIDR notation for IP address filtering. Use CIDR masks to allow or block entire IP ranges:

- **Single IP**: `192.168.1.1` (equivalent to `192.168.1.1/32`)
- **Subnet**: `192.168.1.0/24` (allows IPs from 192.168.1.0 to 192.168.1.255)
- **Larger network**: `10.0.0.0/8` (allows IPs from 10.0.0.0 to 10.255.255.255)
- **Multiple entries**: `192.168.1.0/24, 10.0.0.1, 172.16.0.0/16` (comma-separated)

**Examples:**
- Allow only local network: `Whitelist IP mask = 192.168.1.0/24`
- Block specific IP range: `Blacklist IP mask = 203.0.113.0/24`
- Allow multiple networks: `Whitelist IP mask = 192.168.1.0/24, 10.0.0.0/8`

# Command List
View all available commands:
```
list
```

[All commands](https://github.com/Tristan-dvr/ValheimRcon/blob/master/commands.md) - detailed description with examples.

### Notice
- Player management commands (kick, ban, heal, damage) should be used carefully
- Object deletion commands are irreversible
- The `-force` flag in `deleteObjects` bypasses safety checks and can remove critical game objects (zone controllers, dungeons etc.). Use with extreme caution!
- Not persistent objects (completely controlled by players) cannot be deleted or modified

If result of command execution is too long, it will be truncated in the RCON client, but the full result will be sent to Discord if you have configured a webhook URL in the plugin settings.
For example if you execute findObjects command, it could return a lot of objects, and the RCON client will show only few of them. But the full result will be sent to Discord.

### Examples
Gives player **Ragnar** level 4 Blackmetal Sword:
```
give Ragnar SwordBlackmetal -quality 4
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
If you are a mod developer and want to add your own RCON commands for your server, please refer to the [documentation](https://github.com/Tristan-dvr/ValheimRcon/blob/master/add-custom-command.md).

You can also extend object information display by registering custom ZDO info providers via `ZDOInfoUtil.RegisterInfoProvider()`.

# Contacts
If you have any questions / bug reports / suggestions for improvement or found incompatibility with another mod, feel free to contact me in discord `typedeff` or on GitHub 