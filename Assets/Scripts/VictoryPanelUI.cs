using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictoryPanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private TextMeshProUGUI deathsLabel;
    [SerializeField] private TextMeshProUGUI rankLabel;
    [SerializeField] private TextMeshProUGUI levelNameLabel;
    [SerializeField] private Button nextButton;

    public event System.Action OnNextClicked;

    private void Awake()
    {
        nextButton.onClick.AddListener(HandleNextClicked);
        gameObject.SetActive(false);
    }

    public void ShowResults(float time, int deaths, Rank rank)
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
        timeLabel.text = $" {timeSpan:mm\\:ss\\.ff}";

        deathsLabel.text = deaths.ToString();
        rankLabel.text = rank.ToString();
        levelNameLabel.text = SceneManagerLogic.Instance.GetCurrentLevelName().ToString();
    }

    private void HandleNextClicked()
    {
        OnNextClicked?.Invoke();
    }
}