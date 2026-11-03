# G1-D11 — E3b/3a-b, prima sessione `civil-growth-14` isolata

**Stato:** IMPLEMENTATA COME PROTOTIPO, VALIDAZIONE .NET PENDENTE. **Base Windows confermata:** `v60-E3b2-RC3`, `validate.cmd` interamente superato secondo l'utente. Data: 3 novembre 2026.

## Contenuto reale di questo incremento

Nuovo `src/SimRisorse.Core/Sessions/V14CampaignSession.cs`:

- `FromV13(...)` crea **in memoria** una sessione `civil-growth-14` a partire da `GameSnapshot` v13, passando dal validatore/migratore esplicito esistente. Non esegue scritture sul file originale.
- `Restore(...)` accetta solo un `V14CampaignSnapshot` valido rispetto a regole Scuola e catalogo tecnologie iniettati; `Snapshot()` restituisce lo stato immutabile, senza formati di salvataggio duplicati.
- `BuildSchool(quantity)` richiede edificio civile/alimentare prerequisito e materiali; applica costo progressivo per la famiglia `school` e aggiorna **insieme** saldo, numero Scuole e contatore storico. Niente modifiche su fallimento.
- `EnrollStudents`, `CancelStudents`, `AssignResearchers`, `UnassignResearchers`, `ReturnResearchersToCivilian` riutilizzano `SchoolResearch` e i controlli di popolazione; rispettano la capacità condivisa per il rimborso.
- `BuyTechnology(id)` usa il `ResearchLedger` persistito nello snapshot, non uno stato parallelo; tecnologia acquistata solo su comando esplicito, verifica prerequisiti e saldo.
- Ogni operazione riuscita passa da `V14CampaignSnapshots.Validate(...)` prima del commit dello stato, che non è reso visibile in caso di eccezione.

**Regole monetarie solo candidate:** `V14SchoolConstructionRules.Candidate` = 120 Legno lavorato + 80 Pietra, crescita 1,15 per acquisto, prerequisiti Casa piccola e Deposito alimentare. Sono parametri espliciti di un prototipo isolato, **non una decisione di bilanciamento ratificata**.

## Limiti espliciti

Non sono ancora implementati in `V14CampaignSession`:

- avanzamento del tempo unico (produzione materie, razioni, crescita demografica, formazione e produzione ricerca insieme);
- costruzione e miglioramento dei produttori Base/Grande/Avanzato, assegnazione dei lavoratori tier;
- collegamento alla GUI Avalonia e possibilità di avviare una campagna v14 dalla schermata;
- attivazione dei prezzi progressivi e del nuovo catalogo nella partita normale.

Il nuovo `V14SqliteSaveStore` già isolato è stato esercitato dal test aggiunto di round-trip della sessione. **Nessun adattamento dello store v2 né nuovo database utente viene eseguito automaticamente**.

## Test aggiunti

- `V14CampaignSessionTests.cs`: sei test Application per bootstrap/ripristino, prerequisiti, prezzo successivo, pagamento atomico, popolazione studenti/ricercatori, acquisti tecnologia e saldo libero.
- `V14CampaignSessionPersistenceTests.cs`: un test Infrastructure crea uno SQLite v3 temporaneo, salva i risultati di comandi della sessione, ricarica e ricrea la sessione; confronta saldi, formazione, sblocchi e contatori.

**Incremento: +7 test dichiarativi**; baseline RC3: 380 test Windows passati secondo l'utente. **Obiettivo RC1: 387 test**, salvo variazioni del runner. Compilazione e test di questo incremento **non eseguiti qui**, quindi non dichiarare il gate come superato.

## Gate successivi e rischio

1. Test RC1 su Windows con `validate.cmd`, riparare eventuali regressioni senza disattivare test.
2. E3b/3c: tempo unico e replay deterministico, con ricerca, consumo Cibo, crescita, reclutamento e produzioni; pause e salvataggi intermedi.
3. E3b/3d: tier building commands con acquisti/upgrade, controlli tecnologia e anti-softlock; test di stoccaggio.
4. E3b/3e: ciclo completo save/restore v3 e migrazioni su copie, poi E3b/4 UI ed E4 bilanciamento.

**Decisioni approvate:** accumulo libero di ricerca, spesa volontaria, preferenza per costi crescenti per famiglia/upgrade/Scuola che forma abitanti. **Non approvati definitivamente:** prezzi 15%, costo Scuola, timer 120 s, moltiplicatori tier, requisiti tecnologici, condizioni di vittoria.
