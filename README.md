# Defensor Estelar

Jogo 2D de nave (shooter vertical) feito na **Unity 2022.3** para o trabalho individual de Desenvolvimento de Jogos Digitais.

Você controla uma nave que precisa sobreviver a **5 ondas de inimigos**. Destrua os inimigos para ganhar pontos e pegue os itens que eles soltam. Se perder as 3 vidas, é Game Over.

## Controles

| Tecla | Ação |
|---|---|
| Setas / WASD | Mover a nave |
| Espaço / Z (segurar) | Atirar |
| P / Esc | Pausar / continuar |
| Enter | Começar / jogar de novo |
| M (na pausa) / Esc (no fim) | Voltar ao menu |

## Como rodar

1. Abra a pasta do projeto no **Unity Hub** (versão 2022.3).
2. Abra a cena `Assets/Scenes/Jogo.unity`.
3. Aperte **Play**.

Para gerar o executável, use o menu **Defensor Estelar > Gerar Executável (Windows)**. O jogo fica em `Build/DefensorEstelar.exe`.

O menu **Defensor Estelar > Montar Jogo** recria a cena, os sprites e os prefabs do zero.

## Elementos do jogo

- **Inimigo vermelho:** desce reto e rápido. 1 de vida, vale 100 pontos.
- **Inimigo roxo:** desce em zigue-zague e atira na direção da nave. 3 de vida, vale 250 pontos.
- **Item amarelo:** tiro triplo por 8 segundos.
- **Item verde:** +1 vida (máximo de 5).

## Conceitos aplicados

| Conceito | Onde está |
|---|---|
| Entrada do jogador | `PlayerController` (eixos Horizontal/Vertical e teclas de tiro), `GameManager` (Enter, P, Esc) |
| Movimentação com física | Nave com `Rigidbody2D` dinâmico (gravidade 0) movida pela velocidade em `FixedUpdate` |
| Movimentação com Transform | Tiros, inimigos, itens e estrelas do fundo (`transform.position`) |
| Colisões e interações | Triggers 2D: tiro × inimigo, tiro inimigo × nave, inimigo × nave, item × nave |
| Pontuação, vidas, vitória e derrota | `GameManager`: pontos, 3 vidas, recorde salvo com `PlayerPrefs`, vitória ao vencer as 5 ondas |
| Estados e comportamentos | Máquina de estados `EstadoJogo` (Menu, Jogando, Pausado, GameOver, Vitória) e tipos de inimigo (`TipoInimigo`) |
| Ondas de inimigos | `EnemySpawner`, que usa coroutines e tem as ondas configuráveis no Inspector |
| Interface e feedback | HUD, avisos na tela, nave piscando quando invencível, inimigo piscando ao levar dano, explosões, câmera tremendo e efeitos sonoros |

## Estrutura

```
Assets/
  Scripts/
    GameManager.cs       estados, pontos, vidas, recorde
    PlayerController.cs  nave do jogador
    Enemy.cs             inimigos (reto e zigue-zague)
    EnemySpawner.cs      ondas de inimigos
    Bullet.cs            tiros
    PowerUp.cs           itens
    UIManager.cs         interface
    AudioManager.cs      sons gerados por código
    CameraShake.cs       tremor da câmera
    Explosion.cs         efeito de explosão
    StarField.cs         fundo de estrelas com parallax
  Editor/
    MontadorDoJogo.cs    monta cena, sprites e prefabs automaticamente
  Sprites/  Prefabs/  Scenes/
```

Toda a arte (pixel art) e os sons são gerados por código, sem assets externos.
