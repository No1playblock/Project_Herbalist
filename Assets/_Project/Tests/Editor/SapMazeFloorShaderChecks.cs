namespace Herbalist.Editor
{
    public static class SapMazeFloorShaderChecks
    {
        public static string Run()
        {
var scene=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
var rt=new UnityEngine.RenderTexture(160,160,24);
var pixels=new UnityEngine.Texture2D(160,160,UnityEngine.TextureFormat.RGBA32,false);
var material=new UnityEngine.Material(UnityEngine.Shader.Find("Herbalist/Maze Density Water"));
var mesh=new UnityEngine.Mesh();
var previous=UnityEngine.RenderTexture.active;
try {
var go=new UnityEngine.GameObject("Floor slope fixture",typeof(UnityEngine.MeshFilter),typeof(UnityEngine.MeshRenderer));
UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,scene);
go.GetComponent<UnityEngine.MeshFilter>().sharedMesh=mesh;
var renderer=go.GetComponent<UnityEngine.MeshRenderer>(); renderer.sharedMaterial=material;
material.SetColor("_BaseColor",UnityEngine.Color.magenta);material.SetColor("_EdgeColor",UnityEngine.Color.magenta);
material.SetFloat("_FlowStrength",0);
var properties=new UnityEngine.MaterialPropertyBlock();
properties.SetTexture("_Density",UnityEngine.Texture2D.whiteTexture);
properties.SetTexture("_VisibleCorners",UnityEngine.Texture2D.whiteTexture);
properties.SetMatrix("_MazeFromObject",UnityEngine.Matrix4x4.identity);
properties.SetVector("_Grid",new UnityEngine.Vector4(0,0,1,1));
properties.SetVector("_GridExtent",new UnityEngine.Vector4(1,1,0,0));
properties.SetVector("_WetBounds",new UnityEngine.Vector4(0,0,1,1));
properties.SetFloat("_Threshold",.35f); properties.SetFloat("_WaterUvScale",.25f);
properties.SetVector("_LiquidFloorPlane",new UnityEngine.Vector4(0,0,1,0));
properties.SetFloat("_ReliefLimit",1.4f);
properties.SetVector("_FloorFacing",new UnityEngine.Vector4(0,0,-1,UnityEngine.Mathf.Cos(45*UnityEngine.Mathf.Deg2Rad)));
renderer.SetPropertyBlock(properties);
var cg=new UnityEngine.GameObject("Floor fixture camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cg,scene);
var camera=cg.AddComponent<UnityEngine.Camera>();camera.enabled=false;camera.scene=scene;camera.orthographic=true;camera.orthographicSize=.6f;camera.farClipPlane=20;
camera.transform.position=new UnityEngine.Vector3(.5f,.5f,-5);camera.targetTexture=rt;camera.clearFlags=UnityEngine.CameraClearFlags.SolidColor;camera.backgroundColor=UnityEngine.Color.black;
var output=new System.Collections.Generic.List<string>();
for(int fixture=0;fixture<4;fixture++){
float slope=fixture==0?0:fixture==1?30:fixture==2?75:0;
float offset=fixture==3?3:0;
float dz=UnityEngine.Mathf.Tan(slope*UnityEngine.Mathf.Deg2Rad);
mesh.Clear();mesh.vertices=new[]{new UnityEngine.Vector3(0,0,offset),new UnityEngine.Vector3(1,0,offset+dz),new UnityEngine.Vector3(1,1,offset+dz),new UnityEngine.Vector3(0,1,offset)};
mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateNormals();mesh.RecalculateBounds();
camera.Render();UnityEngine.RenderTexture.active=rt;pixels.ReadPixels(new UnityEngine.Rect(0,0,160,160),0,0);pixels.Apply();
int visible=0;foreach(var p in pixels.GetPixels32())if(p.r>170 && p.b>170 && p.g<65)visible++;
if((fixture<2 && visible<100)||(fixture>=2 && visible!=0))throw new System.Exception("Floor rejection failed fixture="+fixture+" visible="+visible);
output.Add("slope="+slope+" offset="+offset+" water pixels="+visible+" PASS");
}
return string.Join("\n",output);
} finally {
UnityEngine.RenderTexture.active=previous;
UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(rt);
}

        }
    }
}
