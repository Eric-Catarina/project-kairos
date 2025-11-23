using UnityEngine;
using System.Collections;

public class ScreenGlowController : MonoBehaviour
{
    [Header("=== Configuração SPEED (Dourado) ===")]
    public Renderer rendererSpeed;
    public string tagSpeed = "SpeedRing";
    public float speedIncremento = 1.5f;
    public float speedMaximo = 5.0f;

    [Header("=== Configuração JUMP (Azul) ===")]
    public Renderer rendererJump;
    public string tagJump = "JumpRing";
    public float jumpIncremento = 2.0f;
    public float jumpMaximo = 5.0f;

    [Header("=== Configuração DAMAGE (Vermelho/Glitch) ===")]
    public Renderer rendererDamage;
    public string tagDamage = "DamageZone";
    public float damageIncremento = 5.0f;
    public float damageMaximo = 5.0f;

    [Header("=== Configuração Geral ===")]
    public float duracaoDoFade = 1.5f;
    public string nomeDaPropriedade = "_AlphaIntensity"; 

    private Material _matSpeed;
    private float _intensidadeSpeed = 0f;
    private Coroutine _routineSpeed;

    private Material _matJump;
    private float _intensidadeJump = 0f;
    private Coroutine _routineJump;

    private Material _matDamage;
    private float _intensidadeDamage = 0f;
    private Coroutine _routineDamage;

    private int _propID;

    void Start()
    {
        _propID = Shader.PropertyToID(nomeDaPropriedade);

        // --- SETUP SPEED
        if (rendererSpeed != null)
        {
            rendererSpeed.enabled = true;
            _matSpeed = rendererSpeed.material;
            if (_matSpeed.HasProperty(_propID)) _matSpeed.SetFloat(_propID, 0f);
        }

        // --- SETUP JUMP
        if (rendererJump != null)
        {
            rendererJump.enabled = true;
            _matJump = rendererJump.material;
            if (_matJump.HasProperty(_propID)) _matJump.SetFloat(_propID, 0f);
        }

        if (rendererDamage != null)
        {
            _matDamage = rendererDamage.material;
            if (_matDamage.HasProperty(_propID)) _matDamage.SetFloat(_propID, 0f);
            
            rendererDamage.enabled = false; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagSpeed))
        {
            AtivarSpeed();
        }
        else if (other.CompareTag(tagJump))
        {
            AtivarJump();
        }
        else if (other.CompareTag(tagDamage))
        {
            AtivarDamage();
        }
    }

    // --- LÓGICA SPEED ---
    void AtivarSpeed()
    {
        if (_matSpeed == null) return;
        
        _intensidadeSpeed += speedIncremento;
        _intensidadeSpeed = Mathf.Min(_intensidadeSpeed, speedMaximo);
        
        _matSpeed.SetFloat(_propID, _intensidadeSpeed);

        if (_routineSpeed != null) StopCoroutine(_routineSpeed);
        _routineSpeed = StartCoroutine(FadeRoutine((val) => _intensidadeSpeed = val, _intensidadeSpeed, _matSpeed, null));
    }

    // --- LÓGICA JUMP ---
    void AtivarJump()
    {
        if (_matJump == null) return;

        _intensidadeJump += jumpIncremento;
        _intensidadeJump = Mathf.Min(_intensidadeJump, jumpMaximo);
        
        _matJump.SetFloat(_propID, _intensidadeJump);

        if (_routineJump != null) StopCoroutine(_routineJump);
        _routineJump = StartCoroutine(FadeRoutine((val) => _intensidadeJump = val, _intensidadeJump, _matJump, null));
    }

    // --- LÓGICA DAMAGE ---
    void AtivarDamage()
    {
        if (_matDamage == null) return;

        rendererDamage.enabled = true;

        _intensidadeDamage = damageIncremento; 
        _matDamage.SetFloat(_propID, _intensidadeDamage);

        if (_routineDamage != null) StopCoroutine(_routineDamage);
        
        _routineDamage = StartCoroutine(FadeRoutine((val) => _intensidadeDamage = val, _intensidadeDamage, _matDamage, rendererDamage));
    }

    // --- CORROTINA GENÉRICA ---
    IEnumerator FadeRoutine(System.Action<float> atualizarVar, float valorInicial, Material matAlvo, Renderer rendererParaDesligar)
    {
        float elapsed = 0f;

        while (elapsed < duracaoDoFade)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duracaoDoFade;

            float valorNovo = Mathf.Lerp(valorInicial, 0f, t * t);
            
            atualizarVar(valorNovo);
            matAlvo.SetFloat(_propID, valorNovo);

            yield return null;
        }

        // Finalização
        atualizarVar(0f);
        matAlvo.SetFloat(_propID, 0f);

        // SE foi passado um renderer (caso do Damage), desliga ele agora
        if (rendererParaDesligar != null)
        {
            rendererParaDesligar.enabled = false;
        }
    }
}