# ADR 0106 — RC95: contenitore SQLite transazionale della campagna shadow e del finanziamento

**Stato:** sperimentale / da validare su Windows · 07/11/2026. Non autorizza l'attivazione della curva A nel gioco.

## Problema

RC94 tiene insieme lo stato completo della campagna v15 e il pagamento progressivo in memoria. RC93 persiste un escrow **separato**, privo dello stato cittadino; copiare i due file indipendentemente comporterebbe possibili perdite, doppi addebiti e desincronizzazione dopo un crash.

## Decisione

Introdurre `E4bV15FundingCompositeSqliteStore` come **formato sperimentale autonomo** (application_id `0x53465235`, user_version `1`), non sovrapposto al file della partita normale (generation 6) né al sandbox RC93.

Un solo record SQLite contiene la revisione, un'immagine **nativa e validata** di salvataggio v15 generation 6 e un checkpoint escrow Curve A; una tabella di ricevute contiene chiavi di richiesta univoche e payload canonici. `Apply` aggiorna immagine, escrow, revisione e ricevuta **nella stessa transazione**. Dopo il riavvio, la stessa chiave/payload produce `Replayed`; una chiave già utilizzata con payload differente produce `KeyConflict`, e una revisione obsoleta produce `StaleRevision`. Un comando rifiutato non registra ricevute.

L'immagine nativa viene costruita/validata mediante `V15SqliteFoodSaveStore` in un file temporaneo e conservata come BLOB nel contenitore. Il ripristino verifica l'intero checkpoint nativo e riconcilia scorte, capienze e contatore Zecca con l'escrow. Non sono consentite consegne ipotetiche, completamenti nel sandbox, né modifiche alle regole di produzione/GUI.

## Limiti deliberati

- Il contenitore RC95 **non è un salvataggio giocabile**; non viene aperto da `run-game.cmd` e non rimpiazza il salvataggio dell'utente. La campagna normale rimane in generation 6. Nessuna migrazione automatica.
- È ancora supportato un solo cantiere Zecca pendente; solo `BeginMint`, `Deposit` e `Cancel`. **Nessun edificio è confermato** mediante il solo saldo interamente versato.
- Il clock naturale non viene ancora eseguito all'interno della transazione RC95: durante questo laboratorio la campagna shadow è immobile tra un pagamento e l'altro. La successiva milestone deve conciliare produzioni/consumi reali e incremento della capacità con il deposito, senza attribuire al clock risorse inventate.
- Prima dell'uso nel gameplay servono commit atomico cantiere → edificio senza seconda spesa, blocco dei normali writer inconsapevoli del cantiere, gestione delle condizioni di sblocco al completamento, crash recovery e test end-to-end su Windows.

## Test e criteri di uscita

12 nuovi casi Infrastructure: apertura/reload, pagamenti riapribili, replay, collisione delle chiavi, revisione obsoleta, operazioni senza ricevuta, annullamento, rifiuto di consegne/completamenti, rifiuto database v15/RC93, checkpoint alterati e doppia creazione. **1245 test cumulativi attesi** (1233 confermati nella RC94.2). Il green richiede `validate-f1.cmd` con test .NET 10 e gate grafico invariato.
