using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Audio
{
    [DefaultExecutionOrder(230)]
    [RequireComponent(typeof(SfxEmitter))]
    public sealed class SceneSfxPresentation : MonoBehaviour
    {
        [SerializeField] private bool _forestAmbience;
        private SfxEmitter _emitter;
        private StageMinigameKind _minigame;
        private uint _result;
        private int _members, _choice = -1;
        private bool _ready, _initialized;
        private PlayerHud[] _huds;
        private bool[] _help;
        private void Awake() { _emitter = GetComponent<SfxEmitter>(); }
        private void Start()
        {
            _huds = FindObjectsByType<PlayerHud>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _help = new bool[_huds.Length];
            if (StageLevel.Instance != null) _emitter.Play(7);
        }
        private void LateUpdate()
        {
            _emitter.SetLoop(37, _forestAmbience);
            var session = FusionLobbySession.Instance;
            int members = session != null ? session.ConnectedCount : 0;
            var room = RoomControl.Instance;
            int choice = room != null ? room.LocalChoice : -1;
            bool ready = room != null && room.Ready;
            if (session != null && session.State == LobbyState.Waiting)
            {
                if (members > _members) _emitter.Play(4);
                if (_initialized && choice != _choice && choice >= 0) _emitter.Play(5);
                if (ready && !_ready) _emitter.Play(6);
            }
            _members = members; _choice = choice; _ready = ready;
            if (_huds != null) for (int i = 0; i < _huds.Length; i++)
            {
                bool visible = _huds[i] != null && _huds[i].helpPanel != null && _huds[i].helpPanel.activeInHierarchy;
                if (visible && !_help[i]) _emitter.Play(8);
                _help[i] = visible;
            }
            var flow = StageOneFlow.Instance;
            if (flow == null || flow.Progress == null) { _emitter.SetLoop(24, false); _emitter.SetLoop(25, false); _initialized = true; return; }
            if (_initialized && flow.MinigameResultSequence != _result)
            {
                if (_minigame == StageMinigameKind.Gathering)
                {
                    if (flow.MinigameSucceeded) _emitter.Play(21);
                    _emitter.Play(flow.MinigameSucceeded ? 22 : 23);
                }
                else if (_minigame == StageMinigameKind.Crafting) _emitter.Play(flow.MinigameSucceeded ? 28 : 27);
            }
            if (flow.Minigame == StageMinigameKind.Gathering)
            {
                if (_minigame != StageMinigameKind.Gathering)
                {
                    foreach (var actor in StageActor.All)
                        if (actor.Local) _emitter.Play(flow.RoleFor(actor) == Herbalist.Presentation.CharacterRole.Sodam ? 18 : 19);
                }
            }
            _emitter.SetLoop(24, flow.Minigame == StageMinigameKind.Crafting);
            _emitter.SetLoop(25, flow.Minigame == StageMinigameKind.Crafting && flow.Temperature >= _emitter.Catalog.boilingTemperature);
            _result = flow.MinigameResultSequence; _minigame = flow.Minigame;
            _initialized = true;
        }
        private void OnDisable() { _emitter?.StopLoops(); }
    }
}
