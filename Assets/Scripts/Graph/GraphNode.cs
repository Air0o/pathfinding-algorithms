using System;
using System.Collections.Generic;
using NUnit.Framework.Internal.Commands;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UI;

public class GraphNode : MonoBehaviour, INode
{
    private Image bgImage;
    private TextMeshProUGUI text;

    private NodeData data = new();
    private List<GraphConnection> connections = new();

    private GameObject lineRendererPrefab;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.GetSingleton();
        data.cost = int.MaxValue;
        bgImage = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        EmptyText();
    }

    public List<GraphConnection> GetConnections()
    {
        return connections;
    }

    public void AddConnection(GraphConnection connection)
    {
        GraphNode other = (GraphNode)connection.getOther();
        if (other == this) return;

        if (IsConnectedTo(other)) return;

        if (connection.lineRenderer == null)
        {
            InitLineRenderer(connection);
        }

        connections.Add(connection);

        other.AddConnection(new(this, connection.GetWeight(), connection.lineRenderer));
    }

    public void InitLineRenderer(GraphConnection connection)
    {
        if (lineRendererPrefab == null)
        {
            lineRendererPrefab = Resources.Load("ConnectionPrefab") as GameObject;
        }

        GameObject prefabInstace = Instantiate(lineRendererPrefab, new Vector3(0, 0, 10), Quaternion.identity, transform);
        connection.lineRenderer = prefabInstace.GetComponent<LineRenderer>();

        connection.lineRenderer.SetPositions(new Vector3[]{
            transform.position,
            connection.getOther().GetPosition()
            });

        connection.text = connection.lineRenderer.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        connection.text.text = $"{connection.GetWeight()}";
        Vector3 textOffset = new Vector3(0, 0, 10);
        connection.text.transform.position = textOffset + 
        (transform.position + connection.getOther().GetPosition()) / 2;
        connection.text.gameObject.SetActive(gameManager.GetShowWeightText());
        
    }

    public bool IsConnectedTo(GraphNode node)
    {
        if (GetConnectionTo(node) == null)
        {
            return false;
        }
        return true;
    }

    public int GetCost()
    {
        return data.cost;
    }
    public void SetCost(int cost)
    {
        data.cost = cost;
        text.text = $"{cost}";
    }

    public void EmptyText()
    {
        text.text = "";
    }

    public void HighlightFrontierOrActive()
    {
        bgImage.color = ColorManager.FrontierNode;
    }
    public void HighlightPath()
    {
        bgImage.color = ColorManager.PathNode;
    }
    public void HighlightBetter()
    {
        bgImage.color = ColorManager.BetterHighlight;
    }
    public void Highlight()
    {
        bgImage.color = ColorManager.VisitedNode;
    }
    public void RemoveHighlight()
    {
        bgImage.color = Color.lightGray;
    }

    public void HighlightStart()
    {
        bgImage.color = ColorManager.StartNode;
    }
    public void HighlightEnd()
    {
        bgImage.color = ColorManager.GoalNode;
    }

    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        foreach (GraphConnection connection in connections)
        {
            Gizmos.DrawLine(transform.position, connection.getOther().transform.position);
        }
    }*/

    public GraphConnection GetConnectionTo(GraphNode next)
    {
        foreach (GraphConnection connection in connections)
        {
            if ((GraphNode)connection.getOther() == next)
            {
                return connection;
            }
        }

        return null;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
