# GridManager Stop Triggers WindDown

## Objetivo do Teste
Convalidar o comportamento físico de desligamento brando (Máquina de Estados de Energia / Run State) do próprio cérebro da malha. O objetivo é evitar comportamentos não agradáveis, congelamentos sujos no Update ou loops corrompidos de *Physics*.

## Contexto (Arrange)
- Um Motor vivo roda constantemente no centro da grade provendo energia infinita de giros pelo Grid.

## Ação Executada (Act)
- É forçada uma aberração visual pela cena injetando a mecânica em ângulos tortos de rotação (ex `33°` de inclinação num snap de engrenagem).
- Pede-se que o Sistema "Desligue as chaves" do jogo operando `GridManager.Stop()`.
- Roda-se o `GridManager.Tick()` (que agora está bypassando para as interações isoladas do modo `WindDownUpdate`).

## Validação (Assert)
1.  **State Flag:** Constata se o `IsRunning` foi alterado apropriadamente para impedir cálculos falsos e o consumo de novas baterias (Triggers desligados na raiz).
2.  **Soft-Lerp Interactor:** Assegura que o pulso da engrenagem, apesar do desligamento, continua descendo maciamente da rotação torta em que se achava de volta para as origens dos eixos sem perder consistência temporal (`LerpAngle` agindo com smooth factor pra posição neutra rest position).
