# ADR 0108 — RC97: clock reale e deposito finanziamento nello stesso checkpoint sperimentale

**Stato:** adottata soltanto per il laboratorio isolato RC97 (07/11/2026). **Non** è un'autorizzazione ad attivare nuovi prezzi o cantieri nel gioco.

## Contesto

In RC95 città e deposito Curve A venivano salvati insieme, ma non esisteva un comando per far avanzare il clock durante un cantiere. Il registro originario del sandbox imponeva `stock iniziale = stock presente + escrow`, incompatibile con risorse realmente prodotte/consumate dalla città.

## Decisione

1. `E4bV15FundingShadow.TryAdvanceClock` richiama **il medesimo** `V15UnifiedCampaignClock.Plan` del gioco per **un secondo esatto**, passando politiche produttive esplicite e il checkpoint completo. Prima del commit applica `V15StockAuditor.ReconcileSecond` su tutte le scorte fisiche (incluse le filiere alimentari).
2. Il deposito rimane intoccabile dal clock. Il suo registro conserva **crediti e debiti osservati nell'inventario cittadino**, separati dalle risorse in stalle, depositi alimentari e transito. L'invariante diventa: `stock iniziale + consegne ipotetiche + crediti clock = stock città + escrow + materiali impegnati + debiti clock`. Il composito vieta consegne ipotetiche e acquisti artificialmente completati.
3. `E4bV15FundingCompositeSqliteStore.AdvanceClock` salva immagine SQLite v15, deposito, revisione e ricevuta idempotente **in una sola transazione**; la chiave di richiesta è vincolata anche al digest delle politiche del tick. Replay non ripete il secondo; revisioni obsolete vengono rifiutate.
4. Il checkpoint dei finanziamenti aggiunge i due registri in modo opzionale: database sperimentali precedenti senza quei campi sono ancora leggibili con registri vuoti. Non è modificato il formato del **salvataggio giocabile** SQLite generation 6.
5. Continuano a esistere **solo** `validate.cmd` come comando pubblico e i quattro documenti operativi unici. Niente `validate-f1.cmd`, niente manifest `MANIFEST_XXX_SHA256.txt`. Il gate esige anche il diagnostico RC97.

## Limitazioni esplicite

- Un tick RC97 avanza un solo secondo. Il laboratorio è separato dai salvataggi giocabili, dalla UI e dai pulsanti «Costruisci».
- Il ledger registra la variazione **netta della scorta cittadina**, non una falsa produzione lorda: la riconciliazione fisica del clock resta un controllo distinto.
- Una Zecca completamente finanziata **non viene ancora costruita**. Mancano la conclusione atomica, la prova naturale di progressione con Curve A, la scelta dei coefficienti e l'integrazione in gioco.
- La compilazione .NET 10 e il gate completo RC97 dovranno essere confermati su Windows; le analisi statiche non li sostituiscono.

## Verifiche richieste

12 nuovi test xUnit (6 Application, 6 Infrastructure), replay dopo riapertura, revisione obsoleta, conflitto di chiave pagamento/tick, rimborso dopo clock, ripristino legacy, ledger contraffatto. Gate unico `./validate.cmd` su Windows, obiettivo **1257 test**.
