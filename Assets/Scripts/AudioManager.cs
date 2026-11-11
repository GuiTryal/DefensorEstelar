using System.Collections.Generic;
using UnityEngine;

public enum Som { Tiro, TiroInimigo, Acerto, Explosao, Dano, PowerUp, GameOver, Vitoria }

/// <summary>
/// Gera e toca os efeitos sonoros. Os sons são criados por código
/// (ondas quadradas e ruído, estilo videogame antigo), sem arquivos de áudio.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    static AudioManager instancia;

    const int TAXA = 44100;

    AudioSource fonte;
    readonly Dictionary<Som, AudioClip> sons = new Dictionary<Som, AudioClip>();
    readonly System.Random aleatorio = new System.Random(42);

    void Awake()
    {
        instancia = this;
        fonte = GetComponent<AudioSource>();

        sons[Som.Tiro]        = Gerar(0.08f, t => Mathf.Lerp(1200f, 500f, t), false, 0.25f);
        sons[Som.TiroInimigo] = Gerar(0.12f, t => Mathf.Lerp(500f, 250f, t), false, 0.2f);
        sons[Som.Acerto]      = Gerar(0.05f, t => 300f, true, 0.25f);
        sons[Som.Explosao]    = Gerar(0.35f, t => 0f, true, 0.4f);
        sons[Som.Dano]        = Gerar(0.3f, t => Mathf.Lerp(220f, 60f, t), false, 0.35f);
        sons[Som.PowerUp]     = Gerar(0.3f, t => t < 0.33f ? 520f : (t < 0.66f ? 660f : 880f), false, 0.25f);
        sons[Som.GameOver]    = Gerar(1.0f, t => t < 0.33f ? 330f : (t < 0.66f ? 262f : 196f), false, 0.3f);
        sons[Som.Vitoria]     = Gerar(0.9f, t => t < 0.25f ? 523f : (t < 0.5f ? 659f : (t < 0.75f ? 784f : 1046f)), false, 0.3f);
    }

    public static void Tocar(Som som, float volume = 1f)
    {
        if (instancia == null) return;
        instancia.fonte.PlayOneShot(instancia.sons[som], volume);
    }

    /// <param name="duracao">Duração em segundos.</param>
    /// <param name="frequencia">Frequência (Hz) em função do progresso t (0 a 1).</param>
    /// <param name="ruido">true = ruído (explosões), false = onda quadrada.</param>
    AudioClip Gerar(float duracao, System.Func<float, float> frequencia, bool ruido, float volume)
    {
        int total = Mathf.CeilToInt(TAXA * duracao);
        float[] amostras = new float[total];
        float fase = 0f;

        for (int i = 0; i < total; i++)
        {
            float t = (float)i / total;
            float envelope = 1f - t; // volume vai caindo até zero
            float valor;

            if (ruido)
            {
                valor = (float)(aleatorio.NextDouble() * 2.0 - 1.0);
            }
            else
            {
                fase += 2f * Mathf.PI * frequencia(t) / TAXA;
                valor = Mathf.Sin(fase) >= 0f ? 1f : -1f;
            }
            amostras[i] = valor * envelope * volume;
        }

        AudioClip clip = AudioClip.Create("som", total, 1, TAXA, false);
        clip.SetData(amostras, 0);
        return clip;
    }
}
