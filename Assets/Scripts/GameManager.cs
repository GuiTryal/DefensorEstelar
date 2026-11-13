using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estados possíveis do jogo (máquina de estados).
/// </summary>
public enum EstadoJogo { Menu, Jogando, Pausado, GameOver, Vitoria }

/// <summary>
/// Modos de jogo: Ondas (5 ondas e vitória no final) ou Infinito (sobreviver o máximo possível).
/// </summary>
public enum ModoJogo { Ondas, Infinito }

/// <summary>
/// Controla o fluxo geral do jogo: estados, modo, pontuação, vidas, tempo e recordes.
/// Existe apenas um GameManager na cena (padrão Singleton).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    // O modo fica guardado mesmo quando a cena é recarregada (jogar de novo no mesmo modo).
    public static ModoJogo Modo { get; private set; } = ModoJogo.Ondas;

    [Header("Configuração")]
    public int vidasIniciais = 3;
    public int vidasMaximas = 5;

    [Header("Referências da cena")]
    public PlayerController jogador;
    public EnemySpawner spawner;
    public UIManager ui;

    public EstadoJogo Estado { get; private set; }
    public int Pontos { get; private set; }
    public int Vidas { get; private set; }
    public float TempoDePartida { get; private set; }
    public bool NovoRecorde { get; private set; }

    /// <summary>Modo destacado no menu (escolhido com as setas).</summary>
    public ModoJogo ModoSelecionado { get; private set; }

    // Quando o jogador reinicia, a cena é recarregada e a partida começa direto (sem passar pelo menu).
    static bool iniciarDireto;

    void Awake()
    {
        Instancia = this;
        ModoSelecionado = Modo;
    }

    void Start()
    {
        Time.timeScale = 1f;
        jogador.gameObject.SetActive(false);
        MudarEstado(EstadoJogo.Menu);

        // Argumentos de linha de comando para testes: "-iniciar" (ondas) ou "-infinito".
        string[] args = System.Environment.GetCommandLineArgs();
        if (System.Array.IndexOf(args, "-infinito") >= 0) { IniciarPartida(ModoJogo.Infinito); return; }
        if (System.Array.IndexOf(args, "-iniciar") >= 0) { IniciarPartida(ModoJogo.Ondas); return; }

        if (iniciarDireto)
        {
            iniciarDireto = false;
            IniciarPartida(Modo);
        }
    }

    void Update()
    {
        // Cada estado responde a teclas diferentes.
        switch (Estado)
        {
            case EstadoJogo.Menu:
                AtualizarMenu();
                break;

            case EstadoJogo.Jogando:
                TempoDePartida += Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) MudarEstado(EstadoJogo.Pausado);
                break;

            case EstadoJogo.Pausado:
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) MudarEstado(EstadoJogo.Jogando);
                if (Input.GetKeyDown(KeyCode.M)) VoltarAoMenu();
                break;

            case EstadoJogo.GameOver:
            case EstadoJogo.Vitoria:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R)) Reiniciar();
                if (Input.GetKeyDown(KeyCode.Escape)) VoltarAoMenu();
                break;
        }
    }

    void AtualizarMenu()
    {
        // Setas / WASD trocam o modo selecionado.
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            SelecionarModo(ModoJogo.Ondas);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            SelecionarModo(ModoJogo.Infinito);

        // Atalhos: 1 = Ondas, 2 = Infinito.
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) { IniciarPartida(ModoJogo.Ondas); return; }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) { IniciarPartida(ModoJogo.Infinito); return; }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) IniciarPartida(ModoSelecionado);
        if (Input.GetKeyDown(KeyCode.Escape)) Sair();
    }

    /// <summary>Destaca um modo no menu (teclado ou mouse).</summary>
    public void SelecionarModo(ModoJogo modo)
    {
        if (Estado != EstadoJogo.Menu || modo == ModoSelecionado) return;
        ModoSelecionado = modo;
        ui.AtualizarMenu();
        AudioManager.Tocar(Som.Acerto, 0.3f);
    }

    public void MudarEstado(EstadoJogo novo)
    {
        Estado = novo;
        // Pausar = congelar o tempo do jogo.
        Time.timeScale = (novo == EstadoJogo.Pausado) ? 0f : 1f;
        ui.MostrarEstado(novo);

        // Música de fundo de acordo com o estado.
        switch (novo)
        {
            case EstadoJogo.Menu: MusicManager.TocarMenu(); break;
            case EstadoJogo.Jogando: MusicManager.TocarJogo(); break;
            case EstadoJogo.Pausado: MusicManager.Abafar(true); break;
            case EstadoJogo.GameOver:
            case EstadoJogo.Vitoria: MusicManager.Parar(); break;
        }
    }

    public void IniciarPartida(ModoJogo modo)
    {
        Modo = modo;
        ModoSelecionado = modo;
        Pontos = 0;
        Vidas = vidasIniciais;
        TempoDePartida = 0f;
        NovoRecorde = false;

        jogador.gameObject.SetActive(true);
        jogador.Resetar();
        MudarEstado(EstadoJogo.Jogando);
        ui.AtualizarHUD();
        spawner.Iniciar(modo);
    }

    public void AdicionarPontos(int quantidade)
    {
        if (Estado != EstadoJogo.Jogando) return;
        Pontos += quantidade;
        ui.AtualizarHUD();
    }

    public void PerderVida()
    {
        if (Estado != EstadoJogo.Jogando) return;

        Vidas--;
        ui.AtualizarHUD();
        AudioManager.Tocar(Som.Dano);
        CameraShake.Tremer(0.25f, 0.25f);

        if (Vidas <= 0) FimDeJogo();
    }

    public void GanharVida()
    {
        Vidas = Mathf.Min(Vidas + 1, vidasMaximas);
        ui.AtualizarHUD();
    }

    void FimDeJogo()
    {
        jogador.Explodir();
        SalvarRecorde();
        AudioManager.Tocar(Som.GameOver);
        MudarEstado(EstadoJogo.GameOver);
    }

    public void Vitoria()
    {
        if (Estado != EstadoJogo.Jogando) return;
        SalvarRecorde();
        AudioManager.Tocar(Som.Vitoria);
        MudarEstado(EstadoJogo.Vitoria);
    }

    // ------------------------------------------------------------------
    // Recordes (um para cada modo), salvos com PlayerPrefs.
    // ------------------------------------------------------------------

    public static int Recorde(ModoJogo modo) => PlayerPrefs.GetInt("recorde_" + modo, 0);

    /// <summary>Maior tempo sobrevivido no modo Infinito (em segundos).</summary>
    public static float MelhorTempo => PlayerPrefs.GetFloat("melhor_tempo", 0f);

    void SalvarRecorde()
    {
        if (Pontos > Recorde(Modo))
        {
            PlayerPrefs.SetInt("recorde_" + Modo, Pontos);
            NovoRecorde = Pontos > 0;
        }
        if (Modo == ModoJogo.Infinito && TempoDePartida > MelhorTempo)
            PlayerPrefs.SetFloat("melhor_tempo", TempoDePartida);
        PlayerPrefs.Save();
    }

    /// <summary>Formata segundos como mm:ss.</summary>
    public static string FormatarTempo(float segundos)
    {
        int total = Mathf.FloorToInt(segundos);
        return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    void Reiniciar()
    {
        iniciarDireto = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void VoltarAoMenu()
    {
        iniciarDireto = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Sair()
    {
        Application.Quit();
    }
}
