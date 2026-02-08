using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class GameManager : MonoBehaviour
{
    public MapType mapType = MapType.graph;
    public AlgorithmType algorithmType = AlgorithmType.Dijkstra;
    public AlgorithmSpeed algorithmSpeed = AlgorithmSpeed.slow;
    public int nodeCount = 2;
    public int connectionCount = 50;

    private bool isWorking = false;
    private bool showWeightText = true;

    public GameObject graph, maze;

    private IPathfindMap map;
    private UIManager uIManager;

    private void Start() {
        Application.targetFrameRate = 60;
        uIManager = UIManager.GetSingleton();
        uIManager.OnSettingsChange += ChangedSettings;
    }


    private void ChangedSettings()
    {     
        switch (mapType)
        {
            case MapType.maze:
            maze.SetActive(true);
            map = maze.GetComponent<IPathfindMap>();
            break;
            case MapType.graph:
            graph.SetActive(true);
            map = graph.GetComponent<IPathfindMap>();
            break;
        }
    }

    public void SetShowWeightText(bool state)
    {
        if(state != showWeightText)
        {
            ((Graph)map).ChangeShowWeights(state);
        }

        showWeightText = state;
    }

    public void SetIsWorking(bool state)
    {
        if(state != isWorking)
        {
            uIManager.UpdateWorkingText(state);
        }
        isWorking = state;
    }

    public bool GetIsWorking()
    {
        return isWorking;
    }

    public bool GetShowWeightText()
    {
        return showWeightText;
    }

    public static GameManager GetSingleton()
    {
        return FindAnyObjectByType<GameManager>();
    }

    public void GenerateMap()
    {
        map.GenerateMap();
    }

    public void RandomiseStart()
    {
        map.RandomiseStart();
    }

    public void RandomiseEnd()
    {
        map.RandomiseEnd();
    }

    public void StartCalculateCosts()
    {
        map.StartCalculateCosts();
    }
}