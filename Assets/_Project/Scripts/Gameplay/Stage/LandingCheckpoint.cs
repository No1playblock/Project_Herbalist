using UnityEngine;
using Herbalist.Player;
namespace Herbalist.Levels
{
    public sealed class LandingCheckpoint : MonoBehaviour
    {
        [SerializeField] private Collider _surface;
        [SerializeField] private Transform[] _respawns;
        [SerializeField, Min(.01f)] private float _standingTolerance = .35f;
        public Vector3 Point(int slot) => _respawns[Mathf.Clamp(slot, 0, _respawns.Length - 1)].position;
        public bool Supports(PlayerController player)
        {
            if (!isActiveAndEnabled || !player.Motor.IsGrounded || _surface == null) return false;
            var cc = player.GetComponent<CharacterController>();
            Vector3 feet = player.transform.TransformPoint(cc.center) - Vector3.up * (cc.height * Mathf.Abs(player.transform.lossyScale.y) * .5f);
            return Physics.Raycast(feet + Vector3.up * _standingTolerance, Vector3.down, out var hit, _standingTolerance * 2, ~0, QueryTriggerInteraction.Ignore) && hit.collider == _surface;
        }
    }
}
