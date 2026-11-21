
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RoteadorAudioMixer : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Grupos")]
    [SerializeField] private AudioMixerGroup grupoMusica;
    [SerializeField] private AudioMixerGroup grupoSFX;

    [Header("Fonte da Musica")]
    [SerializeField] private AudioSource fonteMusica;

    private readonly List<AudioSource> fontes =
        new List<AudioSource>();

    private void Awake()
    {
        if (fonteMusica == null)
        {
            Transform objeto =
                transform.Find("Musica");

            if (objeto != null)
            {
                fonteMusica =
                    objeto.GetComponent<AudioSource>();
            }
        }
    }

    private void Start()
    {
        if (audioMixer == null ||
            grupoMusica == null ||
            grupoSFX == null)
        {
            Debug.LogError(
                "Roteador de audio: faltam referencias!",
                this
            );

            enabled = false;
            return;
        }

        if (fonteMusica == null)
        {
            Debug.LogError(
                "Roteador: fonte de musica nao atribuida!",
                this
            );

            enabled = false;
            return;
        }

        LogiVeryConfiguracoesVolume
            .AplicarVolumesSalvos(audioMixer);

        AtualizarFontes();
    }

    private void LateUpdate()
    {
        // Inclui fontes criadas durante o jogo.
        AtualizarFontes();
    }

    private void AtualizarFontes()
    {
        fontes.Clear();

        GetComponentsInChildren<AudioSource>(
            true,
            fontes
        );

        foreach (AudioSource fonte in fontes)
        {
            if (fonte == null)
                continue;

            AudioMixerGroup grupoDestino =
                fonte == fonteMusica
                    ? grupoMusica
                    : grupoSFX;

            if (fonte.outputAudioMixerGroup != grupoDestino)
            {
                fonte.outputAudioMixerGroup =
                    grupoDestino;
            }
        }
    }
}