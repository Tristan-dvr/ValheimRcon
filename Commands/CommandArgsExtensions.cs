using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return new ObjectId(args.GetUInt(index), args.GetLong(index + 1));
        }
    }
}
