using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Herbalist.Levels;
using Object=UnityEngine.Object;

namespace Herbalist.Editor
{
    // Editor-only maze authoring. Runtime consumes the existing graph assets and scene objects.
    public static class StageFourMazeLayout
    {
        private const string DataRoot="Assets/_Project/Data/Stages/Stage_04/";
        private const string ArtRoot="Assets/_Project/Art/Environments/Stage_04/";
        private const string MaterialRoot="Assets/_Project/Art/Materials/Environments/Stage_04/";
        [Serializable] private sealed class Point { public float x,y; }
        [Serializable] private sealed class Edge { public int x,y; }
        [Serializable] private sealed class Rect { public float x,y,width,height; }
        [Serializable] private sealed class Layout
        {
            public int page,width,height,start,goal;
            public float boardWidth,boardHeight,bottom;
            public Point[] points;
            public Edge[] edges;
            public int[] leaks;
            public Rect[] wallRects;
        }
        private static Layout Read(int index)
        {
            var asset=AssetDatabase.LoadAssetAtPath<TextAsset>(DataRoot+"Layouts/MazeLayout_"+(index+1)+".json");
            if(asset==null)throw new Exception("Missing traced reference layout "+index);
            return JsonUtility.FromJson<Layout>(asset.text);
        }
        private static Vector2 Position(Layout layout,float x,float y) =>
            new Vector2((x/layout.width-.5f)*layout.boardWidth,layout.bottom+(1-y/layout.height)*layout.boardHeight);
        public static void ApplyDefinition(SapMazeDefinition definition,int index)
        {
            var layout=Read(index);
            definition.nodes=layout.points.Select(p=>Position(layout,p.x,p.y)).ToArray();
            definition.edges=layout.edges.Select(e=>new Vector2Int(e.x,e.y)).ToArray();
            definition.startNode=layout.start;definition.goalNode=layout.goal;definition.leakNodes=layout.leaks;
            // Radius follows the source marker's pixel radius, avoiding neighboring corridors across a wall.
            definition.leakRadius=14f*layout.boardWidth/layout.width;
            if(!definition.IsValid())throw new Exception("Invalid traced maze "+index);
            EditorUtility.SetDirty(definition);
        }
        [MenuItem("Herbalist/Stages/Rebuild Stage Two Block Mazes")]
        public static void Apply()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)
                throw new Exception("Stop Play and save scene edits first.");
            var scene=EditorSceneManager.OpenScene(CurrentStagePaths.Maze);
            var flow=Object.FindFirstObjectByType<DescendingMazeFlow>();
            if(flow==null)throw new Exception("Current descending maze flow not found.");
            var mazes=new SerializedObject(flow).FindProperty("_mazes");
            for(int i=0;i<mazes.arraySize;i++)
            {
                var board=(SapMazeBoard)mazes.GetArrayElementAtIndex(i).objectReferenceValue;
                ApplyDefinition(board.Definition,i);
                ApplyBoard(board,i);
            }
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        public static void ApplyBoard(SapMazeBoard board,int index)
        {
            var layout=Read(index);var root=board.transform;var d=board.Definition;
            foreach(Transform t in root.Cast<Transform>().ToArray())
                if(t.name.StartsWith("Channel_")||t.name=="Start"||t.name=="Goal")
                    Object.DestroyImmediate(t.gameObject);
            RebuildBoardVisuals(board,index);
            var sap=root.Find("MazeSap");
            var start=d.nodes[d.startNode];sap.localPosition=new Vector3(start.x,start.y,-.53f);
            sap.localScale=Vector3.one*(18f*layout.boardWidth/layout.width);
            for(int i=0;i<board.Leaks.Length;i++)
            {
                var p=d.nodes[d.leakNodes[i]];
                board.Leaks[i].transform.localPosition=new Vector3(p.x,p.y,.45f);
                root.Find("LeafSocket_"+(i+1)).localPosition=new Vector3(p.x,p.y,2.9f);
            }
            foreach(string name in new[]{"MazeStatus","LeafInstructions"})
            {
                var label=root.Find(name);var p=label.localPosition;p.y=11.15f;label.localPosition=p;
            }
            EditorUtility.SetDirty(board);
        }
        public static void RebuildBoardVisuals(SapMazeBoard board,int index)
        {
            var layout=Read(index);var root=board.transform;
            foreach(Transform t in root.Cast<Transform>().ToArray())
                if(t.name=="ReferenceFace"||t.name=="ReferenceWalls"||t.name=="MazeFloor"||t.name=="MazeWalls")
                    Object.DestroyImmediate(t.gameObject);
            var frontLayer=LayerMask.NameToLayer("MazeFront");
            if(frontLayer<0)throw new Exception("MazeFront layer is missing");
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="MazeFloor";floor.layer=frontLayer;
            floor.transform.SetParent(root,false);
            floor.transform.localPosition=new Vector3(0,layout.bottom+layout.boardHeight*.5f,-.315f);
            floor.transform.localScale=new Vector3(layout.boardWidth,layout.boardHeight,.06f);
            floor.GetComponent<Renderer>().sharedMaterial=Material("Floor",new Color(.35f,.27f,.19f));
            Object.DestroyImmediate(floor.GetComponent<Collider>());
            var wallMat=Material("BlockWall",new Color(.53f,.28f,.12f));
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cube=primitive.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(primitive);
            var combine=layout.wallRects.Select(r=>{
                Vector2 p=Position(layout,r.x+r.width*.5f,r.y+r.height*.5f);
                return new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(new Vector3(p.x,p.y,-.54f),Quaternion.identity,
                    new Vector3(r.width/layout.width*layout.boardWidth,r.height/layout.height*layout.boardHeight,.4f))};
            }).ToArray();
            var meshPath=ArtRoot+"Meshes/MESH_SapMazeWalls_"+(index+1)+".asset";
            EnsureFolder(ArtRoot+"Meshes");
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
            mesh.name="SapMazeWalls_"+(index+1);mesh.CombineMeshes(combine,true,true);EditorUtility.SetDirty(mesh);
            var walls=new GameObject("MazeWalls",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
            walls.layer=frontLayer;walls.transform.SetParent(root,false);walls.GetComponent<MeshFilter>().sharedMesh=mesh;
            walls.GetComponent<MeshRenderer>().sharedMaterial=wallMat;walls.GetComponent<MeshCollider>().sharedMesh=mesh;
        }
        private static Material Material(string name,Color color)
        {
            var path=MaterialRoot+"MAT_Maze"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null)
            {
                material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material,path);
            }
            material.SetColor("_BaseColor",color);
            material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.08f);
            EditorUtility.SetDirty(material);return material;
        }
        private static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;
            var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');
            EnsureFolder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));
        }
    }
}
