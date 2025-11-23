using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectPanelController : MonoBehaviour
{
    [Header("Componentes de Painel")]
    [SerializeField] private Image panelImage;

    [Header("Listas de Botões e Imagens")]
    // Lista de componentes ButtonJuice (arrastados no Inspector)
    [SerializeField] private List<ButtonJuice> buttons;
    // Lista de Sprites (deve ter o mesmo tamanho da lista de Botões)
    [SerializeField] private List<Sprite> buttonSprites;

    void Start()
    {
        if (panelImage == null)
        {
            Debug.LogError("Panel Image não foi atribuído.", this);
            return;
        }

        if (buttons.Count != buttonSprites.Count)
        {
            Debug.LogError("O número de botões deve ser igual ao número de imagens!");
            return;
        }

        // Inicialmente, esconde a imagem
        HidePanelImage();

        // --- CENTRALIZAÇÃO DA LÓGICA DE EVENTOS (A MÁGICA) ---
        for (int i = 0; i < buttons.Count; i++)
        {
            // Captura o índice 'i' em uma variável local. 
            // Isso é CRUCIAL para garantir que o Lambda Function 'ShowImage'
            // use o índice correto quando for executado.
            int index = i;

            // Liga o evento OnHoverEnter do botão ao método ShowImage, 
            // passando o índice 'index' capturado.
            buttons[i].OnHoverEnter += () => ShowImage(index);

            // Liga o evento OnHoverExit do botão ao método HidePanelImage.
            buttons[i].OnHoverExit += HidePanelImage;
        }
    }

    // --- MÉTODOS DE AÇÃO ---

    private void ShowImage(int imageIndex)
    {
        panelImage.sprite = buttonSprites[imageIndex];
        panelImage.enabled = true; // Ativa a imagem do painel
    }

    private void HidePanelImage()
    {
        panelImage.sprite = null;
        panelImage.enabled = false; // Desativa a imagem do painel
    }
}
