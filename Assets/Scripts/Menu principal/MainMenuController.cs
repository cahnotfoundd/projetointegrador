
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField]
    private string gameScene = "Gameplay";

    [Header("Cena inicializadora")]
    [SerializeField]
    private string initialScene = "Inicializadora";

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject configPanel;

    [Header("Botao Continuar")]
    [SerializeField] private GameObject botaoContinuar;

    [Header("Animacoes dos botoes")]
    [SerializeField]
    private AnimationButton[] menuButtons;

    private void Start()
    {
        Time.timeScale = 1f;

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (configPanel != null)
            configPanel.SetActive(false);

        if (menuPanel != null)
            menuPanel.SetActive(true);

        AtualizarBotaoContinuar();
    }

    public void AtualizarBotaoContinuar()
    {
        bool existeSalvamento =
            LogiVerySaveManager.ExisteSalvamento();

        if (botaoContinuar != null)
        {
            botaoContinuar.SetActive(
                existeSalvamento
            );
        }
        else
        {
            Debug.LogWarning(
                "LogiVery: atribua o Botao Continuar " +
                "no Inspector.",
                this
            );
        }

        Debug.Log(
            "LogiVery: salvamento completo encontrado: " +
            existeSalvamento
        );
    }

    public void NewGame()
    {
        GameplayManager.ApagarProgressoSalvo();

        Time.timeScale = 1f;

        Debug.Log(
            "LogiVery: iniciando nova partida."
        );

        SceneManager.LoadScene(gameScene);
    }

    public void ContinueGame()
    {
        if (!LogiVerySaveManager.ExisteSalvamento())
        {
            Debug.LogWarning(
                "LogiVery: nao existe partida salva."
            );

            AtualizarBotaoContinuar();
            return;
        }

        Time.timeScale = 1f;

        Debug.Log(
            "LogiVery: carregando partida salva."
        );

        SceneManager.LoadScene(gameScene);
    }

    public void OpenYoutube()
    {
        Debug.Log("Abrindo Youtube...");

        Application.OpenURL(
            "https://youtu.be/YkPjSgFzd_g"
        );
    }

    public void OpenSite()
    {
        Debug.Log("Abrindo Site...");

        Application.OpenURL(
            "https://urbanlinkflow.vercel.app/"
        );
    }

    public void OpenCredits()
    {
        Debug.Log("Abrindo Creditos...");

        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void OpenControls()
    {
        Debug.Log("Abrindo Controles...");

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void OpenConfig()
    {
        Debug.Log("Abrindo Configuracoes...");

        if (configPanel != null)
            configPanel.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (configPanel != null)
            configPanel.SetActive(false);

        AtualizarBotaoContinuar();

        if (menuButtons != null)
        {
            foreach (AnimationButton button in menuButtons)
            {
                if (button != null)
                    button.ResetAnimation();
            }
        }

        Debug.Log(
            "Voltando ao menu principal..."
        );
    }

    public void QuitScene()
    {
        Time.timeScale = 1f;

        Debug.Log(
            "Voltando a tela inicializadora..."
        );

        SceneManager.LoadScene(initialScene);
    }
}
