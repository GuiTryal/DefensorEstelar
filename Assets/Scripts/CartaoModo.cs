using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Cartão de escolha de modo no menu. Fica em destaque quando selecionado
/// e também responde ao mouse (passar por cima seleciona, clicar começa).
/// </summary>
public class CartaoModo : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public ModoJogo modo;

    [Header("Partes do cartão")]
    public Image borda;
    public Image fundo;
    public Image icone;
    public Text titulo;
    public Text descricao;
    public Text recorde;

    static readonly Color BORDA_ATIVA = new Color(0.35f, 0.85f, 1f, 1f);
    static readonly Color BORDA_INATIVA = new Color(0.25f, 0.28f, 0.4f, 1f);
    static readonly Color FUNDO_ATIVO = new Color(0.07f, 0.14f, 0.28f, 0.95f);
    static readonly Color FUNDO_INATIVO = new Color(0.04f, 0.05f, 0.11f, 0.9f);
    static readonly Color TITULO_ATIVO = new Color(1f, 0.85f, 0.3f);
    static readonly Color TITULO_INATIVO = new Color(0.6f, 0.62f, 0.72f);

    bool selecionado;
    Color corIcone;

    void Awake() { corIcone = icone.color; }

    public void Destacar(bool ativo)
    {
        selecionado = ativo;
        borda.color = ativo ? BORDA_ATIVA : BORDA_INATIVA;
        fundo.color = ativo ? FUNDO_ATIVO : FUNDO_INATIVO;
        titulo.color = ativo ? TITULO_ATIVO : TITULO_INATIVO;
        Color c = corIcone;
        c.a = ativo ? 1f : 0.45f;
        icone.color = c;
    }

    void Update()
    {
        // O cartão selecionado fica um pouco maior e "respira".
        float alvo = selecionado ? 1.06f + Mathf.Sin(Time.unscaledTime * 4f) * 0.015f : 0.97f;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * alvo, 12f * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData dados) => GameManager.Instancia.SelecionarModo(modo);

    public void OnPointerClick(PointerEventData dados) => GameManager.Instancia.IniciarPartida(modo);
}
