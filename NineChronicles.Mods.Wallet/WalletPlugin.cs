using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace NineChronicles.Mods.Wallet
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class WalletPlugin : BaseUnityPlugin
    {
        private const string PluginGUID = "org.ninechronicles.mods.wallet";
        private const string PluginName = "Wallet";
        private const string PluginVersion = "0.0.1";

        private const string PluginLastDayOfUseKey = PluginName + "_Last_Day_Of_Use";
        private const string PluginDailyOpenKey = PluginName + "_Daily_Open";

        internal static WalletPlugin Instance { get; private set; }

        private Harmony _harmony;

        public static void Log(LogLevel logLevel, object data)
        {
            Instance?.Logger.Log(logLevel, data);
        }

        public static void Log(object data) => Log(LogLevel.Info, data);

        private void Awake()
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException($"{nameof(WalletPlugin)} must be only one instance.");
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(typeof(WalletPlugin));

            Logger.LogInfo("Loaded");
        }

        private void DisableModeGUI()
        {
            //_enhancementGUI = null;
            //// _equipGUI = null;
            //_inventoryGUI = null;
            //_itemCreationGUI = null;
            //_notificationGUI = null;
            //_tabGUI = null;
            //_stageSimulateGUI = null;
            EnableEventSystem();
        }
    }
}
