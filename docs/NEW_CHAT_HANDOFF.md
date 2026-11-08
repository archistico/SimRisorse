# Handoff unico — SimRisorse RC105 (hotfix test Zecca, 08/11/2026)

## A che punto siamo

- **Ultima release completamente confermata:** RC95, **1245 passati**. RC96–RC105 rimangono candidate; RC105 prevede **1360 test**. Primo tentativo RC105: 2 errori compilazione (`CS0119`, `xUnit2013`), corretti. Secondo tentativo Windows (08/11): **compilazione OK, 1359/1360** (Core 248, Application 602/603, Infrastructure 262, Desktop 247); F1 fallito a causa di `Rc98AtomicMintCompletionTests.RemovedPrerequisitePreventsCompletionWithoutRefundingOrCrediting`.
- **Secondo hotfix nel presente ZIP:** il test RC98 rimuoveva la Fonderia d'oro ma lasciava invariata la capacità nella contabilità escrow. La fixture ora ricalcola `Funding.Capacity` sulla città modificata, richiede che il checkpoint con sola città modificata venga respinto, verifica `MissingPrerequisite` e che i materiali pagati non vengano accreditati o rimborsati. **Nessuna modifica al codice applicativo e nessun test/F1 eseguito dopo il secondo hotfix.**
- RC105 introduce `E4bV15UnifiedSavePrototype`: da un composito sperimentale **registrato e autorevole** genera un nuovo SQLite con **16 tabelle cittadine native più `unified_campaign` e `unified_receipt`**, senza BLOB di un database annidato. Supporta preventivi e materiali pendenti, verifica le ricevute, legge con staging privato. **Il file è NON autorevole e NON giocabile; nessuna scrittura o promozione.**
- [ADR 0116](adr/0116-prototipo-sqlite-generation7-citta-e-cantieri.md) · [piano RC105](validation/RC105_UNIFIED_SAVE_PROTOTYPE.md) · [stato](CURRENT_STATE.md).

## Regole da non perdere

- GUI v14 canonica / v15 pixel art con filtri settoriali, tooltip Risorse, icone tecnologie e dettagli edificio; edifici bloccati visibili e consultabili.
- Nuove partite: Municipio con 120 Legno lavorato, 80 Pietra, 30 Cibo, **100 Carbone e 2 Matite**; nessun refill ai caricamenti. Ricerca e Depositi alimentari superiore come già implementato.
- `MintTen` Zecca 10 lavoratori per modulo solo opt-in; curva economica A, pagamenti progressivi e schema generation 7 **solo laboratorio**. Salvataggio giocabile rimane generation 6.
- Consegne ZIP complete, **unico `validate.cmd`**, README essenziale con screenshot reali, `.local-cache` esclusa e nessun `MANIFEST_*_SHA256.txt`. Documenti operativi unici.
- Punteggio e classifica locale **dopo** gameplay/bilanciamento; classifica online via API **alla fine**.

## Cosa ci aspetta

1. Rieseguire ` .\validate.cmd ` su Windows con il **secondo hotfix RC105** (1360/1360 test e gate F1); non confondere il precedente risultato 1359/1360 con la convalida del nuovo ZIP.
2. RC106: aggiungere transazioni e autorità sul nuovo schema, protezione da due sessioni, recovery e migrazione sicura; nessun nuovo prezzo o GUI prima della validazione.
3. Progressione naturale curva A; poi stabilizzazione, punteggio e classifiche secondo [roadmap unica](ROADMAP.md).
