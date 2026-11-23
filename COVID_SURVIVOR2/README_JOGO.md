# COVID SURVIVOR — Como abrir e testar

**Projeto:** Unity 6.5.8f1 (6000.5.8f1), Universal 2D/URP, PC.

## Execução

1. Extraia o ZIP inteiro e abra a pasta `COVID_SURVIVOR` no Unity Hub. Não importe somente o conteúdo de `Assets`: mantenha `Packages` e `ProjectSettings`.
2. Aguarde a primeira importação e confira a janela **Console**.
3. Abra `Assets/Scenes/MainMenu.unity` e pressione **Play**. Essa também é a primeira cena configurada no Build Settings.
4. Clique em **JOGAR**. Use **WASD** ou as **setas** para se mover. O médico começa **automaticamente disparando vacinas** no vírus vivo mais próximo dentro do alcance (6 unidades); nenhum elemento é necessário para o ataque inicial. Colete cristais de XP; ao subir de nível, escolha uma entre quatro melhorias. A primeira opção elemental escolhida desbloqueia uma segunda forma de ataque automático que funciona junto com a vacina.
5. Sobreviva, receba novas ondas e melhorias e continue até o médico morrer. A tela de derrota permite **REINICIAR** ou ir ao **MENU PRINCIPAL**. Não há tela de vitória.

## Regras da partida

- **Vacina básica desde o começo**: `VaccineSpell` busca o inimigo vivo mais próximo a até 6 unidades e atira automaticamente a cada 0,75 s (antes dos bônus). Um tiro causa 10 pontos de dano-base e usa `VaccineProjectile.prefab`.
- **Magias elementais adicionais**: `ElementalSpellController` continua bloqueado até a primeira escolha de elemento, quando começa a atacar em paralelo com a vacina. Elementos subsequentes transformam essa magia conforme as receitas existentes; não removem a vacina.
- **Atributos gerais** (`PlayerStats`) de dano e cadência de ataque afetam **ambas** as armas. Melhoria específica da magia altera **apenas** o sistema elemental. Movimentação e alcance de coleta continuam atributos globais.
- **Feedback de combate sem excesso:** antes do primeiro elemento, somente a vacina aciona `Doctor/Attack` e `shoot`. Depois de desbloquear magia elemental, a vacina continua causando dano e lançando seus projéteis, mas deixa de acionar som/animação. A magia elemental passa a comandar o `Doctor/Attack`, com intervalo visual mínimo de 1,05 s (0,68 s para a vacina inicial) e SFX `element_cast` limitado a um toque a cada 0,9 s, independentemente de quantos projéteis forem criados. Magias persistentes só produzem esse feedback sonoro/visual quando houver um vírus ao alcance. O ataque-base continua mirando automaticamente mesmo com movimento.
- Pontuação: **100 pontos por vírus eliminado + 1 ponto por segundo completo de sobrevivência**.
- O relógio é pausado durante a escolha de melhorias, como o restante do jogo.
- O spawn acelera e os inimigos ganham vida com as ondas, conforme as configurações já existentes no `VirusSpawner`/`WaveManager`.
- **Áudio original:** música de menu, jogo e derrota em loop; efeitos de clique, tiro, impacto,
  morte de vírus, dano no médico, coleta de XP, evolução e derrota.
- A música muda automaticamente com as cenas e os áudios são reproduzidos por
  `CovidAudioManager`, persistente entre cenas, sem necessidade de configurar áudio no Inspector.
- Os menus recebem um `AudioListener` em tempo de execução; a cena de jogo já contém o Listener da câmera.

## Estrutura explicável em aula

- `Assets/Scripts/Gameplay/Characters/Player.cs`: movimento, vida, escolha entre Idle/Walk/Attack/Death; `PlayAttackAnimation` não calcula dano.
- `Assets/Scripts/Gameplay/Animation/SpriteAnimator.cs`: carrega em ordem os quadros das pastas `Resources` e avança pelo `Time.deltaTime`.
- `Assets/Scripts/Gameplay/Spells/BaseSpell.cs`: procura alvos, administra intervalo e aplica melhorias gerais de dano/cadência ao ataque-base.
- `Assets/Scripts/Gameplay/Spells/VaccineSpell.cs`: arma inicial permanente; dispara prefabs de vacina. Solicita animação/SFX só antes do primeiro desbloqueio elemental.
- `Assets/Scripts/Gameplay/Spells/VaccineProjectile.cs`: movimenta o projétil, acerta vírus e aplica dano.
- `Assets/Scripts/Gameplay/Elements/ElementalSpellController.cs`: sistema de receitas e magia; após desbloqueio passa a comandar o feedback visual e sonoro de ataque, sem tocar som por projétil individual.
- `Assets/Scripts/Gameplay/WaveManager.cs`: controla a onda e conta eliminações confirmadas para o placar.
- `Assets/Scripts/Gameplay/GameRunManager.cs`: guarda tempo, calcula pontos, apresenta o HUD e observa `Player.Died`.
- `Assets/Scripts/Gameplay/GameRunState.cs`: guarda os dados da última partida somente entre cenas do aplicativo.
- `Assets/Scripts/UI/SceneUIFactory.cs`: constrói textos, imagens, botões, Canvas e EventSystem via código, sem dependências novas.
- `Assets/Scripts/UI/MainMenuUI.cs` e `Assets/Scripts/UI/GameOverUI.cs`: comportamento dos dois novos menus.
- `Assets/Scripts/Audio/CovidAudioManager.cs`: carrega os WAVs de `Resources/Audio`, toca três músicas
  em loop, efeitos únicos e limita a repetição de disparos/impactos. Os efeitos de dano/morte/coleta foram suavizados e têm limites próprios de repetição.

## Configuração no Inspector

- `MainMenu.unity`: contém o GameObject `MainMenuController`, com `MainMenuUI`. O Canvas aparece **em tempo de execução**.
- `SampleScene.unity`: o GameObject `VirusSpawner` contém `GameRunManager`. O campo `m_player` aponta para o objeto `Player` da cena, `m_waveManager` para o `WaveManager` do próprio `VirusSpawner`, e `m_defeatTransitionDelay` vale 0,85 segundo.
- `GameOver.unity`: contém o GameObject `GameOverController`, com `GameOverUI`.
- O componente `VaccineSpell` está **habilitado** (`m_Enabled: 1`) no objeto `Player` da cena `SampleScene`; seus campos `m_player` e `m_projectilePrefab` apontam respectivamente para o Player e o prefab `VaccineProjectile`. O ataque básico funciona sem escolhas elementais.
- No mesmo objeto, `PlayerStats` e `ElementalSpellController` permanecem habilitados e independentes. O primeiro elemento desbloqueia a magia sem desligar a arma-base.
- Todos os três arquivos estão na ordem correta em **File > Build Profiles > Scene List** (ou Build Settings, conforme a interface da sua versão).
- Os oito quadros novos ficam em `Assets/Resources/Characters/Doctor/Attack`, com Filter Mode **Point** e o mesmo Pixels Per Unit das outras animações. Não é necessário criar Animation Controllers: `SpriteAnimator` executa o flipbook por script.

## Testes obrigatórios antes da entrega escolar

Ver `DEVELOPMENT_STATUS.md` e `TESTES_NO_UNITY.md` para o roteiro detalhado de testes. As verificações estáticas anteriores foram registradas no histórico do status, mas os relatórios temporários foram removidos com o auditor. **A compilação no Unity, Play Mode e build desktop ainda não foram executados aqui.**
As verificações estáticas da entrega anterior não substituem a validação na engine; a arma inicial e o script foram modificados depois delas.

## Validação pendente no Unity

A ferramenta personalizada de auditoria do Editor foi removida a pedido do usuário. Não existe menu `Tools > COVID SURVIVOR > Auditar projeto` neste pacote.

Abra **Window > General > Console** para conferir erros da importação e execute os testes de `TESTES_NO_UNITY.md` em Play Mode. Os testes automáticos de arquivos realizados fora do Unity não comprovam colisões, animações, efeitos elementais ou áudio em execução; isso precisa ser validado no editor e em um build para Windows.


### Visual atual do médico

A partir de 23/11/2026, o personagem usa a arte fiel à nova referência
(cabelo branco volumoso, olhos azuis, jaleco e uniforme azuis). Os quadros
estão prontos em `Assets/Resources/Characters/Doctor/{Idle,Walk,Attack,Death}`;
**não é necessário importar uma spritesheet manualmente**. O projeto emprega
`SpriteAnimator` próprio, e não um Animator Controller Unity para esse personagem.
A arte-fonte está em `References/Art/doctor_animation_source.png` e a cena
`SampleScene` referencia o sprite inicial atualizado.


### Nova família de vírus (versão para teste)
O vírus comum é preto com faixas brancas. Os **corredores** surgem desde a onda 2, com menos vida e maior velocidade; os **tanques** desde a onda 4, com mais vida e menor velocidade. O **Colosso Blindado** surge uma vez nas ondas 5, 15, 25..., enquanto o **Núcleo Mutante** surge nas ondas 10, 20, 30... O boss tem espinhos curtos, núcleo violeta, ataque em pulso após aviso visual e morte por desintegração. Todos usam prefabs próprios e animações via SpriteAnimator. A evolução das ondas segue infinita.
