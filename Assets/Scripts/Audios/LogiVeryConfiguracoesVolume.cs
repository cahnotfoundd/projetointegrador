
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LogiVeryConfiguracoesVolume : MonoBehaviour
{
    private const string ChaveMusica =
        "LogiVery_VolumeMusica";

    private const string ChaveSFX =
        "LogiVery_VolumeSFX";

    private const string ParametroMusica =
        "VolumeMusica";

    private const string ParametroSFX =
        "VolumeSFX";

    private const int VolumeInicialMusica = 70;
    private const int VolumeInicialSFX = 80;
    private const int Passo = 10;

    private static event Action AoAlterarVolume;

    [Header("Audio Mixer")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Header("Grupos do Mixer - opcionais")]
    [SerializeField]
    private AudioMixerGroup grupoMusica;

    [SerializeField]
    private AudioMixerGroup grupoSFX;

    [Header("Fontes de audio do Menu Principal")]
    [SerializeField]
    private AudioSource musicaMenu;

    [SerializeField]
    private AudioSource sfxMenu;

    [Header("Reproducao automatica do menu")]
    [SerializeField]
    private bool iniciarMusicaMenu = true;

    [SerializeField]
    private bool somAutomaticoBotoesVolume = true;

    [Header("Barra de Musica")]
    [SerializeField]
    private Image[] blocosMusica = new Image[10];

    [SerializeField]
    private TMP_Text textoPorcentagemMusica;

    [SerializeField]
    private Button botaoMenosMusica;

    [SerializeField]
    private Button botaoMaisMusica;

    [Header("Barra de SFX")]
    [SerializeField]
    private Image[] blocosSFX = new Image[10];

    [SerializeField]
    private TMP_Text textoPorcentagemSFX;

    [SerializeField]
    private Button botaoMenosSFX;

    [SerializeField]
    private Button botaoMaisSFX;

    [Header("Sprite opcional dos blocos")]
    [SerializeField]
    private Sprite spriteBlocoAzul;

    [Header("Transparencia dos blocos vazios")]
    [Range(0f, 1f)]
    [SerializeField]
    private float opacidadeBlocoVazio = 0.30f;

    [Header("Diagnostico")]
    [SerializeField]
    private bool mostrarDiagnostico = true;

    private void Awake()
    {
        RegistrarBotoes();
    }

    private void OnEnable()
    {
        AoAlterarVolume += AtualizarInterface;

        // Apenas atualiza o visual.
        // O Mixer sera configurado no Start.
        AtualizarInterface();
    }

    private void Start()
    {
        ConfigurarAudioMenu();

        if (audioMixer == null)
        {
            Debug.LogError(
                "LogiVery: Audio Mixer nao atribuido!",
                this
            );

            return;
        }

        AplicarVolumesSalvos(audioMixer);

        AtualizarInterface();

        if (mostrarDiagnostico)
        {
            DiagnosticarAudio();
        }
    }

    private void OnDisable()
    {
        AoAlterarVolume -= AtualizarInterface;
    }

    private void OnDestroy()
    {
        RemoverEventosBotoes();
    }

    private void RegistrarBotoes()
    {
        if (botaoMenosMusica != null)
        {
            botaoMenosMusica.onClick.AddListener(
                DiminuirMusica
            );

            if (somAutomaticoBotoesVolume)
            {
                botaoMenosMusica.onClick.AddListener(
                    TocarClique
                );
            }
        }

        if (botaoMaisMusica != null)
        {
            botaoMaisMusica.onClick.AddListener(
                AumentarMusica
            );

            if (somAutomaticoBotoesVolume)
            {
                botaoMaisMusica.onClick.AddListener(
                    TocarClique
                );
            }
        }

        if (botaoMenosSFX != null)
        {
            botaoMenosSFX.onClick.AddListener(
                DiminuirSFX
            );

            if (somAutomaticoBotoesVolume)
            {
                botaoMenosSFX.onClick.AddListener(
                    TocarClique
                );
            }
        }

        if (botaoMaisSFX != null)
        {
            botaoMaisSFX.onClick.AddListener(
                AumentarSFX
            );

            if (somAutomaticoBotoesVolume)
            {
                botaoMaisSFX.onClick.AddListener(
                    TocarClique
                );
            }
        }
    }

    private void RemoverEventosBotoes()
    {
        if (botaoMenosMusica != null)
        {
            botaoMenosMusica.onClick.RemoveListener(
                DiminuirMusica
            );

            botaoMenosMusica.onClick.RemoveListener(
                TocarClique
            );
        }

        if (botaoMaisMusica != null)
        {
            botaoMaisMusica.onClick.RemoveListener(
                AumentarMusica
            );

            botaoMaisMusica.onClick.RemoveListener(
                TocarClique
            );
        }

        if (botaoMenosSFX != null)
        {
            botaoMenosSFX.onClick.RemoveListener(
                DiminuirSFX
            );

            botaoMenosSFX.onClick.RemoveListener(
                TocarClique
            );
        }

        if (botaoMaisSFX != null)
        {
            botaoMaisSFX.onClick.RemoveListener(
                AumentarSFX
            );

            botaoMaisSFX.onClick.RemoveListener(
                TocarClique
            );
        }
    }

    private void ConfigurarAudioMenu()
    {
        if (musicaMenu != null)
        {
            if (grupoMusica != null)
            {
                musicaMenu.outputAudioMixerGroup =
                    grupoMusica;
            }

            musicaMenu.loop = true;
            musicaMenu.spatialBlend = 0f;

            if (iniciarMusicaMenu &&
                musicaMenu.clip != null &&
                !musicaMenu.isPlaying)
            {
                musicaMenu.Play();
            }
        }

        if (sfxMenu != null)
        {
            if (grupoSFX != null)
            {
                sfxMenu.outputAudioMixerGroup =
                    grupoSFX;
            }

            sfxMenu.spatialBlend = 0f;
            sfxMenu.playOnAwake = false;
            sfxMenu.loop = false;
        }
    }

    private void TocarClique()
    {
        if (sfxMenu == null)
            return;

        if (!sfxMenu.gameObject.activeInHierarchy)
            return;

        if (sfxMenu.clip == null)
        {
            Debug.LogWarning(
                "SFXMenu nao possui AudioClip.",
                this
            );

            return;
        }

        sfxMenu.PlayOneShot(sfxMenu.clip);
    }

    public void AumentarMusica()
    {
        AlterarVolume(
            ChaveMusica,
            VolumeInicialMusica,
            Passo
        );
    }

    public void DiminuirMusica()
    {
        AlterarVolume(
            ChaveMusica,
            VolumeInicialMusica,
            -Passo
        );
    }

    public void AumentarSFX()
    {
        AlterarVolume(
            ChaveSFX,
            VolumeInicialSFX,
            Passo
        );
    }

    public void DiminuirSFX()
    {
        AlterarVolume(
            ChaveSFX,
            VolumeInicialSFX,
            -Passo
        );
    }

    private void AlterarVolume(
        string chave,
        int volumePadrao,
        int alteracao)
    {
        int atual = Mathf.Clamp(
            PlayerPrefs.GetInt(
                chave,
                volumePadrao
            ),
            0,
            100
        );

        int novo = Mathf.Clamp(
            atual + alteracao,
            0,
            100
        );

        if (novo == atual)
            return;

        PlayerPrefs.SetInt(chave, novo);
        PlayerPrefs.Save();

        if (audioMixer != null)
        {
            AplicarVolumesSalvos(audioMixer);
        }

        AoAlterarVolume?.Invoke();
    }

    private void AtualizarInterface()
    {
        int musica = ObterVolumeMusica();
        int sfx = ObterVolumeSFX();

        if (textoPorcentagemMusica != null)
        {
            textoPorcentagemMusica.text =
                musica + "%";
        }

        if (textoPorcentagemSFX != null)
        {
            textoPorcentagemSFX.text =
                sfx + "%";
        }

        AtualizarBarra(
            blocosMusica,
            musica
        );

        AtualizarBarra(
            blocosSFX,
            sfx
        );

        if (botaoMenosMusica != null)
        {
            botaoMenosMusica.interactable =
                musica > 0;
        }

        if (botaoMaisMusica != null)
        {
            botaoMaisMusica.interactable =
                musica < 100;
        }

        if (botaoMenosSFX != null)
        {
            botaoMenosSFX.interactable =
                sfx > 0;
        }

        if (botaoMaisSFX != null)
        {
            botaoMaisSFX.interactable =
                sfx < 100;
        }
    }

    private void AtualizarBarra(
        Image[] blocos,
        int volume)
    {
        if (blocos == null ||
            blocos.Length == 0)
        {
            return;
        }

        int quantidadeCheia = Mathf.RoundToInt(
            blocos.Length * volume / 100f
        );

        for (int i = 0; i < blocos.Length; i++)
        {
            Image bloco = blocos[i];

            if (bloco == null)
                continue;

            if (bloco.sprite == null &&
                spriteBlocoAzul != null)
            {
                bloco.sprite = spriteBlocoAzul;
            }

            Color cor = bloco.color;

            // Preserva a cor escolhida no Inspector.
            // Modifica somente a transparencia.
            cor.a = i < quantidadeCheia
                ? 1f
                : opacidadeBlocoVazio;

            bloco.color = cor;
        }
    }

    private static int ObterVolumeMusica()
    {
        return Mathf.Clamp(
            PlayerPrefs.GetInt(
                ChaveMusica,
                VolumeInicialMusica
            ),
            0,
            100
        );
    }

    private static int ObterVolumeSFX()
    {
        return Mathf.Clamp(
            PlayerPrefs.GetInt(
                ChaveSFX,
                VolumeInicialSFX
            ),
            0,
            100
        );
    }

    public static void AplicarVolumesSalvos(
        AudioMixer mixer)
    {
        if (mixer == null)
            return;

        int musica = ObterVolumeMusica();
        int sfx = ObterVolumeSFX();

        bool musicaAplicada = mixer.SetFloat(
            ParametroMusica,
            ConverterParaDecibeis(musica)
        );

        bool sfxAplicado = mixer.SetFloat(
            ParametroSFX,
            ConverterParaDecibeis(sfx)
        );

        if (!musicaAplicada)
        {
            Debug.LogError(
                "LogiVery: nao foi possivel aplicar " +
                "VolumeMusica. Verifique o parametro " +
                "exposto no Audio Mixer."
            );
        }

        if (!sfxAplicado)
        {
            Debug.LogError(
                "LogiVery: nao foi possivel aplicar " +
                "VolumeSFX. Verifique o parametro " +
                "exposto no Audio Mixer."
            );
        }
    }

    private static float ConverterParaDecibeis(
        int porcentagem)
    {
        if (porcentagem <= 0)
            return -80f;

        return 20f * Mathf.Log10(
            porcentagem / 100f
        );
    }

    // Use pelo menu de contexto do componente
    // durante o Play Mode para recuperar o audio.
    [ContextMenu("TESTE - Restaurar musica e SFX para 100%")]
    public void RestaurarVolumesParaTeste()
    {
        PlayerPrefs.SetInt(
            ChaveMusica,
            100
        );

        PlayerPrefs.SetInt(
            ChaveSFX,
            100
        );

        PlayerPrefs.Save();

        if (audioMixer != null)
        {
            AplicarVolumesSalvos(audioMixer);
        }

        AtualizarInterface();
        AoAlterarVolume?.Invoke();

        Debug.Log(
            "LogiVery: musica e SFX restaurados " +
            "para 100%.",
            this
        );
    }

    [ContextMenu("TESTE - Reproduzir som de clique")]
    public void TestarSomDeClique()
    {
        TocarClique();
    }

    [ContextMenu("TESTE - Iniciar musica do menu")]
    public void TestarMusicaMenu()
    {
        if (musicaMenu == null)
        {
            Debug.LogWarning(
                "MusicaMenu nao atribuida.",
                this
            );

            return;
        }

        if (musicaMenu.clip == null)
        {
            Debug.LogWarning(
                "MusicaMenu sem AudioClip.",
                this
            );

            return;
        }

        if (!musicaMenu.isPlaying)
        {
            musicaMenu.Play();
        }
    }

    [ContextMenu("TESTE - Diagnosticar audio")]
    public void DiagnosticarAudio()
    {
        Debug.Log(
            "LogiVery AUDIO | Mixer: " +
            (audioMixer != null
                ? audioMixer.name
                : "NAO ATRIBUIDO") +
            " | Musica salva: " +
            ObterVolumeMusica() + "%" +
            " | SFX salvo: " +
            ObterVolumeSFX() + "%",
            this
        );

        if (audioMixer != null)
        {
            bool encontrouMusica =
                audioMixer.GetFloat(
                    ParametroMusica,
                    out float dbMusica
                );

            bool encontrouSFX =
                audioMixer.GetFloat(
                    ParametroSFX,
                    out float dbSFX
                );

            Debug.Log(
                "LogiVery AUDIO | Parametro Musica: " +
                encontrouMusica +
                " | dB: " + dbMusica +
                " | Parametro SFX: " +
                encontrouSFX +
                " | dB: " + dbSFX,
                this
            );
        }

        if (musicaMenu != null)
        {
            Debug.Log(
                "LogiVery AUDIO | MusicaMenu tocando: " +
                musicaMenu.isPlaying +
                " | Grupo: " +
                (musicaMenu.outputAudioMixerGroup != null
                    ? musicaMenu.outputAudioMixerGroup.name
                    : "NENHUM"),
                this
            );
        }

        if (sfxMenu != null)
        {
            Debug.Log(
                "LogiVery AUDIO | SFXMenu clip: " +
                (sfxMenu.clip != null
                    ? sfxMenu.clip.name
                    : "NENHUM") +
                " | Grupo: " +
                (sfxMenu.outputAudioMixerGroup != null
                    ? sfxMenu.outputAudioMixerGroup.name
                    : "NENHUM"),
                this
            );
        }
    }
}
