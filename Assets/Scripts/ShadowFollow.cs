using UnityEngine;

public class ShadowFollow : MonoBehaviour
{
    public Transform target; // O objeto do player que a sombra deve seguir
    public float maxDistance = 100f; // Alcance máximo do raio
    public LayerMask groundLayer; // Camada que representa o chão

    void Update()
    {
        if (target != null)
        {
            // Lança um raio para baixo a partir da posição do player
            Ray ray = new Ray(target.position, Vector3.down);
            RaycastHit hit;

            // Verifica se o raio atingiu algo dentro da distância máxima
            if (Physics.Raycast(ray, out hit, maxDistance, groundLayer))
            {
                // Posiciona a sombra no ponto de impacto do raio
                transform.position = hit.point;
                // Opcional: Alinha a sombra com a superfície do chão
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                // Mantém a sombra plana no chão
                // sobe um pouco a sombra para evitar z-fighting
                transform.position += Vector3.up * 0.01f;
            }
            else
            {
                // Se o raio não atingir nada, posiciona a sombra a uma distância fixa abaixo do player
                transform.position = target.position + Vector3.down * maxDistance;
            }
        }
    }
}