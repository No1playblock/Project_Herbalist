using System;
using System.Collections.Generic;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class MazePointerViewChecks
    {
        private static void Check(bool value, string message)
        { if (!value) throw new Exception("Maze pointer: " + message); }
        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        private static T Read<T>(object target, string name) =>
            (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
        private static void UpdateHud(Component target) => target.GetType().GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);

        // Run in a fresh offline Stage_02_Mazes Play session; do not save Play state.
        public static string Run()
        {
            Check(Application.isPlaying && !GameplayPause.IsPaused, "unpaused Play mode required");
            Check(Herbalist.Networking.FusionLobbySession.Instance == null || !Herbalist.Networking.FusionLobbySession.Instance.HasNetworkSession, "offline test only");
            var flow = Object.FindFirstObjectByType<DescendingMazeFlow>();
            Check(flow != null, "descending flow exists");
            StageActor actor = null;
            foreach (var candidate in StageActor.All) if (candidate.Local) { actor = candidate; break; }
            Check(actor != null, "local actor exists");
            var player = actor.GetComponent<PlayerController>();
            if (actor.Abilities.Kind != PlayerAbilityKind.Sap) Check(actor.Abilities.TrySwitchOfflineAbility(), "select water ability");
            // Complete the ordinary entry/carry sequence so its separate transport lock is released.
            for (int tick = 0; tick < 200 && flow.CurrentBoard == null; tick++) flow.Tick(.1f);
            Check(flow.CurrentBoard != null && !player.Motor.MovementLocked, "entry transport completes normally");
            var boards = Read<SapMazeBoard[]>(flow, "_mazes");
            var overlay = Object.FindFirstObjectByType<GameOverlayHud>();
            PlayerHud hud = null;
            foreach (var h in Object.FindObjectsByType<PlayerHud>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (h.slot == actor.Slot) hud = h;
            Check(hud != null && overlay != null && overlay.stageHeader != null, "authored HUD references");
            int moved = 0;
            for (int index = 0; index < boards.Length; index++)
            {
                var board = boards[index];
                var original = board.Definition;
                Check(board.StartMarker != null && board.GoalMarker != null, "editable endpoints are assigned " + index);
                var authoredStart = board.StartMarker.position;
                var authoredGoal = board.GoalMarker.position;
                var definition = Object.Instantiate(original);
                definition.initialVolume = 1000; definition.lossPerSecond = 0;
                Check(board.ConfigureOfflineTestDefinition(definition), "configure temporary test volume");
                Check(board.StartMarker.position == authoredStart && board.GoalMarker.position == authoredGoal,
                    "initialization preserves authored endpoint transforms " + index);
                // Exercise the actual marker-to-simulation connection, independent of legacy graph nodes.
                try
                {
                    board.StartMarker.position = authoredGoal;
                    board.GoalMarker.position = authoredStart;
                    var assignedStart = board.StartMarker.position;
                    var assignedGoal = board.GoalMarker.position;
                    Check(board.ConfigureOfflineTestDefinition(definition), "apply reversed authored endpoints");
                    var startLocal = board.transform.InverseTransformPoint(authoredGoal);
                    var goalLocal = board.transform.InverseTransformPoint(authoredStart);
                    Check(Vector2.Distance(board.SurfaceStart,board.Surface.Snap(new Vector2(startLocal.x,startLocal.y))) < .001f &&
                        Vector2.Distance(board.SurfaceGoal,board.Surface.Snap(new Vector2(goalLocal.x,goalLocal.y))) < .001f,
                        "moving start/goal markers changes actual simulation endpoints " + index);
                    Check(board.StartMarker.position == assignedStart && board.GoalMarker.position == assignedGoal,
                        "runtime never writes marker transforms " + index);
                }
                finally
                {
                    board.StartMarker.position = authoredStart; board.GoalMarker.position = authoredGoal;
                    board.ConfigureOfflineTestDefinition(definition);
                }
                try
                {
                    Field(flow, "_current", index); Field(flow, "_phase", 3);
                    foreach (var other in boards) other.SetPlayable(other == board);
                    var area = board.ControlArea.GetComponent<Collider>();
                    player.Motor.Teleport(area.bounds.center);
                    board.SetPlayable(true);
                    uint cycle = Read<uint>(actor.Abilities, "lastCycle") + 1;
                    actor.Abilities.Tick(cycle, 0, default, .02f);
                    Check(board.Controlling && SapMazeBoard.LocalMazeView == board && player.Motor.MovementLocked, "R enters owned maze view " + index);
                    player.View.Present(.02f, true);
                    var so = new SerializedObject(board);
                    var anchor = so.FindProperty("_frontCamera").objectReferenceValue as Transform;
                    var frame = anchor.GetComponent<MazeCameraFrame>();
                    Check(new SerializedObject(frame).FindProperty("_mazeBounds").objectReferenceValue == board.Surface.MazeCollider, "camera frames the actual interactive mesh " + index);
                    var camera = player.View.Camera;
                    foreach (var point in new[] { board.SurfaceStart, board.SurfaceGoal })
                    {
                        var world = board.transform.TransformPoint(new Vector3(point.x, point.y, board.Surface.SurfaceZ(point, 0)));
                        var screen = camera.WorldToScreenPoint(world);
                        Check(screen.z > 0 && camera.pixelRect.Contains(screen), "start and goal fit visible camera " + index);
                    }
                    UpdateHud(hud); UpdateHud(overlay);
                    Check(!hud.reticle.activeSelf && !hud.helpHint.gameObject.activeSelf && !hud.abilityText.gameObject.activeSelf && !hud.pocket.gameObject.activeSelf && !overlay.stageHeader.activeSelf, "maze view hides reticle and top HUD " + index);
                    Check(!board.TryProjectPointer(camera, new Vector2(-1, -1), out _), "reject pointer outside viewport");
                    Check(!board.TryProjectPointer(camera, new Vector2(float.NaN, 0), out _), "reject invalid pointer");
                    var path = new List<Vector2>();
                    Check(board.Surface.TryFindPath(board.SapPosition, board.SurfaceGoal, path) && path.Count >= 2, "route exists " + index);
                    var before = board.SapPosition;
                    var target = path.Count > 8 ? path[8] : Vector2.MoveTowards(before,path[path.Count-1],.35f);
                    var targetWorld = board.transform.TransformPoint(new Vector3(target.x, target.y, board.Surface.SurfaceZ(target, 0)));
                    var targetScreen = camera.WorldToScreenPoint(targetWorld);
                    Check(board.TryProjectPointer(camera, targetScreen, out var projected) && Vector2.Distance(projected, target) < .05f, "screen click hits current channel rather than stale plane " + index);
                    for (int tick = 0; tick < 12; tick++)
                    {
                        actor.Abilities.Tick(cycle, 0, default, .02f, true, false, true, projected);
                        board.Tick(.02f);
                    }
                    Check(Vector2.Distance(before, board.SapPosition) > .08f, "held left click moves water through ability router " + index);
                    if (!definition.limitPointerPathDistance)
                    {
                        var far = path[path.Count - 1];
                        var farBefore = board.SapPosition;
                        actor.Abilities.Tick(cycle,0,default,.02f,true,false,true,far); board.Tick(.1f);
                        Check(Vector2.Distance(farBefore,board.SapPosition)>.01f,"distant pointer does not freeze a connected route " + index);
                    }
                    moved++;
                    var stopped = board.SapPosition;
                    actor.Abilities.Tick(cycle, 0, default, .02f, false, false, true, projected);
                    board.Tick(.1f);
                    Check(Vector2.Distance(stopped, board.SapPosition) < .001f, "release left click parks water " + index);
                    actor.Abilities.Tick(cycle + 1, 0, default, .02f);
                    Check(!board.Controlling && SapMazeBoard.LocalMazeView == null && !player.Motor.MovementLocked, "R exits and releases owner locks " + index);
                    UpdateHud(hud); UpdateHud(overlay);
                    Check(hud.helpHint.gameObject.activeSelf && hud.abilityText.gameObject.activeSelf && hud.pocket.gameObject.activeSelf && overlay.stageHeader.activeSelf, "HUD restores on exit " + index);
                    Check(Read<Transform>(player.View, "_cameraOverride") == null, "camera override clears " + index);
                }
                finally
                {
                    board.SetPlayable(false);
                    board.ConfigureOfflineTestDefinition(original);
                    Object.DestroyImmediate(definition);
                }
            }
            return $"PASS: {moved} authored mazes; correct camera bounds, start/goal framing, screen-to-channel clicks, left-drag routing, release-to-stop, R enter/exit, movement locks and HUD hide/restore.";
        }
    }
}
