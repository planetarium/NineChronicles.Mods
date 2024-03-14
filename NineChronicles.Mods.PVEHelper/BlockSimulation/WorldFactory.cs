using System.Collections.Generic;
using System.Collections.Immutable;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Helper;
using Nekoyume.Model.State;
using Nekoyume.Module;
using Nekoyume.TableData;

namespace NineChronicles.Mods.PVEHelper.BlockSimulation
{
    public static class WorldFactory
    {
        public static IWorld CreateWorld() => new World(new MockWorldState());

        public static IWorld WithAdmin(this IWorld world, Address address)
        {
            var state = new AdminState(address, long.MaxValue).Serialize();
            return world.SetLegacyState(Addresses.Admin, state);
        }

        public static IWorld WithAdmin(this IWorld world) => world.WithAdmin(new PrivateKey().Address);

        public static IWorld WithNCG(this IWorld world, IImmutableSet<Address> minters, long amount)
        {
            var context = new ActionContext();
            var ncg = Currency.Legacy("NCG", 2, minters);
            var state = new GoldCurrencyState(ncg);
            return world
                .SetLegacyState(state.address, state.Serialize())
                .MintAsset(context, state.address, ncg * amount);
        }

        public static IWorld WithNCG(this IWorld world) => world.WithNCG(null, 1_000_000_000L);

        public static IWorld WithSheets(this IWorld world, Dictionary<string, string> sheets, Dictionary<string, string> sheetsOverride)
        {
            foreach (var (key, value) in sheets)
            {
                var address = Addresses.TableSheet.Derive(key);
                world = world.SetLegacyState(address, value.Serialize());
            }

            return world;
        }

        public static IWorld WithSheetsInLocalCSV(this IWorld world, Dictionary<string, string> sheetsOverride)
        {
            var sheets = TableSheetsHelper.ImportSheets();
            if (sheetsOverride != null)
            {
                foreach (var (key, value) in sheetsOverride)
                {
                    sheets[key] = value;
                }
            }

            return world.WithSheets(sheets, null);
        }

        public static IWorld WithGameConfig(this IWorld world, GameConfigState gameConfigState)
        {
            return world.SetLegacyState(gameConfigState.address, gameConfigState.Serialize());
        }

        public static IWorld WithGameConfig(this IWorld world)
        {
            var gameConfigSheet = world.GetSheetCsv<GameConfigSheet>();
            var gameConfigState = new GameConfigState(gameConfigSheet);
            return world.WithGameConfig(gameConfigState);
        }
    }
}
