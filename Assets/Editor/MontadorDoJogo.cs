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

    static readonly string[] ARTE_TANQUE =
    {
        "...####..........####...",
        "..######........######..",
        ".######################.",
        "########################",
        "##++####++++++++####++##",
        "########++++++++########",
        ".#######++####++#######.",
        "..######++####++######..",
        "...#####++++++++#####...",
        "....#######++#######....",
        ".....######..######.....",
        "......###..##..###......",
        ".......#...##...#.......",
        "...........##...........",
    };

    static readonly string[] ARTE_POWERUP_ESCUDO =
    {
        "...######...",
        "..#......#..",
        ".#..####..#.",
        "#..######..#",
        "#..######..#",
        "#..######..#",
        "#..######..#",
        "#...####...#",
        "#....##....#",
        ".#........#.",
        "..#......#..",
        "...######...",
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
        public Sprite nave, inimigoReto, inimigoZigueZague, tanque, tiro, tiroInimigo, powerUpTiro, powerUpVida, powerUpEscudo, estrela, circulo, anel;
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
            tanque = SalvarSprite("Tanque", TexturaDeArte(ARTE_TANQUE)),
            powerUpEscudo = SalvarSprite("PowerUpEscudo", TexturaDeArte(ARTE_POWERUP_ESCUDO)),
            anel = SalvarSprite("Escudo", TexturaAnel(32)),
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

    /// <summary>Bolha do escudo: borda forte e miolo bem transparente.</summary>
    static Texture2D TexturaAnel(int tamanho)
    {
        var tex = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        float centro = (tamanho - 1) / 2f;
        float raio = tamanho / 2f - 0.5f;
        for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));
                Color c = Color.clear;
                if (d <= raio && d > raio - 2f) c = Color.white;
                else if (d <= raio - 2f) c = new Color(1f, 1f, 1f, 0.2f);
                tex.SetPixel(x, y, c);
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
        public PowerUp powerUpTiro, powerUpVida, powerUpEscudo;
        public Enemy inimigoReto, inimigoZigueZague, tanque;
        public PlayerController jogador;
    }

    static readonly Color COR_JOGADOR = new Color(0.35f, 0.85f, 1f);
    static readonly Color COR_TIRO = new Color(1f, 0.95f, 0.4f);
    static readonly Color COR_TIRO_INIMIGO = new Color(1f, 0.45f, 0.75f);
    static readonly Color COR_INIMIGO_RETO = new Color(1f, 0.35f, 0.3f);
    static readonly Color COR_INIMIGO_ZIGUEZAGUE = new Color(0.75f, 0.45f, 1f);
    static readonly Color COR_POWERUP_TIRO = new Color(1f, 0.8f, 0.2f);
    static readonly Color COR_POWERUP_VIDA = new Color(0.35f, 1f, 0.45f);
    static readonly Color COR_POWERUP_ESCUDO = new Color(0.4f, 0.65f, 1f);
    static readonly Color COR_TANQUE = new Color(1f, 0.55f, 0.15f);

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
        p.powerUpEscudo = CriarPowerUp("PowerUpEscudo", s.powerUpEscudo, COR_POWERUP_ESCUDO, TipoPowerUp.Escudo);
        // Sorteio com pesos: tiro triplo e escudo são 2x mais comuns que vida extra.
        var powerUps = new[] { p.powerUpTiro, p.powerUpTiro, p.powerUpEscudo, p.powerUpEscudo, p.powerUpVida };

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
            e.chancePowerUp = 0.05f;
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
            e.chancePowerUp = 0.15f;
            p.inimigoZigueZague = SalvarPrefab(go).GetComponent<Enemy>();
        }

        // Tanque (resistente, para no alto e atira em leque)
        {
            GameObject go = ObjetoComSprite("Tanque", s.tanque, COR_TANQUE, 11);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.4f, 0.7f);
            CorpoCinematico(go);
            var e = go.AddComponent<Enemy>();
            e.tipo = TipoInimigo.Tanque;
            e.velocidade = 1.8f;
            e.vida = 8;
            e.pontos = 500;
            e.prefabTiro = p.tiroInimigo;
            e.intervaloTiro = 1.5f;
            e.alturaParada = 2.6f;
            e.tempoAtacando = 7f;
            e.velocidadeLateral = 1.4f;
            e.prefabExplosao = p.explosao;
            e.prefabsPowerUp = powerUps;
            e.chancePowerUp = 1f; // o tanque sempre solta um poder
            p.tanque = SalvarPrefab(go).GetComponent<Enemy>();
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

            // Bolha do escudo (fica escondida até pegar o power-up).
            var escudo = new GameObject("Escudo");
            escudo.transform.SetParent(go.transform);
            escudo.transform.localPosition = Vector3.zero;
            escudo.transform.localScale = Vector3.one * 0.8f;
            var srEscudo = escudo.AddComponent<SpriteRenderer>();
            srEscudo.sprite = s.anel;
            srEscudo.color = new Color(0.45f, 0.75f, 1f, 0.5f);
            srEscudo.sortingOrder = 16;
            srEscudo.enabled = false;

            var pc = go.AddComponent<PlayerController>();
            pc.prefabTiro = p.tiro;
            pc.pontoDeTiro = ponto;
            pc.prefabExplosao = p.explosao;
            pc.visualEscudo = srEscudo;
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

        CriarFundoLateral(cam);

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
        spawner.prefabTanque = p.tanque;
        spawner.ondas = new[]
        {
            new Onda { quantidadeRetos = 6,  quantidadeZigueZague = 0, quantidadeTanques = 0, intervaloEntreInimigos = 0.9f,  velocidadeExtra = 0f },
            new Onda { quantidadeRetos = 8,  quantidadeZigueZague = 2, quantidadeTanques = 0, intervaloEntreInimigos = 0.8f,  velocidadeExtra = 0.2f },
            new Onda { quantidadeRetos = 8,  quantidadeZigueZague = 4, quantidadeTanques = 1, intervaloEntreInimigos = 0.75f, velocidadeExtra = 0.3f },
            new Onda { quantidadeRetos = 12, quantidadeZigueZague = 5, quantidadeTanques = 1, intervaloEntreInimigos = 0.65f, velocidadeExtra = 0.5f },
            new Onda { quantidadeRetos = 14, quantidadeZigueZague = 7, quantidadeTanques = 2, intervaloEntreInimigos = 0.55f, velocidadeExtra = 0.7f },
        };

        UIManager ui = CriarInterface(cam, s);

        gm.jogador = jogador.GetComponent<PlayerController>();
        gm.spawner = spawner;
        gm.ui = ui;

        EditorSceneManager.SaveScene(cena, CENA);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CENA, true) };
    }

    // ------------------------------------------------------------------
    // Fundo das laterais (fora da área de jogo)
    // ------------------------------------------------------------------

    /// <summary>
    /// Cria uma segunda câmera (desenhada antes da principal) que mostra uma imagem
    /// espacial fixa ocupando a tela toda, mais uma moldura brilhante em volta da área de jogo.
    /// A câmera principal desenha o jogo por cima, só no retângulo central.
    /// </summary>
    static void CriarFundoLateral(Camera camJogo)
    {
        Texture2D textura = SalvarTextura("FundoLateral", GerarTexturaFundo());

        // Câmera posicionada longe do jogo, para não enxergar nada além do próprio fundo.
        var camGO = new GameObject("Camera Fundo Lateral");
        camGO.transform.position = new Vector3(1000f, 0f, -10f);
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.depth = -1;

        var canvasGO = new GameObject("CanvasFundoLateral");
        canvasGO.transform.SetParent(camGO.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5f;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        // Imagem de fundo cobrindo a tela toda, sem distorcer.
        var imagem = CriarRect("ImagemEspaco", canvasGO.transform);
        Esticar(imagem);
        imagem.gameObject.AddComponent<RawImage>().texture = textura;
        var ajuste = imagem.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        ajuste.aspectRatio = (float)textura.width / textura.height;

        // Moldura: três camadas para dar um efeito de brilho.
        var moldura = canvasGO.AddComponent<MolduraTela>();
        moldura.cameraDoJogo = camJogo;
        moldura.espessuras = new[] { 12f, 6f, 3f };
        float[] opacidades = { 0.12f, 0.3f, 0.9f };
        moldura.camadas = new RectTransform[3];
        for (int i = 0; i < 3; i++)
        {
            var camada = CriarRect("Moldura" + i, canvasGO.transform);
            var img = camada.gameObject.AddComponent<Image>();
            img.color = new Color(COR_JOGADOR.r, COR_JOGADOR.g, COR_JOGADOR.b, opacidades[i]);
            img.raycastTarget = false;
            moldura.camadas[i] = camada;
        }
    }

    /// <summary>Gera uma paisagem espacial em pixel art: nebulosa, estrelas, planeta com anel e lua.</summary>
    static Texture2D GerarTexturaFundo()
    {
        const int L = 320, A = 180;
        var px = new Color[L * A];
        var rnd = new System.Random(7);

        Color baixo = new Color(0.01f, 0.01f, 0.04f);
        Color cima = new Color(0.04f, 0.02f, 0.10f);
        Color nebulosaRoxa = new Color(0.40f, 0.12f, 0.50f);
        Color nebulosaAzul = new Color(0.08f, 0.22f, 0.50f);

        // Céu com gradiente e nebulosas (ruído Perlin em "degraus" para parecer pixel art).
        for (int y = 0; y < A; y++)
            for (int x = 0; x < L; x++)
            {
                Color c = Color.Lerp(baixo, cima, (float)y / A);
                float n1 = Ruido(x * 0.012f, y * 0.012f, 3.1f);
                float n2 = Ruido(x * 0.017f, y * 0.017f, 11.7f);
                c = Color.Lerp(c, nebulosaRoxa, Degraus(Mathf.Clamp01((n1 - 0.45f) * 2.4f), 4) * 0.45f);
                c = Color.Lerp(c, nebulosaAzul, Degraus(Mathf.Clamp01((n2 - 0.5f) * 2.4f), 4) * 0.35f);
                px[y * L + x] = c;
            }

        // Estrelas.
        float[] brilhos = { 0.3f, 0.55f, 1f };
        for (int i = 0; i < 280; i++)
        {
            int x = rnd.Next(1, L - 1), y = rnd.Next(1, A - 1);
            float b = brilhos[rnd.Next(brilhos.Length)];
            px[y * L + x] = Color.Lerp(px[y * L + x], Color.white, b);
            if (i % 20 == 0) // algumas estrelas maiores, em forma de cruz
            {
                foreach (var d in new[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) })
                {
                    int k = (y + d.y) * L + (x + d.x);
                    px[k] = Color.Lerp(px[k], Color.white, 0.45f);
                }
            }
        }

        // Planeta com anel (esquerda), lua com crateras (direita) e um planetinha distante.
        DesenharPlaneta(px, L, A, 50, 92, 30, new Color(0.98f, 0.62f, 0.35f), new Color(0.30f, 0.10f, 0.16f), new Vector3(0.6f, 0.4f, 0.7f), true, false, rnd);
        DesenharPlaneta(px, L, A, 270, 128, 14, new Color(0.78f, 0.82f, 0.92f), new Color(0.16f, 0.18f, 0.30f), new Vector3(-0.6f, 0.4f, 0.7f), false, true, rnd);
        DesenharPlaneta(px, L, A, 292, 42, 6, new Color(0.40f, 0.95f, 0.80f), new Color(0.04f, 0.22f, 0.28f), new Vector3(-0.6f, 0.4f, 0.7f), false, false, rnd);

        var tex = new Texture2D(L, A, TextureFormat.RGBA32, false);
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    static void DesenharPlaneta(Color[] px, int L, int A, int cx, int cy, int r, Color clara, Color escura,
                                Vector3 luz, bool anel, bool crateras, System.Random rnd)
    {
        luz.Normalize();

        // Corpo do planeta com iluminação em "faixas" (5 tons).
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || y < 0 || x >= L || y >= A) continue;
                float dx = (x - cx) / (float)r, dy = (y - cy) / (float)r;
                float d2 = dx * dx + dy * dy;
                if (d2 > 1f) continue;
                Vector3 normal = new Vector3(dx, dy, Mathf.Sqrt(1f - d2));
                float luzAqui = Mathf.Max(0f, Vector3.Dot(normal, luz)) * 0.9f + 0.1f;
                if (anel) luzAqui += Mathf.Sin(dy * 14f) * 0.06f; // faixas de gás
                px[y * L + x] = Color.Lerp(escura, clara, Degraus(Mathf.Clamp01(luzAqui), 5));
            }

        if (crateras)
        {
            for (int i = 0; i < 5; i++)
            {
                int ccx = cx + rnd.Next(-r / 2, r / 2), ccy = cy + rnd.Next(-r / 2, r / 2), cr = rnd.Next(2, 4);
                for (int y = ccy - cr; y <= ccy + cr; y++)
                    for (int x = ccx - cr; x <= ccx + cr; x++)
                        if ((x - ccx) * (x - ccx) + (y - ccy) * (y - ccy) <= cr * cr)
                            px[y * L + x] *= 0.78f;
            }
        }

        if (anel)
        {
            float rx = r * 1.9f, ry = r * 0.42f;
            Color corAnel = new Color(0.95f, 0.85f, 0.65f);
            for (int y = cy - (int)ry - 1; y <= cy + (int)ry + 1; y++)
                for (int x = cx - (int)rx - 1; x <= cx + (int)rx + 1; x++)
                {
                    if (x < 0 || y < 0 || x >= L || y >= A) continue;
                    float ex = (x - cx) / rx, ey = (y - cy) / ry;
                    float e = Mathf.Sqrt(ex * ex + ey * ey);
                    if (e < 0.75f || e > 1f) continue;
                    bool dentroDoPlaneta = (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;
                    if (dentroDoPlaneta && y > cy) continue; // parte de trás do anel fica escondida
                    float forca = e > 0.88f ? 0.55f : 0.85f;
                    px[y * L + x] = Color.Lerp(px[y * L + x], corAnel, forca);
                }
        }
    }

    static float Ruido(float x, float y, float semente)
    {
        float soma = 0f, amplitude = 1f, frequencia = 1f, total = 0f;
        for (int i = 0; i < 4; i++)
        {
            soma += Mathf.PerlinNoise(x * frequencia + semente, y * frequencia + semente * 2f) * amplitude;
            total += amplitude;
            amplitude *= 0.5f;
            frequencia *= 2f;
        }
        return soma / total;
    }

    static float Degraus(float valor, int niveis) => Mathf.Floor(valor * niveis) / niveis;

    static Texture2D SalvarTextura(string nome, Texture2D tex)
    {
        string caminho = PASTA_SPRITES + "/" + nome + ".png";
        File.WriteAllBytes(caminho, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(caminho);

        var importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
        importador.textureType = TextureImporterType.Default;
        importador.npotScale = TextureImporterNPOTScale.None;
        importador.filterMode = FilterMode.Point;
        importador.wrapMode = TextureWrapMode.Clamp;
        importador.mipmapEnabled = false;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
    }

    // ------------------------------------------------------------------
    // Interface (uGUI)
    // ------------------------------------------------------------------

    static Font fonte;

    static UIManager CriarInterface(Camera cam, Sprites s)
    {
        // Sistema de eventos: necessário para o mouse funcionar nos cartões do menu.
        var eventos = new GameObject("EventSystem");
        eventos.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventos.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

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
        ui.textoOndaOuTempo = CriarTexto("OndaOuTempo", hud, "", 22, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -55), new Vector2(300, 35), new Color(0.8f, 0.8f, 1f));
        ui.textoPowerUp = CriarTexto("PowerUp", hud, "", 22, TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(400, 35), COR_POWERUP_TIRO);

        // Aviso central (fica fora do HUD para aparecer sempre)
        ui.textoAviso = CriarTexto("Aviso", raiz, "", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(560, 80), new Color(1f, 0.9f, 0.3f));

        // Menu
        var menu = CriarPainel("PainelMenu", raiz, 0.3f);
        CriarMenu(menu, ui, s);
        ui.painelMenu = menu.gameObject;

        // Pausa
        var pausa = CriarPainel("PainelPausa", raiz, 0.6f);
        CriarTexto("Titulo", pausa, "PAUSADO", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(560, 90), Color.white);
        CriarTexto("Dica", pausa, "P / ESC para continuar\nM para voltar ao menu", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(560, 80), new Color(0.85f, 0.85f, 0.95f));
        ui.painelPausa = pausa.gameObject;

        // Game Over
        var gameOver = CriarPainel("PainelGameOver", raiz, 0.65f);
        CriarTexto("Titulo", gameOver, "GAME OVER", 68, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(560, 90), COR_INIMIGO_RETO);
        ui.textoResultadoGameOver = CriarTexto("Resultado", gameOver, "", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(560, 170), Color.white);
        CriarTexto("Dica", gameOver, "ENTER para jogar de novo\nESC para o menu", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -155), new Vector2(560, 80), new Color(1f, 0.9f, 0.3f));
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

    // ------------------------------------------------------------------
    // Menu principal
    // ------------------------------------------------------------------

    static readonly Color COR_TITULO_2 = new Color(1f, 0.82f, 0.3f);
    static readonly Color COR_TEXTO_SUAVE = new Color(0.7f, 0.74f, 0.88f);
    static Sprite spriteCaixa;

    static void CriarMenu(RectTransform menu, UIManager ui, Sprites s)
    {
        spriteCaixa = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Vector2 c = new Vector2(0.5f, 0.5f);
        var anim = menu.gameObject.AddComponent<MenuAnimado>();

        // Naves decorativas
        anim.nave = CriarIcone("NaveMenu", menu, s.nave, COR_JOGADOR, new Vector2(0, 312), new Vector2(64, 64));
        anim.inimigoEsquerda = CriarIcone("InimigoEsquerda", menu, s.inimigoReto, COR_INIMIGO_RETO, new Vector2(-200, 292), new Vector2(52, 39));
        anim.inimigoDireita = CriarIcone("InimigoDireita", menu, s.inimigoZigueZague, COR_INIMIGO_ZIGUEZAGUE, new Vector2(200, 292), new Vector2(56, 35));

        // Título em duas cores, com contorno
        Text t1 = CriarTexto("Titulo1", menu, "DEFENSOR", 78, TextAnchor.MiddleCenter, c, new Vector2(0, 222), new Vector2(580, 90), COR_JOGADOR);
        Text t2 = CriarTexto("Titulo2", menu, "ESTELAR", 78, TextAnchor.MiddleCenter, c, new Vector2(0, 150), new Vector2(580, 90), COR_TITULO_2);
        Contorno(t1, new Color(0.02f, 0.1f, 0.3f));
        Contorno(t2, new Color(0.35f, 0.15f, 0.02f));
        anim.titulos = new[] { t1.rectTransform, t2.rectTransform };

        // Divisória "ESCOLHA O MODO"
        CriarTexto("EscolhaModo", menu, "ESCOLHA O MODO", 18, TextAnchor.MiddleCenter, c, new Vector2(0, 85), new Vector2(250, 30), COR_TEXTO_SUAVE);
        CriarCaixa("LinhaEsquerda", menu, new Color(0.35f, 0.85f, 1f, 0.5f), new Vector2(-185, 85), new Vector2(150, 2), false);
        CriarCaixa("LinhaDireita", menu, new Color(0.35f, 0.85f, 1f, 0.5f), new Vector2(185, 85), new Vector2(150, 2), false);

        // Cartões dos modos
        ui.cartaoOndas = CriarCartao(menu, "CartaoOndas", ModoJogo.Ondas, -138f, s.inimigoReto, COR_INIMIGO_RETO, new Vector2(48, 36),
                                     "ONDAS", "5 ondas de inimigos.\nVença a onda final!", "1");
        ui.cartaoInfinito = CriarCartao(menu, "CartaoInfinito", ModoJogo.Infinito, 138f, s.tanque, COR_TANQUE, new Vector2(62, 36),
                                        "INFINITO", "Inimigos sem parar.\nAté onde você chega?", "2");

        // Chamada para jogar (pisca)
        anim.textoPiscando = CriarTexto("PressioneEnter", menu, "PRESSIONE ENTER PARA JOGAR", 26, TextAnchor.MiddleCenter, c, new Vector2(0, -177), new Vector2(560, 40), COR_TITULO_2);
        CriarTexto("DicaEscolha", menu, "SETAS ou MOUSE para escolher   |   ESC para sair", 15, TextAnchor.MiddleCenter, c, new Vector2(0, -205), new Vector2(560, 24), COR_TEXTO_SUAVE);

        // Caixa de informações: controles e poderes
        var info = CriarCaixa("Informacoes", menu, new Color(0.03f, 0.05f, 0.12f, 0.88f), new Vector2(0, -300), new Vector2(540, 120), true);

        CriarTecla(info, "SETAS / WASD", -200f, 120f, 28f);
        CriarRotulo(info, "mover", -132f, 28f, 70f);
        CriarTecla(info, "ESPAÇO", -10f, 84f, 28f);
        CriarRotulo(info, "atirar", 40f, 28f, 60f);
        CriarTecla(info, "P", 135f, 32f, 28f);
        CriarRotulo(info, "pausar", 159f, 28f, 80f);

        CriarIcone("IconeTiroTriplo", info, s.powerUpTiro, COR_POWERUP_TIRO, new Vector2(-185, -28), new Vector2(26, 26));
        CriarRotulo(info, "Tiro triplo", -165f, -28f, 110f);
        CriarIcone("IconeEscudo", info, s.powerUpEscudo, COR_POWERUP_ESCUDO, new Vector2(-15, -28), new Vector2(26, 26));
        CriarRotulo(info, "Escudo", 5f, -28f, 80f);
        CriarIcone("IconeVida", info, s.powerUpVida, COR_POWERUP_VIDA, new Vector2(125, -28), new Vector2(26, 26));
        CriarRotulo(info, "+1 vida", 145f, -28f, 80f);
    }

    static CartaoModo CriarCartao(Transform pai, string nome, ModoJogo modo, float x, Sprite icone, Color corIcone, Vector2 tamanhoIcone,
                                  string titulo, string descricao, string tecla)
    {
        Vector2 c = new Vector2(0.5f, 0.5f);

        // Borda (imagem de trás) + fundo (imagem da frente, um pouco menor).
        var borda = CriarCaixa(nome, pai, Color.white, new Vector2(x, -40), new Vector2(256, 200), true);
        var fundo = CriarCaixa("Fundo", borda, Color.black, Vector2.zero, Vector2.zero, true);
        Esticar(fundo);
        fundo.offsetMin = new Vector2(3, 3);
        fundo.offsetMax = new Vector2(-3, -3);
        fundo.GetComponent<Image>().raycastTarget = false;

        var cartao = borda.gameObject.AddComponent<CartaoModo>();
        cartao.modo = modo;
        cartao.borda = borda.GetComponent<Image>();
        cartao.fundo = fundo.GetComponent<Image>();
        cartao.icone = CriarIcone("Icone", borda, icone, corIcone, new Vector2(0, 58), tamanhoIcone).GetComponent<Image>();
        cartao.titulo = CriarTexto("Titulo", borda, titulo, 32, TextAnchor.MiddleCenter, c, new Vector2(0, 14), new Vector2(240, 40), Color.white);
        cartao.descricao = CriarTexto("Descricao", borda, descricao, 16, TextAnchor.MiddleCenter, c, new Vector2(0, -30), new Vector2(240, 44), COR_TEXTO_SUAVE);
        cartao.recorde = CriarTexto("Recorde", borda, "", 15, TextAnchor.MiddleCenter, c, new Vector2(0, -76), new Vector2(240, 24), new Color(1f, 0.9f, 0.5f));

        // Tecla de atalho no canto
        var atalho = CriarCaixa("Atalho", borda, new Color(0.85f, 0.88f, 0.95f, 0.9f), new Vector2(-104, 78), new Vector2(28, 28), true);
        atalho.GetComponent<Image>().raycastTarget = false;
        var textoAtalho = CriarTexto("Tecla", atalho, tecla, 18, TextAnchor.MiddleCenter, c, Vector2.zero, new Vector2(28, 28), new Color(0.05f, 0.07f, 0.15f));
        Object.DestroyImmediate(textoAtalho.GetComponent<Shadow>());

        return cartao;
    }

    static void CriarTecla(Transform pai, string texto, float x, float largura, float y)
    {
        var tecla = CriarCaixa("Tecla_" + texto, pai, new Color(0.85f, 0.88f, 0.95f, 0.95f), new Vector2(x, y), new Vector2(largura, 28), true);
        tecla.GetComponent<Image>().raycastTarget = false;
        var t = CriarTexto("Texto", tecla, texto, 14, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(largura, 28), new Color(0.05f, 0.07f, 0.15f));
        t.fontStyle = FontStyle.Bold;
        Object.DestroyImmediate(t.GetComponent<Shadow>());
    }

    /// <summary>Texto alinhado à esquerda começando em x.</summary>
    static void CriarRotulo(Transform pai, string texto, float x, float y, float largura)
    {
        CriarTexto("Rotulo_" + texto, pai, texto, 17, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f),
                   new Vector2(x + largura / 2f, y), new Vector2(largura, 28), Color.white);
    }

    static RectTransform CriarIcone(string nome, Transform pai, Sprite sprite, Color cor, Vector2 posicao, Vector2 tamanho)
    {
        var rt = CriarRect(nome, pai);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return rt;
    }

    static RectTransform CriarCaixa(string nome, Transform pai, Color cor, Vector2 posicao, Vector2 tamanho, bool arredondada)
    {
        var rt = CriarRect(nome, pai);
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        var img = rt.gameObject.AddComponent<Image>();
        if (arredondada)
        {
            img.sprite = spriteCaixa;
            img.type = Image.Type.Sliced;
        }
        img.color = cor;
        img.raycastTarget = arredondada;
        return rt;
    }

    static void Contorno(Text texto, Color cor)
    {
        var contorno = texto.gameObject.AddComponent<Outline>();
        contorno.effectColor = cor;
        contorno.effectDistance = new Vector2(3f, -3f);
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
        t.raycastTarget = false;

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
