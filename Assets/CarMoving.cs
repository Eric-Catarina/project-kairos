using UnityEngine;

public class CarMoving : MonoBehaviour
{
    // Variável pública para controlar a velocidade pelo Inspector
    [Tooltip("A velocidade de movimento do objeto no eixo X negativo.")]
    public float velocidade = 20.0f;

    // Update é chamado a cada frame
    void Update()
    {
        // Usamos Vector3.left para a direção X negativa.
        // O resto da lógica (multiplicar pela velocidade e tempo) continua igual.
        transform.Translate(Vector3.left * velocidade * Time.deltaTime);
    }
}