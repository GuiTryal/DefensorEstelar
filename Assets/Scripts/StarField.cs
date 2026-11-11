using UnityEngine;

/// <summary>
/// Fundo de estrelas com efeito parallax: estrelas maiores passam mais rápido,
/// dando sensação de profundidade e de que a nave está avançando.
/// </summary>
public class StarField : MonoBehaviour
{
    public Sprite spriteEstrela;
    public int quantidade = 70;
    public float velocidadeMinima = 0.5f;
    public float velocidadeMaxima = 3f;

    Transform[] estrelas;
    float[] velocidades;
    float meiaAltura, meiaLargura;

    void Start()
    {
        Camera cam = Camera.main;
        meiaAltura = cam.orthographicSize + 0.5f;
        meiaLargura = cam.orthographicSize * cam.aspect + 0.5f;

        estrelas = new Transform[quantidade];
        velocidades = new float[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            GameObject go = new GameObject("Estrela");
            go.transform.SetParent(transform);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spriteEstrela;
            sr.sortingOrder = -10;

            float profundidade = Random.value; // 0 = longe, 1 = perto
            go.transform.localScale = Vector3.one * Mathf.Lerp(1f, 2.5f, profundidade);
            sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.25f, 0.9f, profundidade));
            velocidades[i] = Mathf.Lerp(velocidadeMinima, velocidadeMaxima, profundidade);

            go.transform.position = new Vector3(Random.Range(-meiaLargura, meiaLargura), Random.Range(-meiaAltura, meiaAltura), 0f);
            estrelas[i] = go.transform;
        }
    }

    void Update()
    {
        for (int i = 0; i < estrelas.Length; i++)
        {
            Vector3 p = estrelas[i].position;
            p.y -= velocidades[i] * Time.deltaTime;
            if (p.y < -meiaAltura)
            {
                p.y = meiaAltura;
                p.x = Random.Range(-meiaLargura, meiaLargura);
            }
            estrelas[i].position = p;
        }
    }
}
