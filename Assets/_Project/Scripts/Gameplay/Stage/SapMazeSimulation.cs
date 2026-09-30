using System;
using UnityEngine;
namespace Herbalist.Levels
{
    // Pure graph simulation. Presentation and transport consume the same authoritative state.
    public static class SapMazeSimulation
    {
        public static MazeRunState Initial(SapMazeDefinition d,int attempt=0) =>
            new MazeRunState { From=d.startNode,To=d.startNode,Volume=d.initialVolume,Attempt=attempt,Phase=MazePhase.Running };
        public static bool AtLeak(SapMazeDefinition d,MazeRunState s,int index) =>
            s.Phase==MazePhase.Running && Vector2.Distance(d.Position(s),d.nodes[d.leakNodes[index]])<=d.leakRadius;
        public static void Tick(SapMazeDefinition d,ref MazeRunState s,Vector2 input,float dt,Func<int,bool> blocked)
        {
            if(dt<=0 || float.IsNaN(dt) || float.IsInfinity(dt))return;
            if(s.Phase==MazePhase.Solved)return;
            if(s.Phase==MazePhase.Retrying)
            {
                s.RetryRemaining-=dt;
                if(s.RetryRemaining<=0)s=Initial(d,s.Attempt+1);
                return;
            }
            if(float.IsNaN(input.x)||float.IsNaN(input.y)||float.IsInfinity(input.x)||float.IsInfinity(input.y))input=Vector2.zero;
            input=Vector2.ClampMagnitude(input,1);
            // Substeps prevent high frame times from skipping a leak or crossing a junction.
            int steps=Mathf.Max(1,Mathf.CeilToInt(dt*d.speed/(d.leakRadius*.5f)));
            float delta=dt/steps;
            for(int tick=0;tick<steps && s.Phase==MazePhase.Running;tick++)
            {
                for(int i=0;i<d.leakNodes.Length;i++)
                    if(AtLeak(d,s,i) && !blocked(i))s.Volume=Mathf.Max(0,s.Volume-d.lossPerSecond*delta);
                if(s.Volume<=0){s.Phase=MazePhase.Retrying;s.RetryRemaining=d.retryDelay;return;}
                if(input.magnitude<d.inputDeadzone)continue;
                var direction=input.normalized;
                if(s.From==s.To)
                {
                    int next=-1;float best=d.directionThreshold;
                    foreach(var edge in d.edges)
                    {
                        int node=edge.x==s.From?edge.y:edge.y==s.From?edge.x:-1;
                        if(node<0)continue;
                        float dot=Vector2.Dot((d.nodes[node]-d.nodes[s.From]).normalized,direction);
                        if(dot>best){best=dot;next=node;}
                    }
                    if(next<0)continue;
                    s.To=next;s.Progress=0;
                }
                var heading=(d.nodes[s.To]-d.nodes[s.From]).normalized;
                float alignment=Vector2.Dot(heading,direction);
                if(alignment < -d.directionThreshold){(s.From,s.To)=(s.To,s.From);s.Progress=1-s.Progress;alignment=-alignment;}
                if(alignment<d.directionThreshold)continue;
                s.Progress=Mathf.Min(1,s.Progress+d.speed*delta/Vector2.Distance(d.nodes[s.From],d.nodes[s.To]));
                if(s.Progress>=1 || Mathf.Approximately(s.Progress,1)){s.From=s.To;s.Progress=0;if(s.From==d.goalNode)s.Phase=MazePhase.Solved;}
            }
        }
    }
}