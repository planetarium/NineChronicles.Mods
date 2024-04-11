using System.Collections.Generic;
using Nekoyume.Game;
using Nekoyume.Model.Item;
using NineChronicles.Mods.Athena.Components;
using NineChronicles.Mods.Athena.Extensions;
using NineChronicles.Mods.Athena.Factories;
using NineChronicles.Mods.Athena.Manager;
using NineChronicles.Mods.Athena.Models;
using Nekoyume.UI;
using Nekoyume.UI.Model;
using UnityEngine;
using Nekoyume.GraphQL;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace NineChronicles.Mods.Athena.GUIs
{
    public class ArenaGUI : IGUI
    {
        private readonly Rect _arenaLayoutRect;

        public GUIContent SlotContent = new GUIContent();

        public List<AvatarInfo> avatarInfos = new List<AvatarInfo>();

        public ArenaGUI()
        {
            _arenaLayoutRect = new Rect(
                100,
                100,
                GUIToolbox.ScreenWidthReference - 200,
                GUIToolbox.ScreenHeightReference - 100);
            LoadRank();
        }

        private async Task LoadRank()
        {
            var apiClient = Game.instance.ApiClient;

            if (apiClient.IsInitialized)
            {
                var query =
                    $@"query {{
                            abilityRanking(limit: 20) {{
                                ranking
                                avatarAddress
                                name
                                avatarLevel
                                armorId
                                titleId
                                cp
                            }}
                        }}";

                var response = await apiClient.GetObjectAsync<AbilityRankingResponse>(query);
                if (response is null)
                {
                    AthenaPlugin.Log($"Failed getting response : {nameof(AbilityRankingResponse)}");
                    return;
                }

                foreach (var abilityRanking in response.AbilityRanking)
                {
                    var avatarInfo = new AvatarInfo
                    {
                        Name = abilityRanking.Name,
                        Cp = abilityRanking.Cp
                    };
                    avatarInfos.Add(avatarInfo);
                }
            }
        }

        public void OnGUI()
        {
            GUI.matrix = GUIToolbox.GetGUIMatrix();

            using (var areaScope = new GUILayout.AreaScope(_arenaLayoutRect))
            {
                ArenaSimulateBoard.DrawArenaBoard(avatarInfos);
            }
        }
    }
}
