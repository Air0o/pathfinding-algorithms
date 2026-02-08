
using System;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine;

public class GraphConnection : IConnection
{
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        GraphConnection other = (GraphConnection)obj;

        return weight == other.weight && this.other == other.other;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    
    public GraphConnection(GraphNode other, int weight, LineRenderer lineRenderer = null)
    {
        this.other = other;
        this.weight = weight;
        this.lineRenderer = lineRenderer;
    }
    private readonly int weight;
    private GraphNode other;
    public LineRenderer lineRenderer;
    public TextMeshProUGUI text;

    public void Highlight()
    {
        lineRenderer.startColor = ColorManager.PathEdge;
        lineRenderer.endColor = ColorManager.PathEdge;
    }
    public void HighlightPath()
    {
        lineRenderer.startColor = ColorManager.PathEdge;
        lineRenderer.endColor = ColorManager.PathEdge;
    }
    public void RemoveHighlight()
    {
        lineRenderer.startColor = Color.darkGray;
        lineRenderer.endColor = Color.darkGray;
    }

    public INode getOther()
    {
        return other;
    }

    public int GetWeight()
    {
        return weight;
    }

}