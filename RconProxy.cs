using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ValheimRcon.Commands;
using ValheimRcon.Core;

namespace ValheimRcon
{
    public class RconProxy : MonoBehaviour
    {
        internal delegate void CompletedCommandDelegate(IRconPeer peer, string command, IReadOnlyList<string> args, CommandResult result);

        private RconCommandReceiver _receiver;

        public static RconProxy Instance { get; private set; }

        private Dictionary<string, IRconCommand> _commands = new Dictionary<string, IRconCommand>();
        private IRconConnectionManager _connectionManager;

        internal event CompletedCommandDelegate OnCommandCompleted;
        internal event SecurityReportHandler OnSecurityReport;

        private void Awake()
        {
            Instance = this;

            _connectionManager = new AsynchronousSocketListener(IPAddress.Any,
                Plugin.Port.Value,
                HandleSecurityReport,
                Plugin.IpFilter);
            _receiver = new RconCommandReceiver(_connectionManager,
                Plugin.Password.Value,
                HandleCommandAsync,
                HandleSecurityReport);
        }

        internal void Startup()
        {
            _connectionManager.StartListening();
        }

        internal void ShutDown()
        {
            _receiver.Dispose();
            _connectionManager.Dispose();
        }

        private void Update()
        {
            _receiver.Update();
        }

        private void HandleSecurityReport(object endPoint, string reason) => OnSecurityReport?.Invoke(endPoint, reason);

        public void RegisterCommand<T>() where T : IRconCommand => RegisterCommand(typeof(T));

        public void RegisterCommand(Type type)
        {
            var commandInstance = Activator.CreateInstance(type) as IRconCommand;
            if (commandInstance == null || string.IsNullOrEmpty(commandInstance.Command)) return;

            RegisterCommand(commandInstance);
        }

        public void RegisterCommand(IRconCommand command)
        {
            if (_commands.TryGetValue(command.Command, out var existendCommand))
            {
                Log.Error($"Duplicated commands {command.Command}" +
                    $"\n{command.GetType().Name}" +
                    $"\n{command.GetType().Name}");
            }

            _commands[command.Command] = command;
            Log.Info($"Registered command {command.Command} -> {command.GetType().Name}");
        }

        public void RegisterCommand(string command, string description, Func<CommandArgs, CommandResult> commandFunc)
        {
            RegisterCommand(new ActionCommand(command, description, commandFunc));
        }

        private async Task<string> HandleCommandAsync(IRconPeer peer, string command, IReadOnlyList<string> args)
        {
            var completionSource = new TaskCompletionSource<CommandResult>();
            ThreadingUtil.RunInMainThread(() => RunCommand(command, args, completionSource));
            var result = await completionSource.Task;
            Log.Message($"Command completed: {command}\n{result.Text}");
            OnCommandCompleted?.Invoke(peer, command, args, result);
            return result.Text;
        }

        private async void RunCommand(string commandName, IReadOnlyList<string> args, TaskCompletionSource<CommandResult> resultSource)
        {
            try
            {
                if (!_commands.TryGetValue(commandName, out var command))
                {
                    resultSource.TrySetResult(new CommandResult
                    {
                        Text = $"Unknown command {commandName}",
                    });
                    return;
                }

                var result = await command.HandleCommandAsync(new CommandArgs(args));
                resultSource.TrySetResult(result);
            }
            catch (Exception e)
            {
                resultSource.TrySetResult(new CommandResult { Text = e.Message });
            }
        }

        [HarmonyPatch]
        private class Patches
        {
            [HarmonyFinalizer]
            [HarmonyPatch(typeof(ZNet), nameof(ZNet.LoadWorld))]
            private static void ZNet_LoadWorld()
            {
                Instance.Startup();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Game), nameof(Game.Shutdown))]
            private static void Game_Shutdown()
            {
                Instance.ShutDown();
            }
        }

        private class ListCommand : RconCommand
        {
            public override string Command => "list";

            public override string Description => "Prints list of available commands.";

            protected override string OnHandle(CommandArgs args)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Available commands:");
                foreach (var command in Instance._commands.Values)
                {
                    stringBuilder.AppendLine($"{command.Command} - {command.Description}");
                }
                return stringBuilder.ToString().Trim();
            }
        }
    }
}
