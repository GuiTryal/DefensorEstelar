using UnityEngine;

/// <summary>
/// Toca a música de fundo: uma no menu e outra durante a partida, sempre baixinho.
/// Troca de uma para a outra com fade (usa duas fontes de áudio) e abaixa o volume na pausa.
/// Sobrevive à troca de cena (DontDestroyOnLoad), então a música não reinicia à toa.
/// </summary>
public class MusicManager : MonoBehaviour
{
    static MusicManager instancia;

    [Range(0f, 1f)] public float volumeMenu = 0.2f;
    [Range(0f, 1f)] public float volumeJogo = 0.16f;
    [Tooltip("Quanto o volume cai durante a pausa (0.4 = 40% do normal).")]
    [Range(0f, 1f)] public float fatorPausa = 0.4f;
    public float velocidadeFade = 0.8f; // volume por segundo

    // As músicas são geradas uma única vez e reaproveitadas.
    static AudioClip musicaMenu, musicaJogo;

    AudioSource fonteA, fonteB, atual;
    float volumeAlvo;
    bool abafado;

    void Awake()
    {
        // Se já existe um MusicManager (vindo da cena anterior), este é descartado.
        if (instancia != null) { Destroy(gameObject); return; }
        instancia = this;
        DontDestroyOnLoad(gameObject);

        fonteA = CriarFonte();
        fonteB = CriarFonte();
        atual = fonteA;

        if (musicaMenu == null) musicaMenu = GeradorMusica.Menu();
        if (musicaJogo == null) musicaJogo = GeradorMusica.Jogo();
    }

    AudioSource CriarFonte()
    {
        AudioSource f = gameObject.AddComponent<AudioSource>();
        f.loop = true;
        f.playOnAwake = false;
        f.volume = 0f;
        return f;
    }

    public static void TocarMenu() { if (instancia) instancia.Tocar(musicaMenu, instancia.volumeMenu); }
    public static void TocarJogo() { if (instancia) instancia.Tocar(musicaJogo, instancia.volumeJogo); }
    public static void Parar() { if (instancia) instancia.volumeAlvo = 0f; }
    public static void Abafar(bool sim) { if (instancia) instancia.abafado = sim; }

    void Tocar(AudioClip clip, float volume)
    {
        volumeAlvo = volume;
        abafado = false;
        if (atual.clip == clip && atual.isPlaying) return;

        // A outra fonte começa a nova música do zero; a atual vai sumindo.
        AudioSource nova = atual == fonteA ? fonteB : fonteA;
        nova.clip = clip;
        nova.volume = 0f;
        nova.Play();
        atual = nova;
    }

    void Update()
    {
        // unscaledDeltaTime: continua funcionando com o jogo pausado (timeScale = 0).
        float passo = velocidadeFade * Time.unscaledDeltaTime;
        float alvo = volumeAlvo * (abafado ? fatorPausa : 1f);

        atual.volume = Mathf.MoveTowards(atual.volume, alvo, passo);

        AudioSource outra = atual == fonteA ? fonteB : fonteA;
        if (outra.isPlaying)
        {
            outra.volume = Mathf.MoveTowards(outra.volume, 0f, passo);
            if (outra.volume <= 0f) outra.Stop();
        }
    }
}
