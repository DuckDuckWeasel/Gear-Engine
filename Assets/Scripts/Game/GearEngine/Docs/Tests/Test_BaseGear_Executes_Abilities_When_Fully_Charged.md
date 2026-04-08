# BaseGear Executes Abilities When Fully Charged

## Objetivo do Teste
Garantir o fluxo natural de vida da Peça Passiva Padrão (`BaseGearNode`): ser preenchida de *"carga elétrica mecânica"* por intermédio de estímulos externos e explodir/propagar Suas Habilidades quando sobrecarregada (atingir a Maximum Charge).

## Contexto (Arrange)
- A Engrenagem Padrão é spawnada com uma capacidade `MaxCharge = 100f` via Data configuration.
- Ela é injetada virtualmente com o Mock (Fictício) de Ability - e isolada do Motor para observação em câmara limpa do Framework.

## Ação Executada (Act)
- Vários pulsos massivos de evento `DirectionalTriggerEvent` são impulsionados pelo Barramento pra simular que ela está sendo golpeada por peças vizinhas (`+50f` de carga gerada).

## Validação (Assert)
1.  **Sobrevivência:** Com apenas um pulso `(50/100)`, O BaseGearNode tem a obrigação de acumular o valor exato no estado (`CurrentCharge = 50`), **MAS NÃO PODE EXECULTAR AS HABILIDADES**.
2.  **Execução em Massa (Burnout):** No segundo pulso a carga extrapola/bate o limite tático (`100/100`). O assert exige que as Habilidades do SO encorporado sejam totalmente engatilhadas e executadas exatamente uma única vez, liberando sua carga para repousar de volta ao seu estado mínimo `(Charge = 0)` limpo logo em seguida.
