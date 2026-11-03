# G1-D12 — E3b/3c: tempo unico della sessione v14 (isolato)

**Stato:** implementazione candidata `v60-E3b3-RC2`, compilazione e test .NET ancora da confermare su Windows. 3 novembre 2026.

## Baseline verificata

- L'utente ha confermato il superamento di `validate.cmd` della **v60-E3b3-RC1** (387 test attesi).
- `civil-growth-13` rimane il ruleset giocabile; `GameSession`, store SQLite v2, GUI Avalonia e benchmark storici sono invariati.
- La proposta `civil-growth-14` è un aggregato separato. Coefficienti economici, costi Scuola e tecnologie non sono ancora ratificati.

## Implementazione

`V14CampaignSession` aggiunge `Pause()`, `Resume()`, `IsPaused` e `Advance(TimeSpan)` (stesse regole di durata massima della sessione v13). Il nuovo `V14CampaignClock` unifica per ogni secondo simulato, in quest'ordine:

1. piano consumi della popolazione e produzione delle 16 famiglie produttive tramite `ProductionPlanner` già esistente;
2. progressione abitativa, inclusi intervallo, accelerazione delle case e razioni Pane per abitazioni avanzate;
3. addestramento militare individuale tramite il motore esistente;
4. avanzamento degli studenti, passaggio da attesa a formazione e accumulo dei punti ricerca **solo** per ricercatori attivi già assegnati.

Ogni famiglia produttiva viene presentata al pianificatore storico con il **numero effettivo dei lavoratori** in ciascuno dei tre livelli, non con il numero degli edifici. La capacità produttiva aggregata è quella del nuovo portfolio; la capacità dello stoccaggio usa già la relazione 1/3/10.

La Scuola non addebita una seconda volta il Cibo di formazione (pagato all'iscrizione). I diplomati restano inattivi fino all'assegnazione volontaria; i punti ricerca non avviano ricerche in automatico.

`ElapsedTicks` conserva anche frazioni di secondo. Le operazioni per-secondo sono calcolate in base ai confini attraversati, per cui 500ms+500ms corrispondono a 1s. A sessione in pausa il tempo non avanza. Un ripristino v14 parte in pausa.

**Atomicità:** il calcolo dell'intero intervallo usa variabili locali e produce un unico snapshot finale. Solo la validazione finale consente il commit. Un overflow aritmetico non lascia stati intermedi; l'errore è `GameError.NumericLimit`.

## Suite nuova (9 casi)

- Test Application (8): pausa/limiti; 60×1s = 60s; 120×1s = 120s attraversando il completamento della Scuola; frazioni; pausa/ripresa; parity v13/v14 senza nuovi edifici; overflow senza commit; produzione di un impianto avanzato con 10 lavoratori.
- Test Infrastructure (1): salva una sessione durante l'addestramento degli studenti a 37s, ricarica SQLite v3 e verifica lo stesso stato a 120s dopo il completamento della formazione.

Target previsto: **396 test** (387 base + 9 nuovi), a condizione che la discovery dei test sia invariata.

## Limiti deliberati

- Non è ancora integrata la costruzione/upgrade dei tier tramite comandi di sessione (E3b/3d), né la UI (E3b/4).
- La v14 non è abilitata come campagna giocabile, non sostituisce il database personale e non cambia i prezzi della v13.
- Non sono state ancora misurate prestazioni del clock su intervalli di ore o gli effetti quantitativi della nuova economia; saranno validati su Windows.
- La conferma del runtime v14 non equivarrà all'approvazione di prezzi, Scuola, albero tecnologico o condizioni di vittoria.

## Passo successivo

1. Eseguire `./validate.cmd` su Windows e correggere eventuali problemi, con priorità al clock e ai test aggiunti.
2. E3b/3d: comandi per costruzione diretta dei tier, upgrade e gestione dei posti di lavoro, prezzo progressivo e prerequisiti tecnologici.
3. E3b/3e: replay di campagne v14 con salvataggi intermedi, controlli di coerenza.
4. E3b/4: GUI Scuola, Ricerca e taglie; E4 bilanciamento e raggiungibilità, con passaggio controllato alla nuova campagna.

Riferimenti: [ADR 0028](../adr/0028-e3b3c-orologio-unico-v14-isolato.md), [checkpoint D12](../history/checkpoints/G1_D12_CHECKPOINT.md), [roadmap](../ROADMAP.md).
