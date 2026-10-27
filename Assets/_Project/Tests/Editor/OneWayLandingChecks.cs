using System;
using Herbalist.Interaction;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Run in Play mode: exercises real controller contacts, not just support raycasts.
    public static class OneWayLandingChecks
    {
        public static string Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Run this check in Play mode.");
            var platform = new GameObject("OneWayLandingCheck_Surface");
            var player = new GameObject("OneWayLandingCheck_Player");
            CharacterController character = null;
            int landings = 0, crossings = 0;
            try
            {
                platform.transform.position = new Vector3(10000, 1000, 10000);
                var box = platform.AddComponent<BoxCollider>();
                box.size = new Vector3(10.4f, .32f, 12f);
                platform.AddComponent<OneWayPlatform>();
                player.layer = 8;
                character = player.AddComponent<CharacterController>();
                character.height = 1.8f;
                character.center = Vector3.up * .9f;
                character.radius = .3f;
                character.skinWidth = .03f;
                character.stepOffset = .2f;
                foreach (float scale in new[] { 1f, 2.7f })
                foreach (var angles in new[] { Vector3.zero, new Vector3(-3.73f, 195.9f, 5.08f), new Vector3(8, 35, -7) })
                {
                    character.enabled = false;
                    player.transform.localScale = Vector3.one * scale;
                    character.enabled = true;
                    platform.transform.rotation = Quaternion.Euler(angles);
                    Physics.SyncTransforms();
                    foreach (var offset in new[] { Vector3.zero, Vector3.right * 2, Vector3.left * 2, Vector3.forward * 2, Vector3.back * 2 })
                    {
                        var probe = platform.transform.position + offset + Vector3.up * 5;
                        if (!box.Raycast(new Ray(probe, Vector3.down), out var top, 10))
                            throw new Exception("Missing test surface.");
                        Drop(character, box, top.point + Vector3.up * 2, 0, top.point.y);
                        landings++;
                    }
                    box.Raycast(new Ray(platform.transform.position + Vector3.up * 5, Vector3.down), out var center, 10);
                    Drop(character, box, center.point - Vector3.up * (character.height * scale + 1), 35, center.point.y);
                    crossings++;
                }
                return $"PASS: {landings} landings and {crossings} upward crossings/relandings on flat/tilted surfaces at scales 1 and 2.7.";
            }
            finally
            {
                if (character != null) OneWayPlatform.Release(character);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(platform);
            }
        }

        private static void Drop(CharacterController character, Collider surface, Vector3 start, float speed, float surfaceY)
        {
            character.enabled = false;
            character.transform.position = start;
            character.enabled = true;
            Physics.SyncTransforms();
            bool grounded = false;
            for (int i = 0; i < 180; i++)
            {
                speed -= 88f / 60f;
                OneWayPlatform.PrepareMove(character, Vector3.up * speed);
                var flags = character.Move(Vector3.up * (speed / 60f));
                grounded = (flags & CollisionFlags.Below) != 0;
                if (grounded && speed < 0) speed = -8;
            }
            if (!grounded || Physics.GetIgnoreCollision(character, surface) ||
                Mathf.Abs(character.transform.position.y - surfaceY) > .5f)
                throw new Exception("Controller fell through or failed to remain on the tilted surface.");
        }
    }
}
