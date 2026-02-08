using UnityEngine;

public enum MapType
{
    graph,
    maze,
}
public interface IPathfindMap
{
    void GenerateMap();
    void RandomiseStart();
    void RandomiseEnd();
    void StartPathfind();
    void StartCalculateCosts();
}
