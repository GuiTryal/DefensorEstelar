using UnityEngine;

/// <summary>Comportamentos diferentes de inimigo.</summary>
public enum TipoInimigo
{
    Reto,        // desce em linha reta, rápido
    ZigueZague,  // desce fazendo zigue-zague e atira no jogador
    Tanque       // resistente: para no alto da tela, patrulha atirando em leque e depois vai embora
}

/// <summary>Fases do Tanque (máquina de estados do inimigo).</summary>
public enum FaseTanque { Entrando, Atacando, Saindo }

/// <summary>
/// Inimigo: desce pela tela, pode atirar, leva dano dos tiros do jogador
/// e dá pontos (e às vezes um power-up) ao ser destruído.
/// </summary>
public class Enemy : MonoBehaviour
{
    // Quantos inimigos estão vivos agora (usado para saber se a onda acabou).
    public static int Vivos { get; private set; }
    // Quantos tanques estão vivos (o modo infinito limita quantos podem existir ao mesmo tempo).
    public static int TanquesVivos { get; private set; }

    [Header("Comportamento")]
    public TipoInimigo tipo = TipoInimigo.Reto;
    public float velocidade = 2.5f;
    public float amplitude = 1.8f;   // largura do zigue-zague
    public float frequencia = 2f;    // rapidez do zigue-zague

    [Header("Atributos")]
    public int vida = 1;
    public int pontos = 100;

    [Header("Tiro (ZigueZague e Tanque)")]
    public Bullet prefabTiro;
    public float intervaloTiro = 1.6f;

    [Header("Tanque")]
    public float alturaParada = 2.6f;       // altura (y) onde o tanque para de descer
    public float tempoAtacando = 7f;        // quanto tempo fica patrulhando antes de ir embora
    public float velocidadeLateral = 1.4f;
    public float anguloLeque = 22f;         // abertura do tiro em leque

    [Header("Ao morrer")]
    public Explosion prefabExplosao;
    public PowerUp[] prefabsPowerUp;
    [Range(0f, 1f)] public float chancePowerUp = 0.12f;

    public FaseTanque Fase { get; private set; } = FaseTanque.Entrando;

    SpriteRenderer sr;
    Color corOriginal;
    float xInicial;
    float tempoVivo;
    float proximoTiro;
    float fimPiscar;
    float fimAtaque;
    int direcaoLateral = 1;
    bool morto;

    void OnEnable()
    {
        Vivos++;
        if (tipo == TipoInimigo.Tanque) TanquesVivos++;
    }

    void OnDestroy()
    {
        Vivos--;
        if (tipo == TipoInimigo.Tanque) TanquesVivos--;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        corOriginal = sr.color;
    }

    void Start()
    {
        xInicial = transform.position.x;
        proximoTiro = Time.time + Random.Range(0.6f, intervaloTiro);
        direcaoLateral = Random.value < 0.5f ? -1 : 1;
    }

    void Update()
    {
        tempoVivo += Time.deltaTime;

        // MOVIMENTO PELO TRANSFORM (cada tipo se move de um jeito)
        Vector3 p = transform.position;
        switch (tipo)
        {
            case TipoInimigo.Reto:
                p.y -= velocidade * Time.deltaTime;
                break;

            case TipoInimigo.ZigueZague:
                p.y -= velocidade * Time.deltaTime;
                p.x = xInicial + Mathf.Sin(tempoVivo * frequencia) * amplitude;
                break;

            case TipoInimigo.Tanque:
                p = MoverTanque(p);
                break;
        }
        transform.position = p;

        TentarAtirar(p);

        // Feedback: pisca branco ao levar dano.
        sr.color = Time.time < fimPiscar ? Color.white : corOriginal;

        // Passou do fim da tela: some sem dar pontos.
        if (p.y < -Camera.main.orthographicSize - 1.5f) Destroy(gameObject);
    }

    /// <summary>Máquina de estados do Tanque: Entrando -> Atacando -> Saindo.</summary>
    Vector3 MoverTanque(Vector3 p)
    {
        switch (Fase)
        {
            case FaseTanque.Entrando:
                p.y -= velocidade * Time.deltaTime;
                if (p.y <= alturaParada)
                {
                    p.y = alturaParada;
                    Fase = FaseTanque.Atacando;
                    fimAtaque = Time.time + tempoAtacando;
                }
                break;

            case FaseTanque.Atacando:
                // Patrulha de um lado para o outro, virando nas bordas da tela.
                Camera cam = Camera.main;
                float limite = cam.orthographicSize * cam.aspect - 0.9f;
                p.x += direcaoLateral * velocidadeLateral * Time.deltaTime;
                if (p.x > limite) { p.x = limite; direcaoLateral = -1; }
                if (p.x < -limite) { p.x = -limite; direcaoLateral = 1; }

                if (Time.time >= fimAtaque) Fase = FaseTanque.Saindo;
                break;

            case FaseTanque.Saindo:
                p.y -= velocidade * 1.5f * Time.deltaTime;
                break;
        }
        return p;
    }

    void TentarAtirar(Vector3 p)
    {
        if (prefabTiro == null || tipo == TipoInimigo.Reto) return;
        if (GameManager.Instancia.Estado != EstadoJogo.Jogando) return;
        if (Time.time < proximoTiro || p.y > 4.5f) return;
        if (tipo == TipoInimigo.Tanque && Fase != FaseTanque.Atacando) return;

        if (tipo == TipoInimigo.ZigueZague) AtirarNoJogador();
        else AtirarEmLeque();

        proximoTiro = Time.time + intervaloTiro;
    }

    void AtirarNoJogador()
    {
        Vector2 direcao = Vector2.down;
        PlayerController jogador = GameManager.Instancia.jogador;
        if (jogador != null && jogador.gameObject.activeSelf)
            direcao = (jogador.transform.position - transform.position).normalized;

        CriarTiro(direcao);
        AudioManager.Tocar(Som.TiroInimigo, 0.25f);
    }

    void AtirarEmLeque()
    {
        CriarTiro(Vector2.down);
        CriarTiro(Quaternion.Euler(0f, 0f, anguloLeque) * Vector2.down);
        CriarTiro(Quaternion.Euler(0f, 0f, -anguloLeque) * Vector2.down);
        AudioManager.Tocar(Som.TiroInimigo, 0.35f);
    }

    void CriarTiro(Vector2 direcao)
    {
        Bullet tiro = Instantiate(prefabTiro, transform.position + Vector3.down * 0.5f, Quaternion.identity);
        tiro.Configurar(direcao);
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
            e.Configurar(corOriginal, tipo == TipoInimigo.Tanque ? 3f : 1.6f);
        }
        AudioManager.Tocar(Som.Explosao, 0.6f);
        CameraShake.Tremer(0.1f, tipo == TipoInimigo.Tanque ? 0.25f : 0.08f);
        Destroy(gameObject);
    }

    void TentarSoltarPowerUp()
    {
        if (prefabsPowerUp == null || prefabsPowerUp.Length == 0) return;
        if (Random.value > chancePowerUp) return;

        PowerUp escolhido = prefabsPowerUp[Random.Range(0, prefabsPowerUp.Length)];
        Instantiate(escolhido, transform.position, Quaternion.identity);
    }

    // COLISÃO: encostar no jogador causa dano nele. Inimigos comuns são destruídos;
    // o Tanque é resistente e só perde 3 de vida na batida.
    void OnTriggerEnter2D(Collider2D outro)
    {
        if (morto) return;
        PlayerController jogador = outro.GetComponent<PlayerController>();
        if (jogador == null) return;

        jogador.LevarDano();
        if (tipo == TipoInimigo.Tanque) LevarDano(3);
        else Morrer(false);
    }
}
