using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

public class Graph : MonoBehaviour, IPathfindMap
{
    public Vector2 graphAreaSize;
    public int nodeCount;

    private List<GraphNode> nodes = new();
    private GraphNode start, end;
    private GameObject nodePrefab;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.GetSingleton();
        nodePrefab = Resources.Load("GraphNode") as GameObject;
    }

    public void ChangeShowWeights(bool show)
    { 
        foreach (GraphNode node in nodes)
        {
            foreach (GraphConnection connection in node.GetConnections())
            {
                if(connection != null && connection.text != null)
                    connection.text.gameObject.SetActive(show);
            }
        }
    }
    public void EraseGraph()
    {
        foreach (GraphNode node in nodes)
        {
            Destroy(node.gameObject);
        }
        nodes.Clear();
    }

    public IEnumerator GenerateGraph()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            yield return new WaitForEndOfFrame();
            CreateNode();
        }
        nodeCount = nodes.Count;
        Debug.Log("Placed " + nodeCount + " nodes");

        yield return new WaitForSeconds(.4f);

        for (int i = 0; i < nodeCount; i++)
        {
            yield return new WaitForEndOfFrame();

            for (int j = 0; j < gameManager.connectionCount; j++)
            {
                GraphNode other = nodes[Random.Range(0, nodeCount)];
                int patience = 5;
                while(other == nodes[i])
                {
                    if(patience <= 0)
                    {
                        break;
                    }
                    other = nodes[Random.Range(0, nodeCount)];
                    patience--;
                }
                nodes[i].AddConnection(new(other, Mathf.Max(1,(int)(Vector2.Distance(other.transform.position, nodes[i].transform.position) / 3))));
            }
            
        }
        
        gameManager.SetIsWorking(false);
    }
    private void CreateNode()
    {
        Vector2 pos = GetRandomPosition();
        int patience = 16;
        while (!CheckIfValidPosition(pos))
        {
            if (patience < 0)
            {
                Debug.LogWarning("Could not place node!");
                return;
            }
            patience--;
            pos = GetRandomPosition();

        }

        GameObject nodeObj = Instantiate(nodePrefab, pos, Quaternion.identity, transform);

        GraphNode node = nodeObj.GetComponent<GraphNode>();
        nodes.Add(node);
    }

    private Vector2 GetRandomPosition()
    {
        Vector3 worldPos = new Vector3(
            Random.Range(-graphAreaSize.x / 2, graphAreaSize.x / 2),
            Random.Range(-graphAreaSize.y / 2, graphAreaSize.y / 2),
            0
        );

        return worldPos + transform.position;
    }

    private bool CheckIfValidPosition(Vector2 pos)
    {
        foreach (GraphNode node in nodes)
        {
            if (Vector2.Distance(node.transform.position, pos) < node.transform.localScale.magnitude/1.5f)
            {
                return false;
            }
        }

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, graphAreaSize);
    }

    public void RandomiseStart()
    {
        if(start != null)
        {
            start.RemoveHighlight();
        }
        start = nodes[Random.Range(0, nodeCount)];
        start.HighlightStart();
    }

    public void RandomiseEnd()
    {
        if(end != null)
        {
            end.RemoveHighlight();
        }
        end = nodes[Random.Range(0, nodeCount)];
        end.HighlightEnd();
    }

    public void StartPathfind()
    {
        if (start == null || end == null) return;
    }

    public void StartCalculateCosts()
    {
        IGraphAlgorithm algo = gameManager.algorithmType switch
        {
            AlgorithmType.Dijkstra => gameObject.GetOrAddComponent<DijkstraGraph>(),
            AlgorithmType.Astar => throw new System.NotImplementedException(),                      //todo
            _ => throw new System.NotImplementedException(),
        };

        StopAllCoroutines();
        ((MonoBehaviour)algo).StopAllCoroutines();
        foreach (GraphNode node in nodes)
        {
            node.SetCost(0);
            node.EmptyText();
        }

        nodeCount = nodes.Count;
        if (start == null) RandomiseStart();

        algo.SetSpeed(gameManager.algorithmSpeed);
        algo.CalculateCosts(start);
    }

    public void GenerateMap()
    {
        gameManager.SetIsWorking(true);
        StopAllCoroutines();
        nodeCount = gameManager.nodeCount;
        EraseGraph();
        StartCoroutine(GenerateGraph());
    }
}
