using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace NineChronicles.Mods.TestMod
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TestModPlugin : BaseUnityPlugin
    {
        private const string ModGUID = "com.ninechronicles.mods.testmod";
        private const string ModName = "Test Mod";
        private const string ModVersion = "0.1.0";

        private static TestModPlugin Instance;

        private readonly Harmony _harmony = new Harmony(ModGUID);

        internal ManualLogSource _logger;

        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
            }

            _logger = BepInEx.Logging.Logger.CreateLogSource(ModGUID);
            _logger.LogInfo($"{ModName} is loaded!");

            _harmony.PatchAll(typeof(TestModPlugin));
            _harmony.PatchAll(typeof(Patches.NotifyKeyboardInput));
        }
    }
}
