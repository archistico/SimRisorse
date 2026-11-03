# M5.4 — audit tecnico d'integrazione prima del codice (v52)

**Nota successiva v53:** questo audit descrive la baseline storica v52. La versione **`civil-growth-11` è ora dedicata all'unificazione della capienza del Pane**, ed entrambi i controlli militari citati sotto sono stati estesi a v10 e v11. Qualsiasi proposta navale approvata dovrà usare la futura **`civil-growth-12`**, aggiornare la migrazione **v11→v12** e richiedere uno stato navale valido *oltre* a quello militare. Prezzi, capienza e regole delle navi restano **non approvati**.

**28/10/2026 · Solo progettazione, nessun cambiamento alle regole.** Questa scheda è complementare alla [proposta di decisione navale v51](M5_4_DECISION_GATE_V51.md). Le sue ipotesi numeriche **non sono approvate**. Il codice corrente rimane `civil-growth-10` con Caserma e GUI Esercito, **20 edifici / 16 risorse / 276 xUnit attesi**. La v50/v51 non ha ancora un gate Windows confermato.

## Risultato principale dell'audit

L'infrastruttura SQLite è adatta a una nuova tabella navale conservando il **formato di file 2** e la politica S1, ma oggi contiene **due controlli espliciti per `civil-growth-10`**. Non basta aggiungere le navi a `GameSnapshot`: se si costruisse `civil-growth-11` senza adeguare questi controlli, il salvataggio verrebbe rifiutato perché contiene lo stato militare obbligatorio. **È una dipendenza futura, non un errore delle partite v10 attuali.**

## Punti di integrazione verificati nel codice

| Componente effettivo | Stato v10 | Attività necessaria dopo approvazione navale |
| --- | --- | --- |
| `CivilGrowthScenario.CreateCore` | `barracks` e 16 risorse, versione v10 | Aggiungere `shipyard` solo al catalogo v11, come edificio non produttivo se scelto; preservare un factory v10 per prove storiche |
| `CivilGrowthScenario.Changes` e `UpgradeOneStep` | Conversioni `civil-growth-1…-9→-10` | Aggiungere **solo** `-10→-11`, Cantiere a zero e flotta vuota; conservare popolazione, soldati, scorte, tick e frazioni della crescita |
| `GameSession` e `GameSnapshot` | Stato militare obbligatorio v10, snapshot formato 2 | Aggiungere un dominio Flotta distinto dal dominio Esercito; validare identità e invarianti prima del ripristino; pagamento senza mutazioni parziali |
| `SqliteGameSaveStore.SaveCore` | Richiede `Military` esattamente per `civil-growth-10` | Estendere esplicitamente l'obbligo militare a v11 e richiedere lo stato navale **soltanto** per v11; nessuna accettazione indiscriminata di versioni sconosciute |
| `SqliteGameSaveStore.LoadCore` | Anche in lettura lo stato militare è ammesso esattamente in v10 | Adeguare **simmetricamente** le condizioni di lettura e scrittura; righe navali mancanti o invalide devono causare errore, mai generare navi implicitamente |
| `PreserveReplacedRules` | Backup via `VACUUM INTO` prima della scrittura di regole differenti | Verificare che il passaggio v10→v11 crei una copia recuperabile *prima* della mutazione; niente cancellazione di backup o partite personali |
| `GameViewProjector`/`GameView` | Modello di lettura militare, nessun dato navale | Aggiungere un read model della flotta; la GUI non deve ricostruire capacità o costo dal proprio testo |
| `MainWindow`/`MainViewModel` | Navigazione Esercito con visibilità esplicita, fix v50 ancora da verificare | Esporre Flotta solo nello scenario supportato; riusare il criterio di visibilità per evitare il precedente difetto `ListBoxItem.IsVisible` |
| `SaveStoreTests` e `DesktopTests` | Round-trip militare e prove di scenario storico | Estendere i test di uguaglianza con flotta/ID, conversione v10, stato corrotto, salvataggio in pausa e UI a due risoluzioni |

## Contratti tecnici consigliati, indipendenti dai prezzi

1. **Identità distinta:** navi con ID persistente e monotono nell'ambito della partita; non usare la posizione della riga come identificatore. Una nave non è un edificio né una scorta per Magazzino.
2. **Atomicità nell'aggregato:** il prezzo viene scalato e la nave aggiunta solo se tutte le condizioni sono soddisfatte. Su errore, risorse, progressivo ID, flotta ed esercito restano identici. La concorrenza tra comandi passa dall'esistente lock applicativo, non da un lock aggiuntivo in GUI.
3. **Versioni e compatibilità:** una partita **v10** priva di flotta può diventare **v11** con flotta vuota, mentre una partita v11 priva di dati navali è corrotta e va rifiutata. La Foundation e i factory storici non devono contenere stato militare/navale fuori versione.
4. **Validazione degli ID:** vietare ID zero, negativi, ripetuti, oltre limite e progressivo che precede l'ultimo ID allocato. Conservare l'ordine deterministico al salvataggio e dopo il reload.
5. **Tempo:** se acquisto immediato, l'operazione non avanza il tempo. Se si sceglie il timer, il suo stato va salvato e `Advance` deve rispettare pausa, partizionamento del tempo e round-trip come la Caserma.
6. **Nessun accoppiamento anticipato:** le missioni M5.5 saranno le sole a riservare navi/soldati. M5.4 non deve lanciare viaggi, consumare un contingente o applicare perdite e ricompense.

## Test obbligatori mirati alla migrazione

| Caso | Risultato richiesto |
| --- | --- |
| File v10 con reclute in formazione e più di zero Spade/Monete | Upgrade a v11 senza alterare gruppo, secondi residui, persone, scorte e tempo; flotta vuota |
| Riapertura dello stesso file dopo primo salvataggio v11 | Flotta preservata, backup v10 presente e apribile con scenario storico v10; nessun secondo backup cancellato |
| File v11 privo della tabella della flotta o della riga obbligatoria | Errore di dati invalidi; nessuna creazione silenziosa di flotta vuota |
| File v11 con `military_state` mancante, ma flotta valida | Errore di dati invalidi; la nuova versione non perde l'obbligo militare |
| Due acquisti consecutivi, anche dopo save/reload | ID univoci, costo scalato due volte e solo due volte, nessun duplicato |
| Acquisto non consentito dopo un salvataggio già valido | Nessuna spesa e nessuna nave, in memoria e dopo un nuovo round-trip |
| `economy-foundation` e factory `civil-growth-9/-10` | Nessuna scheda Flotta, stato storico valido e benchmark invariati |

## Ordine raccomandato per ridurre regressioni

1. **Gate aperto:** attendere riscontro Windows `validate.cmd` sulla v50 (o sull'archivio v51/v52, che conserva lo stesso codice).
2. **Decisione di gameplay:** approvazione/modifiche degli ID N-01…N-06 della [scheda v51](M5_4_DECISION_GATE_V51.md). L'analisi tecnica non sostituisce tale scelta.
3. **M5.4a:** entità e acquisto isolati nel Core, test atomici e solo dopo catalogo/rules version aggiornati.
4. **M5.4b:** persistenza SQLite, upgrade S1, backup e read model; test prima della GUI.
5. **M5.4c:** pagina Flotta e preflight senza partenze, con test Desktop a **1040×740** e **1240×850**, poi benchmark da colonia reale e gate Windows.

**Audit eseguito mediante lettura statica dei file citati.** SDK .NET non disponibile in questo ambiente; nessun benchmark o test v52 dichiarato eseguito.
