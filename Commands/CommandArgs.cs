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

        public string GetString(int index)
        {
            ValidateIndex(index);

            return Arguments[index];
        }

        private void ValidateIndex(int index)
        {
            if (index >= 0 && index < Arguments.Count)
                return;

            throw new ArgumentException($"Cannot get argument at {index}");
        }

        public override string ToString() => string.Join(" ", Arguments);
    }
}
