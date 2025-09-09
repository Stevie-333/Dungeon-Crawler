using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    private HashSet<Vector2Int> walls;
    private int width, height;

    public AStarPathfinder(HashSet<Vector2Int> wallPositions, int mapWidth, int mapHeight)
    {
        walls = wallPositions;
        width = mapWidth;
        height = mapHeight;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        HashSet<Vector2Int> closedSet = new();
        PriorityQueue<Vector2Int> openSet = new();
        openSet.Enqueue(start, 0);

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, int> gScore = new() { [start] = 0 };

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x < 0 || neighbor.y < 0 || neighbor.x >= width || neighbor.y >= height)
                    continue;

                if (walls.Contains(neighbor) || closedSet.Contains(neighbor))
                    continue;

                int tentativeG = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    int fScore = tentativeG + Heuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        return new List<Vector2Int>(); // No path found
    }

    private int Heuristic(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private static readonly List<Vector2Int> Directions = new()
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
}

public class PriorityQueue<T>
{
    private readonly SortedSet<Node> queue;
    private int insertionIndex = 0;

    public PriorityQueue()
    {
        queue = new SortedSet<Node>(new NodeComparer());
    }

    public int Count => queue.Count;

    public void Enqueue(T item, int priority)
    {
        queue.Add(new Node(priority, insertionIndex++, item));
    }

    public T Dequeue()
    {
        if (queue.Count == 0)
            throw new InvalidOperationException("PriorityQueue is empty.");

        var node = queue.Min;
        queue.Remove(node);
        return node.Item;
    }

    private class Node
    {
        public int Priority { get; }
        public int Index { get; }
        public T Item { get; }

        public Node(int priority, int index, T item)
        {
            Priority = priority;
            Index = index;
            Item = item;
        }
    }

    private class NodeComparer : IComparer<Node>
    {
        public int Compare(Node a, Node b)
        {
            int result = a.Priority.CompareTo(b.Priority);
            if (result == 0)
            {
                result = a.Index.CompareTo(b.Index);
            }
            return result;
        }
    }
}