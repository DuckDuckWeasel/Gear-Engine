# AuraGear Boosts Neighbors And Core Speed

## Objetivo do Teste
Verificar se os nós tipo suporte (`AuraGearNode`) varrem a malha em busca de vizinhos válidos e implementam modificadores contínuos sobre eles com sucesso, sem corrompê-los com hard-locks.

## Contexto (Arrange)
- O tabuleiro 1D instanciável é alimentado com um Core (motor puro no `0,0`) e um AuraGear colocado do lado `(0,1)`.
- O Aura ConfigData é inflado com propriedades multiplicadoras: Exemplo `2.0x Boost Speed`, irradiando para `Vector2Int.down` (direto no seu vizinho CoreGear).

## Ação Executada (Act)
- O `GridManager.Tick()` é processado contínuas vezes simulando um ciclo em loop fechado do jogo.  
*(O GridManager em design é obrigado a invocar e recalcular qualquer modificador Global e Local ANTES de girar as engrenagens fisicamente)*.

## Validação (Assert)
1.  **Verificação Prévia:** Verifica que antes da aplicação da Aura, o Local Speed do motor é exatamente `1.0x`.
2.  **Verificação de Escala Acelerada:** Ao processar a injeção da Aura através do tempo (dT), o validador confere se a `CoreGearNode` absorveu passivamente a velocidade para `2.0x` como modificador local.
3.  **Prova Matemática (Velocidade Relativa):** Ao forçar um `Update(1f segundo completado)` da lógica, avalia-se que a distância rotacional girada foi EXATAMENTE o dobro num cálculo limpo, comprovando a existência fluída de buffs/multiplicadores empilháveis pela plataforma.
