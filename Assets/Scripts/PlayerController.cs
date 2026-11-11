using UnityEngine;

/// <summary>
/// Controla a nave do jogador: lê o teclado, move usando física (Rigidbody2D),
/// atira e fica invencível por um tempo depois de levar dano.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float margemTela = 0.5f;

    [Header("Tiro")]
    public Bullet prefabTiro;
    public Transform pontoDeTiro;
    public float intervaloTiro = 0.18f;
    public float anguloTiroTriplo = 12f;

    [Header("Dano")]
    public float tempoInvencivel = 1.5f;
    public Explosion prefabExplosao;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Color corOriginal;

    Vector2 entrada;
    float proximoTiro;
    float fimInvencivel;
    float fimTiroTriplo;

    public bool Invencivel => Time.time < fimInvencivel;
    public bool TiroTriploAtivo => Time.time < fimTiroTriplo;
    public float TempoRestanteTiroTriplo => Mathf.Max(0f, fimTiroTriplo - Time.time);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        corOriginal = sr.color;
    }

    /// <summary>Coloca a nave na posição inicial (chamado ao começar a partida).</summary>
    public void Resetar()
    {
        transform.position = new Vector3(0f, -3.5f, 0f);
        rb.velocity = Vector2.zero;
        fimInvencivel = Time.time + tempoInvencivel;
        fimTiroTriplo = 0f;
    }

    void Update()
    {
        if (GameManager.Instancia.Estado != EstadoJogo.Jogando)
        {
            entrada = Vector2.zero;
            return;
        }

        // ENTRADA: setas ou WASD (eixos padrão do Input Manager).
        entrada = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (entrada.sqrMagnitude > 1f) entrada.Normalize(); // diagonal não fica mais rápida

        // TIRO: segurar Espaço (ou Z) atira continuamente, respeitando o intervalo.
        bool atirando = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z);
        if (atirando && Time.time >= proximoTiro)
        {
            Atirar();
            proximoTiro = Time.time + intervaloTiro;
        }

        AtualizarVisual();
    }

    void FixedUpdate()
    {
        // MOVIMENTO COM FÍSICA: define a velocidade do Rigidbody2D.
        rb.velocity = entrada * velocidade;

        // Mantém a nave dentro da tela.
        Camera cam = Camera.main;
        float limiteY = cam.orthographicSize - margemTela;
        float limiteX = cam.orthographicSize * cam.aspect - margemTela;
        Vector2 p = rb.position;
        Vector2 limitado = new Vector2(Mathf.Clamp(p.x, -limiteX, limiteX), Mathf.Clamp(p.y, -limiteY, limiteY));
        if (limitado != p) rb.position = limitado;
    }

    void Atirar()
    {
        if (TiroTriploAtivo)
        {
            CriarTiro(0f);
            CriarTiro(anguloTiroTriplo);
            CriarTiro(-anguloTiroTriplo);
        }
        else
        {
            CriarTiro(0f);
        }
        AudioManager.Tocar(Som.Tiro, 0.35f);
    }

    void CriarTiro(float angulo)
    {
        Bullet tiro = Instantiate(prefabTiro, pontoDeTiro.position, Quaternion.identity);
        tiro.Configurar(Quaternion.Euler(0f, 0f, angulo) * Vector2.up);
    }

    /// <summary>Chamado por inimigos e tiros inimigos quando acertam a nave.</summary>
    public void LevarDano()
    {
        if (Invencivel || GameManager.Instancia.Estado != EstadoJogo.Jogando) return;

        fimInvencivel = Time.time + tempoInvencivel;
        GameManager.Instancia.PerderVida();
    }

    public void AtivarTiroTriplo(float duracao)
    {
        fimTiroTriplo = Time.time + duracao;
    }

    public void Explodir()
    {
        if (prefabExplosao != null)
        {
            Explosion e = Instantiate(prefabExplosao, transform.position, Quaternion.identity);
            e.Configurar(corOriginal, 3f);
        }
        AudioManager.Tocar(Som.Explosao);
        CameraShake.Tremer(0.5f, 0.4f);
        gameObject.SetActive(false);
    }

    /// <summary>FEEDBACK VISUAL: pisca quando invencível e fica dourada com o tiro triplo.</summary>
    void AtualizarVisual()
    {
        sr.enabled = !Invencivel || Mathf.Repeat(Time.time * 12f, 1f) > 0.5f;
        sr.color = TiroTriploAtivo ? Color.Lerp(corOriginal, new Color(1f, 0.85f, 0.3f), 0.6f) : corOriginal;
    }
}
