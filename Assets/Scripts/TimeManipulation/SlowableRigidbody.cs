// Local: Assets/Scripts/TimeManipulation/SlowableRigidbody.cs

using UnityEngine;

/// <summary>
/// Implementação concreta da interface ITimeSlowable para objetos com Rigidbody.
/// Desacelera o objeto aumentando seu drag linear e angular.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SlowableRigidbody : MonoBehaviour, ITimeSlowable
{
    [Header("Visual Feedback")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color slowDownColor = Color.cyan;

    private Rigidbody rb;
    private float originalDrag;
    private float originalAngularDrag;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Armazena os valores originais para poder restaurá-los depois.
        originalDrag = rb.linearDamping;
        originalAngularDrag = rb.angularDamping;

        if (objectRenderer != null && objectRenderer.material != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    /// <summary>
    /// Aplica a lógica de desaceleração: aumenta o drag do Rigidbody e muda a cor.
    /// </summary>
    /// <param name="slowFactor">Valor a ser somado ao drag original.</param>
    public void SlowDown(float slowFactor)
    {
        rb.linearDamping = originalDrag + slowFactor;
        rb.angularDamping = originalAngularDrag + slowFactor;

        // Feedback visual
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = slowDownColor;
        }
    }

    /// <summary>
    /// Restaura os valores originais do Rigidbody e a cor do material.
    /// </summary>
    public void RestoreNormalTime()
    {
        rb.linearDamping = originalDrag;
        rb.angularDamping = originalAngularDrag;
        
        // Restaura feedback visual
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }

    private void OnDestroy()
    {
        // Garante que a cor original seja restaurada se o objeto for destruído enquanto desacelerado.
        if (objectRenderer != null && objectRenderer.material != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }
}