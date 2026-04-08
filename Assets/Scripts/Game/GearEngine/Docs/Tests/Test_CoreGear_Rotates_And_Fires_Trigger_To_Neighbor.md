# CoreGear Rotates And Fires Trigger To Neighbor

## Objetivo do Teste
Verificar se o **CoreGearNode** (a engrenagem central do motor) gira de forma correta ao receber *ticks* da engine e se ele transmite o pulso de energia (`DirectionalTriggerEvent`) perfeitamente para as engrenagens vizinhas ao completar uma fase do giro.

## Contexto (Arrange)
- É definida a topologia espacial: um CoreGear posicionado no centro pos(0,0) com `TriggerPattern.FourWay` (4 saídas ortogonais).
- O `GridManager` é provisionado com este nó.
- Um ouvinte fictício (stub) é atrelado ao `Scaffold EventBus` para capturar os despachos do evento `DirectionalTriggerEvent`.

## Ação Executada (Act)
- O tempo é simulado passando múltiplos "ticks" via `NodeUpdate`.
- A engrenagem acumula giro progressivo de acordo com seu `BaseRotationSpeed` (100f).

## Validação (Assert)
1.  **Isolamento Progressivo:** Garante que a engrenagem **não** dispara nos primeiros milissegundos enquanto ainda não completou os exatos 90 graus.
2.  **Trigger Completo:** Exatamente ao ultrapassar os 90 graus (o snap de `FourWay`), a asserção valida se a engrenagem central submeteu instantaneamente o `DirectionalTriggerEvent` pela EventBus direcionado para a coordenada espacial vizinha predefinida `(1, 0)`.
