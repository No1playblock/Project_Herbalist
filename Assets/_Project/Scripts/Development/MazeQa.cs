#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Herbalist.Abilities;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    public sealed partial class DescendingMazeFlow
    {
        internal void ApplyQaPoint(string point, int sapSlot)
        {
            if (!Authority) throw new InvalidOperationException("Host가 미로의 테스트 상태를 준비해야 합니다.");
            if (!Online)
                foreach (var actor in StageActor.All)
                    if (actor.Available) QaStagePlacement.AssignOfflineAbility(actor, sapSlot);
            if (point == "arrival") return;

            bool final = point == "final";
            int maze = final ? _mazes.Length - 1 : int.Parse(point.Split('-')[0]) - 1;
            string section = final ? "solved" : point.Split('-')[1];
            bool descending = section == "descent";
            _current = descending ? maze - 1 : maze;
            _phase = descending ? 2 : 3;
            _travel = 0;
            Array.Clear(_descentOffsets, 0, _descentOffsets.Length);
            Present(_current, _travel, descending);

            foreach (var actor in StageActor.All)
            {
                if (!actor.Available) continue;
                if (!actor.Abilities.Unlocked) continue;
                int side = actor.Abilities.Kind == PlayerAbilityKind.Sap ? 0 : 1;
                var position = _rafts[side].position + _riderOffset;
                if (descending) Carry(actor, position);
                else
                {
                    Release(actor);
                    Teleport(actor, position);
                }
            }
            for (int i = 0; i < _mazes.Length; i++)
                _mazes[i].ApplyQaState(i == maze && !descending, i < maze ? "solved" : i == maze && !descending ? section : "start");
            if (Online) { Current = _current; Phase = _phase; Travel = _travel; }
        }
    }

    public sealed partial class SapMazeBoard
    {
        internal void ApplyQaState(bool playable, string section)
        {
            if (!Authority) return;
            StopControl();
            _state = SapMazeSimulation.Initial(_definition, 0);
            _surfacePosition = _surfaceStart;
            _surfaceHeading = Vector2.zero;
            _leakMask = 0;
            _playable = playable;
            ResetTrail();

            switch (section)
            {
                case "solved":
                    _state.Phase = MazePhase.Solved;
                    break;
                case "retry":
                    _state.Volume = 0;
                    ResetTrail();
                    BeginRetry();
                    break;
                case "leak":
                case "refill":
                case "exit":
                    if (_surface == null || !_surface.Ready)
                        throw new InvalidOperationException("미로의 이동 영역을 찾을 수 없습니다.");
                    Vector2 target = section == "leak" ? _surfaceLeaks[0] : section == "refill" ? _surfaceRefills[0] : _surfaceGoal;
                    float threshold = section == "leak" ? _definition.leakRadius : section == "refill" ? _definition.refillRadius : .15f;
                    if (section == "exit") _state.Volume = Mathf.Max(_state.Volume, _definition.requiredVolume);
                    PlaceWaterBefore(target, threshold);
                    break;
            }
            Publish();
            RefreshAvailability();
        }

        private void PlaceWaterBefore(Vector2 target, float threshold)
        {
            var path = new List<Vector2>();
            Vector2 end = _surface.Snap(target);
            if (!_surface.TryFindPath(_surfaceStart, end, path))
                throw new InvalidOperationException("미로 시작점에서 이 목표까지 경로가 없습니다.");
            path.Insert(0, _surfaceStart);
            path.Add(end);
            float step = _surface.CellSize;
            for (int i = path.Count - 1; i > 0; i--)
            {
                float length = Vector2.Distance(path[i], path[i - 1]);
                for (float offset = 0; offset <= length; offset += step)
                {
                    Vector2 candidate = _surface.Snap(Vector2.MoveTowards(path[i], path[i - 1], offset));
                    if (Vector2.Distance(candidate, target) <= threshold) continue;
                    _surfacePosition = candidate;
                    if (_fluid != null) _fluid.Reset(_surface.GridCell(candidate), _state.Volume);
                    if (_tube != null) _tube.Reset(candidate, target, _state.Volume);
                    if (DistanceToWater(target) > threshold + step * .25f) return;
                }
            }
            throw new InvalidOperationException("목표 바로 전의 수액 위치를 배치할 수 없습니다.");
        }
    }
}
#endif
