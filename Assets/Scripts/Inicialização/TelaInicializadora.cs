
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TelaInicializadora : MonoBehaviour
{
    [Header("Cena de destino")]
    [SerializeField]
    private string cenaMenu = "MenuPrincipal";

    private Button botao;
    private bool iniciando;

    private void Awake()
    {
        botao = GetComponent<Button>();

        if (botao != null)
            botao.onClick.AddListener(IrParaMenu);
    }

    private void Update()
    {
        bool enterPressionado = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            enterPressionado =
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        enterPressionado |=
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter);
#endif

        if (enterPressionado)
            IrParaMenu();
    }

    public void IrParaMenu()
    {
        if (iniciando)
            return;

        iniciando = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(cenaMenu);
    }

    private void OnDestroy()
    {
        if (botao != null)
            botao.onClick.RemoveListener(IrParaMenu);
    }
}
