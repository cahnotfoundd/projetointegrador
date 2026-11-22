
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
    [SerializeField]
    private GameObject menuPanel;

    [SerializeField]
    private GameObject creditsPanel;

    [SerializeField]
    private GameObject controlsPanel;

    [SerializeField]
    private GameObject configPanel;

    [Header("Botao Continuar")]
    [SerializeField]
    private GameObject botaoContinuar;

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

    // Verifica se existe uma partida salva.
    // Sem salvamento, o botao fica INVISIVEL.
    public void AtualizarBotaoContinuar()
    {
        bool existeSalvamento =
            GameplayManager.ExisteProgressoSalvo();

        if (botaoContinuar != null)
        {
            botaoContinuar.SetActive(existeSalvamento);
        }
        else
        {
            Debug.LogWarning(
                "LogiVery: Botao Continuar nao foi " +
                "atribuido no Inspector.",
                this
            );
        }

        Debug.Log(
            "LogiVery: progresso salvo encontrado: " +
            existeSalvamento
        );
    }

    // NOVO JOGO
    public void NewGame()
    {
        // Um novo jogo nao deve recuperar
        // o progresso de uma partida anterior.
        GameplayManager.ApagarProgressoSalvo();

        Time.timeScale = 1f;

        Debug.Log(
            "LogiVery: iniciando nova partida."
        );

        SceneManager.LoadScene(gameScene);
    }

    // CONTINUAR JOGO
    public void ContinueGame()
    {
        if (!GameplayManager.ExisteProgressoSalvo())
        {
            Debug.LogWarning(
                "LogiVery: nenhum progresso salvo " +
                "para continuar."
            );

            AtualizarBotaoContinuar();
            return;
        }

        Time.timeScale = 1f;

        Debug.Log(
            "LogiVery: continuando partida salva."
        );

        // O GameplayManager restaurara os dados.
        SceneManager.LoadScene(gameScene);
    }

    // YOUTUBE
    public void OpenYoutube()
    {
        Debug.Log("Abrindo Youtube...");

        Application.OpenURL(
            "https://youtu.be/YkPjSgFzd_g"
        );
    }

    // SITE
    public void OpenSite()
    {
        Debug.Log("Abrindo Site...");

        Application.OpenURL(
            "https://urbanlinkflow.vercel.app/"
        );
    }

    // CREDITOS
    public void OpenCredits()
    {
        Debug.Log("Abrindo Creditos...");

        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    // CONTROLES
    public void OpenControls()
    {
        Debug.Log("Abrindo Controles...");

        if (controlsPanel != null)
            controlsPanel.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    // CONFIGURACOES
    public void OpenConfig()
    {
        Debug.Log("Abrindo Configuracoes...");

        if (configPanel != null)
            configPanel.SetActive(true);

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    // VOLTAR AO MENU PRINCIPAL
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

    // SAIR PARA A INICIALIZADORA
    public void QuitScene()
    {
        Time.timeScale = 1f;

        Debug.Log(
            "Voltando a tela inicializadora..."
        );

        SceneManager.LoadScene(initialScene);
    }
}
