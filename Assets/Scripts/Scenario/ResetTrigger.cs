using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    [Tooltip("A Tag do objeto que deve ser resetado (ex: 'Car').")]
    public string targetTag = "Car";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            PositionMemory memory = other.GetComponentInParent<PositionMemory>();

            if (memory != null)
            {
                memory.ResetPosition();
            }
        }
    }
}