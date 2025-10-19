using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectLogic : MonoBehaviour
{
    public List<Button> levelSelectButtons;

    private void Start()
    {
        int levelIndex = 1; // Começa do índice 1, assumindo que 0 é o menu principal

        for (int i = 0; i < levelSelectButtons.Count; i++)
        {
            int index = levelIndex; // Captura o valor atual de levelIndex
            levelSelectButtons[i].onClick.RemoveAllListeners();
            levelSelectButtons[i].onClick.AddListener(() => SceneManagerLogic.Instance.LoadSceneByBuildIndex(index));
            levelIndex++;
        }
    }
}
