
using UnityEngine;
using UnityEngine.SceneManagement;

public class BotaoFimDeJogo : MonoBehaviour
{
    [Header("Cena do menu principal")]
    [SerializeField]
    private string cenaMenuPrincipal = "MenuPrincipal";

    public void FinalizarJogo()
    {
        // Apaga o progresso da partida concluida.
        // Nao apaga as configuracoes de audio.
        GameplayManager.ApagarProgressoSalvo();

        // Garante que o tempo volte ao normal.
        Time.timeScale = 1f;

        Debug.Log(
            "LogiVery: partida concluida. " +
            "Progresso encerrado."
        );

        // Retorna ao menu principal.
        SceneManager.LoadScene(cenaMenuPrincipal);
    }
}
