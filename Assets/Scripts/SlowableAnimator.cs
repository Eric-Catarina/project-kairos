using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SlowableAnimator : MonoBehaviour, ITimeSlowable
{
    private Animator _animator;
    private float _storedSpeed;
    private bool _isSlowed;
    
    // Cache para evitar alocação de memória em updates se necessário no futuro
    private Color _originalColor; 

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.Unregister(this);
        }
    }

    public void SlowDown(float slowPercentage)
    {
        if (_isSlowed) return;
        _isSlowed = true;

        _storedSpeed = _animator.speed;

        // Se slowPercentage for 100, o multiplicador será 0 (parada total)
        // Se for 50, o multiplicador será 0.5 (meia velocidade)
        float multiplier = 1f - (slowPercentage / 100f);
        
        _animator.speed = _storedSpeed * multiplier;
    }

    public void RestoreNormalTime()
    {
        if (!_isSlowed) return;
        _isSlowed = false;

        _animator.speed = _storedSpeed;
    }

    public void SetSlowDownColor(Color newColor)
    {
        // Implementação vazia propositalmente.
        // Geralmente a mudança de cor é responsabilidade do SlowableRigidbody ou de um script de Material separado.
        // Se desejar alterar a cor via Animator, isso deve ser feito via parâmetros do Animator, não diretamente aqui.
    }
}