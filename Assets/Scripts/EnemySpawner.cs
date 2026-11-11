using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Configuração de uma onda de inimigos (editável no Inspector).</summary>
[System.Serializable]
public class Onda
{
    public int quantidadeRetos = 5;
    public int quantidadeZigueZague = 0;
    public float intervaloEntreInimigos = 0.8f;
    public float velocidadeExtra = 0f;
}

/// <summary>
/// Cria os inimigos.
/// - Modo Ondas: 5 ondas definidas no Inspector; vencer todas = vitória.
/// - Modo Infinito: inimigos sem parar, com dificuldade que cresce até um limite e depois se mantém.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public Enemy prefabReto;
    public Enemy prefabZigueZague;

    [Header("Modo Ondas")]
    public Onda[] ondas;
    public float pausaEntreOndas = 2.5f;

    [Header("Modo Infinito")]
    [Tooltip("Segundos até chegar na dificuldade máxima. Depois disso a dificuldade para de subir.")]
    public float tempoAteDificuldadeMaxima = 150f;
    public float intervaloInicial = 1.1f;
    public float intervaloMinimo = 0.32f;
    public float velocidadeExtraMaxima = 1.6f;
    [Range(0f, 1f)] public float chanceZigueZagueInicial = 0.08f;
    [Range(0f, 1f)] public float chanceZigueZagueMaxima = 0.45f;

    public int OndaAtual { get; private set; }
    public int TotalOndas => ondas.Length;

    /// <summary>Dificuldade atual do modo infinito, de 0 (início) a 1 (máxima).</summary>
    public float Dificuldade { get; private set; }

    public void Iniciar(ModoJogo modo)
    {
        StopAllCoroutines();
        OndaAtual = 0;
        Dificuldade = 0f;
        StartCoroutine(modo == ModoJogo.Ondas ? RodarOndas() : RodarInfinito());
    }

    bool Jogando => GameManager.Instancia.Estado == EstadoJogo.Jogando;

    // ------------------------------------------------------------------
    // Modo Ondas
    // ------------------------------------------------------------------

    IEnumerator RodarOndas()
    {
        for (int i = 0; i < ondas.Length; i++)
        {
            OndaAtual = i + 1;
            GameManager.Instancia.ui.AtualizarHUD();
            GameManager.Instancia.ui.MostrarAviso(i == ondas.Length - 1 ? "ONDA FINAL!" : "ONDA " + OndaAtual);
            yield return new WaitForSeconds(pausaEntreOndas);

            // Monta a lista de inimigos da onda e embaralha.
            Onda onda = ondas[i];
            List<Enemy> fila = new List<Enemy>();
            for (int r = 0; r < onda.quantidadeRetos; r++) fila.Add(prefabReto);
            for (int z = 0; z < onda.quantidadeZigueZague; z++) fila.Add(prefabZigueZague);
            Embaralhar(fila);

            foreach (Enemy prefab in fila)
            {
                if (!Jogando) yield break;
                Criar(prefab, onda.velocidadeExtra);
                yield return new WaitForSeconds(onda.intervaloEntreInimigos);
            }

            // Espera todos os inimigos da onda serem destruídos (ou saírem da tela).
            while (Enemy.Vivos > 0)
            {
                if (!Jogando) yield break;
                yield return null;
            }
        }

        GameManager.Instancia.Vitoria();
    }

    // ------------------------------------------------------------------
    // Modo Infinito
    // ------------------------------------------------------------------

    IEnumerator RodarInfinito()
    {
        GameManager.Instancia.ui.MostrarAviso("SOBREVIVA!");
        yield return new WaitForSeconds(1.5f);

        bool avisouMaxima = false;

        while (Jogando)
        {
            // A dificuldade cresce com o tempo de partida e para em 1 (máxima).
            Dificuldade = Mathf.Clamp01(GameManager.Instancia.TempoDePartida / tempoAteDificuldadeMaxima);

            if (Dificuldade >= 1f && !avisouMaxima)
            {
                avisouMaxima = true;
                GameManager.Instancia.ui.MostrarAviso("DIFICULDADE MÁXIMA!");
            }

            float chanceZigueZague = Mathf.Lerp(chanceZigueZagueInicial, chanceZigueZagueMaxima, Dificuldade);
            Enemy prefab = Random.value < chanceZigueZague ? prefabZigueZague : prefabReto;
            Criar(prefab, Mathf.Lerp(0f, velocidadeExtraMaxima, Dificuldade));

            // Intervalo com uma pequena variação aleatória para não ficar previsível.
            float intervalo = Mathf.Lerp(intervaloInicial, intervaloMinimo, Dificuldade) * Random.Range(0.8f, 1.2f);
            yield return new WaitForSeconds(intervalo);
        }
    }

    // ------------------------------------------------------------------

    void Criar(Enemy prefab, float velocidadeExtra)
    {
        Camera cam = Camera.main;
        float meiaLargura = cam.orthographicSize * cam.aspect;
        float margem = prefab.tipo == TipoInimigo.ZigueZague ? prefab.amplitude + 0.5f : 0.6f;
        float limiteX = Mathf.Max(0f, meiaLargura - margem);

        Vector3 posicao = new Vector3(Random.Range(-limiteX, limiteX), cam.orthographicSize + 1f, 0f);
        Enemy inimigo = Instantiate(prefab, posicao, Quaternion.identity);
        inimigo.velocidade += velocidadeExtra;
    }

    static void Embaralhar<T>(List<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }
}
