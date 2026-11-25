using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Herbalist.Audio;
using Herbalist.Levels;
using Herbalist.GameUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class SfxFeedback20261126
    {
        private static readonly List<string> Report = new();
        private static SfxCatalog Catalog;
        private static void Check(bool ok, string s) { if (!ok) throw new Exception(s); Report.Add("PASS " + s); }
        private static void Set(Object obj, string field, Object value) { var so = new SerializedObject(obj); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Number(Object obj, string field, int value) { var so = new SerializedObject(obj); so.FindProperty(field).intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static T Read<T>(Object obj, string field) where T:Object => (T)new SerializedObject(obj).FindProperty(field).objectReferenceValue;
        private static void Backup(string p)
        {
            string b = "CodexBackups/SfxFeedback20261126/" + p; Directory.CreateDirectory(Path.GetDirectoryName(b));
            if (!File.Exists(b)) File.Copy(p,b);
        }
        private static SfxEmitter Emitter(GameObject go)
        {
            var e = go.GetComponent<SfxEmitter>() ?? go.AddComponent<SfxEmitter>();Set(e,"_catalog",Catalog);
            var so = new SerializedObject(e);var channels = so.FindProperty("_channels");
            if (channels.arraySize < 3)
            {
                channels.arraySize = 3;
                for (int i=0;i<3;i++)
                {
                    var child = new GameObject("AudioChannel_"+i);child.transform.SetParent(go.transform,false);
                    var src = child.AddComponent<AudioSource>();src.playOnAwake=false;src.dopplerLevel=0;
                    channels.GetArrayElementAtIndex(i).objectReferenceValue=src;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return e;
        }
        private static void Ambient(Transform parent, int id, Vector3 point)
        {
            var t=parent.Find("SfxAmbient_"+id);
            var go=t!=null?t.gameObject:new GameObject("SfxAmbient_"+id);
            go.transform.SetParent(parent,false);go.transform.position=point;Emitter(go);
            var p=go.GetComponent<WorldSfxPresentation>()??go.AddComponent<WorldSfxPresentation>();
            Number(p,"_kind",(int)SfxWorldKind.Ambient);Number(p,"_ambientSoundId",id);Set(p,"_subject",parent);
            Report.Add("BOUND ambient "+id+" to "+parent.name);
        }
        public static void ApplyAndValidate()
        {
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Catalog=AssetDatabase.LoadAssetAtPath<SfxCatalog>(SfxAuthoring.CatalogPath);
                Check(Catalog!=null,"Catalog loaded");
                Check(Catalog.Find(39).clips[0]==Catalog.Find(32).clips[0],"Inlet uses water impact clip");
                Check(Catalog.Find(65).clips[0]!=Catalog.Find(39).clips[0] && Catalog.Find(65).spatialBlend==0,"Countdown has old inlet clip and 2D output");
                Check(Catalog.Find(45).clips[0].name.Contains("45_") && !Catalog.Find(45).clips[0].name.Contains("20261125"),"Original descent recording restored");
                Check(Catalog.Find(45).loop,"Descent duration still follows movement");
                Check(Catalog.quietStageExemptSoundId==49 && Mathf.Approximately(Catalog.quietStageGain,.15f),"Stage 3 attenuation excludes talisman slide");
                foreach(var s in Catalog.sounds) Check(s.clips.Length>0&&s.clips.All(c=>c!=null&&c.length>0),"Valid sound "+s.id);
                string path="Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
                Backup(path);var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var transforms=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
                var poses=transforms.ToDictionary(t=>t,t=>t.localToWorldMatrix.ToString("R"));
                var falls=transforms.Where(t=>t.name.StartsWith("TSI_Waterfall_02A")).ToArray();
                Check(falls.Length>0,"Waterfall visual objects found");
                foreach(var t in falls)
                {
                    var renderer=t.GetComponentInChildren<Renderer>(true);
                    Ambient(t,62,renderer!=null?renderer.bounds.center:t.position);
                }
                var streams=transforms.Select(t=>t.GetComponent<StageOneFixedWaterStream>()).Where(s=>s!=null &&
                    (s.GetComponentInParent<StageOneDocumentMechanism>()?.name.StartsWith("LeafBlock_04_Musroom")??false)).ToArray();
                Check(streams.Length==3,"Exactly three tree outlet streams found");
                foreach(var stream in streams) Ambient(stream.transform,63,stream.GetPathPoint(.2f));
                int cloths=0;
                foreach(var cloth in transforms.Select(t=>t.GetComponent<HangingCloth>()).Where(c=>c!=null))
                {
                    var knot=Read<Transform>(cloth,"_knot");if(knot==null)continue;
                    Emitter(cloth.gameObject);
                    var p=cloth.GetComponent<WorldSfxPresentation>()??cloth.gameObject.AddComponent<WorldSfxPresentation>();
                    Number(p,"_kind",(int)SfxWorldKind.Cloth);Set(p,"_subject",cloth);Set(p,"_motion",knot);cloths++;
                }
                Check(cloths>0,"Moving cloth audio bindings: "+cloths);
                Check(poses.All(p=>p.Key!=null&&p.Value==p.Key.localToWorldMatrix.ToString("R")),"Existing world transforms preserved");
                EditorSceneManager.MarkSceneDirty(scene);Check(EditorSceneManager.SaveScene(scene),"Stage 1 saved");
                path="Assets/_Project/Scenes/01_Title/MainMenu.unity";Backup(path);scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var menuScene=scene;
                path="Assets/ui_select_chracter.unity";Backup(path);scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
                int buttons=0;
                foreach(var room in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CharacterSelectRoom>(true)))
                foreach(var field in new[]{"_copy","_options","_back"})
                {
                    var b=Read<Button>(room,field);Check(b!=null,"Room button "+field);
                    var s=b.GetComponent<SfxUiFeedback>()??b.gameObject.AddComponent<SfxUiFeedback>();Number(s,"_clickId",1);buttons++;
                    Report.Add("BOUND click "+field+" to "+b.name);
                }
                Check(buttons>=3,"Copy, options and back use existing click sound");
                Check(menuScene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<SfxEmitter>(true)).Any(e=>new SerializedObject(e).FindProperty("_scenePool").boolValue),"Menu audio pool exists");
                EditorSceneManager.MarkSceneDirty(scene);Check(EditorSceneManager.SaveScene(scene),"Additive character room saved");
                AssetDatabase.SaveAssets();Directory.CreateDirectory("Logs");File.WriteAllLines("Logs/SfxFeedback20261126.txt",Report);
                Debug.Log("SFX_FEEDBACK_20261126_PASS");EditorApplication.Exit(0);
            }
            catch(Exception e) { Directory.CreateDirectory("Logs");File.WriteAllText("Logs/SfxFeedback20261126.txt",string.Join("\n",Report)+"\nFAIL "+e);Debug.LogException(e);EditorApplication.Exit(1); }
        }
    }
}
