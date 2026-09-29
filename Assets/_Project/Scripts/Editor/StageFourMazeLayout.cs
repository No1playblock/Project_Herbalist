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
    // Editor-only reference authoring. Runtime consumes the existing graph assets and scene objects.
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
        [MenuItem("Herbalist/Stages/Apply PDF Stage Four Maze Layouts")]
        public static void Apply()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)
                throw new Exception("Stop Play and save scene edits first.");
            var scene=EditorSceneManager.OpenScene(StageFourBuilder.Path);
            var flow=Object.FindFirstObjectByType<StageFourFlow>();
            for(int i=0;i<flow.Mazes.Length;i++)
            {
                ApplyDefinition(flow.Mazes[i].Definition,i);
                ApplyBoard(flow.Mazes[i],i);
            }
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        public static void ApplyBoard(SapMazeBoard board,int index)
        {
            var layout=Read(index);var root=board.transform;var d=board.Definition;
            foreach(Transform t in root.Cast<Transform>().ToArray())
                if(t.name.StartsWith("Channel_")||t.name=="Start"||t.name=="Goal"||t.name=="ReferenceFace"||t.name=="ReferenceWalls")
                    Object.DestroyImmediate(t.gameObject);
            var texturePath=ArtRoot+"Textures/TEX_SapMaze_"+(index+1)+".jpeg";
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(texture==null)
            {
                texturePath=ArtRoot+"Textures/TEX_SapMaze_"+(index+1)+".jpg";
                texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            }
            if(texture==null)throw new Exception("Missing source image "+index);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            var faceMat=Material("Reference_"+(index+1),Color.white);
            faceMat.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(faceMat);
            var face=GameObject.CreatePrimitive(PrimitiveType.Quad);face.name="ReferenceFace";
            face.transform.SetParent(root,false);face.transform.localPosition=new Vector3(0,layout.bottom+layout.boardHeight*.5f,-.315f);
            face.transform.localScale=new Vector3(layout.boardWidth,layout.boardHeight,1);
            face.GetComponent<Renderer>().sharedMaterial=faceMat;Object.DestroyImmediate(face.GetComponent<Collider>());
            var wallMat=Material("ReferenceWall",Color.black);
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cube=primitive.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(primitive);
            var combine=layout.wallRects.Select(r=>{
                Vector2 p=Position(layout,r.x+r.width*.5f,r.y+r.height*.5f);
                return new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(new Vector3(p.x,p.y,-.38f),Quaternion.identity,
                    new Vector3(r.width/layout.width*layout.boardWidth,r.height/layout.height*layout.boardHeight,.12f))};
            }).ToArray();
            var meshPath=ArtRoot+"Meshes/MESH_SapMazeWalls_"+(index+1)+".asset";
            EnsureFolder(ArtRoot+"Meshes");
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
            mesh.name="SapMazeWalls_"+(index+1);mesh.CombineMeshes(combine,true,true);EditorUtility.SetDirty(mesh);
            var walls=new GameObject("ReferenceWalls",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
            walls.transform.SetParent(root,false);walls.GetComponent<MeshFilter>().sharedMesh=mesh;
            walls.GetComponent<MeshRenderer>().sharedMaterial=wallMat;walls.GetComponent<MeshCollider>().sharedMesh=mesh;
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
        private static Material Material(string name,Color color)
        {
            var path=MaterialRoot+"MAT_Maze"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null)
            {
                material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material,path);
            }
            material.SetColor("_BaseColor",color);EditorUtility.SetDirty(material);return material;
        }
        private static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;
            var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');
            EnsureFolder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));
        }
    }
}
