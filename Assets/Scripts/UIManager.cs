using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface: HUD (pontos, vidas, onda ou cronômetro), avisos na tela e os painéis
/// de Menu (com escolha de modo), Pausa, Game Over e Vitória.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public GameObject hud;
    public Text textoPontos;
    public Text textoVidas;
    public Text textoOndaOuTempo;   // "ONDA 2/5" no modo Ondas, "TEMPO 01:23" no Infinito
    public Text textoPowerUp;
    public Text textoAviso;

    [Header("Painéis")]
    public GameObject painelMenu;
    public GameObject painelPausa;
    public GameObject painelGameOver;
    public GameObject painelVitoria;

    [Header("Menu")]
    public Text textoModoOndas;
    public Text textoModoInfinito;
    public Text textoDescricaoModo;
    public Text textoRecordeMenu;

    [Header("Resultados")]
    public Text textoResultadoGameOver;
    public Text textoResultadoVitoria;

    static readonly Color COR_SELECIONADO = new Color(1f, 0.9f, 0.3f);
    static readonly Color COR_NAO_SELECIONADO = new Color(0.55f, 0.55f, 0.65f);

    Coroutine avisoAtual;

    void Awake()
    {
        textoAviso.text = "";
    }

    void Update()
    {
        GameManager gm = GameManager.Instancia;

        // Cronômetro do modo infinito (atualiza todo frame).
        if (GameManager.Modo == ModoJogo.Infinito && gm.Estado != EstadoJogo.Menu)
            textoOndaOuTempo.text = "TEMPO " + GameManager.FormatarTempo(gm.TempoDePartida);

        // Tempo restante dos poderes ativos.
        PlayerController jogador = gm.jogador;
        string poderes = "";
        if (jogador != null && jogador.gameObject.activeSelf)
        {
            if (jogador.TiroTriploAtivo) poderes += "TIRO TRIPLO: " + Mathf.CeilToInt(jogador.TempoRestanteTiroTriplo) + "s";
            if (jogador.EscudoAtivo) poderes += (poderes.Length > 0 ? "\n" : "") + "ESCUDO: " + Mathf.CeilToInt(jogador.TempoRestanteEscudo) + "s";
        }
        textoPowerUp.text = poderes;
    }

    public void MostrarEstado(EstadoJogo estado)
    {
        painelMenu.SetActive(estado == EstadoJogo.Menu);
        painelPausa.SetActive(estado == EstadoJogo.Pausado);
        painelGameOver.SetActive(estado == EstadoJogo.GameOver);
        painelVitoria.SetActive(estado == EstadoJogo.Vitoria);
        hud.SetActive(estado != EstadoJogo.Menu);

        if (estado == EstadoJogo.Menu) AtualizarMenu();
        if (estado == EstadoJogo.GameOver || estado == EstadoJogo.Vitoria) AtualizarResultado();
        if (estado != EstadoJogo.Jogando && estado != EstadoJogo.Pausado) textoAviso.text = "";
    }

    /// <summary>Destaca o modo selecionado e mostra os recordes no menu.</summary>
    public void AtualizarMenu()
    {
        bool ondas = GameManager.Instancia.ModoSelecionado == ModoJogo.Ondas;

        textoModoOndas.text = ondas ? "> MODO ONDAS <" : "MODO ONDAS";
        textoModoOndas.color = ondas ? COR_SELECIONADO : COR_NAO_SELECIONADO;
        textoModoInfinito.text = !ondas ? "> MODO INFINITO <" : "MODO INFINITO";
        textoModoInfinito.color = !ondas ? COR_SELECIONADO : COR_NAO_SELECIONADO;

        textoDescricaoModo.text = ondas
            ? "Vença as 5 ondas de inimigos!"
            : "Inimigos sem parar. Sobreviva o máximo que puder!";

        textoRecordeMenu.text =
            "RECORDE ONDAS: " + GameManager.Recorde(ModoJogo.Ondas) +
            "\nRECORDE INFINITO: " + GameManager.Recorde(ModoJogo.Infinito) +
            "  (" + GameManager.FormatarTempo(GameManager.MelhorTempo) + ")";
    }

    void AtualizarResultado()
    {
        GameManager gm = GameManager.Instancia;
        string resultado = "PONTOS: " + gm.Pontos;
        if (GameManager.Modo == ModoJogo.Infinito)
            resultado += "\nTEMPO: " + GameManager.FormatarTempo(gm.TempoDePartida);
        resultado += "\nRECORDE: " + GameManager.Recorde(GameManager.Modo);
        if (gm.NovoRecorde) resultado += "\n\nNOVO RECORDE!";

        textoResultadoGameOver.text = resultado;
        textoResultadoVitoria.text = resultado;
    }

    public void AtualizarHUD()
    {
        GameManager gm = GameManager.Instancia;
        textoPontos.text = "PONTOS: " + gm.Pontos;
        textoVidas.text = "VIDAS: " + gm.Vidas;

        if (GameManager.Modo == ModoJogo.Ondas)
            textoOndaOuTempo.text = gm.spawner.OndaAtual > 0 ? "ONDA " + gm.spawner.OndaAtual + "/" + gm.spawner.TotalOndas : "";
        else
            textoOndaOuTempo.text = "TEMPO " + GameManager.FormatarTempo(gm.TempoDePartida);
    }

    public void MostrarAviso(string mensagem)
    {
        if (avisoAtual != null) StopCoroutine(avisoAtual);
        avisoAtual = StartCoroutine(Aviso(mensagem));
    }

    IEnumerator Aviso(string mensagem)
    {
        textoAviso.text = mensagem;
        Color c = textoAviso.color;
        float duracao = 1.8f;

        for (float t = 0f; t < duracao; t += Time.deltaTime)
        {
            c.a = t < duracao * 0.6f ? 1f : Mathf.InverseLerp(duracao, duracao * 0.6f, t);
            textoAviso.color = c;
            yield return null;
        }

        textoAviso.text = "";
        c.a = 1f;
        textoAviso.color = c;
    }
}
