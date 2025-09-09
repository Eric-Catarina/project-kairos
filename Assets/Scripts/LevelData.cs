// Local: Assets/Scripts/Scoring/LevelData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

// Enum para representar os ranques de forma clara e segura.
public enum Rank { S, A, B, C, D, None }

/// <summary>
/// Define a relação entre um ranque e o tempo máximo necessário para alcançá-lo.
/// </summary>
[Serializable]
public struct RankThreshold
{
    public Rank rank;
    [Tooltip("Tempo máximo em segundos para alcançar este ranque.")]
    public float timeRequired;
}

/// <summary>
/// ScriptableObject para armazenar os dados de pontuação de um nível específico.
/// Permite que designers configurem os tempos para cada ranque sem tocar no código.
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "Scoring/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Tooltip("Lista de tempos para cada ranque. Ordene do melhor (S) para o pior (D).")]
    [SerializeField] private List<RankThreshold> rankThresholds = new List<RankThreshold>();

    /// <summary>
    /// Calcula e retorna o ranque apropriado com base no tempo de conclusão do jogador.
    /// </summary>
    /// <param name="playerTime">O tempo que o jogador levou para completar o nível.</param>
    /// <returns>O ranque alcançado.</returns>
    public Rank GetRankForTime(float playerTime)
    {
        // Percorre a lista de tempos (que deve estar ordenada do melhor para o pior).
        foreach (var threshold in rankThresholds)
        {
            // Se o tempo do jogador for menor ou igual ao tempo exigido, ele ganha esse ranque.
            if (playerTime <= threshold.timeRequired)
            {
                return threshold.rank;
            }
        }
        // Se o tempo do jogador for maior que todos os tempos da lista, retorna o pior ranque possível.
        return Rank.D; 
    }
}