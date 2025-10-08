// Local: Assets/Scripts/UI/ScoreUIEntry.cs
using TMPro;
using UnityEngine;

public class ScoreUIEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI timeText;

    public void Populate(int rank, ScoreEntry data)
    {
        rankText.text = $"{rank}.";
        nameText.text = data.playerName;
        timeText.text = $"{data.scoreTime:F2}s";
    }
}