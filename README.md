<!-- RC40 F1-D01: nuova architettura alimentare in sviluppo, non ancora attiva nel gameplay. -->
> **F1-D01 / RC40:** approvata la riforma alimentare (Pesce, Pane, Carne, Frutta, Verdura → Deposito alimentare → Cibo pronto). Aggiunto il modello **Core isolato** con prove di capacità e riserva Pane; le nuove filiere NON sono ancora costruibili nella partita e le regole RC39 restano identiche. [Roadmap](docs/ROADMAP.md) · [Specifica](docs/design/F1_D01_RC40_MODELLO_ALIMENTARE.md). Baseline Windows RC38 541 test verdi; RC40 552 attesi, da validare.

# Aggiornamento RC39 / E4a-D02 (2026-11-04)

**Ultimo checkpoint confermato:** RC38 con **541 test passati**. **RC39** aggiunge 4 test (**545 attesi, non ancora eseguiti**). La v14 resta il gioco predefinito. Il benchmark E4a-D02 per scenari **naturali E4A-01…05** parte dalle risorse e dalla popolazione della partita nuova, senza fondi artificiali; produce JSON/CSV e distingue successi, timeout e blocchi.

```powershell
.\validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --scenario E4A-01 --output .local-cache/e4a/natural-01-run1
```

**Nessun risultato economico è stato ancora misurato con RC39.** [Rapporto E4a-D02](docs/design/E4A_D02_RC39_BENCHMARK_NATURALI.md) · [Piano test](docs/validation/E4A_D02_RC39_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

## RC38 — Correzione test SQLite (2026-11-04)

Dopo il riscontro Windows **540/541 sulla RC37**, corretto un test end-to-end che confrontava gli edifici per posizione anziché per identificativo. **Nessuna modifica al gioco**: RC38 mantiene **541 test attesi, ancora da eseguire**. Vedi [rapporto F1](docs/design/E4A_D01_RC38_FIX_ORDERING_SQLITE.md) e [piano di validazione](docs/validation/E4A_D01_RC38_TEST_PLAN.md). Avvio ordinario v14 invariato.

# Aggiornamento RC37 / E4a-D01 (2026-11-04)

La GUI **v14 rimane predefinita**; v13 recuperabile con `--legacy-v13`. È pronto il primo benchmark **controllato E4A-06/07**, per misurare due studenti con una oppure due Scuole, con una fixture dichiarata. Nessun costo, consumo, gameplay, file SQLite o schema è cambiato.

```powershell
.\validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-001
```

**Attenzione:** RC37 è una candidata, **541 test attesi non eseguiti**; ultima suite Windows confermata RC30 (511 test). Nessun risultato di benchmark è stato ancora generato. [Rapporto E4a](docs/design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Piano test](docs/validation/E4A_D01_RC37_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# SimRisorse — RC36 / P1-D50: consolidamento Scuola e Ricerca

**RC36 candidata: 538 test .NET attesi, non eseguiti in questo ambiente.** RC30 (511) resta l'ultima validazione Windows confermata; le RC31–RC35 sono incluse cumulativamente. Due nuovi test verificano un salvataggio SQLite pre-Matite con studente già in corso e il rifiuto di un checkpoint con rimborsi impossibili. Preparati i benchmark E4a, **senza misurazioni o modifiche al bilanciamento**. GUI v14 predefinita, v13 legacy preservata.

[Rapporto D50](docs/design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md) · [Piano RC36](docs/validation/P1_D50_RC36_TEST_PLAN.md) · [Protocollo E4a](docs/design/E4A_PROTOCOLLO_BENCHMARK.md) · [Manifest](benchmarks/e4a/scenarios.csv) · [Roadmap](docs/ROADMAP.md) · [ADR 0047](docs/adr/0047-gate-p1-e-misure-e4a.md)

---

# SimRisorse — RC35 / P1-D49: integrazione Scuola → Ricerca → Edifici

**RC35 candidata, 536 test attesi e non eseguiti qui.** RC30 (511) rimane l'ultimo checkpoint Windows confermato; RC31–RC34 sono incluse nella verifica cumulativa. Aggiunti tre test integrati su comandi veri e SQLite v3: Matite, formazione, qualifica, ricerca, acquisto Lavorazioni I, upgrade e ripresa in pausa. **Nessuna modifica al gioco, alla GUI, al bilanciamento o allo schema dei salvataggi.**

[Rapporto P1-D49](docs/design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Piano RC35](docs/validation/P1_D49_RC35_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [ADR 0046](docs/adr/0046-catena-integrata-scuola-ricerca-tier.md)

---

# SimRisorse — RC34 / P1-D48: Albero tecnologico con benefici verificati

**RC34 candidata, 533 test attesi non ancora eseguiti su Windows.** RC30 (511) è l'ultima validazione confermata. Il catalogo ricerca v14 viene confrontato con gli sblocchi reali Grande/Avanzato e la pagina Tecnologia indica esplicitamente gli edifici sbloccati. Restano invariati bilanciamento, SQLite e GUI v13; le RC31–RC33 sono comprese nel pacchetto.

[Rapporto D48](docs/design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Piano RC34](docs/validation/P1_D48_RC34_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [ADR 0045](docs/adr/0045-tecnologie-effetti-verificati.md)

---

# SimRisorse — RC33 / P1-D47: filiera Matite

**Ultimo gate Windows confermato:** RC30 (511 test). **RC33 candidata:** 524 test cumulativi da verificare; include le RC31 e RC32. Il laboratorio Matite usa ora un'unica verifica dominio/UI, con suggerimenti quantitativi sui lotti e senza cambiare ricette o salvataggi. La v14 resta predefinita; v13 accessibile solo con `--legacy-v13` e database separato.

[Rapporto D47](docs/design/P1_D47_RC33_FILIERA_MATITE.md) · [Validazione RC33](docs/validation/P1_D47_RC33_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [ADR 0044](docs/adr/0044-unica-verifica-laboratorio-matite.md)

---

# SimRisorse — RC32 / P1-D46: avanzamento Scuola

**RC30:** ultimo checkpoint Windows confermato dall'utente (511 test previsti). **RC31:** v14 predefinita, 515 test attesi, ancora da verificare su Windows. **RC32:** riepilogo dei progressi individuali degli studenti in formazione, 3 test aggiunti (**518 cumulativi attesi**). Nessuna modifica ai tempi, ai costi, al motore o alla persistenza.

[Rapporto D46](docs/design/P1_D46_RC32_CICLO_FORMAZIONE_SCUOLA.md) · [Piano test RC32](docs/validation/P1_D46_RC32_TEST_PLAN.md) · [ADR 0043](docs/adr/0043-dettaglio-individuale-formazione-scuola.md) · [Roadmap](docs/ROADMAP.md)

La v14 rimane predefinita (`.\run-game.cmd`), la v13 e' disponibile con `--legacy-v13` e conserva il proprio database. Da verificare su Windows con `.\validate.cmd`.

---

# SimRisorse — RC31 / G1-D45: GUI v14 predefinita

**RC30 confermata dall'utente (511 test previsti).** La nuova GUI v14 è stata approvata per l'uso ordinario: `.\run-game.cmd` la apre senza parametri. La vecchia GUI rimane accessibile con `.\run-game.cmd --legacy-v13`, ma **nessun salvataggio è spostato o convertito**. **RC31** introduce quattro test di regressione (515 attesi): compilazione e verifica Windows ancora pendenti.

[Avvio e comandi](#modalità-di-avvio-rc31) · [Stato corrente](docs/CURRENT_STATE.md) · [Roadmap](docs/ROADMAP.md) · [ADR 0042](docs/adr/0042-promozione-v14-e-conservazione-slot.md) · [Piano RC31](docs/validation/G1_D45_RC31_TEST_PLAN.md)

> I resoconti delle release precedenti sotto questa linea sono **storico**: in particolare le descrizioni «v13 modalità normale» e «P0 condizione sospensiva» sono superate dalla decisione RC31. Il dossier delle 44 prove visive P0 rimane senza screenshot reali, distinto dall'approvazione dell'utente.

---

# SimRisorse — RC30 / G1-D44 (strumenti P0, candidata)

**Ultima baseline confermata:** RC27, **507 test superati**. **RC28–RC30:** **511 test .NET attesi, non ancora verificati su Windows**. RC30 corregge due falsi positivi nello strumento di raccolta delle evidenze visive (file PNG non validi e 44 righe NA) e aggiunge una verifica PowerShell autonoma. **Applicazione, GUI, test xUnit e salvataggi invariati; P0 visivo 0/44, ancora aperto.**

[Rapporto G1-D44](docs/design/G1_D44_RC30_RAFFERZAMENTO_EVIDENZE_P0.md) · [Piano test RC30](docs/validation/G1_D44_RC30_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

---

# SimRisorse — RC29 / G1-D43 (strumenti audit, candidata)

**Ultima suite confermata:** RC27, 507 test. **RC28/RC29:** 511 test cumulativi attesi, non verificati. In RC29 l'applicazione è identica a RC28: è stato introdotto solo `scripts/p0-ui-evidence.ps1` per registrare 44 verifiche visive v13/v14 (22 voci × 2 dimensioni), con report HTML locale, senza toccare gameplay, test, salvataggi o grafica. **P0 visuale resta aperto: 0/44 prove eseguite qui.**

[Rapporto G1-D43](docs/design/G1_D43_RC29_AUDIT_VISIVO.md) · [Istruzioni e checklist](docs/validation/G1_D43_RC29_PIANO_EVIDENZE.md) · [Roadmap](docs/ROADMAP.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

---

# SimRisorse — RC28 / G1-D42 (candidata)

**Checkpoint Windows confermato:** RC27, **507 test superati**. RC28 introduce il marcatore del focus per la tastiera nella sola GUI v14, con 4 nuovi test headless (**511 attesi; non eseguiti qui**). La GUI classica v13, la simulazione e i salvataggi rimangono invariati. Il confronto grafico delle 22 voci P0 è ancora aperto.

[Roadmap operativa](docs/ROADMAP.md) · [Rapporto](docs/design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano validazione RC28](docs/validation/G1_D42_RC28_TEST_PLAN.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

---

# SimRisorse — RC27 validata, nuova roadmap G1-D42

**RC27 / G1-D41:** **507 test passati** (conferma dell'utente). Questa consegna **aggiorna solo la documentazione**, non il codice, i test, la grafica, SQLite o il bilanciamento; non si tratta di RC28. La [roadmap ufficiale G1-D42](docs/ROADMAP.md) definisce P0 (focus, prova visiva di 22 voci, correzioni e gate), P1 Scuola/tecnologie, E4a misurazione, E4b bilanciamento approvato, E4c migrazione sicura e M5.4–M7. [Runbook P0](docs/validation/G1_D42_P0_RUNBOOK.md). **P0 visivo ancora aperto.**

---

# SimRisorse RC27 / G1-D41 — Scorrimento dettaglio Edifici (candidata)

**RC26 confermata dall’utente su Windows: 504 test superati.** RC27 aggiunge 3 test Desktop (totale **507 attesi**, da eseguire) e corregge la navigazione del dettaglio v14: cambiando edificio, testo principale e diagnosi partono dall’alto; aggiornare lo stesso edificio non cancella la posizione di lettura. Nessun cambio di grafica, focus forzato, economia, SQLite o GUI v13. **Gate P0 visivo ancora aperto.**

[Rapporto D41](docs/design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Test Windows](docs/validation/G1_D41_RC27_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Matrice P0](docs/validation/G1_D39_P0_UI_MATRIX.md).

---

# SimRisorse RC26 / G1-D40 — Audit Risorse e Popolazione (candidata)

**Baseline Windows confermata:** RC25, **501 test superati** (comunicazione dell’utente). **RC26:** +3 test Desktop, **504 attesi**, esecuzione .NET 10 ancora pendente. Ripristinati nella **sola presentazione v14** il progresso frazionario e la stima `~Ns` della crescita accelerata come nella GUI classica. Aggiunti casi per case piene, tick a velocità ×1,15 e durata di riempimento basata sul saldo netto. Nessuna modifica a v13, motore, SQLite, ricette, prezzi o grafica XAML. **P0 resta aperto** fino al confronto visivo delle 22 verifiche.

[Rapporto D40](docs/design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Piano Windows](docs/validation/G1_D40_RC26_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Matrice P0](docs/validation/G1_D39_P0_UI_MATRIX.md).

---

# SimRisorse — RC25 / G1-D39-F1 (candidata)

**RC20 è l'ultimo gate Windows confermato (490 test).** RC24 non ha compilato per due errori CS1061 (`MouseMove`): mancava `using Avalonia.Headless;` nel test degli upgrade. **RC25** applica solo quell'import, preserva i **501 test attesi** e deve ancora essere compilata/testata su Windows. P0 visivo ancora aperto, E4 sospeso.

[Rapporto fix](docs/design/G1_D39_RC25_FIX_CS1061_HEADLESS.md) · [Piano Windows](docs/validation/G1_D39_RC25_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md) · [Matrice P0](docs/validation/G1_D39_P0_UI_MATRIX.md).

---

# SimRisorse — RC24 / G1-D39 (candidata)

**Ultima versione con test Windows confermati:** RC20, 490/490. **RC24:** 501 test attesi (include RC21–RC23 ancora senza validazione); corregge i suggerimenti Scuola durante «Nuova partita?» e aggiunge una matrice di chiusura P0, **non** dichiara superato il gate grafico. La GUI v13 e i dati SQLite restano invariati.

[Stato](docs/CURRENT_STATE.md) · [Matrice operativa P0](docs/validation/G1_D39_P0_UI_MATRIX.md) · [Rapporto D39](docs/design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Piano test](docs/validation/G1_D39_RC24_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# SimRisorse — RC23 / G1-D38

**RC20 ultimo gate Windows confermato (490 test)**; RC21/RC22 non testate; **RC23 candidata con 499 test attesi**. D38 aggiunge tre controlli di navigazione e selezione v14 senza modificare motore, XAML, GUI v13 o salvataggi. P0 grafico resta aperto; E4 ancora sospeso. [Rapporto D38](docs/design/G1_D38_RC23_AUDIT_NAVIGAZIONE_SELEZIONE.md) · [Piano test](docs/validation/G1_D38_RC23_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# SimRisorse — RC22 / G1-D37

**RC20:** ultimo checkpoint Windows dichiarato verde (490 test). **RC21 non testata:** 493 attesi. **RC22 candidata:** 496 test attesi (+3). Nel dettaglio Edifici v14 la diagnostica lunga ha ora scorrimento verticale indipendente (massimo 104 px), così le due righe di comandi rimangono leggibili. Aggiunti due test layout e un test per progressi/addestramento Esercito; nessuna modifica al motore o alla GUI v13. **Audit grafico P0 ancora aperto**, E4 sospeso. [Rapporto D37](docs/design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano RC22](docs/validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# SimRisorse — RC21 / G1-D36

**RC20:** 490 test attesi, dichiarati superati dall'utente. **RC21:** 493 test attesi (3 test nuovi), compilazione/xUnit Windows pendenti. Nel dettaglio Edifici v14 i due pulsanti **Potenziamento** usano ora lo stesso stile di Costruisci: verde se pagabili, grigio se non pagabili, hover ocra senza movimento; costi e requisiti restano visibili. Invariati motore, salvataggi SQLite e GUI classica v13. [Rapporto D36](docs/design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md) · [Piano RC21](docs/validation/G1_D36_RC21_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md). **P0 grafico ancora aperto**.

---

# SimRisorse — RC20 / G1-D35

**Ultimo gate Windows confermato:** RC16, **476 test superati**. RC17 (481), RC18 (484), RC19 (487) **non sono state testate**. **RC20** aggiunge tre casi di regressione alla tabella Risorse v14, per **490 test attesi**, senza cambiare l'applicazione. La GUI classica e la tabella a sette colonne restano immutate. La verifica visiva P0 è ancora aperta; E4 sospeso.

[Rapporto D35](docs/design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano Windows RC20](docs/validation/G1_D35_RC20_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# SimRisorse — RC19 / G1-D34

**RC16:** 476 test confermati. **RC17:** 481 attesi, non testata. **RC18:** 484 attesi, non testata. **RC19:** **487 test attesi**, non eseguiti qui. La RC19 include RC17+RC18 e corregge i **suggerimenti Esercito**: stato durante «Nuova partita?», costi e carenze numeriche per i lotti, singolare «1 recluta». Nessuna modifica a comandi, GUI v13, motore, database o bilanciamento. **Audit grafico P0 ancora aperto.**

[Rapporto D34](docs/design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano test RC19](docs/validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap ufficiale](docs/ROADMAP.md).

---

# SimRisorse — RC18 / G1-D33

**Baseline confermata:** RC16, 476 test superati. **RC17:** 481 attesi, non ancora testati. **RC18:** 484 test attesi (+3), non ancora testati. La nuova versione include integralmente la RC17 e corregge un suggerimento nella Scuola: se scegli +5/+10 ricercatori ma i posti liberi non bastano, il messaggio mostra il limite effettivo invece di indicare erroneamente che puoi assegnarli. I comandi e le regole di simulazione non cambiano. **Gate grafico P0 aperto**, E4 sospeso. [Rapporto RC18](docs/design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Piano test](docs/validation/G1_D33_RC18_TEST_PLAN.md).

---

# SimRisorse — RC17 / G1-D32

**Baseline Windows confermata:** RC16 (476 test superati dall'utente). **RC17 candidata:** 481 test attesi, non ancora compilati/eseguiti su Windows. Il flusso v14 **Nuova partita → Avvia nuova** non sovrascrive più automaticamente il checkpoint precedente: il nuovo gioco viene salvato soltanto premendo **Salva v14**; poi riprende l'autosalvataggio. Durante la conferma, **Salva/Carica** sono disabilitati; la chiusura della finestra conserva la colonia precedente. Nessun cambiamento a GUI v13, Core, SQLite o bilanciamento. **Gate grafico P0 aperto.** [Rapporto RC17](docs/design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano RC17](docs/validation/G1_D32_RC17_TEST_PLAN.md) · [ADR 0039](docs/adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md).

## Contesto precedente — RC16 / G1-D31

**RC16 confermata successivamente dall’utente:** 476 test passati. D31 blocca durante la conferma «Nuova partita?» tutte le azioni che possono modificare la colonia (Edifici, Scuola/Matite, Esercito e ricerca), oltre al pulsante Pausa/Riprendi già corretto in RC15. Gate grafico P0 ancora aperto, senza E4 o navale. [Rapporto RC16](docs/design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](docs/validation/G1_D31_RC16_TEST_PLAN.md).

**Storico RC14 / G1-D29-F1 (03/11/2026):** RC12 era già stata dichiarata verde (**468 test attesi**). Avviata la roadmap in [sei fasi](docs/ROADMAP.md); RC13 ha introdotto la correzione del tooltip della capacità **Matite** in base agli edifici realmente posseduti, e aggiunge 2 test (**470 attesi**); RC13 non compilava per 2 errori CS0136, **corretti in RC14** rinominando due variabili locali. RC14 testata: 470 dichiarati passati; RC15 ora con test dichiarati passati; RC16 da compilare e testare. La **parità grafica P0 v13/v14 non è approvata**; E4 e navale restano sospesi. [Audit D29](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano test RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

Gioco gestionale di risorse e popolazione in **C# / .NET 10**, interfaccia desktop **Avalonia 12** in stile pixel art retro 8-bit, persistenza **SQLite**, test **xUnit**.

## Modalità di avvio (RC31)

**La GUI v14 è ora quella predefinita**, in seguito all'approvazione comunicata dall'utente dopo RC30. Le 44 prove visive del precedente dossier P0 non sono state raccolte e non vengono considerate superate.

| Modalità | Comando | Salvataggio |
| --- | --- | --- |
| **Gioco normale v14** | `.\run-game.cmd` | SQLite v3, slot v14 già esistente |
| Avvio v14 esplicito | `.\run-game.cmd --v14` | Stesso slot v14 |
| Compatibilità vecchie scorciatoie | `.\run-game.cmd --experimental-v14` | Stesso slot v14 |
| GUI classica v13 | `.\run-game.cmd --legacy-v13` | SQLite v2, distinto |
| Scenario storico v13 | `.\run-game.cmd --scenario economy-foundation` | Slot v13 storico |

La CLI `--scenario civil-growth` e i controlli `--smoke-test` restano **legacy** per compatibilità con gli script pubblicati. Per selezionare sempre la v14 usare l'avvio senza argomenti oppure `--v14`.

## Come avviare e validare

Su Windows con SDK .NET 10 e dipendenze ripristinabili:

```powershell
.\validate.cmd
.\run-game.cmd
```

La **stessa partita v14** delle release precedenti rimane in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`. La cartella `experiments` non viene rinominata per non perdere la continuità con i salvataggi esistenti. La v13 utilizza `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`; `economy-foundation` utilizza `game.sqlite3`. Nessuna migrazione o sovrascrittura automatica fra v13 e v14. Prima di test di migrazione o manipolazioni manuali, effettuare backup con l'app chiusa.

**Validazione:** RC30 dichiarata superata (511 test attesi); RC31 include 4 nuovi test e deve essere ancora verificata (515 attesi). [Roadmap](docs/ROADMAP.md) · [ADR 0042](docs/adr/0042-promozione-v14-e-conservazione-slot.md) · [Piano di prova](docs/validation/G1_D45_RC31_TEST_PLAN.md).

## Contratto dell'interfaccia

La **GUI v14 è la modalità ordinaria**, mantenendo lo stile funzionale della v13 come riferimento storico e possibilità di recupero. Nella barra laterale della v14 si aggiungono **Scuola** e **Albero tecnologico** senza eliminare Edifici, Risorse ed Esercito. La tabella Risorse resta a **sette colonne**, con ordinamento, capacità, tooltip e allineamenti storici; Esercito mantiene i comandi di addestramento. Ogni livello di produttore è un **edificio distinto** nell'elenco (es. Segheria / Segheria grande / Segheria avanzata): niente ComboBox di livello nei dettagli. Costruzioni +1/+5/+10, upgrade separati, prerequisiti e motivi di blocco chiari. Gli edifici bloccati sono grigi ma selezionabili; nel dettaglio v14 due righe:
`Costruisci | +1 | +5 | +10` (stato grigio se non pagabile, hover ocra se attivo) e
`Lavoratore | +1 | +5 | +10` (disabilitazione quando mancano abitanti/posti). Un `−1` compatto resta accanto alla label Lavoratore per liberare personale. La Scuola si costruisce esclusivamente da Edifici; il tab Scuola è riservato a formazione, ricercatori e laboratorio Matite, senza pulsante Costruisci. «Esci senza salvare» non compare più nelle due interfacce. **Non riprogettare le schede già funzionanti.**

La ricerca si **accumula liberamente** e si spende su decisione del giocatore. Nell’Albero tecnologico i punti sono mostrati con due decimali; «Acquista ricerca» è grigio se non acquistabile, con verifica esatta del saldo prima dell’acquisto. I coefficienti economici, prezzi e tempi della Scuola restano **candidati** finché non saranno espressamente approvati attraverso il bilanciamento.

## Documentazione di riferimento

- [Ripresa rapida](docs/START_HERE.md) e [handoff completo per nuova chat](docs/HANDOFF.md)
- [Prompt di passaggio a nuova chat](docs/NEW_CHAT_HANDOFF.md)
- [Stato validazione corrente](docs/CURRENT_STATE.md)
- [Roadmap dettagliata e gate](docs/ROADMAP.md)
- [Decisioni approvate / candidate](docs/AUTHORITY_REGISTRY.md)
- [Changelog consolidato](CHANGELOG.md) e [storico integrale precedente](docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md)
- [Matrice di parità GUI](docs/validation/G1_D19_MATRICE_PARITA_GUI.md), [rapporto RC6 diagnostica produttiva](docs/design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md), [piano RC6](docs/validation/G1_D22_RC6_TEST_PLAN.md), [piano RC5](docs/validation/G1_D21_RC5_TEST_PLAN.md) e [correzione test RC7](docs/validation/G1_D23_RC7_TEST_PLAN.md), [nuovo piano RC8](docs/validation/G1_D24_RC8_TEST_PLAN.md), [piano RC9](docs/validation/G1_D25_RC9_TEST_PLAN.md), [rapporto RC9](docs/design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) e [rapporto D24](docs/design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md)

[RC11: layout e batch lavoratori](docs/design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [piano test RC11](docs/validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](docs/adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).

**Ordine di sviluppo:** prima correggere e validare la **parità GUI**; poi audit del bilanciamento e migrazioni; infine riprendere il navale. Ogni incremento di codice richiede ZIP integrale, test, documentazione e stato esplicito.

[RC12: acquisto ricerca e due decimali](docs/design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano RC12](docs/validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](docs/adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).

[RC14: diagnosi e fix CS0136](docs/design/G1_D29_RC14_FIX_CS0136.md) · [Validazione RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md).
