using System.Collections;
using UnityEngine;

public class TelaControlador : MonoBehaviour
{
    [Header("Configurações Gerais")]
    [SerializeField] private Renderer telaRenderer;

    [Header("Efeito de Flicker (Piscar)")]
    [SerializeField] private bool habilitarFlicker = true;
    [SerializeField] [Range(0f, 1f)] private float intensidadeMinima = 0.8f;
    [SerializeField] private float velocidadeFlicker = 15f;

    [Header("Troca de Canal (Materiais)")]
    [SerializeField] private bool habilitarTrocaCanal = true;
    [SerializeField] private Material[] canais;
    [SerializeField] private float tempoPorCanal = 5f;

    private int indiceCanalAtual = 0;
    private Color corEmissaoBase;
    private float perlinOffset;

    void Start()
    {
        if (telaRenderer == null)
        {
            telaRenderer = GetComponent<Renderer>();
        }

        perlinOffset = Random.Range(0f, 100f);

        if (habilitarTrocaCanal && canais.Length > 0)
        {
            StartCoroutine(RotinaTrocaCanal());
        }
        else
        {
            ObterCorEmissaoBase();
        }
    }

    void Update()
    {
        if (habilitarFlicker && telaRenderer.material != null)
        {
            ExecutarFlicker();
        }
    }

    private void ExecutarFlicker()
    {
        float perlin = Mathf.PerlinNoise(Time.time * velocidadeFlicker, perlinOffset);
        float multiplicador = Mathf.Lerp(intensidadeMinima, 1.0f, perlin);

        telaRenderer.material.SetColor("_EmissionColor", corEmissaoBase * multiplicador);
    }

    private IEnumerator RotinaTrocaCanal()
    {
        TrocarParaCanal(indiceCanalAtual);

        while (true)
        {
            yield return new WaitForSeconds(tempoPorCanal);

            indiceCanalAtual = (indiceCanalAtual + 1) % canais.Length;
            
            TrocarParaCanal(indiceCanalAtual);
        }
    }

    private void TrocarParaCanal(int index)
    {
        if (index < 0 || index >= canais.Length) return;

        telaRenderer.material = canais[index];

        ObterCorEmissaoBase();
    }

    private void ObterCorEmissaoBase()
    {
        if (telaRenderer.material.HasProperty("_EmissionColor"))
        {
            corEmissaoBase = telaRenderer.material.GetColor("_EmissionColor");
        }
    }
}