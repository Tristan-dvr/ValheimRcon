using BepInEx.Configuration;
using System;
using System.IO;

class Helper
{
    public static void WatchConfigFileChanges(ConfigFile config, Action onChanged = null)
    {
        var path = config.ConfigFilePath;
        WatchFileChanges(path, config.Reload);

        config.SettingChanged += (a, b) => onChanged?.Invoke();
    }

    public static void WatchFileChanges(string path, Action onChanged)
    {
        var watcher = new FileSystemWatcher();
        var directory = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        watcher.Path = directory;
        watcher.Filter = fileName;
        watcher.NotifyFilter = NotifyFilters.LastWrite
            | NotifyFilters.FileName 
            | NotifyFilters.DirectoryName;

        watcher.Changed += (obj, arg) => onChanged?.Invoke();
        watcher.Deleted += (obj, arg) => onChanged?.Invoke();
        watcher.Created += (obj, arg) => onChanged?.Invoke();
        watcher.Renamed += (obj, arg) => onChanged?.Invoke();

        watcher.EnableRaisingEvents = true;
    }
}
