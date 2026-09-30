using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Herbalist.Levels;
namespace Herbalist.Editor
{
    public static class StageFourReferenceChecks
    {
        public static bool Reachable(SapMazeDefinition d,int goal)
        {
            var seen=new HashSet<int>{d.startNode};var queue=new Queue<int>();queue.Enqueue(d.startNode);
            while(queue.Count>0)
            {
                int n=queue.Dequeue();if(n==goal)return true;
                foreach(var edge in d.edges)
                {
                    int next=edge.x==n?edge.y:edge.y==n?edge.x:-1;
                    if(next>=0&&seen.Add(next))queue.Enqueue(next);
                }
            }
            return false;
        }
        public static string Run()
        {
            var messages=new List<string>();
            for(int i=0;i<3;i++)
            {
                var d=AssetDatabase.LoadAssetAtPath<SapMazeDefinition>(
                    "Assets/_Project/Data/Stages/Revision_260928/SO_RevisionMaze_"+(i+1)+".asset");
                if(!d.IsValid()||d.leakNodes.Length!=i+3)throw new Exception("Invalid reference maze "+i);
                foreach(var edge in d.edges)
                {
                    var delta=d.nodes[edge.y]-d.nodes[edge.x];
                    if(Mathf.Abs(delta.x)>.0001f&&Mathf.Abs(delta.y)>.0001f)throw new Exception("Non-cardinal reference edge");
                }
                var unreachable=Enumerable.Range(0,d.leakNodes.Length).Where(n=>!Reachable(d,d.leakNodes[n])).ToArray();
                if(!unreachable.SequenceEqual(i==1?new[]{0}:new int[0]))throw new Exception("Unexpected unreachable leak");
                var route=SapMazeRoute.Find(d,d.startNode,d.goalNode);
                var state=SapMazeSimulation.Initial(d);int index=1,guard=0;
                while(state.Phase==MazePhase.Running&&guard++<30000)
                {
                    if(state.From==route[index]&&state.To==state.From)index++;
                    if(index>=route.Count)break;
                    var delta=d.nodes[route[index]]-d.Position(state);
                    var key=Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?new Vector2(Mathf.Sign(delta.x),0):new Vector2(0,Mathf.Sign(delta.y));
                    SapMazeSimulation.Tick(d,ref state,key,.01f,_=>true);
                }
                if(state.Phase!=MazePhase.Solved)throw new Exception("Cardinal route failed "+i);
                messages.Add("PDF page "+(28+i)+": cardinal route to goal PASS; "+d.leakNodes.Length+" reference leaks");
            }
            messages.Add("Source preserved: page 29 leak 1 is enclosed and unreachable.");
            return string.Join("\n",messages);
        }
    }
}
