using UnityEngine;
using UnityEngine.Rendering; // <--- BIBLIOTECA CERTA DO URP

public class VolumeTriggerController : MonoBehaviour
{
    [Header("Tags dos Objetos")]
    public string tagSpeed = "SpeedRing";
    public string tagJump = "JumpRing";
    public string tagDamage = "DamageZone";

    [Header("Configuração")]
    public float duracaoDoFlash = 0.5f; // Rápido, pois é um anel

    private Coroutine _currentRoutine;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se é um dos objetos de interesse
        if (other.CompareTag(tagSpeed) || other.CompareTag(tagJump) || other.CompareTag(tagDamage))
        {
            // Tenta pegar o Volume DO URP no objeto tocado
            Volume volumeTocado = other.GetComponent<Volume>();

            if (volumeTocado != null)
            {
                if (_currentRoutine != null) StopCoroutine(_currentRoutine);
                _currentRoutine = StartCoroutine(FlashVolume(volumeTocado));
            }
        }
    }

    System.Collections.IEnumerator FlashVolume(Volume targetVolume)
    {
        // Fase 1: Sobe o peso (Efeito aparece)
        float elapsed = 0f;
        while (elapsed < duracaoDoFlash / 2)
        {
            elapsed += Time.deltaTime;
            // Vai de 0 a 1
            targetVolume.weight = Mathf.Lerp(0f, 1f, elapsed / (duracaoDoFlash / 2));
            yield return null;
        }
        targetVolume.weight = 1f;

        // Fase 2: Desce o peso (Efeito some)
        elapsed = 0f;
        while (elapsed < duracaoDoFlash / 2)
        {
            elapsed += Time.deltaTime;
            // Vai de 1 a 0
            targetVolume.weight = Mathf.Lerp(1f, 0f, elapsed / (duracaoDoFlash / 2));
            yield return null;
        }
        targetVolume.weight = 0f;
    }
}