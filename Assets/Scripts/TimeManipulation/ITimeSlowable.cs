// Local: Assets/Scripts/TimeManipulation/ITimeSlowable.cs

/// <summary>
/// Define uma interface para objetos que podem ter seu tempo localmente desacelerado.
/// Implementa o Padrão de Estratégia, onde cada objeto define sua própria lógica
/// de como desacelerar e restaurar o tempo.
/// </summary>
public interface ITimeSlowable
{
    /// <summary>
    /// Inicia o efeito de desaceleração no objeto.
    /// </summary>
    /// <param name="slowFactor">O fator pelo qual o objeto deve ser desacelerado (ex: 10f para aumentar o drag).</param>
    void SlowDown(float slowFactor);

    /// <summary>
    /// Restaura a velocidade e o comportamento normais do objeto.
    /// </summary>
    void RestoreNormalTime();
}