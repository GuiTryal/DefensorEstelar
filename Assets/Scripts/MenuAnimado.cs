using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pequenas animações do menu: naves flutuando, título balançando
/// e o texto "pressione ENTER" piscando.
/// </summary>
public class MenuAnimado : MonoBehaviour
{
    public RectTransform nave;
    public RectTransform inimigoEsquerda;
    public RectTransform inimigoDireita;
    public RectTransform[] titulos;
    public Text textoPiscando;

    Vector2 posNave, posEsquerda, posDireita;
    Vector2[] posTitulos;

    void Awake()
    {
        posNave = nave.anchoredPosition;
        posEsquerda = inimigoEsquerda.anchoredPosition;
        posDireita = inimigoDireita.anchoredPosition;
        posTitulos = new Vector2[titulos.Length];
        for (int i = 0; i < titulos.Length; i++) posTitulos[i] = titulos[i].anchoredPosition;
    }

    void Update()
    {
        float t = Time.unscaledTime;

        nave.anchoredPosition = posNave + new Vector2(0f, Mathf.Sin(t * 2f) * 6f);
        inimigoEsquerda.anchoredPosition = posEsquerda + new Vector2(Mathf.Sin(t * 1.4f) * 8f, Mathf.Cos(t * 1.4f) * 4f);
        inimigoDireita.anchoredPosition = posDireita + new Vector2(Mathf.Sin(t * 1.4f + 2f) * 8f, Mathf.Cos(t * 1.4f + 2f) * 4f);

        for (int i = 0; i < titulos.Length; i++)
            titulos[i].anchoredPosition = posTitulos[i] + new Vector2(0f, Mathf.Sin(t * 1.5f + i * 0.6f) * 3f);

        Color c = textoPiscando.color;
        c.a = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(t * 4f));
        textoPiscando.color = c;
    }
}
