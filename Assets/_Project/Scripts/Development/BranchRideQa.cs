#if UNITY_EDITOR
using System;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    public sealed partial class BranchRideFlow
    {
        internal void ApplyQaPoint(string point, int sapSlot)
        {
            if (!Authority || !Configured) throw new InvalidOperationException("Host의 부적 경로가 준비되지 않았습니다.");
            var actors = StageActor.All.Where(actor => actor.Available).OrderBy(actor => actor.Slot).ToArray();
            if (!Online)
                foreach (var actor in actors) QaStagePlacement.AssignOfflineAbility(actor, sapSlot);
            if (point == "start") return;
            if (actors.Length != 2) throw new InvalidOperationException("부적 QA 지점에는 두 캐릭터가 필요합니다.");

            if (point == "one")
            {
                if (!TryBoard(actors[0], out _)) throw new InvalidOperationException("첫 번째 캐릭터가 부적에 탑승하지 못했습니다.");
                return;
            }

            foreach (var actor in actors)
            {
                if (!TryBoard(actor, out _)) throw new InvalidOperationException("캐릭터가 부적에 탑승하지 못했습니다.");
                actor.Abilities.SetInputLock(this, false);
            }
            _phase = BranchRidePhase.Riding;
            _contactRemaining = Vector2.zero;
            Array.Clear(_boardingRemaining, 0, _boardingRemaining.Length);
            if (point == "retry")
            {
                ResetRide(true);
                Publish();
                return;
            }

            switch (point)
            {
                case "both":
                    break;
                case "leaf":
                    QaPosition(_track[0], Mathf.Min(_track[0].Length * .3f, _speed * 3f));
                    break;
                case "t1":
                case "t2":
                case "t3":
                case "t4":
                    int branch = point[1] - '1';
                    QaSetBranches(branch);
                    var entry = _track.FirstOrDefault(t => t.SwitchIndex != branch && t.Exits.Any(e => e != null && e.SwitchIndex == branch)) ??
                        _track.First(t => t.SwitchIndex == branch);
                    QaPosition(entry, Mathf.Max(0, entry.Length - _speed * 4f));
                    break;
                case "drop1":
                case "drop2":
                    int drop = point == "drop1" ? 0 : 1;
                    QaSetBranches(drop == 0 ? 1 : 4);
                    var dropTrack = _track.First(t => t.IsDrop && t.name == (drop == 0 ? "Drop1" : "Drop2"));
                    var approach = _track.First(t => !t.IsDrop && t.Exits.Contains(dropTrack) && FindConnectedExit(t) == Array.IndexOf(_track, dropTrack));
                    QaPosition(approach, Mathf.Max(0, approach.Length - _speed * 2f));
                    break;
                case "stones":
                    QaSetBranches(3);
                    var stones = _track.First(t => t.name == "StoneRun");
                    QaPosition(stones, 0);
                    break;
                case "landing":
                case "ending":
                case "credits":
                    QaSetBranches(4);
                    var finish = _track.First(t => t.IsFinish);
                    if (point == "landing")
                    {
                        QaPosition(finish, Mathf.Max(0, finish.Length - _speed * .5f));
                        break;
                    }
                    QaPosition(finish, finish.Length);
                    BeginLanding();
                    if (_phase != BranchRidePhase.Landing) throw new InvalidOperationException("최종 착지 지형을 찾을 수 없습니다.");
                    if (point == "credits")
                    {
                        _carrierPosition.y = _landingY;
                        _carrierRotation = _landingRotation;
                        _endingElapsed = _endingSeconds;
                        _phase = BranchRidePhase.Complete;
                    }
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(point), point, "등록되지 않은 3스테이지 지점");
            }
            CarryRiders();
            Publish();
            Present();
        }

        private void QaSetBranches(int count)
        {
            _branchMask = (1 << count) - 1;
            for (int i = 0; i < _switches.Length; i++)
            {
                _turns[i] = i < count ? 1 : 0;
                _switches[i].Present(_turns[i]);
            }
        }

        private void QaPosition(BranchRideTrack track, float distance)
        {
            _segment = Array.IndexOf(_track, track);
            _distance = distance;
            _carrierPosition = track.Sample(distance, out var forward) + Vector3.up * _carrierHeight;
            var horizontal = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (horizontal.sqrMagnitude > .001f) _carrierRotation = Quaternion.LookRotation(horizontal);
            _incomingDirection = horizontal;
        }
    }
}
#endif
