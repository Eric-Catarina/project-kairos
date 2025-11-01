// Local: Assets/Scripts/UI/ScoreUIEntry.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image background;
    [SerializeField] private Color highlightColor = Color.yellow;
    private Color _defaultColor;

    private void Awake()
    {
        if (background != null)
        {
            _defaultColor = background.color;
        }
    }

    public void Populate(int rank, ScoreEntry data, bool isLocalPlayer = false)
    {
        if (rank <= 0)
        {
            rankText.text = "--";
        }
        else
        {
            rankText.text = $"{rank}.";
        }
        
        nameText.text = data.playerName;
        timeText.text = $"{data.scoreTime:F3}s";

        if (background != null)
        {
            background.color = isLocalPlayer ? highlightColor : _defaultColor;
        }
    }
}