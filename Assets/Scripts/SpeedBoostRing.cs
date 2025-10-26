// Local: Assets/Scripts/PowerUps/SpeedBoostRing.cs

using UnityEngine;
using System.Collections; // ---> ADICIONADO: Necessário para usar Corrotinas

public class SpeedBoostRing : BasePowerUpRing
{
    public enum BoostMode
    {
        [Tooltip("Multiplica a velocidade atual do jogador, mantendo sua direção.")]
        MultiplyVelocity,
        [Tooltip("Aplica um impulso na direção que o jogador está olhando.")]
        PlayerForwardDirection,
        [Tooltip("Aplica um impulso em uma direção fixa, definida neste componente.")]
        FixedDirection
    }

    [Header("Configurações do Impulso")]
    [SerializeField] private BoostMode boostMode = BoostMode.PlayerForwardDirection;
    [SerializeField] private float boostForce = 50f;
    [SerializeField] private float velocityMultiplier = 1.5f;
    [SerializeField] private Vector3 fixedDirection = Vector3.forward;
    [SerializeField] private bool resetMomentum = false;

    [Header("Efeitos Visuais")]
    [SerializeField] private ParticleSystem boostParticles;
    [Tooltip("Duração da aura de velocidade no jogador, em segundos.")]
    [SerializeField] private float auraDuration = 1f; // ---> ADICIONADO: Variável para a duração da aura

    protected override void ApplyEffect(GameObject playerObject)
    {
        // Toca o efeito de partículas no anel
        if (boostParticles != null)
        {
            boostParticles.Play();
        }

        // ---> SEÇÃO DA AURA ADICIONADA <---
        // Procura pelo sistema de partículas da aura no jogador
        var playerAura = playerObject.GetComponentInChildren<ParticleSystem>(); // Busca o primeiro ParticleSystem nos filhos do player
        if (playerAura != null)
        {
            // Inicia a corrotina que vai controlar a aura
            StartCoroutine(ActivateAura(playerAura));
        }
        // ---> FIM DA SEÇÃO DA AURA <---

        var playerMovement = playerObject.GetComponent<PlayerMovementController>();
        if (playerMovement == null) return;

        switch (boostMode)
        {
            case BoostMode.MultiplyVelocity:
                playerMovement.MultiplyVelocity(velocityMultiplier);
                break;
            case BoostMode.PlayerForwardDirection:
                Vector3 playerForward = playerMovement.GetOrientation().forward;
                playerMovement.ApplyExternalForce(playerForward, boostForce, resetMomentum);
                break;
            case BoostMode.FixedDirection:
                playerMovement.ApplyExternalForce(fixedDirection.normalized, boostForce, resetMomentum);
                break;
        }
    }
    
    // ---> NOVA FUNÇÃO ADICIONADA <---
    /// <summary>
    /// Corrotina que ativa a aura do jogador e a desativa após um tempo.
    /// </summary>
    private IEnumerator ActivateAura(ParticleSystem aura)
    {
        // Ativa a emissão de partículas da aura
        aura.Play();

        // Espera pela duração definida (ex: 1 segundo)
        yield return new WaitForSeconds(auraDuration);

        // Para a emissão de partículas da aura
        aura.Stop();
    }
    // ---> FIM DA NOVA FUNÇÃO <---

    private void OnDrawGizmosSelected()
    {
        if (boostMode == BoostMode.FixedDirection)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, fixedDirection.normalized * 5f);
            Gizmos.DrawWireSphere(transform.position + fixedDirection.normalized * 5f, 0.5f);
        }
    }
}