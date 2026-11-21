
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField]
    private string gameScene = "Gameplay";

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject configPanel;

    [Header("Animacoes dos botoes")]
    [SerializeField]
    private AnimationButton[] menuButtons;

    [Header("Botao Continuar")]
    [SerializeField]
    private Button botaoContinuar;

    private void Start()
    {
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

    private void OnEnable()
    {
        AtualizarBotaoContinuar();
    }

    // =========================================
    // NOVO JOGO
    // =========================================

    public void NewGame()
    {
        Debug.Log("Iniciando novo jogo LogiVery...");

        // Apaga apenas o progresso anterior.
        // As configuracoes de audio permanecem salvas.
        GameplayManager.ApagarProgressoSalvo();

        Time.timeScale = 1f;

        SceneManager.LoadScene(gameScene);
    }

    // =========================================
    // CONTINUAR JOGO
    // =========================================

    public void ContinueGame()
    {
        if (!GameplayManager.ExisteProgressoSalvo())
        {
            Debug.LogWarning(
                "LogiVery: nao existe progresso salvo."
            );

            AtualizarBotaoContinuar();
            return;
        }

        Debug.Log(
            "LogiVery: continuando progresso salvo..."
        );

        Time.timeScale = 1f;

        SceneManager.LoadScene(gameScene);
    }

    private void AtualizarBotaoContinuar()
    {
        bool existeSalvamento =
            GameplayManager.ExisteProgressoSalvo();

        if (botaoContinuar != null)
        {
            // Esconde completamente o botao
            // quando nao existe salvamento.
            botaoContinuar.gameObject.SetActive(
                existeSalvamento
            );

            // Quando aparecer, permite clicar.
            botaoContinuar.interactable =
                existeSalvamento;
        }

        Debug.Log(
            "LogiVery: botao Continuar " +
            (existeSalvamento
                ? "visivel e habilitado."
                : "invisivel.")
        );
    }

    // =========================================
    // LINKS
    // =========================================

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

    // =========================================
    // PAINEIS
    // =========================================

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

        if (menuButtons != null)
        {
            foreach (AnimationButton button in menuButtons)
            {
                if (button != null)
                {
                    button.ResetAnimation();
                }
            }
        }

        AtualizarBotaoContinuar();

        Debug.Log("Voltando ao menu principal...");
    }

    // =========================================
    // SAIR
    // =========================================

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");

        Application.Quit();
    }
}
