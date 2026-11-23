# COVID SURVIVOR — Checklist do professor

**Enunciado recebido:** submeter o ZIP com `ProjectSettings`, `Assets` e `Packages`; somente
uma pessoa por grupo envia; registrar integrantes no comentário; apresentar
antes de 07/12 apenas avisando o professor.

Data da revisão: **23/11/2026**.

| Requisito exigido | Implementação localizada | Verificação atual | Teste necessário no Unity |
|---|---|---|---|
| **Troca de cenas** | `MainMenu` → `SampleScene` → `GameOver`; botões jogar, reiniciar e voltar ao menu | Cenas e Build Settings verificados por código | Executar cada botão e confirmar transição sem erros |
| **Animação** | Médico Idle/Walk/Attack/Death e vírus Walk/Death por `SpriteAnimator` | 36 frames, metadados e chamadas auditados | Observar sprites, movimento, disparo e mortes no Play Mode |
| **Sons: Background, SFX e UI** | 3 loops + 9 SFX originais; `CovidAudioManager` | 12 WAVs no projeto; formato e referências inspecionados estaticamente | Ouvir três músicas, tiros, danos, coleta, evolução, derrota e cliques |
| **Gameplay totalmente funcional** | Movimento, vida, perseguição, spawn, dano, XP, upgrades, ondas, timer, score e derrota | Configuração, scripts e referências verificados estaticamente; **funcionamento não comprovado** | Testar fluxo completo, progressão, receitas elementais e build |
| **UI e HUD** | Vida, XP, onda, tempo, pontos e seleção de quatro melhorias | Componentes e textos encontrados no projeto | Conferir escalas, cliques e valores em tela |
| **Utilização de prefab** | `Virus`, `ElementalProjectile` e 3 prefabs de cristais (mais prefab legado) | Arquivos e referências serializadas encontrados | Instanciar e verificar comportamento no Play Mode |

**Estado honesto:** nenhuma das seis linhas acima recebeu validação funcional completa
em Play Mode neste ambiente. A auditoria estática passou, mas **não equivale a compilar
nem jogar no Unity**.

## Correção do início da partida (23/11/2026)

O `VaccineSpell` foi reativado. A vacina é a arma **inicial e permanente**; quando o primeiro elemento é adquirido, o jogador passa a atacar com as duas armas. Antes de adquirir elementos, apenas a vacina solicita animação de ataque e SFX `shoot`. Após adquirir a primeira magia, a vacina mantém projéteis e dano, porém cede o feedback de ataque à magia elemental, cujo som `element_cast` tem intervalo mínimo de 0,9 s. Animações têm intervalo mínimo de 0,68 s (vacina) ou 1,05 s (elemental); efeitos de impacto, morte e coleta tiveram volumes e cadências reduzidos. Toda a integração precisa de validação no Play Mode.

## Materiais e evidências

- As verificações estáticas da fase anterior não cobrem os novos ajustes da arma básica; executar os testes de `TESTES_NO_UNITY.md`.
- `TESTES_NO_UNITY.md`: roteiro para Editor, Play Mode e build.
- `AUDIO_MANIFEST.json`: informações técnicas dos 12 arquivos de áudio.

## Preparação da entrega escolar

1. Abrir o projeto na versão `6000.5.8f1`, aguardar a importação e verificar a Console (auditor personalizado removido).
2. Executar o roteiro de `TESTES_NO_UNITY.md`; corrigir erros da Console e repetir os testes.
3. Gerar um build PC de teste e rodá-lo sem o Editor.
4. Somente após a aprovação, preparar o ZIP do projeto com as pastas obrigatórias.
5. Uma pessoa do grupo envia e escreve os integrantes no comentário da atividade.
