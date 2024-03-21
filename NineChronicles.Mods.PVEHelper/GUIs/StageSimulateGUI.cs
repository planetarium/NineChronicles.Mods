using System;
using System.Linq;
using System.Threading.Tasks;
using Bencodex.Types;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Nekoyume;
using Nekoyume.Blockchain;
using Nekoyume.Game;
using Nekoyume.State;
using Nekoyume.TableData;
using UnityEngine;

namespace NineChronicles.Mods.PVEHelper.GUIs
{
    public class StageSimulateGUI : IGUI
    {
        private readonly Rect _rect;

        private bool _isCalculating;
        private string _simulationInformationText;

        private bool enabled;

        private int selectedStageId = 0;

        private (WorldSheet WorldSheet, StageSheet StageSheet, int clearedStageId)? StateData { get; set; } = null;
        private DateTimeOffset? LastSheetsUpdated { get; set; } = null;

        private readonly int _avatarIndex;

        public StageSimulateGUI(int avatarIndex)
        {
            var width = 1000;
            var height = 500;
            _rect = new Rect(
                (GUIToolbox.ScreenWidthReference - width) / 2,
                (GUIToolbox.ScreenHeightReference - height) / 2,
                width,
                height);

            _avatarIndex = avatarIndex;
        }

        public void Show()
        {
            this.enabled = true;
        }
        
        public void Close()
        {
            this.enabled = false;
        }

        public void OnGUI()
        {
            if (this.enabled)
            {
                UpdateStateData();

                if (!(StateData is { } stateData))
                {
                    return;
                }

                if (selectedStageId == 0)
                {
                    selectedStageId = stateData.clearedStageId;
                }

                var style = new GUIStyle
                {
                    normal =
                    {
                        background = CreateColorTexture(_rect, Color.black)
                    }
                };

                GUILayout.BeginArea(_rect, style);
                GUILayout.BeginVertical();
                {
                    CloseButton();

                    var marginStyle = new GUIStyle
                    {
                        margin =
                        {
                            top = 25,
                            bottom = 25,
                        },
                    };

                    GUILayout.BeginHorizontal(marginStyle);
                    GUILayout.Box("", GUILayout.Width(700));
                    DrawSimulationSection(stateData.StageSheet);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        private void CloseButton()
        {
            var style = new GUIStyle
            {
                margin =
                {
                    left = 960,
                    top = 30,
                    right = 20,
                },
                fixedWidth = 20,
                fixedHeight = 20,
                normal =
                {
                    textColor = Color.white,
                },
                fontSize = 20
            };

            if (GUILayout.Button("X", style))
            {
                PVEHelperPlugin.Instance.Log(
                    LogLevel.Info,
                    "Close simulation mode");
                Close();
            }
        }
        
        private void ControllablePicker(string[] list, Action<string[], int> onChanged, int index = 0)
        {
            var buttonStyle = new GUIStyle
            {
                margin =
                {
                    left = 10,
                    right = 10,
                },
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 20,
                fixedHeight = 50,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };

            void Btn(string text, int change)
            {
                if (GUILayout.Button(text, buttonStyle))
                {
                    if (index + change > 0 && index + change < list.Length)
                    {
                        onChanged(list, index + change);
                    }
                }
            }
            
            var labelStyle = new GUIStyle
            {
                margin =
                {
                    left = 15,
                    right = 15,
                },
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 50,
                fixedHeight = 50,
                fontSize = 30,
                fontStyle = FontStyle.Bold
            };

            var rectStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
            };

            GUILayout.BeginHorizontal(rectStyle);
            {
                Btn("<<", -5);
                Btn("<", -1);
                GUILayout.Label(list[index], labelStyle);
                Btn(">", 1);
                Btn(">>", 5);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawSimulationSection(StageSheet stageSheet)
        {
            var titleStyle = new GUIStyle
            {
                margin =
                {
                    left = 10,
                    right = 10,
                },
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleCenter,
                // fixedWidth = 100,
                // fixedHeight = 50,
                fontSize = 30,
                fontStyle = FontStyle.Bold
            };

            GUILayout.BeginVertical(GUILayout.Width(200));
            GUILayout.Label("Stage", titleStyle);
            ControllablePicker(
                stageSheet.Keys.Select(x => x.ToString()).ToArray(),
                (_, index) => selectedStageId = index + 1,
                selectedStageId - 1);
            SimulateButton();
            SimulationResultTextArea();
            GUILayout.EndVertical();
        }

        private void SimulateButton()
        {
            var style = new GUIStyle
            {
                margin =
                {
                    top = 30,
                    bottom = 30,
                },
                padding =
                {
                    top = 10,
                    bottom = 10,
                    left = 10,
                    right = 10,
                },
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white,
                },
                alignment = TextAnchor.MiddleCenter,
                border =
                {
                    top = 2,
                    bottom = 2,
                    left = 2,
                    right = 2,
                },
            };
            GUILayout.Button("Simulate", style);
        }

        private void SimulationResultTextArea()
        {
            var style = new GUIStyle
            {
                margin =
                {
                    top = 30,
                    bottom = 30,
                },
                padding =
                {
                    top = 10,
                    bottom = 10,
                    left = 10,
                    right = 10,
                },
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white,
                },
                alignment = TextAnchor.MiddleCenter,
                border =
                {
                    top = 2,
                    bottom = 2,
                    left = 2,
                    right = 2,
                },
            };
            const float winRate = 100.0f;
            const float missRate = 100.0f;
            GUILayout.TextArea($"Win Rate: {winRate}%\nMiss Rate: {missRate}%", style);
        }

        private Texture2D CreateColorTexture(Rect rect, Color color)
        {
            int width = (int)rect.width, height = (int)rect.height;
            Color[] buf = new Color[width * height];
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = color;
            }

            var texture = new Texture2D(width, height);
            texture.SetPixels(buf);
            texture.Apply();
            return texture;
        }

        private async Task UpdateStateData()
        {
            if (LastSheetsUpdated is { } lastSheetsUpdated &&
                DateTimeOffset.UtcNow.Subtract(lastSheetsUpdated).CompareTo(TimeSpan.FromSeconds(30)) <= 0)
            {
                return;
            }

            LastSheetsUpdated = DateTimeOffset.UtcNow;

            var sheets = await Game.instance.Agent.GetSheetsAsync(new[]
            {
                Addresses.GetSheetAddress<WorldSheet>(),
                Addresses.GetSheetAddress<StageSheet>(),
            });

            var worldSheet = new WorldSheet();
            worldSheet.Set((Text)sheets[Addresses.GetSheetAddress<WorldSheet>()]);
            
            var stageSheet = new StageSheet();
            stageSheet.Set((Text)sheets[Addresses.GetSheetAddress<StageSheet>()]);

            var stid = States.Instance.CurrentAvatarState.worldInformation.TryGetLastClearedStageId(out int stageId)
                ? stageId
                : 0;

            StateData = (worldSheet, stageSheet, stid);
        }
    }
}
