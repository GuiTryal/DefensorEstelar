using UnityEngine;

public enum TipoPowerUp { TiroTriplo, VidaExtra }

/// <summary>
/// Item que cai de inimigos destruídos. Ao encostar na nave, dá um bônus.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public TipoPowerUp tipo = TipoPowerUp.TiroTriplo;
    public float velocidade = 1.5f;
    public float duracaoTiroTriplo = 8f;

    Vector3 escalaOriginal;

    void Start() { escalaOriginal = transform.localScale; }

    void Update()
    {
        transform.position += Vector3.down * velocidade * Time.deltaTime;

        // Feedback visual: "pulsa" para chamar atenção.
        float pulso = 1f + Mathf.Sin(Time.time * 8f) * 0.12f;
        transform.localScale = escalaOriginal * pulso;

        if (transform.position.y < -Camera.main.orthographicSize - 1f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        PlayerController jogador = outro.GetComponent<PlayerController>();
        if (jogador == null) return;

        if (tipo == TipoPowerUp.TiroTriplo)
        {
            jogador.AtivarTiroTriplo(duracaoTiroTriplo);
            GameManager.Instancia.ui.MostrarAviso("TIRO TRIPLO!");
        }
        else
        {
            GameManager.Instancia.GanharVida();
            GameManager.Instancia.ui.MostrarAviso("+1 VIDA!");
        }

        AudioManager.Tocar(Som.PowerUp);
        Destroy(gameObject);
    }
}
