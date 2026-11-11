using UnityEngine;

/// <summary>Comportamentos diferentes de inimigo.</summary>
public enum TipoInimigo
{
    Reto,        // desce em linha reta, rápido
    ZigueZague   // desce fazendo zigue-zague e atira no jogador
}

/// <summary>
/// Inimigo: desce pela tela, pode atirar, leva dano dos tiros do jogador
/// e dá pontos (e às vezes um power-up) ao ser destruído.
/// </summary>
public class Enemy : MonoBehaviour
{
    // Quantos inimigos estão vivos agora (usado para saber se a onda acabou).
    public static int Vivos { get; private set; }

    [Header("Comportamento")]
    public TipoInimigo tipo = TipoInimigo.Reto;
    public float velocidade = 2.5f;
    public float amplitude = 1.8f;   // largura do zigue-zague
    public float frequencia = 2f;    // rapidez do zigue-zague

    [Header("Atributos")]
    public int vida = 1;
    public int pontos = 100;

    [Header("Tiro (só ZigueZague)")]
    public Bullet prefabTiro;
    public float intervaloTiro = 1.6f;

    [Header("Ao morrer")]
    public Explosion prefabExplosao;
    public PowerUp[] prefabsPowerUp;
    [Range(0f, 1f)] public float chancePowerUp = 0.12f;

    SpriteRenderer sr;
    Color corOriginal;
    float xInicial;
    float tempoVivo;
    float proximoTiro;
    float fimPiscar;
    bool morto;

    void OnEnable() { Vivos++; }
    void OnDestroy() { Vivos--; }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        corOriginal = sr.color;
    }

    void Start()
    {
        xInicial = transform.position.x;
        proximoTiro = Time.time + Random.Range(0.6f, intervaloTiro);
    }

    void Update()
    {
        tempoVivo += Time.deltaTime;

        // MOVIMENTO PELO TRANSFORM
        Vector3 p = transform.position;
        p.y -= velocidade * Time.deltaTime;
        if (tipo == TipoInimigo.ZigueZague)
            p.x = xInicial + Mathf.Sin(tempoVivo * frequencia) * amplitude;
        transform.position = p;

        // O ZigueZague atira periodicamente (só enquanto o jogo está rolando).
        bool jogando = GameManager.Instancia.Estado == EstadoJogo.Jogando;
        if (tipo == TipoInimigo.ZigueZague && jogando && prefabTiro != null && Time.time >= proximoTiro && p.y < 4.5f)
        {
            AtirarNoJogador();
            proximoTiro = Time.time + intervaloTiro;
        }

        // Feedback: pisca branco ao levar dano.
        sr.color = Time.time < fimPiscar ? Color.white : corOriginal;

        // Passou do fim da tela: some sem dar pontos.
        if (p.y < -Camera.main.orthographicSize - 1f) Destroy(gameObject);
    }

    void AtirarNoJogador()
    {
        Vector2 direcao = Vector2.down;
        PlayerController jogador = GameManager.Instancia.jogador;
        if (jogador != null && jogador.gameObject.activeSelf)
            direcao = (jogador.transform.position - transform.position).normalized;

        Bullet tiro = Instantiate(prefabTiro, transform.position + Vector3.down * 0.4f, Quaternion.identity);
        tiro.Configurar(direcao);
        AudioManager.Tocar(Som.TiroInimigo, 0.25f);
    }

    public void LevarDano(int dano)
    {
        if (morto) return;
        vida -= dano;
        fimPiscar = Time.time + 0.08f;
        if (vida <= 0) Morrer(true);
        else AudioManager.Tocar(Som.Acerto, 0.4f);
    }

    void Morrer(bool porTiroDoJogador)
    {
        morto = true;

        if (porTiroDoJogador)
        {
            GameManager.Instancia.AdicionarPontos(pontos);
            TentarSoltarPowerUp();
        }

        if (prefabExplosao != null)
        {
            Explosion e = Instantiate(prefabExplosao, transform.position, Quaternion.identity);
            e.Configurar(corOriginal, 1.6f);
        }
        AudioManager.Tocar(Som.Explosao, 0.6f);
        CameraShake.Tremer(0.1f, 0.08f);
        Destroy(gameObject);
    }

    void TentarSoltarPowerUp()
    {
        if (prefabsPowerUp == null || prefabsPowerUp.Length == 0) return;
        if (Random.value > chancePowerUp) return;

        PowerUp escolhido = prefabsPowerUp[Random.Range(0, prefabsPowerUp.Length)];
        Instantiate(escolhido, transform.position, Quaternion.identity);
    }

    // COLISÃO: encostar no jogador causa dano nele e destrói o inimigo.
    void OnTriggerEnter2D(Collider2D outro)
    {
        if (morto) return;
        PlayerController jogador = outro.GetComponent<PlayerController>();
        if (jogador != null)
        {
            jogador.LevarDano();
            Morrer(false);
        }
    }
}
