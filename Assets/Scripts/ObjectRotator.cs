using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [Tooltip("Velocidade de rotação em graus por segundo.")]
    public float rotationSpeed = 50f;

    [Tooltip("O eixo de rotação. Por exemplo: Vector3.up para o eixo Y.")]
    public Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}