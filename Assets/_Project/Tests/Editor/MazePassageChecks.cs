using System;
using System.Collections.Generic;
using System.Reflection;
using Herbalist.Levels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class MazePassageChecks
    {
        private static void Check(bool value, string label)
        { if (!value) throw new Exception("Maze passages: " + label); }
        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        private static void Quad(List<Vector3> v, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            int n = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(d);
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) > 0)
                triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
            else triangles.AddRange(new[] { n, n + 2, n + 1, n, n + 3, n + 2 });
        }
        private static void Wall(List<Vector3> v, List<int> t, float x0, float y0, float x1, float y1)
        {
            Quad(v,t,new Vector3(x0,y0,0),new Vector3(x1,y0,0),new Vector3(x1,y1,0),new Vector3(x0,y1,0),Vector3.back);
            Quad(v,t,new Vector3(x0,y0,0),new Vector3(x0,y1,0),new Vector3(x0,y1,1),new Vector3(x0,y0,1),Vector3.left);
            Quad(v,t,new Vector3(x1,y0,0),new Vector3(x1,y1,0),new Vector3(x1,y1,1),new Vector3(x1,y0,1),Vector3.right);
            Quad(v,t,new Vector3(x0,y0,0),new Vector3(x1,y0,0),new Vector3(x1,y0,1),new Vector3(x0,y0,1),Vector3.down);
            Quad(v,t,new Vector3(x0,y1,0),new Vector3(x1,y1,0),new Vector3(x1,y1,1),new Vector3(x0,y1,1),Vector3.up);
        }
        private static Mesh Maze(float gap, bool thin)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            Quad(v,t,new Vector3(0,0,1),new Vector3(4,0,1),new Vector3(4,4,1),new Vector3(0,4,1),Vector3.back);
            Wall(v,t,0,0,.2f,4); Wall(v,t,3.8f,0,4,4); Wall(v,t,.2f,0,3.8f,.2f); Wall(v,t,.2f,3.8f,3.8f,4);
            float x0 = thin ? 2.002f : 1.95f, x1 = thin ? 2.005f : 2.05f;
            Wall(v,t,x0,.2f,x1,1.961f); Wall(v,t,x0,1.961f+gap,x1,3.8f);
            if (gap > 0)
                Quad(v,t,new Vector3(x0,1.961f,.35f),new Vector3(x1,1.961f,.35f),new Vector3(x1,1.961f+gap,.35f),new Vector3(x0,1.961f+gap,.35f),Vector3.back);
            var mesh = new Mesh { name = "MazePassageFixture" }; mesh.SetVertices(v); mesh.SetTriangles(t,0); mesh.RecalculateBounds(); return mesh;
        }
        public static string Run()
        {
            var root = new GameObject("MazePassageFixture") { hideFlags = HideFlags.HideAndDontSave };
            var meshes = new List<Mesh>();
            try
            {
                var collider = root.AddComponent<MeshCollider>();
                var surface = root.AddComponent<SapMazeSurface>();
                Field(surface,"_mazeCollider",collider); Field(surface,"_cellSize",.01f);
                Field(surface,"_channelDepthFraction",.2f); // This fixture has a raised, but open, floor at depth .35.
                var route = new List<Vector2>();
                foreach (var test in new[] { (gap:.03f,thin:false,open:true), (gap:0f,thin:false,open:false), (gap:0f,thin:true,open:false) })
                {
                    var mesh = Maze(test.gap,test.thin); meshes.Add(mesh); collider.sharedMesh = mesh; Physics.SyncTransforms();
                    Check(surface.Build(root.transform), "surface builds");
                    Check(surface.TryFindPath(new Vector2(1,1),new Vector2(3,1),route) == test.open,
                        test.open ? "narrow shallow opening connects both sides" : "solid wall remains closed, including sub-cell wall");
                    if (!test.open)
                    {
                        var p = surface.Move(new Vector2(1.8f,1),new Vector2(.5f,0));
                        Check(p.x < (test.thin ? 2.002f : 1.95f), "movement cannot tunnel through wall");
                        continue;
                    }
                    var current = new Vector2(1,1);
                    foreach (var waypoint in route)
                    {
                        for (int tick = 0; tick < 200 && Vector2.Distance(current,waypoint)>.002f; tick++)
                            current=surface.Move(current,Vector2.ClampMagnitude(waypoint-current,.015f));
                    }
                    Check(Vector2.Distance(current,new Vector2(3,1)) < .02f,"point-sized water passes .03-wide shallower opening");
                    var authored = new Vector2(1.013f,1.027f);
                    Check(surface.Snap(authored) == authored,"valid non-grid authored point is exact");
                    float width=surface.ChannelSection(new Vector2(2,1.976f),Vector2.right,1f,.2f,out _);
                    Check(width>0 && width<.1f,"water narrows instead of disappearing or requiring body clearance");
                }
                var tiny = Maze(.003f,false); meshes.Add(tiny); collider.sharedMesh=tiny; Physics.SyncTransforms();
                Check(surface.Build(root.transform),"sub-grid opening fixture builds");
                var from = new Vector2(1.8f,1.9625f); var to = new Vector2(2.2f,1.9625f);
                Check(surface.TryFindPath(from,to,route),"direct liquid drag fits an opening narrower than one sample cell");
                var through = surface.Move(from,to-from);
                Check(Vector2.Distance(through,to)<.002f,"liquid crosses .003-wide opening without radius or cell clearance");
                float tinyWidth=surface.ChannelSection(new Vector2(2,1.9625f),Vector2.right,1f,.2f,out _);
                Check(tinyWidth>0 && tinyWidth<=.0031f,"visual water also squeezes into the sub-grid gap");
                return "PASS: narrow shallow and sub-grid .003 openings, actual point movement, closed/sub-grid wall rejection, exact authored position, nonzero narrow water width.";
            }
            finally { Object.DestroyImmediate(root); foreach(var mesh in meshes) Object.DestroyImmediate(mesh); }
        }
    }
}
