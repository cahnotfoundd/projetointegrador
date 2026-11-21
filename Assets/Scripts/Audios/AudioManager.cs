
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Tela de entrega")]
    [SerializeField] private GameObject painelEntrega;

    [Header("Chegada ao destino")]
    [SerializeField] private GameObject painelFimViagem;

    [Header("Pop-ups que devem tocar som")]
    [SerializeField] private GameObject[] paineisPopUp;

    [Header("Fontes de audio")]
    [SerializeField] private AudioSource musicaFundo;
    [SerializeField] private AudioSource somTransito;
    [SerializeField] private AudioSource fontePopUp;

    [Header("Arquivos de audio")]
    [SerializeField] private AudioClip arquivoMusica;
    [SerializeField] private AudioClip arquivoTransito;
    [SerializeField] private AudioClip arquivoPopUp;

    [Header("Volumes da musica")]
    [Range(0f, 1f)]
    [SerializeField]
    private float volumeMusicaNormal = 0.35f;

    [Range(0f, 1f)]
    [SerializeField]
    private float volumeMusicaEntrega = 0.18f;

    [Header("Volume do transito")]
    [Range(0f, 1f)]
    [SerializeField]
    private float volumeTransito = 0.28f;

    [Header("Volume dos pop-ups")]
    [Range(0f, 1f)]
    [SerializeField]
    private float volumePopUp = 0.75f;

    [Header("Transicao suave")]
    [Min(0.05f)]
    [SerializeField]
    private float tempoTransicao = 1.5f;

    [Header("Diagnostico")]
    [SerializeField]
    private bool mostrarLogs = true;

    private bool transitoEncerrado;

    private readonly HashSet<GameObject> popUpsVisiveis =
        new HashSet<GameObject>();

    private void Start()
    {
        if (musicaFundo == null ||
            somTransito == null ||
            painelEntrega == null)
        {
            Debug.LogError(
                "AudioManager: configure Painel Entrega, " +
                "Musica Fundo e Som Transito!",
                this
            );

            enabled = false;
            return;
        }

        if (arquivoMusica != null)
            musicaFundo.clip = arquivoMusica;

        if (arquivoTransito != null)
            somTransito.clip = arquivoTransito;

        ConfigurarFonteContinua(musicaFundo);
        ConfigurarFonteContinua(somTransito);

        PrepararFontePopUp();

        bool chegadaVisivel =
            PainelEstaVisivel(painelFimViagem);

        transitoEncerrado = chegadaVisivel;

        bool viagemAtiva =
            painelEntrega.activeInHierarchy &&
            !transitoEncerrado;

        musicaFundo.volume = viagemAtiva
            ? volumeMusicaEntrega
            : volumeMusicaNormal;

        somTransito.volume = viagemAtiva
            ? volumeTransito
            : 0f;

        if (musicaFundo.clip != null &&
            !musicaFundo.isPlaying)
        {
            musicaFundo.Play();
        }

        if (viagemAtiva &&
            somTransito.clip != null)
        {
            somTransito.Play();
        }
    }

    private void ConfigurarFonteContinua(
        AudioSource fonte)
    {
        if (fonte == null)
            return;

        fonte.playOnAwake = false;
        fonte.loop = true;
        fonte.spatialBlend = 0f;
        fonte.mute = false;
    }

    private void PrepararFontePopUp()
    {
        if (fontePopUp == null)
        {
            Transform objetoPopUp =
                transform.Find("PopUps");

            if (objetoPopUp == null)
            {
                GameObject novoObjeto =
                    new GameObject("PopUps");

                novoObjeto.transform.SetParent(
                    transform,
                    false
                );

                objetoPopUp = novoObjeto.transform;
            }

            fontePopUp =
                objetoPopUp.GetComponent<AudioSource>();

            if (fontePopUp == null)
            {
                fontePopUp =
                    objetoPopUp.gameObject
                    .AddComponent<AudioSource>();
            }
        }

        fontePopUp.playOnAwake = false;
        fontePopUp.loop = false;
        fontePopUp.spatialBlend = 0f;
        fontePopUp.mute = false;
        fontePopUp.volume = 1f;
    }

    // Verifica se o painel realmente esta visivel.
    // Funciona com SetActive, CanvasGroup
    // e paineis ocultados por escala zero.
    private bool PainelEstaVisivel(GameObject painel)
    {
        if (painel == null ||
            !painel.activeInHierarchy)
        {
            return false;
        }

        Transform atual = painel.transform;

        while (atual != null)
        {
            Vector3 escala = atual.localScale;

            if (Mathf.Abs(escala.x) < 0.001f ||
                Mathf.Abs(escala.y) < 0.001f)
            {
                return false;
            }

            Canvas canvas =
                atual.GetComponent<Canvas>();

            if (canvas != null && !canvas.enabled)
                return false;

            CanvasGroup[] grupos =
                atual.GetComponents<CanvasGroup>();

            bool ignorarGruposAcima = false;

            foreach (CanvasGroup grupo in grupos)
            {
                if (grupo == null || !grupo.enabled)
                    continue;

                if (grupo.alpha <= 0.05f)
                    return false;

                if (grupo.ignoreParentGroups)
                    ignorarGruposAcima = true;
            }

            if (ignorarGruposAcima)
                break;

            atual = atual.parent;
        }

        return true;
    }

    // Comando publico para outros scripts
    // que ja chamam EncerrarTransito().
    public void EncerrarTransito()
    {
        if (transitoEncerrado)
            return;

        transitoEncerrado = true;

        if (somTransito != null)
        {
            somTransito.Stop();
            somTransito.volume = 0f;
        }

        if (mostrarLogs)
        {
            Debug.Log(
                "LogiVery: transito PARADO " +
                "na chegada ao destino.",
                this
            );
        }
    }

    private void Update()
    {
        if (painelEntrega == null ||
            musicaFundo == null ||
            somTransito == null)
        {
            return;
        }

        bool chegadaVisivel =
            PainelEstaVisivel(painelFimViagem);

        bool painelEntregaAberto =
            painelEntrega.activeInHierarchy;

        // Prioridade absoluta:
        // chegou ao destino, para o transito.
        if (chegadaVisivel)
        {
            EncerrarTransito();
        }
        else if (!painelEntregaAberto)
        {
            // Libera o som para uma futura entrega.
            transitoEncerrado = false;
        }

        bool viagemAtiva =
            painelEntregaAberto &&
            !transitoEncerrado &&
            !chegadaVisivel;

        AtualizarMusica(viagemAtiva);
        AtualizarTransito(viagemAtiva);

        // Observa os pop-ups mesmo quando o
        // painel principal de entrega continua aberto.
        MonitorarPopUps();
    }

    private void AtualizarMusica(bool viagemAtiva)
    {
        float volumeDesejado = viagemAtiva
            ? volumeMusicaEntrega
            : volumeMusicaNormal;

        float duracao =
            Mathf.Max(0.05f, tempoTransicao);

        float velocidade =
            Mathf.Abs(
                volumeMusicaNormal -
                volumeMusicaEntrega
            ) / duracao;

        musicaFundo.volume = Mathf.MoveTowards(
            musicaFundo.volume,
            volumeDesejado,
            velocidade * Time.unscaledDeltaTime
        );

        if (musicaFundo.clip != null &&
            !musicaFundo.isPlaying)
        {
            musicaFundo.Play();
        }
    }

    private void AtualizarTransito(bool viagemAtiva)
    {
        // Quando a entrega termina, nao faz fade:
        // o transito para imediatamente.
        if (transitoEncerrado)
        {
            if (somTransito.isPlaying)
                somTransito.Stop();

            somTransito.volume = 0f;
            return;
        }

        if (viagemAtiva &&
            somTransito.clip != null &&
            !somTransito.isPlaying)
        {
            somTransito.volume = 0f;
            somTransito.Play();
        }

        float volumeDesejado = viagemAtiva
            ? volumeTransito
            : 0f;

        float duracao =
            Mathf.Max(0.05f, tempoTransicao);

        float velocidade =
            volumeTransito / duracao;

        somTransito.volume = Mathf.MoveTowards(
            somTransito.volume,
            volumeDesejado,
            velocidade * Time.unscaledDeltaTime
        );

        if (!viagemAtiva &&
            somTransito.isPlaying &&
            somTransito.volume <= 0.001f)
        {
            somTransito.Stop();
        }
    }

    private void MonitorarPopUps()
    {
        if (paineisPopUp == null)
            return;

        foreach (GameObject painel in paineisPopUp)
        {
            if (painel == null)
                continue;

            bool visivel =
                PainelEstaVisivel(painel);

            if (!visivel)
            {
                // Quando fechar, permite tocar
                // novamente na proxima abertura.
                popUpsVisiveis.Remove(painel);
                continue;
            }

            // Add retorna false se o painel
            // ja estava visivel anteriormente.
            if (popUpsVisiveis.Add(painel))
            {
                TocarSomPopUp(painel.name);
            }
        }
    }

    private void TocarSomPopUp(string nomePainel)
    {
        if (fontePopUp == null)
            PrepararFontePopUp();

        if (fontePopUp == null)
        {
            Debug.LogError(
                "AudioManager: fonte PopUps nao encontrada!",
                this
            );
            return;
        }

        AudioClip som = arquivoPopUp != null
            ? arquivoPopUp
            : fontePopUp.clip;

        if (som == null)
        {
            Debug.LogError(
                "AudioManager: atribua o Arquivo Pop Up!",
                this
            );
            return;
        }

        if (!fontePopUp.gameObject.activeInHierarchy)
        {
            fontePopUp.gameObject.SetActive(true);
        }

        if (!fontePopUp.enabled)
            fontePopUp.enabled = true;

        fontePopUp.mute = false;
        fontePopUp.loop = false;
        fontePopUp.spatialBlend = 0f;
        fontePopUp.volume = 1f;

        // Impede que sons antigos se acumulem.
        fontePopUp.Stop();

        fontePopUp.PlayOneShot(
            som,
            volumePopUp
        );

        if (mostrarLogs)
        {
            Debug.Log(
                "LogiVery: POPUP tocado ao abrir " +
                nomePainel,
                this
            );
        }
    }

    // Teste direto pelo menu do componente.
    [ContextMenu("TESTAR SOM POPUP")]
    private void TestarSomPopUp()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Entre no Play Mode antes do teste.",
                this
            );
            return;
        }

        TocarSomPopUp("TESTE MANUAL");
    }
}
