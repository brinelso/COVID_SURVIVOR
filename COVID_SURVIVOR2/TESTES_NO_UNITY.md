# Roteiro de validação no Unity — COVID SURVIVOR

> **PENDENTE** — Este arquivo é um roteiro; a execução real no Unity ainda não foi
> possível neste ambiente. Marque [x] somente depois de observar o teste.

## 1. Importação e Console

- [ ] Abrir no Unity Hub, selecionar `6000.5.8f1`, aguardar todo o processamento.
- [ ] Verificar Console sem erros de compilação, `Missing Script` ou referências ausentes.
- [ ] Confirmar que **não há o menu de auditoria personalizada** (ele foi removido do projeto).
- [ ] Conferir `Project Settings > Player > Active Input Handling`: **Input System Package**.
- [ ] Em `File > Build Profiles > Scene List`: MainMenu, SampleScene, GameOver (nessa ordem).

## 2. Menu e som de UI

- [ ] Abrir `Assets/Scenes/MainMenu.unity`, pressionar Play; aparecem título e dois botões.
- [ ] Escutar a música do menu (sem avisos de `no audio listeners` nem `multiple audio listeners`).
- [ ] Clicar **JOGAR**: escutar clique, abrir a arena, trocar para a música da partida.

## 3. Arena, HUD e animação

- [ ] O médico aparece corretamente, sem quadrado provisório ou sprite desfocado.
- [ ] WASD e setas movem em todas as direções e diagonais sem velocidade dobrada.
- [ ] Idle ao parar e Walk ao mover, sem tremulação ou quadro de ataque preso.
- [ ] Vírus aparecem perto do jogador, perseguem, animam e causam dano por contato.
- [ ] HUD mostra vida, barra de XP, onda, cronômetro e pontos; não sobrepõe cartas.
- [ ] **Antes de selecionar qualquer elemento**, o médico dispara automaticamente vacinas nos vírus próximos (alcance ~6), causa dano e consegue eliminá-los.
- [ ] **Sem elemento**, somente os disparos de vacina acionam a animação Attack e o som de vacina; o intervalo sonoro é confortável.
- [ ] Mover-se e mirar para a esquerda/direita: a seringa do sprite acompanha o alvo e o médico pode continuar caminhando.
- [ ] Escolher um upgrade geral de **dano** ou **cadência**: o tiro de vacina passa a aplicar o bônus correspondente.
- [ ] Coletar cristais: escutar som, acumular XP, atualizar barra.

## 4. Melhorias e combate elemental

- [ ] Ao subir de nível, tocar som de evolução e abrir **quatro** opções clicáveis.
- [ ] O tempo/ondas param durante a escolha; a música e o clique continuam audíveis.
- [ ] Selecionar primeiro elemento: o jogo retoma, a vacina **continua funcionando** e o ataque elemental automático também começa.
- [ ] **Com elemento desbloqueado**, a vacina segue voando e causando dano, mas não faz nenhum som nem reinicia a animação. Só os disparos elementais solicitam feedback Attack e SFX elemental.
- [ ] Com vários projéteis por salva, há **no máximo um som elemental por 0,9 s**, não um som por projétil; o personagem alterna Attack e Walk/Idle sem tremular; o ataque elemental tem pausa visual mínima de 1,05 s.
- [ ] Em magia persistente (órbita, cone, aura ou nevasca), sem vírus próximos, o SFX elemental não toca; com vírus no alcance, o som é discreto (aproximadamente um a cada 0,9 s).
- [ ] Em ondas avançadas, muitos impactos, mortes e coleta de XP não abafam música, cliques, aviso de evolução ou dano recebido.
- [ ] Médico pode caminhar e disparar; espelhamento horizontal correto; retorna a Walk/Idle.
- [ ] Projéteis causam dano aos vírus; impacto e morte têm sons; o inimigo anima antes de sumir.
- [ ] Matar vírus acrescenta **100 pontos por eliminação**, além de **1 ponto por segundo** vivo.
- [ ] Verificar pelo menos uma receita de 2 e outra de 3 elementos (cor e comportamento).
- [ ] Confirmar que melhorias **gerais** de dano e velocidade de ataque afetam vacina e magia. Melhorias **elementais** (ex.: número de projéteis da magia) não modificam a vacina.
- [ ] Conferir melhorias de atributos e magia e que a partida não possui condição de vitória.
- [ ] Sobreviver a múltiplas ondas: spawn mais frequente, vírus com mais vida, pontuação acumulada.

## 5. Derrota e reinício

- [ ] Perder toda a vida: ouvir dano/derrota, ver seis quadros de Death.
- [ ] Após a animação, aparece GameOver com pontos, tempo, eliminações e onda corretos.
- [ ] Escutar música de derrota (sem música da arena tocando junto).
- [ ] **REINICIAR**: clique, nova cena do jogo com onda 1, tempo 00:00, XP e score zerados.
- [ ] Morrer novamente e clicar **MENU PRINCIPAL**: volta ao menu com sua música.
- [ ] Conferir Console sem `NullReferenceException`, erros de áudio ou de sistema elemental.

## 6. Build e stress

- [ ] Criar build standalone para Windows e abrir o `.exe` fora do Unity.
- [ ] Repetir menu → partida → derrota → reinício; testar música e cliques no executável.
- [ ] Em 1366×768 e 1920×1080, conferir botões e HUD legíveis, sem cortes.
- [ ] Jogar por ao menos 10 minutos e até uma onda com muitos inimigos: sem travar.
- [ ] Ao terminar, registrar problemas e evidências (prints, Console, cena e passos de reprodução).

## Se o projeto apresentar erro

Copie a **primeira mensagem de erro** da Console e o stack trace, junto com a cena
em que ocorreu; erros seguintes podem ser só consequências do primeiro.
Não apresente como finalizado antes de resolver e repetir o teste.


## Teste da nova arte do médico (referência integrada)

1. Abra `Assets/Scenes/SampleScene.unity` sem Play e selecione `Player`:
   o `SpriteRenderer` já deve mostrar o médico de cabelo branco estilizado,
   não o placeholder geométrico antigo.
2. Rode a cena desde `MainMenu`; sem encostar nas teclas, veja a animação
   de respiração (`Idle`, 4 frames).
3. Ande nas quatro direções: `Walk` deve mostrar passos sem mudança perceptível
   de tamanho/centro; o personagem deve espelhar horizontalmente quando necessário.
4. Sem poderes elementais, deixe o `VaccineSpell` disparar: o médico deve
   exibir 8 quadros de `Attack`, a seringa apontando para o lado correto.
5. Escolha o primeiro elemento: a vacina segue atacando silenciosamente;
   a animação e SFX são solicitados pelo elemental sob o limite de frequência
   já implementado. Confira caminhada durante intervalos de ataque.
6. Deixe a vida do personagem chegar a zero: a animação `Death` deve mostrar
   os seis quadros, incluindo dissolução, antes de `GameOver`.
7. Reinicie do `GameOver`, confirme que o médico reaparece com arte correta.
8. Observe a Console para erros de `Resources.LoadAll`, `Missing Sprite`,
   importação de textura ou scripts. Se o personagem parecer grande/pequeno,
   ajuste em conjunto os PPU de TODOS os 24 frames, não o zoom da câmera.

As validações 1–8 **não foram executadas neste ambiente** (sem Unity Editor).


## Testes da nova família de vírus

1. Abrir `Assets/Prefabs/Virus.prefab` e confirmar o novo sprite preto; em Play, a onda 1 só deve mostrar vírus comuns.
2. Na **onda 2**, verificar corredores pequenos com velocidade maior que a do vírus comum.
3. Na **onda 4**, verificar tanques maiores e mais lentos; a vacina e magias devem causar dano normalmente a ambos.
4. Na **onda 5**, verificar **um Colosso Blindado** com armadura pesada e ataque com aviso vermelho; o pulso só deve ferir o médico se ele estiver ao alcance no momento do disparo.
5. Na **onda 10**, verificar **um Núcleo Mutante** com espinhos curtos, núcleo violeta e ataque em círculo. O boss deve se desintegrar na morte e conceder XP.
6. O mini-boss reaparece em 15, 25 etc., e o boss em 20, 30 etc.; os encontros não encerram o jogo.
7. Verificar que os chefes **não surgem repetidos a cada morte** dentro da mesma onda, pois `WaveChanged` também é emitido em eliminações.
8. Verificar colisões, dano por contato, área de ataque, animação de morte e desaparecimento do objeto. Revisar Console e build Windows.
9. Conferir tamanho visual e equilíbrio na câmera: Runner < Virus comum < Tank < Colosso < Núcleo Mutante.
10. Testar derrotas/reinícios e a política de áudio anterior (sem som por projétil de vírus).

**Importante:** estes testes precisam ser executados em Play Mode no Unity. A análise estática do ZIP não os substitui.
