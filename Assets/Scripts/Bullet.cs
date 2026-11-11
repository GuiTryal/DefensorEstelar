using UnityEngine;

/// <summary>
/// Projétil (do jogador ou do inimigo). Move-se pelo Transform
/// e trata a colisão com o alvo usando Trigger.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float velocidade = 14f;
    public bool doJogador = true;
    public int dano = 1;
    public Vector2 direcao = Vector2.up;

    /// <summary>Define a direção e gira o sprite para "olhar" para onde vai.</summary>
    public void Configurar(Vector2 novaDirecao)
    {
        direcao = novaDirecao.normalized;
        transform.up = direcao;
    }

    void Update()
    {
        // MOVIMENTO PELO TRANSFORM: posição += direção * velocidade * tempo.
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);

        // Saiu da tela? Destrói para não acumular objetos.
        Vector3 p = transform.position;
        if (Mathf.Abs(p.y) > 8f || Mathf.Abs(p.x) > 10f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (doJogador)
        {
            Enemy inimigo = outro.GetComponent<Enemy>();
            if (inimigo != null)
            {
                inimigo.LevarDano(dano);
                Destroy(gameObject);
            }
        }
        else
        {
            PlayerController jogador = outro.GetComponent<PlayerController>();
            if (jogador != null)
            {
                jogador.LevarDano();
                Destroy(gameObject);
            }
        }
    }
}
