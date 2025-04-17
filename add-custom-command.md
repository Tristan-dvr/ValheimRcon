# Adding custom commands

You can easily create a plugin that add your own RCON commands for your server.

1. To do this, you need to create a plugin and reference `ValheimRcon.dll` in it
2. Create a class that inherits from `RconCommand`
``` c#
internal class Kick : RconCommand
{
    public override string Command => "kick";
    
    protected override string OnHandle(CommandArgs args)
    {
        var user = args.GetString(0);
        ZNet.instance.Kick(user);
        return $"Kicked {user}";
    }
}
```
3. Call the function `RconCommandsUtil.RegisterAllCommands(Assembly.GetExecutingAssembly());` in the `Awake` method of your `BaseUnityPlugin`
```
[BepInDependency(ValheimRcon.Plugin.Guid)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        RconCommandsUtil.RegisterAllCommands(Assembly.GetExecutingAssembly());
    }
}
```

If everything is successful, you will see something like this in your server log at startup:

```
[Info   :Valheim Rcon] [14/04/2025 18:44:00] Registering rcon commands...
[Info   :Valheim Rcon] [14/04/2025 18:44:00] Registered command addAdmin -> AddAdmin
[Info   :Valheim Rcon] [14/04/2025 18:44:00] Registered command addPermitted -> AddPermitted

```
4. Use your custom RCON commands just like any other command
5. That's it
