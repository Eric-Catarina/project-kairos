// Assets/Scripts/TimeStopChanger.cs

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Linq; // Adicionado para facilitar a busca

public class TimeStopChanger : MonoBehaviour
{
    [Header("Global Volumes")]
    [Tooltip("Volume para o estado normal do jogo. Se não for atribuído, o primeiro Volume encontrado na cena será usado.")]
    public Volume volumeA; // Volume Normal
    [Tooltip("Volume para o efeito de time stop. Se não for atribuído, o segundo Volume encontrado na cena será usado.")]
    public Volume volumeB; // Volume do Time Stop

    [Header("Transition Settings")]
    public float transitionDuration = 0.2f;

    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        // Encontra e valida os volumes ao iniciar.
        FindAndValidateVolumes();
    }
    
    private void OnEnable()
    {
        if (volumeA == null || volumeB == null)
        {
            enabled = false; // Desativa o componente se a configuração estiver incorreta.
            return;
        }

        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.OnTimeStopStarted += HandleTimeStopStarted;
            TimeManipulationManager.Instance.OnTimeStopStopped += HandleTimeStopStopped;
            SetInitialState();
        }
        else
        {
            Debug.LogError("TimeManipulationManager.Instance não encontrado! O TimeStopChanger será desativado.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.OnTimeStopStarted -= HandleTimeStopStarted;
            TimeManipulationManager.Instance.OnTimeStopStopped -= HandleTimeStopStopped;
        }
    }

    // *** LÓGICA DE BUSCA RESTAURADA E MELHORADA ***
    private void FindAndValidateVolumes()
    {
        // Se ambos os volumes já foram atribuídos no Inspector, não faz nada.
        if (volumeA != null && volumeB != null)
        {
            return;
        }

        // Busca todos os componentes de Volume na cena, incluindo os inativos.
        var allVolumes = FindObjectsByType<Volume>(sortMode: FindObjectsSortMode.InstanceID);

        // Se não houver pelo menos dois volumes na cena, a busca automática não é possível.
        if (allVolumes.Length < 2)
        {
            // Apenas exibe o erro se os volumes não foram preenchidos manualmente.
            if (volumeA == null || volumeB == null)
            {
                Debug.LogError("TimeStopChanger não conseguiu encontrar pelo menos dois componentes 'Volume' na cena. Por favor, adicione-os à cena ou atribua-os manualmente no Inspector.", this);
            }
            return;
        }

        // Se o volumeA não foi definido, pega o primeiro da lista.
        if (volumeA == null)
        {
            volumeA = allVolumes[1];
        }

        // Se o volumeB não foi definido, busca o próximo volume que seja DIFERENTE do volumeA.
        if (volumeB == null)
        {
            // Usa Linq para encontrar o primeiro volume na lista que não seja o mesmo que o volumeA.
            volumeB = allVolumes.FirstOrDefault(v => v != volumeA);
        }

        // Validação final: Garante que, após a busca, temos dois volumes distintos.
        if (volumeA == null || volumeB == null || volumeA == volumeB)
        {
            Debug.LogError("A busca automática por Volumes falhou ou resultou em volumes duplicados. Verifique a configuração da sua cena ou atribua os volumes manualmente no Inspector.", this);
        }
    }

    private void SetInitialState()
    {
        if (volumeA == null || volumeB == null) return;
        
        bool isSlowed = TimeManipulationManager.Instance.IsTimeSlowed;

        volumeA.weight = isSlowed ? 0f : 1f;
        volumeA.enabled = !isSlowed;
        
        volumeB.weight = isSlowed ? 1f : 0f;
        volumeB.enabled = isSlowed;
    }

    private void HandleTimeStopStarted() => StartTransition(volumeA, volumeB);
    private void HandleTimeStopStopped() => StartTransition(volumeB, volumeA);
    
    private void StartTransition(Volume from, Volume to)
    {
        if (!gameObject.activeInHierarchy) return;
        StopTransition();
        _transitionCoroutine = StartCoroutine(TransitionVolumes(from, to, transitionDuration));
    }

    private void StopTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    private IEnumerator TransitionVolumes(Volume from, Volume to, float duration)
    {
        if (from != null) from.enabled = true;
        if (to != null) to.enabled = true;

        float time = 0f;
        float fromStart = from != null ? from.weight : 0f;
        float toStart = to != null ? to.weight : 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);

            if (from != null) from.weight = Mathf.Lerp(fromStart, 0f, t);
            if (to != null) to.weight = Mathf.Lerp(toStart, 1f, t);

            yield return null;
        }

        if (from != null) { from.weight = 0f; from.enabled = false; }
        if (to != null) { to.weight = 1f; to.enabled = true; }

        _transitionCoroutine = null;
    }
}