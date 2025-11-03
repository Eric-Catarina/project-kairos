// Assets/Scripts/UI/DoubleJumpUIController.cs

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class DoubleJumpUIController : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("A imagem da UI com o Image Type configurado para 'Filled'.")]
    [SerializeField] private Image doubleJumpIcon;

    [Header("Configurações da Animação")]
    [Tooltip("A duração da animação de preenchimento (fill).")]
    [SerializeField] private float animationDuration = 0.1f;
    [Tooltip("A força do efeito de 'punch' na escala.")]
    [SerializeField] private float punchStrength = 0.2f;
    [Tooltip("A duração do efeito de 'punch'.")]
    [SerializeField] private float punchDuration = 0.2f;

    private PlayerMovementController _playerMovementController;

    private void Awake()
    {
        _playerMovementController = FindObjectOfType<PlayerMovementController>();
    }

    private void OnEnable()
    {
        if (_playerMovementController == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _playerMovementController.OnDoubleJumpGained += HandleDoubleJumpGained;
        _playerMovementController.OnGroundLanded += HandleDoubleJumpGained;
        _playerMovementController.OnDoubleJumpUsed += HandleDoubleJumpUsed;

        SetInitialState();
    }

    private void OnDisable()
    {
        if (_playerMovementController == null) return;

        _playerMovementController.OnDoubleJumpGained -= HandleDoubleJumpGained;
        _playerMovementController.OnGroundLanded -= HandleDoubleJumpGained;

        _playerMovementController.OnDoubleJumpUsed -= HandleDoubleJumpUsed;
    }
    
    private void SetInitialState()
    {
        if (doubleJumpIcon != null)
        {
            doubleJumpIcon.fillAmount = _playerMovementController.canDoubleJump ? 1f : 0f;
        }
    }

    private void HandleDoubleJumpGained()
    {
        if(doubleJumpIcon.fillAmount >= 1f) return;
        AnimateIcon(1f);
    }

    private void HandleDoubleJumpUsed()
    {
        AnimateIcon(0f);
    }

    private void AnimateIcon(float targetFill)
    {
        if (doubleJumpIcon == null) return;

        doubleJumpIcon.DOKill();
        
        doubleJumpIcon.DOFillAmount(targetFill, animationDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                doubleJumpIcon.rectTransform.DOKill();
                doubleJumpIcon.rectTransform.DOPunchScale(Vector3.one * punchStrength, punchDuration, 10, 1);
            });
    }
}