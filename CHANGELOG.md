# RC105 hotfix 2 — 08/11/2026 · Fixture Zecca / capacità fisica (CANDIDATA)

- **Evidenza Windows ricevuta dopo il primo hotfix:** compilazione riuscita; Core **248/248**, Application **602/603**, Infrastructure **262/262**, Desktop **247/247**: **1359 passati su 1360**, 1 fallito. `validate.cmd`/F1 interrotti dal test `Rc98AtomicMintCompletionTests.RemovedPrerequisitePreventsCompletionWithoutRefundingOrCrediting` (`System.ArgumentException` in `E4bV15FundingShadow.Restore`). Questi numeri descrivono l'esecuzione precedente, non il nuovo hotfix.
- **Causa:** il test azzerava la Fonderia d'oro nel checkpoint cittadino ma conservava nell'escrow la capacità fisica precedente, che include la quota di deposito locale della Fonderia. `Restore` respinge correttamente tale disallineamento.
- **Correzione solo test:** ricalcolo `Funding.Capacity` tramite `V15CityStorage.Calculate` dopo la rimozione della Fonderia; la verifica negativa di checkpoint con capienza falsificata rimane esplicita; controlli aggiunti sull'invarianza di stock, escrow e pagamenti committed. Il percorso `MissingPrerequisite` resta verificato **senza rilassare** la validazione del salvataggio.
- Nessuna modifica a codice applicativo, prezzo, curva, GUI, protocollo o formato salvataggi. **Non eseguiti qui test .NET 10 né gate F1**: rieseguire ` .\validate.cmd ` su Windows, attendendo **1360/1360** e gate completo prima di considerare RC105 convalidata.

---

# RC105 hotfix — 08/11/2026 · Correzione compilazione test (CANDIDATA)

- Risolto `CS0119` in `Rc105UnifiedSavePrototypeTests`: `System.Convert.ToInt64` evita il conflitto con il metodo helper `Convert` della stessa classe.
- Risolto `xUnit2013` in `Rc99NativeSaveBridgeTests`: `Assert.Single` sostituisce il controllo di lunghezza effettuato con `Assert.Equal`.
- Il `validate.cmd` eseguito su Windows l'08/11/2026 si era **fermato in compilazione con 2 errori**, prima di poter eseguire i test e il gate F1. Questa consegna corregge i sorgenti coinvolti; **compilazione, 1360 test attesi e gate F1 sono da rieseguire e non sono dichiarati superati**.
- Nessuna modifica a gameplay, GUI, prezzi, salvataggi o modello sperimentale. La versione rimane RC105 candidata; RC106 resta la prossima milestone funzionale.

---

# RC105 — 07/11/2026 · Prototipo SQLite generation 7 unificato (CANDIDATA)

- Base RC104 candidata; ultima baseline Windows confermata RC95 (**1245 test**). RC105 aggiunge **15 test Infrastructure**: **1360 test cumulativi attesi**, compilazione e gate Windows non ancora eseguiti.
- `E4bV15UnifiedSavePrototype`: esporta in un **nuovo file sperimentale SQLite** le tabelle reali della città v15 e, accanto a esse, identità persistente, stato del finanziamento, materiali versati e tutte le ricevute; non include un altro database come BLOB.
- Ingresso esclusivamente da composito registrato e autorevole secondo il preflight RC104; verifica revisioni e ramo prima e dopo la conversione, con snapshot SQLite consistente, staging privato e nessuna sovrascrittura.
- `Inspect`: controlla integrità, schema, identità, ricevute contigue e riconciliazione fisica di città e deposito tramite il loader generation 6 su una copia temporanea. Il file risultante è **esplicitamente NON autorevole** e non è aperto dal gioco: nessuna API per build, clock, salvataggio o promozione è esposta dalla RC105.
- Invariati GUI, prezzi, Municipio, save giocabile generation 6, unico `validate.cmd`. Niente `.local-cache` o `MANIFEST_*_SHA256.txt` nella distribuzione. [ADR 0116](docs/adr/0116-prototipo-sqlite-generation7-citta-e-cantieri.md), [piano test RC105](docs/validation/RC105_UNIFIED_SAVE_PROTOTYPE.md).

# RC104 — 07/11/2026 · Preflight per salvataggio giocabile unico (CANDIDATA)

- Base RC103 candidata; **RC95 rimane l'ultima versione confermata** (1245 test). RC104 aggiunge **15 test Infrastructure**, portando a **1345 test cumulativi attesi**; compilazione .NET 10 e gate Windows non eseguiti in questa consegna.
- `E4bV15UnifiedSavePreflight`: controlla SQLite, identità della campagna, ricevute e ramo autorevole senza effettuare migrazioni o trasferimenti. Impedisce di classificare un cantiere aperto come esportabile nel formato generation 6, che non potrebbe conservarlo.
- ADR 0115 e piano RC104: il formato nativo futuro resta da progettare, così come la transazione unica e l'autorità nel salvataggio giocabile. Il composito rimane un laboratorio separato.
- Pianificati punteggio e classifica locale **dopo la stabilizzazione del gameplay**; l'API della classifica online è riservata alla fase finale. Nessuna implementazione anticipata.
- Invariati GUI, salvataggi normali, prezzi e Municipio; unico `validate.cmd`, nessuna `.local-cache` o manifest SHA-256 negli ZIP.

# RC103 — 07/11/2026 · Identità persistente della campagna sperimentale (CANDIDATA)

- Partenza RC102 candidata; **RC95 è l'ultima versione confermata** (1245 test). RC103 aggiunge **12 test Infrastructure**, per **1330 attesi**. Nessuna compilazione/test .NET 10 eseguiti in questa consegna.
- Ogni nuovo contenitore composito memorizza un `campaign_id` univoco, indipendente dal percorso. Il registro lega la propria autorità all'identità del file e la verifica prima di `Current`, pagamento e clock. Una sostituzione del ramo attivo con una campagna differente viene respinta.
- Backup e ripristino conservano l'identità; la promozione accetta esclusivamente copie dello stesso ramo con contenuti e ricevute equivalenti. Tre nuovi trigger proteggono l'ID sui compositi registrati, conservando gli otto trigger RC102.
- Migrazione dei database sperimentali RC101–RC102 registrati e assegnazione controllata di ID ai compositi legacy non registrati. Un backup anteriore all'identificazione non può essere promosso automaticamente.
- **Invariati** salvataggio giocabile v15 generation 6, GUI, costi ordinari, MintTen opt-in e Municipio con 100 Carbone e 2 Matite. Unico `validate.cmd`; nessun manifest SHA-256 o `.local-cache` nello ZIP. [ADR 0114](docs/adr/0114-identita-stabile-campagna-composita-e-autorita.md), [piano RC103](docs/validation/RC103_CAMPAIGN_IDENTITY.md).

---

# RC102 — 07/11/2026 · Vincolo di scrittura sui compositi registrati (CANDIDATA)

- Baseline Windows confermata RC95: **1245 test superati**. RC96–RC101 restano da verificare; RC102 aggiunge **12 test Infrastructure**, per **1318 test attesi**. Compilazione .NET 10 e gate F1 non eseguiti in questa consegna.
- Registrazione dell'autorità con tabella di binding e otto trigger SQLite persistenti; accessi diretti di scrittura e SQL ordinario rifiutati sui file registrati. Il registro mantiene il lock e autorizza la connessione della sola operazione controllata; i compositi legacy non registrati rimangono compatibili.
- Backup, ripristino e promozione mantengono il vincolo; rifiuto registri copiati in altri percorsi e candidati divergenti; apertura di registri RC101 preesistenti applica il vincolo al ramo autorevole. Test RC101 di alterazione intenzionale delle ricevute adattato al nuovo scenario.
- ADR 0113, piano test RC102, stato e roadmap operativi unici; `validate.cmd` solo comando pubblico, ZIP senza manifest SHA-256 e senza `.local-cache`. Nessuna modifica a prezzi, GUI, gameplay ordinario o SQLite giocabile.

---

# RC101 — 07/11/2026 · Autorità cooperativa dei salvataggi compositi (CANDIDATA)

- Baseline Windows confermata RC95: **1245 test superati**. RC96–RC100 non ancora confermate; RC101 aggiunge **12 test Infrastructure**, per **1306 test cumulativi attesi**. Nessuna compilazione .NET 10 eseguita in questa preparazione.
- Nuovo `E4bV15FundingAuthorityRegistry` nello spazio sperimentale: un solo percorso composito autorevole, epoca e token persistenti. Pagamenti e tick passano attraverso un lock SQLite condiviso fra scrittori cooperanti.
- Cambio d'autorità esplicito soltanto verso una copia con stato della città, deposito e ricevute identici; il vecchio token è invalidato. Rifiuto di backup obsoleti o ricevute alterate. Originali non sovrascritti.
- Limiti: **non** protegge chiamate dirette allo store, registri copiati o file giocabili; non abilita Curve A nel gameplay. Non cambiano GUI, prezzi, schema native v15 o dotazione iniziale.
- ADR 0112, piano RC101, documenti operativi unici, gate aggiornato a 1306; `validate.cmd` unico comando pubblico, nessun manifest SHA-256 o cache distribuita.

---

# RC100 — Backup coerente e ripristino del contenitore finanziamenti (07/11/2026; CANDIDATA)

- Introdotto `E4bV15FundingCompositeRecovery` in Infrastructure.Experimental. Usa l’API SQLite di online backup per copiare un unico snapshot coerente del database composito, **anche con versamenti pendenti**, in un nuovo file; staging validato (`integrity_check`, ripristino città/escrow/ricevute) e pubblicazione senza overwrite. Ripristino soltanto in una nuova destinazione, senza alterare il backup o il database di origine.
- Rafforzato `E4bV15FundingCompositeSqliteStore.Load()`: le ricevute devono corrispondere a tutte le revisioni 1..N, senza lacune, duplicazioni di revisione o campi vuoti. Errori strutturali impediscono sia l’utilizzo sia il backup del database alterato. Nessuna modifica al formato SQLite composito.
- **12 nuovi test Infrastructure, totale 1294 attesi**, ancora da compilare e verificare su Windows. Ultima baseline confermata: RC95 con 1245 test; RC96–RC100 candidate. Unico comando `validate.cmd`, nessun manifest né `.local-cache` nello ZIP.
- Il cantiere resta **sperimentale**: niente GUI, cambio salvataggio del giocatore, prezzi Curve A attivi o prova di progressione naturale. [ADR 0111](docs/adr/0111-backup-consistente-ripristino-composito-e-ricevute.md), [piano test](docs/validation/RC100_COMPOSITE_BACKUP_RECOVERY.md).

---

# RC99 — Ponte non distruttivo fra salvataggi v15 e laboratorio finanziamenti (07/11/2026; CANDIDATA)

- Nuova API `E4bV15FundingSaveBridge` per **importare in sola lettura** un salvataggio v15 generation 6 verso un nuovo contenitore composito, oppure **esportare** un checkpoint senza finanziamento pendente verso un nuovo salvataggio giocabile. Gli originali non vengono modificati o sovrascritti.
- Import/export usano uno staging privato nella directory di destinazione, validazione e pubblicazione senza overwrite. L'export di un cantiere incompleto è vietato: non perdere silenziosamente le rate. Il database composito conserva sempre le ricevute; il file v15 esportato contiene soltanto la città e gli edifici già completati.
- 12 nuovi test Infrastructure; **1282 cumulativi attesi**, non eseguiti qui. Baseline Windows confermata: RC95 (1245); RC96–RC99 candidate. Prezzi/GUI/salvataggio normale e `validate.cmd` invariati; nessun manifest nelle release. [ADR 0110](docs/adr/0110-ponte-sicuro-salvataggi-v15-e-finanziamenti.md).

---

# RC98 — Completamento atomico della Zecca finanziata (07/11/2026; CANDIDATA)

- **Baseline confermata RC95:** 1245 test verdi. RC96–RC97 non ancora convalidati su Windows. **RC98 aggiunge 13 test** (7 Application, 6 Infrastructure), per **1270 attesi**, non eseguiti qui.
- `E4bV15FundingShadow.TryCompleteMint` costruisce una Zecca **solo nello shadow sperimentale** dopo un deposito interamente pagato, con ricerca/prerequisiti ricontrollati. Nessun secondo addebito: tier, contatore storico e capacità aggiornati in una sola proiezione validata.
- L'escrow registra ricevute di completamento con tier e costo Curve A, riconciliandole con `Committed` e il patrimonio reale; seconda Zecca con costo crescente. `E4bV15FundingCompositeSqliteStore.Apply(Complete)` salva immagine campagna, escrow, nuova revisione e ricevuta idempotente nello stesso commit.
- Nuova CLI `--study-e4b-v15-mint-commit` con città **sinteticamente pre-provvista** (non una prova naturale), integrata nel gate F1. Corretto il test RC95 che in precedenza rifiutava a priori l'azione `Complete` anziché rifiutarne l'esecuzione senza pagamento.
- GUI, giocabilità, prezzi correnti, SQLite generation 6, `validate.cmd` unico, README e screenshot **non modificati**. Niente `validate-f1.cmd` o manifest SHA256. [ADR 0109](docs/adr/0109-completamento-atomico-zecca-finanziata-in-sandbox.md), [piano](docs/validation/RC98_ATOMIC_MINT_COMPLETION_TEST_PLAN.md).

---

# RC97 — Clock reale e finanziamento nello stesso checkpoint (07/11/2026; CANDIDATA)

- **Baseline confermata RC95:** 1245 test passati. RC96 ha modificato il gate, che resta da riconfermare. RC97 aggiunge **12 test** (6 Application e 6 Infrastructure); **1257 test attesi**, compilazione e gate non eseguiti qui.
- `E4bV15FundingShadow.TryAdvanceClock`: un secondo del clock nativo v15, con controllo fisico `V15StockAuditor` e mantenimento dei versamenti escrow; ledger di variazioni nette della sola città.
- `E4bV15FundingCompositeSqliteStore.AdvanceClock`: commit atomico di immagine SQLite v15, registri del finanziamento e ricevuta; protezione da retry, revisioni obsolete e riuso di ID.
- Compatibilità in lettura dei vecchi checkpoint **sperimentali** RC95 privi dei registri del clock. Salvataggi del gioco generation 6 invariati; nessuna costruzione concessa o curva A abilitata.
- CLI e gate: `--study-e4b-v15-funding-clock`, mantenuto come unico comando pubblico `.\validate.cmd`; nessun `validate-f1.cmd` e nessun manifest SHA-256 di release. ADR 0108, piano di verifica, roadmap e handoff aggiornati.

---

# RC96 — Comando unico, README con screenshot e cache di validazione limitata (07/11/2026)

- RC95 **validata dall'utente con 1245 test superati**. RC96 non cambia gameplay, salvataggi, Desktop o test xUnit; resta da verificare su Windows il nuovo gate.
- Eliminato `validate-f1.cmd`. Unico comando pubblico `validate.cmd` → gate F1 → `scripts/validate.ps1` in processo separato, senza ricorsione. Conservati i controlli di screenshot e native smoke e il numero atteso di test.
- Rifiuto di report `validation.json` obsoleti; pulizia automatica delle vecchie cartelle GUID in `m4/runs` e `m4/f1-gates`, ultimo log anche in caso di errore, NuGet e salvataggi intatti; pulizia manuale `clean.cmd -CacheOnly`.
- README sintetico con schermata vera conservata e ritagli esplicitamente identificati. Aggiornati stato, roadmap, indice, handoff, guida sviluppo, ADR 0107 e piano RC96.
- Non si distribuiscono più file `MANIFEST_XXX_SHA256.txt`. La verifica ZIP usa CRC e controlli locali; nessun checksum per release allegato.

---

# RC95 — 07/11/2026 · SQLite composito v15 + finanziamento Curve A (CANDIDATA)

- **Baseline confermata dall’utente:** RC94.2, 1233 test verdi. RC95 aggiunge **12 test Infrastructure**, totale **1245 attesi**: la compilazione .NET e il gate Windows non sono stati eseguiti in questo ambiente.
- Introdotto `E4bV15FundingCompositeSqliteStore`, contenitore sperimentale autonomo: un'immagine nativa e validata della campagna v15 e il checkpoint escrow vengono salvati con revisione e ricevuta di richiesta in **una transazione SQLite**.
- Gestione `BeginMint`/`Deposit`/`Cancel` con revisioni ottimistiche, richieste idempotenti, replay dopo riavvio, rifiuto collisioni e stock/capienza riconciliati; proibiti `DeliverHypothetical` e `Complete` nel composito.
- Rifiuto preventivo dei salvataggi normali v15 generation 6 e del precedente database RC93; nessun cambio a GUI, clock, prezzi, salvataggi normali o regole edifici. **Non** è ancora un cantiere utilizzabile nella partita.
- Documentazione: [ADR 0106](docs/adr/0106-contenitore-sqlite-composito-v15-finanziamento-isolato.md), [Piano RC95](docs/validation/RC95_COMPOSITE_SQLITE_TEST_PLAN.md), roadmap/handoff/stato correnti aggiornati.

# RC94.2 — 07/11/2026 · Hotfix test Zecca dopo Municipio con 100 Carbone (CANDIDATA)

- **Evidenza Windows RC94.1:** 1233 test eseguiti, **1228 passati e 5 falliti**, tutti nella teoria parametrizzata `Rc81LiveMintCrewTests.LiveClockConsumesInputsOnlyForCompletedMintTeams` (personale 0/9/10/19/20). Il test si aspettava 6/5/4 Carbone anziché 106/105/104, perché il Municipio fornisce ora 100 Carbone iniziali.
- **Correzione minima:** il test confronta il **consumo del tick** (`iniziale - finale`) per Oro e Carbone con il numero dei moduli produttivi, invece di pretendere un saldo assoluto. La convalida delle squadre e la produzione Monete restano inalterate.
- **Nessun cambiamento funzionale:** motore Core/Application/Infrastructure, GUI, SQLite, inventari iniziali, prezzi e finanziamento sandbox RC94 invariati. Unico sorgente C# modificato: **test RC81**.
- **Totale invariato: 1233 test attesi**. Nessuna compilazione .NET 10 eseguita nell'ambiente di preparazione; nuova prova Windows e gate F1 richiesti. [Piano hotfix](docs/validation/RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md).

---

# RC94.1 — 07/11/2026 · Municipio: riserva iniziale di 100 Carbone (CANDIDATA)

- **Richiesta esplicita:** ogni nuova partita v15 riceve dal Municipio **100 Carbone oltre alle 2 Matite**. Il Carbone entra nell'inventario ordinario e usa la capacità reale del Municipio (1.000 per risorsa); rimangono le risorse base preesistenti (120 Legno lavorato, 80 Pietra e 30 Cibo). Non cambia la dotazione delle campagne storiche.
- La dotazione viene applicata unicamente in `V15FoodCampaignCheckpoints.NewCampaign`, **mai** durante `Load`, `FromMigrationPlan` o un avanzamento del clock. Le scorte già consumate nei salvataggi non si rigenerano.
- Aggiornati test su bootstrap, migrazione, inventario della GUI, caricamento e audit RC87; **1 nuovo test** verifica che il laboratorio della Scuola consumi 1 Carbone e 1 Legno lavorato producendo 4 Matite **prima** di Estrazione I.
- RC94 shadow Curve A invariato; salvataggi v15 generation 6, prezzi attivi, layout pixel art, filtri, produzione e personale invariati. **1233 test attesi**, baseline Windows confermata RC93.1 (1220); compilazione/gate RC94 e RC94.1 da eseguire. [ADR 0105](docs/adr/0105-riserva-carbone-municipio-avvio-v15.md), [piano RC94.1](docs/validation/RC94_1_MUNICIPIO_CARBONE_TEST_PLAN.md).

---

# RC94 — 07/11/2026 · Checkpoint composito v15 + escrow Curve A (CANDIDATA)

- Baseline **RC93.1: 1220 test superati**, confermati dall'utente. RC94 **+12 test Application**, gate **1232 attesi**, ancora da compilare/verificare qui.
- Introdotto `E4bV15FundingShadow`: copia di checkpoint v15 e deposito Curve A nella **stessa proiezione**, con scarico materiale unico, verifiche di validità città, prezzi congelati, contatori storici, capacità reale, revisioni, storno e ripristino coerente.
- Il composito rifiuta inventari divergenti, capacità/contatori falsi, consegne ipotetiche o spese già completate nel vecchio sandbox. Il controllo delle ricerche e dei prerequisiti per la Zecca è preparatorio; **non** autorizza una costruzione reale.
- Aggiunta CLI `--study-e4b-v15-shadow` e controllo nel gate F1, ADR 0104, piano RC94 e aggiornamento degli unici documenti operativi.
- **Invariati:** GUI pixel art, Core, Infrastructure, clock, prezzi e formato SQLite v15. Il contratto è sperimentale **in memoria**; il finanziamento non è ancora attivo nella partita.

---

# RC93.1 — 07/11/2026 · Hotfix CS0246 ProductionTier (CANDIDATA)

- Corretto l'errore di compilazione `CS0246` in `src/SimRisorse.Infrastructure/Experimental/E4bCurveAFundingSqliteStore.cs`: aggiunto `using SimRisorse.Core.Construction;`, che espone l'enum `ProductionTier` usato per deserializzare il checkpoint.
- Nessun cambiamento a transazioni, importi, idempotenza, prezzi, GUI, Core, salvataggi della partita o struttura SQL; la RC93 resta uno studio isolato.
- Conteggio invariato: **1220 test attesi**. La compilazione Windows e i test RC93.1 non sono ancora stati eseguiti; ultima baseline confermata RC90.3 (**1167/1167**). Run richiesto: ` .\validate-f1.cmd `.

---

# RC93 — 07/11/2026 · Checkpoint e persistenza SQLite isolata per Curva A (CANDIDATA)

- Aggiunto `E4bFundingCheckpoint` con esportazione/ripristino validato (quote congelate, stock, capacità, consegne ipotetiche, versamenti, costi completati, contatore, tier). Aggiunto `ProgressiveConstructionEscrow.Restore` con verifica di capienza della quota pagata rispetto al preventivo.
- Aggiunto `E4bCurveAFundingSqliteStore` in Infrastructure/Experimental: database **distinto dai salvataggi**, application_id proprio, versione formato 1 isolata, operazioni interamente transazionali con checkpoint+ricevuta, chiavi idempotenti, comparazione payload e controllo di revisione. Rollback delle richieste respinte; riavvio, rimborso e completamento sicuri nel sandbox.
- Aggiunti **17 test xUnit** (5 Application e 12 Infrastructure), gate F1 **1220 attesi**. Confermati dall'utente solo i **1167** della RC90.3; RC91.3, RC92 e RC93 rimangono da verificare con .NET 10/Windows.
- **Invarianti:** solo laboratorio, rifornimenti ipotetici; non cambiano prezzi di partita, GUI pixel art, SQLite della campagna v15 (`user_version=6`), simulazione, lavoratori, salvataggi né impostazione Legacy/MintTen. ADR 0103 e piano RC93 aggiornati.

---

# RC92 — 06/11/2026 · Sandbox finanziamento progressivo Curve A (CANDIDATA)

- Aggiunto `E4bCurveAFundingSandbox` in Application: preventivo Curve A congelato per tier, versamenti atomici in memoria, contatore comune delle Zecche aggiornato **solo al completamento** e rimborso annullamento vincolato alla capienza fisica. Nessuna concessione al motore v15.
- Rifornimenti etichettati **ipotetici**: conservazione materiali fra stock/escrow/spesa, nessuna produzione naturale presunta e nessun cambiamento di prezzi live, salvataggi, GUI o lavoratori.
- Demo CLI `--study-e4b-curve-a-funding` (6 acquisti per 3 tier, JSON/CSV/Markdown) e 15 nuovi casi xUnit Application. Il gate F1 verifica ambito e riconciliazione; **1203 test attesi**.
- ADR 0102, piano test RC92 e documentazione operativa unica aggiornati. Baseline completamente confermata **RC90.3: 1167 test**; RC91.3 (1188) e RC92 (1203) ancora da validare in Windows. **SDK .NET 10 non disponibile** nell'ambiente di preparazione.

---

# RC91.3 — 06/11/2026 · Hotfix nomi screenshot della scheda Edifici (CANDIDATA)

- **Risultato Windows RC91.2:** compilazione, test e pubblicazione del Desktop hanno raggiunto il controllo dei render; `validate.ps1` ha fermato il gate perché cercava `v15-buildings-1040.png`, mentre il nuovo test dei filtri per settore genera `v15-buildings-all-1040.png` (oltre a `food` e `industry`). La diagnostica ricevuta non conferma ancora l'esito integrale della suite né il gate F1.
- **Correzione:** `scripts/validate.ps1` richiede ora **sei** screenshot della scheda Edifici, con settori `all`, `food` e `industry` sia a **1040×740** sia a **1240×850**. Restano obbligatori anche i render Scuola, Militari e tutti i render storici; nessun controllo grafico è stato disabilitato. In caso di file mancante, il messaggio elenca i PNG effettivamente prodotti.
- **Perimetro:** un solo script di validazione, etichetta del gate, documentazione e manifest. **Nessuna modifica ai sorgenti o test C#**, nessuna modifica alla GUI, al gameplay, ai salvataggi o ai prezzi. **1188 test attesi**, conteggio invariato; ultima baseline completamente confermata dall'utente **RC90.3 (1167/1167)**.

---

# RC91.2 — 06/11/2026 · Hotfix test evidenziazione settore attivo (CANDIDATA)

- **Evidenza Windows RC91.1:** `SimRisorse.Desktop.Tests` ha eseguito 247 test: **246 passati, 1 fallito**. `ExactlyOneButtonHasActivePresentationAfterEveryChange` usava `Single(other => other.Sector != option.Sector)`: con nove categorie trova otto pulsanti non attivi e genera `InvalidOperationException`.
- **Correzione:** per ogni categoria scelta si verifica un unico `IsSelected == true`; tutti gli altri pulsanti devono avere `IsSelected == false` e colori di sfondo, primo piano e bordo distinti dall'attivo. Il test ora usa `Assert.All` sui pulsanti non selezionati, senza scegliere arbitrariamente un singolo elemento.
- **Perimetro:** modifica soltanto `Rc91BuildingSectorFilterTests.cs`, nome milestone gate e documenti operativi. **1188 test cumulativi attesi**, conteggio invariato; nessuna modifica a Desktop applicativo, Core, Application, Infrastructure, GUI, gameplay, prezzi o SQLite. Compilazione e suite RC91.2 da verificare su Windows; ultima baseline integralmente confermata **RC90.3 (1167 test)**.

---

# RC91.1 — 06/11/2026 · Hotfix xUnit2029 (CANDIDATA)

- Evidenza Windows RC91: compilazione interrotta dall'analizzatore **xUnit2029** in `Rc91BuildingSectorFilterTests.cs:63`. L'asserzione `Assert.Empty(collection.Where(predicate))` è stata sostituita con `Assert.DoesNotContain(collection, predicate)`, preservando il significato del test.
- Modifica limitata a **un'asserzione nel progetto Desktop.Tests**, gate identificato come RC91.1 e documenti operativi. **1188 test attesi** (nessun test aggiunto o eliminato). Compilazione e suite complete non eseguite in questo ambiente; ultima baseline Windows confermata: **RC90.3, 1167 test superati**.
- Restano invariati Core, Application, Infrastructure, Desktop applicativo, GUI, salvataggi, costi e regole produttive.

---

# RC91 — 06/11/2026 · Barra «Filtra» per settori nella scheda Edifici (CANDIDATA)

- **RC90.3 validata dall'utente:** tutti i **1167 test superati**; layout e icone dell'Albero tecnologico funzionano correttamente.
- Rimosso il pulsante con toggle «Filiere alimentari» nella scheda Edifici v15 e sostituito con etichetta fissa «Filtra» + pulsanti compatti Tutti, Alimentazione, Estrazione, Industria, Lavorazioni, Logistica, Abitazioni, Civici e Militari. La disposizione usa WrapPanel per non sovrapporsi alle intestazioni su finestre strette.
- Classificazione esplicita per famiglia/identificativo stabile: tutti i tier e gli edifici alimentari superiori appartengono al proprio settore, compresi quelli bloccati (consultabili, attenuati). Conservo la selezione finché è visibile; quando si cambia settore seleziono la prima voce utile. «Tutti» non perde edifici o cambia ordine di catalogo.
- **21 nuovi casi xUnit** per classificazione, dettaglio selezionato, refresh, gruppi, sblocchi; adattati il precedente test del vecchio pulsante e i test visuali a 1040×740 / 1240×850. Gate F1 **1188 test attesi**, da eseguire su Windows .NET 10.
- Modifiche confinante a Desktop, Desktop.Tests, gate, ADR 0101 e documentazione operativa unica. Nessuna modifica a Core, Application, Infrastructure, prezzi, tecnologie, clock, risorse o SQLite. La curva A e i pagamenti frazionati restano fuori dal gameplay fino a prova di sostenibilità.

---

# RC90.3 — 06/11/2026 · Icone nell'Albero tecnologico (CANDIDATA)

- Aggiunte **icone pixel art** nei due elenchi Tecnologie, v14 e v15, riutilizzando i pittogrammi approvati. Navigazione ha un nuovo sprite originale 8×8 (`ship`); i rami senza icona specifica usano `research`.
- La sezione **SBLOCCA EDIFICI** è una lista visuale di righe con **icona edificio + nome + livello Grande/Avanzato**, derivata dagli sblocchi reali del catalogo. In v15 Alimentazione I/II include Deposito alimentare Grande/Avanzato; non riappare il vecchio `food-store`. Conservati i prerequisiti, gli stati dei rami non disponibili, i bottoni e gli schemi della GUI.
- Rimosso il testo ridondante con gli stessi nomi dalla descrizione tecnologia; il riepilogo v14 mantiene il testo dei prerequisiti e non ripete il nome nel sottotitolo dell'elenco.
- Aggiunti **10 casi xUnit** (icone, sblocchi reali, tier, refresh selezione e binding Avalonia v14/v15); adattati i test testuali RC90 già presenti. Gate F1: **1167 casi attesi**. RC90.2 non è stata ancora verificata dall'utente; baseline certa RC89.1: **1135 passati**. Compilazione .NET 10 e xUnit RC90.3 da eseguire su Windows.
- Solo Desktop, Desktop.Tests, gate, README/docs e manifest; invariati Core, Application, Infrastructure, economie, capacità, prezzi e SQLite. [ADR 0100](docs/adr/0100-icone-albero-tecnologico-e-sblocchi-edifici.md).

---

# RC90.2 — 06/11/2026 · Hotfix fixture test Depositi alimentari (CANDIDATA)

- **Evidenza Windows RC90.1:** compilazione riuscita; quattro suite: Core 248/248, Application 549/557 (8 fallimenti), Infrastructure 136/136, Desktop 216/216; **1149 passati su 1157**.
- **Causa unica:** `Rc90FoodDepotTiersTests.Setup()` iniettava 10.000 unità sia di `planks` sia di `stone` nel magazzino v15, superando l'effettiva capacità dello stock cittadino; `V15CityCampaignValidator.ValidateNative` rifiutava correttamente lo stato prima che partissero gli otto test.
- **Correzione:** risorse aggiunte ridotte a 800 `planks` e 450 `stone`: inventario totale 920/530, nei limiti del Municipio (1.000 per risorsa), ma sufficiente ai costi della coppia Deposito Grande/Avanzato. Aggiunta verifica esplicita di capienza nel setup.
- **Perimetro:** cambia solo un fixture di test Application, oltre a documentazione e manifest. Nessuna modifica al validatore, alla capacità cittadina, alle regole di acquisto/produzione, alla GUI o a SQLite. Il gate rimane a **1157 test**, ancora da rieseguire su Windows; ultima baseline totalmente confermata RC89.1 (1135 test).

---

# RC90.1 — 06/11/2026 · Hotfix compilazione popolazione `long` (CANDIDATA)

- **Evidenza Windows RC90:** compilazione bloccata in `FoodIndustryCampaignState.cs:66` (`CS1503`: passaggio di `long` a `int` in `FoodIndustryBuildings.Validate`). La suite xUnit RC90 non è partita, quindi non è stata validata.
- **Correzione:** `FoodIndustryBuildings.Validate(long population, long busyOutsideFood)` mantiene il medesimo contratto di `PlannerPersonnel(long, long)` e `FoodIndustryPersonnel` senza conversioni esplicite potenzialmente pericolose. Gli altri overload restano invariati.
- **Copertura:** esteso un test Core già presente per verificare l’invocazione e la validazione con popolazione maggiore di `int.MaxValue`. Nessun test nuovo; il gate resta **1157 test attesi**.
- **Perimetro:** un metodo Core, un test Core già esistente e documentazione/manifest; invariati gameplay, formule, salvataggi, UI, tecnologia e deposito. Compilazione e test .NET 10 **da verificare su Windows**. Baseline confermata dall’utente: **RC89.1, 1135 test superati**.

---

# RC90 — Descrizioni delle tecnologie e Depositi alimentari Grande/Avanzato (06/11/2026)

**Candidata da validare.** RC89.1 confermata dall’utente con **1135 test passati**; RC90 aggiunge **22 nuovi test** e porta il gate a **1157 attesi**. Non è disponibile l’SDK .NET 10 per eseguire compilazione o xUnit qui.

- In Albero tecnologico v15, ogni tecnologia elenca edifici effettivamente sbloccati (Grande/Avanzato) e prerequisiti. Tecnologie differite non pubblicizzano ricompense inesistenti. La schermata v14 aveva già la descrizione.
- Alimentazione I/II abilita acquisto diretto di `food-depot-large`/`food-depot-advanced`, con 3/10 posti, costi scalati 2,6×/6,5×, capienza 3×/10×, prezzi progressivi per famiglia, lavoratori non condivisi tra tier e clock alimentare unico. Rimangono distinti dai produttori v14 `food-store` ritirati.
- Salvataggi v15 invariati come formato SQLite6, leggibili con sei o otto righe di `food_industry_site`; i due nuovi tier vengono scritti nelle righe già previste dal modello tabellare. Supporto di round-trip e carichi storici senza refill.
- GUI pixel art, icone, tabella Risorse e tooltip RC89.1 conservati. Nessuna attivazione curve economiche A/B/C o MintTen di default.
- ADR 0099 e test plan RC90; gate F1 aggiornato a **1157**.

---

# RC89.1 — Tooltip di Produzione/Consumo per fonte (06/11/2026)

**Stato:** candidata alla validazione Windows. **Baseline confermata:** RC88.1 con 1105 test passati; RC89 con 1123 test previsti non ancora verificati. **RC89.1:** +12 nuovi casi xUnit, **1135 cumulativi attesi**, compilazione .NET 10 non disponibile qui.

- **Risorse:** su hover delle singole celle `Produzione /s` e `Consumo /s` la GUI v15/v14/storica mostra origine/destinazione, numero edifici/unità attive, quantità /s e totale. Nessuna modifica alla larghezza/allineamento delle sette colonne e alle funzionalità già approvate.
- `V15UnifiedCampaignClock.Plan` raccoglie fonti dai medesimi planner che emettono i flussi lordi: produzioni/consumi delle ricette storiche, Stalla/Cacciatore/Macellaio, Orto/Frutteto, Deposito alimentare e pasti atomici. Spostamenti di scorte al deposito esclusi. Il `V15EconomyProjector` espone `Contributors` in modo read-only senza modificare checkpoint e SQLite.
- `ResourceFlowBreakdown` proietta le stesse metriche per v14 e MainWindow; mostrate solo le fonti con flusso positivo nel prossimo secondo. Per origini insufficientemente descritte appare una differenza esplicita, non un totale simulato.
- 9 nuovi test Desktop e 3 nuovi test Application sui totali, fonti, refresh, inattività, tre viste e integrità della tabella; gate F1 aggiornato a 1135. [ADR 0098](docs/adr/0098-tooltip-flussi-risorse-per-fonte.md) e [piano RC89.1](docs/validation/RC89_1_RESOURCE_FLOW_TOOLTIPS_TEST_PLAN.md).
- **Invariati:** costi, bilanciamento, squadre, salvataggi, slot SQLite, posizione degli elementi UI, tabella Risorse a sette colonne e ricette. Lo studio A/B/C della RC89 rimane candidato e non attivato.

---

# RC89 / E4b.3a — Prontezza attivazione prezzi e capienza (06/11/2026)

**Stato:** candidata, **1123 test attesi** (18 nuovi), SDK .NET 10 non disponibile qui. **Conferma utente:** RC88.1 **1105 test tutti passati**; chiusa la regressione da dizionario sparso in RC88.

- Nuovo studio `E4bPriceReadinessStudy`: per ogni curva A/B/C e tier Base/Grande/Avanzato riconcilia prezzo prima e seconda Zecca, lotto x2, equivalenti Tagliapietre, squadre MintTen, copertura della capacità iniziale risorsa per risorsa, percorso Magazzini ed escrow minimo teorico; ogni ipotesi rimane esplicitamente **non approvata e non attiva**.
- Nuova CLI `--study-e4b-price-readiness` con input RC88 facoltativo verificato dallo stesso audit delle ricevute, provenienza SHA-256, JSON/CSV/Markdown, rifiuto sovrascrittura; nessun risultato di completamento attribuito ai run diagnostici.
- 18 test nuovi, gate F1 cumulativo a **1123**, ADR 0097 e piano test; aggiornati unici file operativi di documentazione. Nessuna modifica a Core, gameplay, UI, SQLite, prezzi o save.
- Prossimo passo: test Windows, decisione esplicita su coefficienti e finanziamento sostenibile, poi implementazione opt-in con verifica naturale completa.

---

# RC88.1 — 06/11/2026 · Hotfix audit stock sparso (CANDIDATA)

- **Evidenza Windows RC88:** compilazione completata; test xUnit **1103 eseguiti, 1087 superati, 16 falliti**, tutti in `Rc88IndustrialChainAuditTests` per `KeyNotFoundException: swords`. Moduli Core **248/248**, Infrastructure **133/133**, Desktop **196/196** superati; in Application **510/526** superati. Il gate F1 non è stato superato.
- **Causa:** `ResourceBundle.Amounts` è un dizionario *sparso*: una risorsa con quantità zero può non comparire. L'audit indicizzava direttamente `initial["swords"]`, `initial["coins"]` e `initial[output]`. Sono stati sostituiti i tre accessi con l'indicizzatore di `ResourceBundle` che restituisce zero in assenza della chiave, mantenendo intatti gli esatti confronti di provenienza delle risorse non nulle.
- **Regression test:** aggiunta teoria parametrizzata su Spade e Monete iniziali assenti dal dizionario (2 casi); il gate F1 ora attende **1105** test. Nessuna modifica a gameplay, motore Core, Desktop, Infrastructure, SQLite, GUI, costi o progressione.
- **Verifica locale:** controllo statico dei tre accessi, controlli d'integrità e manifest dell'archivio completo. **Non eseguiti qui** i test .NET 10; RC88.1 è da validare su Windows con `./validate-f1.cmd`.

---

# RC88 — 06/11/2026 · E4b.2j, audit output industriale (CANDIDATA)

- **Ultima baseline confermata dall'utente:** RC86.1, **1067 test superati**. RC87 +18 test non ancora eseguiti; RC88 +18 test, totale **1103 previsti**, compilazione .NET 10 e suite da eseguire su Windows.
- `V15IndustrialProgressionBenchmark` registra nei milestone Spade/Monete i millesimi di produzione reale del primo secondo del clock (`ProducedMilliunits`). È un campo di evidenza del report, non nuova produzione nel gioco.
- Nuovo `V15IndustrialChainAudit`: ricevute ordinate e pagate, scenari Forgia/Zecca, regime Legacy/MintTen, tecnologie acquistate, team, primo output e stock fisico. Rifiuta completamenti non sostenuti da dati coerenti; timeout e diagnostiche non implicano softlock.
- Nuova CLI `--audit-v15-industry-chain` per sette percorsi default senza grant, diagnostica 0 secondi oppure simulazione con orizzonte esplicito, analisi offline `--input`, export JSON/CSV/Markdown senza sovrascrittura; test e gate estesi.
- [ADR 0096](docs/adr/0096-audit-filiera-industriale-ricevute-clock-v15.md), [piano test](docs/validation/RC88_INDUSTRIAL_CHAIN_TEST_PLAN.md) e documentazione operativa unica aggiornati. **Invariati** Core, Desktop, Infrastructure, prezzi, SQLite, GUI canonica, regime di default.

---

# RC87 — 06/11/2026 · E4b.2i, audit bootstrap ricerca (CANDIDATA)

- **Baseline RC86.1 confermata dall’utente:** tutti i 1067 test passati su Windows. La RC87 aggiunge **18 test xUnit Application**, portando il totale a **1085 attesi**, non verificati qui (SDK .NET 10 assente).
- Corretto `V15-I01`: la prima milestone `Estrazione I` non resta ostaggio di costruzioni di pane e carbone successive all'acquisto. `research-first` termina dopo l'acquisto; `bread-first` paga prima l'investimento nel pane. `V15-I02`/`V15-I03` invariati.
- Implementato `V15ResearchBootstrapAudit` con ricevute reali, controllo di avvio con 2 Matite, costo studente 2 Matite, costo ricerca 150 punti, verifica di sequenza e campione finale; rifiuta prove false e confronto tempi non dimostrato.
- Nuovo comando `--audit-v15-research-bootstrap` con output JSON/CSV/Markdown e gate F1 diagnostico a 0 secondi. Aggiunti ADR 0095 e piano test, aggiornati README, ROADMAP, CURRENT_STATE, AUTHORITY_REGISTRY e NEW_CHAT_HANDOFF senza duplicati.
- Nessun intervento su Core, Desktop, Infrastructure, salvataggi, prezzi attivi o regime `MintTen`. Controlli statici e archivio non equivalgono alla validazione .NET.

---

# RC86.1 — 06/11/2026 · Correzione compilazione xUnit2031 (CANDIDATA)

- Risolto il blocco di compilazione riportato dall'utente in `tests/SimRisorse.Application.Tests/Rc85MintHousingBufferTests.cs:44`: `Assert.Single(sequence.Where(...))` sostituito da `Assert.Single(sequence, predicate)`; stessa semantica di esattamente una corrispondenza.
- Corrette preventivamente due asserzioni con lo stesso antipattern in `tests/SimRisorse.Infrastructure.Tests/SaveStoreTests.cs` (checkpoint Municipio).
- **Nessuna modifica a Core, Application runtime, Desktop, Infrastructure runtime, gameplay, interfaccia, dati, salvataggi o dipendenze**; soltanto tre asserzioni di test e documenti di stato. Numero dei test invariato: **1067 cumulativi attesi**.
- **Verifica effettuata**: ricerca statica degli schemi `Assert.Single(...Where(...))`, controllo delle sostituzioni e integrità ZIP/manifest SHA-256. **Da eseguire su Windows**: `./validate-f1.cmd` con SDK .NET 10. La compilazione RC86 ha fallito sul codice precedente; la RC86.1 non è stata compilata qui (SDK assente).

---

# RC86 — 06/11/2026 · E4b.2h (CANDIDATA)

- Corretto il ciclo Scuola/Matite/Carbone: nuova campagna **v15** dotata di **2 Matite iniziali del Municipio**, accreditate una sola volta da `V15FoodCampaignCheckpoints.NewCampaign`. Desktop e benchmark naturali/industriali condividono questa factory. Import da v14 e caricamento di checkpoint esistenti **senza refill**; SQLite generation 6 invariato.
- Corretto l'ordine della ricerca nei runner industriali: primo studente con Matite iniziali, `Estrazione I` prima della Miniera di carbone; laboratorio Matite rimane necessario dopo il primo studente. Le misure naturali delle versioni precedenti sono storiche e non confrontabili a inventario identico.
- Rimosso nelle viste V14/V15 il costo testo ridondante; conservati titolo, dettagli a icone e preventivi 5/10; aggiunti 9 test mirati GUI e Core/Application (1067 cumulativi attesi).
- Documentazione consolidata: documenti operativi **unici** (`CURRENT_STATE`, `ROADMAP`, `AUTHORITY_REGISTRY`, `NEW_CHAT_HANDOFF`), README/indice riscritti, eliminati 11 file operativi duplicati e vecchie istantanee operative in `docs/history/operational`. Conservati ADR, piani/rapporti di test, decisioni tecniche, changelog e storia dei checkpoint.
- [ADR 0094](docs/adr/0094-starter-matite-e-documentazione-unica.md), [piano test RC86](docs/validation/RC86_BOOTSTRAP_UI_DOCS_TEST_PLAN.md). **Baseline test confermata dall'utente: RC82/1002; RC83–RC86 ancora non compilate/testate in questa consegna.**

---

# RC85 — 06/11/2026 · E4b.2g, strategia abitativa naturale per la Zecca (CANDIDATA)

- Esteso il benchmark `V15IndustrialProgressionBenchmark` con la terza politica **opt-in** `housing-buffer` solo per la Zecca. Mantiene tutti i passi di `bread-first` e tenta **due Case piccole aggiuntive** prima della filiera Oro, con costi e crescita demografica effettivi, senza grants né modifica al gameplay.
- CLI `--benchmark-e4b-mint-ten` con `--policy housing-buffer` o `--include-housing-buffer` (default storico a due strategie invariato). Con `--compare-legacy` genera fino a sei run omogenei.
- Nuovo `E4bMintHousingBufferStudy` con comando `--review-e4b-housing-buffer` di sola lettura: coppie `bread-first`/`housing-buffer` per regime, spese reali, case realmente pagate, hash SHA-256 di input e delta tempi **soltanto con doppio completamento naturale**. Export JSON/CSV/Markdown senza sovrascrittura.
- Corretto il criterio `CompleteNaturalPolicyPairObserved` di RC84: richiede espressamente `research-first` **e** `bread-first`, non due politiche arbitrarie.
- Aggiunti **20 casi xUnit**, totale cumulativo **1058 attesi** (RC82 1002 confermati; RC83 +18, RC84 +18, RC85 +20 ancora da eseguire). Gate F1 esteso con sei diagnostiche a 0 secondi + revisione offline non conclusiva. ADR 0093, specifica, piano test, README, roadmap e handoff aggiornati.
- Nessun file Core, Desktop o Infrastructure modificato; GUI canonica, SQLite, salvataggi, prezzi e modalità MintTen di default invariati. **Compilazione .NET e xUnit NON eseguiti qui** per indisponibilità SDK; controlli statici e integrità archivio distinti.

---

# RC84 — 06/11/2026 · E4b.2f, audit evidenze naturali MintTen vs proposte (candidata)

- Introdotto `E4bMintNaturalFeasibilityAudit` in Application/Benchmarks: distingue i dati reali del benchmark naturale (prezzi legacy, nessun grant) dalle nove ipotesi economiche di RC83 A/B/C, **non attivate**. Controlla scenario, contratto `rules_version` MintTen/Legacy, identico inventario iniziale/orizzonte e corretta catena costruzione → squadra → evento di prima produzione.
- Nuova CLI di sola lettura `--analyze-e4b-mint-ten --input RESULTS_JSON [--output DIRECTORY]`: importa risultati registrati, controlla ledger e milestone tramite analizzatori esistenti, esporta JSON/CSV/Markdown e SHA-256 dell'input, senza sovrascrittura o avanzamento clock. Le attese, i deficit misurati e i campioni di capacità satura **non** sono classificati come softlock.
- **18 nuovi casi xUnit**, **1038 cumulativi attesi** (RC83 +18, RC84 +18 sulla RC82 confermata a 1002). Gate `validate-f1.cmd` esteso: verifica su diagnostica RC82 a 0 secondi 4 run non conclusivi e 9 ipotesi dichiarate come non applicate.
- Creati ADR 0092, specifica e piano test; aggiornati README, roadmap e handoff. Nessuna modifica a Core, Desktop, Infrastructure, prezzi attivi, salvataggi o crescita ×20. **SDK .NET 10 non disponibile**: controlli statici/archivio, ma non build/test/benchmark naturale completo. [Piano](docs/validation/E4B_RC84_TEST_PLAN.md).

# RC83 — 06/11/2026 · E4b.2e, rischio di scalabilità della seconda Zecca (CANDIDATA)

- **RC82 validata dall'utente:** 1002/1002 test passati. Questa dichiarazione aggiorna il checkpoint operativo precedente RC78; i test RC83 restano da eseguire.
- Aggiunto lo studio **solo analitico** `E4bMintScaleRiskStudy`: **9 righe** A/B/C × Base/Grande/Avanzato, confronto una/due Zecche, squadra piena 10/30/100 → 20/60/200, budget di popolazione, filiera aggiuntiva, prezzo della seconda copia, lotto atomico e limite del percorso Magazzini. Rate teoriche calcolate dalla capacità iniziale, non finanziamento realizzato.
- Confronto del prezzo diretto delle due Zecche con il lavoro materiale di un Tagliapietre, distinguendo le fasce sotto 100, 100–1000 e sopra 1000. Non è una metrica temporale né la somma dei prerequisiti, e non attiva la curva A.
- CLI `--study-e4b-mint-scale` con JSON/CSV/Markdown protetti da sovrascrittura; **18 nuovi casi xUnit Application**, gate portato a **1020 test attesi** e controllo output separato. Aggiornati ADR 0091, specifica, roadmap, handoff, test plan.
- **Non modificati** Core, Desktop, Infrastructure, database SQLite, GUI approvata, prezzi live, crescita ×20, Deposito Cibo e default `Legacy`. SDK .NET 10 assente qui: nessuna build né prova RC83 eseguita.

---

# RC82 — 06/11/2026 · E4b.2d, benchmark naturale MintTen (CANDIDATA)

- Esteso **solo** `V15IndustrialProgressionBenchmark` con modalità esplicita `MintTen` per `V15-I03`, esposta come `E4B-M01`: identico inventario della nuova campagna RC77, nessun grant; una chiamata autentica `AssignCityProducerWorkers("mint", Base, 10)` invece di un singolo addetto.
- Entrambe le policy `research-first` e `bread-first` conservano i propri prerequisiti, la ricerca con Matite e il clock v15. Il benchmark registra eventi reali, attese, stock, popolazione, spese e milestone **solo se osservate**; legacy I01–I03 e test RC77 non modificati.
- CLI `--benchmark-e4b-mint-ten` con `--compare-legacy`, `--policy`, `--max-seconds` e output JSON/CSV/Markdown su directory nuova, protetta da sovrascrittura. Manifest E4b `not-run` e gate F1 esteso al controllo diagnostico da zero secondi senza dichiarare alcuna progressione naturale raggiunta.
- **17 nuovi casi xUnit Application**, totale **1002 attesi** (926 RC78 verificati, 76 successivi da compilare/testare). Aggiornati ADR 0090, specifica, validazione, roadmap, handoff e README.
- **Non modificati** Core, Desktop, GUI/XAML, Infrastructure, schema o contenuto SQLite, costi, default della Zecca, escrow e crescita ×20. Nessuna prova .NET 10 eseguita in questo ambiente.

---

# RC81 — 06/11/2026 · E4b.2c, attivazione opt-in squadre minime v15 (candidata)

- Corretto il difetto di pooling inter-tier della RC80: due gruppi da 9 lavoratori su Base/Grande della Zecca non costituiscono una squadra. Il `ProductionPlanner` usa ora il limite esplicito di moduli completi derivato dai singoli tier.
- Attivato il contratto nel runtime v15 **solo mediante scelta esplicita** `EnableMintCrews()`: Zecca 10 addetti per modulo Base/Grande/Avanzato (1/3/10 moduli), senza modificare le altre famiglie, i costi, la crescita o il default della GUI.
- Comandi `Assign/Release/Upgrade`, validatore nativo, clock per secondo, diagnostica e riepilogo GUI integrati; il personale resta in unità fisiche. La stessa sessione conserva il regime nel checkpoint.
- SQLite generation 6 invariata nelle tabelle e nel `user_version`, ma l'header `rules_version` distingue in modo esplicito `civil-growth-15` da `civil-growth-15-mint-crew10`. Salvataggio/caricamento rifiutano valori ignoti.
- **20 test nuovi**, 985 cumulativi **attesi**. RC79, RC80 e RC81 NON compilate/testate in questo ambiente; ultimo riscontro reale dell'utente: RC78 **926/926**. GUI v14 canonica mantenuta, 7 colonne risorse e pulsanti storici non modificati. [ADR 0089](docs/adr/0089-attivazione-opt-in-squadre-zecca-v15-versionata.md) · [Piano test](docs/validation/E4B_RC81_RUNTIME_TEST_PLAN.md).

# RC80 — 06/11/2026 · E4b.2b, squadre minime con planner autentico (candidata)

- Nuovo `CrewTeamPortfolio` in Core: moduli tier 1/3/10, capienza personale `moduli × CrewSize`, assegnazione/rilascio/costruzione immutabili, controllo per-tipo di squadre incomplete e deficit prossimo modulo.
- `ProductionPlanner` accetta un parametro **facoltativo** di dimensione squadra; per il gioco reale (nessun parametro) il comportamento resta identico, mentre la proiezione RC80 usa lo stesso algoritmo di priorità, input e magazzino per singola esecuzione del modulo. Aggiunto `ProductionBlockReason.InsufficientCrew`.
- `CrewProductionProjection` controlla capienza per tier, intero roster, assenza di doppia assegnazione e budget demografico comprensivo delle attività esterne. Studio con la ricetta autentica della Zecca su dieci fixture dichiarate, export JSON/CSV/Markdown con `--study-e4b-crews`.
- **22 test aggiunti** (18 Core, 4 Application), gate cumulativo aggiornato a **965 test attesi** e nuovo comando da verificare. Nessun test eseguito in questo ambiente: **926/926 RC78** rimane l'ultimo riscontro dell'utente. RC79 non ancora validata.
- Non modificati prezzi, squadre nella partita, GUI, crescita, tick del clock in v15, SQLite o salvataggi. [ADR 0088](docs/adr/0088-proiezione-squadre-minime-v15-isolata.md) · [Specifica](docs/design/E4B_RC80_CREW_MINIMUM_PROJECTION.md) · [Piano test](docs/validation/E4B_RC80_CREW_TEST_PLAN.md).

# RC79 — 06/11/2026 · E4b.2a, opzioni di finanziamento delle costruzioni (candidata)

- Nuovo prototipo isolato `ProgressiveConstructionEscrow` (Core): depositi materiali senza perdita/doppio conteggio, rimborso subordinato alla capienza, rate esatte al millesimo senza materializzare lunghe liste. **Non integrato** nel gioco e nei salvataggi.
- Nuovo confronto di 27 casi (3 curve × 3 tier × lotti 1/5/10): acquisto atomico, copie sequenziali e minimo teorico di rate su capacità dichiarata. CLI `--study-e4b-financing` con output JSON/CSV/Markdown indipendente dai report RC78.
- Aggiornato il gate F1 per verificare anche la nuova CLI e contare 17 nuovi casi xUnit: **943 test attesi**, **non compilati né eseguiti in questo ambiente**. Baseline RC78: **926 test superati dall'utente**, come registrato nell'archivio.
- Nessuna variazione di prezzi, produzione, lavoratori, crescita, GUI o SQLite; scelta di curva A/B/C, moduli e rateizzazione ancora sospesa. [ADR 0087](docs/adr/0087-studio-finanziamento-progressivo-senza-attivazione.md), [specifica](docs/design/E4B_RC79_FINANCING_FEASIBILITY.md), [piano test](docs/validation/E4B_RC79_FINANCING_TEST_PLAN.md).

# RC78 — 06/11/2026 · confronto analitico di tre curve economiche

- Studio isolato con 81 preventivi, prima/seconda copia e lotti 5/10 per tutti i tier, 18 investimenti di filiera, ricerca e personale alimentato/alloggiato. Nessuna modifica al gioco corrente.
- Audit di liquidità con Magazzini pagati, lotti atomici e upgrade separati: un percorso bloccato non viene chiamato stallo globale. Proiezione delle squadre Zecca da dieci e moduli candidati 1/3/10.
- CLI `--study-e4b-curves`, importazione opzionale e hash della baseline naturale, JSON/CSV/Markdown; cinque file identici al replay e cinque rifiuti CLI verificati.
- **926/926 test**, 27 nuovi casi; gate completo, build senza errori o avvisi, audit e smoke superati. [Rapporto](docs/validation/E4B_RC78_CURVE_COMPARISON.md), [verifiche](docs/validation/E4B_RC78_TEST_PLAN.md), [ADR 0086](docs/adr/0086-confronto-curve-economiche-isolato.md).
- Raccomandata A come base da decidere; nessun coefficiente applicato. Costi di filiera iniziale compresi Magazzini: circa 325/948/1.405 equivalenti di lavoro Tagliapietre per A/B/C. Tempi delle proposte non misurati; moduli tier e parametri da approvare prima dell'implementazione versionata.

# RC77-M01 — 05/11/2026 · misure naturali complete

- Eseguiti i tre scenari alimentari e i sei abbinamenti industria/politica con orizzonti integrali, senza grants: **9/9 milestone osservate**, zero timeout/blocchi/diagnostiche.
- Pane a 481s; Estrazione I a 2520s/2580s; prime Spade/Monete a 4563s/4623s (`bread-first`/`research-first`). A parità di spesa `bread-first` anticipa di 60s; la ricerca domina l'attesa industriale. Non sono tempi di una campagna completa o di un giocatore ottimale.
- Ripetuti i run e rianalizzati i JSON: **21/21 file identici**. Provenienza con SHA-256 di 144 sorgenti/configurazioni e tre assembly. Manifest aggiornati solo con esiti osservati; [rapporto](docs/validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md).
- Nessuna modifica al codice, alla GUI, alla crescita o ai prezzi; ultimo gate del codice **899/899**. Prossimo studio: tre curve E4b/RC78, coefficienti da approvare. Confronto interattivo F1 ancora distinto.

# RC77 — 05/11/2026 · E4a.3, produzione non realizzata e recupero controllato

- Telemetria opzionale del planner eseguito dal clock v15: nominale/prodotto e deficit esclusivi per spazio/input, con vincoli misti distinti. Tick e flussi invariati quando la diagnostica è attivata; nessuna modifica a GUI, prezzi, crescita o SQLite.
- Nuovo benchmark controllato con tre scenari e due politiche, checkpoint finanziati dichiarati, comandi reali, ricevute e riconciliazione fisica per secondo. CLI, JSON, CSV campioni/azioni e Markdown, senza sovrascritture.
- Misurati in 120s: Magazzino → 240 Spade o 360 Monete; due addetti aggiuntivi al Carbone → 238 Spade e 357 Monete. Replay identico su quattro file; i manifest naturali restano `not-run`.
- Quattordici regressioni nuove; gate esteso ai sei esperimenti. **899/899 test superati** su Windows, build senza errori o avvisi, smoke nativo esistente e audit superati. [Specifica](docs/design/E4A_V15_RC77_PRODUZIONE_PERSA_E_RECUPERO.md), [evidenze](docs/validation/E4A_V15_RC77_TEST_PLAN.md), [ADR 0085](docs/adr/0085-misure-produzione-non-realizzata-e-recupero-controllato.md).

# RC76-HF04 — 05/11/2026 · icone, intestazione e crescita ×20

- Su richiesta: Bovini con testa di mucca, Pane più marrone, Pescatore con pesce, Tagliapietre con cubo grigio. Palette dedicata al Pane senza modificare grano o tetti.
- Intestazione v15 fissa Abitanti, Cibo, Legno lavorato, Pietra; pulsanti «Carica» e «Salva» senza suffisso.
- Tetto della crescita v15 elevato a ×20, con bonus abitativi precedenti e calcolo unico per clock e forecast. A pieno bonus arriva un abitante ogni 3 secondi, se alloggi e razioni lo consentono; checkpoint e schema SQLite invariati.
- Undici regressioni nuove: **885/885 test superati** nel gate completo `validate-f1.cmd`, incluso `validate.cmd`; build senza errori o avvisi, smoke e audit baseline/combinato superati. [Rapporto HF04](docs/validation/RC76_HF04_ICONS_AND_GROWTH.md).

# RC76-HF03 — 05/11/2026 · correzioni grafiche v15

- Filtro filiere su una riga dedicata per non sovrapporre le intestazioni a 1040×740; costi con nomi italiani coerenti con le righe con icone; materiali Scuola/Esercito dentro le card con testo che va a capo.
- Cinque nuove regressioni Desktop e otto catture v15 richieste da `validate.cmd`; gate F1 aggiornato a **874 test** e milestone HF03.
- `validate-f1.cmd`, incluso `validate.cmd`, superato su Windows: **874/874**, zero falliti o saltati, build senza errori o avvisi, smoke nativo esistente e audit baseline/combinato da 120s superati.
- Ispezionate le otto catture v15 a 1040×740 e 1240×850. Il confronto interattivo Windows resta pendente; F1-D05e formalmente aperta. [Rapporto e limiti](docs/validation/RC76_HF03_VISUAL_REGRESSIONS.md).
- Nessuna modifica a regole economiche, formule produttive, prezzi o SQLite.

# RC76-HF02 — 05/11/2026 · validazione Windows e diagnostica E4a

- Corretto il test di regressione sul personale: confronto tra sessione e benchmark dopo le stesse quattro azioni al secondo zero, verificando 5 lavoratori iniziali e 3 dopo le assegnazioni.
- Eseguiti `validate.cmd` e `validate-f1.cmd` su Windows con SDK 10.0.201: **869/869 superati**, zero falliti o saltati, build senza errori o avvisi; smoke nativo e audit v15 baseline/combinato superati.
- Eseguita diagnostica industriale V15-I01 / research-first da 120 secondi; `diagnostic-only`, Scuola in attesa di 73 unità di Legname lavorato. Rianalisi con sei report identici byte per byte. Manifest a orizzonte pieno ancora `not-run`.
- Aggiornati documenti operativi e [rapporto delle esecuzioni](docs/validation/RC76_WINDOWS_VALIDATION.md). Verifica visiva GUI ancora da confermare, F1-D05e formalmente aperta; nessun cambiamento al motore, bilanciamento o SQLite.

# RC76-HF02 — 05/11/2026 · ripristino contratto CityCatalog (candidata al momento della consegna)

- Corretti gli 8 errori CS1061 in `SimRisorse.Desktop`, `SimRisorse.Application.Tests` e `SimRisorse.Infrastructure.Tests`: dopo i refactoring v15 mancava l'esposizione pubblica di `V15FoodIndustryCampaignSession.CityCatalog`.
- Ripristinata la proprietà pubblica di tipo `V15CampaignCatalogView`, delegando a `V15CampaignCatalogProjection.Project(_baseline, _current, _catalog)`. La proiezione legge lo snapshot corrente, senza cache e senza alterare lo stato; l'elenco degli edifici e la tabella delle risorse v15 rimangono quelli già previsti.
- Nessuna modifica a XAML, ViewModel, produzione, costi, SQLite o API dei benchmark. Gli otto utilizzi già presenti in sorgenti/test costituiscono il controllo di compilazione del contratto; **nessun nuovo test aggiunto**. Totale atteso invariato: **869**.
- Gate F1 interrotto finora in fase di compilazione, non nei test o negli audit. Compilazione .NET 10 e test **non eseguiti qui**; [piano di validazione RC76-HF02](docs/validation/RC76_HF02_CITY_CATALOG_TEST_PLAN.md). Ultimo checkpoint effettivamente validato: RC63, 771 test.

# RC76-HF01 — 05/11/2026 · correzione compilazione E4a v15 (candidata)

- Corretti i due errori CS0117 dei runner `V15NaturalProgressionBenchmark` e `V15IndustrialProgressionBenchmark`: il metodo Core `BusyOutsideFood` era `internal` e non era utilizzabile da Application. Le due proiezioni ora richiamano `V15FoodCampaignCheckpoints.AvailableWorkers(checkpoint)`, API pubblica che conteggia anche le sei filiere alimentari.
- La sessione giocabile v15 usa la medesima API per `AvailableWorkers`, mantenendo una sola formula autoritativa. Nessun cambiamento a regole, formule produttive, salvataggi o UI.
- Due nuovi test di regressione sulla concordanza sessione/benchmark e sui **2 lavoratori liberi** nello scenario combinato da 24 abitanti: **869 test cumulativi attesi** (867 RC76 + 2); nessuna validazione .NET 10 eseguita in questo ambiente.
- `validate-f1.cmd` aggiornato a 869 test. Il messaggio generico `dotnet failed with exit code 1` era conseguenza degli errori di compilazione; il gate F1 resta aperto finché non passa su Windows e non viene confermata la GUI.
- [Piano di verifica](docs/validation/RC76_HF01_COMPILATION_TEST_PLAN.md). I risultati RC76 antecedenti a questa correzione rimangono documentati come storico.

# RC76 — 05/11/2026 · E4a.2c (candidata)

- Telemetria opzionale sui tentativi falliti: deficit materiali quotati (al millesimo), preservando le ricevute delle sole azioni riuscite.
- Campioni di scorte/capacità cittadine per Legname lavorato, Pietra, Carbone, Minerale di ferro, Ferro, Minerale d'oro e Oro.
- Nuovo `V15IndustrialConstraintAnalysis`: evidenze distinte tra mancanza misurata, codice di errore e saturazione osservata; output JSON/CSV/Markdown aggiuntivi dal runner e dall'analizzatore RC75; compatibilità di lettura con i JSON RC75.
- 10 test Application, totale cumulativo atteso **867**; gate F1 aggiornato, nessun test .NET 10 eseguito qui.
- Documentazione: ADR 0084, specifica, piano test, roadmap, stato, handoff. Nessuna modifica a GUI, Core, regole di gioco o SQLite; benchmark completi `not-run`.

## RC75 / E4a.2b — Benchmark industriale/tecnologico v15 (CANDIDATA, 05/11/2026)

- `V15IndustrialProgressionBenchmark` e fixture naturale: scenari I01 (Scuola/Estrazione I), I02 (Industria I e prime Spade), I03 (Industria I e prime Monete); due strategie dallo stesso checkpoint, nessun grant. Percorsi con Pane/Casa grande/Carbone e Matite (2 per studente), coerenti con la GUI v15; allineato anche il contratto di Scuola del runner RC73, senza cambiarne le ricette alimentari.
- Costi materiali effettivi completi per azione eseguita e punti ricerca acquistati separatamente; `V15IndustrialProgressionAnalysis` riporta classificazioni prudenti e confronta durate solo quando entrambe le strategie osservano il completamento senza orizzonti abbreviati.
- CLI per benchmark e analisi da JSON; risultati JSON/CSV/Markdown, protezione da sovrascritture; manifest iniziale in stato `not-run`.
- **14 nuovi test, 857 cumulativi attesi**, compilazione .NET 10 non eseguita; RC63 ultimo checkpoint validato con 771 test. Gate F1 aggiornato a 857, F1 ancora aperta.
- Nessuna modifica a GUI, Core, database, v14, prezzi, ricette o fabbisogno di lavoratori. [ADR 0083](docs/adr/0083-benchmark-industria-ricerca-v15-senza-grants.md) · [Specifica](docs/design/E4A_V15_RC75_INDUSTRIA_RICERCA.md) · [Piano test](docs/validation/E4A_V15_RC75_TEST_PLAN.md).

## RC74 / E4a.2a — Analisi naturale v15 senza alterare il gioco (CANDIDATA)

- Nuovo postprocessore `V15NaturalProgressionAnalysis` dei JSON RC73: classificazione dei risultati, tempi d'attesa e spese tracciate, controllo di coerenza dei dati.
- Nuova CLI `--analyze-e4a-v15-natural` con report JSON, CSV e Markdown e nessuna sovrascrittura dei file già presenti.
- Nove test xUnit nuovi; suite cumulativa attesa **843**. Aggiornato il gate F1 alla nuova dimensione.
- GUI, C# del motore economico, cataloghi, SQLite e runner RC73 invariati. Nessun test .NET o benchmark di progressione completo eseguito qui; RC63 (771 test) ultimo checkpoint confermato.
- [Specifiche RC74](docs/design/E4A_V15_RC74_ANALISI_PROGRESSI.md), [piano test](docs/validation/E4A_V15_RC74_TEST_PLAN.md), [ADR 0082](docs/adr/0082-analisi-basata-su-evidenze-benchmark-v15.md).

## v60-E3b4-RC73 — Runner E4a.1 naturale v15 (CANDIDATA, 05/11/2026)

- Nuovo `V15NaturalProgressionBenchmark` per avvii autentici v15 senza finanziamenti artificiali: tre scenari V15-N01/N02/N03, policy seriale versionata, eventi/campioni, timeout/blocked senza durate inventate.
- CLI separata `--benchmark-e4a-v15-natural` con JSON + due CSV e protezione dalla sovrascrittura; `--benchmark-e4a-natural` v14 invariato.
- Aggiunti 8 test Application; **834 test attesi** (826 RC72 + 8), non eseguiti qui. Aggiornato il default del gate F1 a 834, senza chiudere F1.
- GUI, motore, persistente SQLite, prezzi/ricette, strumenti v14 invariati. Manifest degli scenari v15 in stato `not-run`.
- [Specifica RC73](docs/design/E4A_V15_RC73_NATURAL_RUNNER.md) · [Piano test](docs/validation/E4A_V15_RC73_TEST_PLAN.md) · [ADR 0081](docs/adr/0081-runner-naturale-v15-separato-dai-benchmark-v14.md).

# RC72 — F1-D05e.3 · Gate finale riproducibile (05/11/2026)

- Quattro test di regressione cumulativi (3 Application, 1 Infrastructure): filiere combinate 120s, ledger al millesimo, clock frazionato/nullo, tre riprese SQLite generation 6. **826 test attesi** (RC71: 822); non eseguiti su .NET 10 in questo ambiente, RC63 771/771 resta l'ultimo checkpoint confermato.
- `validate-f1.cmd` e `scripts/validate-f1-gate.ps1`: eseguono `validate.cmd`, controllano TRX, smoke nativo Windows, screenshot, audit v15 base e combinato; producono `f1-gate.json` con esito ed evidenze. Stress combinato 24h opzionale. Nessun risultato è presunto prima dell'esecuzione.
- Riordinati README, stato corrente, roadmap e handoff; aggiunto ADR 0080, specifica RC72, piano test. Nessun cambiamento a C# produttivo, GUI/XAML, SQLite, prezzi, ricette o `validate.cmd`. F1-D05e resta aperta fino al gate cumulativo verde e alla verifica grafica.

---

## RC71 — 05/11/2026 — F1-D05e.2b.2b.2f, planner senza slot legacy (candidata)

- `ProductionPlanner.Plan` salta i produttori espressamente soppressi **prima** di cercarne il `BuildingState`; se assente restituisce il report disabilitato senza introdurre uno stato fittizio. I produttori attivi continuano a richiedere la propria riga.
- `V15UnifiedCampaignClock` non crea più `BuildingState(food-store,0,0)`; nessun cambiamento a ricette, flussi, costi o SQLite generation 6.
- +4 test Application per filiere combinate senza slot, importazioni legacy occupate ma non operative, frazioni di secondo, rigetto di portfolio incompleti: **822 test cumulativi attesi**, non eseguiti. RC63 771/771 ultimo checkpoint validato.
- Nessuna modifica a Desktop/XAML, al motore delle sessioni v14 o ai formati di salvataggio. [ADR 0079](docs/adr/0079-planner-v15-senza-slot-produttore-ritirato.md) · [Specifica](docs/design/F1_D05E2B2B2F_RC71_PLANNER_SENZA_SLOT.md) · [Piano test](docs/validation/F1_D05E2B2B2F_RC71_TEST_PLAN.md).

## RC70 — 05/11/2026 — F1-D05e.2b.2b.2e, audit persistenza (candidata)

- `V15SqliteFoodSaveStore.Save`: confronto semantico completo fra checkpoint canonico e file SQLite temporaneo riletto, **prima** di backup e promozione. Campi scalari, progresso individuale, risorse e inventari per proprietario verificati; collezioni indicizzate confrontate per ID.
- `V15CityStorage.Calculate`: capacità Matite calcolata direttamente da città v15, con identica formula storica per Municipio/Magazzini/Scuola ma senza bridge `ToInherited()`.
- +4 test xUnit (2 Application, 2 Infrastructure), **818 cumulativi attesi**; non eseguiti. RC63 (771) ultimo checkpoint con test confermati.
- Nessun file Desktop/XAML, motore v14 o schema SQLite modificato. Generation 6 invariata. [ADR 0078](docs/adr/0078-verifica-semantica-salvataggio-v15.md) · [Specifica](docs/design/F1_D05E2B2B2E_RC70_AUDIT_SQLITE.md) · [Piano test](docs/validation/F1_D05E2B2B2E_RC70_TEST_PLAN.md).

## RC69 — 05/11/2026 — F1-D05e.2b.2b.2d, checkpoint civico v15 nativo (candidata)

- `V15FoodCampaignCheckpoint.CityCampaign` è ora `V15CityCampaignSnapshot` come unico stato civico. `NativeCityCampaign` diventa alias di lettura dello stesso oggetto.
- Comandi civici operano su tipi di famiglie e contatori v15; validazione e clock non ricostruiscono snapshot v14. `food-store` resta soltanto come slot temporaneo del `ProductionPlanner` (RC71) o nell'importazione storica esplicita.
- `V15SqliteFoodSaveStore` carica direttamente i DTO v15, senza passaggi attraverso `V14CampaignSnapshot`; resta **SQLite generation 6** (stesso schema). L'eventuale famiglia legacy vuota viene omessa prima di salvare, quella occupata resta rifiutata.
- 8 nuovi test xUnit (6 Application, 2 Infrastructure); **814 cumulativi attesi**, non eseguiti. Ultimo gate validato dall'utente: RC63 (771).
- Desktop/XAML, sistema economico, ricette, costi, schemi SQLite e gioco v14 invariati.
- [ADR 0077](docs/adr/0077-radice-civica-v15-nativa.md) · [Specifica](docs/design/F1_D05E2B2B2D_RC69_RADICE_CHECKPOINT_NATIVE.md) · [Piano test](docs/validation/F1_D05E2B2B2D_RC69_TEST_PLAN.md).

## RC68-DOC01 — 05/11/2026 — Riordino documentale e pianificazione dettagliata (solo documenti)

- Nessuna modifica al codice RC68, ai test, alla GUI, agli asset o allo schema SQLite generation 6. L'ultima RC confermata rimane RC63 (771/771); RC68 è candidata con 806 test attesi da Windows.
- Consolidati README, START_HERE, indice documentale, CURRENT_STATE, ROADMAP, NEW_CHAT_HANDOFF, registro delle decisioni, catalogo, game design e contratto salvataggi. `docs/HANDOFF.md` ridotto a rimando per retrocompatibilità dei link.
- Roadmap sequenziale con gate G0, refactoring nativo RC69–72, benchmark v15 E4a, proposta costi/squadre E4b e sviluppo M5.4–M7; esplicitate dipendenze, criteri di uscita, rischi e vincoli GUI.
- Preservati ADR 0001–0076, specifiche, test, storia e dossier; vecchi archivi esclusi dalla navigazione corrente ma non eliminati senza audit storico. Nuovo piano di pulizia in [RC68-DOC01](docs/validation/RC68_DOC01_DOCUMENTATION_AUDIT.md).

## RC68 — 05/11/2026 — F1-D05e.2b.2b.2c

- Introdotti i record nativi v15 `V15ProductiveFamilySnapshot` e `V15PurchaseCounter` per portafogli per tier e storicità degli acquisti. `V15CityCampaignSnapshot` usa i nuovi tipi; adattamento 1:1 solo su `FromInherited`/`ToInherited`.
- Rifiutate le collezioni non inizializzate durante le conversioni. Preservati identità delle famiglie, quantità, personale, contatori inclusa la storia `food-store` e ordine degli elementi.
- Adeguati due confronti di test RC67 che equiparavano erroneamente la backing-array identity al contenuto degli `ImmutableArray`; nove nuovi test RC68. **806 test cumulativi attesi**, .NET 10 non eseguito; **RC63** ultima versione validata 771/771.
- Nessuna modifica a GUI, SQLite generation 6, formule produttive, risorse, costi, progressione o alla partita v14. Consumer persistenti e slot del planner ancora in roadmap.
- [ADR 0076](docs/adr/0076-portafogli-contatori-v15-nativi.md) · [Specifica](docs/design/F1_D05E2B2B2C_RC68_PORTAFOGLI_CONTATORI_V15.md) · [Piano test](docs/validation/F1_D05E2B2B2C_RC68_TEST_PLAN.md).

## RC67 — 05/11/2026 — F1-D05e.2b.2b.2b, contratto civico v15 nativo (incremento)

- Nuovo record `V15CityCampaignSnapshot` senza header di versione v14. Conversione `FromInherited` stretta e `ToInherited` senza trasformazioni economiche.
- `V15CityCampaignValidator.ValidateNative` e `V15FoodCampaignCheckpoint.NativeCityCampaign`: validazione sul modello v15 senza introdurre `food-store` artificiale.
- `V15CityStorage.Calculate` accetta anche il nuovo tipo, mantenendo una sola fonte per la capacità delle Matite.
- Sei nuovi test Application; **797 cumulativi attesi**, SDK .NET 10 non disponibile qui. RC63 resta ultima RC validata, 771 test. GUI/SQLite generation 6/costi/ricette invariati.
- Residui dichiarati: DTO primario `V14CampaignSnapshot`, contatore storico `food-store`, slot locale del planner. [ADR 0075](docs/adr/0075-contratto-civico-v15-proiezione-nativa.md) · [Specifica](docs/design/F1_D05E2B2B2B_RC67_CONTRATTO_CIVICO_V15.md) · [Test](docs/validation/F1_D05E2B2B2B_RC67_TEST_PLAN.md).

## RC66 — 05/11/2026 — F1-D05e.2b.2b.2a, validazione civica v15 indipendente

- Introduzione di `V15CityCampaignValidator` in Core/Food; elimina la proiezione sintetica `food-store` dal percorso ordinario di validazione `V15FoodCampaignCheckpoints.Validate`.
- Validazione dei 15 produttori, tier, tecnologie, cronologia degli acquisti, Municipio, Scuola, militari, popolazione e capienza fisica della città v15, senza ritoccare il modello economico.
- Importazioni con `food-store` ancora occupato verificabili prima del ritiro esplicito, ma SQLite generation 6 continua a rifiutarne il salvataggio.
- 7 nuovi test Application: **791 totali attesi**; RC63 confermata a 771, RC64/65/66 ancora da compilare/validare su Windows.
- Nessuna modifica Desktop/XAML, store SQLite, ricette, costi, salvataggi v14; DTO civico e contatore legacy rimandati ad incremento successivo. [ADR 0074](docs/adr/0074-validatore-civico-v15-senza-proiezione-v14.md).

# RC65 — 2026-11-05 — F1-D05e.2b.2b.1 (candidata)

- **Refactoring funzionale:** eliminate le invocazioni a `V14CampaignSession.Restore` dalla superficie dei comandi civici v15; nove entry point pubblici operano direttamente su `V15FoodCampaignCheckpoint` attraverso la nuova classe parziale `V15NativeCityCommands.cs`.
- Un solo catalogo di famiglie produttive valido per la sessione, filtrando `food-store` prima di costruzioni/upgrade/personale.
- Riuso di `TieredConstruction`, `ProgressiveConstructionPricing`, `TechnologyPurchases`, `V14SchoolSupplies`, `V15CityStorage` senza riscriverne costi o formule. Risultati atomici e stessi codici errore; risorse scuola sempre fisicamente separate dal Cibo del Deposito.
- Test nuovi: 6 Application (`F1D05e2b2bNativeCityCommandTests`) e 1 Infrastructure (`F1D05e2b2bNativeCityPersistenceTests`). **784 attesi** a partire da RC64 777; non validati in .NET in questo ambiente.
- Nessuna modifica all'interfaccia Desktop, XAML, v14, al formato SQLite generation 6 o ai coefficienti economici. Validazione `V14CampaignSnapshot` e contatore legacy rimangono per l'incremento seguente.
- Documentati ADR 0073, specifica RC65, piano di validazione, roadmap, stato e handoff.

# RC64 — F1-D05e.2b.2a · Stato v15 senza lo slot produttivo `food-store` (05/11/2026)

**Baseline verificata dall'utente:** RC63, **771/771 test superati**; aspetto della GUI v14 ripristinato nella v15 e giudicato conforme. **RC64:** +6 test (5 Application, 1 Infrastructure), **777 attesi**, compilazione .NET 10 non eseguibile in questo ambiente. Non descrivere RC64 come già validata.

**Obiettivo incrementale:** il normale checkpoint v15 conserva **15 famiglie produttive attive**, senza ricostruire stabilmente la sedicesima `food-store` dopo una nuova partita, un comando civico o la rilettura SQLite. Una proiezione *temporanea* a 16 famiglie rimane confinata all'uso del validatore e dei comandi civici v14 ancora riutilizzati. Il clock aggiunge localmente un produttore disabilitato a zero lavoratori solo per soddisfare il catalogo storico del `ProductionPlanner`; questa riga **non** torna nello stato v15. La generazione SQLite rimane **6**, senza migrazioni né modifica del percorso di salvataggio. Il contatore storico `food-store` è conservato finché non riscriviamo i comandi nativi v15.

**Protezione dello stato:** un `food-store` ereditato con edifici/lavoratori occupati non viene scartato automaticamente: richiede `RetireLegacyFoodStore` prima di poter salvare; il ritiro non rimborsa costi né altera scorte. Nessuna modifica a formule, ricette, prezzi, capacità, tecnologie, GUI storica, XAML o salvataggi v14.

**Test da eseguire su Windows:** ` .\validate.cmd ` (**777 test attesi**). Poi ` .\run-game.cmd ` (v15 predefinita) e confronto visuale con ` .\run-game.cmd --v14 `; facoltativo `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined` e stress 24 ore. Confermare che la v15 mostri Stalla, Cacciatore, Macellaio, Orto, Frutteto, Deposito e mantenga tutti i vincoli visivi v14 della RC63.

**Resta F1-D05e.2b.2b:** eliminare anche l'adattatore `V14CampaignSession`, rendere nativi i comandi civici e la struttura del checkpoint, rimuovere il contatore storico e sostituire la dipendenza di validazione da `V14CampaignSnapshot`. **Poi E4a** (benchmark economici), **E4b** (costi quasi esponenziali e almeno 10 addetti minimi per ciascuna Zecca operativa). Non anticipare modifiche di GUI o coefficenti economici.

[ADR 0072](docs/adr/0072-checkpoint-v15-famiglie-attive-senza-slot-legacy.md) · [Specifica](docs/design/F1_D05E2B2A_RC64_PORTFOLIO_V15.md) · [Test](docs/validation/F1_D05E2B2A_RC64_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

---

# RC63 — F1-D05e · Ripristino contratto grafico v14 nella GUI v15 (5 novembre 2026)

**Motivo:** la RC62 ha reso v15 predefinita ma il pannello grafico v15 era una semplificazione non autorizzata delle scelte già validate in v14. È una regressione di prodotto, non un'occasione per ridisegnare la GUI.

**Obiettivo RC63:** la v14 è il riferimento **vincolante** per layout e interazioni. Le sei nuove filiere sono estensioni del catalogo e del simulatore, NON un nuovo design. Nessuna modifica ai coefficienti economici, al clock, alla persistenza SQLite generation 6 o alla partita v14.

- Edifici: lista con colonne **STRUTTURA / EDIFICI / PERSONALE**, icona 24×24 in riquadro, righe bloccate attenuate (`Opacity=0.48`) ma sempre selezionabili. Nessun tasto Costruisci per strutture bloccate; prerequisiti leggibili in dettaglio.
- Dettaglio: descrizione, costi del prossimo edificio come elenco di icone, prezzi cumulativi +5/+10, prerequisiti, **produce / consuma / capacità** per risorsa con PixelGlyph e allineamenti della v14; diagnosi limitata e scorrevole.
- Comandi: due righe **Costruisci +1 / +5 / +10** e **Lavoratore −1, +1 / +5 / +10**, pulsanti di upgrade in fondo al box come v14. Per le nuove filiere, le ricette alternative di Macellaio e Deposito sono esplicitamente indicate come alternative, non simultanee.
- Sidebar: tornano **CRESCITA**, progress bar, fabbisogni Cibo/Pane, posti abitativi e abitanti disponibili. Proiezione v15 sola lettura che usa il `PopulationPlanner` del motore e i medesimi coefficienti di housing.
- Risorse: resta la tabella a sette colonne della v14 (quantità arrotondata, saldo colorato, hover della capienza, selezione stabile); cambiano soltanto i valori, derivati dal clock v15.
- Esercito e Scuola: ripristinati layout di dettaglio, posti, progress bar individuali, materiali, selector quantitativi 1/5/10 e relative operazioni v15. Congedo rinviato come nella v14; tecnologie a livello diretto (no ComboBox nel dettaglio edifici).
- Filtro "Filiere alimentari (6)" resta un'aggiunta localizzata nell'intestazione della lista, non sostituisce l'elenco generale. È ancora possibile mostrare tutti gli edifici.
- Le differenze v14/v15 sono contrattualizzate tramite **9 nuovi test Desktop di regressione**. **Totale atteso RC63: 771 test** (RC62 762 previsti + 9; RC62 non ancora confermata integralmente su Windows).

**Validazione in questo ambiente:** XML/XAML e controlli statici verificati; compilazione/Headless xUnit **non eseguibili qui** (SDK .NET 10 assente). Eseguire `./validate.cmd` su Windows; risolvere eventuali errori prima di proseguire con D05e.2b.2. Nessuna affermazione che i 771 test siano già verdi.

**Prossimo gate:** RC63 build + 771 test, controllo visivo delle cinque sezioni e dei due stati bloccato/disponibile a 1040×740 e 1240×850. Poi completamento disaccoppiamento del checkpoint v15 (F1-D05e.2b.2), benchmark E4a, progressione E4b (costi e squadre minime).

---

# RC62 — F1-D05e.2b.1-fix · 05/11/2026

- Ricevuto il risultato RC61: compilazione C# OK, 760/761 test superati, **1 test Infrastructure fallito** per `user_version`: expected 5 / actual 6 (test `SchemaFiveRoundTripPreservesEveryIntermediateMilliunitAndSixBuildingCounts`).
- Aggiornato il test allo **schema 6** (entrambi `PRAGMA user_version` e `game_save.snapshot_version`), rinominato e aggiornati i percorsi temporanei/backup; nessuna modifica al motore né allo store SQLite.
- **GUI v15 predefinita** per l'avvio normale, vecchia GUI **v14 esplicita** (`--v14`) con salvataggio personale separato. `--v15`, v13/legacy e smoke-test continuano a funzionare.
- Aggiunto filtro **Filiere alimentari (6)** nella lista Edifici v15, con ritorno a **Tutti gli edifici**; nessuna modifica a catalogo, prerequisiti, costi, clock, layout Risorse a sette colonne.
- Aggiornati i test Desktop d'avvio, aggiunto un caso per filtro/ripristino (`+1`). **762 test cumulativi attesi**, non eseguiti in questo ambiente.
- Nuovo ADR 0070, specifica tecnica, piano dei test, roadmap, stato, handoff e README aggiornati. La rottura dei soli DB sperimentali generation 5 rimane esplicita. Base validata RC60 (756/756); RC61 760/761.

---

# RC61 — F1-D05e.2b.1 — SQLite v15 gen. 6 senza famiglia legacy persistita (5/11/2026)

- **RC60 confermata: 756/756 test.** `V15FoodCampaignCheckpoints.FileVersion=6`, store SQLite `user_version=6` e `snapshot_version=6`.
- La tabella `progression_family` salva **45 righe** per le 15 famiglie v15 attive; `food-store` non viene persistito ed è vietato dal `CHECK` SQL. Il solo `V15LegacyProducerAdapter` lo ricrea vuoto **in RAM** per i comandi/validatori storici. Restano temporaneamente il contatore `progression_acquisitions` e la dipendenza dello snapshot civico dal v14.
- Persistenza rifiutata se lo slot legacy ha ancora edifici o lavoratori; ritiro esplicito necessario, nessuna perdita implicita. La vecchia generation 5 è rifiutata senza sovrascrittura, in linea con la decisione di tagliare i DB sperimentali.
- **5 test Infrastructure nuovi**, totale **761 attesi**. SQLite DDL verificato staticamente, SDK .NET 10 e xUnit da verificare su Windows. Invariati motore del clock, inventari, UI v14/v15, costi e ricette.
- [Specifica](docs/design/F1_D05E2B1_RC61_FORMATO6_PERSISTENZA.md) · [Piano test](docs/validation/F1_D05E2B1_RC61_TEST_PLAN.md) · [ADR](docs/adr/0069-generazione6-senza-famiglia-produttiva-legacy.md) · [Roadmap](docs/ROADMAP.md).

---

# RC60 — F1-D05e.2a-fix · Correzione budget occupazionale fixture combinata (05/11/2026)

- **Gate RC59 reale (Windows/.NET 10):** 755 test, **754 riusciti, 1 fallito** (`FixtureHasEveryNewSiteStaffedWithoutExceedingTheSharedPopulationBudget`, atteso 5 contro effettivo 2). Infrastructure e Desktop riusciti. Il messaggio conclusivo `dotnet failed with exit code 1` deriva dal test fallito, non da un errore di sintassi/compilazione C#.
- **Causa:** `CivilGrowthScenario.Create()` porta tre addetti già occupati (Taglialegna, Segheria, Tagliapietra). La fixture aggiunge 13 occupati alimentari storici e 6 nuovi, per **22 lavoratori su 24**, **2 disponibili**. Il vecchio test dimenticava il personale iniziale; il motore calcolava correttamente.
- **Correzione:** atteso aggiornato a 2; nuova regressione sul budget completo, compresi impieghi storici, nuovi, civici, Scuola e militari. Aggiornato il commento della fixture e rettificate le cifre RC59 nelle specifiche, nel piano di test e nell'ADR 0068.
- **Totale atteso RC60: 756 test (+1).** .NET 10 non disponibile in questo ambiente; validazione da eseguire su Windows. Nessun cambiamento alle regole economiche, allo stato delle partite, alla GUI o allo schema SQLite generation 5.

[Analisi](docs/design/F1_D05E2A_RC60_FIX_PERSONALE.md) · [Piano RC60](docs/validation/F1_D05E2A_RC60_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).
---

# RC59 — F1-D05e.2a · Stress simultaneo delle filiere (05/11/2026)

- **Gate precedente superato:** RC58, **744 test** verdi comunicati dall’utente; audit v15 standard 24 h: 86.400/86.400 secondi, 0 pasti mancati, 0 squilibri, 0 produzione legacy.
- **Nuovo:** `V15CombinedFoodStressScenario` in Application, 24 abitanti e 22 occupati (3 iniziali, 13 filiera storica, 6 nuove filiere; rettificato in RC60), scorte iniziali dichiaratamente sintetiche.
- **Nuovo:** `V15CombinedStressAudit` e `--audit-v15 ... --profile combined`: riconcilia ogni secondo e rende il cumulato della produzione di Bovini, Selvaggina, Carne, Verdura, Frutta, Pane e Cibo; eccezione esplicita in caso di squilibrio.
- **Test:** 9 casi Application (4 Fact + Theory a 5 dati) e 2 Infrastructure = **+11**, **755 attesi**. Nessun test .NET eseguito durante la pacchettizzazione.
- **Immutati:** GUI pixel art v14 e v15, schema SQLite, clock e regole, migrazioni, costi, cartelle dei salvataggi.
- **Prossima:** F1-D05e.2b, scomposizione del checkpoint v15 dallo slot strutturale `food-store`; E4a benchmark e E4b costi/organici restano successivi.

[Specifica](docs/design/F1_D05E2A_RC59_STRESS_FILIERE.md) · [Test](docs/validation/F1_D05E2A_RC59_TEST_PLAN.md) · [ADR](docs/adr/0068-stress-combinato-filiere-e-checkpoint-legacy.md).

---

# RC58 / F1-D05e.1-fix — 5 novembre 2026 — Confronto logico del checkpoint SQLite

- **Evidenza RC57 su Windows:** 743 casi eseguiti; **741 superati e 2 falliti** nella suite Infrastructure, mentre la suite Desktop è verde. Il messaggio finale `dotnet failed with exit code 1` è conseguenza dei test falliti, **non** di errori del compilatore C#.
- I due fallimenti si interrompono entrambi nel medesimo `AssertSame` alla riga 77: `V15SqliteFoodSaveStore.ReadFile` restituisce `OtherBuildings` in `ORDER BY building_id`, mentre lo stato iniziale usa l'ordine del catalogo. Stessi `BuildingId`, `Count` e `AssignedWorkers`, ordine differente.
- **Correzione circoscritta ai test:** `AssertSame` confronta in ordine canonico ordinal per ID `OtherBuildings`, `ProductiveFamilies`, `PurchaseCounters` e `UnlockedTechnologyIds`; i progressi individuali di ricerca e militari mantengono il confronto **posizionale**. Nessuna modifica a motore, schema SQL, filiere, salvataggi o GUI.
- Aggiunto 1 test mirato `KeyedCampaignCollectionsCanBeReorderedWithoutChangingCheckpointMeaning`: **744 casi attesi** (743 RC57 + 1). Nessuna compilazione o esecuzione .NET 10 eseguita in fase di confezionamento; l'ultimo gate integralmente verde resta **RC56: 731/731**.
- [Analisi e modifica](docs/design/F1_D05E1_RC58_FIX_ORDINE_CHECKPOINT.md) · [Piano di validazione](docs/validation/F1_D05E1_RC58_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md) · [ADR 0067](docs/adr/0067-riconciliazione-fisica-v15-e-stress-separato.md).

---

# RC57 / F1-D05e.1 — 5 novembre 2026 — Riconciliazione fisica e stress v15 (candidata)

- **RC56 validata dall’utente: 731 test superati.** Nuovo `V15StockAuditor` puro per ricostruire i totali città, recinti, transito, deposito grezzi e Cibo pronto e controllare la conservazione al millesimo dal ledger reale del clock.
- Aggiunti **12 casi xUnit** (10 Application contando 2 casi parametrizzati + 2 Infrastructure), **743 attesi**, non eseguiti qui senza .NET 10. Coprono trasferimenti, saturazione, vecchio `food-store` inerte, sessioni spezzate e salvataggi SQLite ripetuti.
- Nuovo stress opzionale in `tools/SimRisorse.Simulation`: `--audit-v15 --seconds N` o `--hours N` (fino a 48 h), senza accesso al DB dell’utente. Il comando da 24h è predisposto, **non eseguito** nel confezionamento e non entra in `validate.cmd`.
- Nessuna variazione di GUI v14/v15, ricette, costi, risorse, salvataggi o formato SQLite. Slot legacy `food-store` ancora nello stato ereditato; pulizia fisica rimandata a D05e.2.
- Documenti: [Specifica](docs/design/F1_D05E1_RC57_RICONCILIAZIONE_STRESS.md), [Piano test](docs/validation/F1_D05E1_RC57_TEST_PLAN.md), [ADR 0067](docs/adr/0067-riconciliazione-fisica-v15-e-stress-separato.md), [Roadmap](docs/ROADMAP.md), [Handoff](docs/NEW_CHAT_HANDOFF.md).

---

# RC56 / F1-D05d.4b.2 — 5 novembre 2026 — Flussi reali e ETA nel pannello Risorse v15

- **Gate precedente:** RC55 confermata dall'utente con **713 test passati**.
- `V15UnifiedCampaignClock.Plan` registra produzione e consumo del primo secondo presso i veri planner (legacy, Stalla/Cacciatore/Macellaio, Orto/Frutteto, deposito, pasto atomico). `V15EconomyProjector` produce una proiezione pura del prossimo secondo, senza avanzare la partita o scrivere SQLite.
- Il ricevimento di ingredienti dalla città/transito al deposito è un **trasferimento**, non produzione né consumo. Il vecchio `food-store` rimane escluso; riconciliazione al millesimo tra saldi e inventari.
- `V15CampaignCatalogProjection` include il dettaglio della capacità per proprietario, visualizzato su hover nella GUI. `V15ResourceRow` ora espone Produzione/s, Consumo/s, Saldo/s con segno e ETA al saldo locale, mantenendo l'originale tabella a **sette colonne**.
- **+18 test nuovi**: 9 Application e 9 Desktop (di cui 6 varianti ETA), totale **731 attesi**, .NET 10 non disponibile nell'ambiente di confezionamento. Verificare con `validate.cmd` su Windows.
- Nessuna modifica alla GUI v14, ai prezzi, alle ricette, allo schema SQLite 5 o ai percorsi di salvataggio. Prossimo: F1-D05e consolidamento e stress; poi E4a benchmark, E4b progressione quasi esponenziale e Zecca 10 addetti/unità.
- [Specifica](docs/design/F1_D05D4B2_RC56_FLUSSI_ETA.md) · [Test](docs/validation/F1_D05D4B2_RC56_TEST_PLAN.md) · [ADR 0066](docs/adr/0066-flussi-v15-misurati-dal-clock-e-eta-locale.md).

# RC55 / F1-D05d.4b.1-fix — 5 novembre 2026 — Correzione compilazione Desktop v15

- **Segnalazione reale Windows RC54:** `SimRisorse.Desktop` non compilava per **2 errori CS0103** in `V15PreviewViewModel.cs` (righe 382 e 399): `GameSaveException` non risolto. Nessun esito xUnit completo RC54 disponibile, poiché la build si è interrotta.
- **Causa e fix:** mancava `using SimRisorse.Application.Persistence;`, namespace pubblico in cui è definita `GameSaveException`. Aggiunto l'import, già utilizzato da `V14PreviewViewModel` e dagli store SQLite.
- Nessuna modifica a catch, persistenza, sessione, XAML, motore, test o salvataggi. Il test `CorruptV15CheckpointBlocksAutosaveInsteadOfBeingReplaced` rimane la regressione esistente sul percorso di errore.
- **0 test aggiunti; 713 attesi.** Test e compilazione RC55 non eseguiti nell'ambiente di consegna senza SDK .NET 10. RC53 rimane l'ultimo checkpoint verde (**705/705**). Ripetere `./validate.cmd` su Windows.
- [Piano RC55](docs/validation/F1_D05D4B1_RC55_FIX_BUILD_TEST_PLAN.md) · [Stato](docs/CURRENT_STATE.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md) · [ADR 0065, errata](docs/adr/0065-gui-v15-opt-in-e-salvataggio-isolato.md).

# RC54 / F1-D05d.4b.1 — 5 novembre 2026 — GUI v15 opt-in (candidata)

- **RC53 validata dall'utente: 705 test superati.** Nuova opzione `--v15` con slot separato `experiments/civil-growth-v15.sqlite3`. Modalità v14 classica resta default.
- Nuove `V15PreviewViewModel` / `V15PreviewWindow`: Core v15 collegato ad Avalonia (clock, Edifici +1/+5, personale, avanzamento tier, Scuola/Matite/ricercatori, ricerca, esercito, salvataggio/caricamento, nuova colonia con conferma). Layout pixel art storico, 5 schede e **Risorse a 7 colonne**; costruzione non mostrata con prerequisiti bloccati.
- Aggiunti 11 sprite 8×8 per siti e risorse intermedi. Proiezione Risorse senza doppio conteggio; **flussi lordi e ETA ancora «—», da realizzare in 4b.2**. Nessun cambio a v14 UI, schema SQLite o economy E4b.
- +8 test Desktop (`V15DesktopIntegrationTests`), totale **713 attesi**; non è stato possibile eseguire .NET 10 qui. Controllare compilazione, test e QA manuale sotto Windows.
- [Specifica](docs/design/F1_D05D4B1_RC54_GUI_V15.md) · [Test](docs/validation/F1_D05D4B1_RC54_TEST_PLAN.md) · [ADR 0065](docs/adr/0065-gui-v15-opt-in-e-salvataggio-isolato.md).

# RC53 / F1-D05d.4a-fix — 5 novembre 2026 — Correzione degli 8 errori RC52 (candidata)

- **Evidenza reale RC52:** Windows .NET 10 ha eseguito **703 test: 695 riusciti, 8 falliti, 0 ignorati**; suite Desktop riuscita. Il wrapper `validate.ps1` ha restituito exit code 1 per i test falliti, non sono riportati errori C# dal compilatore.
- **5 fixture difettose** (4 Application, 1 Infrastructure) facevano risultare `fisher`, `farm` e `well` assenti perché modificavano `OtherBuildings` anziché `ProductiveFamilies`; corretto anche il controllo dell'assenza del vecchio `food-store` e il prerequisito del Deposito.
- **1 fixture di personale non valida all'origine:** gli addetti Hunter erano già più delle persone disponibili. Il test ora costruisce uno stato valido che satura il budget e verifica il rifiuto di un ulteriore lavoratore.
- **2 fallimenti condivisi di validazione**: il controllo dello stock v15 respingeva Pesce/Pane prodotti nella città e non ancora consegnati al Deposito. Ammessi i due inventari fisici (città e raw del Deposito); Cibo pronto resta **solo nel Deposito** e Bovini/Selvaggina/Carne/Frutta/Verdura restano nei rispettivi comparti.
- Il vecchio test SQLite che vietava anche il Pesce in città ora controlla correttamente il **Cibo pronto**. **+2 test nuovi**: somma risorse fra contenitori e round-trip SQLite al millesimo. **705 attesi, non ancora validati su Windows**.
- Nessun cambiamento alla GUI v14, ricette, coefficienti economici, schema SQLite generation 5 o sessione giocabile. [Rapporto](docs/design/F1_D05D4A_RC53_FIX_REGRESSIONI.md) · [Piano test](docs/validation/F1_D05D4A_RC53_REGRESSIONI_TEST_PLAN.md) · [ADR 0064](docs/adr/0064-ingredienti-in-transito-citta-deposito-e-fixture-produttive.md).

# RC52 / F1-D05d.4a — 5 novembre 2026 — Catalogo e comandi cittadini v15 (candidata)

- `V15FoodIndustryCampaignSession` ora espone comandi headless di costruzione civile, produttori Base/Grande/Avanzato, lavoratori, Scuola, Matite, ricercatori, ricerca e militari, senza mai avviare il vecchio clock. La popolazione è condivisa con i sei siti alimentari.
- La **Scuola v15** richiede Casa piccola e **nuovo Deposito**; iscrizioni e rimborsi addebitano esclusivamente `Depot.ReadyFood`, con capienza fisica verificata. Matite e punti ricerca rimangono nella città; nessun doppio Cibo.
- `V15CampaignCatalogProjection.CityCatalog` produce voci edificio con tier diretti e risorse aggregate fra città, animali, transito e Deposito; `food-store` è escluso da catalogo, costruzione e personale v15 e ritirabile con comando esplicito. Rimane lo slot storico nel checkpoint interno e nel tree legacy.
- `V15CityStorage.Calculate` centralizza le capacità città e risolve gli stessi valori nel clock, nei rimborsi Scuola e nella proiezione. Il salvataggio sperimentale resta SQLite generation 5; **GUI v14 e salvataggio personale invariati**.
- Aggiunti **20 nuovi test `[Fact]`** (18 Application + 2 Infrastructure), **non eseguiti**. RC47 620 confermati; RC48 +11, RC49 +17, RC50 +19, RC51 +16, RC52 +20: **703 indicativi**. L'SDK .NET 10 non è disponibile qui.
- Documentazione: [specifica](docs/design/F1_D05D4A_RC52_COMANDI_CATALOGO_V15.md), [test](docs/validation/F1_D05D4A_RC52_TEST_PLAN.md), [ADR 0063](docs/adr/0063-comandi-v15-cibo-esclusivo-e-catalogo-di-presentazione.md), [stato](docs/CURRENT_STATE.md), [roadmap](docs/ROADMAP.md), [handoff](docs/NEW_CHAT_HANDOFF.md).

# RC51 / F1-D05d.3b — 5 novembre 2026 — Clock unico alimentare v15 (candidata)

- Nuovo `V15UnifiedCampaignClock.Plan`: produzione dei vecchi edifici (solo `food-store` escluso), nuove filiere e unico Deposito produttore di Cibo, pasti atomici Pane/Cibo, crescita, addestramento individuale e Scuola/ricerca nello **stesso secondo di gioco**. Il commit avviene solo dopo validazione di tutto il checkpoint.
- `V15FoodIndustryCampaignSession` acquisisce `Pause`, `Resume`, `Advance`, limite 24h e accumulo dei millisecondi. `ProductionPlanner.Plan` aggiunge una esclusione **opzionale** dei produttori, senza modificare il clock v14 storico. `StepFood` resta seam diagnostico, non da invocare insieme a `Advance`.
- **16 nuovi test `[Fact]`** (14 Application + 2 Infrastructure) su doppia produzione, razioni, Pane avanzato, crescita, Scuola, formazione individuale, tick frazionari, continuità dei salvataggi SQLite. RC47 620 confermati; RC48 +11, RC49 +17, RC50 +19, RC51 +16 **non ancora eseguiti**: **683 test indicativi**. SDK .NET 10 non disponibile in questo ambiente.
- SQLite v15 `user_version=5` **invariato**. Nessun file Avalonia/GUI v14 modificato; `food-store` storico compare ancora nel checkpoint/catalogo ed è inattivo **solo** nel clock v15. La UI v15 non è ancora integrata. Nessuna modifica E4b/Zecca.
- Documenti aggiornati: [specifica](docs/design/F1_D05D3B_RC51_CLOCK_UNIFICATO.md), [test](docs/validation/F1_D05D3B_RC51_TEST_PLAN.md), [ADR 0062](docs/adr/0062-clock-unico-v15-e-produttore-cibo-esclusivo.md), [stato](docs/CURRENT_STATE.md), [roadmap](docs/ROADMAP.md), [handoff](docs/NEW_CHAT_HANDOFF.md).

# RC50 / F1-D05d.3a — 5 novembre 2026 — Catalogo edifici alimentari e personale (candidata)

- **Nuovi componenti Core**, senza alterare codice precedente: `FoodIndustryConstructionCatalog` con Stalla, Cacciatore, Macellaio, Orto, Frutteto, Deposito direttamente costruibili, prerequisiti e costi progressivi al 15% candidati. 1 posto per edificio base, capacità derivate dai conteggi (Bovini/Selvaggina, carne/frutta/verdura in transito, materie prime e Cibo pronto). Un solo `food-depot` nel nuovo catalogo, non il legacy `food-store`.
- `V15FoodIndustryCampaignSession`: `CheckBuild`, `Build`, assegnazione/rilascio lavoratori con budget condiviso per tutte le occupazioni, `Capacities`, `StepFood` esplicito con capienze dei veri edifici. Azioni atomiche, nessun acquisto o staffing parziale; `Snapshot` compatibile con SQLite generation 5 RC49.
- **19 nuovi test `[Fact]`** (17 Application, 2 Infrastructure) su catalogo, acquisti, costi, staff, capacità, due tick e persistenza. RC47 620 confermati; RC48 +11, RC49 +17, RC50 +19 **non eseguiti**: 667 indicativi. SDK .NET 10 non disponibile; nessun test/build RC50 dichiarato passato.
- **Nessuna modifica al gioco v14 o XAML/GUI**. Il `food-store` storico esiste ancora: serve F1-D05d.3b per unificare clock, crescita e produzione del Cibo prima dell'attivazione della nuova GUI. Nessuna modifica a Zecca o coefficiente E4b.
- Documentazione: [Specifica RC50](docs/design/F1_D05D3A_RC50_COSTRUZIONI_FILIERA.md), [test plan](docs/validation/F1_D05D3A_RC50_TEST_PLAN.md), [ADR 0061](docs/adr/0061-edifici-alimentari-diretti-e-capacita-derivate.md), [stato](docs/CURRENT_STATE.md), [roadmap](docs/ROADMAP.md), [handoff](docs/NEW_CHAT_HANDOFF.md). Compatibilità vecchi database di sviluppo **non obbligatoria**.

---

## RC49 — F1-D05d.2: checkpoint integrale della filiera alimentare (candidata, 05/11/2026)

- **Nuova direttiva dell'utente:** in sviluppo non si richiede compatibilità con vecchi database; i salvataggi sperimentali RC46–RC48 (`user_version=4`) non sono più supportati dal nuovo store alimentare. Nessuna migrazione obbligatoria. **La GUI e il suo database v14 rimangono intatti.** [ADR 0060](docs/adr/0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md).
- Aggiunto `FoodIndustryCampaignState`: stock Bovini/Selvaggina e prodotti in transito separati, sei edifici alimentari con lavoratori (Stalla, Cacciatore, Macellaio, Orto, Frutteto, Deposito), posti derivati dai conteggi e budget di popolazione unico con industria, scuola e militari esistenti.
- Evoluto `V15FoodCampaignCheckpoint` e `V15SqliteFoodSaveStore`: **schema SQLite generation 5**, tre nuove tabelle STRICT (`food_industry_site`, `food_animals_stock`, `food_transit_stock`) e 16 tabelle totali; validazione campagna completa, staging/round-trip e backup del precedente checkpoint valido.
- Aggiunto `V15FoodCampaignTicks.Plan`: ponte puro di simulazione che riproduce un tick alimentare e aggiorna lo snapshot, **senza avanzare il clock storico**. Round-trip con transito di materiali non consegnabili verificato a livello di test scritto, ma non ancora eseguito.
- **17 test `[Fact]` nuovi (12 Infrastructure + 5 Core).** Ultimo gate RC47 **620 confermati** dall'utente, RC48 631 e RC49 **648 attesi**, nessuna compilazione/xUnit .NET 10 eseguita qui. Non vengono attivati cataloghi, costruzione o GUI; questa è la preparazione alla D05d.3.
- [Contratto](docs/design/F1_D05D2_RC49_CHECKPOINT_FILIERA_SQLITE5.md) · [Piano test](docs/validation/F1_D05D2_RC49_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

## RC48 — F1-D05d.1: coordinatore unico delle filiere alimentari (candidata, 05/11/2026)

- **RC47 confermata dall'utente: 620 test passati.** Lo store v15 e la migrazione copy-on-write di RC46/RC47 risultano validati cumulativamente, restando isolati dal gameplay.
- Aggiunto **`FoodIndustryTickPlanner` nel Core**, senza cambiare alcun file C# preesistente: un passo puro per Stalla, Cacciatore, Macellaio, Orto, Frutteto e Deposito, con ordine di consumo dell'Acqua, animali disponibili solo dal passo successivo, trasferimento capiente degli ingredienti e razioni Cibo/Pane atomiche.
- Inventari separati `City`, `Animals`, `Transit`, `Depot` e preflight di personale su posti e popolazione realmente libera. Bovini/Selvaggina e scorte intermedie non finiscono nell'inventario cittadino storico e non vengono scartati per saturazione.
- **11 nuovi test Core; RC48: 631 cumulativi attesi, non eseguiti in questo ambiente senza SDK .NET 10.** Nessun cambio a v13, v14, UI, SQLite, costi o tecnologie. Questa è la prima tranche di D05d, **non una versione giocabile della nuova economia**.
- Prima dello switch servono una persistente estensione versionata degli stock intermedi e delle cinque strutture, una sessione integrata, un backup/rollback di attivazione e gli audit GUI. **F1-D05d.2–d.4** sono i prossimi passi; F1-D05e/E4a/E4b mantengono l'ordine deliberato.
- [Specifica](docs/design/F1_D05D1_RC48_COORDINAMENTO_FILIERA.md) · [Piano test](docs/validation/F1_D05D1_RC48_TEST_PLAN.md) · [ADR 0059](docs/adr/0059-confini-produzione-alimentare-e-proprieta-stock.md) · [Roadmap](docs/ROADMAP.md).


## RC47 — F1-D05c: migrazione v14→v15 copy-on-write (candidata, 05/11/2026)

- Aggiunto `V14ToV15FoodMigrator` in Infrastructure, senza collegamento al gioco: preflight read-only, SHA-256 del file principale, doppia validazione logica della v14, separazione scorte al millesimo e diagnostica sovracapacità.
- La nuova v15 viene prima salvata su un **percorso di staging nella stessa directory**, ricaricata e confrontata integralmente (risorse, lavoratori, edifici, scuola, militari, progressi, ricerca, tier e popolazione). Solo successivamente viene promossa con `File.Move` senza overwrite.
- Destinazione già identica: richiesta idempotente senza scrittura. Destinazione incompatibile/evoluta: rifiuto senza perdita; v14 originale sempre conservata per rollback. Non è ancora un cambio di salvataggio attivo e non dispone di switch o backup aggiuntivo dell'originale.
- **11 nuovi test Infrastructure**; RC46 609 attesi + 11 = **620 test RC47 attesi**, senza SDK .NET 10 per l'esecuzione locale. **Ultimo checkpoint Windows confermato RC45: 600/600**. GUI, gioco, ruleset e costi invariati.
- [Contratto F1-D05c](docs/design/F1_D05C_RC47_MIGRAZIONE_COPY_ON_WRITE.md) · [Test RC47](docs/validation/F1_D05C_RC47_TEST_PLAN.md) · [ADR 0058](docs/adr/0058-migrazione-v14-v15-senza-sovrascrittura.md) · [Roadmap](docs/ROADMAP.md).

## RC46 — F1-D05b: formato SQLite v15 alimentare separato (candidato)

- Creato `V15FoodCampaignCheckpoint` con inventario cittadino senza alimenti e deposito con scorte grezze + Cibo pronto; validazione della proprietà esclusiva e conservazione delle scorte storiche anche oltre le nuove capienze.
- Aggiunto `V15SqliteFoodSaveStore` completamente separato dai writer v13 e v14: schema SQLite v4, regole `civil-growth-15`, 13 tabelle STRICT, salvataggio temporaneo con validazione, backup consistente e protezione contro sovrascrittura di file di generazione diversa.
- Aggiunti **9 test di regressione** su round-trip, precisione, compatibilità v14, corruzione, versioni future, backup, scorte sovracapienti.
- GUI, motore economico, filiere attive, file v14 e percorsi live invariati; nessuna migrazione automatica. Nessun coefficiente E4b modificato.
- **Validazione:** RC45 600/600 confermati; RC46 **609 test attesi, non eseguiti su Windows**. DDL controllato con SQLite; documentazione, handoff e ADR 0057 aggiornati.

# RC45-DOC01 — 4 novembre 2026 · Riordino documentale e strategia economica (solo documenti)

- **Gate verificato:** RC45 confermata dall'utente con **600 test superati**. Nessun nuovo test applicativo o modifica codice in RC45-DOC01.
- **Decisione di game design:** futura progressione dei costi per epoche/famiglie di ordine quasi esponenziale; prima Zecca oggi troppo economica (210 Legno lavorato + 160 Pietra vs 30 + 10 del Tagliapietre). Intervallo di investimento guida equivalente a **100–1.000 Tagliapietre base**, non prezzo approvato.
- **Soglie per lavoratori:** Zecca da almeno 10 addetti per modulo produttivo; due Zecche operative con 20. Il modello tier 1/3/10 posti va riprogettato per le famiglie interessate, non alterato senza migrazione.
- **Nessuna nuova economia attivata:** prima F1-D05b/c/d/e (v15 separata, migrazione e filiere alimentari), poi benchmark E4a, decisione dei coefficienti E4b, E4c e navale.
- Creati [specifica economica](docs/design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md), [matrice di test](docs/validation/E4B_P00_COSTI_SQUADRE_TEST_MATRIX.md), [ADR 0056](docs/adr/0056-progressione-tardiva-e-squadre-minime-proposta.md) e [registro pulizia](docs/history/DOCUMENTATION_CLEANUP_RC45_DOC01.md).
- Riscritti i documenti operativi per eliminare gli aggiornamenti di versione concatenati; eliminati solo vecchi doppioni e audit senza riferimenti attivi, lasciando integra la cronologia degli ADR e del changelog.

---

# RC45 / F1-D05a-F1 — Fix test macellazione alternativa (2026-11-04)

- **Evidenza RC44:** 600 test eseguiti, **599 passati e 1 fallito** (`UnifiedFoodSupplyTests.CattleAndGameAreNotFoodUntilButcheredAndDepotProcessed`): la vecchia fixture produceva legittimamente 6 Carne (2 Bovini × 3), mentre si attendeva 4.
- **Corretto soltanto il test**: una ricetta Bovini ora consuma 2 capi per 3 Carne e la ricetta Selvaggina consuma 1 capo per 1 Carne. Il test usa la scorta del tick precedente e verifica esplicitamente l'esecuzione di entrambe le ricette e l'assenza di animali residui.
- Nessun cambiamento a produzione reale, salvataggi, GUI, cataloghi o sistema di backup; **600 test attesi su RC45**, ancora da eseguire su Windows. Ultima suite integralmente confermata RC42: 577 test.
- [Rapporto](docs/design/F1_D05A_RC45_FIX_SCENARIO_CARNE.md) · [Test Windows](docs/validation/F1_D05A_RC45_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# RC44 / F1-D05a — Piano reversibile di pre-migrazione (2026-11-04)

- **RC42 validata: 577 test verdi. RC43 non ancora validata: 590 attesi. RC44: 10 test aggiunti, 600 attesi.** Nessun test Windows eseguito per RC44 in questo ambiente.
- `V14FoodStockMigration` sposta logicamente `food` nel Cibo pronto e gli ingredienti nel deposito, con ricostruzione esatta dell'inventario precedente e diagnosi delle capacità insufficienti. Nessuna conversione o perdita di risorse.
- Nuovo `V14SqliteSaveStore.PreviewFoodMigration`: legge un checkpoint v14 valido senza modificarlo e senza creare backup. Aggiunti 8 test Application e 2 Infrastructure (byte SQLite invariati e checkpoint mancante).
- **V14 giocabile immutata**: nessun nuovo formato/slot, nuovo tick o filiera attiva. F1-D05b nuovo file SQLite versionato, D05c migrazione copy-on-write, D05d attivazione simultanea cataloghi/tick/UI. E4b sospesa.
- [Rapporto](docs/design/F1_D05A_RC44_PREMIGRAZIONE_SCORTE.md) · [Piano Windows](docs/validation/F1_D05A_RC44_TEST_PLAN.md) · [ADR 0055](docs/adr/0055-preflight-migrazione-alimentare-immutabile.md) · [Roadmap](docs/ROADMAP.md).

---

# RC43 / F1-D04 — Confine unificato del Deposito alimentare (2026-11-04)

- **RC42 validata dall'utente: 577 test superati**, Salva funzionante anche dopo i 100 backup storici.
- Aggiunto `src/SimRisorse.Core/Food/UnifiedFoodSupply.cs`: policy completa dei 5 alimenti, passaggio dalla scorta grezza al deposito con capacità, lavorazione per personale e consumo atomico Cibo+Pane.
- Il Pane protetto non diventa Cibo generico; niente scorte duplicate, perdita materiale o conversione senza lavoratori; nuove prove integrate anche con Orto/Frutteto e Macellaio.
- **13 test Core aggiunti; RC43: 590 test attesi, non eseguiti** in questo ambiente. Report, ADR 0054, roadmap, stato e handoff aggiornati.
- **Nessuna attivazione nel gameplay**: `V14CampaignSession`, clock, popolazione, cataloghi attivi, GUI e SQLite invariati; F1-D05 richiede migrazione sicura e collaudo end-to-end.
- [Rapporto F1-D04](docs/design/F1_D04_RC43_DEPOSITO_UNIFICATO.md) · [Piano test](docs/validation/F1_D04_RC43_TEST_PLAN.md) · [ADR](docs/adr/0054-confine-unico-del-deposito-e-razioni-atomiche.md) · [Roadmap](docs/ROADMAP.md).

---

# RC42 / F1-D03 — Ortofrutta e correzione urgente backup v14 (2026-11-04)

- **RC41 validata dall'utente: 563/563 test superati**. **RC42 candidata: 577 test attesi**, 14 nuovi, esecuzione Windows pendente.
- Corretto `V14SqliteSaveStore.BackupExisting`: dopo i 100 backup storici occupati crea backup aggiuntivi **senza limiti di slot**, con nome univoco e `VACUUM INTO`, conservando **tutti** i backup precedenti. Nessuna rotazione automatica; lo spazio disco può crescere. Due nuovi test Infrastructure su backup 101/102 e checkpoint invalidi.
- Introdotto `HorticultureModel.cs` Core **isolato**: Orto→Verdura e Frutteto→Frutta, personale e capienza indipendenti, uso facoltativo di Acqua configurabile e lotti atomici. 12 test Core. Nessun costo/ricetta definitiva approvata.
- **Gioco corrente invariato salvo il fix del salvataggio:** catalogo, GUI v14/v13, motore di produzione, schema SQLite e regole alimentari attive non modificati. Ortofrutta non ancora costruibile; collegamento al deposito e migrazioni in F1-D04/D05.
- Documentazione: [rapporto](docs/design/F1_D03_RC42_ORTOFRUTTA_E_BACKUP_V14.md) · [validazione](docs/validation/F1_D03_RC42_TEST_PLAN.md) · [ADR 0052](docs/adr/0052-backup-v14-oltre-cento-slot.md) · [ADR 0053](docs/adr/0053-orto-frutteto-modello-isolato.md) · [roadmap](docs/ROADMAP.md).

---

# RC41 / F1-D02 — Stalla, Cacciatore e Macellaio a ricette alternative (2026-11-04)

- RC40 **validata dall'utente, 552 test superati**; nuova RC41 **563 test attesi e non eseguiti qui**.
- Creato `src/SimRisorse.Core/Food/MeatChainModel.cs`: produzione distinta Grano+Acqua→Bovini, Cacciatore→Selvaggina e due ricette alternative Macellaio→Carne, con lotti interi, capienza, lavoratori condivisi, priorità configurabile e divieto di macellazione nello stesso tick degli animali appena prodotti.
- Aggiunti **11 test Core** in `MeatChainFoundationTests.cs`, incluso il passaggio obbligato Carne→Deposito→Cibo pronto e la verifica di assenza di sprechi. Rese dei test solo illustrative, **nessun nuovo coefficiente approvato**.
- **Dominio isolato:** nessuna attivazione delle tre nuove risorse o degli edifici nella v14; invariati la GUI, il catalogo della partita, le regole economiche, SQLite e i salvataggi. La migrazione e i nuovi tier restano in F1-D04/D05.
- Aggiornati [rapporto F1-D02](docs/design/F1_D02_RC41_FILIERA_CARNE.md), [piano test](docs/validation/F1_D02_RC41_TEST_PLAN.md), [ADR 0051](docs/adr/0051-due-origini-carne-e-priorita-macellaio.md), roadmap, stato corrente e handoff.

---

# RC40 / F1-D01 — Modello di deposito alimentare (2026-11-04)

- Recepita la riforma approvata: Pesce, Pane, Carne, Frutta e Verdura passano dal Deposito alimentare; Stalla e Cacciatore alimentano due vie verso il Macellaio; Orto e Frutteto sono filiere distinte. Pane necessario anche alle abitazioni avanzate, con riserva non convertibile due volte.
- Creato `SimRisorse.Core/Food/FoodDepotModel.cs`: inventario grezzo/Cibo pronto, ricezione atomica per capacità, trasformazione a lotti interi entro i limiti di personale e capienza, priorità deterministica e protezione della riserva Pane.
- **7 test Core aggiunti** in `FoodDepotFoundationTests.cs`; **552 test complessivi attesi**. Ultima conferma RC38: **541/541**; RC39 (545) e RC40 (552) da validare. Nessun benchmark E4a misurato.
- Implementazione **isolata**: il nuovo planner NON è ancora collegato a V14CampaignSession, GUI, catalogo, save SQLite o motore. Nessuna modifica ai parametri produttivi, al bilanciamento o ai salvataggi esistenti.
- Nuovi [modello F1-D01](docs/design/F1_D01_RC40_MODELLO_ALIMENTARE.md), [ADR 0050](docs/adr/0050-filiera-alimentare-unificata-deposito.md), [piano Windows](docs/validation/F1_D01_RC40_TEST_PLAN.md). Roadmap/handoff aggiornati; E4a storica resta utilizzabile solo come precedente economico, E4b rimandata.

---

# RC39 / E4a-D02 — Benchmark di progressione naturale (2026-11-04)

- **Baseline RC38:** 541 test Windows confermati superati dall'utente. **RC39 candidata:** +4 test, **545 attesi**, non eseguiti nel confezionamento.
- Aggiunto `E4aNaturalBenchmark` per E4A-01…05: stessa partita iniziale v14 non finanziata, politica `natural-reference-v1` dichiarata, eventi dei comandi, checkpoint periodici ogni 60 secondi simulati e obiettivi osservabili (Cibo, Pane/abitante avanzato, Scuola, tecnologia, Segheria Grande).
- Comando CLI `--benchmark-e4a-natural [--scenario E4A-0X] [--output DIRECTORY]`: JSON/CSV, prevenzione sovrascritture e esiti `pass`/`timeout`/`blocked` senza tempi inventati. Quattro test Application su stock iniziale, avanzamento reale, determinismo, formato e parametri.
- Documentazione E4a-D02, ADR 0049, piano Windows e handoff aggiornati. `benchmarks/e4a/scenarios.csv` resta **not-run**: nessun benchmark dichiarato misurato.
- **Invariati:** Core, GUI v13/v14, SQLite, salvataggi, costi, ricette e coefficienti. E4b subordinato ad analisi e approvazione.

---

# RC38 / E4a-D01-F1 — Confronto snapshot SQLite indipendente dall'ordine (2026-11-04)

- **Evidenza RC37 su Windows:** 541 test eseguiti, 540 superati e 1 fallito: `V14SchoolResearchEndToEndTests.PencilsToStudentToResearchToLargeTierSurvivesTwoSqliteRoundTrips`, per ordine di `OtherBuildings` diverso dopo caricamento SQLite.
- Corretto esclusivamente il confronto nel test: `OtherBuildings` per `BuildingId`, `ProductiveFamilies` e `PurchaseCounters` per `FamilyId`, `UnlockedTechnologyIds` per ID, sempre con `StringComparer.Ordinal`. Confronto **integrale** dei record e verifica ordinata della formazione invariati.
- Nessuna modifica a GUI, Core, Application, SQLite, save schema, ricette o benchmark; **541 test attesi, RC38 non ancora validata**.
- [Rapporto](docs/design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) · [Piano Windows](docs/validation/E4A_D01_RC38_TEST_PLAN.md).

---

# RC37 / E4a-D01 — Runner riproducibile per confronto Scuole (2026-11-04)

- Primo benchmark **v14 realmente eseguibile** nella CLI `SimRisorse.Simulation --benchmark-e4a-school`, scenari controllati E4A-06/07 (1 o 2 Scuole): stessa dotazione di partenza dichiarata, comandi veri, 30 s per campione, CSV/JSON senza tempi o checkpoint fittizi.
- Nuovo componente `SimRisorse.Application.Benchmarks.E4aSchoolBenchmark`: nessun cambiamento alle regole del gioco, ai coefficienti, alla GUI, ai salvataggi. Output non sovrascrivibile.
- **3 test nuovi** (esito/tempi, ripetibilità, CSV): **541 test cumulativi attesi; NON eseguiti qui.** Ultimo gate Windows confermato RC30 (511); RC31–RC37 ancora candidate.
- Protocollo, report, piano di validazione, ADR 0048 e handoff aggiornati. **E4a non ha ancora risultati misurati; E4b non iniziata.**
- [Rapporto E4a-D01](docs/design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Test plan](docs/validation/E4A_D01_RC37_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# RC36 / P1-D50 — Consolidamento Scuola/Ricerca e preparazione E4a (2026-11-04)

- Aggiunti **2 test** a `V14SchoolPencilPersistenceTests.cs`: salvataggio SQLite pre-Matite con uno studente realmente in formazione e assenza fisica della colonna `pencil_paid_students`; rifiuto del ledger Matite impossibile, senza sovrascrivere il checkpoint valido.
- Creati registro dei 10 scenari E4a (`natural`/`controlled`), template risultati e protocollo di misurazione; **non eseguiti**, nessun dato prestazionale dichiarato.
- Aggiornati piano test, roadmap, stato, handoff e ADR 0047. **Nessuna modifica al codice applicativo, GUI, costi, ricette, ricerca o formato SQLite.**
- **538 test cumulativi attesi; non eseguiti qui** (RC35: 536). Ultimo gate Windows confermato: RC30 (511). Gate P1 ancora aperto.

---

## RC35 / P1-D49 — Verifica end-to-end scuola, ricerca e tier (2026-11-04)

- Aggiunti tre test di integrazione `V14SchoolResearchEndToEndTests` su sessione v14 e SQLite v3: filiera Matite, iscrizione, 120 s formazione, assegnazione, accumulo 100 punti, acquisto Lavorazioni I, upgrade Segheria Grande, due round-trip e ripresa in pausa.
- Coperti anche formazione parziale e fallimenti atomici senza mutazioni; fixture con fondi entro capacità, nessun punto ricerca o tecnologia conferiti artificialmente.
- **536 test cumulativi attesi (533 + 3), non eseguiti qui**. Ultima conferma utente RC30 511; RC31–RC34 ancora candidate.
- **Nessuna modifica ai sorgenti dell'applicazione**, alla GUI v13/v14, al DB SQLite, a ricette o costi. Prossimo gate: P1-D50, dossier di consolidamento prima di E4a.
- [Rapporto](docs/design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Validazione](docs/validation/P1_D49_RC35_TEST_PLAN.md) · [ADR 0046](docs/adr/0046-catena-integrata-scuola-ricerca-tier.md).

## RC34 / P1-D48 — Benefici effettivi dell'Albero tecnologico (2026-11-04)

- Introdotto `TechnologyTierEffects`: proiezione da regole tier concrete, verifica di copertura dichiarata e dipendenze Grande→Avanzato, rifiuto di ricerche acquistabili senza effetto.
- La sessione v14 valida il catalogo prima di accettare una nuova partita o un caricamento. Il dettaglio della ricerca ora mostra esplicitamente gli edifici Grande/Avanzato sbloccati; Logistica e Navigazione rimangono non disponibili.
- **9 test aggiunti, 533 cumulativi attesi; compilazione/xUnit pendenti su Windows.** Baseline RC30 (511 test confermati). RC31–RC33 incluse cumulativamente.
- Nessuna modifica a costi, produzione, personale, Matite, GUI v13, salvataggi o schema SQLite.
- [Rapporto](docs/design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Piano](docs/validation/P1_D48_RC34_TEST_PLAN.md) · [ADR 0045](docs/adr/0045-tecnologie-effetti-verificati.md).

## RC33 / P1-D47 — Filiera Matite e stato comandi (2026-11-04)

- Unificata la verifica della produzione Matite in `V14CampaignSession.CheckPencilCraft`: stessa valutazione del comando reale, senza modifiche durante la consultazione.
- Scuola v14: quantità mancanti di Legno lavorato e Carbone per +1/+5/+10, spazio Matite disponibile/necessario e pulsante coerente col dominio.
- Rafforzata l'atomicità dei lotti tramite test; test per annullamenti misti legacy/prepagati. Nessuna variazione a ricette, rimborsi, capacità, economia, SQLite o v13.
- **6 nuovi test (524 cumulativi attesi), da validare su Windows.** RC30 è l'ultimo checkpoint testato (511 confermati); RC31 e RC32 restano candidate incluse nella RC33.
- [Rapporto](docs/design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano test](docs/validation/P1_D47_RC33_TEST_PLAN.md) · [ADR 0044](docs/adr/0044-unica-verifica-laboratorio-matite.md).

## RC32 / P1-D46 — Scuola: progressi individuali (2026-11-04)

- Aggiunto nella Scuola v14 un riepilogo di sola lettura degli avanzamenti individuali (fino a 6 posti con altri aggregati), aggiornato a ogni tick/caricamento.
- Conservati la barra del primo completamento e i comandi esistenti; invariati formazione, Matite, ricercatori, costi e SQLite.
- Aggiunti 3 test: UI binding headless, ripresa di due progressi differenziati con coda e test Core di formazione parallela.
- RC30 confermata 511 test; RC31 (515) non testata; RC32 **518 attesi e non testati**. Si richiede validazione Windows cumulativa.
- Documenti: [Rapporto](docs/design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md), [Piano](docs/validation/P1_D46_RC32_TEST_PLAN.md), [ADR 0043](docs/adr/0043-dettaglio-individuale-formazione-scuola.md).

## 2026-11-04 — G1-D45 / RC31 — Promozione v14 a GUI predefinita (candidata)

- Dopo la conferma dell'utente sulla RC30 (511 test), la GUI v14 diventa la modalità **normale** all'avvio senza argomenti; v13 disponibile come `--legacy-v13`.
- `--v14` e `--experimental-v14` rimangono alias v14; `--scenario` e `--smoke-test` preservano il routing legacy degli script esistenti.
- Nessun trasferimento dei checkpoint: la v14 utilizza lo **stesso** file `experiments/civil-growth-v14.sqlite3`; protezione dei percorsi riservati contro aperture incrociate v13/v14.
- Eliminati i testi «sperimentale» dai banner della v14; struttura grafica e caratteristiche del gioco invariate. Aggiunti **4 test xUnit** per il routing (totale **515 attesi**, non ancora eseguiti qui).
- **Accettazione della GUI comunicata dall'utente**, non confondere con il dossier P0 di 44 screenshot mai acquisiti. ADR 0042, rapporto D45, piano RC31 e handoff aggiornati. Prossimo P1-D46 Scuola; E4 successivo.

---

## 2026-11-04 — G1-D44 / RC30 — Robustezza raccolta evidenze P0

- `scripts/p0-ui-evidence.ps1`: PNG controllati per contenuto leggibile, formato e dimensioni compatibili con viewport; false evidenze segnalate come `EVIDENZA_MANCANTE`.
- Le eccezioni `NA` non fanno risultare completata l'intera matrice; introdotta azione `Verify` con codici uscita 0/2 e messaggio di gate sempre subordinato ad approvazione manuale.
- Nuovo auto-test PowerShell autonomo `scripts/test-p0-ui-evidence.ps1`, rapporto D44, piano di validazione e ADR 0041.
- **Nessun cambiamento** a sorgenti dell'app, UI v13/v14, economia, salvataggi o ai 511 test .NET della candidata.
- **Baseline confermata:** RC27 507 test; RC28–RC30 511 attesi e **non eseguiti qui**; P0 visivo senza evidenze reali, aperto.

---

## v60-E3b4-RC29 / G1-D43 — Preparazione confronto visivo P0 (CANDIDATA, 04/11/2026)

- RC27 rimane l'ultima suite confermata dall'utente: **507 test**. RC28 introduce +4 test headless (511 attesi); **RC29 mantiene invariati 511 test, non eseguiti** qui.
- Creato `scripts/p0-ui-evidence.ps1`: inizializza registro da **44 scenari** senza sovrascrivere prove esistenti, opzionalmente salva PNG espliciti dalla clipboard Windows, e genera report HTML offline affiancando v13 e v14. Il report segnala evidenze mancanti e non chiude mai automaticamente P0.
- Audit statico delle due finestre: dimensioni condivise, sidebar 240, stessa griglia Risorse 7 colonne; differenza Edifici 310/410 e sezioni nuove Scuola/Albero; nessuna prova visiva reale disponibile. Documenti G1-D43 e handoff aggiornati. **P0 APERTO, 0/44 scenari visivi eseguiti qui.**
- Sorgenti applicativi, test xUnit, GUI v13, SQLite, costi, capacità e ricette **immutati** rispetto a RC28. Nessuna pretesa di build/test Windows o di prova runtime PowerShell. E4 sospeso.

---

## v60-E3b4-RC28 / G1-D42 — Focus e tastiera v14 (CANDIDATA, 04/11/2026)

- **Baseline valida Windows:** RC27, **507 test superati** confermati dall'utente. **RC28:** +4 esecuzioni Desktop headless (2 scenari, due risoluzioni), **511 test attesi**, non ancora compilati/testati in questo ambiente.
- Inseriti solo nella finestra v14 stili di **focus visibile** per menu laterale e liste Edifici/Risorse/Tecnologie. Bordo a larghezza costante (trasparente → ocra quando focalizzato); colori di selezione e hover immutati, nessun salto di layout da bordo di focus.
- Test di focus, frecce del menu, sincronizzazione delle schede, colore di selezione e geometria alle viewport 1040×740 e 1240×850. TAB/Shift+TAB e confronto visivo P0 restano da verificare realmente su Windows.
- Invariati GUI v13, Core, Application, SQLite v2/v3, ricette, prezzi, Matite e salvataggi; E4 non avviato. [Rapporto](docs/design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano](docs/validation/G1_D42_RC28_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## G1-D42 — 04/11/2026 — Roadmap dettagliata post-RC27 (solo documentazione)

- **RC27 validata dall'utente:** 507 test superati su Windows; resta la stessa baseline applicativa, **nessuna RC28** e nessun nuovo test.
- `docs/ROADMAP.md` riscritta come piano canonico per D42–D45 (P0), P1-D46–D50, E4a, E4b, E4c e M5.4–M7; precedente roadmap storicizzata in `docs/history/operational/ROADMAP_G1_D41_RC27_ARCHIVE.md`.
- Nuovo `docs/validation/G1_D42_P0_RUNBOOK.md` per evidenze UI-01…UI-22, due viewport, focus, stato, salvataggi e gating; nuovo ADR 0040 sulla disciplina dei gate.
- Allineati README, START_HERE, CURRENT_STATE e handoff. **Nessun sorgente, test, schemi SQLite, salvataggio o coefficiente economico modificato.** P0 grafico non dichiarato chiuso.

---

## v60-E3b4-RC27 / G1-D41 — Reset scorrimento sul cambio edificio (CANDIDATA, 04/11/2026)

- **RC26 verde su Windows, 504 test superati**, confermati dall’utente. RC27: tre nuovi casi Desktop headless, **507 attesi**, non compilati/eseguiti qui per assenza di .NET 10.
- **UI-07/UI-12/UI-18:** reset degli offset del dettaglio principale e della diagnostica v14 al `SelectionChanged` dell’elenco Edifici. Refresh e aggiornamenti dello stesso edificio non causano reset, nessun trasferimento forzato del focus.
- Lo XAML aggiunge soltanto un nome al viewer principale; evento XAML collegato al code-behind; nuovi test a 1040×740 e 1240×850 più conservazione della posizione sullo stesso edificio.
- **Invariati:** GUI v13, disposizione pulsanti, tabella Risorse, economica/ricerca, motore, SQLite, salvataggi e ricette. Gate P0 aperto, E4 sospeso.
- [Rapporto D41](docs/design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Piano](docs/validation/G1_D41_RC27_TEST_PLAN.md).

## v60-E3b4-RC26 / G1-D40 — Presentazione crescita e audit flussi (CANDIDATA, 04/11/2026)

- **RC25 confermata su Windows dall’utente:** 501 test passati. **RC26:** +3 test Desktop, **504 attesi**, build e xUnit non eseguiti nell’ambiente di confezionamento.
- **UI-14:** nella v14 il testo di crescita perdeva i ventesimi di progresso e l’ETA del prossimo abitante dopo l’accelerazione; riutilizzata la logica di formattazione della GUI v13 con `GrowthActiveStatus`, tempo residuo arrotondato per eccesso e precisione 0,05 s. I blocchi Cibo/Pane/abitazioni conservano la precedenza.
- **UI-05:** test di regressione della stima `si riempie tra` basata su produzione meno consumo, con casi pieno, senza capienza, saldo nullo/negativo e tempi lunghi.
- **Invariati:** GUI v13, XAML, motore, regole E4, SQLite e salvataggi; P0 grafico delle 22 voci ancora aperto.
- [Rapporto](docs/design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Piano](docs/validation/G1_D40_RC26_TEST_PLAN.md) · [Matrice](docs/validation/G1_D39_P0_UI_MATRIX.md).

## RC25 / G1-D39-F1 — 2026-11-04 — Correzione compilazione test CS1061

- **RC24:** compilazione `SimRisorse.Desktop.Tests` fallita prima dei test per `CS1061` su `V14PreviewWindow.MouseMove` (righe 71 e 136 di `V14BuildingUpgradeAuditTests.cs`).
- **RC25:** aggiunto `using Avalonia.Headless;` al file test; `MouseMove` è un metodo di estensione già usato con lo stesso namespace in altri test.
- **Nessun nuovo test:** 501 attesi, esecuzione Windows pendente; ultima suite confermata RC20, 490 passati. Nessuna modifica a codice applicativo, GUI, SQLite, prezzi o salvataggi; P0 visuale ancora aperto.
- Documentazione: [report](docs/design/G1_D39_RC25_FIX_CS1061_HEADLESS.md), [piano test](docs/validation/G1_D39_RC25_TEST_PLAN.md), roadmap e handoff aggiornati.

## v60-E3b4-RC24 / G1-D39 — Coerenza suggerimenti Scuola e matrice gate P0 (CANDIDATA, 04/11/2026)

- **Baseline confermata Windows:** RC20, 490 test. **RC21/RC22/RC23:** 493/496/499 attesi, non verificati singolarmente. **RC24:** +2 test Desktop, **501 attesi**, da validare cumulativamente su Windows.
- **D39-01 / UI-16, UI-20:** durante la conferma «Nuova partita?» i comandi iscrizione, annullamento studenti e produzione Matite erano bloccati, ma i messaggi della Scuola non lo indicavano. I tre suggerimenti ora mostrano «sospeso/a/i» e tornano allo stato precedente con Annulla.
- Nuovi test ViewModel + Avalonia: `V14SchoolConfirmationHintsAuditTests.cs`. Non cambiati costo 2 Matite/studente, simulazione, GUI v13, XAML, SQLite, salvataggi o bilanciamento.
- **D39-02:** nuova [matrice operativa delle 22 verifiche P0](docs/validation/G1_D39_P0_UI_MATRIX.md), con evidenza test/stato e controlli visivi 1040×740 e 1240×850 ancora da registrare.
- [Rapporto D39](docs/design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Piano Windows](docs/validation/G1_D39_RC24_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md). **P0 aperto, E4 sospeso.**

---

## v60-E3b4-RC23 / G1-D38 — Audit navigazione e selezione (CANDIDATA, 04/11/2026)

- **Ultima baseline Windows confermata:** RC20 (490 test). RC21 (493), RC22 (496), RC23 (**499 attesi**) non ancora eseguite su Windows.
- **UI-01/UI-19:** 3 nuovi test Desktop in `V14NavigationSelectionAuditTests.cs` (due viewport sidebar/schede/tick e uno Save/Load + identità selezioni); nessun bug di layout certo rilevato, pertanto nessuna modifica arbitraria a GUI o gameplay.
- **Invariati:** XAML v13 e v14, Core/Application/Infrastructure, SQLite e salvataggi, coefficienti. P0 visivo resta aperto; E4 sospeso.
- [Rapporto D38](docs/design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Piano test](docs/validation/G1_D38_RC23_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC22 / G1-D37 — Scorrimento diagnosi Edifici e audit progressi Esercito (CANDIDATA, 03/11/2026)

- **Baseline verde confermata:** RC20, 490 test. **RC21 ancora non testata** (493 attesi); RC22 cumulativa, **496 test attesi** (+3), da validare su Windows.
- **Correzione GUI v14:** diagnosi produttiva nel box Edifici con scroll indipendente e altezza massima di 104 px; testi invariati, comandi costruzione/lavoratori e upgrade protetti dalla compressione verticale. La descrizione resta scrollabile a parte.
- **Test:** 2 casi Avalonia a 1040×740 e 1240×850 con diagnosi volutamente lunga, controllo di visibilità/scroll e selezione; 1 nuovo caso Avalonia sui progressi delle reclute dopo 10/30 secondi.
- **Ambito:** XAML v14 + test Desktop + documentazione. GUI classica v13, Core, economia, SQLite, schemi e salvataggi invariati. Nessun E4.
- **Gate:** ZIP/XAML verificabili staticamente, build .NET 10 e xUnit **non eseguiti qui**; P0 grafico resta aperto.
- [Rapporto D37](docs/design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano RC22](docs/validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC21 / G1-D36 — Stati espliciti degli upgrade Edifici (CANDIDATA, 03/11/2026)

- **RC20 validata dall'utente:** 490 test attesi, tutti dichiarati passati. **RC21:** +3 casi Desktop, **493 attesi**; non compilati/eseguiti qui per assenza SDK .NET 10.
- **D36-01 / UI-10/UI-12/UI-18:** `V14UpgradeNext` e `V14UpgradeAdvanced` si affidavano solo a `CanExecute`, senza il binding `IsEnabled` e lo stile coerente con Costruisci. Ora sono grigi se non pagabili; verde + hover ocra solo quando pagabili, senza salti geometrici. Potenziamenti sbloccati ma non pagabili **restano visibili** con costo e carenze.
- **Tre test**: 1040×740, 1240×850, aggiornamento dinamico della disponibilità dal costo esatto e upgrade atomico Base→Grande.
- **Ambito ristretto:** solo XAML v14, nuovo file test, documentazione. Invariati GUI v13, Core, Application, Infrastructure, prezzi, SQLite e salvataggi. **Audit visivo P0 ancora aperto**. E4 sospeso.
- [Rapporto D36](docs/design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md) · [Piano RC21](docs/validation/G1_D36_RC21_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC20 / G1-D35 — Audit e test di non-regressione Risorse (CANDIDATA, 03/11/2026)

- **Ultima conferma Windows:** RC16 con 476 test. RC17 (481), RC18 (484), RC19 (487) non testate. **RC20:** 490 test attesi (+3), nessuna esecuzione .NET in questo ambiente.
- Il confronto XAML/VM con v13 mostra sette colonne, margine scrollbar e righe stabili. **Nessuna regressione riproducibile dimostrata dal codice:** evita modifiche arbitrarie al layout già approvato.
- +3 casi test: due viewport Avalonia headless (1040×740 / 1240×850), sei colonne numeriche allineate a destra, gutter, selezione ocra, tooltip e aggiornamenti; un test ordine alfabetico italiano/17 risorse/identità righe e dettagli Matite.
- **Nessuna modifica ai sorgenti dell'applicazione**: GUI v13/v14, cataloghi, economia, SQLite e salvataggi immutati. P0 visivo aperto, E4 sospeso. Prossimo G1-D36 Edifici.
- [Rapporto D35](docs/design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano Windows](docs/validation/G1_D35_RC20_TEST_PLAN.md) · [Matrice](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md).

---

## v60-E3b4-RC19 / G1-D34 — Audit suggerimenti Esercito (CANDIDATA, 03/11/2026)

- **Ultimo gate Windows verificato:** RC16, 476 test. RC17 (481), RC18 (484) non testate; **RC19 attende 487 test** (+3 Desktop) e include integralmente le due candidate precedenti.
- **UI-15/UI-20:** nell'Esercito i suggerimenti di arruolamento, annullamento e limiti non potevano riconoscere la conferma «Nuova partita?» e potevano annunciare un comando possibile ma disabilitato. Ora indicano la sospensione temporanea e tornano coerenti dopo Annulla.
- Le carenze di Spade, abitanti, reclute rimborsabili e spazio di deposito riportano lotto, quantità disponibili e richieste. Corretto anche «1 recluta» al singolare. **Nessuna modifica ai comandi militari o al gameplay.**
- +3 test Desktop (stati, lotti e binding Avalonia); nessun cambiamento XAML, GUI v13, Core, SQLite, catalogo o salvataggi.
- [Rapporto D34](docs/design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano RC19](docs/validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md). **P0 visivo aperto, E4 sospeso.**

---

## v60-E3b4-RC18 / G1-D33 — Coerenza suggerimenti Scuola e lotti ricercatori (CANDIDATA, 03/11/2026)

- **Stato confermato:** RC16, 476 test Windows dichiarati superati dall'utente. **RC17 non ancora testata** (481 attesi); su richiesta dell'utente sviluppo proseguito. **RC18 candidata**, 3 nuovi test Desktop, **484 attesi**, suite .NET non eseguita in questo ambiente.
- **Difetto UI-16:** con Scuola e qualche posto di ricerca ancora disponibile, il suggerimento indicava «Assegna» anche con lotto +5/+10 superiore ai posti liberi, nonostante il comando fosse correttamente disabilitato.
- **Soluzione:** suggerimento ora confronta i posti ricerca disponibili con la quantità selezionata, distingue Scuola mancante, insufficienza di posti e insufficienza di ricercatori qualificati. Non cambia la possibilità di assegnare né i controlli del comando.
- **Test:** verifica del lotto +5 con 2 posti, cambio a +1 seguito da assegnazione effettiva, Scuola assente, e 6 posti con un solo qualificato. Stessi stati di errore e di abilitazione nei test. Nessuna modifica a GUI v13, XAML, Core, economia, SQLite o salvataggi.
- **Audit:** G1-D33 primo controllo aggiuntivo UI-16; UI-02/05/07/15 e verifica visiva 1040×740 / 1240×850 ancora aperti. E4 sospeso.
- [Rapporto D33](docs/design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Test RC18](docs/validation/G1_D33_RC18_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## v60-E3b4-RC17 / G1-D32 — Conferma nuova partita e autosalvataggio protetto (CANDIDATA, 03/11/2026)

- **Gate RC16:** l’utente ha comunicato **476 test superati**. RC17 aggiunge **5 test Desktop, 481 attesi**; compilazione/test Windows RC17 ancora pendenti.
- **UI-20/UI-21:** durante la conferma «Nuova partita?» ora vengono disabilitati anche Salva v14, Carica v14 e l'apertura di una seconda conferma; metodi pubblici Save/Load rifiutano l’operazione senza modificare la sessione.
- **Protezione del checkpoint:** dopo «Avvia nuova» l'autosalvataggio a intervalli e in chiusura resta disabilitato finché il giocatore non salva manualmente; conferma non ancora risolta + chiusura significa annullamento e salvataggio della vecchia colonia. Un caricamento riuscito ripristina il regime di autosalvataggio.
- Nuovo messaggio esplicito sull'autosalvataggio sospeso, nessuna modifica a GUI v13, Core, SQLite, schema o coefficienti.
- +5 test, ADR 0039, rapporto G1-D32 e piano Windows; **P0 visivo ancora aperto, E4 sospeso**.
- [Rapporto](docs/design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano test](docs/validation/G1_D32_RC17_TEST_PLAN.md) · [ADR](docs/adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md).

## v60-E3b4-RC16 / G1-D31 — Blocco delle azioni durante «Nuova partita» (CANDIDATA, 03/11/2026)

- **Gate RC15:** l'utente ha dichiarato superati i test Windows (**473 attesi**). RC16 **476 test attesi**, non compilati/eseguiti nell'ambiente di confezionamento privo di .NET 10.
- **Difetto D31-01 (UI-20):** nella v14 la conferma «Nuova partita?» bloccava Pausa/Riprendi, ma lasciava abilitati Costruisci, Lavoratore, ricerca, Scuola, Matite ed Esercito. La v13 proteggeva le azioni di gioco durante la conferma.
- **Soluzione:** guardia unica `CanMutateColony` su tutte le azioni mutanti, anche su comandi legacy non esposti nel layout, con `CanExecute` aggiornato, disabilitazione esplicita delle azioni che usano binding `IsEnabled` e doppia protezione in `Perform`. Conferma e annullamento rimangono utilizzabili; cancellazione della conferma ripristina i comandi senza alterare scorte. Salva/Carica conservano il comportamento della v13; flusso da riesaminare separatamente in audit UI-20/UI-21.
- **Test:** +3 Desktop (no mutazioni/reversibilità, ricerca già acquistabile, stato visivo Avalonia); **476 attesi**. **Nessuna modifica** a GUI v13, XAML, Core, economia, SQLite, salvataggi o coefficienti. P0 grafico e E4 ancora aperti.
- [Rapporto D31](docs/design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](docs/validation/G1_D31_RC16_TEST_PLAN.md) · [Audit UI-01…UI-22](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md).

## v60-E3b4-RC15 / G1-D30 — Parità capienza Matite e conferma nuova partita (CANDIDATA, 03/11/2026)

- **RC14: 470 test dichiarati superati dall’utente.** La GUI P0 resta da confrontare visivamente su Windows alle due risoluzioni previste.
- **UI-04/UI-12:** dettagli Edifici v14 ora elencano la capienza Matite di Municipio, Magazzino e Scuola con la stessa icona e lo stesso componente delle altre risorse. Per i primi due l’importo è ricavato dallo storage di Legno lavorato, per Scuola dalla costante Core nominata; valori economici effettivi invariati.
- **UI-20:** durante la conferma «Nuova partita» il comando Pausa/Riprendi è disabilitato come in v13 e torna disponibile alla chiusura della conferma.
- **Test:** +3 Desktop (dettagli capacità, guardia comando, disabilitazione visuale Avalonia), **473 attesi**; non eseguibili qui senza SDK .NET 10.
- **Ambito:** v14 presentazione e 1 costante nominata nel Core v14 senza cambiare la formula, test e documentazione. Nessun cambiamento a v13, XAML, SQLite, economie o salvataggi.
- [Report D30](docs/design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](docs/validation/G1_D30_RC15_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

# Changelog — SimRisorse

## v60-E3b4-RC14 / G1-D29-F1 — Fix compilazione tooltip Matite (CANDIDATA, 03/11/2026)

- **Risultato RC13 su Windows:** compilazione bloccata in `SimRisorse.Desktop` con **2 CS0136** (`owned`, `free` in `V14PreviewViewModel.Parity.cs`); suite xUnit non avviata. Ultimo gate Windows verde: RC12, **468 test attesi**, dichiarati passati dall'utente.
- **Causa:** nomi locali dichiarati nel ramo Matite che confliggono con nomi utilizzati nell'ambito contenitore dello stesso metodo C#.
- **Fix esclusivamente lessicale:** `owned` → `ownedBuilding` e `free` → `freePencilSpace` nel ramo tooltip Matite. Nessun cambiamento a formule, testi visibili, condizioni o comportamento.
- **Ambito:** un solo file C# di presentazione v14, più documentazione. Test invariati; **470 attesi RC14, da eseguire su Windows**. GUI v13/v14, Core, SQLite e salvataggi invariati. Audit grafico P0 e bilanciamento E4 ancora aperti.
- [Diagnosi](docs/design/G1_D29_RC14_FIX_CS0136.md) · [Piano RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

## v60-E3b4-RC13 / G1-D29 — Roadmap in sei fasi, audit GUI e tooltip Matite (CANDIDATA, 03/11/2026)

- **Gate RC12:** test dichiarati tutti passati dall'utente (**468 previsti**, conteggio TRX effettivo non comunicato). La GUI P0 v13/v14 **non è ancora certificata visivamente**.
- **Roadmap riscritta:** sequenza P0 GUI → Scuola/tecnologie → E4a benchmark → E4b bilanciamento approvato → E4c migrazione/backup → M5.4–M7 espansione navale; stop rule e gate espliciti.
- **Audit iniziale D29:** inventario UI-01…UI-22 con evidenze, difetti e prove Windows ancora necessarie; nessun gate marcato verde senza verifiche.
- **Correzione presentazione v14:** tooltip della capacità Matite con effettivi Municipio, Magazzini e Scuole posseduti, quantità e spazio libero, coerente col calcolo Core. Niente bonus fissi attribuiti a edifici non costruiti.
- **Test:** +2 casi Desktop, iniziale e checkpoint con 2 Magazzini + 1 Scuola e selezione persistente dopo Load: **470 attesi**, non eseguiti qui per assenza SDK .NET 10.
- **Ambito:** un solo file di sorgente Desktop v14 e un test Desktop; v13, XAML, Core, Application, SQLite, ricetta Matite e salvataggi invariati. [Roadmap](docs/ROADMAP.md) · [Audit](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano RC13](docs/validation/G1_D29_RC13_TEST_PLAN.md) · [ADR 0038](docs/adr/0038-roadmap-gate-gui-e-audit-matite.md).

## v60-E3b4-RC12 / G1-D28 — Albero tecnologico: punti e stato Acquista (CANDIDATA, 03/11/2026)

- **Gate RC11:** test dichiarati tutti passati dall’utente (**466 attesi**; numero effettivo non comunicato). La parità visiva P0 non è ancora approvata.
- **Punti ricerca:** saldo, velocità di produzione e costi ricerca mostrati con due decimali `it-IT` (`N2`); valore interno invariato in millipunti (precisione 1/1000).
- **Acquista ricerca:** corretto il comando che verificava solo la selezione. Ora rifiuta all'interfaccia tecnologie già acquistate, non implementate, prive di prerequisiti/edifici o con saldo insufficiente. Pulsante `IsEnabled` esplicito, grigio in disabled e verde soltanto quando acquistabile; hover ocra e geometria invariati.
- **Test:** 2 nuovi casi Desktop (saldo 99,999 vs costo 100,000, acquisto esatto e stato post-acquisto; stile Avalonia grigio/verde/hover), **468 attesi**, non eseguiti qui per assenza SDK .NET 10. Verificata staticamente struttura XAML e diff circoscritto.
- **Ambito:** solo presentazione/XAML v14, nuovo selettore di stile per il pulsante ricerca, test Desktop e documentazione. Campagna v13, motore, salvataggi SQLite e Scuola/Matite non modificati.
- [Rapporto G1-D28](docs/design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano test](docs/validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](docs/adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).

## v60-E3b4-RC11 / G1-D27 — Comandi Edifici in due righe (CANDIDATA, 03/11/2026)

- **Gate RC10:** test dichiarati tutti passati dall’utente, **464 attesi**, numero effettivo non specificato; verifica grafica P0 ancora da completare.
- **Dettaglio Edifici v14:** label **Costruisci** e pulsanti **+1 / +5 / +10** sulla stessa riga; sotto label **Lavoratore** e pulsanti **+1 / +5 / +10**. L’azione preesistente **−1** resta accanto alla label Lavoratore per evitare una regressione funzionale.
- **Nuovi comandi:** assegna 5/10 lavoratori attraverso l’API di sessione già esistente, in un’unica operazione. Abilitazione solo se ci sono abbastanza abitanti liberi e postazioni disponibili; niente assegnazioni parziali, aggiornamento dei comandi dopo ogni cambio stato o selezione.
- **Stati UI:** i tre pulsanti di costruzione mantengono `CanAffordSelected/Five/Ten`, classe `primary build`, disabilitazione grigia e hover ocra senza spostamenti; i comandi personale hanno stati disabilitati espliciti. Lotti +5/+10 di costruzione conservano quote cumulative e tooltip.
- **Test:** sostituito il test del vecchio layout con una teoria a due risoluzioni (1040×740 e 1240×850), adattati i controlli sull’hover e sullo stato dei pulsanti, aggiunto test di assegnazione batch e rifiuto per limiti. **+2 casi**, **466 attesi**, non eseguiti in questo ambiente senza .NET 10.
- **Ambito:** solo ViewModel/XAML v14, test Desktop e documentazione; **v13, Core, economia, SQLite, Matite e salvataggi invariati**. [Report](docs/design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Test plan](docs/validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](docs/adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).

## v60-E3b4-RC10 / G1-D26 — Fix compilazione dei test Matite (CANDIDATA, 03/11/2026)

- **Esito RC9 su Windows (utente):** build interrotta da **1 errore CS0246** in `V14SchoolSuppliesTests.cs:15` (`GameScenario` non risolto). Non sono stati eseguiti test xUnit per la RC9: non descrivere RC9 come 464/464 verdi.
- **Causa:** `GameScenario` è in `SimRisorse.Core.Content`, namespace non importato dal nuovo test Application.
- **Correzione unica al sorgente:** aggiunto `using SimRisorse.Core.Content;` in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs`; nessuna modifica a Core, Application produttiva, Desktop, XAML, SQLite, migrazioni, salvataggi o valori economici.
- **Evidenza:** nel log dell’utente gli altri otto progetti/assemblies indicati compilano; la soluzione globale non compila finché il riferimento del test non viene corretto. **464 test attesi in RC10**, non eseguiti in questo ambiente privo di .NET SDK 10.
- **Stato:** ultimo gate Windows positivo RC8 **456/456** confermato; RC10 **in attesa di validazione Windows e test grafici/manuali P0**. E4 rimane sospeso. [Diagnosi D26](docs/design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano D26](docs/validation/G1_D26_RC10_TEST_PLAN.md).


## v60-E3b4-RC9 / G1-D25 — Scuola con progressi e Matite (CANDIDATA, 03/11/2026)

- **RC8: 456 test superati su Windows**, confermati dall'utente il 03/11/2026. Nessuna dichiarazione di parità grafica già completata.
- Rimossi da entrambe le finestre i pulsanti visibili «Esci senza salvare». Conservate le modalità di chiusura tecnica dei test e il salvataggio ordinario.
- **Scuola v14** allineata al layout Esercito: abitanti, posti, coda, progressi individuali (barra e tempo primo allievo), qualificati e ricercatori attivi; costruzione resta soltanto in Edifici.
- Nuova risorsa **Matite** della sola v14; laboratorio manuale **1 Legno lavorato + 1 Carbone → 4 Matite**, nuova riga alfabetica nella tabella Risorse a 7 colonne con capienza condivisa (Municipio +1.000, Magazzino +500, Scuola +50). **2 Matite + 10 Cibo per iscrizione**; consumi e rimborsi atomici con vincoli di stoccaggio.
- Persistenza SQLite v3 sperimentale estesa con **campo opzionale `pencil_paid_students`**, leggendo ancora i salvataggi RC8 senza generare rimborsi Matite fittizi. Il clock preserva lo stock Matite e libera il diritto al rimborso quando termina la formazione.
- **8 test nuovi: 464 attesi**, non eseguiti qui senza SDK .NET 10. Modificati Core v14, SQLite v3, presentazione e XAML v13/v14 (solo per togliere il pulsante v13), test e documenti. **Catalogo v13 (16 risorse), simulazione v13 e SQLite v2 preservati.** Il confronto grafico P0 e il bilanciamento E4 sono ancora aperti.
- [Rapporto D25](docs/design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [ADR 0035](docs/adr/0035-scuola-formazione-matite-v14.md) · [Piano RC9](docs/validation/G1_D25_RC9_TEST_PLAN.md).

## v60-E3b4-RC8 / G1-D24 — Stati Edifici, pulsanti e Scuola (CANDIDATA, 03/11/2026)

- Corretta la GUI **v14** a seguito del feedback dell'utente: edifici bloccati per prerequisiti **grigi nell'elenco ma sempre selezionabili**; costi e motivazioni leggibili nel dettaglio. Le righe solo prive di materiali mantengono aspetto normale.
- Il pulsante principale **Costruisci +1** ora occupa tutta una riga e usa `IsEnabled=CanAffordSelected`, distinto da `IsVisible=CanBuildSelected`; stato disabilitato grigio, hover attivo ocra senza movimento.
- Seconda riga dedicata a **−1 lavoratore / +1 lavoratore / +5 / +10**; lotti disabilitati esplicitamente se non pagabili. Pannello dettagli v14 ampliato a 410 px per non troncare le etichette.
- Rimossa la costruzione duplicata dalla scheda Scuola: rimangono iscrizioni e operazioni ricercatori; costruzione Scuola disponibile soltanto nella scheda Edifici.
- **5 nuovi casi automatici** (3 test + hover in 2 viewport), aggiornato 1 test della Scuola: **456 test attesi, non eseguiti qui**. RC7 non è stata confermata verde; ultimo run comunicato: **RC6 450/451** per l'asserzione Avalonia errata, corretta in RC7.
- Modificati esclusivamente presentazione/XAML v14 e test Desktop, più documentazione. **V13, stili globali, Core, Application, SQLite e salvataggi invariati.** [Rapporto D24](docs/design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md) · [Piano D24](docs/validation/G1_D24_RC8_TEST_PLAN.md) · [ADR 0034](docs/adr/0034-v14-stati-edifici-e-layout-azioni.md).

Il changelog registra **cosa è stato introdotto**, non approva automaticamente gameplay, grafica o valori numerici. Date riferite al **3 novembre 2026**. Il file storico precedente, conservato integralmente in [`docs/history/releases/CHANGELOG_STORICO_G1_D19.md`](docs/history/releases/CHANGELOG_STORICO_G1_D19.md), contiene i dettagli e i checkpoint delle versioni anteriori; non è la fonte dello stato corrente.

Legenda: **Validata** = test Windows confermati dall'utente; **Candidata** = codice prodotto ma verifica pendente; **Respinta visivamente** = test automatici insufficienti per accettare la GUI; **Documentazione** = nessuna modifica funzionale.

## v60-E3b4-RC7 / G1-D23 — P0 allineamento test Avalonia 12 (CANDIDATA, 03/11/2026)

- **Esito reale della RC6 su Windows, fornito dall’utente:** **451 test eseguiti, 450 superati, 1 fallito** (`V14UiParityRegressionTests.UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled`, riga 191). RC5 e RC6 sono dunque state esercitate insieme, **ma il gate complessivo non è verde**.
- **Causa verificata nel codice Avalonia 12.1.4:** il test leggeva `Button.IsEnabled` invece di `Button.IsEffectivelyEnabled`. `Button.IsEnabledCore` incorpora il risultato di `Command.CanExecute` nell'abilitazione effettiva, senza necessariamente modificare il valore `IsEnabled` impostato dall'utente. Prima dell'asserzione errata il test aveva già dimostrato che entrambi gli upgrade erano visibili, `CanUpgrade* == false`, `Command.CanExecute == false` e i costi riportavano «Mancano:».
- **Correzione circoscritta:** due asserzioni sull'abilitazione effettiva, più due controlli sull'identità dei comandi associati ai pulsanti. **Nessuna modifica** al ViewModel, simulazione, XAML, GUI v13, schemi DB, salvataggi o prezzi. Test count **invariato: 451**.
- **RC7 da validare** su Windows con `./validate.cmd`. Non dedurre un nuovo esito verde dai 450 test superati in RC6. Audit grafico v13/v14 ancora aperto.
- [Analisi causa D23](docs/design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md) · [Piano test D23](docs/validation/G1_D23_RC7_TEST_PLAN.md).

## v60-E3b4-RC6 / G1-D22 — P0 diagnostica produzione v14 (CANDIDATA, 03/11/2026)

- **RC5 non testata dall’utente:** a seguito dell’indicazione «non posso testare», prosegue lo sviluppo senza attribuire validazioni alla RC5. Ultima suite dichiarata verde: **RC4** (443 casi attesi; conteggio effettivo non comunicato).
- In Edifici v14 integrata la lettura diagnostica del **ProductionPlanner**: lavoratori produttivi/assegnati per filiera, input insufficienti, capacità di uscita, lotti e rimedi (Magazzino e, per Pane, Forno/Municipio; il Deposito alimentare non conserva Pane).
- Costi insufficienti per il prossimo acquisto e blocchi della produzione esistente sono mostrati insieme; addetti senza lavoro evidenziati. La misura di esecuzione è **aggregata Base/Grande/Avanzato**, non stimata per tier.
- **4 test Desktop** aggiunti: previsti **451** complessivi se discovery invariata. **Build, xUnit e audit Windows ancora pendenti**. Nessuna variazione a XAML, GUI v13, motore, catalogo, SQLite o regole economiche.
- [Rapporto D22](docs/design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md) · [Piano RC6](docs/validation/G1_D22_RC6_TEST_PLAN.md).

## v60-E3b4-RC5 / G1-D21 — P0 preventivi +5/+10 e upgrade (CANDIDATA, 03/11/2026)

- **RC4: test dichiarati passati dall'utente** (443 erano attesi; non è stato fornito il conteggio effettivo né il log). Non considerare chiuso l'audit grafico Windows.
- Ripristinati nella GUI v14 preventivi cumulativi progressivi per **Costruisci +5/+10**, con mancanze quantità disponibili/richieste e tooltip senza cambiare l'elenco Risorse.
- Corretto l'errore per cui un upgrade sbloccato scompariva quando mancavano risorse: pulsante **visibile ma disabilitato**, prerequisiti tecnologici mancanti ⇒ pulsante assente e messaggio.
- La UI usa un unico calcolo quotazione per i pulsanti dei lotti +1/+5/+10; la simulazione, la GUI v13 e SQLite v2/v3 non sono state modificate.
- **4 test Desktop aggiunti**, nuovo totale **447 attesi, NON eseguiti qui**. Analisi statica e XML verificabili separatamente dalla prova Windows.
- [Rapporto D21](docs/design/G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md) · [Piano RC5](docs/validation/G1_D21_RC5_TEST_PLAN.md).

## v60-E3b4-RC4 / G1-D20 — P0 diagnostica Pane (CANDIDATA, 03/11/2026)

- Isolata una regressione concreta della RC3: la v14 nascondeva il fabbisogno Pane prima del primo residente avanzato e mostrava solo una spiegazione generica per gli stati successivi.
- Collegato lo **stesso `BreadSupplyProjector`** alle letture della sessione v13 e v14; adattatore read-only per i tier v14 e formatter condiviso fra le due GUI. Stati: primo arrivo, capacità nominale, input, spazio di stoccaggio, produzione prevista e riserva.
- Aggiunti **4 test** (3 Application, 1 Desktop): totale previsto **443**. **Non eseguiti nell’ambiente di confezionamento RC4**; la suite è stata successivamente confermata **passata dall’utente** il 3/11/2026 (numero effettivo non comunicato). Audit grafico ancora aperto.
- Nessuna modifica a XAML, clock, schemi SQLite, filiere economiche, logica salvataggi o GUI classica salvo delega alla stessa formattazione testuale.
- La parità RC3/RC4 non è certificata: resta indispensabile la matrice manuale completa.
- [Rapporto tecnico D20](docs/design/G1_D20_E3B4_RC4_DIAGNOSTICA_PANE.md) · [Piano di prova RC4](docs/validation/G1_D20_RC4_TEST_PLAN.md).

## G1-D19 — Riordino documentale per nuova chat (DOCUMENTAZIONE, 03/11/2026)

- Consolidati `README.md`, `CHANGELOG.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/NEW_CHAT_HANDOFF.md`, `docs/START_HERE.md`, `docs/CURRENT_STATE.md`, `docs/README.md`, `docs/AUTHORITY_REGISTRY.md`.
- Distinti **427 test RC1 confermati**, regressioni RC2 osservate e **439 test RC3 solo attesi**.
- Formalizzato il blocco delle nuove funzionalità finché la GUI v14 non raggiunge la parità con v13, con matrice di controllo e gate di uscita.
- Archiviati senza cancellarli changelog, handoff, roadmap, registro decisioni e indice documentale precedenti in `docs/history/operational/`.
- **Zero modifiche** a `.cs`, `.axaml`, progetti, script, salvataggi o asset. Nessun test .NET eseguito o dichiarato superato per questo riordino.

## v60-E3b4-RC3 — Correzione delle regressioni GUI (CANDIDATA)

- Dopo il feedback dell'utente, recupero della presentazione Risorse a sette colonne con `ResourceRow`, quantità/capacità, flussi e tempo al riempimento; ripristino dei dati demografici e dei comandi Esercito (arruola/annulla).
- Edifici bloccati: pulsanti Costruisci **non visibili**; sbloccati ma senza materiali: azioni **disabilitate**, con motivi del blocco. Ripristino pulsanti +1/+5/+10 e upgrade distinti; nessuna ComboBox di livello.
- Migliorata la protezione da salvataggi sperimentali incompatibili e la chiusura della finestra.
- Nove nuovi test rispetto ai 430 previsti per RC2, totale **439 attesi**; compilazione/xUnit e ispezione GUI Windows **pendenti**.
- Gap noto: diagnostica approfondita Pane e dettagli grafici/hover/scroll da confrontare integralmente con la v13; XAML v14 ancora duplicato.
- [Rapporto D18](docs/design/G1_D18_E3B4_RC3_RIPRISTINO_FUNZIONI_GUI.md) · [Piano RC3](docs/validation/V60_E3B4_RC3_PARITY_TEST_PLAN.md) · [ADR 0033](docs/adr/0033-preservazione-funzionale-gui-v13-v14.md).

## v60-E3b4-RC2 — Riallestimento classico incompleto (RESPINTA VISIVAMENTE)

- Introdotti menu laterale e livelli produttivi come righe di edificio distinte, senza ComboBox nel dettaglio.
- **Regressioni segnalate dall'utente:** pagina Risorse semplificata, comandi Costruisci mostrati su edifici ancora bloccati, mancanza di altre funzioni già presenti nella vecchia GUI; Esercito non completo.
- +3 test su RC1, **430 attesi**. Il riordino non costituisce accettazione della RC2.
- [Rapporto D17](docs/design/G1_D17_E3B4_RC2_RIALLESTIMENTO_GUI_CLASSICA.md) · [ADR 0032](docs/adr/0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md).

## v60-E3b4-RC1 — Anteprima Avalonia separata (VALIDATA NEI TEST, GUI NON APPROVATA)

- Avvio esplicito `--experimental-v14`, SQLite v3 separato e sezione sperimentale per Scuola, tecnologia, produttori a livelli, civili.
- 10 test Desktop introdotti, **427 test passati su Windows secondo l'utente**.
- UI sperimentale iniziale giudicata non soddisfacente; l'esito verde xUnit **non equivale** a correttezza grafica o parità funzionale.
- [Rapporto D16](docs/design/G1_D16_E3B4_ANTEPRIMA_DESKTOP_V14.md) · [ADR 0031](docs/adr/0031-e3b4-gui-v14-opt-in.md).

## v60-E3b3-RC5 — Sessione v14 e replay (VALIDATA)

- Aggiunta costruzione civile (Case, Magazzino, Caserma) e confronto di due sessioni equivalenti con salvataggio/ripristino SQLite nel mezzo.
- **417/417 test confermati** dall'utente; valori di economia ancora sperimentali.
- [Rapporto D15](docs/design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md).

## v60-E3b3-RC4 — Livelli, costruzioni e upgrade (VALIDATA)

- Comandi v14 per costruzione diretta Base/Grande/Avanzato, upgrade, assegnazione/rilascio lavoratori, prezzi progressivi e verifiche su risorse e tecnologie.
- **408/408 test confermati** dall'utente. [Rapporto D14](docs/design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md).

## v60-E3b3-RC3 — Fixture orologio (VALIDATA)

- Corretta popolazione iniziale dei test del clock; vincolo Core contro duplicazioni di lavoratori/ricercatori/militari mantenuto.
- **396/396 test confermati** dall'utente. [Rapporto](docs/validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md).

## v60-E3b3-RC2 — Clock unico v14 (SOSTITUITA DA RC3)

- Avanzamento coerente di economia, crescita, addestramento e ricerca, ma **7 test fallivano** per dati della fixture: 389/396 superati su Windows. Corretti nella RC3.

## v60-E3b3-RC1 — Comandi Scuola, ricercatori e tecnologie (VALIDATA)

- Sessione v14 isolata, creazione Scuola, formazione, assegnazione ricercatori e acquisto volontario tecnologie; **387/387 test confermati**.

## v60-E3b2-RC3 — SQLite v3 con controllo studenti (VALIDATA)

- Controllo di integrità della coda di formazione in SQLite v3; **380/380 test confermati**.

## v60-E3b2-RC2 — Fix e pulizia directory principale (VALIDATA)

- Corretti confronti ordine dati SQLite nei test e test della posizione formativa eliminata; archiviati 25 file storici della root in `docs/history/`. **377 test superati** dopo la correzione.

## E1 / E2 / E3 / E3b/1 — Fondamenta v14 (IMPLEMENTATE, POI VALIDATE INDIRETTAMENTE)

- E1: prezzi progressivi per famiglia e saldo di ricerca liberamente accumulabile.
- E2: Base/Grande/Avanzato, lavoratori e capacità locali, upgrade separati.
- E3: Scuola, formazione e otto tecnologie produttive sperimentali.
- E3b/1: snapshot v14, conversione v13→v14 in memoria e controlli di consistenza.
- Modelli confluiti nel ramo della sessione v14 che ha superato gate successivi. **Non** significa che costi, tempi o coefficienti siano approvati.

## v59-RC1 e precedenti — Campagna storica (VALIDATA)

- Ruleset `civil-growth-13`, Municipio, produzione e consumi, Magazzino/Pane condivisi, abitazioni, filiera Pane, Ferro/Oro/Forgia/Zecca, Caserma e addestramento individuale.
- Ultima campagna normale conservata nel ramo sperimentale v60. Cronologia e risultati dettagliati nello [storico integrale](docs/history/releases/CHANGELOG_STORICO_G1_D19.md).

## Stato successivo

**Prima:** RC5 `./validate.cmd` e confronto visivo/funzionale con v13. **Poi:** eliminare le disparità rimaste e condividere componenti GUI senza cambiare il layout originale. **Solo dopo:** bilanciamento e approvazione quantitativi E4, migrazione sicura, nuova campagna; quindi navale M5.4 e altre milestone. [Roadmap dettagliata](docs/ROADMAP.md).
