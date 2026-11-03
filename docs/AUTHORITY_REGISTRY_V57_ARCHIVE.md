# SimRisorse — registro dell'autorità documentale

**Aggiornato il 28 ottobre 2026; checkpoint v57 documentale.** Serve a risolvere contraddizioni apparenti tra documenti di periodi diversi senza perdere la tracciabilità storica.

## Ordine di precedenza

1. **Codice e test presenti nel pacchetto v57**, per stabilire ciò che è realmente implementato (in particolare `CivilGrowthScenario.cs`, modelli Core e test).
   Per la nomenclatura effettiva di risorse/strutture usare [CATALOG_CURRENT.md](CATALOG_CURRENT.md) e verificarne la corrispondenza con il catalogo nel codice.
2. **Decisioni esplicite dell'utente** riportate in [CURRENT_STATE.md](CURRENT_STATE.md) e nelle schede di scelta approvate. Il codice non trasforma una proposta in una decisione.
3. **[CURRENT_STATE.md](CURRENT_STATE.md)** e **[NEW_CHAT_HANDOFF.md](NEW_CHAT_HANDOFF.md)**, che descrivono l'ultima baseline e i gate pendenti; [HANDOFF.md](HANDOFF.md) è un riassunto operativo.
4. **[V55_STATIC_AUDIT.md](validation/V55_STATIC_AUDIT.md)** per l'esito statico; per il gate runtime prevale un **nuovo log `validate.cmd` realmente eseguito** dall'utente sulla versione identificata.
5. **[ROADMAP.md](ROADMAP.md)** *riepilogo attivo iniziale* e [M5_4_DECISION_GATE_V55.md](design/M5_4_DECISION_GATE_V55.md) per piano e proposte. Il dossier M5.4 **non è approvazione**.
6. **CHANGELOG, ADR, GAME_DESIGN, SAVE_FORMAT, rapporti v54 e precedenti, altri paragrafi storici di ROADMAP/HANDOFF/README** per origine e motivazioni. Le frasi «stato corrente» di quelle versioni hanno valore **solo nel loro checkpoint**.

## Decisioni vincolanti

| Ambito | Decisione acquisita | Cosa NON dedurre |
| --- | --- | --- |
| Campagna | **22 edifici-obiettivo**; Castello ultimo; una conquista per sbloccarlo | Municipio conteggiato tra i 22, oppure Cantiere/Castello già codificati |
| Municipio | Unico, fisso, iniziale; +1.000 posti per **ciascuna** risorsa; nessuna dotazione aggiuntiva in migrazione | Municipio ricostruibile, assegnazione di addetti o stock duplicati |
| Stoccaggio | Magazzino +500 a tutte, produttori +50 ai propri output, case +10/+25 a Cibo/Pane, Caserma +20 Spade; inventario condiviso | Depositi privati o regole Pane separate |
| Economia | Contratti A Ferro/Oro/Forgia/Zecca già integrati, contesa Carbone definita | Monetizzazione generale di tutte le costruzioni |
| Militare | Contratto Caserma A e regole M-01…M-04 approvati; reclutamento, coda e congedo implementati | Missioni o perdite di soldati già abilitate |
| Salvataggio | S1, migrazioni progressive con backup, schema SQLite v2, factory storiche congelate | Aggiornamento silenzioso di vecchi checkpoint o produzione offline |
| Interfaccia | 8-bit monospace, liste, icone coerenti, descrizioni di blocco, selezione ocra | Mappa tattica o posizionamento obbligatorio |
| Perimetro | Carne fuori prima campagna; demolizione rinviata | Rimborso demolizione del 25% approvato |

## Contratti sospesi

- **N-01…N-07 M5.4:** [proposta v55](design/M5_4_DECISION_GATE_V55.md). Tutti gli importi, posti nave, identità e modello di acquisto sono **proposte**, non nuovi requisiti autorizzati.
- **Missioni/conquista/Castello:** decisione sul ruolo della conquista acquisita, ma parametri di navigazione, isole, durata, forze, ricompense e implementazione della vittoria aperti.
- **Bilanciamento/classifica:** tempo-obiettivo 8/32+ ore non ancora dimostrato per la campagna completa.

## Regole per ogni incremento

1. Mantenere il **Core indipendente dalla UI**; niente formule economiche duplicate in Avalonia.
2. Nuove regole economiche o schema di snapshot: identificare la versione e aggiungere conversione, benchmark, test unitari e integrazione. Nuovi read model/diagnosi senza modifica alle regole non richiedono un nuovo ruleset.
3. Preservare save personali e fixture storiche. Test/benchmark solo su percorsi privati; nessun aggiornamento in-place di benchmark congelati.
4. Riportare ogni volta **versione, stato reale dei test, modifiche, roadmap e punti aperti**. Usare «proposto», «implementato ma non validato», «verificato» con precisione.
5. Se il nuovo utente riferisce un gate positivo, annotare **quale pacchetto** ha testato e se il log completo è disponibile; solo allora aggiornare il checkpoint.
