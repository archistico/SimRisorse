# Piano di validazione — RC105 (candidata)

**Baseline completamente confermata:** RC95 con 1245 test superati. **RC105: 1360 cumulativi attesi**, inclusi 15 nuovi test Infrastructure `Rc105UnifiedSavePrototypeTests`. RC96–RC105 non ancora convalidate. Primo gate 08/11: `CS0119` in `Rc105UnifiedSavePrototypeTests.cs:73` e `xUnit2013` in `Rc99NativeSaveBridgeTests.cs:161` corretti. Secondo gate 08/11: **compilazione riuscita, 1359/1360** (Core 248/248, Application 602/603, Infrastructure 262/262, Desktop 247/247), ma fallimento di `Rc98AtomicMintCompletionTests.RemovedPrerequisitePreventsCompletionWithoutRefundingOrCrediting` perché la fixture azzerava la Fonderia d'oro senza ricalcolare la capienza nell'escrow. Secondo hotfix: correzione del solo test, incluse verifica del rifiuto della capienza stantia e conservazione del finanziamento. **Build/test/F1 non ancora rieseguiti dopo il secondo hotfix.**

Usare **solo** `./validate.cmd` da PowerShell nella radice Windows del progetto (`.\validate.cmd`). Il gate deve completare compilation, 1360 test/0 falliti, test di rendering, smoke e audit. Conservare l'ultima evidenza in `.local-cache/m4/`; non includerla nello ZIP.

## Casi RC105

- Origine autorevole con tabelle native generation 6, identità, versione 7 e modalità sempre inattiva.
- Round-trip di preventivo non pagato, rata parziale e ricevute contigue; Zecca conclusa con saldo e contatore invariati.
- Rifiuto di compositi non registrati, backup inattivi, registro mancante, destinazioni esistenti/uguali/non creabili.
- Rifiuto di ricevute mancanti, JSON finanziario non coerente, tentativi di impostare autorità attiva e uso con il loader generation 6.
- Protezione dei byte dei database originali e rimozione degli staging dopo una conversione valida o fallita.

## Controlli manuali al prossimo gate

Verificare sul Desktop Windows che `validate.cmd` sia l'unico comando, che gli screenshot attesi siano generati, che la pulizia della cache non rimuova l'ultima prova fallita e che i salvataggi personali generation 6 non siano modificati. **Non aprire il prototipo RC105 come partita.**
