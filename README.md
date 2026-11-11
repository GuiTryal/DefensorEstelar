# Defensor Estelar

Jogo 2D de nave (shooter vertical) feito na **Unity 2022.3** para o trabalho individual de Desenvolvimento de Jogos Digitais.

Você controla uma nave e enfrenta inimigos que descem pela tela. Destrua os inimigos para ganhar pontos e pegue os itens que eles soltam. Se perder as 3 vidas, é Game Over.

## Modos de jogo

- **Modo Ondas:** 5 ondas de inimigos, cada uma mais difícil que a anterior. Vencer a última onda dá a vitória.
- **Modo Infinito:** os inimigos não param de chegar. A dificuldade aumenta com o tempo (mais inimigos, mais rápidos e mais atiradores) até 2min30s e depois fica no nível máximo. A tela mostra a pontuação e um cronômetro.

Cada modo tem o próprio recorde. No Infinito também fica salvo o maior tempo sobrevivido.

## Controles

| Tecla | Ação |
|---|---|
| Setas (no menu) | Escolher o modo |
| 1 / 2 (no menu) | Começar direto no modo Ondas / Infinito |
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
- **Tanque laranja:** desce, para no alto da tela, patrulha de um lado para o outro atirando em leque (3 tiros) por 7 segundos e depois vai embora. 8 de vida, vale 500 pontos e sempre solta um poder. Tem uma máquina de estados própria (`FaseTanque`: Entrando, Atacando, Saindo).
- **Item amarelo:** tiro triplo por 6 segundos (atira um pouco mais devagar enquanto ativo).
- **Item azul:** escudo por 10 segundos, que absorve um golpe.
- **Item verde:** +1 vida (máximo de 5).

## Conceitos aplicados

| Conceito | Onde está |
|---|---|
| Entrada do jogador | `PlayerController` (eixos Horizontal/Vertical e teclas de tiro), `GameManager` (Enter, P, Esc) |
| Movimentação com física | Nave com `Rigidbody2D` dinâmico (gravidade 0) movida pela velocidade em `FixedUpdate` |
| Movimentação com Transform | Tiros, inimigos, itens e estrelas do fundo (`transform.position`) |
| Colisões e interações | Triggers 2D: tiro × inimigo, tiro inimigo × nave, inimigo × nave, item × nave |
| Pontuação, vidas, vitória e derrota | `GameManager`: pontos, 3 vidas, recorde salvo com `PlayerPrefs`, vitória ao vencer as 5 ondas |
| Estados e comportamentos | Máquina de estados `EstadoJogo` (Menu, Jogando, Pausado, GameOver, Vitória), modos `ModoJogo` (Ondas, Infinito) e tipos de inimigo (`TipoInimigo`) |
| Geração de inimigos | `EnemySpawner` com coroutines: ondas configuráveis no Inspector e modo infinito com curva de dificuldade (`Mathf.Lerp` até um teto) |
| Interface e feedback | HUD com pontos, vidas e onda ou cronômetro, avisos na tela, nave piscando quando invencível, inimigo piscando ao levar dano, explosões, câmera tremendo e efeitos sonoros |
| Resolução | `ProporcaoTela` mantém a área de jogo em 3:4 em qualquer janela. As laterais mostram um cenário espacial com moldura (`MolduraTela`) |

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
    ProporcaoTela.cs     mantém a proporção da área de jogo
    MolduraTela.cs       moldura em volta da área de jogo
  Editor/
    MontadorDoJogo.cs    monta cena, sprites e prefabs automaticamente
  Sprites/  Prefabs/  Scenes/
```

Toda a arte (pixel art) e os sons são gerados por código, sem assets externos.
