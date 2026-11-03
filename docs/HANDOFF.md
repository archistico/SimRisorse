# Handoff tecnico completo — SimRisorse G1-D28

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** RC11 dichiarata verde dall'utente (**466 test attesi**, numero esatto non comunicato). RC12: ricerca a **due decimali**, pulsante «Acquista ricerca» disabilitato e grigio se saldo o prerequisiti insufficienti, verde con hover ocra solo quando acquistabile. Controllo prezzi/saldi sul valore **non arrotondato** in millipunti. **468 test attesi, non eseguiti** in questo ambiente privo di SDK .NET 10. GUI v13, core e SQLite invariati; audit visivo P0 ancora aperto. [Report RC12](design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).


**AGGIORNAMENTO G1-D27 / RC11 (03/11/2026):** l’utente conferma **test RC10 passati (464 attesi)**, senza comunicare un conteggio dettagliato. RC11 riordina il dettaglio Edifici della sola v14 in due righe: **Costruisci | +1 | +5 | +10** e **Lavoratore | +1 | +5 | +10**; il pulsante **−1** rimane compatto accanto alla label Lavoratore per non perdere la funzione di rilascio. Aggiunti comandi atomici per assegnare 5/10 persone, con limiti di popolazione libera e posti di lavoro, stati disabled espliciti e hover già previsto. **466 test attesi** (+2 casi), non eseguiti qui per assenza dell’SDK .NET 10; audit GUI P0 ancora aperto. [Rapporto RC11](design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Piano RC11](validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).


**AGGIORNAMENTO G1-D26 / RC10 (03/11/2026):** la compilazione Windows della RC9 si è interrotta con **CS0246** in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs` (riga 15): `GameScenario` è definito in `SimRisorse.Core.Content`, ma mancava il relativo `using`. **RC10 aggiunge esclusivamente tale import nel codice dei test.** Tutti gli altri progetti sono stati compilati nel log dell’utente; la suite xUnit RC9 non è partita. **Ultima suite verde: RC8, 456/456 test confermati. RC10: 464 test attesi, non ancora eseguiti; audit GUI P0 ancora aperto.** [Diagnosi RC10](design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](validation/G1_D26_RC10_TEST_PLAN.md).


**AGGIORNAMENTO G1-D25 / RC9 (03/11/2026):** **RC8 456/456 test superati, comunicato dall'utente.** RC9 rimuove «Esci senza salvare» in entrambe le UI; la tab Scuola segue l'Esercito con coda, formazione/progresso, ricercatori; introduce la risorsa **Matite** solo nella v14 (1 Legno lavorato + 1 Carbone → 4 Matite, fabbricazione manuale, 2 Matite + 10 Cibo per studente), persistenza v3 retrocompatibile e rimborsi non fittizi. **464 test attesi, NON eseguiti**; confronto visivo ancora aperto. [Rapporto RC9](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano RC9](validation/G1_D25_RC9_TEST_PLAN.md) · [ADR 0035](adr/0035-scuola-formazione-matite-v14.md).


**Aggiornato:** 3 novembre 2026 · **Pacchetto:** v60-E3b4-RC12 / G1-D28 · **Destinatario:** prossima chat / sviluppatore. **Ultimo gate confermato: RC11 (466 attesi); RC12 468 attesi, da provare su Windows.** La sezione successiva su RC8 è conservata come nota storica e non descrive più l'ultimo gate.

> **STORICO G1-D24 / RC8 (validata successivamente):** l'utente ha segnalato una regressione grafica in RC7: edifici non costruibili troppo simili ai disponibili, pulsante «Costruisci» tagliato/verde nonostante risorse insufficienti, pulsanti lavoratori/+5/+10 non raggruppati come richiesto, costruzione duplicata nel tab Scuola. RC8 attenua le sole righe bloccate, senza disabilitarne la selezione; separa Costruisci +1 a larghezza piena e una seconda riga per −1/+1 lavoratore, +5/+10; lega `IsEnabled` alla copertura delle risorse; elimina Costruisci dal tab Scuola ma lascia Scuola costruibile da Edifici. **456 test attesi, NON eseguiti**, audit Windows a 1040×740 e 1240×850 necessario. Ultima suite verde confermata RC4; ultima eseguita RC6 = 450/451, corretto controllo Avalonia nella RC7 ma RC7 non verificata. **Non avviare E4**. [D24](design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md) · [piano](validation/G1_D24_RC8_TEST_PLAN.md) · [ADR 0034](adr/0034-v14-stati-edifici-e-layout-azioni.md).

> **AGGIORNAMENTO STORICO G1-D23:** l’utente ha eseguito RC6 su Windows: **451 test, 450 superati, 1 fallito** (`V14UiParityRegressionTests.UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled`, riga 191). Il test stava controllando `Button.IsEnabled`, mentre Avalonia 12.1.4 applica `Command.CanExecute` a `Button.IsEffectivelyEnabled` tramite `IsEnabledCore`. RC7 sostituisce le due asserzioni e verifica il binding dei rispettivi comandi. **Nessuna modifica di runtime**; 451 test da ripetere su RC7, non considerare la suite verde finché non ci sarà conferma. GUI v13/v14 da confrontare manualmente. [Rapporto D23](design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md) · [Piano D23](validation/G1_D23_RC7_TEST_PLAN.md).

> **AGGIORNAMENTO STORICO G1-D22:** ultima suite dichiarata verde dall’utente **RC4**, 443 attesi senza numero esatto; **RC5 non testata** (447 attesi); **RC6 candidata** (451 attesi, non compilata/testata nell’ambiente corrente). La RC6 recupera per gli Edifici v14 diagnosi di input e capacità via `V14EconomyPreview.Buildings`: rendimento per **filiera aggregata** Base/Grande/Avanzato, rimedi magazzino e Forno/Pane, coesistenza con avvisi di prezzo. XAML, v13, Core e SQLite invariati. [Rapporto D22](design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md) · [piano RC6](validation/G1_D22_RC6_TEST_PLAN.md). La GUI non è ancora approvata visivamente.

> **AGGIORNAMENTO STORICO G1-D21:** l’utente ha dichiarato **superati i test RC4** (443 attesi, senza conteggio effettivo). RC5 espone i preventivi cumulativi +5/+10 e mantiene **visibili ma disabilitati** gli upgrade con ricerca sbloccata e materiali mancanti; aggiunti 4 test (**447 attesi, non eseguiti qui**). Il confronto grafico completo v13/v14 rimane aperto. [Rapporto RC5](design/G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md) · [piano RC5](validation/G1_D21_RC5_TEST_PLAN.md).

> **LEGGERE PRIMA DI MODIFICARE IL CODICE:** è prioritario **ripristinare integralmente la GUI originale v13 nella v14 sperimentale**. La v14 aggiunge Scuola, Albero tecnologico e nuove taglie degli edifici; **non deve sottrarre alcuna funzione** di Risorse, Edifici, Esercito, popolazione o diagnostica Pane. La RC2 è stata giudicata regressiva dall'utente; la RC3 è una correzione **da testare e verificare visivamente**, non un risultato già approvato.

## 1. Origine e stato esatto

- **Stack:** C#/.NET 10 (SDK `10.0.200`, `global.json`), Avalonia 12.1.4, SQLite, xUnit. Soluzione `SimRisorse.slnx`; moduli `Core`, `Application`, `Infrastructure`, `Desktop`, test e tool Simulation. Windows 11 è l'ambiente dell'utente.
- **Campagna stabile:** `civil-growth-13`; programma normale `./run-game.cmd`; salvataggio SQLite v2 nel profilo LocalAppData. Questo ramo va preservato come baseline visiva e funzionale.
- **Prototipo:** `--experimental-v14`, `civil-growth-14` (candidato), sessione `V14CampaignSession`, snapshot `V14CampaignSnapshot`, store SQLite v3, finestra `V14PreviewWindow`, stato `V14PreviewViewModel*`. Avvio volontario e DB separato; nessuna migrazione automatica delle partite.
- **Ultima suite automatica confermata dall’utente:** v60-E3b4-RC4 **test passati** (443 attesi; numero e log non comunicati). RC1 aveva già **427/427**; un test verde non significa layout approvato.
- **v60-E3b4-RC2:** riprogettazione visiva parziale, **respinta nel feedback**: elenco Risorse e relative colonne alterati, comandi Costruisci su edifici bloccati, funzionalità Esercito e dettagli mancanti; 430 casi erano solo il numero atteso.
- **v60-E3b4-RC3:** candidato correttivo: Risorse a sette colonne, Esercito, crescita, pulsanti, protezioni salvataggi; 439 test attesi prima dei nuovi test RC4/RC5. Il checkpoint G1-D19 era documentale.
- **v60-E3b4-RC4 / G1-D20:** diagnostica Pane condivisa v13/v14, +4 test, suite **dichiarata superata dall’utente** (443 attesi, conteggio effettivo sconosciuto). Non dichiarare parità visiva.
- **v60-E3b4-RC5 / G1-D21:** prezzi +5/+10 cumulativi in dettaglio, tooltip e cause dei pulsanti disabilitati; upgrade visibile ma disabilitato quando ricerca presente e scorte insufficienti. +4 test Desktop, **447 attesi allora, eseguiti successivamente insieme alla RC6**. Nessun cambiamento a motore, GUI v13 o salvataggi.
- **v60-E3b4-RC6 / G1-D22:** diagnosi produzione reale di ogni famiglia già costruita (input, output saturi, lotti, attività aggregata Base/Grande/Avanzato, indicazione magazzino), che si aggiunge alla diagnosi costi. +4 test Desktop; **451 test eseguiti su Windows, 450 passati, 1 fallito nel controllo `IsEnabled`**. Nessuna modifica a motore, XAML, GUI v13 o salvataggi.
- **v60-E3b4-RC7 / G1-D23:** sistemato **solo il test headless** sullo stato realmente interattivo del pulsante: verifica `Command` e `IsEffectivelyEnabled`. Conteggio **451**; esito non comunicato.
- **v60-E3b4-RC8 / G1-D24:** attenuazione dei bloccati, comando Costruisci per intera riga con `IsEnabled` esplicito, 4 pulsanti sulla riga successiva, Scuola senza pulsante costruzione duplicato. +5 casi: **456 attesi**, prova Windows e parità UX ancora aperte.
- **Prima della GUI:** E3b/3e RC5 ha superato 417 test; RC4 408, RC3 clock 396, SQLite RC3 380. Questi gate non autorizzano i coefficienti quantitativi della nuova economia.

## 2. Requisiti dell'utente (da trattare come vincoli)

**2.1 Interfaccia (priorità assoluta):** mantenere **identico disegno e comportamento** della vecchia GUI pixel art. Sidebar con **Edifici, Risorse, Esercito, Scuola, Albero tecnologico**. Un produttore Base/Grande/Avanzato è **una riga edificio distinta**, costruibile direttamente. Segheria, Segheria grande e Segheria avanzata NON sono un unico dettaglio con ComboBox. Gli upgrade sono operazioni separate. Nessuna ComboBox di taglia o lotto nel dettaglio.

**2.2 Risorse:** sette colonne originali **Risorsa | Quantità | Capacità | Produzione | Consumo | Saldo | Si riempie tra**; ordine alfabetico italiano; numeri allineati a destra; quantità in tabella arrotondata, dato preciso disponibile; `hh:mm:ss`, scrollbar non coperta, hover/selezione ocra; capacità con tooltip che scompone Municipio/Magazzino/capacità locali. **Pane non deve avere un inventario eccezionale**.

**2.3 Edifici:** righe bloccate visivamente grigie ma selezionabili; nel dettaglio **Costruisci +1 su una riga autonoma**, e **−1 lavoratore / +1 lavoratore / +5 / +10 su seconda riga**; stato disabled grigio e hover leggibile e immobile. Scuola costruibile da Edifici, non dal tab Scuola. Dettaglio compatto con costi, produzione, consumi e capacità, sempre con icona e font coerenti; righe selezionabili anche se bloccate. **Prerequisiti mancanti → pulsanti Costruisci assenti**, ma dettaglio consultabile. **Materiali insufficienti → pulsanti disabilitati**, con materiali richiesti/disponibili e causa specifica. Costruisci +1/+5/+10 e upgrade separato; lotti multiacquisto con prezzi cumulativi corretti; hover non sposta gli elementi.

**2.4 Popolazione, Scuola e ricerca:** case incrementano disponibilità e accelerano la crescita prevista; uno studente/ricercatore esiste nella popolazione e non può essere duplicato con lavoratori/militari. Scuola forma ricercatori; i punti **si accumulano senza preselezione** e si spendono volontariamente; nessun acquisto tecnologico automatico. Tecnologie senza effetti (Logistica/Navigazione) non acquistabili.

**2.5 Esercito e salvataggi:** la scheda Esercito completa della v13 deve restare accessibile e funzionante (Spade, Caserme, reclute, progressi, arruolamento e annullamento). Mai sovrascrivere salvataggi v13; backup e migrazione v14 solo esplicita. La GUI sperimentale deve potersi chiudere anche con DB non valido.

## 3. Decisioni non ancora approvate

Non chiamare «definitivi» i seguenti valori, anche se codificati nel prototipo: aumento +15% per nuova costruzione di una famiglia; moltiplicatori di costo 1 / 2,6 / 6,5; capacità locali +50 / +150 / +500; prezzo e disponibilità della Scuola, 120 secondi per studente, 10 Cibo, due posti ricercatori; 0,1 punto/s per ricercatore; costi delle otto tecnologie e loro prerequisiti. Sono dati di test e bilanciamento da sottoporre all'utente. Il requisito di dimensioni **1/3/10 lavoratori** e livelli come edifici separati è confermato.

Le proposte navali non sono implementate: esplorazione e conquista rimangono ambiti distinti; successi deterministici e assenza di perdite irreversibili iniziali sono orientamenti espressi in fase di analisi, non dettagli quantitativi ratificati.

## 4. Mappa dei file software e dei contratti

| Responsabilità | File / area |
| --- | --- |
| Vecchia GUI da mantenere | `src/SimRisorse.Desktop/Views/MainWindow.axaml` e `Presentation/MainViewModel.cs` |
| Modello Risorse originale | `src/SimRisorse.Desktop/Presentation/ResourceRow.cs` |
| Stili / pittogrammi comuni | `src/SimRisorse.Desktop/App.axaml` e `Presentation/PixelGlyph.cs` |
| Nuova GUI v14 opt-in | `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml[.cs]` |
| ViewModel v14 e adattamenti di parità | `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.cs`, `.Buildings.cs`, `.Parity.cs`, `.School.cs` |
| Avvio e profili | `src/SimRisorse.Desktop/App.axaml.cs`, `DesktopOptions.cs`, `Program.cs` |
| Sessione gameplay v13 | `src/SimRisorse.Core/Sessions/GameSession.cs` e Application dello scenario |
| Sessione gameplay v14 | `src/SimRisorse.Core/Sessions/V14CampaignSession.cs`, `V14CampaignSnapshot.cs` |
| Persistenza v13/v14 | `src/SimRisorse.Infrastructure/` (store v2 storico e `V14SqliteSaveStore` v3 isolato) |
| Test GUI e regressioni | `tests/SimRisorse.Desktop.Tests/V14PreviewTests.cs`, `V14UiParityRegressionTests.cs` |
| Validazione completa | `validate.cmd`, `scripts/validate.ps1` |

Nella RC3 **esiste ancora duplicazione tra viste v13 e v14**: prima completare il recupero delle funzioni, poi estrarre componenti XAML/VM condivisi *senza riprogettazione estetica*. Usare `ResourceRow` come precedente per la condivisione semantica dei dati.

## 5. Controlli da eseguire per primi (P0)

1. Estrarre il **pacchetto integrale RC9** in una nuova cartella Windows, con SDK .NET 10. Conservare lo ZIP RC8 (ultima versione con **456/456 test confermati**) e fare backup di entrambi i database personali prima della prima apertura v14.
2. Eseguire `./validate.cmd` e registrare numero di test e `.trx`; obiettivo **464/464**, che non è stato verificato in questo ambiente.
3. Seguire [piano RC9](validation/G1_D25_RC9_TEST_PLAN.md) e [rapporto D25](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md): laboratorio Matite, due Matite per studente, progressi, coda, qualificati, anteprima del materiale e rimborsi, salvataggio/ricaricamento e lettura di file SQLite v3 RC8 senza nuovo campo.
4. Avviare `./run-game.cmd` (v13) e `./run-game.cmd --experimental-v14` e confrontare Risorse, Esercito, Edifici/hover, Pane e Scuola a 1040×740 e 1240×850. Seguire la [matrice P0](validation/G1_D19_MATRICE_PARITA_GUI.md).
5. Verificare che il comando visibile «Esci senza salvare» non compaia più, che sia possibile chiudere una finestra anche con DB incompatibile e che il DB v13 **non** venga mai sovrascritto dalla v14.
6. **Non iniziare E4** finché test RC9 e parità visiva non saranno confermati; per le prossime modifiche produrre sempre ZIP integrale + test + ADR + stato/roadmap/changelog/handoff.

## 6. Fasi successive, dopo P0

**P1 — Parità definitiva e condivisione sicura:** verificare visivamente la diagnosi Pane RC4, preventivi/upgrade RC5 e diagnosi produzione RC6, eliminando ulteriori mancanze; audit dei componenti duplicati, template unificati solo quando preservano v13; test headless su colonne e pulsanti; controllo manuale Windows.

**P2 — Rifinitura v14:** Scuola/coda, ricercatori/ruoli, albero tecnologia con saldo e costi, tier come edifici singoli, prezzi cumulativi, messaggi di blocco.

**P3 — E4 anti-blocco:** scenari reali, stock/storage, costi crescenti, personale e Cibo/Pane, progressione tecnologica; benchmark con report misurati, nessun valore inventato.

**P4 — E4 decision gate e attivazione:** approvazione esplicita numeri, migrazione v13→v14 non distruttiva, rollback e compatibilità storica.

**P5 — M5.4 e successive:** Cantiere/Navale, esplorazione, conquista, Castello; sviluppare solo dopo fondazioni stabili.

Dettagli [Roadmap](ROADMAP.md), [decisioni](AUTHORITY_REGISTRY.md), [rapporto D18](design/G1_D18_E3B4_RC3_RIPRISTINO_FUNZIONI_GUI.md).

## 7. Norme di consegna e validazione

- Ogni incremento: **ZIP completo del progetto**, non solo patch; test xUnit e controlli statici, diff/scope, changelog, ADR se la decisione cambia, roadmap, handoff e istruzioni; root pulita.
- Non dire «test passati» per test solo scritti. Distinguere test riferiti dall'utente, controlli statici del pacchetto e audit grafico manuale.
- Non modificare salvataggi personali, screenshot/assets storici o la v13 per rifinire la v14 senza motivo esplicito. Non eliminare la storia: archiviare in `docs/history/`.
- Per problemi grafici, il criterio è la **parità osservata**, non la somiglianza dei colori. Per problemi di economia, il criterio è progressione raggiungibile e dati quantitativi verificati, non compilazione riuscita.

## 8. Come avviare il prossimo turno

Messaggio suggerito: «Prosegui SimRisorse dal pacchetto v60-E3b4-RC9 G1-D25. Leggi `docs/NEW_CHAT_HANDOFF.md`, `docs/AUTHORITY_REGISTRY.md`, `docs/ROADMAP.md` e `docs/validation/G1_D23_RC7_TEST_PLAN.md`. Prima valida la RC9 (RC8 ha superato 456 test) e completa la parità GUI v13/v14; non sviluppare E4 prima dei gate. Consegnami sempre ZIP integrale, test e documentazione.»

**Questo file sostituisce i precedenti handoff operativi come fonte corrente**. Lo storico resta in `docs/history/operational/` e `HANDOFF_HISTORY.md`; eventuali vecchie intestazioni «stato attivo» sono riferimenti storici.
