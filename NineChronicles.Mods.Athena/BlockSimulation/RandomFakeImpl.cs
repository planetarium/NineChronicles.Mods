using Libplanet.Action;

namespace NineChronicles.Mods.Athena.BlockSimulation
{
    public class RandomFakeImpl : IRandom
    {
        private readonly System.Random _random;

        public int Seed { get; private set; }
        public decimal FakeRatio { get; private set; }

        public RandomFakeImpl(int seed = default, decimal fakeRatio = 1m)
        {
            Seed = seed;
            FakeRatio = fakeRatio;
            _random = new System.Random(Seed);
        }

        public int Next() => _random.Next();

        public int Next(int maxValue) => (int)(maxValue * FakeRatio);

        public int Next(int minValue, int maxValue) => minValue + (int)((maxValue - minValue) * FakeRatio);

        public void NextBytes(byte[] buffer) => _random.NextBytes(buffer);

        public double NextDouble() => _random.NextDouble();
    }
}
