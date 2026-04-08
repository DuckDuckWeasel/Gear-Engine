# GearMergeService Merges Identical Gears

## Objetivo do Teste
Validar se o algoritmo do `GearMergeService` (responsável pelo mecado da junção de peças similares por gestos do usuário) obedece as regras estritas de compatibilidade e sobe engrenagens de nível corretamente pelo sistema de `GearConfigData`.

## Contexto (Arrange)
- São criadas as referências de duas instâncias independentes de Nível 1 (`GearConfigData_Level1_A` e `GearConfigData_Level1_B`).
- A Engrenagem de Nível 1 porta em sua hierarquia de arquitetura uma referência lincada (ponteiro) explícita apontando para `Target Level 2`.
- O `GearMergeService` é injetado pronto para atuar.

## Ação Executada (Act)
- O método de junção do serviço é evocado `TryMergeGears(...)`, alimentando as duas instâncias e pedindo que a plataforma decida a validade do merge.

## Validação (Assert)
1.  **Merge Sucesso:** Confirma que o serviço atesta como viavel (retorna TRUE).
2.  **Conversão Realizada:** Assegura que o objeto de saída (`resultConfig`) entregue pelo Merge não é nenhum do tipo Level 1, mas sim instanciou com precisão estrutural do `Level 2`.
3.  **Transferência Posicional/Identidade:** Garante que a classe resultante reteu os identificadores mecânicos da família correta da engrenagem submetida.
