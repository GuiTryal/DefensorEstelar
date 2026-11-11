using UnityEngine;

/// <summary>
/// Posiciona a moldura brilhante em volta da área de jogo.
/// A área de jogo muda conforme o tamanho da janela (veja ProporcaoTela),
/// então a moldura acompanha o retângulo da câmera principal.
/// </summary>
public class MolduraTela : MonoBehaviour
{
    public Camera cameraDoJogo;
    public RectTransform[] camadas;   // cada camada é um pouco maior que a área de jogo
    public float[] espessuras;        // em pixels

    void LateUpdate()
    {
        Rect r = cameraDoJogo.rect;
        for (int i = 0; i < camadas.Length; i++)
        {
            camadas[i].anchorMin = r.min;
            camadas[i].anchorMax = r.max;
            camadas[i].offsetMin = new Vector2(-espessuras[i], -espessuras[i]);
            camadas[i].offsetMax = new Vector2(espessuras[i], espessuras[i]);
        }
    }
}
