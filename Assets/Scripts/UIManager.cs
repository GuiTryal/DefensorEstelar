using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Indicador de um poder ativo: ícone + barra de tempo + segundos restantes.
/// </summary>
[System.Serializable]
public class IndicadorPoder
{
    public GameObject raiz;
    public RectTransform preenchimento;  // a barra "esvazia" mudando a âncora direita
    public Text segundos;

    public void Mostrar(bool ativo, float restante, float total)
    {
        raiz.SetActive(ativo);
        if (!ativo) return;
        float fracao = total > 0f ? Mathf.Clamp01(restante / total) : 0f;
        preenchimento.anchorMax = new Vector2(fracao, 1f);
        segundos.text = Mathf.CeilToInt(restante) + "s";
    }
}

/// <summary>
/// Interface: HUD (pontos, corações de vida, onda ou cronômetro, poderes ativos),
/// avisos na tela e os painéis de Menu, Pausa, Game Over e Vitória.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public GameObject hud;
    public Text textoPontos;
    public Image[] coracoes;
    public Text textoRotuloCentro;   // "ONDA" ou "TEMPO"
    public Text textoValorCentro;    // "2/5" ou "01:23"
    public IndicadorPoder indicadorTiroTriplo;
    public IndicadorPoder indicadorEscudo;
    public Text textoAviso;

    [Header("Painéis")]
    public GameObject painelMenu;
    public GameObject painelPausa;
    public GameObject painelGameOver;
    public GameObject painelVitoria;

    [Header("Menu")]
    public CartaoModo cartaoOndas;
    public CartaoModo cartaoInfinito;

    [Header("Resultados")]
    public Text textoResultadoGameOver;
    public Text textoResultadoVitoria;

    static readonly Color CORACAO_CHEIO = new Color(1f, 0.3f, 0.4f);
    static readonly Color CORACAO_VAZIO = new Color(0.22f, 0.22f, 0.32f, 0.9f);

    Coroutine avisoAtual;
    float pontosMostrados;   // o número na tela "corre" até o valor real
    int vidasAnteriores = -1;

    void Awake()
    {
        textoAviso.text = "";
    }

    void Update()
    {
        GameManager gm = GameManager.Instancia;

        // Pontos sobem de forma animada.
        pontosMostrados = Mathf.MoveTowards(pontosMostrados, gm.Pontos, Mathf.Max(200f, Mathf.Abs(gm.Pontos - pontosMostrados) * 8f) * Time.unscaledDeltaTime);
        textoPontos.text = Mathf.RoundToInt(pontosMostrados).ToString("N0");

        // Corações voltam ao tamanho normal depois do "pulo".
        foreach (Image c in coracoes)
            c.rectTransform.localScale = Vector3.Lerp(c.rectTransform.localScale, Vector3.one, 10f * Time.unscaledDeltaTime);

        // Cronômetro do modo infinito (atualiza todo frame).
        if (GameManager.Modo == ModoJogo.Infinito && gm.Estado != EstadoJogo.Menu)
            textoValorCentro.text = GameManager.FormatarTempo(gm.TempoDePartida);

        // Barras dos poderes ativos.
        PlayerController j = gm.jogador;
        bool vivo = j != null && j.gameObject.activeSelf;
        indicadorTiroTriplo.Mostrar(vivo && j.TiroTriploAtivo, vivo ? j.TempoRestanteTiroTriplo : 0f, vivo ? j.DuracaoTiroTriplo : 1f);
        indicadorEscudo.Mostrar(vivo && j.EscudoAtivo, vivo ? j.TempoRestanteEscudo : 0f, vivo ? j.DuracaoEscudo : 1f);
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
        cartaoOndas.Destacar(ondas);
        cartaoInfinito.Destacar(!ondas);

        cartaoOndas.recorde.text = "RECORDE: " + GameManager.Recorde(ModoJogo.Ondas);
        cartaoInfinito.recorde.text = "RECORDE: " + GameManager.Recorde(ModoJogo.Infinito) +
                                      "  -  " + GameManager.FormatarTempo(GameManager.MelhorTempo);
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

        // Corações: cheios até o número de vidas, vazios até o máximo.
        for (int i = 0; i < coracoes.Length; i++)
        {
            coracoes[i].gameObject.SetActive(i < gm.vidasMaximas);
            coracoes[i].color = i < gm.Vidas ? CORACAO_CHEIO : CORACAO_VAZIO;
        }

        // O coração que mudou dá um "pulo" (feedback de dano ou de vida extra).
        if (vidasAnteriores >= 0 && gm.Vidas != vidasAnteriores)
        {
            int indice = Mathf.Clamp(Mathf.Min(gm.Vidas, vidasAnteriores), 0, coracoes.Length - 1);
            if (gm.Vidas > vidasAnteriores) indice = Mathf.Clamp(gm.Vidas - 1, 0, coracoes.Length - 1);
            coracoes[indice].rectTransform.localScale = Vector3.one * 1.7f;
        }
        vidasAnteriores = gm.Vidas;

        if (GameManager.Modo == ModoJogo.Ondas)
        {
            textoRotuloCentro.text = "ONDA";
            textoValorCentro.text = Mathf.Max(1, gm.spawner.OndaAtual) + "/" + gm.spawner.TotalOndas;
        }
        else
        {
            textoRotuloCentro.text = "TEMPO";
            textoValorCentro.text = GameManager.FormatarTempo(gm.TempoDePartida);
        }
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
            // Entra um pouco maior e "assenta" no tamanho normal.
            textoAviso.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, Mathf.Clamp01(t * 6f));
            yield return null;
        }

        textoAviso.text = "";
        c.a = 1f;
        textoAviso.color = c;
    }
}
