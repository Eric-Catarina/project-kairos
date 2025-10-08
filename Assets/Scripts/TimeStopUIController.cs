// Local: Assets/Scripts/UI/TimeStopUIController.cs

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TimeStopUIController : MonoBehaviour
{
    private Slider _timeStopSlider;

    private void Awake()
    {
        _timeStopSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Start é chamado depois de todos os Awakes, garantindo que TimeManipulationManager.Instance não seja nulo.
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.OnChargeChanged += UpdateSlider;
        }
        else
        {
            Debug.LogError("TimeManipulationManager não encontrado na cena! A UI da bateria não funcionará.");
            gameObject.SetActive(false); // Desativa a UI para evitar erros
        }
    }

    private void OnDestroy()
    {
        // É uma boa prática se desinscrever do evento quando o objeto da UI for destruído.
        if (TimeManipulationManager.Instance != null)
        {
            TimeManipulationManager.Instance.OnChargeChanged -= UpdateSlider;
        }
    }

    private void UpdateSlider(float normalizedCharge)
    {
        _timeStopSlider.value = normalizedCharge;
    }
}