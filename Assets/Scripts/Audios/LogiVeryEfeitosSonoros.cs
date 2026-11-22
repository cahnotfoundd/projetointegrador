
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LogiVeryEfeitosSonoros : MonoBehaviour
{
    public static LogiVeryEfeitosSonoros Instancia
    {
        get;
        private set;
    }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup grupoSFX;

    [Header("Sons da preparacao")]
    [SerializeField] private AudioClip papeis;
    [SerializeField] private AudioClip embalagem;
    [SerializeField] private AudioClip botoes;
    [SerializeField] private AudioClip desenhoRota;
    [SerializeField] private AudioClip etiqueta;

    [Header("Sons da interface")]
    [SerializeField] private AudioClip popUps;
    [SerializeField] private AudioClip notificacoes;
    [SerializeField] private AudioClip cronometro;

    [Header("Som do semaforo")]
    [SerializeField] private AudioClip luzSemaforo;

    [Header("Sons da entrega")]
    [SerializeField] private AudioClip animacaoCliente;
    [SerializeField] private AudioClip comemoracaoVitoria;

    [Header("Sons dos produtos")]
    [SerializeField] private AudioClip adesivo;
    [SerializeField] private AudioClip cadernoAgenda;
    [SerializeField] private AudioClip kitMarcadores;
    [SerializeField] private AudioClip postIt;
    [SerializeField] private AudioClip estojo;

    [Header("Configuracoes gerais")]
    [Range(0f, 1f)]
    [SerializeField] private float volumeEfeitos = 0.65f;

    [SerializeField]
    private bool somAutomaticoNosBotoes = true;

    private readonly Dictionary<string, AudioSource> fontes =
        new Dictionary<string, AudioSource>();

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Debug.LogWarning(
                "Ja existe um gerenciador de efeitos sonoros.",
                this
            );

            enabled = false;
            return;
        }

        Instancia = this;
    }

    private void Start()
    {
        if (grupoSFX == null)
        {
            Debug.LogWarning(
                "LogiVery: atribua o grupo SFX " +
                "no gerenciador de efeitos.",
                this
            );
        }

        if (somAutomaticoNosBotoes)
            VincularBotoes();
    }

    private AudioSource ObterFonte(string caminho)
    {
        if (fontes.TryGetValue(
            caminho,
            out AudioSource existente
        ) && existente != null)
        {
            if (grupoSFX != null)
                existente.outputAudioMixerGroup = grupoSFX;

            return existente;
        }

        Transform objeto = transform.Find(caminho);

        if (objeto == null)
        {
            Debug.LogWarning(
                "Objeto de som nao encontrado: " + caminho,
                this
            );

            return null;
        }

        AudioSource fonte =
            objeto.GetComponent<AudioSource>();

        if (fonte == null)
        {
            fonte = objeto.gameObject
                .AddComponent<AudioSource>();
        }

        fonte.playOnAwake = false;
        fonte.spatialBlend = 0f;
        fonte.loop = false;

        if (grupoSFX != null)
            fonte.outputAudioMixerGroup = grupoSFX;

        fontes[caminho] = fonte;

        return fonte;
    }

    private void Tocar(string caminho, AudioClip arquivo)
    {
        AudioSource fonte = ObterFonte(caminho);

        if (fonte == null)
            return;

        AudioClip som = arquivo != null
            ? arquivo
            : fonte.clip;

        if (som == null)
            return;

        fonte.PlayOneShot(som, volumeEfeitos);
    }

    private void IniciarLoop(
        string caminho,
        AudioClip arquivo)
    {
        AudioSource fonte = ObterFonte(caminho);

        if (fonte == null)
            return;

        AudioClip som = arquivo != null
            ? arquivo
            : fonte.clip;

        if (som == null)
            return;

        if (fonte.isPlaying &&
            fonte.loop &&
            fonte.clip == som)
        {
            return;
        }

        fonte.Stop();
        fonte.clip = som;
        fonte.loop = true;
        fonte.volume = volumeEfeitos;
        fonte.Play();
    }

    private void PararLoop(string caminho)
    {
        AudioSource fonte = ObterFonte(caminho);

        if (fonte == null)
            return;

        fonte.Stop();
        fonte.loop = false;
        fonte.volume = 1f;
    }

    // BOTOES

    public void VincularBotoes()
    {
        // Unity 6.4: usa a sobrecarga atual,
        // sem FindObjectsSortMode.
        Button[] todosBotoes =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include
            );

        foreach (Button botao in todosBotoes)
        {
            if (botao == null)
                continue;

            botao.onClick.RemoveListener(TocarBotoes);
            botao.onClick.AddListener(TocarBotoes);
        }
    }

    public void TocarBotoes()
    {
        Tocar("Botoes", botoes);
    }

    // PREPARACAO

    public void TocarPapeis()
    {
        Tocar("Papeis", papeis);
    }

    public void TocarEmbalagem()
    {
        Tocar("Embalagem", embalagem);
    }

    public void TocarEtiqueta()
    {
        Tocar("Etiqueta", etiqueta);
    }

    // DESENHO DE ROTAS

    public void TocarDesenhoRota()
    {
        Tocar("DesenhoRota", desenhoRota);
    }

    public void IniciarSomDesenhoRota()
    {
        IniciarLoop("DesenhoRota", desenhoRota);
    }

    public void PararSomDesenhoRota()
    {
        PararLoop("DesenhoRota");
    }

    // INTERFACE

    public void TocarPopUps()
    {
        Tocar("PopUps", popUps);
    }

    public void TocarNotificacoes()
    {
        Tocar("Notificacoes", notificacoes);
    }

    public void TocarCronometro()
    {
        Tocar("Cronometro", cronometro);
    }

    public void IniciarSomCronometro()
    {
        IniciarLoop("Cronometro", cronometro);
    }

    public void PararSomCronometro()
    {
        PararLoop("Cronometro");
    }

    // ENTREGA

    public void TocarAnimacaoCliente()
    {
        Tocar("AnimacaoCliente", animacaoCliente);
    }

    public void TocarComemoracaoVitoria()
    {
        Tocar(
            "ComemoracaoVitoria",
            comemoracaoVitoria
        );
    }

    // PRODUTOS

    public void TocarAdesivo()
    {
        Tocar("Produtos/Adesivo", adesivo);
    }

    public void TocarCadernoAgenda()
    {
        Tocar(
            "Produtos/CadernoAgenda",
            cadernoAgenda
        );
    }

    public void TocarKitMarcadores()
    {
        Tocar(
            "Produtos/KitMarcadores",
            kitMarcadores
        );
    }

    public void TocarPostIt()
    {
        Tocar("Produtos/PostIt", postIt);
    }

    public void TocarEstojo()
    {
        Tocar("Produtos/Estojo", estojo);
    }

    // SEMAFORO

    public void TocarLuzSemaforo()
    {
        Tocar("Semaforo", luzSemaforo);
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
}
