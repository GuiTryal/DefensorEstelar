using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Facilita abrir o projeto na Unity:
/// - se o editor abrir numa cena vazia ("Untitled"), abre a cena do jogo automaticamente;
/// - o botão Play sempre começa pela cena do jogo.
/// </summary>
[InitializeOnLoad]
public static class AbrirCenaPrincipal
{
    const string CENA_PRINCIPAL = "Assets/Scenes/Jogo.unity";

    static AbrirCenaPrincipal()
    {
        // Espera o editor terminar de carregar antes de mexer nas cenas.
        EditorApplication.delayCall += Abrir;
    }

    static void Abrir()
    {
        if (Application.isBatchMode) return;

        var cena = AssetDatabase.LoadAssetAtPath<SceneAsset>(CENA_PRINCIPAL);
        if (cena == null) return;

        // O Play sempre começa pela cena do jogo.
        EditorSceneManager.playModeStartScene = cena;

        // Só troca de cena se a atual estiver vazia e sem nada para salvar.
        var atual = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(atual.path) && !atual.isDirty && !EditorApplication.isPlayingOrWillChangePlaymode)
            EditorSceneManager.OpenScene(CENA_PRINCIPAL);
    }
}
