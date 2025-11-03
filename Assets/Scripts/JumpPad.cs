// Local: Assets/Scripts/JumpPad.cs

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpPad : MonoBehaviour
{
    [Header("Configurações do Impulso")]
    [Tooltip("A força do impulso a ser aplicada ao jogador.")]
    [SerializeField] private float launchForce = 30f;
    [Tooltip("O ângulo (em graus) do lançamento. 90 = vertical, 0 = horizontal.")]
    [SerializeField, Range(0f, 90f)] private float launchAngle = 45f;
    [Tooltip("Se verdadeiro, a velocidade do jogador será zerada antes de aplicar o impulso. Se falso, o impulso será somado à velocidade atual.")]
    [SerializeField] private bool resetPlayerMomentum = false;
    
    [Header("Efeitos")]
    [Tooltip("Partícula a ser instanciada no momento do impulso.")]
    [SerializeField] private GameObject launchEffect;
    [Tooltip("Nome do som a ser tocado no momento do impulso.")]
    [SerializeField] private string launchSfx = "JumpPadLaunch";

    [Header("Cooldown")]
    [Tooltip("Tempo em segundos antes que o jump pad possa ser usado novamente pelo mesmo jogador.")]
    [SerializeField] private float cooldownDuration = 0.5f;

    private float _lastLaunchTime = -999f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < _lastLaunchTime + cooldownDuration)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerMovementController playerMovement = other.GetComponent<PlayerMovementController>();
            if (playerMovement != null)
            {
                LaunchPlayer(playerMovement);
            }
        }
    }

    private void LaunchPlayer(PlayerMovementController player)
    {
        _lastLaunchTime = Time.time;
        
        Vector3 forwardDirection = transform.forward;
        Quaternion rotation = Quaternion.AngleAxis(-launchAngle, transform.right);
        Vector3 launchDirection = rotation * forwardDirection;

        player.ApplyExternalForce(launchDirection, launchForce, resetPlayerMomentum);
        player.ResetDoubleJump();
        
        TriggerEffects();
    }
    
    private void TriggerEffects()
    {
        if (launchEffect != null)
        {
            Instantiate(launchEffect, transform.position, transform.rotation);
        }

        if (!string.IsNullOrEmpty(launchSfx) && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(launchSfx);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 forwardDirection = transform.forward;
        Quaternion rotation = Quaternion.AngleAxis(-launchAngle, transform.right);
        Vector3 launchDirection = rotation * forwardDirection;
        
        Gizmos.DrawRay(transform.position, launchDirection * 5f);
    }
}