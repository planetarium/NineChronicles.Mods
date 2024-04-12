using Libplanet.Crypto;

namespace NineChronicles.Mods.Athena.Models
{
    public class AvatarInfo
    {
        public int Cp { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public double WinRate { get; set; } = -1;
    }
}