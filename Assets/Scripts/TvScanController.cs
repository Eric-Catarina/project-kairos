using UnityEngine;

public class TVScanMaterialController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Material backgroundMaterial; // Arraste o TVScanBackgroundMaterial aqui
    [SerializeField] private string scanSpeedProperty = "_ScanSpeed"; // Nome da propriedade no shader (padrão)
    [SerializeField] private string distortionStrengthProperty = "_DistortionStrength"; // Nome da propriedade no shader

    [Header("Configurações")]
    [SerializeField] private float scanSpeed = 2f; // Velocidade das linhas de scan
    [SerializeField] private float distortionStrength = 0.05f; // Intensidade da distorção tremelique
    [SerializeField] private float flickerSpeed = 1f; // Velocidade do flicker (se usado no shader)

    private float timeAccumulator = 0f; // Acumulador de tempo não escalado

    private void Update()
    {
        // Usa Time.unscaledDeltaTime para continuar mesmo com Time.timeScale = 0
        timeAccumulator += Time.unscaledDeltaTime;

        // Atualiza as propriedades do material com base no tempo acumulado
        if (backgroundMaterial != null)
        {
            // Para scan: anima baseado no tempo (ex.: seno para movimento contínuo)
            float scanValue = Mathf.Sin(timeAccumulator * scanSpeed) * 0.5f + 0.5f; // Valor entre 0 e 1
            backgroundMaterial.SetFloat(scanSpeedProperty, scanValue);

            // Para distorção: varia aleatoriamente com tempo
            float distortionValue = distortionStrength + Mathf.PerlinNoise(timeAccumulator * flickerSpeed, 0f) * 0.02f; // Adiciona ruído
            backgroundMaterial.SetFloat(distortionStrengthProperty, distortionValue);

            // Se o shader tiver uma propriedade para tempo direto, passe o acumulador
            // backgroundMaterial.SetFloat("_UnscaledTime", timeAccumulator); // Descomente se o shader precisar
        }
    }
}