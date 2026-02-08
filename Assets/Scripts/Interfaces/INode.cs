using UnityEngine;

public interface INode
{
    void Highlight();
    void HighlightBetter();
    void HighlightPath();
    void HighlightStart();
    void HighlightEnd();
    void RemoveHighlight();

    Vector3 GetPosition();
}