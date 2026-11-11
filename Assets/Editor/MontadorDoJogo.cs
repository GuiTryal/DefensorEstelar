using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ferramenta de editor que monta o jogo inteiro automaticamente:
/// gera os sprites (pixel art), cria os prefabs, monta a cena e configura o projeto.
/// Menu: "Defensor Estelar > Montar Jogo".
/// </summary>
public static class MontadorDoJogo
{
    const string PASTA_SPRITES = "Assets/Sprites";
    const string PASTA_PREFABS = "Assets/Prefabs";
    const string PASTA_CENAS = "Assets/Scenes";
    const string CENA = PASTA_CENAS + "/Jogo.unity";
    const int PIXELS_POR_UNIDADE = 16;

    // ------------------------------------------------------------------
    // Pixel art: '#' = branco, '+' = cinza, '.' = transparente.
    // Os sprites são brancos e recebem cor pelo SpriteRenderer.
    // ------------------------------------------------------------------
    static readonly string[] ARTE_NAVE =
    {
        ".......##.......",
        ".......##.......",
        "......####......",
        "......####......",
        ".....##++##.....",
        ".....#++++#.....",
        "....###++###....",
        "....########....",
        "...##########...",
        "..############..",
        ".##############.",
        "################",
        "###.########.###",
        "##...######...##",
        "#.....+..+.....#",
        "......+..+......",
    };

    static readonly string[] ARTE_INIMIGO_RETO =
    {
        "#..............#",
        "##............##",
        "###..######..###",
        "################",
        "################",
        ".######++######.",
        "..####++++####..",
        "...###++++###...",
        "....########....",
        ".....######.....",
        "......####......",
        ".......##.......",
    };

    static readonly string[] ARTE_INIMIGO_ZIGUEZAGUE =
    {
        ".....######.....",
        "...##########...",
        "..####++++####..",
        ".######++######.",
        "################",
        "#+#+#+#++#+#+#+#",
        ".##############.",
        "...##########...",
        ".....#....#.....",
        "....#......#....",
    };

    static readonly string[] ARTE_TIRO =
    {
        "##", "##", "##", "##", "##", "++",
    };

    static readonly string[] ARTE_TIRO_INIMIGO =
    {
        ".####.",
        "##++##",
        "#++++#",
        "#++++#",
        "##++##",
        ".####.",
    };

    static readonly string[] ARTE_POWERUP_TIRO =
    {
        "...######...",
        "..#......#..",
        ".#........#.",
        "#..#.##.#..#",
        "#..#.##.#..#",
        "#..#.##.#..#",
        "#..#.##.#..#",
        "#..#.##.#..#",
        "#..#.##.#..#",
        ".#........#.",
        "..#......#..",
        "...######...",
    };

    static readonly string[] ARTE_POWERUP_VIDA =
    {
        "...######...",
        "..#......#..",
        ".#...##...#.",
        "#....##....#",
        "#....##....#",
        "#.########.#",
        "#.########.#",
        "#....##....#",
        "#....##....#",
        ".#...##...#.",
        "..#......#..",
        "...######...",
    };

    static readonly string[] ARTE_ESTRELA = { "#" };

    // ------------------------------------------------------------------
    // Menu principal
    // ------------------------------------------------------------------

    [MenuItem("Defensor Estelar/Montar Jogo")]
    public static void MontarTudo()
    {
        CriarPasta(PASTA_SPRITES);
        CriarPasta(PASTA_PREFABS);
        CriarPasta(PASTA_CENAS);

        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        // A cena nova é criada primeiro: criar uma cena descarrega assets da memória.
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprites s = GerarSprites();
        Prefabs p = CriarPrefabs(s);
        CriarCena(cena, s, p);
        ConfigurarProjeto();

        AssetDatabase.SaveAssets();
        Debug.Log("[Montador] Jogo montado com sucesso! Abra Assets/Scenes/Jogo.unity e aperte Play.");
    }

    [MenuItem("Defensor Estelar/Gerar Executável (Windows)")]
    public static void GerarExecutavel()
    {
        ConfigurarProjeto();
        var opcoes = new BuildPlayerOptions
        {
            scenes = new[] { CENA },
            locationPathName = "Build/DefensorEstelar.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        var relatorio = BuildPipeline.BuildPlayer(opcoes);
        Debug.Log("[Montador] Resultado do build: " + relatorio.summary.result);

        if (Application.isBatchMode && relatorio.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    // ------------------------------------------------------------------
    // Sprites
    // ------------------------------------------------------------------

    class Sprites
    {
        public Sprite nave, inimigoReto, inimigoZigueZague, tiro, tiroInimigo, powerUpTiro, powerUpVida, estrela, circulo;
    }

    static Sprites GerarSprites()
    {
        var s = new Sprites
        {
            nave = SalvarSprite("Nave", TexturaDeArte(ARTE_NAVE)),
            inimigoReto = SalvarSprite("InimigoReto", TexturaDeArte(ARTE_INIMIGO_RETO)),
            inimigoZigueZague = SalvarSprite("InimigoZigueZague", TexturaDeArte(ARTE_INIMIGO_ZIGUEZAGUE)),
            tiro = SalvarSprite("Tiro", TexturaDeArte(ARTE_TIRO)),
            tiroInimigo = SalvarSprite("TiroInimigo", TexturaDeArte(ARTE_TIRO_INIMIGO)),
            powerUpTiro = SalvarSprite("PowerUpTiro", TexturaDeArte(ARTE_POWERUP_TIRO)),
            powerUpVida = SalvarSprite("PowerUpVida", TexturaDeArte(ARTE_POWERUP_VIDA)),
            estrela = SalvarSprite("Estrela", TexturaDeArte(ARTE_ESTRELA)),
            circulo = SalvarSprite("Circulo", TexturaCirculo(16)),
        };
        return s;
    }

    static Texture2D TexturaDeArte(string[] linhas)
    {
        int altura = linhas.Length;
        int largura = 0;
        foreach (string l in linhas) largura = Mathf.Max(largura, l.Length);

        var tex = new Texture2D(largura, altura, TextureFormat.RGBA32, false);
        for (int y = 0; y < altura; y++)
        {
            string linha = linhas[altura - 1 - y]; // textura começa por baixo
            for (int x = 0; x < largura; x++)
            {
                char c = x < linha.Length ? linha[x] : '.';
                Color cor = c == '#' ? Color.white : (c == '+' ? new Color(0.55f, 0.55f, 0.6f, 1f) : Color.clear);
                tex.SetPixel(x, y, cor);
            }
        }
        tex.Apply();
        return tex;
    }

    static Texture2D TexturaCirculo(int tamanho)
    {
        var tex = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        float centro = (tamanho - 1) / 2f;
        float raio = tamanho / 2f;
        for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                tex.SetPixel(x, y, d <= raio - 0.5f ? Color.white : Color.clear);
            }
        tex.Apply();
        return tex;
    }

    static Sprite SalvarSprite(string nome, Texture2D tex)
    {
        string caminho = PASTA_SPRITES + "/" + nome + ".png";
        File.WriteAllBytes(caminho, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(caminho);

        var importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.spritePixelsPerUnit = PIXELS_POR_UNIDADE;
        importador.filterMode = FilterMode.Point;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.alphaIsTransparency = true;
        importador.mipmapEnabled = false;
        importador.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    // ------------------------------------------------------------------
    // Prefabs
    // ------------------------------------------------------------------

    class Prefabs
    {
        public Bullet tiro, tiroInimigo;
        public Explosion explosao;
        public PowerUp powerUpTiro, powerUpVida;
        public Enemy inimigoReto, inimigoZigueZague;
        public PlayerController jogador;
    }

    static readonly Color COR_JOGADOR = new Color(0.35f, 0.85f, 1f);
    static readonly Color COR_TIRO = new Color(1f, 0.95f, 0.4f);
    static readonly Color COR_TIRO_INIMIGO = new Color(1f, 0.45f, 0.75f);
    static readonly Color COR_INIMIGO_RETO = new Color(1f, 0.35f, 0.3f);
    static readonly Color COR_INIMIGO_ZIGUEZAGUE = new Color(0.75f, 0.45f, 1f);
    static readonly Color COR_POWERUP_TIRO = new Color(1f, 0.8f, 0.2f);
    static readonly Color COR_POWERUP_VIDA = new Color(0.35f, 1f, 0.45f);

    static Prefabs CriarPrefabs(Sprites s)
    {
        var p = new Prefabs();

        // Tiro do jogador
        {
            GameObject go = ObjetoComSprite("TiroJogador", s.tiro, COR_TIRO, 5);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            CorpoCinematico(go);
            var b = go.AddComponent<Bullet>();
            b.doJogador = true;
            b.velocidade = 14f;
            p.tiro = SalvarPrefab(go).GetComponent<Bullet>();
        }

        // Tiro do inimigo
        {
            GameObject go = ObjetoComSprite("TiroInimigo", s.tiroInimigo, COR_TIRO_INIMIGO, 5);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            CorpoCinematico(go);
            var b = go.AddComponent<Bullet>();
            b.doJogador = false;
            b.velocidade = 5f;
            b.direcao = Vector2.down;
            p.tiroInimigo = SalvarPrefab(go).GetComponent<Bullet>();
        }

        // Explosão
        {
            GameObject go = ObjetoComSprite("Explosao", s.circulo, Color.white, 20);
            go.AddComponent<Explosion>();
            p.explosao = SalvarPrefab(go).GetComponent<Explosion>();
        }

        // Power-ups
        p.powerUpTiro = CriarPowerUp("PowerUpTiroTriplo", s.powerUpTiro, COR_POWERUP_TIRO, TipoPowerUp.TiroTriplo);
        p.powerUpVida = CriarPowerUp("PowerUpVida", s.powerUpVida, COR_POWERUP_VIDA, TipoPowerUp.VidaExtra);
        var powerUps = new[] { p.powerUpTiro, p.powerUpTiro, p.powerUpVida }; // tiro triplo é 2x mais comum

        // Inimigo reto
        {
            GameObject go = ObjetoComSprite("InimigoReto", s.inimigoReto, COR_INIMIGO_RETO, 10);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.85f, 0.6f);
            CorpoCinematico(go);
            var e = go.AddComponent<Enemy>();
            e.tipo = TipoInimigo.Reto;
            e.velocidade = 2.6f;
            e.vida = 1;
            e.pontos = 100;
            e.prefabExplosao = p.explosao;
            e.prefabsPowerUp = powerUps;
            e.chancePowerUp = 0.1f;
            p.inimigoReto = SalvarPrefab(go).GetComponent<Enemy>();
        }

        // Inimigo zigue-zague (atira)
        {
            GameObject go = ObjetoComSprite("InimigoZigueZague", s.inimigoZigueZague, COR_INIMIGO_ZIGUEZAGUE, 10);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.5f);
            CorpoCinematico(go);
            var e = go.AddComponent<Enemy>();
            e.tipo = TipoInimigo.ZigueZague;
            e.velocidade = 1.3f;
            e.amplitude = 1.8f;
            e.frequencia = 2f;
            e.vida = 3;
            e.pontos = 250;
            e.prefabTiro = p.tiroInimigo;
            e.intervaloTiro = 1.8f;
            e.prefabExplosao = p.explosao;
            e.prefabsPowerUp = powerUps;
            e.chancePowerUp = 0.25f;
            p.inimigoZigueZague = SalvarPrefab(go).GetComponent<Enemy>();
        }

        // Jogador (física: Rigidbody2D dinâmico sem gravidade)
        {
            GameObject go = ObjetoComSprite("Jogador", s.nave, COR_JOGADOR, 15);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f; // hitbox menor que o desenho, mais justo para o jogador

            var ponto = new GameObject("PontoDeTiro").transform;
            ponto.SetParent(go.transform);
            ponto.localPosition = new Vector3(0f, 0.55f, 0f);

            var pc = go.AddComponent<PlayerController>();
            pc.prefabTiro = p.tiro;
            pc.pontoDeTiro = ponto;
            pc.prefabExplosao = p.explosao;
            p.jogador = SalvarPrefab(go).GetComponent<PlayerController>();
        }

        return p;
    }

    static PowerUp CriarPowerUp(string nome, Sprite sprite, Color cor, TipoPowerUp tipo)
    {
        GameObject go = ObjetoComSprite(nome, sprite, cor, 8);
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;
        CorpoCinematico(go);
        var pu = go.AddComponent<PowerUp>();
        pu.tipo = tipo;
        return SalvarPrefab(go).GetComponent<PowerUp>();
    }

    static GameObject ObjetoComSprite(string nome, Sprite sprite, Color cor, int ordem)
    {
        var go = new GameObject(nome);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = cor;
        sr.sortingOrder = ordem;
        return go;
    }

    /// <summary>Rigidbody2D cinemático: movido pelo Transform, mas detecta triggers.</summary>
    static void CorpoCinematico(GameObject go)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;
    }

    static GameObject SalvarPrefab(GameObject go)
    {
        string caminho = PASTA_PREFABS + "/" + go.name + ".prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, caminho);
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ------------------------------------------------------------------
    // Cena
    // ------------------------------------------------------------------

    static void CriarCena(UnityEngine.SceneManagement.Scene cena, Sprites s, Prefabs p)
    {
        // Câmera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<CameraShake>();
        camGO.AddComponent<ProporcaoTela>();
        cam.depth = 0;

        // Câmera de fundo: pinta de preto as faixas fora da área de jogo.
        var fundoCamGO = new GameObject("Camera Faixas Pretas");
        var fundoCam = fundoCamGO.AddComponent<Camera>();
        fundoCam.clearFlags = CameraClearFlags.SolidColor;
        fundoCam.backgroundColor = Color.black;
        fundoCam.cullingMask = 0;
        fundoCam.depth = -1;

        // Fundo de estrelas
        var fundo = new GameObject("FundoEstrelas").AddComponent<StarField>();
        fundo.spriteEstrela = s.estrela;

        // Jogador
        var jogador = (GameObject)PrefabUtility.InstantiatePrefab(p.jogador.gameObject);
        jogador.transform.position = new Vector3(0f, -3.5f, 0f);

        // Gerenciadores
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        gmGO.AddComponent<AudioSource>().playOnAwake = false;
        gmGO.AddComponent<AudioManager>();
        var spawner = gmGO.AddComponent<EnemySpawner>();
        spawner.prefabReto = p.inimigoReto;
        spawner.prefabZigueZague = p.inimigoZigueZague;
        spawner.ondas = new[]
        {
            new Onda { quantidadeRetos = 6,  quantidadeZigueZague = 0,  intervaloEntreInimigos = 0.9f,  velocidadeExtra = 0f },
            new Onda { quantidadeRetos = 8,  quantidadeZigueZague = 2,  intervaloEntreInimigos = 0.8f,  velocidadeExtra = 0.2f },
            new Onda { quantidadeRetos = 8,  quantidadeZigueZague = 4,  intervaloEntreInimigos = 0.7f,  velocidadeExtra = 0.4f },
            new Onda { quantidadeRetos = 12, quantidadeZigueZague = 6,  intervaloEntreInimigos = 0.55f, velocidadeExtra = 0.6f },
            new Onda { quantidadeRetos = 14, quantidadeZigueZague = 10, intervaloEntreInimigos = 0.45f, velocidadeExtra = 0.9f },
        };

        UIManager ui = CriarInterface(cam);

        gm.jogador = jogador.GetComponent<PlayerController>();
        gm.spawner = spawner;
        gm.ui = ui;

        EditorSceneManager.SaveScene(cena, CENA);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CENA, true) };
    }

    // ------------------------------------------------------------------
    // Interface (uGUI)
    // ------------------------------------------------------------------

    static Font fonte;

    static UIManager CriarInterface(Camera cam)
    {
        fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Canvas preso à câmera: a interface ocupa só a área de jogo (sem as faixas pretas).
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5f;
        canvas.sortingOrder = 100; // acima de todos os sprites
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(600, 800);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var ui = canvasGO.AddComponent<UIManager>();
        Transform raiz = canvasGO.transform;

        // HUD
        var hud = CriarRect("HUD", raiz);
        Esticar(hud);
        ui.hud = hud.gameObject;
        ui.textoPontos = CriarTexto("Pontos", hud, "PONTOS: 0", 26, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(20, -15), new Vector2(300, 40), Color.white);
        ui.textoVidas = CriarTexto("Vidas", hud, "VIDAS: 3", 26, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-20, -15), new Vector2(250, 40), new Color(0.5f, 1f, 0.6f));
        ui.textoOnda = CriarTexto("Onda", hud, "", 22, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -55), new Vector2(300, 35), new Color(0.8f, 0.8f, 1f));
        ui.textoPowerUp = CriarTexto("PowerUp", hud, "", 22, TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(400, 35), COR_POWERUP_TIRO);

        // Aviso central (fica fora do HUD para aparecer sempre)
        ui.textoAviso = CriarTexto("Aviso", raiz, "", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(560, 80), new Color(1f, 0.9f, 0.3f));

        // Menu
        var menu = CriarPainel("PainelMenu", raiz, 0.55f);
        CriarTexto("Titulo", menu, "DEFENSOR\nESTELAR", 72, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(560, 200), COR_JOGADOR);
        CriarTexto("Instrucoes", menu,
            "SETAS / WASD  -  mover\nESPAÇO / Z  -  atirar\nP / ESC  -  pausar\n\nSobreviva às 5 ondas de inimigos!\nItens: amarelo = tiro triplo, verde = +1 vida",
            22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(560, 220), new Color(0.85f, 0.85f, 0.95f));
        CriarTexto("Iniciar", menu, "Pressione ENTER para começar", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -190), new Vector2(560, 50), new Color(1f, 0.9f, 0.3f));
        ui.textoRecordeMenu = CriarTexto("Recorde", menu, "RECORDE: 0", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(560, 40), Color.white);
        ui.painelMenu = menu.gameObject;

        // Pausa
        var pausa = CriarPainel("PainelPausa", raiz, 0.6f);
        CriarTexto("Titulo", pausa, "PAUSADO", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(560, 90), Color.white);
        CriarTexto("Dica", pausa, "P / ESC para continuar\nM para voltar ao menu", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(560, 80), new Color(0.85f, 0.85f, 0.95f));
        ui.painelPausa = pausa.gameObject;

        // Game Over
        var gameOver = CriarPainel("PainelGameOver", raiz, 0.65f);
        CriarTexto("Titulo", gameOver, "GAME OVER", 68, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(560, 90), COR_INIMIGO_RETO);
        ui.textoResultadoGameOver = CriarTexto("Resultado", gameOver, "", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(560, 150), Color.white);
        CriarTexto("Dica", gameOver, "ENTER para jogar de novo\nESC para o menu", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(560, 80), new Color(1f, 0.9f, 0.3f));
        ui.painelGameOver = gameOver.gameObject;

        // Vitória
        var vitoria = CriarPainel("PainelVitoria", raiz, 0.65f);
        CriarTexto("Titulo", vitoria, "VITÓRIA!", 68, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(560, 90), COR_POWERUP_VIDA);
        CriarTexto("Sub", vitoria, "Você defendeu a galáxia!", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 85), new Vector2(560, 50), Color.white);
        ui.textoResultadoVitoria = CriarTexto("Resultado", vitoria, "", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(560, 150), Color.white);
        CriarTexto("Dica", vitoria, "ENTER para jogar de novo\nESC para o menu", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(560, 80), new Color(1f, 0.9f, 0.3f));
        ui.painelVitoria = vitoria.gameObject;

        pausa.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(false);
        vitoria.gameObject.SetActive(false);

        return ui;
    }

    static RectTransform CriarRect(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return (RectTransform)go.transform;
    }

    static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static RectTransform CriarPainel(string nome, Transform pai, float opacidade)
    {
        var rt = CriarRect(nome, pai);
        Esticar(rt);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0.05f, opacidade);
        return rt;
    }

    static Text CriarTexto(string nome, Transform pai, string conteudo, int tamanho, TextAnchor alinhamento,
                           Vector2 ancora, Vector2 posicao, Vector2 dimensao, Color cor)
    {
        var rt = CriarRect(nome, pai);
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = dimensao;

        var t = rt.gameObject.AddComponent<Text>();
        t.font = fonte;
        t.text = conteudo;
        t.fontSize = tamanho;
        t.alignment = alinhamento;
        t.color = cor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        var sombra = rt.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color(0f, 0f, 0f, 0.8f);
        sombra.effectDistance = new Vector2(2f, -2f);
        return t;
    }

    // ------------------------------------------------------------------
    // Projeto
    // ------------------------------------------------------------------

    static void ConfigurarProjeto()
    {
        PlayerSettings.productName = "Defensor Estelar";
        PlayerSettings.companyName = "DefensorEstelar";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 600;
        PlayerSettings.defaultScreenHeight = 800;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
    }

    static void CriarPasta(string caminho)
    {
        if (!AssetDatabase.IsValidFolder(caminho))
        {
            string pai = Path.GetDirectoryName(caminho).Replace('\\', '/');
            AssetDatabase.CreateFolder(pai, Path.GetFileName(caminho));
        }
    }
}
