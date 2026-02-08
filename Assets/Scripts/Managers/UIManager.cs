using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Singleton]
public class UIManager : MonoBehaviour
{
    public TMP_Dropdown speedDropdown, mapTypeDropdown, algorithmTypeDropdown;
    public TextMeshProUGUI nodeCountCounter, connectionPercentCounter, workingText;
    public Slider nodeCountSlider, connectionPercentSlider;
    public Toggle showWeightsToggle;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.GetSingleton();
        ChangeSettings();
    }

    public delegate void ChangedSettings();
    public event ChangedSettings OnSettingsChange;

    public void ChangeSettings()
    {
        gameManager.mapType = (MapType)mapTypeDropdown.value;
        gameManager.algorithmType = (AlgorithmType)algorithmTypeDropdown.value;
        gameManager.algorithmSpeed = (AlgorithmSpeed)speedDropdown.value;

        gameManager.nodeCount = (int)nodeCountSlider.value;
        nodeCountCounter.text = $"{nodeCountSlider.value}";
        
        connectionPercentSlider.maxValue = gameManager.nodeCount-1;
        gameManager.connectionCount = (int)connectionPercentSlider.value;
        connectionPercentCounter.text = $"{connectionPercentSlider.value}";

        gameManager.SetShowWeightText(showWeightsToggle.isOn);

        OnSettingsChange.Invoke();
    }

    public void UpdateWorkingText(bool state)
    {
        workingText.gameObject.SetActive(state);
    }

    public void GenerateMap()
    {
        gameManager.GenerateMap();
    }

    public void RandomiseStart()
    {
        gameManager.RandomiseStart();
    }

    public void RandomiseEnd()
    {
        gameManager.RandomiseEnd();
    }

    public void StartCalculateCosts()
    {
        gameManager.StartCalculateCosts();
    }

    public static UIManager GetSingleton()
    {
        return FindAnyObjectByType<UIManager>();
    }
}
