using UnityEngine;
namespace Herbalist.Levels
{
    [CreateAssetMenu(menuName="Herbalist/Stage/Sap Maze")]
    public sealed class SapMazeDefinition : ScriptableObject
    {
        public Vector2[] nodes;
        public Vector2Int[] edges;
        public int startNode, goalNode;
        public int[] leakNodes;
        [Min(.01f)] public float initialVolume=8;
        [Min(.01f)] public float speed=1.5f;
        [Min(.01f)] public float leakRadius=.45f;
        [Min(0)] public float lossPerSecond=6;
        [Min(0)] public float retryDelay=2;
        [Range(0,1)] public float inputDeadzone=.2f;
        [Range(0,1)] public float directionThreshold=.6f;
        public Vector2 Position(MazeRunState state) => Vector2.Lerp(nodes[state.From],nodes[state.To],state.Progress);
        public bool IsValid()
        {
            if(nodes==null || nodes.Length<2 || edges==null || leakNodes==null || startNode<0 || startNode>=nodes.Length || goalNode<0 || goalNode>=nodes.Length || startNode==goalNode)return false;
            foreach(var e in edges)if(e.x<0 || e.y<0 || e.x>=nodes.Length || e.y>=nodes.Length || e.x==e.y || Vector2.Distance(nodes[e.x],nodes[e.y])<.001f)return false;
            foreach(int n in leakNodes)if(n<0 || n>=nodes.Length)return false;
            return initialVolume>0 && speed>0 && leakRadius>0;
        }
    }
    public enum MazePhase { Running=1, Retrying=2, Solved=3 }
    public struct MazeRunState
    {
        public int From,To,Attempt;
        public float Progress,Volume,RetryRemaining;
        public MazePhase Phase;
    }
}