// Local: Assets/Scripts/TimeManipulation/ITimeSlowable.cs

using UnityEngine;

/// <summary>
/// Define uma interface para objetos que podem ter seu tempo localmente desacelerado.
/// Implementa o Padrão de Estratégia, onde cada objeto define sua própria lógica
/// de como desacelerar e restaurar o tempo.
/// </summary>
public interface ITimeSlowable
{
    /// <summary>
    /// Salva o estado atual do objeto e inicia o efeito de desaceleração.
    /// </summary>
    /// <param name="slowPercentage">A porcentagem de lentidão (0-100). 100 significa parada total.</param>
    void SlowDown(float slowPercentage);
    void SetSlowDownColor(Color newColor);

    /// <summary>
    /// Restaura a velocidade e o comportamento normais do objeto a partir do estado salvo.
    /// </summary>
    void RestoreNormalTime();
}