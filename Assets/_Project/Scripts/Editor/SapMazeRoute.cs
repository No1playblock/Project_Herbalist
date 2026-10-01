using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Levels;

namespace Herbalist.Editor
{
    internal static class SapMazeRoute
    {
        public static List<int> Find(SapMazeDefinition maze, int start, int goal)
        {
            var parent = Enumerable.Repeat(-1, maze.nodes.Length).ToArray();
            var queue = new Queue<int>();
            queue.Enqueue(start);
            parent[start] = start;
            while (queue.Count > 0)
            {
                int node = queue.Dequeue();
                if (node == goal) break;
                foreach (var edge in maze.edges)
                {
                    int next = edge.x == node ? edge.y : edge.y == node ? edge.x : -1;
                    if (next >= 0 && parent[next] < 0) { parent[next] = node; queue.Enqueue(next); }
                }
            }
            if (parent[goal] < 0) throw new Exception("Disconnected maze");
            var route = new List<int>();
            for (int node = goal; ; node = parent[node]) { route.Add(node); if (node == start) break; }
            route.Reverse();
            return route;
        }
    }
}
