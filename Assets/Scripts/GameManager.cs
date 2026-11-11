using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Estados possíveis do jogo (máquina de estados).
/// </summary>
public enum EstadoJogo { Menu, Jogando, Pausado, GameOver, Vitoria }

/// <summary>
/// Controla o fluxo geral do jogo: estados, pontuação, vidas e recorde.
/// Existe apenas um GameManager na cena (padrão Singleton).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

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
    public int Recorde { get; private set; }

    const string CHAVE_RECORDE = "recorde";

    // Quando o jogador reinicia, a cena é recarregada e a partida começa direto (sem passar pelo menu).
    static bool iniciarDireto;

    void Awake()
    {
        Instancia = this;
        Recorde = PlayerPrefs.GetInt(CHAVE_RECORDE, 0);
    }

    void Start()
    {
        Time.timeScale = 1f;
        jogador.gameObject.SetActive(false);
        MudarEstado(EstadoJogo.Menu);

        // "-iniciar" na linha de comando também pula o menu (útil para testes).
        bool argumentoIniciar = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-iniciar") >= 0;
        if (iniciarDireto || argumentoIniciar)
        {
            iniciarDireto = false;
            IniciarPartida();
        }
    }

    void Update()
    {
        // Cada estado responde a teclas diferentes.
        switch (Estado)
        {
            case EstadoJogo.Menu:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) IniciarPartida();
                if (Input.GetKeyDown(KeyCode.Escape)) Sair();
                break;

            case EstadoJogo.Jogando:
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

    public void MudarEstado(EstadoJogo novo)
    {
        Estado = novo;
        // Pausar = congelar o tempo do jogo.
        Time.timeScale = (novo == EstadoJogo.Pausado) ? 0f : 1f;
        ui.MostrarEstado(novo);
    }

    public void IniciarPartida()
    {
        Pontos = 0;
        Vidas = vidasIniciais;
        jogador.gameObject.SetActive(true);
        jogador.Resetar();
        MudarEstado(EstadoJogo.Jogando);
        ui.AtualizarHUD();
        spawner.Iniciar();
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

    void SalvarRecorde()
    {
        if (Pontos > Recorde)
        {
            Recorde = Pontos;
            PlayerPrefs.SetInt(CHAVE_RECORDE, Recorde);
            PlayerPrefs.Save();
        }
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
