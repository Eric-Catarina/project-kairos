using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Referências de Painéis")]
    public GameObject[] panels;

    private GameObject activePanel;

    void Start()
    {

        if (panels.Length > 0)
        {
            SetActivePanel(panels[0]);
        }
    }

    public void SetActivePanel(GameObject newPanel)
    {
        if (activePanel != null)
            activePanel.SetActive(false);

        newPanel.SetActive(true);
        activePanel = newPanel;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }
}
