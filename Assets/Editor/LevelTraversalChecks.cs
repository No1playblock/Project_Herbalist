using System;
using System.Collections.Generic;
using UnityEngine;
using Herbalist.Levels;
using Herbalist.Player;
namespace Herbalist.Editor
{
    public static class LevelTraversalChecks
    {
        public static string Run()
        {
            if(!Application.isPlaying)throw new Exception("Play mode required");
            var p=UnityEngine.Object.FindAnyObjectByType<PlayerController>();var motor=p.Motor;var before=motor.CaptureState();
            var fixture=new GameObject("TransientTraversalCheck");var targets=new List<Vector3>();
            var stage=UnityEngine.Object.FindAnyObjectByType<Herbalist.StageOne.StageOneFlow>();GameObject blocker=stage!=null?stage.entranceBlocker:null;bool closed=blocker!=null&&blocker.activeSelf;
            try
            {
                if(blocker!=null)blocker.SetActive(false);
                var guide=UnityEngine.Object.FindAnyObjectByType<LevelRouteGuide>();
                foreach(var point in guide.expectedRoute)targets.Add(point.position);
                if(stage!=null)
                {
                    targets.RemoveAt(3);
                    for(int i=0;i<4;i++){var r=GameObject.Find("GatheringJumpRock_"+i).GetComponent<Renderer>();targets.Insert(3+i,new Vector3(r.bounds.center.x,r.bounds.max.y+.05f,r.bounds.center.z));}
                    targets.Insert(7,new Vector3(-41,3.65f,20));
                }
                else
                {
                    for(int i=1;i<targets.Count-1;i++)targets[i]+=Vector3.up*.1f;
                }
                Physics.SyncTransforms();motor.Teleport(targets[0]+Vector3.up*.1f);
                for(int settle=0;settle<30;settle++)motor.Simulate(Vector3.zero,.02f);
                for(int i=1;i<targets.Count;i++)
                {
                    if(stage==null&&i<targets.Count-1)
                    {
                        Vector3 outward=Vector3.ProjectOnPlane(targets[i]-targets[0],Vector3.up).normalized;
                        var go=new GameObject("TemporaryLeafSurface_"+i,typeof(BoxCollider));go.transform.SetParent(fixture.transform);
                        go.transform.SetPositionAndRotation(targets[i]-Vector3.up*.1f-outward*1.08f,Quaternion.LookRotation(outward));go.GetComponent<BoxCollider>().size=new Vector3(5.2f,.16f,6);
                        if(fixture.transform.childCount>3)UnityEngine.Object.DestroyImmediate(fixture.transform.GetChild(0).gameObject);
                        Physics.SyncTransforms();
                    }
                    bool arrived=false;Vector3 lastProgress=p.transform.position;
                    for(int n=0;n<1500;n++)
                    {
                        Vector3 delta=targets[i]-p.transform.position;Vector3 flat=Vector3.ProjectOnPlane(delta,Vector3.up);
                        if(flat.magnitude<.35f&&Mathf.Abs(delta.y)<.3f){arrived=true;break;}
                        if(motor.IsGrounded && delta.y>.25f && flat.magnitude<3)motor.TryJump();
                        if(n%15==0){if(motor.IsGrounded&&flat.magnitude>.4f&&(p.transform.position-lastProgress).sqrMagnitude<.01f)motor.TryJump();lastProgress=p.transform.position;}
                        motor.Simulate(flat.normalized*p.Tuning.moveSpeed,.02f);
                    }
                    if(!arrived)return "FAIL route segment "+i+" position="+p.transform.position+" target="+targets[i];
                }
                return "PASS CharacterController traversal: "+targets.Count+" authored landing/route points";
            }
            finally{UnityEngine.Object.DestroyImmediate(fixture);if(blocker!=null)blocker.SetActive(closed);motor.RestoreState(before);}
        }
    }
}
