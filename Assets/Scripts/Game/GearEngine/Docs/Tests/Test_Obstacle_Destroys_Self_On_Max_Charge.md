# Obstacle Destroys Self On Max Charge

## Objetivo do Teste
Comprovar o poder da arquitetura modular do Grid baseada estritamente em **Composições** não injetando Heranças clássicas como `class ObstacleManager`. Avaliar se uma engrenagem fria se torna um componente 100% obstrutivo (como uma parede ou uma pedra quebrável).

## Contexto (Arrange)
- Uma Peça Passiva Normal é gerada por dados de Data-Structure, sem classes parasitas. Usa-se apenas um limitador `IsInteractable = false` via configuração, tornando o objeto ignorável via Mouse inputs.
- Seu limiar de quebra via Trigger da pancada de engrenagens centrais é medido num pool de Vida de `3.0x Acertos de Carga`.
- A ela, a inteligência `DestroySelfAbility` (Habilidade destrutora final) fica ligada em array.

## Ação Executada (Act)
- Eventos e Ticks contínuos espalham vibrações pelo Barramento rumo a colisão de "Rock". O motor estressado bate exatamente 3 vezes gerando a tensão acumulativa.

## Validação (Assert)
1.  **Dureza Consistente (Durabilidade):** Enquanto o acúmulo das porradas é menor ou estritamente igual à 2, a arquitetura afere que pedra nunca envia requisições e a Flag do Barramento de mortes continua nula (Peça inteira perante 2 acertos).
2.  **Autodestruição Súbita (Break!):** No envio da exata terceira e massiva bala final do impacto a cota de preenchimento (`MaxCharge`) alcança seu pico de vida final. O sistema inspeciona se estourou-se, de dentro dela mesma, o gatilho vital do Dispatch chamando a `GearDestroyedEvent` informando exatamente sua posição espacial que faleceu na colisão, limpando a grade. Garantindo que todo elemento orgânico e não orgânico da rede respondem perfeitamente aos barramentos do Backbone em tempo real.
