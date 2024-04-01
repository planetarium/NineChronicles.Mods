using System;
using Cysharp.Threading.Tasks;
using Nekoyume;
using UnityEngine;

namespace NineChronicles.Mods.Shared.Utils
{
    public static class AnalyticsUtils
    {
        public static async void TrackOnce(string pluginName)
        {
            if (Analyzer.Instance is null)
            {
                await UniTask.WaitUntil(() => Analyzer.Instance is not null);
            }

            var days = (DateTime.Now - new DateTime(2019, 3, 11)).Days;
            if (days > PlayerPrefs.GetInt(GetPluginLastDayOfUseKey(pluginName), 0))
            {
                Analyzer.Instance.Track(GetPluginDailyOpenKey(pluginName));
                PlayerPrefs.SetInt(GetPluginLastDayOfUseKey(pluginName), days);
            }
        }

        private static string GetPluginLastDayOfUseKey(string pluginName)
        {
            return $"{pluginName}_Last_Day_Of_Use";
        }

        private static string GetPluginDailyOpenKey(string pluginName)
        {
            return $"{pluginName}_Daily_Open";
        }
    }
}
