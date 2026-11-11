using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface: HUD (pontos, vidas, onda), avisos na tela e os painéis
/// de Menu, Pausa, Game Over e Vitória.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public GameObject hud;
    public Text textoPontos;
    public Text textoVidas;
    public Text textoOnda;
    public Text textoPowerUp;
    public Text textoAviso;

    [Header("Painéis")]
    public GameObject painelMenu;
    public GameObject painelPausa;
    public GameObject painelGameOver;
    public GameObject painelVitoria;

    [Header("Textos dos painéis")]
    public Text textoRecordeMenu;
    public Text textoResultadoGameOver;
    public Text textoResultadoVitoria;

    Coroutine avisoAtual;

    void Awake()
    {
        textoAviso.text = "";
    }

    void Update()
    {
        // Mostra quanto tempo de tiro triplo ainda resta.
        PlayerController jogador = GameManager.Instancia.jogador;
        bool mostrar = jogador != null && jogador.gameObject.activeSelf && jogador.TiroTriploAtivo;
        textoPowerUp.text = mostrar ? "TIRO TRIPLO: " + Mathf.CeilToInt(jogador.TempoRestanteTiroTriplo) + "s" : "";
    }

    public void MostrarEstado(EstadoJogo estado)
    {
        GameManager gm = GameManager.Instancia;

        painelMenu.SetActive(estado == EstadoJogo.Menu);
        painelPausa.SetActive(estado == EstadoJogo.Pausado);
        painelGameOver.SetActive(estado == EstadoJogo.GameOver);
        painelVitoria.SetActive(estado == EstadoJogo.Vitoria);
        hud.SetActive(estado != EstadoJogo.Menu);

        textoRecordeMenu.text = "RECORDE: " + gm.Recorde;

        string resultado = "PONTOS: " + gm.Pontos + "\nRECORDE: " + gm.Recorde;
        if (gm.Pontos > 0 && gm.Pontos >= gm.Recorde) resultado += "\n\nNOVO RECORDE!";
        textoResultadoGameOver.text = resultado;
        textoResultadoVitoria.text = resultado;

        if (estado != EstadoJogo.Jogando && estado != EstadoJogo.Pausado) textoAviso.text = "";
    }

    public void AtualizarHUD()
    {
        GameManager gm = GameManager.Instancia;
        textoPontos.text = "PONTOS: " + gm.Pontos;
        textoVidas.text = "VIDAS: " + gm.Vidas;
        textoOnda.text = gm.spawner.OndaAtual > 0 ? "ONDA " + gm.spawner.OndaAtual + "/" + gm.spawner.TotalOndas : "";
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
