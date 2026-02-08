using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DijkstraGraph : MonoBehaviour, IGraphAlgorithm
{
    float visitDelay = 0;
    float processDelay = 0;
    float cleanupDelay = 0;
    float pathHighlightDelay = 0;
    
    
    public void SetSpeed(AlgorithmSpeed speed)
    {
        visitDelay = speed switch
        {
            AlgorithmSpeed.slow => .4f,
            AlgorithmSpeed.medium => .2f,
            AlgorithmSpeed.fast => 0,
            AlgorithmSpeed.superFast => -1,
            AlgorithmSpeed.performace => -2,
            _ => throw new System.NotImplementedException(),
        };
        processDelay = speed switch
        {
            AlgorithmSpeed.slow => .5f,
            AlgorithmSpeed.medium => .25f,
            AlgorithmSpeed.fast => 0,
            AlgorithmSpeed.superFast => -1,
            AlgorithmSpeed.performace => -2,
            _ => throw new System.NotImplementedException(),
        };
        cleanupDelay = speed switch
        {
            AlgorithmSpeed.slow => .2f,
            AlgorithmSpeed.medium => .1f,
            AlgorithmSpeed.fast => 0,
            AlgorithmSpeed.superFast => -1,
            AlgorithmSpeed.performace => -2,
            _ => throw new System.NotImplementedException(),
        };
        pathHighlightDelay = speed switch
        {
            AlgorithmSpeed.slow => .4f,
            AlgorithmSpeed.medium => .2f,
            AlgorithmSpeed.fast => 0,
            AlgorithmSpeed.superFast => -1,
            AlgorithmSpeed.performace => -2,
            _ => throw new System.NotImplementedException(),
        };
    }

    public void CalculateCosts(GraphNode start)
    {
        GameManager.GetSingleton().SetIsWorking(true);
        if(visitDelay == -2)
        {
            CalculateCostsNoAnimation(start);
            GameManager.GetSingleton().SetIsWorking(false);
        }
        else
        {
            StartCoroutine(CalculateCostsCoroutine(start));
        }
    }

    public IEnumerator CalculateCostsCoroutine(GraphNode start)
    {
        var openSet = new SortedSet<NodeCostPair>(
            Comparer<NodeCostPair>.Create((a, b) => a.cost.CompareTo(b.cost) != 0
                ? a.cost.CompareTo(b.cost)
                : a.node.GetInstanceID().CompareTo(b.node.GetInstanceID())));

        var cameFrom = new Dictionary<GraphNode, GraphNode>();           // ← added for path reconstruction
        var costSoFar = new Dictionary<GraphNode, int>();

        start.SetCost(0);
        openSet.Add(new NodeCostPair(start, 0));
        costSoFar[start] = 0;
        cameFrom[start] = null;  // or leave unset - we stop at start

        while (openSet.Count > 0)
        {
            var pair = openSet.Min;
            openSet.Remove(pair);
            GraphNode current = pair.node;

            if (current.GetCost() < pair.cost) continue;

            // Highlight current node as being processed
            current.Highlight();
            yield return new WaitForSeconds(visitDelay);
            

            // Highlight current best path TO this node (start → ... → current)
            HighlightPathToCurrent(cameFrom, current, start);

            if(pathHighlightDelay != -1)
            {
                yield return new WaitForSeconds(pathHighlightDelay); // ← give time to see the path
            }

            // Remove temporary path highlight (keep current node highlighted if you want)
            ClearPathHighlight(cameFrom, current, start);

            foreach (GraphConnection connection in current.GetConnections())
            {
                GraphNode neighbour = (GraphNode)connection.getOther();

                if(processDelay != -1)
                {
                    connection.Highlight();
                    neighbour.Highlight();
                    yield return new WaitForSeconds(processDelay);
                }

                int newCost = costSoFar[current] + connection.GetWeight();

                if (!costSoFar.ContainsKey(neighbour) || newCost < costSoFar[neighbour])
                {
                    costSoFar[neighbour] = newCost;
                    neighbour.SetCost(newCost);

                    cameFrom[neighbour] = current;   // ← record parent / predecessor

                    openSet.Add(new NodeCostPair(neighbour, newCost));

                    if(visitDelay != -1)
                    {
                        // Optional: special flash for better path found
                        neighbour.HighlightBetter();
                        yield return new WaitForSeconds(visitDelay);
                    }
                }

                if(cleanupDelay != -1)
                {
                    yield return new WaitForSeconds(cleanupDelay);
                }
                connection.RemoveHighlight();
                neighbour.RemoveHighlight();
            }
            current.RemoveHighlight();
            start.HighlightStart();
        }

        GameManager.GetSingleton().SetIsWorking(false);
        Debug.Log("Dijkstra finished.");
    }
    private void HighlightPathToCurrent(Dictionary<GraphNode, GraphNode> cameFrom, GraphNode current, GraphNode start)
    {
        var path = ReconstructPath(cameFrom, current, start);

        for (int i = 0; i < path.Count - 1; i++)
        {
            var node = path[i];
            var next = path[i + 1];

            // Highlight nodes along the path
            if (node != start && node != current)
                node.HighlightPath();  // ← use a dedicated path color, e.g. ColorManager.PathNode

            // Try to highlight the edge (you need a way to get connection between two nodes)
            var connection = node.GetConnectionTo(next); // ← implement this method on GraphNode
            if (connection != null)
            {
                connection.HighlightPath(); // e.g. ColorManager.PathEdge
            }
        }

        // Optional: make start and current stand out even more
        start.HighlightStart();
        current.HighlightFrontierOrActive();
    }

    private void ClearPathHighlight(Dictionary<GraphNode, GraphNode> cameFrom, GraphNode current, GraphNode start)
    {
        var path = ReconstructPath(cameFrom, current, start);

        for (int i = 0; i < path.Count - 1; i++)
        {
            var node = path[i];
            var next = path[i + 1];

            if (node != start && node != current)
                node.RemoveHighlight();

            var connection = node.GetConnectionTo(next);
            if (connection != null)
                connection.RemoveHighlight();
        }
    }

    private List<GraphNode> ReconstructPath(Dictionary<GraphNode, GraphNode> cameFrom, GraphNode current, GraphNode start)
    {
        var path = new List<GraphNode>();
        var node = current;

        while (node != null)
        {
            path.Add(node);
            if (node == start) break;
            cameFrom.TryGetValue(node, out node); // returns null when we reach start (or if no path)
        }

        path.Reverse(); // now it's start → ... → current
        return path;
    }
    // Small helper struct/class needed for SortedSet
    private struct NodeCostPair
    {
        public GraphNode node;
        public int cost;

        public NodeCostPair(GraphNode n, int c)
        {
            node = n;
            cost = c;
        }
    }
// Using .NET 8 / CommunityToolkit / custom implementation
// or any proper binary heap priority queue

    public void CalculateCostsNoAnimation(GraphNode start)
    {
        var openSet = new SortedSet<NodeCostPair>(
            Comparer<NodeCostPair>.Create((a, b) =>
                a.cost != b.cost ? a.cost.CompareTo(b.cost) : a.node.GetInstanceID().CompareTo(b.node.GetInstanceID())
            ));

        var costSoFar = new Dictionary<GraphNode, int>(64);

        costSoFar[start] = 0;
        openSet.Add(new NodeCostPair(start, 0));

        while (openSet.Count > 0)
        {
            var pair = openSet.Min;
            openSet.Remove(pair);
            GraphNode current = pair.node;

            // Skip outdated entries (very important!)
            if (!costSoFar.TryGetValue(current, out int knownCost) || knownCost < pair.cost)
                continue;

            foreach (var conn in current.GetConnections())
            {
                GraphNode next = (GraphNode)conn.getOther();
                int newCost = knownCost + conn.GetWeight();

                if (!costSoFar.TryGetValue(next, out int oldCost) || newCost < oldCost)
                {
                    costSoFar[next] = newCost;
                    openSet.Add(new NodeCostPair(next, newCost));
                }
            }
        }

        foreach (var kvp in costSoFar)
            kvp.Key.SetCost(kvp.Value);
    }
    public void Pathfind(GraphNode start, GraphNode end)
    {
        throw new System.NotImplementedException();
    }
}