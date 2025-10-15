// Local: Assets/Scripts/UI/LeaderboardUIController.cs

using UnityEngine;
using System.Linq; // Necessário para OrderBy

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private Transform topScoresContentParent;
    [SerializeField] private GameObject loadingIndicator;

    [Header("UI do Jogador Local")]
    [SerializeField] private GameObject playerScoreContainer;
    [SerializeField] private ScoreUIEntry playerScoreUIEntry;
    
    private float? _lastRunTime;

    public void SetLastRunTime(float time)
    {
        _lastRunTime = time;
    }

    public async void ShowLeaderboard()
    {
        if (levelData == null || LeaderboardManager.Instance == null || PlayerProfile.Instance == null)
        {
            Debug.LogError("Dependências não configuradas para o Leaderboard!");
            return;
        }

        ClearLeaderboard();
        gameObject.SetActive(true);
        if (playerScoreContainer != null) playerScoreContainer.SetActive(false);
        loadingIndicator.SetActive(true);
        
        // --- Passo 1: Exibição Imediata do Tempo Local ---
        ScoreEntry localRunEntry = null;
        if (_lastRunTime.HasValue)
        {
            localRunEntry = new ScoreEntry(
                PlayFabAuthManager.Instance.PlayFabId, // Usa o PlayFabId para consistência
                PlayerProfile.Instance.CurrentProfile.PlayerName,
                _lastRunTime.Value,
                levelData.GetFullLevelId()
            );
            
            if (playerScoreContainer != null && playerScoreUIEntry != null)
            {
                // Mostra o tempo da corrida atual instantaneamente, com ranking provisório
                playerScoreUIEntry.Populate(0, localRunEntry, true); 
                playerScoreContainer.SetActive(true);
            }
        }
        
        // --- Passo 2: Busca dos Dados do Servidor em Segundo Plano ---
        var leaderboardResult = await LeaderboardManager.Instance.GetLeaderboardWithPlayerAsync(levelData.GetFullLevelId(), 10);
        loadingIndicator.SetActive(false);

        // --- Passo 3: Atualização da UI com Dados Completos ---
        
        // Popula o Top 10 sem filtrar o jogador local
        if (leaderboardResult?.TopEntries != null)
        {
            foreach (var score in leaderboardResult.TopEntries)
            {
                GameObject entryGO = Instantiate(scoreEntryPrefab, topScoresContentParent);
                ScoreUIEntry entryUI = entryGO.GetComponent<ScoreUIEntry>();
                bool isLocalPlayer = score.playerId == PlayFabAuthManager.Instance.PlayFabId;
                entryUI.Populate(score.Position, score, isLocalPlayer);
            }
        }

        // Determina o melhor tempo real do jogador (comparando o da API com o da corrida atual)
        ScoreEntry bestPlayerEntry = leaderboardResult.PlayerEntry;
        if (localRunEntry != null)
        {
            // Se o tempo da corrida atual for melhor que o recorde online, ou se não houver recorde online
            if (bestPlayerEntry == null || localRunEntry.scoreTime < bestPlayerEntry.scoreTime)
            {
                bestPlayerEntry = localRunEntry;
                // Como não sabemos a posição real deste novo recorde, o rank fica como provisório ("--")
                bestPlayerEntry.Position = 0; 
            }
        }
        
        // Atualiza a seção do jogador local com o melhor tempo definitivo
        if (bestPlayerEntry != null && playerScoreContainer != null && playerScoreUIEntry != null)
        {
            playerScoreUIEntry.Populate(bestPlayerEntry.Position, bestPlayerEntry, true);
            playerScoreContainer.SetActive(true);
        }
        else if (playerScoreContainer != null)
        {
            // Se, mesmo após tudo, não há pontuação, esconde o container
            playerScoreContainer.SetActive(false);
        }

        _lastRunTime = null; // Reseta para a próxima vez
    }

    private void ClearLeaderboard()
    {
        foreach (Transform child in topScoresContentParent)
        {
            Destroy(child.gameObject);
        }
    }
}