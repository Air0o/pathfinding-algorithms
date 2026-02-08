using System.Collections;
using System.Collections.Generic;

public enum AlgorithmSpeed
{
    slow,
    medium,
    fast,
    superFast,
    performace
}
public interface IGraphAlgorithm
{
    void SetSpeed(AlgorithmSpeed speed);
    void Pathfind(GraphNode start, GraphNode end);
    void CalculateCosts(GraphNode start);
}