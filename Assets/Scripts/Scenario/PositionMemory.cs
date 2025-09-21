using UnityEngine;

public class PositionMemory : MonoBehaviour
{
    private Vector3 initialPosition;
    void Awake()
    {
        // Guarda a posição atual do objeto no momento em que ele é criado
        initialPosition = transform.position;
    }
    public void ResetPosition()
    {
        // Move o carro de volta para a posição inicial.
        transform.position = initialPosition;
    }
}