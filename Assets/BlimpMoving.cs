using UnityEngine;

public class BlimpMoving : MonoBehaviour
{
    [Tooltip("A velocidade com que o objeto se move para a frente.")]
    public float speed = 5.0f;

    [Tooltip("A velocidade com que o objeto gira. Use valores pequenos como 10 ou 20. Zero para não girar.")]
    public float turnSpeed = 15.0f; // Nova variável

    void Update()
    {
        // Movimento para a frente (igual ao anterior)
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);

        // Rotação suave no eixo Y (gira para os lados)
        transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
    }
}