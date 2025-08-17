using System;
using System.Collections.Generic;

namespace ValheimRcon.Commands
{
    public class CommandArgs
    {
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

        public override string ToString() => string.Join(" ", Arguments);
    }
}
