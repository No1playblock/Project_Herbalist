using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class TreeSapInletRollout
    {
        public const string MushroomPath = "Assets/_Project/Art/Environments/Stage_01/Stage1_Assets/Models/ImportedFromLatest/Mushroom/Mushroom.fbx";
        public static void Set(Object obj, string name, Object value)
        {
            var so = new SerializedObject(obj); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Transform[] All() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
        private static void Guard()
        {
            if (Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != StageOneDocumentAuthoring.ScenePath)
                throw new InvalidOperationException("Imported Stage One Edit mode required.");
        }
        public static string ReplaceWallInlets()
        {
            Guard();
            var sample = GameObject.Find("TreeHoleVisual").transform;
            var scale = sample.lossyScale;
            var asset = PrefabUtility.LoadPrefabContents(TreeSapInletAuthoring.PrefabPath);
            try { asset.transform.localScale = scale; PrefabUtility.SaveAsPrefabAsset(asset, TreeSapInletAuthoring.PrefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(asset); }
            var ports = All().Where(t=>t.name.StartsWith("WallWaterInlet")).ToArray();
            foreach (var port in ports) ReplaceVisual(port);
            // The user moved the sample visual as well as enlarging it: aim at its visible opening.
            Set(sample.parent.GetComponent<SapInjectionPort>(), "_aimPoint", sample);
            Save();
            return "Replaced " + ports.Length + " WallWaterInlet visuals; prefab world scale " + scale;
        }
        public static GameObject ReplaceVisual(Transform port)
        {
            var existing = port.Find("TreeInletVisual");
            if (existing != null) return existing.gameObject;
            foreach (var renderer in port.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            foreach (var collider in port.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            var receiver = port.GetComponent<SapInjectionPort>();
            if (receiver == null) receiver = port.gameObject.AddComponent<SapInjectionPort>();
            receiver.enabled = true;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreeSapInletAuthoring.PrefabPath);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, port);
            visual.name = "TreeInletVisual";
            Object.DestroyImmediate(visual.GetComponent<SapInjectionPort>());
            // Preserve a common world size beneath differently scaled imported parents.
            var size = prefab.transform.localScale;
            visual.transform.localScale = new Vector3(size.x / Mathf.Abs(port.lossyScale.x), size.y / Mathf.Abs(port.lossyScale.y), size.z / Mathf.Abs(port.lossyScale.z));
            visual.transform.position = port.position;
            visual.transform.rotation = Quaternion.LookRotation(port.forward, Vector3.up);
            // Match only the original tree surface; never snap onto neighbouring mushrooms or helpers.
            var trees = All().Select(t=>t.GetComponent<MeshCollider>()).Where(c=>c!=null && c.name=="seonag_tree").ToArray();
            RaycastHit best = default; bool found = false; float distance = 3f;
            var directions = new[] { port.forward, -port.forward, port.up, -port.up, Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            foreach (var tree in trees) foreach (var direction in directions)
            {
                if (!tree.Raycast(new Ray(port.position + direction * 3, -direction), out var hit, 6)) continue;
                float d = Vector3.Distance(hit.point, port.position);
                if (d >= distance) continue;
                distance = d; best = hit; found = true;
            }
            if (found)
            {
                visual.transform.SetPositionAndRotation(best.point + best.normal * .035f, Quaternion.LookRotation(best.normal, Vector3.up));
            }
            Set(receiver, "_aimPoint", visual.transform.Find("InjectionAimPoint"));
            return visual;
        }
        public static string MatchTreeTextures()
        {
            Guard();Physics.SyncTransforms();int count=0;
            var trees=All().Select(t=>t.GetComponent<MeshCollider>()).Where(c=>c!=null&&c.name=="seonag_tree").ToArray();
            var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Art/Environments/Stage_01/Models/TreeSapInlet/TreeSapInlet.asset");
            foreach(var visual in All().Where(t=>t.name=="TreeInletVisual"))
            {
                var tree=trees.FirstOrDefault(c=>c.Raycast(new Ray(visual.position+visual.forward*2,-visual.forward),out var h,4)&&Vector3.Distance(h.point,visual.position)<.5f);
                if(tree==null)continue;
                string path="Assets/_Project/Art/Environments/Stage_01/Models/TreeSapInlet/TreeInlet_"+GlobalObjectId.GetGlobalObjectIdSlow(visual).targetObjectId+".asset";
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(mesh==null){mesh=Object.Instantiate(source);AssetDatabase.CreateAsset(mesh,path);}
                var vertices=source.vertices;var uv=source.uv;
                for(int i=0;i<vertices.Length;i++)
                {
                    var world=visual.TransformPoint(new Vector3(vertices[i].x,vertices[i].y,0));
                    if(!tree.Raycast(new Ray(world+visual.forward*3,-visual.forward),out var surface,6))continue;
                    uv[i]=surface.textureCoord;
                    // Match the outer rim to bark; retain the authored cavity depth in front.
                    vertices[i]=visual.InverseTransformPoint(surface.point+visual.forward*(vertices[i].z*Mathf.Abs(visual.lossyScale.z)+.035f));
                }
                mesh.vertices=vertices;mesh.uv=uv;mesh.RecalculateBounds();mesh.RecalculateNormals();mesh.RecalculateTangents();EditorUtility.SetDirty(mesh);
                visual.GetComponentInChildren<MeshFilter>().sharedMesh=mesh;count++;
            }
            Save();return "Matched tree surface and bark UV for "+count+" inlets.";
        }
        private static void Save()
        {
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        public static string BindSix(bool addNearbyInlets)
        {
            Guard();
            var all = All();
            var mushrooms = all.Select(t=>t.GetComponent<MeshFilter>()).Where(m=>m!=null && AssetDatabase.GetAssetPath(m.sharedMesh)==MushroomPath).OrderBy(m=>m.GetComponent<Renderer>().bounds.center.y).ToArray();
            if (mushrooms.Length != 6) throw new InvalidOperationException("Expected exactly six selected mushrooms.");
            var originalPads = all.Select(t=>t.GetComponent<ExteriorJumpPad>()).Where(p=>p!=null && p.enabled && p.gameObject.activeInHierarchy).ToArray();
            var template = originalPads.First(p=>p.name.StartsWith("EntryPad_"));
            var root = GameObject.Find("Stage1_DocumentGimmicks").transform;
            if (root.Find("MushroomFbxJumpPads") != null) throw new InvalidOperationException("Six-pad binding already exists.");
            var group = new GameObject("MushroomFbxJumpPads").transform; group.SetParent(root,false);
            var ports = all.Where(t=>t.name.StartsWith("WallWaterInlet")).Select(t=>t.GetComponent<SapInjectionPort>()).Where(p=>p!=null).ToList();
            var report = new System.Text.StringBuilder();
            var surfaces = mushrooms.ToDictionary(m=>m, m=>Top(m));
            // Resolve nearby authored pairs first so distant mushrooms cannot steal their inlets.
            var assigned = new Dictionary<MeshFilter,SapInjectionPort>();
            foreach (var mushroom in mushrooms.OrderBy(m=>ports.Min(p=>Vector3.Distance(p.InteractionPoint,m.GetComponent<Renderer>().bounds.center))))
            {
                var point = surfaces[mushroom];
                var receiver = ports.OrderBy(p=>Vector3.Distance(p.InteractionPoint,mushroom.GetComponent<Renderer>().bounds.center)).First();
                float distance = Vector3.Distance(receiver.InteractionPoint,mushroom.GetComponent<Renderer>().bounds.center);
                if (addNearbyInlets && distance > 15)
                {
                    var inlet = new GameObject("WallWaterInlet_"+mushroom.name); inlet.transform.SetParent(group,false);
                    PlaceNearby(inlet.transform, mushroom, point);
                    receiver = inlet.AddComponent<SapInjectionPort>();
                    var visual = ReplaceVisual(inlet.transform);
                    report.AppendLine("Added nearby inlet: "+mushroom.name);
                }
                else ports.Remove(receiver);
                assigned.Add(mushroom,receiver);
            }
            int count=0;
            foreach (var mushroom in mushrooms)
            {
                var clone = Object.Instantiate(template.gameObject, group);
                clone.name = "JumpPad_"+(++count).ToString("00")+"_"+mushroom.name;
                clone.transform.position = surfaces[mushroom];
                var pad = clone.GetComponent<ExteriorJumpPad>();
                var so = new SerializedObject(pad);
                so.FindProperty("_ports").arraySize=1;
                so.FindProperty("_ports").GetArrayElementAtIndex(0).objectReferenceValue=assigned[mushroom];
                so.FindProperty("_recallLeafAfterLaunch").boolValue=true;
                so.ApplyModifiedPropertiesWithoutUndo();
                var target = clone.GetComponentInChildren<LeafInstallTarget>(true);
                var targetSo=new SerializedObject(target);
                targetSo.FindProperty("targetId").intValue=620000+count;
                targetSo.FindProperty("_keepLeafUntilUsed").boolValue=true;
                targetSo.ApplyModifiedPropertiesWithoutUndo();
                // Remove the cloned entry inlet: every pad now receives exactly its linked wall inlet.
                foreach(var inlet in clone.GetComponentsInChildren<SapInjectionPort>(true)) Object.DestroyImmediate(inlet.gameObject);
                clone.SetActive(true);
                report.AppendLine(mushroom.name+" <-> "+GlobalObjectId.GetGlobalObjectIdSlow(assigned[mushroom].transform).targetObjectId+" distance="+Vector3.Distance(surfaces[mushroom],assigned[mushroom].InteractionPoint).ToString("F2"));
            }
            foreach(var pad in originalPads) pad.gameObject.SetActive(false);
            Save();
            System.IO.File.WriteAllText("Temp/TreeSapInletRollout/Pairing.txt", report.ToString());
            return report.ToString();
        }

        private static Vector3 Top(MeshFilter model)
        {
            var collider=model.GetComponent<MeshCollider>();
            if(collider==null)collider=model.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh=model.sharedMesh;collider.enabled=true;
            Physics.SyncTransforms();
            var bounds=model.GetComponent<Renderer>().bounds;
            var point=new Vector3(bounds.center.x,bounds.max.y,bounds.center.z);
            float highest=float.NegativeInfinity;
            for(int x=-2;x<=2;x++) for(int z=-2;z<=2;z++)
            {
                var start=new Vector3(bounds.center.x+bounds.size.x*x*.08f,bounds.max.y+1,bounds.center.z+bounds.size.z*z*.08f);
                if(collider.Raycast(new Ray(start,Vector3.down),out var hit,bounds.size.y+2)&&hit.point.y>highest){highest=hit.point.y;point=hit.point;}
            }
            return point+Vector3.up*.15f;
        }

        private static void PlaceNearby(Transform inlet, MeshFilter mushroom, Vector3 top)
        {
            var tree=All().Select(t=>t.GetComponent<MeshCollider>()).First(c=>c!=null&&c.name=="seonag_tree");
            var origin=top+Vector3.up*1.3f;
            RaycastHit best=default;float distance=10;bool found=false;
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI/12;
                if(tree.Raycast(new Ray(origin,new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))),out var hit,10)&&hit.distance<distance)
                {best=hit;distance=hit.distance;found=true;}
            }
            if(!found)
            {
                // No nearby tree at these two summit mushrooms: inset the wooden socket into
                // the existing mushroom's side, rather than leaving a floating hole in the air.
                var bounds=mushroom.GetComponent<Renderer>().bounds;
                origin=new Vector3(bounds.max.x+2,bounds.max.y-Mathf.Min(1.2f,bounds.size.y*.25f),bounds.center.z);
                found=mushroom.GetComponent<MeshCollider>().Raycast(new Ray(origin,Vector3.left),out best,bounds.size.x+4);
            }
            if(!found)
            {
                var bounds=mushroom.GetComponent<Renderer>().bounds;
                origin=new Vector3(bounds.center.x+bounds.extents.x*.65f,bounds.max.y+2,bounds.center.z);
                found=mushroom.GetComponent<MeshCollider>().Raycast(new Ray(origin,Vector3.down),out best,bounds.size.y+4);
            }
            if(!found)throw new InvalidOperationException("No visible nearby surface for "+mushroom.name);
            var up=Mathf.Abs(Vector3.Dot(best.normal,Vector3.up))>.95f?Vector3.forward:Vector3.up;
            inlet.SetPositionAndRotation(best.point+best.normal*.04f,Quaternion.LookRotation(best.normal,up));
        }
    }
}
