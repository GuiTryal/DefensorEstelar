using UnityEngine;

/// <summary>
/// Mantém a área de jogo sempre na mesma proporção (ex.: 3:4), qualquer que seja
/// o tamanho da janela. Se a tela for mais larga, aparecem faixas pretas nas laterais;
/// se for mais alta, faixas em cima e embaixo. Assim o jogo fica igual em qualquer monitor.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ProporcaoTela : MonoBehaviour
{
    [Tooltip("Largura dividida pela altura da área de jogo. 0.75 = 3:4 (em pé).")]
    public float proporcaoAlvo = 0.75f;

    Camera cam;
    int larguraAnterior, alturaAnterior;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Ajustar();
    }

    void Update()
    {
        // Só recalcula quando o tamanho da janela muda (ex.: maximizar, tela cheia).
        if (Screen.width != larguraAnterior || Screen.height != alturaAnterior) Ajustar();
    }

    void Ajustar()
    {
        larguraAnterior = Screen.width;
        alturaAnterior = Screen.height;

        float proporcaoTela = (float)Screen.width / Screen.height;
        Rect r = new Rect(0f, 0f, 1f, 1f);

        if (proporcaoTela > proporcaoAlvo)
        {
            // Tela mais larga: faixas nas laterais.
            r.width = proporcaoAlvo / proporcaoTela;
            r.x = (1f - r.width) / 2f;
        }
        else
        {
            // Tela mais alta: faixas em cima e embaixo.
            r.height = proporcaoTela / proporcaoAlvo;
            r.y = (1f - r.height) / 2f;
        }

        cam.rect = r;
    }
}
