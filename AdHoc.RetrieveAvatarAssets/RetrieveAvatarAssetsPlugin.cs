using System.Linq;
using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Nekoyume;
using Nekoyume.Multiplanetary;
using NineChronicles.Mods.Athena.GUIs;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AdHoc.RetrieveAvatarAssets
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class RetrieveAvatarAssetsPlugin : BaseUnityPlugin
    {
        private const string PluginGUID = "ad-hoc.retrieve-avatar-assets";
        private const string PluginName = "RetrieveAvatarAssets";
        private const string PluginVersion = "0.0.1";

        private Harmony _harmony;

        private bool _initialized = false;
        private bool _drawGUI = false;
        private bool _hasSubmitted = false;
        private string _urlPrefix = string.Empty;
        private TxId? _txId = null;
        private string _txHash => _txId?.ToHex() ?? string.Empty;

        private Camera _mainCamera;
        private Color _mainCameraBackgroundColor;
        private int _mainCameraCullingMask;
        private EventSystem _eventSystem;

        private Address?[] _avatarAddresses;
        private string[] _avatarAddressStrings;
        private int _selectedAvatarIndex = 0;

        private void Awake()
        {
            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(typeof(RetrieveAvatarAssetsPlugin));

            _eventSystem = FindObjectOfType<EventSystem>();
            InitializeAsync();
            Logger.LogInfo("Loaded");
        }

        private async void InitializeAsync()
        {
            await UniTask.WaitUntil(() => Nekoyume.Game.Game.instance?.Agent != null);
            var game = Nekoyume.Game.Game.instance;
            game.Agent.OnMakeTransaction.Subscribe(tx =>
            {
                var action = tx.actions.OfType<Nekoyume.Action.RetrieveAvatarAssets>().FirstOrDefault();
                if (action is null)
                {
                    return;
                }

                _txId = tx.tx.Id;
            }).AddTo(this);

            await UniTask.WaitUntil(() => game.CurrentPlanetId.HasValue);
            var planetId = game.CurrentPlanetId.Value;
            if (planetId.Equals(PlanetId.Odin))
            {
                _urlPrefix = "https://9cscan.com/tx/";
            }
            else if (planetId.Equals(PlanetId.Heimdall))
            {
                _urlPrefix = "https://heimdall.9cscan.com/tx/";
            }

            await UniTask.WaitUntil(() => game.States?.CurrentAvatarState != null);
            var states = game.States;
            _avatarAddresses = Enumerable.Range(0, GameConfig.SlotCount)
                .Select(avatarKey => states.AvatarStates.ContainsKey(avatarKey)
                    ? states.AvatarStates[avatarKey].address
                    : (Address?)null)
                .ToArray();
            _avatarAddressStrings = _avatarAddresses
                .Select((address, index) => address?.ToHex() ?? $"Empty ({index})")
                .ToArray();

            _initialized = true;
            Logger.LogInfo("Initialized");
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            if (!_drawGUI &&
                Input.GetKeyDown(KeyCode.A))
            {
                _drawGUI = true;
                DisableEventSystem();
            }

            if (_drawGUI &&
                Input.GetKeyDown(KeyCode.Escape))
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

            GUI.BeginGroup(new Rect(10f, 10f, 300f, 300f));
            GUI.Box(new Rect(0f, 0f, 300f, 300f), string.Empty);
            GUI.Label(new Rect(5f, 5f, 290f, 30f), "Retrieve Selected Avatar's NCG");

            GUI.Label(new Rect(5f, 40f, 290f, 20f), $"- Select an avatar below(current: {_selectedAvatarIndex})");
            _selectedAvatarIndex = GUI.SelectionGrid(
                new Rect(5f, 65f, 290f, 80f),
                _selectedAvatarIndex,
                _avatarAddressStrings,
                1);
            GUI.enabled = !string.IsNullOrEmpty(_txHash);
            if (GUI.Button(new Rect(5f, 170f, 290f, 50f), "Open Tx"))
            {
                var url = $"{_urlPrefix}{_txHash}";
                Application.OpenURL(url);
            }

            GUI.enabled = _initialized &&
                _avatarAddresses[_selectedAvatarIndex].HasValue &&
                !_hasSubmitted;
            if (GUI.Button(new Rect(5f, 225f, 290f, 70f), "Sign & Submit"))
            {
                SignAndSubmit();
            }

            GUI.enabled = true;
            GUI.EndGroup();
        }

        private void SignAndSubmit()
        {
            var burnAsset = new Nekoyume.Action.RetrieveAvatarAssets(
                _avatarAddresses[_selectedAvatarIndex].Value);
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
