// Local: Assets/Scripts/VFX/VFXFovScaler.cs

using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Escala um Transform para compensar as mudanças no Field of View (FOV) de uma câmera,
/// garantindo que o objeto pareça ter o mesmo tamanho na tela.
/// Ideal para VFX parentados a uma câmera.
/// </summary>
public class VFXFovScaler : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("A câmera Cinemachine cujo FOV será monitorado.")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [Tooltip("O Transform do objeto VFX que será escalado. Se nulo, usará o próprio Transform.")]
    [SerializeField] private Transform vfxTransform;

    private float _baseFov;
    private Vector3 _baseScale;

    private void Start()
    {
        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera não foi atribuída no VFXFovScaler.", this);
            enabled = false;
            return;
        }

        if (vfxTransform == null)
        {
            vfxTransform = transform;
        }

        // Armazena os valores iniciais como referência
        _baseFov = cinemachineCamera.Lens.FieldOfView;
        _baseScale = vfxTransform.localScale;
    }

    private void LateUpdate()
    {
        // Pega o FOV atual da câmera
        float currentFov = cinemachineCamera.Lens.FieldOfView;

        // Calcula o fator de escala necessário usando trigonometria (tangente do ângulo)
        // Isso garante que a escala seja proporcional à mudança de perspectiva
        float scaleFactor = CalculateScaleFactor(_baseFov, currentFov);

        // Aplica a nova escala
        vfxTransform.localScale = _baseScale * scaleFactor;
    }
    
    /// <summary>
    /// Calcula o fator de escala necessário para um objeto manter seu tamanho aparente na tela
    /// quando o FOV da câmera muda.
    /// </summary>
    private float CalculateScaleFactor(float fovBase, float fovCurrent)
    {
        if (fovBase <= 0 || fovCurrent <= 0) return 1f;

        // Converte FOV para radianos e calcula a tangente da metade do ângulo
        float baseTan = Mathf.Tan(fovBase * 0.5f * Mathf.Deg2Rad);
        float currentTan = Mathf.Tan(fovCurrent * 0.5f * Mathf.Deg2Rad);
        
        // A escala é a razão entre as tangentes
        return currentTan / baseTan;
    }
}