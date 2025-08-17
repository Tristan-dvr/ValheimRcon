using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace ValheimRcon.Commands
{
    internal class GetServerStats : RconCommand
    {
        public override string Command => "serverStats";

        public override string Description => "Prints server statistics including player count, FPS, memory usage, and world information.";

        private StringBuilder builder = new StringBuilder();

        protected override string OnHandle(CommandArgs args)
        {
            builder.Clear();

            var worldName = ZNet.World?.m_name ?? "INVALID WORLD";
            var playersCount = ZNet.instance?.m_peers.Count ?? -1;
            var fps = 1f / Time.deltaTime;
            var totalZdoCount = ZDOMan.instance?.m_objectsByID?.Count ?? -1;
            var day = EnvMan.instance?.GetDay(ZNet.instance?.GetTimeSeconds() ?? 0) ?? -1;
            var deadZdos = ZDOMan.instance?.m_deadZDOs.Count ?? 0;
            var monoMemory = ToMegabytes(Profiler.GetMonoUsedSizeLong());
            var monoHeapSize = ToMegabytes(Profiler.GetMonoHeapSizeLong());

            builder.AppendLine($"Stats - Online {playersCount} FPS {fps:0.0}");
            builder.AppendLine($"Memory - Mono {monoMemory}MB, Heap {monoHeapSize}MB");
            builder.Append($"World - Day {day}, Objects {totalZdoCount}, Dead objects {deadZdos}");

            return builder.ToString();
        }

        private int ToMegabytes(long bytes)
        {
            return Mathf.FloorToInt(bytes / 1048576f);
        }
    }
}
