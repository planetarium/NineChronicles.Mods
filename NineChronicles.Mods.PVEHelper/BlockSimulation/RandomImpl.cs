using Libplanet.Action;

namespace NineChronicles.Mods.PVEHelper.BlockSimulation
{
    public class RandomImpl : IRandom
    {
        private readonly System.Random _random;

        public int Seed { get; private set; }

        public RandomImpl(int seed = default)
        {
            Seed = seed;
            _random = new System.Random(Seed);
        }

        public int Next() => _random.Next();

        public int Next(int maxValue) => _random.Next(maxValue);

        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

        public void NextBytes(byte[] buffer) => _random.NextBytes(buffer);

        public double NextDouble() => _random.NextDouble();
    }
}
