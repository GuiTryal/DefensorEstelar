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
/// Cria os inimigos em ondas. Quando todas as ondas são derrotadas, o jogador vence.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public Enemy prefabReto;
    public Enemy prefabZigueZague;
    public Onda[] ondas;
    public float pausaEntreOndas = 2.5f;

    public int OndaAtual { get; private set; }
    public int TotalOndas => ondas.Length;

    public void Iniciar()
    {
        StopAllCoroutines();
        StartCoroutine(RodarOndas());
    }

    bool Jogando => GameManager.Instancia.Estado == EstadoJogo.Jogando;

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
