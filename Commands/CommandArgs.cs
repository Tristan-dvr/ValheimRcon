using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ValheimRcon.Commands
{
    public class CommandArgs
    {
        private static readonly Regex OptionalArgumentRegex = new Regex(@"^-[A-Za-z]+$");

        public IReadOnlyList<string> Arguments { get; }

        public CommandArgs(IReadOnlyList<string> args)
        {
            Arguments = args;
        }

        public int GetInt(int index)
        {
            ValidateIndex(index);

            if (!int.TryParse(Arguments[index], out var value))
                throw new ArgumentException($"Argument at {index} is invalid");

            return value;
        }

        public int TryGetInt(int index, int defaultValue = 0)
        {
            return HasArgument(index)
                ? GetInt(index)
                : defaultValue;
        }

        public long GetLong(int index)
        {
            ValidateIndex(index);
            if (!long.TryParse(Arguments[index], out long value))
                throw new ArgumentException($"Argument at {index} is invalid");

            return value;
        }

        public long TryGetLong(int index, long defaultValue)
        {
            return HasArgument(index)
                ? GetLong(index)
                : defaultValue;
        }

        public float GetFloat(int index)
        {
            ValidateIndex(index);
            if (!float.TryParse(Arguments[index], out float value))
                throw new ArgumentException($"Argument at {index} is invalid");

            return value;
        }

        public float TryGetFloat(int index, float defaultValue)
        {
            return HasArgument(index)
                ? GetFloat(index)
                : defaultValue;
        }

        public string GetString(int index)
        {
            ValidateIndex(index);

            return Arguments[index];
        }

        public string TryGetString(int index, string defaultValue = "")
        {
            return HasArgument(index)
                ? GetString(index)
                : defaultValue;
        }

        public uint GetUInt(int index)
        {
            ValidateIndex(index);
            if (!uint.TryParse(Arguments[index], out uint value))
                throw new ArgumentException($"Argument at {index} is invalid");
            return value;
        }

        public uint TryGetUInt(int index, uint defaultValue = 0)
        {
            return HasArgument(index)
                ? GetUInt(index)
                : defaultValue;
        }

        private void ValidateIndex(int index)
        {
            if (HasArgument(index))
                return;

            throw new ArgumentException($"Cannot get argument at {index}");
        }

        private bool HasArgument(int index)
        {
            return index >= 0 && index < Arguments.Count;
        }

        public IEnumerable<int> GetOptionalArguments()
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (OptionalArgumentRegex.IsMatch(Arguments[i]))
                {
                    yield return i;
                }
            }
        }

        public override string ToString() => string.Join(" ", Arguments);
    }
}
