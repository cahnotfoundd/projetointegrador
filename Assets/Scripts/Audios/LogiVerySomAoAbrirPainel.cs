
using UnityEngine;

public class LogiVerySomAoAbrirPainel : MonoBehaviour
{
    public enum TipoSom
    {
        PopUp,
        Notificacao,
        AnimacaoCliente,
        Vitoria
    }

    [Header("Som ao abrir este painel")]
    [SerializeField]
    private TipoSom tipoSom = TipoSom.PopUp;

    [Header("Deteccao de visibilidade")]
    [SerializeField]
    private bool verificarCanvasGroup = true;

    [Range(0f, 1f)]
    [SerializeField]
    private float alphaMinimoVisivel = 0.05f;

    [Header("Diagnostico")]
    [SerializeField]
    private bool mostrarDiagnostico = true;

    private CanvasGroup[] gruposCanvas;
    private bool somTocadoNestaAbertura;

    private void Awake()
    {
        AtualizarGruposCanvas();
    }

    private void OnEnable()
    {
        somTocadoNestaAbertura = false;

        AtualizarGruposCanvas();

        if (mostrarDiagnostico)
        {
            Debug.Log(
                "LogiVery: painel ativado: " +
                gameObject.name +
                " | Tipo: " + tipoSom,
                this
            );
        }

        VerificarAbertura();
    }

    private void Start()
    {
        // Tenta novamente se o gerenciador de audio
        // ainda nao existia no OnEnable.
        VerificarAbertura();
    }

    private void Update()
    {
        // Tambem detecta quando um CanvasGroup
        // muda de invisivel para visivel.
        VerificarAbertura();
    }

    private void OnTransformParentChanged()
    {
        AtualizarGruposCanvas();
    }

    private void AtualizarGruposCanvas()
    {
        gruposCanvas =
            GetComponentsInParent<CanvasGroup>(true);
    }

    private bool PainelEstaVisivel()
    {
        if (!isActiveAndEnabled ||
            !gameObject.activeInHierarchy)
        {
            return false;
        }

        if (!verificarCanvasGroup)
            return true;

        if (gruposCanvas == null)
            AtualizarGruposCanvas();

        foreach (CanvasGroup grupo in gruposCanvas)
        {
            if (grupo == null)
                continue;

            if (grupo.alpha <= alphaMinimoVisivel)
                return false;
        }

        return true;
    }

    private void VerificarAbertura()
    {
        bool visivel = PainelEstaVisivel();

        if (!visivel)
        {
            // Permite tocar novamente na proxima abertura.
            somTocadoNestaAbertura = false;
            return;
        }

        if (somTocadoNestaAbertura)
            return;

        LogiVeryEfeitosSonoros sons =
            LogiVeryEfeitosSonoros.Instancia;

        // Se o gerenciador ainda nao estiver pronto,
        // o Update tentara novamente.
        if (sons == null)
            return;

        switch (tipoSom)
        {
            case TipoSom.PopUp:
                sons.TocarPopUps();
                somTocadoNestaAbertura = true;
                break;

            case TipoSom.Vitoria:
                sons.TocarComemoracaoVitoria();
                somTocadoNestaAbertura = true;
                break;

            case TipoSom.Notificacao:
                // Som controlado pelo DeliveryScreenManager.
                break;

            case TipoSom.AnimacaoCliente:
                // Som controlado pelo DeliveryScreenManager.
                break;
        }

        if (somTocadoNestaAbertura &&
            mostrarDiagnostico)
        {
            Debug.Log(
                "LogiVery: abertura detectada em " +
                gameObject.name +
                " | Som: " + tipoSom,
                this
            );
        }
    }

    // Opcional: permite disparar pelo OnClick
    // ou por um Animation Event.
    public void ReproduzirSomAgora()
    {
        LogiVeryEfeitosSonoros sons =
            LogiVeryEfeitosSonoros.Instancia;

        if (sons == null)
        {
            Debug.LogWarning(
                "Gerenciador de efeitos nao encontrado.",
                this
            );

            return;
        }

        if (tipoSom == TipoSom.PopUp)
        {
            sons.TocarPopUps();
            somTocadoNestaAbertura = true;
        }
        else if (tipoSom == TipoSom.Vitoria)
        {
            sons.TocarComemoracaoVitoria();
            somTocadoNestaAbertura = true;
        }
    }

    private void OnDisable()
    {
        somTocadoNestaAbertura = false;
    }
}
