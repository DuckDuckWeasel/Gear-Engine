# RuntimeAbility Deactivates Node Temporarily

## Objetivo do Teste
Estabelecer que "Abilities Modificadoras Baseadas No Tempo" duráveis (Status Effects como *Stun/Congelamento* num node) penetram à arquitetura mecânica da engrenagem e controlam a vitalidade da peça perfeitamente.

## Contexto (Arrange)
- Um BaseGearNode operante está presente com sua Flag interna `IsActive = true`.
- É instanciado em modo Runtime pela arquitetura o Mock ativo do `InactiveAbility` - scriptableObject provisório treinado exclusivamente para trancar o status `IsActive == false` do Cérebro.
- O Sistema força essa injeção com durabilidade/Tempo de Vida curto = `2 Segundos`.

## Ação Executada (Act)
- O tempo global progride pela métrica de 1 Segundo Completo em pulso falso no loop. 

## Validação (Assert)
1.  **Interceptador Cego (Lock):** Como apenas 1 segundo se passou (defeitos ainda existem no relógio por não gastarem os 2 inteiros), afere-se se a peça passiva continua inteiramente "CONGELADA" (não consome e não gira nenhum grau) porque se recusa a responder via Early-Returns de performance se inativa.
2.  **O Tempo escorrega:** Garante que embora a peca mecânica não mexe, os componentes virtuais anexados nela em segredo (`TickAbilities`) continuam drenando logicamente o sangue do tempo que falta pela Wrapper Class em Runtime (`RuntimeAbility`).
