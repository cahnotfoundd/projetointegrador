
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LogiVeryControleEscape : MonoBehaviour
{
    private void Start()
    {
        Debug.Log(
            "LogiVery: controle ESC ativado na cena " +
            SceneManager.GetActiveScene().name
        );
    }

    private void Update()
    {
        bool escapePressionado = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            escapePressionado = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escapePressionado = true;
        }
#endif

        if (escapePressionado)
        {
            FecharJogo();
        }
    }

    private void FecharJogo()
    {
        Debug.Log(
            "LogiVery: ESC pressionado. Encerrando jogo..."
        );

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
