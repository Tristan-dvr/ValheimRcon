using BepInEx.Logging;

namespace ValheimRcon.Tests
{
    internal class MockLogSource : ManualLogSource
    {
        public MockLogSource() : base("Mock")
        {
        }
    }
}
