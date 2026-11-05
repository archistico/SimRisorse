# Handoff corrente — RC77-M01, misure naturali complete (05/11/2026)

**Misure concluse:** nove run naturali senza `--max-seconds`, **9/9 pass**, zero timeout/blocchi/diagnostiche. Pane 481s; Estrazione I 2520s (`bread-first`) o 2580s; Spade/Monete 4563s o 4623s. **21/21 file identici** fra replay e rianalisi. Manifest naturali aggiornati con dati effettivi; [rapporto RC77-M01](validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md), provenienza con SHA-256 di sorgenti e assembly nella cache. Nessun codice o coefficiente modificato.

**Sorgente:** workspace RC77. Gate completo eseguito da Codex su Windows/.NET SDK 10.0.201: **899/899 test verdi**, zero saltati, build senza errori o avvisi, smoke nativo esistente, audit alimentari e sei esperimenti controllati superati. [Esiti RC77](validation/E4A_V15_RC77_TEST_PLAN.md).

**RC77:** diagnostica opzionale del primo secondo dal clock reale e CLI `--benchmark-e4a-v15-recovery`; tre scenari controllati, due politiche ciascuno, 120s, inventari riconciliati e pagamenti reali. Registrati 240 Spade/360 Monete dopo un Magazzino e 238 Spade+357 Monete dopo due assegnazioni al Carbone. Checkpoint finanziati dichiarati; nessun dato di progressione naturale inventato. Quattordici nuove regressioni. GUI, prezzi, crescita ×20 e SQLite invariati.

**Richiesta corrente applicata:** Bovini/testa di mucca, Pane più marrone, Pescatore/pesce, Tagliapietre/cubo grigio. Intestazione v15 fissa Abitanti/Cibo/Legno lavorato/Pietra; pulsanti Carica/Salva senza suffisso. Crescita v15 autorizzata fino a ×20 con i bonus per casa precedenti: al massimo un arrivo ogni 3 secondi. Non ripristinare il limite ×2 per applicare le vecchie note sul congelamento del bilanciamento. Undici nuovi test; formato SQLite invariato.

**Correzioni HF03:** filtro filiere sopra le intestazioni anziché comprimerle, nomi italiani nei costi, testo dei materiali che va a capo nelle card Scuola/Esercito. Cinque test nuovi; il gate richiede otto catture v15, tutte ispezionate a 1040×740 e 1240×850.

La regressione confrontava lo stato iniziale (5 lavoratori liberi) con il campione dopo le assegnazioni al secondo zero (3 liberi). Il test ora applica le quattro azioni reali prima del confronto; nessuna modifica al motore.

**E4a diagnostica storica:** V15-I01 da 120s e relativa rianalisi restano evidenze della HF02. Sono seguite le misure naturali integrali RC77-M01 riportate sopra.

**Ripresa:** avviare RC78/E4b, studio delle tre curve candidate sulla base delle misure ora disponibili. Non applicare coefficienti non approvati. Nei run industriali 3540s sono attesa ricerca, 900/960s materiali e 120s formazione; il requisito futuro della Zecca a 10 addetti non è implementato. Il confronto interattivo Windows resta distinto e necessario per promuovere F1; il gate del codice è già verde (899 test). Ripeterlo quando cambiano codice o test, non per questi soli dati/documenti. Stress 24h non eseguito. Le sezioni sotto sono storiche.

---

# Aggiornamento handoff storico — RC76-HF02 (05/11/2026)

**Sorgente corrente:** archivio RC76-HF02 completo. Dopo la correzione CS0117 della RC76-HF01, il gate Windows ha rilevato 8 errori CS1061: manca `V15FoodIndustryCampaignSession.CityCatalog`, utilizzato dalla GUI v15 e da sette chiamate nei test. La proprietà è stata ripristinata nel Core come proiezione di `V15CampaignCatalogProjection.Project` calcolata dallo snapshot corrente, senza alterare GUI, logica, salvataggi o benchmark. **869 test attesi e non ancora verificati**. L'ultimo checkpoint validato resta RC63 (771 test) e la GUI RC63 è il contratto canonico, da non modificare. [Piano RC76-HF02](validation/RC76_HF02_CITY_CATALOG_TEST_PLAN.md).

**Prossimi passi:** `./validate.cmd` poi `./validate-f1.cmd` sulla RC76-HF02; risolvere eventuali ulteriori errori cumulativi prima di RC77/E4a.3. Il gate precedente si è fermato prima dei test; non dichiarare falliti i test di simulazione. Evitare qualsiasi redesign o sostituzione della GUI.

---

# Aggiornamento handoff — RC76-HF01 (05/11/2026)

**Codice da usare:** RC76-HF01 completo, non RC76 precedente. L'utente ha eseguito il gate su RC76 e trovato **due errori CS0117** in Application: i benchmark naturale e industriale chiamavano un metodo `BusyOutsideFood` visibile solo in Core. Hotfix: API pubblica `V15FoodCampaignCheckpoints.AvailableWorkers`, condivisa con la sessione v15; due regressioni aggiunte. **869 test attesi, non confermati**, compilazione .NET 10 indisponibile qui. RC63 resta l'ultimo checkpoint confermato (771 test). La GUI RC63 è il contratto canonico: non cambiarla.

**Priorità:** validare `./validate-f1.cmd` sulla hotfix. L'errore precedente blocca la compilazione prima di TRX/audit, quindi non prova che i benchmark o i test abbiano fallito. Seguono RC77/E4a.3, poi studio E4b su costi e squadre. [Piano RC76-HF01](validation/RC76_HF01_COMPILATION_TEST_PLAN.md).

---

# Handoff operativo — SimRisorse RC76 / E4a.2c

**Archivio di riferimento:** RC76, progetto integrale. **867 test cumulativi attesi; .NET 10 non eseguito.** Ultimo checkpoint confermato da utente: RC63 con 771/771 e GUI v14 ripristinata/approvata in v15. L'utente potrebbe non poter eseguire test per molte sessioni: continuare per incrementi piccoli, dichiarando candidati i risultati non verificati.

## Stato tecnico

La v15 ha filiere Stalla/Cacciatore/Macellaio/Orto/Frutteto/Deposito, Cibo esclusivo nel Deposito, Pane per case avanzate, stock distinti e personale condiviso con industria, Scuola e militari. Clock unico, capacità e flussi reali nella GUI, salvataggi SQLite gen. 6 separati da v14. Il `V15FoodCampaignCheckpoint` usa un modello civico nativo, i comandi e la validazione civici sono nativi, il planner non fabbrica più un `food-store` fittizio. F1-D05e non è chiusa prima del gate.

**GUI non negoziabile:** riferimento canonico RC63/GUI v14, ADR 0071: edifici bloccati attenuati ma consultabili, dettaglio edificio con elenchi e icone di costi, produzione, consumo, capacità; pulsanti costruzione/lavoratori dove già approvati; progresso abitante sotto sidebar; tabella Risorse a 7 colonne, Scuola ed Esercito. No ComboBox per tier, no grafica alternativa.

**Risorse e salvataggi:** stock al millesimo, pasti atomici, nessun doppio conteggio dei trasferimenti; Pane e Pesce possono trovarsi in compartimenti diversi. DB v14 personale conservato, DB v15 sperimentali non necessitano retrocompatibilità.

## Risultati incrementali

- RC63: GUI validata; **771 test confermati**.
- RC64–RC71: migrazione dei tipi civici alla v15, salvataggio e planner senza slot legacy; test cumulativi non ancora confermati.
- RC72: gate automatico `validate-f1.cmd` (TRX + audit + smoke), ancora da eseguire; 826 test attesi in quella release.
- RC73: runner naturale alimentare v15 `--benchmark-e4a-v15-natural`, V15-N01/N02/N03; 834 test attesi.
- RC74: analizzatore dei report naturali, costi parziali Legname lavorato/Pietra e classificazioni prudenti; 843 test attesi.
- **RC75:** runner avanzato con 3 scenari (V15-I01 Scuola/Estrazione I, V15-I02 Industria I + Spade, V15-I03 Industria I + Monete) e due politiche ciascuno (`research-first` e `bread-first`): la Scuola può essere costruita prima o dopo Pane/Carbone, ma servono **2 Matite a studente** fabbricate con Legname lavorato e Carbone per iscrivere ricercatori. Ledger di **tutti i materiali effettivamente pagati** dalle azioni, punti ricerca spesi a parte, campioni sulla popolazione e sui ricercatori, analisi/CSV/Markdown con nessun tempo inventato. **+14 test, totale 857 attesi.** Nessuna modifica a GUI/Core/SQLite/bilanciamento. Manifest dei sei abbinamenti **`not-run`**.

## Comandi

```powershell
.\run-game.cmd
.\run-game.cmd --v14
.\validate.cmd
.\validate-f1.cmd

dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-industry --scenario V15-I01 --policy research-first --max-seconds 120 --output .local-cache/e4a-v15/industry-diagnostic-001

dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-industry --output .local-cache/e4a-v15/industry-measurement-001
```

Le CLI avanzate **non** accedono ai database personali; output nuovi senza sovrascrittura. `--max-seconds` abbrevia l'orizzonte e rende la misura *diagnostica*. Un timeout o blocco di uno script non dimostra un deadlock strutturale. Nessun benchmarking a orizzonte completo eseguito in questa consegna.

## Direzione di sviluppo

1. **Gate F1** da eseguire appena possibile: attesi 867 test e audit verdi, più ispezione grafica manuale. Correggere eventuali regressioni prima di promuovere il checkpoint.
2. **RC77 / E4a.3:** quantificare la produzione persa per saturazione e sperimentare politiche di recupero controllate; RC76 misura già i deficit di acquisto e la pressione sulle capacità, non ancora la causalità dello stallo.
3. **E4b:** studio delle curve quasi esponenziali; Zecca con costo di fine progressione indicativamente confrontabile con 100–1.000 Tagliapietre e **almeno 10 lavoratori per Zecca** (2 Zecche = 20); nessuna produzione/consumo sotto squadra minima. I coefficienti sono ancora da determinare e approvare, non modificarli ora.
4. Più avanti M5.4–M7 navale, esplorazione vs conquista deterministica, Castello, obiettivi/vittoria.

- **RC76:** telemetria per deficit dei tentativi, scorte/capacità cittadine e analizzatore dei vincoli con output JSON/CSV/Markdown; +10 test, totale 867 attesi. Gli scenari sono ancora `not-run`, nessuna modifica a GUI, Core, SQLite, prezzi o ricette.

**Documenti prioritari:** [stato](CURRENT_STATE.md), [roadmap](ROADMAP.md), [ADR GUI](adr/0071-gui-v14-contratto-canonico-v15.md), [spec RC75](design/E4A_V15_RC75_INDUSTRIA_RICERCA.md), [test RC75](validation/E4A_V15_RC75_TEST_PLAN.md), [ADR 0083](adr/0083-benchmark-industria-ricerca-v15-senza-grants.md), [scenari industriali](../benchmarks/e4a-v15/industrial-scenarios.csv).
