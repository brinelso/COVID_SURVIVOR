# COVID SURVIVOR — Estado de desenvolvimento

Atualizado em 23/11/2026. Consulte também `PROJECT_CONTEXT.md` para a visão geral e `AGENTS.md` para as regras de trabalho.

## Projeto e ambiente

- Jogo acadêmico de sobrevivência em arena 2D top-down para PC, protagonizado por um médico que enfrenta criaturas inspiradas em vírus.
- Unity 6.5.8f1 (6000.5.8f1), template Universal 2D (URP), C#.
- Prioridade: código simples, compreensível e explicável pelos integrantes ao professor.

## Estado atual

O projeto já contém movimentação, vida, inimigos perseguidores, spawn, combate, coleta e progressão de XP, ondas, melhorias e HUD. O sistema elemental foi integrado ao fluxo de escolhas por level-up. Há 30 receitas com comportamentos próprios configurados: as 20 anteriores, Tsunami, Pântano Profundo, Tempestade Glacial, Planta, Colosso e as cinco evoluções descritas no final deste arquivo. As evoluções recentes e o ajuste da Tempestade Glacial ainda precisam de validação no Play Mode.

O botão temporário `DEBUG — Subir nível`, descrito nas etapas anteriores, foi removido na atualização de ataque e cenas. O level-up continua pelo fluxo normal de experiência e quatro escolhas.

## Funcionalidades implementadas

- **Personagens:** `BaseCharacter` mantém vida e dano; `Player` trata movimentação e atributos do médico; `BaseEnemy` e `Virus` representam inimigos que perseguem o Player.
- **Câmera:** `CameraFollow` acompanha o Player.
- **Inimigos e ondas:** `EnemySpawner` cria vírus; `WaveManager` controla ondas, eliminações e progressão; `WaveUI` apresenta a onda e o progresso.
- **XP:** `ExperienceOrb` coleta cristais; `ExperienceSpawner` cria cristais durante a partida; `ExperienceDropper` decide o drop na morte do inimigo; `ExperienceManager` gerencia XP e níveis.
- **HUD:** `HealthBarUI` e `ExperienceBarUI` exibem vida e XP.
- **Melhorias:** `UpgradeDefinition`, `UpgradeChoice`, `UpgradeChoiceManager`, `UpgradeChoiceUI` e `PlayerStats` descrevem, sorteiam e aplicam opções gerais e de magia.
- **Receitas elementais:** `ElementRecipe`, `ElementalSpellDefinition` e `ElementalRecipeCatalog` representam e procuram as 34 receitas aprovadas. A ordem das escolhas não altera a receita; repetições são permitidas até três elementos.
- **Combate elemental:** `ElementalSpellController` desbloqueia com a primeira escolha, procura o inimigo vivo mais próximo, controla disparo e melhorias, e usa `GetRecipeColor()` para calcular a cor da receita. `ElementalProjectile` e efeitos auxiliares compartilham movimento, colisões, dano, áreas e estados. Trinta receitas têm comportamentos próprios; as demais continuam com parâmetros genéricos.
- **Visual do projétil:** `ElementalProjectileSprite.png` preserva forma e transparência do sprite provisório em branco neutro; `ElementalProjectileUnlit.mat` usa o shader URP 2D Unlit. O prefab elemental usa esses assets para que o tint do `SpriteRenderer` mostre a cor calculada.

## Arquitetura e dependências

```text
Player + PlayerStats + ElementalSpellController
                    ↑ escolhas
ExperienceManager → UpgradeChoiceManager → UpgradeChoiceUI
                    ↓ receita desbloqueada
ElementalRecipeCatalog → cor da receita → ElementalProjectile

EnemySpawner → Virus → BaseEnemy → BaseCharacter
                           ├→ ExperienceDropper → prefabs de cristais
                           └→ WaveManager (contagem de mortes)
ExperienceSpawner → ExperienceOrb → ExperienceManager
```

`VaccineSpell` e `VaccineProjectile` são o sistema anterior/provisório e não devem ser usados pelo ataque elemental genérico. `ElementalSpellBehavior` oferece ajustes opcionais ao ataque compartilhado. Vinte e cinco receitas têm comportamentos especiais configurados; as demais continuam usando o ataque genérico.

## Regras de design aprovadas

- Quatro elementos: Água `#258FE8`, Fogo `#F05A32`, Terra `#98613B`, Ar `#BDE9F4`.
- Até três escolhas elementais; repetição permitida; receita definida pelas quantidades, sem depender da ordem. Não oferecer novo elemento após completar três escolhas.
- O catálogo aprovado contém quatro receitas individuais, dez receitas de dois elementos e vinte evoluções de três elementos. Não renomear nem substituir receitas.
- Cada level-up oferece quatro opções. O peso inicial de elemento é 15%; o restante é distribuído entre atributos e melhorias da magia conforme opções elegíveis. Melhorias da magia continuam disponíveis depois de completar a receita.
- O jogador começa sem magia; a primeira escolha desbloqueia o ataque. Manter as melhorias ligadas ao combate real e evitar sistemas paralelos.
- Não acrescentar complexidade, dependências ou funcionalidades sem autorização.

## Estado dos testes

### Implementado e testado (verificações limitadas)

- Compilação C# após a correção visual: `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**.
- Inspeção dos assets confirmou sprite 64×64 com transparência e canais RGB neutros, material com shader `Universal Render Pipeline/2D/Sprite-Unlit-Default`, prefab referenciando ambos e catálogo contendo as quatro cores aprovadas.
- O Unity Editor foi aberto em Play Mode antes desta correção visual. A cena iniciou e mostrou a onda 1, nível 1 e inimigos; naquele momento a Console visível não mostrava erros. Esse teste não alcançou uma escolha elemental e não valida o ataque.
- O usuário relatou anteriormente que vírus apareciam, perseguiam o médico e recebiam projéteis do sistema anterior. Esse relato não substitui os testes pendentes abaixo.

- Água Pressurizada: compilação executada após a implementação com `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**. Isso confirma compilação C#, não o comportamento em jogo.
- Água Pressurizada: o usuário informou que validou manualmente a receita no Play Mode.
- Rajadas Cortantes, Rocha Pesada e Chamas Intensas: compilação concluída sem erros ou avisos; verificação estática confirmou que cada asset está atribuído somente à sua própria definição. Isso não substitui validação de combate no Play Mode.
- Aperfeiçoamento dos três comportamentos: compilação concluída com **0 erros e 0 avisos**. Verificação estática confirmou parâmetros de queimadura/lentidão/perseguição e as associações existentes. No Play Mode foi confirmada apenas a inicialização da cena, a exibição do HUD, o spawn de um vírus e Console sem erros/avisos; as mecânicas novas não foram alcançadas.
- Terceiro grupo elemental: `dotnet build COVID_SURVIVOR.slnx --no-restore` passou com **0 erros e 0 avisos**; verificações estáticas confirmaram associações exclusivas, parâmetros, collider circular e caminhos de código. Play Mode para Gelo Perfurante, Chamas Giratórias e Tempestade de Areia não foi executado.
- Reformulação da Tempestade de Areia: `dotnet build COVID_SURVIVOR.slnx --no-restore` passou com **0 erros e 0 avisos**. A inspeção estática confirmou o modo de projétil que vira área no impacto e os valores configurados no asset. Não foi executado teste funcional no Play Mode.
- Vapor Escaldante e Lama: compilação concluída com **0 erros e 0 avisos** após incluir temporariamente o script novo no `.csproj` gerado; a entrada temporária foi removida em seguida. Inspeção estática confirmou as duas associações e os parâmetros iniciais. Testes funcionais no Play Mode pendentes.

### Implementado, mas ainda não testado funcionalmente

- Ataque elemental depois do desbloqueio, impacto, dano, morte do vírus por esse ataque e comportamento após escolhas dois e três.
- Mudança de cor do projétil para cada elemento e para as receitas combinadas. A cadeia de código foi inspecionada; a correção de sprite/material desta atualização ainda não foi observada em Play Mode com uma receita desbloqueada.
- Cada melhoria elemental em jogo, especialmente a nova opção de velocidade.
- Coexistência prolongada entre combate, ondas, drops, XP e level-ups.
- Água Pressurizada (`WaterWater`): o asset especial aplica dano ×1,5, alcance de busca ×1,5, velocidade ×1,5, comprimento ×1,8, largura ×0,45 e perfuração. O projétil usa `CapsuleCollider2D` e registra os `BaseEnemy` já atingidos. Validação manual em Play Mode relatada pelo usuário.
- Rajadas Cortantes (`AirAir`): dano ×1,3, alcance ×1,3, velocidade ×1,8, comprimento ×1,7 e largura ×0,4.
- Rocha Pesada (`EarthEarth`): dano ×2, alcance ×1, velocidade ×0,65, comprimento ×1,4 e largura ×1,5.
- Chamas Intensas (`FireFire`): dano ×1,5, alcance ×1, velocidade ×1,2, comprimento ×1,4 e largura ×1,4.
- Os três novos comportamentos usam a escala e o gatilho de colisão genéricos, sem perfuração. Não foram testados no Play Mode.
- Chamas Intensas aplica queimadura por 2 segundos, com 2 de dano a cada 0,5 segundo (quatro pulsos, 8 de dano total se o vírus sobreviver); reaplicações renovam a duração sem criar instâncias acumuladas. As marcas visuais não foram implementadas. A lógica compila, mas precisa de teste no Play Mode.
- Rocha Pesada reduz a velocidade de movimento em 30% por 2 segundos. A velocidade-base do vírus permanece intacta e o multiplicador temporário volta a 1 ao terminar; reaplicações renovam a duração sem somar reduções. Marcas visuais não foram implementadas. A lógica compila, mas precisa de teste no Play Mode.
- Rajadas Cortantes cria 3 projéteis por ataque, com 50% do dano individual anterior, homing e velocidade de curva configurável inicialmente em 360 graus/segundo. Se o alvo morrer, cada projétil procura outro inimigo vivo dentro do alcance de busca da receita; se não houver, é destruído. A lógica compila, mas precisa de teste no Play Mode.
- Gelo Perfurante (`WaterAir`): dano ×1,3, alcance ×1,2, velocidade ×1,3, escala ×1,6/×0,45, chance de congelamento de 25% e duração configurável inicialmente 1,5 s. `BaseEnemy` para de mover enquanto congelado e renova o estado sem acumular instâncias.
- Chamas Giratórias (`FireAir`): área circular móvel, segue em linha reta a direção inicial escolhida pelo controlador, dura 3 s e aplica 40% do dano-base como pulso por inimigo a cada 0,5 s. Raio inicial 0,75 unidade e velocidade ×1,2.
- Tempestade de Areia (`EarthAir`): comportamento anterior substituído pela reformulação descrita em “Continuidade — reformulação da Tempestade de Areia”.
- Vapor Escaldante (`WaterFire`) e Lama (`WaterEarth`): comportamentos descritos em “Continuidade — Vapor Escaldante e Lama”. Implementados e compilados, ainda sem validação funcional no Play Mode.
- Os dois ataques em área mantêm um temporizador individual por `BaseEnemy`, aplicam um pulso imediato no primeiro contato e depois respeitam o intervalo configurado. As referências a inimigos mortos são removidas enquanto a área permanece ativa.

### Planejado e ainda não implementado

- Efeitos e comportamentos exclusivos para as outras receitas do catálogo, usando `ElementalSpellBehavior` quando houver autorização e necessidade.
- Outras habilidades planejadas no contexto (Álcool 70%, Anticorpos e Supervacina) e sistemas de evolução/combinação de magias.
- Condições finais e demais metas do jogo que não estejam cobertas pelos sistemas atuais.

## Problemas conhecidos

- A correção visual do projétil usa shader URP 2D explícito e sprite branco neutro, mas falta confirmar no Play Mode que os quatro tons e as médias das combinações aparecem como esperado.
- Em uma observação anterior da cena, detalhes magenta eram visíveis no sprite provisório do vírus. Não foram modificados nesta tarefa; confirmar separadamente se é a arte provisória ou um problema de renderização.
- A maioria das receitas ainda usa o ataque genérico; 25 receitas agora têm comportamentos específicos. A presença de assets não implica que todos os 34 efeitos estejam implementados.
- As queimaduras, lentidão e perseguição dos projéteis precisam de validação manual no Play Mode: renovação e término dos estados, restauração da velocidade, reacquisição após a morte do alvo e destruição dos projéteis sem novos alvos.
- A lógica compartilhada de impacto único por projétil permanece em `ElementalProjectile`; conferir durante os testes que os novos efeitos são aplicados uma vez por impacto e não alteram as outras receitas.
- Durante a verificação, o botão temporário `DEBUG — Subir nível` foi clicado, mas a tela de escolha não abriu; ainda não foi possível selecionar receitas especiais pelo fluxo da cena. O teste dos comportamentos deve aguardar que o botão de debug funcione ou ser feito por progressão normal.
- O prefab compartilhado `ElementalProjectile` agora tem `CapsuleCollider2D` para tiros comuns e `CircleCollider2D` alternado em tempo de execução para ataques em área. A cena iniciou após a importação no Unity Editor, sem erros/avisos; ainda é preciso testar o acionamento e a permanência real dos colliders de área.
- Chamas Giratórias e Tempestade de Areia usam a mesma imagem circular provisória tingida pela cor da receita; não há arte de tornado ou areia nesta etapa. A mecânica não depende de um visual específico.

## Botão temporário de debug de level-up

- `UpgradeChoiceUI` cria no Canvas um botão vermelho `DEBUG — Subir nível`, ancorado no canto inferior esquerdo; oculta-o enquanto as quatro opções normais estão abertas e o mostra novamente após a escolha.
- `UpgradeChoiceManager` conecta o clique ao `ExperienceManager`; a pausa, sorteio e aplicação continuam pelo fluxo normal.
- `ExperienceManager.DebugGrantExperienceForNextLevel()` calcula `ExperienceRequired - CurrentExperience` e concede esse valor pelo evento usual do `Player`. Não altera a receita elemental diretamente e não atua se o Player estiver morto.
- Para remover antes da entrega: retirar o método de debug em `ExperienceManager`, o botão e suas ações em `UpgradeChoiceUI`, e a conexão em `UpgradeChoiceManager`.
- Verificação: `dotnet build COVID_SURVIVOR.slnx --no-restore` concluído com **0 erros e 0 avisos**. Inspeção estática confirmou que o botão chama o método público do `ExperienceManager`, que concede XP pela API existente do Player, e que a tela de escolhas oculta o botão.
- Play Mode ainda pendente: confirmar posição/tamanho, interação por mouse, ganho de exatamente um nível por clique, ocultação durante escolhas e retomada do jogo.

## Próxima etapa recomendada

Abra `SampleScene`, importe e compile os novos assets no Unity Editor e valide no Play Mode as 11 receitas de Fogo registradas na seção “Continuidade — 11 comportamentos de receitas com Fogo”. Teste cada receita com mais de um vírus e registre separadamente o resultado de dano, direção, duração, visual e limpeza. Não inicie outras receitas até confirmar esse grupo.

## Resumo de continuidade para ChatGPT

O botão temporário de debug está implementado no HUD por código e compila. Ele concede o XP faltante pelo fluxo normal, ocultando-se durante a escolha; não houve alteração nas receitas nem implementação de combinações novas. Ainda não foi validado no Unity Play Mode. Próxima sessão: testar o botão repetidamente e a pausa/retomada; depois validar Água Pressurizada (duas escolhas de Água, projétil azul alongado, velocidade/dano/alcance e perfuração com dano único por vírus); comparar uma receita comum. Só após esses testes, iniciar três combinações por vez e pausar entre grupos para validação manual.

### Continuidade — queimadura, lentidão e projéteis teleguiados

- Implementado: Chamas Intensas aplica queimadura renovável (2 s, 2 de dano por pulso a cada 0,5 s); Rocha Pesada aplica lentidão renovável de 30% por 2 s; Rajadas Cortantes dispara 3 projéteis com 50% do dano anterior e perseguição configurável a até 360°/s.
- Atributos são definidos nos assets `IntenseFlamesBehavior`, `HeavyRockBehavior` e `SharpGustsBehavior`. As chamadas de estado ficam em `BaseEnemy`, e o movimento do vírus consulta a velocidade efetiva sem alterar a base.
- `BaseEnemy.StatusEffectAppliedOrRefreshed` e `StatusEffectEnded` são pontos simples de integração futura para animações/partículas. Nenhum visual ou animação provisória foi criado.
- Compilação e verificações estáticas passaram; não foi executado Play Mode para estas alterações. Validar renovação e término dos efeitos, dano periódico, velocidade restaurada, reacquisição de alvo e encerramento dos projéteis sem alvo antes de prosseguir.

### Continuidade — terceiro grupo de comportamentos

- Implementado `WaterAir` (Gelo Perfurante), `FireAir` (Chamas Giratórias) e `EarthAir` (Tempestade de Areia), cada um com asset exclusivo. Nomes, receitas, catálogo e level-up foram preservados.
- `BaseEnemy` agora também controla congelamento; `CurrentMovementSpeed` retorna zero durante o estado, que termina ou é renovado pelo temporizador. Eventos de status incluem `Frozen`.
- `ElementalProjectile` alterna entre cápsula de impacto único e círculo de área. Modo móvel usa a direção inicial do alvo; modo estacionário nasce no alvo. Para áreas, usa `OnTriggerEnter2D`/`OnTriggerStay2D` e agenda o próximo dano individual por vírus.
- Valores iniciais: Gelo dano ×1,3/alcance ×1,2/velocidade ×1,3/congelamento 25% por 1,5 s; Tornado pulso ×0,4, raio 0,75, 3 s, intervalo 0,5 s e velocidade ×1,2; Areia pulso ×0,3, raio 1,5, 3 s, intervalo 0,5 s e lentidão 25% por 1 s.
- Sem sprites, partículas, sons ou animações novos. `StatusEffectAppliedOrRefreshed` e `StatusEffectEnded` continuam disponíveis para apresentação futura.
- Compilação e verificações estáticas passaram. As três mecânicas ainda não foram verificadas no Play Mode; também verificar no Editor se o collider circular do prefab foi importado e habilitado corretamente.

## Arquivos da implementação mais recente

- Implementação Água Pressurizada: criada `Assets/Scripts/Gameplay/Elements/PressurizedWaterBehavior.cs` e seu asset; modificados `ElementalSpellBehavior.cs`, `ElementalSpellController.cs`, `ElementalProjectile.cs`, `ElementalProjectile.prefab` e `WaterWater.asset`. O asset usa dano ×1,5, alcance ×1,5, velocidade ×1,5, comprimento ×1,8, largura ×0,45 e perfuração. O usuário informou validação manual em Play Mode.
- Implementação deste grupo: criado `Assets/Scripts/Gameplay/Elements/ElementalProjectileModifierBehavior.cs` e `.meta`, mais os assets `Assets/Elemental/Behaviors/SharpGustsBehavior.asset`, `HeavyRockBehavior.asset` e `IntenseFlamesBehavior.asset`, com seus `.meta`. Modificados `Assets/Elemental/Definitions/AirAir.asset`, `EarthEarth.asset`, `FireFire.asset` e este documento.
- Decisão técnica: os três novos assets compartilham a classe simples de multiplicadores e são atribuídos individualmente às definições corretas. O controlador continua aplicando a cor do catálogo e o projétil compartilhado continua causando dano; a perfuração permanece exclusiva de Água Pressurizada.
- Valores-base atuais do controlador: dano 10, busca 6, velocidade 7; escala-base do prefab 0,25. Resultados sem melhorias acumuladas: Rajadas Cortantes — dano 13, alcance 7,8, velocidade 12,6, escala relativa (1,7; 0,4); Rocha Pesada — dano 20, alcance 6, velocidade 4,55, escala (1,4; 1,5); Chamas Intensas — dano 15, alcance 6, velocidade 8,4, escala (1,4; 1,4). As dimensões visuais em mundo também dependem da escala-base do prefab e das melhorias escolhidas.
- Testes deste grupo: compilação executada com `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**; verificação estática confirmou os valores e que cada asset é usado apenas pela receita esperada. Testes no Play Mode ainda pendentes.

### Continuidade — 11 comportamentos de receitas com Fogo

**Implementado, mas ainda não testado no Play Mode.** Foram conectados 11 assets exclusivos às receitas `FireEarth` (Magma), `WaterWaterFire` (Gêiser), `WaterFireFire` (Vapor Explosivo), `WaterFireEarth` (Erupção), `WaterFireAir` (Supertempestade), `FireFireEarth` (Rocha Vulcânica), `FireFireAir` (Furacão Flamejante), `FireEarthAir` (Erupção Vulcânica), `FireAirAir` (Fênix Celestial), `FireFireFire` (Inferno) e `FireEarthEarth` (Meteorito). Os nomes, receitas, catálogo e fluxo de level-up foram preservados.

- A classe compartilhada `ElementalProjectileModifierBehavior` expõe os parâmetros das habilidades no Inspector. O `ElementalAttackMode` escolhe a operação no projétil sem criar um controlador de combate paralelo.
- Magma voa até colidir, aplica dano direto e permanece como uma pequena área de lava com pulsos de dano por 4 s (raio 0,8; intervalo 0,75 s; pulso ×0,45).
- Gêiser cria uma coluna na posição do alvo (largura 0,8; altura 3,5; subida 0,15 s), causando pulsos por 1 s (raio 0,6; intervalo 0,25 s; dano ×1,3, pulso ×0,8).
- Vapor Explosivo voa até o alvo e causa dano direto ×1,4 e explosão instantânea de raio 3,2 com dano ×1,3; apresentação provisória dura 0,4 s. O alvo do impacto é excluído da explosão para não receber dois danos na mesma ativação.
- Erupção desenha uma fissura provisória do médico até o alvo e executa cinco explosões espaçadas em 0,25 s (raio 0,75; dano dos pulsos ×0,7; dano base ×1,2). Cada inimigo só é atingido uma vez durante essa sequência.
- Supertempestade é uma área móvel que segue a direção do alvo inicial durante 4 s (raio 1,3; pulsos a cada 0,5 s; dano ×0,45; velocidade ×0,55).
- Rocha Vulcânica aplica dano de impacto ×2 e lança seis fragmentos (×0,45 de dano; velocidade 5,5; vida 1,2 s; tamanho 0,25).
- Furacão Flamejante usa a última direção de movimento do Player (ou a direção guardada), atinge todos no cone frontal (comprimento 2,5; ângulo 75°; ativação visual de 0,25 s; dano ×0,8), sem perseguir alvos.
- Erupção Vulcânica aplica uma explosão de raio 1,8 e lança dez fragmentos (dano da explosão ×1,4; dano dos fragmentos ×0,6; velocidade 6; vida 1,4 s; tamanho 0,22).
- Fênix Celestial usa projétil perfurante e teleguiado, reaquisição de alvos após cada acerto, busca ×1,5, velocidade ×1,2, curva 260°/s e vida ×2 do tempo configurado no controlador (6 s com o valor-base atual de 3 s). Cada alvo só recebe dano uma vez por projétil.
- Inferno cria área estacionária no alvo por 6 s (raio 2,2; pulsos a cada 0,5 s; dano ×1,2; pulso ×0,45; alcance de busca ×1,2).
- Meteorito parte de 4 unidades acima do alvo e cai, causando impacto ×2, explosão com dano ×1,5 em raio 2 e deixando área residual por 5 s (pulsos a cada 1 s; dano ×0,35).
- Fragmentos criados por uma mesma ativação compartilham uma lista de inimigos atingidos. Em sequências e explosões de fragmentos, um inimigo já atingido não recebe dano de outro pulso/fragmento da mesma ativação; áreas contínuas continuam usando seu intervalo periódico próprio.
- `Player.FacingDirection` guarda a última direção não nula de movimento, usada pelo Furacão Flamejante.
- `ElementalBurstVisual` e `ElementalFragment` fornecem círculos, linha e fragmentos provisórios sem depender de sprites finais. A apresentação dos efeitos pode ser substituída sem alterar a seleção de alvos e a aplicação de dano.
- Modificados: `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `Assets/Scripts/Gameplay/Characters/Player.cs`, `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs` e o asset existente `Assets/Elemental/Behaviors/CelestialPhoenixBehavior.asset` (multiplicador de duração configurado em 2).
- Criados: `Assets/Scripts/Gameplay/Spells/ElementalBurstVisual.cs` e `.meta`, `ElementalFragment.cs` e `.meta`; 11 assets e metadados em `Assets/Elemental/Behaviors/`: `MagmaBehavior`, `GeyserBehavior`, `ExplosiveSteamBehavior`, `EruptionBehavior`, `SuperstormBehavior`, `VolcanicRockBehavior`, `FlamingHurricaneBehavior`, `VolcanicEruptionBehavior`, `CelestialPhoenixBehavior`, `InfernoBehavior` e `MeteoriteBehavior`; modificadas as definições correspondentes de `Assets/Elemental/Definitions/` e seus campos `m_behavior`.
- Compilação: `dotnet build COVID_SURVIVOR.slnx --no-restore` passou com **0 erros e 0 avisos**, com entradas temporárias para os dois scripts novos no `.csproj`; as entradas foram removidas depois. A importação/compilação pelo Unity em batch mode não concluiu: o processo relatou `attempt to write a readonly database` ao iniciar o cliente de licenciamento. Não houve confirmação de importação pelo Editor; ao abrir o projeto, aguarde o Unity atualizar os assets e regenerar o projeto C#.
- Verificação estática: as 11 definições apontam para o asset correto e os GUIDs dos assets existem. Não houve execução de Play Mode, teste de colisões, validação visual no URP nem confirmação de importação pelo Unity Editor.
- Pendências: testar todas as 11 receitas em Play Mode; verificar direção, área, dano, cadência, duração, limpeza e cor; conferir que inimigos não recebem dano duplicado; verificar a resolução do asset e a aparência de LineRenderer/fragmentos no material URP; observar se upgrades de quantidade de projéteis produzem a quantidade desejada nos modos especiais.
- Próximo passo: abrir a cena, importar/compilar no Unity Editor e testar cada receita com vários inimigos. Corrigir somente problemas observados e registrar os resultados antes de iniciar outras receitas.
- Aperfeiçoamento deste grupo: modificados `ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `ElementalProjectile.cs`, `BaseEnemy.cs`, `Virus.cs`, os assets `SharpGustsBehavior.asset`, `HeavyRockBehavior.asset`, `IntenseFlamesBehavior.asset` e este documento. Estados de queimadura/lentidão vivem em `BaseEnemy`; projéteis teleguiados guardam alvo, ajustam ângulo em `FixedUpdate`, reacquirem inimigos vivos dentro do alcance e se destroem quando não encontram outro alvo. Os valores e testes pendentes estão descritos acima.
- Verificação Unity Play Mode desta sessão: `SampleScene` iniciou, o HUD foi exibido e um vírus apareceu; a Console mostrou 0 erros e 0 avisos. O botão `DEBUG — Subir nível` não abriu as opções quando acionado, então queimadura, lentidão, rajadas e interações entre efeitos não foram testadas funcionalmente. Play Mode foi encerrado sem salvar alterações na cena.
- Terceiro grupo: criados `Assets/Elemental/Behaviors/PiercingIceBehavior.asset`, `FireWhirlwindBehavior.asset`, `SandstormBehavior.asset` e respectivos `.meta`; modificados `ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `ElementalProjectile.cs`, `BaseEnemy.cs`, `ElementalProjectile.prefab`, `WaterAir.asset`, `FireAir.asset`, `EarthAir.asset` e este documento. Compilação passou com 0 erros/avisos; verificações estáticas passaram. Sem teste funcional no Play Mode.
- Inicialização do Unity após as alterações: `SampleScene` entrou em Play Mode, mostrou o HUD e gerou um vírus; contadores da Console permaneceram em 0 erros e 0 avisos. Play Mode foi encerrado sem salvar alterações. As três receitas não foram selecionadas.

### Continuidade — reformulação da Tempestade de Areia

- `EarthAir` agora usa `ImpactArea`: o controlador cria o projétil na posição do médico. Ele voa com o colisor cápsula; ao tocar um vírus vivo, registra o ponto de impacto e troca, no próximo `FixedUpdate`, para uma área circular estacionária.
- `SandstormBehavior.asset` configura dano ×0,3, duração da área de 5 s, raio 1,5 unidade, intervalo de dano de 1 s, redução máxima de velocidade de 100% e progressão até o máximo em 2 s. Com dano-base 10, cada pulso causa 3 de dano, sujeito ao arredondamento já usado pelo sistema.
- `BaseEnemy` acompanha as áreas que o afetam. A redução progride como `1 - (redução máxima × progresso do tempo)`, chegando a imóvel após 2 s. Áreas sobrepostas compartilham o efeito sem acumular reduções; a saída da última área remove a lentidão. Reentrar depois de sair reinicia a progressão. Morte ou destruição da área também limpa a fonte.
- Cada área mantém seu próprio próximo instante de dano para cada vírus: um pulso no contato e pulsos seguintes com intervalo de 1 s. Referências de inimigos mortos são removidas.
- Arquivos modificados nesta reformulação: `Assets/Scripts/Gameplay/Characters/BaseEnemy.cs`, `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `Assets/Scripts/Gameplay/Elements/ElementalProjectileModifierBehavior.cs`, `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs`, `Assets/Elemental/Behaviors/SandstormBehavior.asset` e este documento. Não foram alterados cena, prefab, controlador ou outras receitas.
- Verificação: compilação `dotnet build COVID_SURVIVOR.slnx --no-restore` concluída com **0 erros e 0 avisos**. A lógica e o asset foram inspecionados; não houve teste funcional no Play Mode. Permanecem pendentes voo, impacto, marcos de lentidão aos 0,5/1/1,5/2 s, dano, saída/reentrada e fim da área após 5 s.
- Próximo passo: validar a receita Terra + Ar no Play Mode e registrar os resultados antes de iniciar outra mecânica.

### Continuidade — Vapor Escaldante e Lama

- `WaterFire` recebe exclusivamente `ScaldingSteamBehavior`. No primeiro impacto, o projétil aplica dano direto normal uma vez, consulta `BaseEnemy` dentro do alcance e filtra por distância e ângulo usando a direção real do projétil. O alvo direto é excluído da explosão; cada outro vírus elegível recebe no máximo um pulso de dano. O cone visual é um mesh provisório temporário, separado em `ElementalConeVisual`, com centro claro e bordas alaranjadas.
- Valores iniciais do Vapor: comprimento 3 unidades, abertura 70°, dano da explosão ×1, duração visual 0,3 s. A explosão não aplica dano contínuo nem queimadura.
- `WaterEarth` recebe exclusivamente `MudBehavior`. O projétil usa um sprite quadrado provisório branco tingido de marrom e `BoxCollider2D` criado somente na instância de Lama; mede 0,8 unidade no eixo de deslocamento por 1,5 de largura e usa a velocidade-base modificável pelo multiplicador no asset. No impacto, causa dano direto ao vírus atingido e dano instantâneo uma vez aos outros vírus dentro do raio de 1,5; depois para e volta ao sprite circular existente para representar uma mancha estacionária.
- A mancha dura 5 s, tem raio de 1,5 e aplica imediatamente 40% de lentidão enquanto o vírus estiver dentro. O dano de impacto não se repete durante a duração da mancha. Não foi necessário um tempo de rampa para a Lama; a redução acompanha a presença na área.
- `BaseEnemy` rastreia fontes de lentidão de área por `ElementalProjectile`. A velocidade efetiva usa a redução mais forte entre as fontes, sem multiplicá-las. Sair de uma mancha remove somente aquela fonte; lentidão progressiva da Tempestade, lentidão temporizada e outras manchas continuam valendo enquanto ativas. Morte/destruição limpa as referências.
- Arquivos criados: `Assets/Scripts/Gameplay/Spells/ElementalConeVisual.cs` e `.meta`; `Assets/Elemental/Behaviors/ScaldingSteamBehavior.asset` e `.meta`; `Assets/Elemental/Behaviors/MudBehavior.asset` e `.meta`.
- Arquivos modificados: `Assets/Scripts/Gameplay/Characters/BaseEnemy.cs`, `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `Assets/Scripts/Gameplay/Elements/ElementalProjectileModifierBehavior.cs`, `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs`, `Assets/Elemental/Definitions/WaterFire.asset`, `Assets/Elemental/Definitions/WaterEarth.asset` e este documento. Nenhuma cena ou prefab foi modificado; o colisor retangular da Lama é adicionado apenas às instâncias em runtime.
- Compilação: **0 erros e 0 avisos**. Como o `.csproj` gerado estava desatualizado e não incluía o novo script, ele foi incluído temporariamente somente para a compilação e restaurado logo depois. Verificações estáticas confirmaram modos, parâmetros e associações exclusivas. Nenhum teste foi executado no Play Mode.
- Pendências: testar cone em todas as direções, exclusão do alvo direto e de inimigos fora da região; conferir material/visibilidade do mesh no URP; validar retângulo e orientação do projétil de Lama, impacto, lentidão, sobreposição com Lama/Tempestade/slow temporizado e desaparecimento após 5 s.
- Próximo passo: abrir `SampleScene`, desbloquear as duas receitas e completar esses testes no Play Mode antes de implementar outra combinação.

### Ajuste — dano de impacto da Lama

- A Lama agora causa o dano direto normal ao vírus atingido e aplica uma única explosão de dano aos demais vírus dentro do raio configurado para a mancha (atualmente 1,5 unidade). O vírus direto é excluído da explosão para não receber dano duplicado.
- O dano ao redor acontece apenas no impacto; a mancha continua causando somente lentidão de 40% por até 5 s, sem pulsos de dano. A duração, o raio e a lentidão não foram alterados.
- Arquivos alterados: `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs` e este documento.
- Compilação final: `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**. A primeira compilação após o ajuste usou uma entrada duplicada temporária do script no projeto gerado e apresentou aviso CS2002; a entrada duplicada foi removida e a compilação final limpa passou. Não foi executado Play Mode.
- Testes pendentes: confirmar que o alvo direto e os vírus próximos recebem o dano uma única vez, vírus fora do raio não recebem dano, e que a lentidão e a duração da mancha permanecem inalteradas.
- Próximo passo: testar Água + Terra no Play Mode e registrar os resultados.

### Continuidade — crescimento da Tempestade de Areia durante o voo

- A Tempestade começa em 25% da escala final e cresce suavemente por 1 s durante o voo. Se atingir um vírus antes disso, a área assume imediatamente a escala final no ponto de impacto; a ativação da área interrompe movimento e crescimento e mantém a escala fixa durante os 5 s de duração.
- A escala final é calculada a partir do raio configurado e dos limites do sprite. A mesma escala é usada ao configurar o `CircleCollider2D`, mantendo coerentes o tamanho visual final e o raio de dano. Os parâmetros novos são lidos apenas quando o comportamento usa `ImpactArea`, preservando os outros ataques no prefab compartilhado.
- No asset `SandstormBehavior`, `m_impactGrowthStartScale` começa em 0,25 e `m_impactGrowthDuration` em 1 s. Os outros valores de dano, raio, duração e lentidão foram preservados.
- Arquivos modificados nesta etapa: `ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalProjectile.cs`, `SandstormBehavior.asset` e `DEVELOPMENT_STATUS.md`.
- Verificações: `dotnet build COVID_SURVIVOR.slnx --no-restore` passou com **0 erros e 0 avisos**. Verificações estáticas confirmaram que o crescimento é limitado a `ImpactArea`, os valores novos estão no asset de Sandstorm, e raio, duração, dano e associação da receita foram preservados. Play Mode não executado.
- Testes pendentes: saída pequena, crescimento, impacto antecipado e tardio, escala fixa durante a área, raio/dano, lentidão e duração. Próximo passo: validar a tempestade no Play Mode antes de iniciar outra combinação.

### Reformulação — Magma, Furacão Flamejante, Órbita Ígnea e Inferno; debug elemental

**Implementado, ainda sem teste funcional no Play Mode.**

- **Magma:** `MagmaBehavior` agora deixa o projétil maior (×1,8), causa dano direto ×1,5 e uma explosão instantânea em raio 1,7 com dano ×0,8. O alvo direto é excluído da explosão. A receita não deixa área residual nem causa dano periódico.
- **Furacão Flamejante:** foi substituído o cone disparado em rajadas por `ElementalFlamethrower`, uma única área persistente que segue o Player, orienta-se pela última direção válida e causa dano a cada 0,5 s em cone de 2,5 unidades e 75°. Dano do pulso: ×0,6 do dano já modificado pela receita.
- **Órbita Ígnea:** `FireAirAir.asset` agora apresenta o nome Órbita Ígnea e usa `FireOrbitBehavior.asset`; o antigo asset `CelestialPhoenixBehavior.asset` foi renomeado mantendo o GUID. `ElementalOrbitingFire` mantém tornados igualmente espaçados em círculo, com raio-base 1,5, rotação 180°/s, tamanho 0,65, dano a cada 0,5 s e dano ×0,25. O número de tornados acompanha o atributo atual de quantidade de projéteis. Cada melhoria de alcance da magia acrescenta seu bônus ao raio orbital, sem alterar o alcance de busca de alvos.
- **Inferno:** `ElementalInfernoEffect` mantém uma aura circular centrada no médico, com raio 3 e dano ×0,2 a cada 1 s; além disso, cria uma onda a cada 3 s. `ElementalFireWave` registra o ponto de origem, expande até raio 5 à velocidade 8, causa dano ×1,5 uma vez por inimigo e desaparece ao completar a expansão. Aura e onda atingem somente `BaseEnemy`; as ondas são destruídas junto ao efeito quando a magia deixa de estar ativa.
- **Modo temporário do botão de debug:** `UpgradeChoiceManager` recebe o clique em um caminho próprio e cria quatro opções distintas de elementos, usando as mesmas receitas, previews e cores. Elementos já escolhidos podem voltar em level-ups seguintes; o limite de três é mantido. Ao atingir o limite, o botão é desativado e informa isso. O método probabilístico normal `TryCreateFourChoices` e seus pesos não foram alterados.
- A arquitetura persistente foi separada dos projéteis temporários: `ElementalFlamethrower`, `ElementalOrbitingFire` e `ElementalInfernoEffect` são criados somente enquanto suas respectivas receitas estão ativas e removidos ao trocar de modo, desativar o controlador ou morrer o Player. `ElementalFireWave` pertence ao efeito Inferno e tem centro fixo.
- Parâmetros estão nos assets `MagmaBehavior`, `FlamingHurricaneBehavior`, `FireOrbitBehavior` e `InfernoBehavior`; extensão de comportamento e valores aparecem em `ElementalSpellBehavior` e `ElementalProjectileModifierBehavior`.
- Arquivos modificados: `Assets/Scripts/Gameplay/UpgradeChoiceManager.cs`, `Assets/Scripts/UI/UpgradeChoiceUI.cs`, `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs`, `Assets/Elemental/Behaviors/MagmaBehavior.asset`, `FlamingHurricaneBehavior.asset`, `InfernoBehavior.asset`, renomeado `CelestialPhoenixBehavior.asset` → `FireOrbitBehavior.asset` (e o `.meta`, GUID preservado), `Assets/Elemental/Definitions/FireAirAir.asset` e este documento.
- Arquivos criados: `Assets/Scripts/Gameplay/Spells/ElementalFlamethrower.cs`, `ElementalOrbitingFire.cs`, `ElementalFireWave.cs`, `ElementalInfernoEffect.cs` e respectivos `.meta`.
- Verificação: `dotnet build COVID_SURVIVOR.slnx --no-restore` passou com **0 erros e 0 avisos**. As quatro entradas dos scripts novos foram incluídas temporariamente no `.csproj` gerado para a compilação e removidas em seguida; as entradas que o Unity já regenerou para os dois scripts auxiliares anteriores foram mantidas. A tentativa de importação Unity em batch mode não concluiu por erro `attempt to write a readonly database` no cliente de licenciamento. Não houve Play Mode.
- Pendências: verificar no Unity a importação do asset renomeado e a referência pelo GUID; observar no Play Mode cone, rotação e dano dos orbitadores, órbita ampliada por upgrade de alcance, aura/onda do Inferno e explosão sem poça do Magma. Também testar as quatro opções únicas no botão de debug em zero, uma, duas e três escolhas e confirmar que um level-up normal ainda usa as categorias e pesos originais.
- Próximo passo: abrir o projeto no Unity Editor, deixar os assets importarem, compilar e testar primeiro o botão de debug; em seguida validar cada uma das quatro magias, sem alterar as outras sete receitas aprovadas.

### Ajustes de gameplay — rajadas, órbita e aura do Inferno

**Implementado e compilado; teste funcional no Play Mode pendente.**

- `ElementalFlamethrower` deixou de manter o cone ativo continuamente. Ele inicia uma rajada a cada **0,55 s**, mantém o cone visível por **0,3 s** e aplica **3 pulsos** separados por **0,1 s**. A escala do cone pulsa em cada aplicação para comunicar as baforadas. Cada pulso usa **25%** do dano configurado pela receita e deduplica colliders do mesmo inimigo naquele pulso. A cada frame, a direção visual e de dano se aproxima da direção lembrada pelo Player a até **1440°/s**; parado, `Player.FacingDirection` conserva a última direção não nula. Alcance **2,5** e abertura **75°** foram mantidos.
- A consulta de colisão das rajadas usa um buffer fixo de 128 colliders e um `HashSet` reutilizado; o cone de mesh é criado uma vez e apenas habilitado durante a rajada. Isso evita alocar arrays/conjuntos novos por pulso.
- `FireOrbitBehavior.asset`: tamanho visual do tornado passou de **0,65 para 1,0** e raio-base da órbita de **1,5 para 2,5**. Rotação (**180°/s**), intervalo de dano (**0,5 s**), multiplicador de dano (**0,25**), contagem ligada à melhoria de projéteis e adição do bônus de alcance ao raio orbital foram preservados.
- `InfernoBehavior.asset`: somente o raio da aura passou de **3 para 4,5**. O sistema continua escalando o sprite de aura com esse raio; dano e intervalo (**1 s**) e todos os parâmetros das ondas (**3 s**, raio **5**, velocidade **8**, dano ×**1,5**, largura **0,12**) foram preservados.
- Arquivos modificados: `Assets/Scripts/Gameplay/Spells/ElementalFlamethrower.cs`, `ElementalOrbitingFire.cs`, `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `Assets/Elemental/Behaviors/FlamingHurricaneBehavior.asset`, `FireOrbitBehavior.asset`, `InfernoBehavior.asset` e este documento. Nenhuma cena, prefab, receita ou código do botão de debug foi alterado.
- Compilação `dotnet build COVID_SURVIVOR.slnx --no-restore`: **0 erros e 0 avisos**. A primeira tentativa revelou uso de `Vector2.RotateTowards` inexistente e avisos de `Physics2D.OverlapCircleNonAlloc` obsoleto; foram corrigidos usando `Vector3.RotateTowards` e a sobrecarga de `Physics2D.OverlapCircle` com `ContactFilter2D` e buffer reutilizável.
- Verificação estática confirmou os valores dos três assets e ausência dos campos antigos de pulso contínuo. Play Mode não foi executado.
- Testes pendentes: fluidez da rotação ao mudar de direção e parado; três pulsos por rajada, intervalo e dano sem duplicação por múltiplos colliders; tamanho/raio e movimento dos tornados; raio 4,5 da aura; confirmar que dano e ondas do Inferno não mudaram.
- Próximo passo: testar as três receitas no Play Mode com inimigos em movimento e registrar os resultados antes de outros ajustes.

### Mira pelo mouse e dimensões do Furacão Flamejante

**Implementado e compilado; teste funcional no Play Mode pendente.**

- `ElementalFlamethrower` agora lê a posição do mouse pelo Input System (`Mouse.current.position`) e usa a `MainCamera` já existente. A posição de tela é convertida para mundo com `Camera.ScreenToWorldPoint`, usando como profundidade a distância em Z entre a câmera e o médico; a direção desejada é o vetor do médico até esse ponto convertido.
- A suavização da direção permanece a **1440°/s**. A direção só é atualizada se o vetor tiver magnitude acima de `0.0001`; com o cursor sobre o médico, sem mouse ou sem MainCamera, mantém a última direção válida. A cena tem câmera `MainCamera` ortográfica em Z = −10, portanto a profundidade usada aponta para o plano Z do Player.
- No asset `FlamingHurricaneBehavior`, o alcance/comprimento passou de **2,5 para 2,875** (+15%), e `m_flamethrowerVisualWidthMultiplier` passou a **0,85** (−15% na largura visual). O ângulo funcional de **75°** e o dano por pulso não mudaram. Como reduzir a abertura geométrica alteraria o ângulo, a redução de espessura foi aplicada ao mesh visual; a região de dano preserva o cone original de 75°.
- Intervalo de rajadas (**0,55 s**), duração (**0,3 s**), pulsos (**3 × 0,1 s**), dano (**25% por pulso**) e deduplicação por pulso foram preservados. Não foi criado nem alterado um asset de Input Actions.
- Arquivos modificados: `Assets/Scripts/Gameplay/Spells/ElementalFlamethrower.cs`, `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `Assets/Elemental/Behaviors/FlamingHurricaneBehavior.asset` e este documento. Nenhuma outra magia, câmera, cena, prefab, melhoria ou botão de debug foi alterado.
- Compilação `dotnet build COVID_SURVIVOR.slnx --no-restore`: **0 erros e 0 avisos**. Inspeção estática confirmou MainCamera na cena, conversão e parâmetros do asset. Play Mode não foi executado.
- Testes pendentes: acompanhar o mouse com médico parado e em movimento; testar cantos da tela, mouse sobre o médico e ausência temporária do mouse; verificar suavização, direção dos três pulsos, alcance 2,875 e aparência 15% mais estreita; confirmar cadência, dano e as demais magias preservadas.
- Próximo passo: validar o Furacão Flamejante no Unity Play Mode usando o botão temporário de debug para selecionar Fogo + Fogo + Ar e registrar os resultados.

### Cinco primeiras evoluções elementais sem Fogo

**Implementadas e compiladas; testes funcionais no Play Mode pendentes.** Foram associadas apenas estas receitas: Tsunami (`WaterWaterWater`), Pântano Profundo (`WaterWaterEarth`), Tempestade Glacial (`WaterWaterAir`), Planta (`WaterEarthAir`) e Colosso (`EarthEarthEarth`). Nenhum nome/receita do catálogo foi alterado.

- **Tsunami:** asset `TsunamiBehavior`, modo `Tsunami`, dano ×1,3, intervalo ×2 do intervalo normal, largura 4,5 unidades, percurso 12 unidades e empurrão 4,5 unidades/s durante 0,35 s. A onda parte do médico na direção fixada para o alvo mais próximo, atravessa inimigos, aplica dano uma vez por inimigo e se destrói ao percorrer a distância definida.
- **Pântano Profundo:** asset `DeepSwampBehavior`, modo `DeepSwamp`, área estacionária de raio 2,6 por 5 s, dano a cada 0,75 s ×0,55 e lentidão de 70%. A velocidade é registrada como fonte de área no `BaseEnemy`; ao sair, somente essa fonte é removida. Intervalo de ataque ×2,5.
- **Tempestade Glacial:** asset `GlacialStormBehavior`, modo `GlacialStorm`, área estacionária de raio 2,8 por 4 s. A cada 0,45 s surgem dois impactos em pontos aleatórios, cada um com raio 0,5 e dano ×0,65. Cada impacto deduplica seus alvos e tem 25% de chance de congelar por 1,25 s usando `BaseEnemy.ApplyFreeze`. Intervalo de ataque ×4.
- **Planta:** asset `PlantBehavior`, modo `Plant`, região de raio 2 por 3,5 s, com dano ×0,4 a cada 0,65 s e intervalo de ataque ×4. Inimigos dentro ficam presos pelo registro de congelamento por fonte em `BaseEnemy`; linhas e pequenos pulsos provisórios indicam raízes e ataques das vinhas. Sair remove apenas a fonte da Planta.
- **Colosso:** asset `ColossusBehavior`, modo `Colossus`, preparação de 0,75 s, impacto em raio 2,5 com dano ×3 e empurrão radial de 5 unidades/s durante 0,3 s. O sprite provisório cresce e desce antes do impacto; dois círculos visuais representam o golpe e a onda de choque. A ativação aplica dano uma vez por inimigo, sem dano duplicado pela onda visual. Intervalo de ataque ×5.
- `ElementalSpellController` aplica `AttackIntervalMultiplier` à cadência de cada receita e mantém as melhorias de velocidade de ataque operantes sobre o intervalo efetivo. Os valores-base e multiplicadores ficam nos cinco ScriptableObjects. O ataque continua usando o `ElementalProjectile` e o prefab compartilhados.
- `BaseEnemy` agora mantém empurrão temporário e congelamento vinculado a fontes de área. `ElementalProjectile` implementa a onda móvel larga, áreas estacionárias, impactos aleatórios da tempestade, linhas de raiz, dano por intervalo e o golpe do Colosso. `ElementalSpellBehavior` e `ElementalProjectileModifierBehavior` expõem os parâmetros e os modos novos.
- Arquivos modificados: `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs`, `Assets/Scripts/Gameplay/Characters/BaseEnemy.cs`, as definições `WaterWaterWater.asset`, `WaterWaterEarth.asset`, `WaterWaterAir.asset`, `WaterEarthAir.asset`, `EarthEarthEarth.asset` e este documento.
- Arquivos criados: `Assets/Elemental/Behaviors/TsunamiBehavior.asset` e `.meta`, `DeepSwampBehavior.asset` e `.meta`, `GlacialStormBehavior.asset` e `.meta`, `PlantBehavior.asset` e `.meta`, `ColossusBehavior.asset` e `.meta`.
- Compilação após esta atualização: `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**. A compilação valida C#, mas não importa nem executa os assets como faria o Unity Editor.
- Verificações estáticas: as cinco definições mantêm suas receitas e nomes e apontam a assets próprios; os valores dos assets correspondem aos parâmetros descritos acima. Não foi executado Play Mode.
- Testes pendentes: direção/distância/largura e empurrão do Tsunami; lentidão e saída/entrada do Pântano; distribuição, dano e chance de congelamento da Tempestade Glacial; prisão, dano periódico e limpeza da Planta; preparação, dano em área, empurrão e ausência de acerto duplo do Colosso. Também verificar que as demais 29 receitas preservam o comportamento anterior.
- Próximo passo: testar as cinco receitas no Unity Play Mode, uma por vez, e anotar os resultados antes de iniciar qualquer evolução nova.

### Reformulação — Tempestade Glacial e Planta

**Implementadas e compiladas; testes funcionais no Play Mode pendentes.** As definições e nomes `WaterWaterAir` (Tempestade Glacial) e `WaterEarthAir` (Planta) foram preservados. Apenas seus assets de comportamento e o código compartilhado necessário foram atualizados.

- **Causa do problema da Tempestade Glacial:** a versão anterior desativava os colliders do efeito e tentava causar dano apenas em pequenas regiões aleatórias de raio 0,5, sem dano contínuo ao restante da área. Vírus dentro da tempestade podiam não coincidir com esses impactos; por isso o ataque parecia não causar dano. Agora a tempestade usa o `CircleCollider2D` compartilhado, que acompanha o projétil.
- **Tempestade Glacial:** parte do médico na direção do alvo selecionado, com direção fixa. O raio é **1,5**, alcance máximo **12** unidades e velocidade normal a velocidade do projétil configurada. Enquanto pelo menos um vírus estiver na área, a tempestade avança a **40%** da velocidade; ao deixar de haver contato, volta à velocidade normal. Cada vírus recebe dano por pulso **×0,65** a cada **0,45 s**, com temporizador individual. A cada **0,45 s**, cada inimigo em contato também tem uma tentativa independente de congelamento com chance de **25%** e duração de **1,25 s**. O limite de distância é contado durante a desaceleração.
- O dano usa `m_nextDamageTime`, separado por `BaseEnemy`; tentativas de congelamento usam `m_nextFreezeAttemptTime`, também individual. `m_areaContacts` identifica contato e controla a desaceleração; as saídas e mortes removem registros. O congelamento continua usando `BaseEnemy.ApplyFreeze` e expira pelo temporizador próprio, sem referência persistente à tempestade.
- **Planta:** voltou a ser um projétil verde lançado pelo médico. No primeiro impacto, aplica dano direto **×0,8** e propaga raízes em cadeia a inimigos a até **2** unidades de cada alvo alcançado, até **6 inimigos** por disparo. Cada inimigo é atingido uma vez por volley, inclusive quando há melhoria de quantidade de projéteis. As raízes aplicam imobilização por **1 s**; o veneno causa dano **×0,25** a cada **0,5 s** durante **3 s**, continuando após a liberação.
- A propagação usa uma fila de posições, não referências vivas como origem; assim, a morte de um vírus não interrompe a cadeia. `BaseEnemy.ApplyPoison` mantém um único temporizador de veneno e reaplicações renovam o estado em vez de criarem instâncias acumuladas. `ApplyFreeze` preserva a duração restante maior de outro congelamento ativo. Linhas verdes provisórias mostram as raízes e pequenos pulsos verdes indicam a aplicação do veneno.
- As melhorias de dano continuam entrando em `GetCurrentDamage`; a velocidade de ataque continua sendo ajustada pelo multiplicador de intervalo da receita. A melhoria de quantidade de projéteis pode criar projéteis da Tempestade Glacial e da Planta; a Planta compartilha um conjunto de alvos entre eles para manter o limite de seis e impedir efeitos repetidos no mesmo volley. As áreas de Pântano Profundo e Colosso continuam limitadas a uma ativação para evitar empilhamento.
- Arquivos modificados: `Assets/Scripts/Gameplay/Spells/ElementalProjectile.cs`, `Assets/Scripts/Gameplay/Elements/ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `Assets/Scripts/Gameplay/Characters/BaseEnemy.cs`, `Assets/Elemental/Behaviors/GlacialStormBehavior.asset`, `PlantBehavior.asset` e este documento. Não foram alterados cenas, prefabs ou definições de receita.
- Parâmetros nos assets: Glacial (`m_areaRadius` 1,5; `m_damageTickInterval` 0,45; `m_areaDamageMultiplier` 0,65; `m_glacialTravelDistance` 12; `m_glacialContactSpeedMultiplier` 0,4; chance de congelar 0,25; duração 1,25; intervalo de nova tentativa 0,45). Planta (`m_plantProjectileColor` verde; dano inicial 0,8; raio de propagação 2; máximo 6; raiz 1 s; veneno 3 s, ×0,25 a cada 0,5 s).
- Compilação final: `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**. Verificação estática confirmou referências exclusivas dos dois assets e os valores serializados. Não foi executado Unity Play Mode.
- Revisão final em 20/11/2026: removida uma atribuição duplicada sem efeito ao conjunto compartilhado de alvos da Planta. Nova compilação `dotnet build COVID_SURVIVOR.slnx --no-restore`: **0 erros e 0 avisos**; associações `WaterWaterAir` → `GlacialStormBehavior` e `WaterEarthAir` → `PlantBehavior` confirmadas nos assets. Nenhum teste de Play Mode foi executado.
- Testes pendentes: verificar no Play Mode a viagem, área e dano contínuo da Glacial; desaceleração e retomada; dano em vários inimigos; chance/renovação do congelamento; alcance máximo. Para Planta: tint verde, dano de impacto, propagação em cadeia com grupos desconectados, limite de seis, deduplicação com mais de um projétil, raiz de 1 s, veneno por 3 s após a liberação e morte de alvos durante a propagação. Confirmar também melhorias de velocidade de ataque e quantidade e a preservação das demais receitas.
- Próximo passo: selecionar Água + Água + Ar e Água + Terra + Ar no Play Mode e verificar os itens pendentes antes de iniciar outra receita.

### Cinco evoluções elementais restantes e ajuste da Tempestade Glacial

**Implementadas e compiladas; testes funcionais no Play Mode pendentes.** Foram associadas somente `EarthEarthAir` (Tempestade de Areia), `EarthAirAir` (Redemoinho de Rochas), `AirAirAir` (Tornado), `WaterEarthEarth` (Avalanche de Lama) e `WaterAirAir` (Nevasca). Nomes e receitas foram mantidos.

- **Tempestade Glacial:** raio do asset aumentado de **1,5 para 2,1** unidades (+40%). `GlacialSpeedMultiplier` passou a **0,6**: velocidade normal e de contato ficam em 60% da implementação anterior, preservando a razão de contato configurada em **0,4** (ou seja, 0,24 da velocidade normal anterior). Alcance, dano ×0,65/0,45 s, chance de congelar, duração e demais parâmetros não mudaram. A escala visual e o `CircleCollider2D` usam o mesmo raio.
- **Tempestade de Areia:** `SandstormBehavior` foi associado à receita e usa o modo `SandstormVortex`. O efeito nasce no médico com direção inicial para o alvo, continua em linha sem perseguir um inimigo específico, dura **5 s**, raio **1,8**, atração de raio **3** e velocidade de atração **3,5**; causa dano ×**0,35** a cada **0,5 s**. Intervalo da receita ×**7**.
- **Redemoinho de Rochas:** `RockWhirlwindBehavior` cria oito pedras orbitais, com raio **1,2**, rotação **540°/s**, perseguição a **4 unidades/s**, dano ×**0,35** por contato e intervalo individual **0,5 s**. O centro da habilidade é calculado pela média dos vírus próximos ao alvo mais próximo; se o grupo desaparecer, o redemoinho mantém seu deslocamento até encontrar outro. Duração **4 s**, intervalo ×**5,5**. Melhorias de quantidade adicionam pedras; tamanho e alcance também afetam pedra/órbita.
- **Tornado:** `TornadoBehavior` persegue o alvo vivo mais próximo, mantendo movimento quando precisa procurar outro. A área cresce de raio **0,7 para 2** em **2 s**, atrai vírus até raio **3** com velocidade **4** e causa dano ×**0,35** a cada **0,5 s** durante **5 s**. O intervalo da receita é ×**7**. A escala visual e o collider crescem juntos.
- **Avalanche de Lama:** `MudAvalancheBehavior` dispara uma massa em linha reta por **10 unidades**, com dimensão **2 × 2,5**, dano ×**1,2** e empurrão a **4 unidades/s**. Cada inimigo recebe dano e empurrão uma vez por projétil. Um único `LineRenderer` e `EdgeCollider2D` constroem o rastro contínuo; a faixa dura **5 s**, aplica lentidão de **50%** enquanto houver contato e remove somente sua própria fonte ao sair/desaparecer. Melhorias de quantidade criam faixas adicionais; tamanho e velocidade também são aplicados.
- **Nevasca:** `BlizzardBehavior` cria uma área que acompanha o médico por **6 s**, raio **3**, dano ×**0,3** a cada **0,5 s**. A chance de congelamento é individual: começa em **5%**, cresce **10 pontos percentuais por segundo** e limita-se a **50%**, com tentativa a cada **1 s** e congelamento por **1,25 s**. Sair da área remove a progressão daquele inimigo; reentrada começa em 5%. A aura reaparece segundo o intervalo de disparo ×**8,5** (6,375 s no valor-base, antes das melhorias), deixando uma pausa curta entre ativações.
- A atração é registrada por fonte em `BaseEnemy.SetAttraction`/`RemoveAttraction`; o inimigo escolhe a força ativa mais forte e a fonte é removida ao sair do raio ou destruir o efeito. `EnterSlowArea(Component, ...)`/`ExitSlowArea(Component)` permitem que o rastro use o controle de lentidão existente sem sobrepor a restauração de outras fontes.
- Scripts criados: `ElementalMovingVortex.cs`, `ElementalRockWhirlwind.cs`, `ElementalOrbitingRock.cs`, `ElementalMudAvalanche.cs`, `ElementalMudTrail.cs` e `ElementalBlizzardEffect.cs` (com respectivos `.meta`). Os assets criados são `RockWhirlwindBehavior.asset`, `TornadoBehavior.asset`, `MudAvalancheBehavior.asset` e `BlizzardBehavior.asset` (com `.meta`). `SandstormBehavior.asset` já existia e foi reutilizado.
- Scripts modificados: `ElementalSpellBehavior.cs`, `ElementalProjectileModifierBehavior.cs`, `ElementalSpellController.cs`, `ElementalProjectile.cs` e `BaseEnemy.cs`. Assets modificados: `GlacialStormBehavior.asset`, `SandstormBehavior.asset`; definições modificadas: `EarthEarthAir.asset`, `EarthAirAir.asset`, `AirAirAir.asset`, `WaterEarthEarth.asset`, `WaterAirAir.asset`; e este documento. Nenhuma cena, prefab, catálogo ou regra de level-up foi alterada.
- Compilação: `dotnet build COVID_SURVIVOR.slnx --no-restore`, **0 erros e 0 avisos**. Os seis scripts novos foram adicionados temporariamente ao `.csproj` gerado apenas durante a compilação; o arquivo gerado foi restaurado ao fim. Verificação estática confirmou as cinco associações receita→asset e os parâmetros serializados das cinco novas evoluções e do asset glacial.
- Não foi executado Unity Play Mode. A janela do Unity está aberta na `SampleScene`, mas nenhuma mecânica foi executada nesta validação.
- Pendências: testar visual, movimento, aquisição/troca de alvo, dano e expiração da Tempestade de Areia; alvo-grupo, órbita e intervalo individual das pedras; crescimento, dano e atração do Tornado; colisão, empurrão, continuidade do rastro, lenta e restauração da Avalanche; progressão/reinício de chance, dano e acompanhamento da Nevasca; aumento de 40% e velocidades da Tempestade Glacial. Repetir com melhorias de dano, velocidade, alcance e quantidade, e confirmar que as receitas antigas continuam inalteradas.
- Próximo passo: validar uma receita por vez no Play Mode, começando pela Tempestade Glacial e seguindo pelas cinco novas, e registrar os resultados antes de novos ajustes.

## Etapa — sprites e animações do médico e do vírus (23/11/2026)

**Implementado e compilável em teoria; sem validação no Unity Editor ainda.** Escopo autorizado: arte pixel art de alta qualidade seguindo `References/Art/doctor_reference.png`, animações com número de frames definido por mim, e remoção dos sons desta etapa. Cronômetro, pontuação e cenas (Menu → Jogo → Derrota com reinício) ficam para a próxima etapa, após confirmação.

### Decisões técnicas

- **Pixel art por regras geométricas**, não por reamostragem da referência: a amostragem direta gerou contornos e cabelo irregulares. Cada pixel é testado pelo centro, sem anti-aliasing. As cores foram extraídas da referência.
- **Escala de exportação**: médico em canvas 24×32 ampliado em 3× (72×96); vírus em 24×24 ampliado em 3× (72×72). `spritePixelsToUnits` ajustado (76,8 e 72) para manter o mesmo tamanho em mundo dos placeholders (1,25 e 1 unidade).
- **Importação**: `textureType` Sprite, `filterMode` Point (pixel perfeito) e sem compressão. Os `.meta` foram gerados a partir do modelo do sprite provisório.
- **Frames em `Resources`**: cada clipe é uma pasta com `frame_00.png`, `frame_01.png`, etc. O script lê os frames pelo nome, sem exigir arraste de sprites no Inspector.
- **Animador adicionado em código** (`AddComponent`) na inicialização do Player e do Virus. Isso evita editar à mão o YAML do prefab e da cena, o que não consigo validar sem o Unity.
- **Morte com espera**: o inimigo desativa colisores e Rigidbody imediatamente (como antes), mas só é destruído quando a animação de morte termina. Se não houver clipe `Death`, o comportamento anterior é mantido.

### Animações

| Personagem | Clipe | Frames | FPS | Loop |
|---|---|---|---|---|
| Médico | Idle | 4 | 6 | sim |
| Médico | Walk | 6 | 10 | sim |
| Médico | Death | 6 | 8 | não |
| Vírus | Walk | 6 | 10 | sim |
| Vírus | Death | 6 | 10 | não |

- Médico Idle: respiração (sobe e desce 1 px). Walk: pernas alternam, pés levantam no passo, corpo sobe e desce. Death: achata e some com padrão Bayer.
- Vírus Walk: espículas giram (±7,5°) e o corpo oscila. Death: estoura, encolhe e some com padrão Bayer.

### Arquivos criados

- `Assets/Resources/Characters/Doctor/{Idle,Walk,Death}/frame_XX.png` + `.meta` (16 frames)
- `Assets/Resources/Characters/Virus/{Walk,Death}/frame_XX.png` + `.meta` (12 frames)
- `Assets/Resources/Characters/` e subpastas, com `.meta` de pasta
- `Assets/Scripts/Gameplay/Animation/SpriteAnimator.cs` + `.meta` (flipbook: Play, PlayOnce, HasClip, callback ao terminar)
- `References/Art/Generator/common.py`, `doctor.py`, `virus.py` (geradores reproduzíveis dos frames; fora de `Assets/` para não serem importados pelo Unity)
- Prévia fora do projeto: `previa_personagens.png`

### Arquivos modificados

- `Assets/Scripts/Gameplay/Characters/Player.cs`: campo `m_animator`; criação em `Setup`; `UpdateLogic` alterna Idle/Walk; `OnDeath` toca Death (o fim de jogo ainda não existe).
- `Assets/Scripts/Gameplay/Characters/BaseEnemy.cs`: `OnDeath` toca Death e destrói o objeto ao terminar; mantém o caminho antigo quando não há clipe.
- `Assets/Scripts/Gameplay/Characters/Virus.cs`: `Setup` cria o animador com a pasta `Characters/Virus`; `UpdateEnemyLogic` toca Walk.
- Não foram alterados: cenas, prefabs, `ElementalSpellController`, sistemas elementais, HUD ou botão de debug.

### Testes

- Verificação estrutural dos scripts alterados (chaves, parênteses e colchetes balanceados): OK.
- Não há compilador C# neste ambiente; a compilação precisa ser confirmada no Unity Editor.
- Inspeção visual dos frames (ampliados e em sequência) feita nesta etapa; sequências de andar e morte conferidas.
- Pendente no Play Mode: o médico troca de Idle para Walk ao mover e volta a Idle ao parar; a animação não reinicia a cada quadro; o vírus anda com Walk; ao morrer, o vírus toca Death e só então some; o XP continua sendo concedido na morte; o médico toca Death ao morrer, sem erros no Console; a troca de sprites não aparece com borrão (filtro Point).
- Pendente: confirmar no Unity que `Resources.LoadAll` encontra os frames na ordem correta.

### Problemas conhecidos e pendências

- O sprite da cena ainda é o placeholder até o primeiro quadro de `Update`; a troca é imediata, mas pode haver um quadro com o placeholder.
- Não há animação de ataque do médico: o disparo é feito pelo `ElementalSpellController`, e vincular a animação exigiria alterar esse sistema. Pode ser feito em etapa futura, se autorizado.
- Sem efeito visual de dano (flash). Pode ser adicionado depois.
- O arquivo `.csproj` gerado não inclui o novo script até o Unity regenerar o projeto C#.

### Próxima etapa (aguardando confirmação)

Validar no Play Mode os itens pendentes acima. Após confirmação, implementar, nesta ordem: menu inicial, cenas, cronômetro, pontuação e tela de derrota com reinício. Os sons ficam para a etapa seguinte, conforme combinado.


## Etapa — animação de ataque, fluxo de cenas e placar (23/11/2026)

**Escopo autorizado:** conectar a animação de ataque ao `ElementalSpellController` existente; concluir menu inicial → partida infinita → derrota/reinício; não incluir sons. As receitas e upgrades anteriores foram mantidos.

### Conceito e implementação

- **Visual:** `Assets/Resources/Characters/Doctor/Attack/` contém 8 quadros de 72×96 px, RGBA, Point Filtering e mesmo pixels-per-unit do médico. Os novos desenhos mantêm a cabeça, proporção, roupa e posição dos pés do `Idle`; uma seringa é levantada e aparece um pequeno brilho ciano. A referência visual de conceito fica em `References/Art/doctor_attack_concept.png` e não participa do build.
- **Médico:** `Player.PlayAttackAnimation(Vector2)` toca uma animação não repetível a **14 fps**. O ataque tem prioridade visual sobre Idle/Walk, mas não impede movimentação; a imagem vira horizontalmente conforme a direção do ataque. O clipe não é reiniciado caso outro projétil saia antes de acabar. Ao morrer, `Death` tem prioridade.
- **Inimigo:** o primeiro quadro do vírus é mostrado durante `Setup`, antes do primeiro Update, eliminando o placeholder temporário. O médico passa a apresentar o primeiro quadro do Idle em `Setup` pelo mesmo motivo.
- **Combate real:** `ElementalSpellController.FireAt` chama a animação antes de disparar. Modos persistentes (`ContinuousCone`, `OrbitingFire`, `InfernoAura`, `Blizzard`) chamam o efeito visual com intervalo para não depender de outro sistema de ataques. Todos os efeitos, alvos, cor, dano e receitas existentes permanecem no controlador.
- **Ondas e placar:** `WaveManager.TotalDefeated` acumula mortes confirmadas durante todas as ondas, sem alterar `DefeatedThisWave`. O placar é **100 pontos por vírus + 1 ponto por segundo completo sobrevivido**. O cronômetro não avança durante a pausa de escolha de upgrade.
- **Cenas:** `MainMenu.unity` apresenta instruções, Jogar e Sair; `SampleScene.unity` é o jogo, com `GameRunManager` configurado no `VirusSpawner`; `GameOver.unity` apresenta pontuação, eliminações, tempo e onda, com Reiniciar e Menu Principal. As três cenas estão em `EditorBuildSettings.asset` nessa ordem. Não há vitória.
- **Derrota:** o `GameRunManager` registra o resultado quando o `Player.Died` é emitido, interrompe a corrotina do spawner, aguarda 0,85 s para a animação de morte (6 quadros a 8 fps) e muda para `GameOver`; reiniciar limpa o resultado anterior e carrega uma nova `SampleScene`.
- **Interface:** `SceneUIFactory` cria os canvases, painéis e botões no início da cena, com `InputSystemUIInputModule.AssignDefaultActions()` para os cliques. A UI nova não requer montagem manual de Canvas.
- **Limpeza:** retirados o botão de debug e suas ações em `UpgradeChoiceUI`, `UpgradeChoiceManager` e `ExperienceManager`; removida a condição de vitória antiga da descrição do objetivo no `PROJECT_CONTEXT.md`. Não foram inseridos áudios.

### Requisitos da referência e frames presentes

| Personagem | Clipe | Frames | Estado |
|---|---|---:|---|
| Médico | Idle | 4 | Preservado |
| Médico | Walk | 6 | Preservado |
| Médico | Death | 6 | Preservado |
| Médico | Attack | 8 | Criado e conectado ao ataque elemental |
| Vírus | Walk | 6 | Preservado |
| Vírus | Death | 6 | Preservado |

### Arquivos adicionados

- `Assets/Resources/Characters/Doctor/Attack.meta` e os oito `frame_00.png` a `frame_07.png` com seus `.meta`.
- `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/GameOver.unity` e respectivos `.meta`.
- `Assets/Scripts/Gameplay/GameRunManager.cs`, `Assets/Scripts/Gameplay/GameRunState.cs`, `Assets/Scripts/UI/SceneUIFactory.cs`, `Assets/Scripts/UI/MainMenuUI.cs`, `Assets/Scripts/UI/GameOverUI.cs` e `.meta` de cada script.
- `References/Art/doctor_attack_concept.png` (conceito não importado em Assets), `README_JOGO.md` e `QA_STATIC.json`.

### Arquivos alterados

- `Assets/Scripts/Gameplay/Animation/SpriteAnimator.cs` (consulta ao clipe em reprodução).
- `Assets/Scripts/Gameplay/Characters/Player.cs` (Idle inicial, ataque, prioridade de animação, espelho do sprite).
- `Assets/Scripts/Gameplay/Characters/Virus.cs` (Walk inicial).
- `Assets/Scripts/Gameplay/Elements/ElementalSpellController.cs` (sinal de ataque normal e persistente).
- `Assets/Scripts/Gameplay/WaveManager.cs` (abates totais).
- `Assets/Scripts/UI/UpgradeChoiceUI.cs`, `Assets/Scripts/Gameplay/UpgradeChoiceManager.cs`, `Assets/Scripts/Gameplay/ExperienceManager.cs` (retirada do debug).
- `Assets/Scenes/SampleScene.unity` (`GameRunManager` no `VirusSpawner`), `ProjectSettings/EditorBuildSettings.asset` (ordem das cenas), `PROJECT_CONTEXT.md` e este arquivo.

### Verificações efetuadas

- Auditoria estática (`QA_STATIC.json`): frames, resolução, filtros Point, transparência, integridade dos metadados e GUIDs, serialização YAML e componentes das cenas, ordem de Build Settings, vínculos do ataque e inexistência das chamadas removidas de debug. **Sem falhas na auditoria estática.**
- Arquivos de imagem PNG conferidos quanto ao tamanho e sequência. `Doctor/Attack` contém pelo menos seis quadros visualmente distintos; começa no mesmo Idle de referência.
- **Limitação:** o Unity Editor e um compilador C# para Unity não estão disponíveis neste ambiente. Não foi executada compilação de Unity, Play Mode ou build executável. Essas funcionalidades devem ser consideradas *implementadas, mas pendentes de validação funcional*. A auditoria não substitui abrir as cenas nem testar colisões/receitas.

### Teste manual recomendado no Unity

1. Abra o projeto em Unity **6000.5.8f1**, aguarde a importação e confira a Console por erros de C# e assets.
2. Abra `Assets/Scenes/MainMenu.unity` e dê Play: veja o menu; clique **JOGAR** para entrar no jogo.
3. Mova o médico com WASD e setas: verifique Idle/Walk, sprites e animação sem tremulações; acompanhe o cronômetro.
4. Colete cristais até abrir uma tela de nível; confirme a pausa, quatro opções e retorno ao jogo. Escolha Água, Fogo, Terra ou Ar.
5. Aproxime-se de vírus: observe o médico sacar a seringa durante os disparos automáticos, com espelhamento e movimentação simultânea. Confira os projéteis reais e os danos.
6. Confira que os abates aumentam o score em 100 pontos, que a contagem de tempo acrescenta 1 ponto/s e que o placar prossegue pelas ondas.
7. Tome dano até a morte: confira Death, transição para tela de derrota, resultado final e dados da partida.
8. Clique **REINICIAR**: confirme tempo, pontos, onda, elementos, XP e vida zerados/restaurados à configuração inicial. Retorne ao menu pelo botão **MENU PRINCIPAL**.
9. Em evoluções que usem aura, órbita, cone ou nevasca, veja se a animação visual acompanha os pulsos sem alterar os comportamentos.
10. Faça um build desktop e repita o fluxo de cenas, para verificar interações do mouse e resolução de tela.

### Pendências e próximo passo

**Pendente:** compilação no Editor; teste de Play Mode dos botões, seleção de upgrade, animações e 30 comportamentos especiais das receitas anteriores; avaliação visual do menu em resoluções menores, performance com dezenas de vírus e build PC. Sem som nesta entrega. A próxima etapa recomendada é o usuário testar estes itens e trazer quaisquer erros da Console ou ajustes visuais desejados antes de evoluir novos recursos.

## Git e compartilhamento

- Configurado `.gitignore` para caches e arquivos gerados do Unity, IDE e macOS. `Assets/`, `Packages/`, `ProjectSettings/`, arquivos `.meta` e documentos do projeto permanecem incluídos.
- O histórico local da branch `main` foi reiniciado porque o histórico anterior continha arquivos de `Library/` com até 137,67 MB. Nenhum arquivo de projeto ou cache foi apagado do disco; caches e configurações locais agora ficam fora do commit.
- Commit inicial limpo criado com a mensagem `Initial project setup - COVID SURVIVOR`. `Assets`, `Packages`, `ProjectSettings` e `.meta` foram conferidos; não há blobs rastreados acima de 100 MB e a árvore de trabalho estava limpa após o commit.
- O push está pendente: a consulta ao GitHub não pôde ser feita por falta de conexão. O remoto local atual usa `COVID-SURVIVOR`, enquanto o endereço informado pelo usuário usa `COVID_SURVIVOR`; é necessário confirmar o destino e sua visibilidade privada antes de enviar.
- Próximo passo: confirmar o URL correto e a visibilidade privada no GitHub, então enviar somente a branch `main`.


## Etapa — requisitos do professor, sons e correção do combate legado (23/11/2026)

**Autorização:** o usuário pediu explicitamente rever todos os itens do enunciado,
adicionar sons e verificar o Unity. Esta etapa não substitui testes no Editor não
instalado neste ambiente. O resultado é um pacote **para teste no Unity**, não
uma versão final aprovada.

### O que foi feito

- **Áudio original:** foram criados 11 WAVs PCM 16-bit mono 22.050 Hz por síntese
  digital própria (nenhum trecho comercial): `menu_loop`, `game_loop`,
  `defeat_loop`, `ui_click`, `shoot`, `enemy_hit`, `enemy_death`, `player_hit`,
  `pickup`, `level_up`, `defeat`.
- **Integração em runtime:** `CovidAudioManager` é criado ao primeiro uso,
  persiste por `DontDestroyOnLoad`, usa duas `AudioSource`s 2D (música em loop
  e efeitos de um toque), troca músicas por cena, carrega via `Resources/Audio`
  e aplica limites de repetição dos efeitos mais frequentes.
- **Listeners:** como menu e GameOver só têm GameObject com script e Canvas,
  `SceneUIFactory.EnsureMenuAudioListener` adiciona um AudioListener nesses
  menus; a cena de jogo já contém AudioListener na câmera.
- **Eventos ligados:** menu, jogo, derrota, clique, disparo normal/persistente,
  dano no médico e vírus, morte dos vírus, XP coletado, level-up e derrota.
- **Falha identificada e corrigida:** o `VaccineSpell` legado estava **habilitado**
  em `SampleScene` e poderia disparar automaticamente antes da escolha do
  elemento. O campo `m_Enabled` da instância foi ajustado para 0; arquivos
  originais e lógica do legado foram preservados. O único sistema ativo de
  ataque do jogador agora é o elemental.
- **Auditoria:** `Assets/Editor/CovidProjectAudit.cs` adiciona o comando
  `Tools > COVID SURVIVOR > Auditar projeto` para executar *dentro* do Editor;
  este comando não foi executado aqui, pois Unity Editor não está instalado.
- **Checklist:** `CHECKLIST_PROFESSOR.md` confronta explicitamente os seis
  requisitos; `TESTES_NO_UNITY.md` documenta os testes práticos, e
  `QA_SUBMISSION.json` registra a auditoria estática.

### Novos arquivos

- `Assets/Resources/Audio/Music/*.wav` (3) e `.meta` de cada um.
- `Assets/Resources/Audio/SFX/*.wav` (8) e `.meta` de cada um.
- Metas de pastas `Audio`, `Music`, `SFX`, `Scripts/Audio`, `Editor`.
- `Assets/Scripts/Audio/CovidAudioManager.cs` e `.meta`.
- `Assets/Editor/CovidProjectAudit.cs` e `.meta`.
- `AUDIO_MANIFEST.json`, `QA_SUBMISSION.json`, `CHECKLIST_PROFESSOR.md`,
  `TESTES_NO_UNITY.md`.

### Arquivos modificados

- `Assets/Scenes/SampleScene.unity` (desativação do VaccineSpell legado).
- `Assets/Scripts/UI/SceneUIFactory.cs` (AudioListener de menu e cliques).
- `Assets/Scripts/UI/MainMenuUI.cs`, `GameOverUI.cs`, `UpgradeChoiceUI.cs`.
- `Assets/Scripts/Gameplay/GameRunManager.cs`, `ExperienceOrb.cs`,
  `UpgradeChoiceManager.cs`, `Characters/BaseCharacter.cs`,
  `Characters/BaseEnemy.cs`, `Characters/Player.cs`,
  `Elements/ElementalSpellController.cs` (eventos sonoros e comentários).
- `README_JOGO.md` e este documento de status.

### Testes realizados e limitações

- `validate_covid.py`: auditoria estrutural antiga concluída sem falhas.
- `qa_submission.py`: **82/82** verificações de estrutura, metadados, GUID,
  sprites/quadros/Point, formato e sinal de áudio, cenas, referências, prefabs,
  scripts e hooks sonoros passaram. 11 WAVs distintos, sem áudio vazio ou clipping.
- `Unity Editor`: **não instalado**, não abriu o projeto nesta etapa.
- Compilação C# do jogo: **não realizada** nesta etapa.
- Play Mode, testes visuais/auditivos, build executável: **não realizados**.
- Testes funcionais de receitas elementais continuam pendentes, como nas
  seções anteriores do histórico. Não inferir estabilidade de 30+ receitas
  especiais a partir da inspeção estática.

### Próximo passo para concluir a entrega

Executar `Tools > COVID SURVIVOR > Auditar projeto` no Unity **6000.5.8f1**,
seguir `TESTES_NO_UNITY.md`, corrigir os erros identificados e repetir os
cenários, incluindo build Windows. Só depois classificar o jogo como final.


## Etapa — retorno do ataque inicial e remoção do auditor (23/11/2026)

**Pedido do usuário:** remover a ferramenta própria de auditoria do Unity, reativar o `VaccineSpell`, garantir que o médico comece atacando e resolver as interações dessa arma com as magias e com as animações.

### Decisões e funcionamento

- A vacina é um **ataque inicial e permanente**: é automática, busca o inimigo vivo mais próximo a até 6 unidades, dispara com intervalo-base de 0,75 s e dano-base de 10. O médico não precisa desbloquear elementos para atacar.
- O `ElementalSpellController` continua intacto no fluxo de escolhas: desbloqueia após a primeira escolha elemental. Vacina e magia ficam **ativas em paralelo** a partir desse momento. As receitas, os comportamentos de magias e as cartas de melhoria foram preservados.
- Ambos acionam `Player.PlayAttackAnimation` e `SoundCue.Shoot`. A proteção que não reinicia o clipe `Attack` já em execução e o cooldown do gerenciador de áudio evitam tremulação ou disparo de SFX duplo no mesmo instante.
- Ajuste de espelhamento: os sprites de ataque originais exibem a seringa do lado esquerdo, portanto o ataque espelha horizontalmente quando mira à direita.
- As melhorias gerais de `PlayerStats` **DamageBonus** e **AttackIntervalReduction** passam a atuar também no ataque básico via `BaseSpell`. As melhorias específicas elementais continuam exclusivas ao `ElementalSpellController`. O intervalo mínimo da vacina é 0,1 s.

### Modificações

- `Assets/Scenes/SampleScene.unity`: habilitado `VaccineSpell` no Player com o prefab original ainda vinculado.
- `Assets/Scripts/Gameplay/Spells/BaseSpell.cs`: cálculo de dano e intervalo considerando `PlayerStats`.
- `Assets/Scripts/Gameplay/Spells/VaccineSpell.cs`: animação, SFX e dano com bônus geral.
- `Assets/Scripts/Gameplay/Characters/Player.cs`: correção da direção do sprite de ataque.
- Removidos: `Assets/Editor/CovidProjectAudit.cs` e respectivo `.meta`, pasta `Assets/Editor`/`.meta` e relatórios temporários `QA_SUBMISSION.json` e `QA_STATIC.json` de verificações anteriores. **A verificação automática no menu Tools deixa de existir.** O histórico de testes antigos permanece documentado nesta seção de status.
- Atualizados `README_JOGO.md`, `TESTES_NO_UNITY.md`, `CHECKLIST_PROFESSOR.md` e `DEVELOPMENT_STATUS.md` para as regras atuais.

### Verificação e pendências

- Conferir integridade de arquivos Unity, GUIDs, referências serializadas, arte, áudios, prefabs e ordem das cenas via scripts externos durante o empacotamento.
- **Unity Editor não disponível neste ambiente**: compilação C# na engine, Play Mode (inclusive disparo inicial), audio listeners, colisão da vacina, upgrade duplo, receitas elementais e build Windows precisam ser testados no Unity 6000.5.8f1 pelo roteiro.
- Próximo passo: abrir no Unity, conferir a Console sem erros e executar `TESTES_NO_UNITY.md`, em especial os casos novos da vacina e sua coexistência com o elemental. Não classificar como final antes disso.


## Etapa — controle de poluição sonora e prioridade visual (23/11/2026)

**Pedido do usuário:** com as duas armas disparando juntas e a cadência aumentando, evitar sobreposição excessiva de SFX e animações. Preservar o ataque inicial da vacina e a magia elemental.

### Decisão de gameplay

- A vacina **nunca é desligada** e continua com busca de alvo, disparos, dano, melhorias e prefabs originais.
- **Antes do primeiro elemento:** `VaccineSpell` solicita a animação `Attack` e o SFX `shoot`.
- **Depois do primeiro elemento:** `VaccineSpell` continua disparando, porém seu SFX e sua solicitação de animação ficam silenciosos. O `ElementalSpellController` assume exclusivamente o feedback da animação e do som de ataque. Se o controlador elemental estiver desabilitado, o feedback da vacina volta a funcionar como contingência.
- **Magias de projétil:** somente um aviso ao lançar a salva, independentemente do número de projéteis; áudio `element_cast` limitado a 1 por 0,9 segundo. O áudio não altera a cadência real de disparo.
- **Magias persistentes:** efeito visual e dano continuam com suas regras existentes; o médico só solicita animação/SFX de ataque quando houver inimigo ao alcance, verificando no máximo a cada 1,05 segundo. O SFX ainda respeita 0,9 segundo mínimo.
- `Player.PlayAttackAnimation`: trava visual de 0,68 s para a vacina inicial e 1,05 s para o elemental (ambos configuráveis no Inspector), sem reiniciar clipe `Attack` em andamento. Assim o movimento volta a aparecer entre ataques mesmo com cadência alta. O movimento e o dano não sofrem essa trava.
- Sons frequentes de acerto, morte e coleta têm cooldown maior e volume mais baixo. Alertas importantes (level-up, dano do jogador, derrota) continuam audíveis. O fundo musical e o áudio de UI foram preservados.
- Criado SFX **original** `element_cast.wav` (WAV PCM 16-bit mono, 22.050 Hz) com `.meta` Unity e metadados em `AUDIO_MANIFEST.json`. Agora são 3 músicas e 9 SFX.

### Arquivos modificados

- `Assets/Scripts/Gameplay/Spells/VaccineSpell.cs` — consulta estado do elemental e retira feedback duplicado após desbloqueio.
- `Assets/Scripts/Gameplay/Characters/Player.cs` — cooldown visual independente da cadência dos projéteis.
- `Assets/Scripts/Gameplay/Elements/ElementalSpellController.cs` — SFX elemental dedicado por salva e feedback reduzido de efeitos persistentes.
- `Assets/Scripts/Audio/CovidAudioManager.cs` — cue elemental, cooldowns e níveis por evento.
- `Assets/Resources/Audio/SFX/element_cast.wav` e `.meta` — novo SFX discreto.
- `AUDIO_MANIFEST.json`, `README_JOGO.md`, `TESTES_NO_UNITY.md`, `CHECKLIST_PROFESSOR.md`, `DEVELOPMENT_STATUS.md` — documentação.

### Validação

- Conferência estática da versão atual: **128/128 checagens aprovadas** (presença de Assets/Packages/ProjectSettings, versão, cenas, configuração VaccineSpell ativo, GUIDs, prefabs, animações, 12 WAVs e pontos de integração). Comparação com a versão anterior: nenhum arquivo removido, 2 assets novos e 9 arquivos alterados. ZIP íntegro.
- Sem compilador C# ou Unity Editor neste ambiente; não foi possível executar Play Mode nem confirmar percepção auditiva no jogo.
- **Pendente:** validar em Play Mode que antes do primeiro elemento a vacina soa/anima; após desbloquear, vacina permanece funcional e silenciosa; elemental anima/soa sem efeitos repetitivos; ouvir SFX em onda avançada e testar reinício.
- **Próximo passo:** abrir o pacote no Unity 6000.5.8f1, executar `TESTES_NO_UNITY.md`, revisar volumes por audição e ajustar eventuais problemas de compilação/execução.


## Etapa — substituição visual do médico pela referência (23/11/2026)

**Pedido:** integrar diretamente no projeto o médico de cabelo branco, olhos azuis,
jaleco branco e roupas azuis da referência visual enviada, com animações completas.

### Alterações e motivos

- Foram substituídos **24 PNGs** das quatro pastas em
  `Assets/Resources/Characters/Doctor/`: `Idle` (4), `Walk` (6),
  `Attack` (8) e `Death` (6). Todos mantiveram seus nomes `frame_XX.png`
  e **GUIDs dos arquivos `.meta`**, preservando carregamento por `SpriteAnimator`.
- Os quadros foram individualmente extraídos da arte enviada, sem depender do
  Sprite Editor ou de cortes manuais. Como a fonte possui espaçamentos irregulares,
  cada pose foi isolada, alinhada em canvas transparente de **256 × 224 px**
  e centrada pelo corpo, para evitar tremulação durante as transições.
- Importadores Unity dos 24 PNGs preservam `Sprite (2D)`, alpha e filtro `Point`,
  sem compressão de textura. Pixels Per Unit definido para **166.4**,
  equilibrando o tamanho do personagem no mundo com o anterior (76.8 PPU
  para quadros de 72 × 96 px). Pivot central 0.5 × 0.5.
- `SampleScene.unity`: o `SpriteRenderer` do Player agora aponta para
  `Doctor/Idle/frame_00.png`, inclusive na visualização sem executar Play.
- `Player.cs`: como a nova animação de ataque aponta naturalmente para a
  **direita**, `flipX` passa a ser ativado ao atacar para a esquerda.
  A lógica de seleção entre `VaccineSpell` e magias elementais, os respectivos
  limites de áudio/animação, upgrades, cenas e prefabs **não foi modificada**.
- Incluído `References/Art/doctor_animation_source.png` como fonte integral
  da arte utilizada para gerar os quadros. Ele fica fora de Assets e não
  consome recursos na compilação do jogo.
- `SpriteAnimator.cs` continua responsável por executar os clipes via
  `Resources.LoadAll<Sprite>`; este projeto **não utiliza Animator Controller
  para esse personagem**, portanto não foram criados `.anim` redundantes.

### Verificações e testes pendentes

- Verificação estática dos 24 PNGs, formatos, dimensões, `.meta`, GUIDs,
  cenas e scripts; ZIP validado por CRC ao empacotar.
- Compilação C#, importação efetiva dos novos sprites, Play Mode, tamanho
  na câmera, colisões, transições e desempenho **dependem do Unity Editor**.
- Teste recomendado: abrir `Assets/Scenes/MainMenu.unity`, iniciar partida,
  observar Idle/Walk; esperar vírus para vacina; verificar Attack com e sem
  magia; tomar dano suficiente para visualizar Death e o GameOver; reiniciar.
- **Próximo passo:** testar no Unity 6000.5.8f1, anotar erros de Console e
  ajustar o projeto se houver comportamentos inesperados. Não chamar de final
  antes desses testes.


## Etapa — novos vírus, Colosso mini-boss e Núcleo Mutante boss (23/11/2026)

**Autorizado pelo usuário:** substituir o vírus verde por variantes pretas com faixas brancas, adicionar corredor, tanque, mini-boss C (Colosso Blindado) e boss B (Núcleo Mutante). Reduzir as garras do boss, preservando o pulso de energia e a morte em explosão.

### Arte

- Sprites transparentes, PNGs individuais em `Assets/Resources/Characters/` nas pastas `Virus`, `VirusRunner`, `VirusTank`, `VirusColossus` e `VirusNucleusBoss`. Total: 72 frames de Walk, Attack, Death e Idle distribuídos entre os cinco inimigos.
- O vírus comum mantém seus GUIDs antigos de Walk/Death, portanto o prefab original e as referências existentes continuam funcionando.
- No boss, o desenho é mais viral (esfera, espinhos curtos, faixas brancas) e as animações de explosão e pulso de energia permanecem.
- A arte do Colosso deriva da opção C aprovada, com armadura preta e branca, olho vermelho durante a carga, movimento pesado e morte com ruínas.
- Sprites utilizam filtro Point, alpha e compressão desabilitada.

### Gameplay e integração

- `Virus.cs`: variantes compartilham código de perseguir, contato, XP, estados elementais e animação. Parâmetros próprios de vida, movimento, recompensa e ataque de pulso são configurados em cada prefab.
- `EnemySpawner.cs`: vírus comum desde o início; corredores desde onda 2; tanques desde onda 4; Colosso nas ondas 5, 15, 25 ...; Núcleo Mutante nas ondas 10, 20, 30 ... (os chefes têm prioridade nos múltiplos de 10).
- O boss e o mini-boss emitem pulso em área com aviso visual pelo clipe Attack, checagem de distância e recarga. Apenas o jogador em alcance recebe dano, não há projéteis extras e não se adicionam sons por projétil.
- `WaveManager`, vacinas, magias elementais, XP, HUD, menu e derrota permanecem como antes. O jogo segue infinito, sem condição de vitória.
- Foram criados prefabs `VirusRunner`, `VirusTank`, `VirusColossus`, `VirusNucleusBoss`; o `Virus` existente foi atualizado para a nova arte.

### Testes e pendências

- Verificar PNGs, alpha e GUIDs, referências de cena, variantes de prefab e código de spawn por análise estática.
- **Unity Editor não disponível**: compilação C#, Play Mode, balanço de dano, tamanho em cena, sincronização da animação e build Windows ainda dependem de testes na engine.
- Roteiro: iniciar no MainMenu, conferir vírus comum; avançar até as ondas 2, 4, 5 e 10; testar pulse e morte dos chefes, XP, dano elemental, vacina inicial, HUD e derrota/reinício.
