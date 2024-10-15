using Libplanet.Action;

namespace NineChronicles.Modules.BlockSimulation.Factories
{
    public static class ActionContextFactory
    {
        public static IActionContext CreateDefault() => new ActionContext(
            signer: default,
            txid: default,
            miner: default,
            blockIndex: default,
            blockProtocolVersion: default,
            previousState: default,
            randomSeed: default,
            isPolicyAction: default,
            gasLimit: default,
            txs: default,
            evidence: default);
    }
}
