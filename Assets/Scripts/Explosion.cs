using UnityEngine;

/// <summary>
/// Efeito de explosão simples: um círculo que cresce e desaparece.
/// </summary>
public class Explosion : MonoBehaviour
{
    public float duracao = 0.35f;

    SpriteRenderer sr;
    Color cor = Color.white;
    float tamanhoFinal = 1.6f;
    float tempo;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    public void Configurar(Color novaCor, float tamanho)
    {
        cor = novaCor;
        tamanhoFinal = tamanho;
    }

    void Update()
    {
        tempo += Time.deltaTime;
        float t = Mathf.Clamp01(tempo / duracao);

        transform.localScale = Vector3.one * Mathf.Lerp(0.3f, tamanhoFinal, t);
        Color c = Color.Lerp(Color.white, cor, t);
        c.a = 1f - t;
        sr.color = c;

        if (t >= 1f) Destroy(gameObject);
    }
}
