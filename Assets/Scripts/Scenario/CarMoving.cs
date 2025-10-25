using UnityEngine;

public class CarMoving : MonoBehaviour
{
    public float velocidade = 20.0f;

    void Update()
    {
        transform.Translate(Vector3.forward* velocidade * Time.deltaTime);
    }
}