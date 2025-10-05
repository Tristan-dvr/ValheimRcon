using System;
using UnityEngine;

namespace ValheimRcon.Commands
{
    public static class CommandArgsExtensions
    {
        public static Vector3 GetVector3(this CommandArgs args, int index)
        {
            return new Vector3(
                args.GetFloat(index),
                args.GetFloat(index + 1),
                args.GetFloat(index + 2));
        }

        public static ObjectId GetObjectId(this CommandArgs args, int index)
        {
            var text = args.GetString(index);
            var parts = text.Split(':');
            if (parts.Length == 2
                && uint.TryParse(parts[0], out var id)
                && long.TryParse(parts[1], out var userId))
            {
                return new ObjectId(id, userId);
            }
            else
            {
                throw new ArgumentException($"Cannot parse {text} as object id (expected format is ID:User)");
            }
        }
    }
}
