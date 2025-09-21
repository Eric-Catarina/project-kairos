using UnityEngine;

public class BlimpMoving : MonoBehaviour
{
    public float speed = 5.0f;

    public float turnSpeed = 15.0f;

    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);

        transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
    }
}