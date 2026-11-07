# ADR 0103 — RC93: persistenza SQLite atomica e idempotenza del sandbox di finanziamento

**Stato:** accettata **per esperimento isolato**, non per il gameplay. **Data:** 07/11/2026.

## Contesto

RC92 dimostra versamenti progressivi in memoria; la perdita del processo eliminava il deposito. Per un eventuale regime giocabile occorrono checkpoint completi, ripresa, prevenzione di doppi addebiti e controllo di scritture concorrenti. I prezzi Curve A non sono ancora stati approvati.

## Decisione

1. Aggiungere `E4bFundingCheckpoint` con versione, scorte iniziali/correnti, capacità, consegne **ipotetiche**, costi completati, contatore Zecche, tier, preventivo congelato e materiali vincolati. `Restore` ricalcola il preventivo A e rifiuta quote alterate, depositi eccedenti, capienze o bilanci incoerenti; non concede nulla alla campagna.
2. Aggiungere `E4bCurveAFundingSqliteStore` in `Infrastructure/Experimental`, **in un file SQLite indipendente dai salvataggi v15** (application_id distinto, `user_version=1` di questo *database sperimentale*, non modifica dello schema v15 generation 6).
3. Ogni comando accettato salva stato integrale e ricevuta dell'ID richiesta **nella stessa transazione**. Ogni richiesta ha ID stabile, payload canonico e versione attesa; il replay di un ID già applicato e identico ritorna `Replayed` senza addebitare nuovamente; stesso ID ma payload diverso ritorna `KeyConflict`. Uno stato cambiato dopo la lettura dà `StaleRevision`. Un comando respinto non crea ricevute.
4. Annullamento: rimborso completo **solo se la capienza lo consente**; in caso contrario, nessun cambiamento e deposito ancora recuperabile. Completamento: solo dopo pagamento completo, solo nel sandbox.
5. L'inventario di prova è alimentato da `DeliverHypothetical`: questa non è una produzione reale, non è un acquisto giocabile e non misura tempi naturali. Il ledger non si collega al clock, ai salvataggi, ai costi, ai pulsanti o ai personaggi.
6. Non è un protocollo contro manomissioni intenzionali del file sul disco né una soluzione distribuita multi-processo; la transazione SQLite, il controllo versione e le invarianti proteggono il flusso applicativo e i riavvii. La promozione in-game richiederà una progettazione ulteriore del salvataggio unico della campagna.

## Test necessari

Snapshot vuoto/parziale, quote immutabili, formati malformati, saldo non riconciliato, riavvio dopo pagamento, completamento singolo, annullamento senza capienza, replay dopo altre operazioni, chiave duplicata con payload diverso, revisione obsoleta, operazioni respinte senza ricevuta, database estraneo e sessioni duplicate. Piano: [RC93](../validation/RC93_FUNDING_PERSISTENCE.md).

**Validazione:** RC90.3 1167 test confermati dall'utente; RC91.3 1188 e RC92 1203 non ancora confermate; RC93 aggiunge 17 casi, **1220 attesi**, compilazione e test Windows da eseguire.
