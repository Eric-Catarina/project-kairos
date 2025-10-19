// Local: Assets/Scripts/UI/SettingsPanel.cs

using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : UIPanel
{
    [Header("Referências Internas")]
    [Tooltip("O botão 'Voltar' ou 'Fechar' dentro deste painel.")]
    [SerializeField] private Button backButton;

    private void Awake()
    {
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        // Remove listeners antigos do Inspector para evitar chamadas duplicadas
        backButton?.onClick.RemoveAllListeners();

        // Atribui o listener via código, garantindo a referência correta ao Singleton
        if (UIManager.Instance != null)
        {
            backButton?.onClick.AddListener(UIManager.Instance.CloseSettingsPanel);
        }
        else
        {
            Debug.LogError("UIManager.Instance não foi encontrado. O botão 'Back' do SettingsPanel não funcionará.");
        }
    }
}