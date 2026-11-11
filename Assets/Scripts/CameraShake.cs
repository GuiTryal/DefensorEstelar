using UnityEngine;

/// <summary>
/// Faz a câmera tremer (feedback de impacto). Fica na Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    static CameraShake instancia;

    Vector3 posicaoOriginal;
    float fimTremor;
    float intensidade;

    void Awake()
    {
        instancia = this;
        posicaoOriginal = transform.localPosition;
    }

    public static void Tremer(float duracao, float forca)
    {
        if (instancia == null) return;
        instancia.fimTremor = Mathf.Max(instancia.fimTremor, Time.time + duracao);
        instancia.intensidade = Mathf.Max(instancia.intensidade, forca);
    }

    void LateUpdate()
    {
        if (Time.time < fimTremor)
        {
            transform.localPosition = posicaoOriginal + (Vector3)(Random.insideUnitCircle * intensidade);
        }
        else
        {
            transform.localPosition = posicaoOriginal;
            intensidade = 0f;
        }
    }
}
