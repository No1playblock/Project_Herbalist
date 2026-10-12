using Herbalist.Player;
using UnityEngine;
namespace Herbalist.StageOne
{
    [DefaultExecutionOrder(200)]
    public sealed class StageCarryPresentation : MonoBehaviour
    {
        public StageActor actor;
        public GameObject herb, potion;
        public Renderer herbRenderer, potionRenderer;
        [SerializeField] private Vector3 _handOffset;
        [SerializeField] private Vector3 _handEuler;
        [SerializeField] private Vector3 _herbHandOffset;
        [SerializeField] private Vector3 _herbHandEuler;
        [SerializeField] private Vector3 _herbHandScale = Vector3.one;
        private MaterialPropertyBlock block;
        private Renderer[] _herbRenderers, _potionRenderers;
        private PlayerCharacterPresentation _character;
        private Transform _attachedHand;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private void Awake()
        {
            block = new MaterialPropertyBlock();
            _character = actor != null ? actor.GetComponent<PlayerCharacterPresentation>() : GetComponentInParent<PlayerCharacterPresentation>();
            _herbRenderers = herb.GetComponentsInChildren<Renderer>(true);
            _potionRenderers = potion.GetComponentsInChildren<Renderer>(true);
        }
        private void LateUpdate()
        {
            AttachToHand();
            var flow = StageOneFlow.Instance;
            var item = flow != null && actor != null && actor.Available && actor.Slot >= 0 && actor.Slot < 2 ? flow.settings.Item(flow.Progress.Held[actor.Slot]) : null;
            herb.SetActive(item != null && item.type == StageItemType.Herb);
            potion.SetActive(item != null && item.type == StageItemType.Potion);
            if (item == null) return;
            block.SetColor(BaseColor, item.color);
            foreach (var renderer in _herbRenderers) renderer.SetPropertyBlock(block);
            foreach (var renderer in _potionRenderers) renderer.SetPropertyBlock(block);
        }
        private void AttachToHand()
        {
            var animator = _character != null ? _character.ActiveAnimator : null;
            if (animator == null || !animator.isHuman || !animator.isActiveAndEnabled) return;
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand == null || hand == _attachedHand) return;
            _attachedHand = hand;
            herb.transform.SetParent(hand, false);
            herb.transform.localPosition = _herbHandOffset;
            herb.transform.localRotation = Quaternion.Euler(_herbHandEuler);
            herb.transform.localScale = _herbHandScale;
            potion.transform.SetParent(hand, true);
            potion.transform.localPosition = _handOffset;
            potion.transform.localRotation = Quaternion.Euler(_handEuler);
        }
    }
}
