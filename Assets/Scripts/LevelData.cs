// Local: Assets/Scripts/LevelData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public enum Rank { S, A, B, C, D, NA }

[Serializable]
public struct RankThreshold
{
    public Rank rank;
    [Tooltip("Tempo máximo em segundos para alcançar este ranque.")]
    public float timeRequired;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Scoring/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Identificação do Nível")]
    [Tooltip("Identificador único para este nível (ex: 'City_Dash_01').")]
    [SerializeField] private string levelId = "Level_01";
    [Tooltip("Versão do nível. Aumente se fizer mudanças que afetem os tempos.")]
    [SerializeField] private int levelVersion = 1;

    [Header("Configuração de Ranques")]
    [Tooltip("Lista de tempos para cada ranque. Ordene do melhor (S) para o pior (D).")]
    [SerializeField] private List<RankThreshold> rankThresholds = new List<RankThreshold>();

    public string GetFullLevelId() => $"{levelId}_v{levelVersion}";
    
    public Rank GetRankForTime(float playerTime)
    {
        foreach (var threshold in rankThresholds)
        {
            if (playerTime <= threshold.timeRequired)
            {
                return threshold.rank;
            }
        }
        return Rank.D; 
    }
}