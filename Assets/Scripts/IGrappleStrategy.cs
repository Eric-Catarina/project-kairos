using UnityEngine;

/// <summary>
/// Define a interface para diferentes comportamentos do grappling hook (Padrão Strategy).
/// Cada estratégia encapsula um algoritmo: balançar, puxar um objeto, etc.
/// </summary>
public interface IGrappleStrategy
{
    /// <summary>
    /// Executa a lógica principal quando o gancho se conecta.
    /// </summary>
    /// <param name="context">A referência ao controlador do gancho que está usando esta estratégia.</param>
    void Execute(GrapplingHookController context);

    /// <summary>
    /// Interrompe a lógica da estratégia quando o gancho é solto.
    /// </summary>
    void Stop(GrapplingHookController context);

    /// <summary>
    /// Lógica a ser executada no FixedUpdate (relacionada à física).
    /// </summary>
    void FixedUpdate(GrapplingHookController context);

    /// <summary>
    /// Lógica a ser executada no LateUpdate (geralmente para renderização, como a corda).
    /// </summary>
    void LateUpdate(GrapplingHookController context);
}