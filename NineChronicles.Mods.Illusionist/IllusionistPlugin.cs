using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using NineChronicles.Mods.Illusionist.Patches;

namespace NineChronicles.Mods.Illusionist
{
    [BepInPlugin(Manifest.PluginGuid, Manifest.PluginName, Manifest.PluginVersion)]
    public class IllusionistPlugin : BaseUnityPlugin
    {
        private static IllusionistPlugin _instance;

        private Harmony _harmony;

        public static void Log(LogLevel logLevel, object data)
        {
            _instance.Logger.Log(logLevel, data);
            // Debug.Log(data);
        }

        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _instance = this;

            _harmony = new Harmony(Manifest.PluginGuid);
            _harmony.PatchAll(typeof(IllusionistPlugin));
            _harmony.PatchAll(typeof(SpriteHelperPatch));

            Log(LogLevel.Info, $"{nameof(IllusionistPlugin)}.{nameof(Awake)}() end.");
        }
    }
}
