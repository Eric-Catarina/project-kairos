using UnityEngine;

/// <summary>
/// Interface para objetos que podem ser agarrados pelo GrapplingHookController.
/// Permite que o hook verifique dinamicamente a validade da conexão.
/// </summary>
public interface IGrappleable
{
    /// <summary>
    /// Verifica se o objeto ainda pode ser agarrado.
    /// Chamado pelo GrapplingHookController para validar a conexão atual.
    /// </summary>
    /// <returns>True se o objeto ainda é agarrável, False caso contrário.</returns>
    bool CanBeGrappledNow();

    /// <summary>
    /// Retorna o GameObject associado a esta interface.
    /// </summary>
    GameObject GetGameObject();
}