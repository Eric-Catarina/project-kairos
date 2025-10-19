// Local: Assets/Scripts/UI/ScoreUIEntry.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI; // Para a cor

public class ScoreUIEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image background; // Opcional: para destacar a pontuação do jogador
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
        timeText.text = $"{data.scoreTime:F2}s";

        if (background != null)
        {
            background.color = isLocalPlayer ? highlightColor : _defaultColor;
        }
    }
}