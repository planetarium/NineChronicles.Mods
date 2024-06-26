﻿using System.Linq;
using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Libplanet.Types.Tx;
using Nekoyume.Multiplanetary;
using Nekoyume.Multiplanetary.Extensions;
using NineChronicles.Mods.Athena.GUIs;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdHoc.BurnAsset
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class BurnAssetPlugin : BaseUnityPlugin
    {
        private const string PluginGUID = "ad-hoc.burn-asset";
        private const string PluginName = "BurnAsset";
        private const string PluginVersion = "0.0.1";

        // NOTE: Fill the planet ID you want to burn assets.
        //       e.g. PlanetId.Odin, PlanetId.Heimdall
        private static readonly PlanetId _planetId = PlanetId.Heimdall;

        // NOTE: Fill the owner address you want to burn assets.
        private const string _ownerAddressString = "0x";
        private static readonly Address _ownerAddress = new(_ownerAddressString);

        // NOTE: Fill the amount you want to burn.
        private static readonly FungibleAssetValue _amount = 9_999L * Currencies.Crystal;

        // NOTE: Fill the memo you want to burn.
        private const string _memo = "";

        // NOTE: Fill the URL prefix of the explorer.
        //       e.g. "https://9cscan.com/tx/", "https://heimdall.9cscan.com/tx/"
        private const string _urlPrefix = "https://heimdall.9cscan.com/tx/";

        private Harmony _harmony;


        private bool _drawGUI = false;
        private bool _agentSubscribed = false;
        private bool _hasSubmitted = false;
        private TxId? _txId = null;

        private bool IsPlanetIdMatched => Nekoyume.Game.Game.instance?.CurrentPlanetId.Equals(_planetId) ?? false;
        private string TxHash => _txId?.ToHex() ?? string.Empty;

        private Camera _mainCamera;
        private Color _mainCameraBackgroundColor;
        private int _mainCameraCullingMask;
        private EventSystem _eventSystem;

        private void Awake()
        {
            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(typeof(BurnAssetPlugin));

            _eventSystem = FindObjectOfType<EventSystem>();
            LazySubscribe();
            Logger.LogInfo("Loaded");
        }

        private async void LazySubscribe()
        {
            await UniTask.WaitUntil(() => Nekoyume.Game.Game.instance?.Agent != null);
            Nekoyume.Game.Game.instance.Agent.OnMakeTransaction.Subscribe(tx =>
            {
                var burnAsset = tx.actions.OfType<Nekoyume.Action.BurnAsset>().FirstOrDefault();
                if (burnAsset is null)
                {
                    return;
                }

                _txId = tx.tx.Id;
            }).AddTo(this);
            _agentSubscribed = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                _drawGUI = true;
                DisableEventSystem();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _drawGUI = false;
                EnableEventSystem();
            }
        }

        private void OnGUI()
        {
            if (!_drawGUI)
            {
                return;
            }

            GUI.matrix = GUIToolbox.GetGUIMatrix();

            GUI.BeginGroup(new Rect(10f, 10f, 300f, 355f));
            GUI.Box(new Rect(0f, 0f, 300f, 355f), string.Empty);
            GUI.Label(new Rect(5f, 5f, 290f, 50f), _planetId.ToLocalizedPlanetName(containsPlanetId: false));
            GUI.Label(new Rect(5f, 60f, 290f, 50f), _ownerAddressString);
            GUI.Label(new Rect(5f, 115f, 290f, 50f), _amount.ToString());
            GUI.Label(new Rect(5f, 170f, 290f, 50f), TxHash);
            GUI.enabled = !string.IsNullOrEmpty(TxHash);
            if (GUI.Button(new Rect(5f, 225f, 290f, 50f), "Open Tx"))
            {
                var url = $"{_urlPrefix}{TxHash}";
                Application.OpenURL(url);
            }

            GUI.enabled = IsPlanetIdMatched && _agentSubscribed && !_hasSubmitted;
            if (GUI.Button(new Rect(5f, 280f, 290f, 70f), "Sign & Submit"))
            {
                SignAndSubmit();
            }

            GUI.enabled = true;
            GUI.EndGroup();
        }

        private void SignAndSubmit()
        {
            var burnAsset = new Nekoyume.Action.BurnAsset(_ownerAddress, _amount, _memo);
            var agent = Nekoyume.Game.Game.instance.Agent;
            agent.EnqueueAction(burnAsset);
            _hasSubmitted = true;
        }

        private void DisableEventSystem()
        {
            if (_mainCamera is null)
            {
                _mainCamera = Camera.main;
                _mainCameraBackgroundColor = _mainCamera.backgroundColor;
                _mainCameraCullingMask = _mainCamera.cullingMask;
                _mainCamera.backgroundColor = Color.gray;
                _mainCamera.cullingMask = 0;
            }

            if (_eventSystem != null)
            {
                _eventSystem.enabled = false;
            }
        }

        private void EnableEventSystem()
        {
            if (_mainCamera)
            {
                _mainCamera.backgroundColor = _mainCameraBackgroundColor;
                _mainCamera.cullingMask = _mainCameraCullingMask;
                _mainCamera = null;
            }

            if (_eventSystem == null)
            {
                _eventSystem = FindObjectOfType<EventSystem>();
            }

            if (_eventSystem != null)
            {
                _eventSystem.enabled = true;
            }
        }
    }
}
