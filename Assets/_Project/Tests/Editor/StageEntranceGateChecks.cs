using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Herbalist.Levels;
using Herbalist.Player;

namespace Herbalist.Editor
{
    public static class StageEntranceGateChecks
    {
        public static string Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Run in Play mode.");
            var scope = new GameObject("TemporaryEntranceGateCheck");
            scope.transform.position = new Vector3(1000, 0, 1000);
            var tuning = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<PlayerTuning>("Assets/_Project/Data/Characters/SO_PlayerTuning.asset"));
            tuning.airAcceleration = 1000;
            var results = new List<string>();
            try
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(scope.transform, false);
                floor.transform.localPosition = Vector3.down * .5f;
                floor.transform.localScale = new Vector3(20, 1, 20);
                var gateObject = new GameObject("Gate");
                gateObject.SetActive(false);
                gateObject.transform.SetParent(scope.transform, false);
                gateObject.transform.localRotation = Quaternion.Euler(0, 37, 0);
                var gate = gateObject.AddComponent<StageEntranceGate>();
                var entry = gateObject.AddComponent<BoxCollider>();
                entry.isTrigger = true; entry.center = Vector3.up * 2; entry.size = new Vector3(8, 4, 4);
                var barrierObject = new GameObject("Barrier");
                barrierObject.transform.SetParent(gateObject.transform, false);
                var barrier = barrierObject.AddComponent<BoxCollider>();
                barrier.center = entry.center; barrier.size = new Vector3(8, 4, .3f);
                var serialized = new SerializedObject(gate);
                serialized.FindProperty("_gateIndex").intValue = 31;
                serialized.FindProperty("_entryVolume").objectReferenceValue = entry;
                serialized.FindProperty("_returnBarrier").objectReferenceValue = barrier;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                gateObject.SetActive(true);
                var a = CreateMotor(scope.transform, tuning, "A");
                var b = CreateMotor(scope.transform, tuning, "B");
                Vector3 start = gateObject.transform.TransformPoint(new Vector3(-1, .03f, -2));
                Vector3 direction = gateObject.transform.forward;
                a.RestoreState(new MotorState { Position = start });
                b.RestoreState(new MotorState { Position = gateObject.transform.TransformPoint(new Vector3(1, .03f, -2)) });
                Physics.SyncTransforms();
                for (int i = 0; i < 70; i++) { a.RestoreState(a.CaptureState()); a.Simulate(direction * 3, .02f); }
                Require((a.CaptureState().EnteredGateMask & gate.Mask) != 0, "A passage registered");
                StageEntranceGate.PrepareMove(a.GetComponent<CharacterController>(), a.CaptureState().EnteredGateMask);
                StageEntranceGate.PrepareMove(b.GetComponent<CharacterController>(), b.CaptureState().EnteredGateMask);
                Require(!Physics.GetIgnoreCollision(a.GetComponent<CharacterController>(), barrier), "A blocked");
                Require(Physics.GetIgnoreCollision(b.GetComponent<CharacterController>(), barrier), "B still allowed");
                for (int i = 0; i < 100; i++) { a.RestoreState(a.CaptureState()); a.Simulate(-direction * 3, .02f); }
                Require(gateObject.transform.InverseTransformPoint(a.transform.position).z > .25f, "A cannot return");
                for (int i = 0; i < 70; i++) b.Simulate(direction * 3, .02f);
                Require((b.CaptureState().EnteredGateMask & gate.Mask) != 0, "B independently enters");
                results.Add("Rotated gate: independent entry, reverse collision, prediction restore");
                a.Teleport(a.transform.position);
                Require((a.CaptureState().EnteredGateMask & gate.Mask) != 0, "Teleport preserves passage");
                a.RestoreState(new MotorState { Position = start });
                StageEntranceGate.PrepareMove(a.GetComponent<CharacterController>(), a.CaptureState().EnteredGateMask);
                Require(Physics.GetIgnoreCollision(a.GetComponent<CharacterController>(), barrier), "Prediction rewind restores entry");
                Vector3 from = gateObject.transform.TransformPoint(new Vector3(0, 1, -5));
                Vector3 to = gateObject.transform.TransformPoint(new Vector3(0, 1, 5));
                Require((gate.EvaluatePassage(from, to, .3f, 0) & gate.Mask) != 0, "Fast swept entry");
                Require(gate.EvaluatePassage(to, from, .3f, 0) == 0, "Reverse is not entry");
                Require(gate.EvaluatePassage(from + gateObject.transform.right * 10, to + gateObject.transform.right * 10, .3f, 0) == 0, "Outside volume");
                results.Add("Fast crossing, outside rejection, reverse rejection, teleport and rollback state");
                return string.Join("\n", results);
            }
            finally { UnityEngine.Object.DestroyImmediate(scope); UnityEngine.Object.DestroyImmediate(tuning); }
        }

        private static CharacterControllerMotor CreateMotor(Transform parent, PlayerTuning tuning, string name)
        {
            var go = new GameObject(name); go.SetActive(false); go.transform.SetParent(parent, false);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.center = Vector3.up * .9f; cc.radius = .25f; cc.skinWidth = .03f;
            var motor = go.AddComponent<CharacterControllerMotor>();
            var so = new SerializedObject(motor);
            so.FindProperty("controller").objectReferenceValue = cc;
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.ApplyModifiedPropertiesWithoutUndo(); go.SetActive(true);
            return motor;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception("Entrance gate check failed: " + message);
        }
    }
}
