using UnityEngine;

public class MainMenuController : MonoBehaviour
{
   // atribuindo a cena em uma variável
   [Header("Scenes")]
   [SerializeField] private string gameScene = "Gameplay";

   [Header("Panels")]
   [SerializeField] private GameObject menuPanel;
   [SerializeField] private GameObject creditsPanel;

   [Header("Animações dos botões")]
    [SerializeField] private AnimationButton[] menuButtons;

    // funcionalidade para botão de Novo Jogo
   public void NewGame()
    {
        Debug.Log("Iniciando LogiVery...");

        //carrega a cena do jogo
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
    }

    // funcionalidade para botão de Youtube
    public void OpenYoutube()
    {
        Debug.Log("Abrindo Youtube...");

        // abre o link do youtube
        Application.OpenURL("https://www.youtube.com/");
    }

    public void OpenCredits()
    {
        Debug.Log("Abrindo Créditos...");

        // ativa o painel de créditos e desativa o painel do menu
        creditsPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        // ativa o painel do menu e desativa o painel de créditos
        menuPanel.SetActive(true);
        creditsPanel.SetActive(false);

        // reinicia a animação dos botões do menu principal
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
        
        Debug.Log("Voltando ao menu principal...");
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");

        // fecha o jogo
        Application.Quit();
    }
}
