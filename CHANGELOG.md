## v60-E3b4-RC12 / G1-D28 — Albero tecnologico: punti e stato Acquista (CANDIDATA, 03/11/2026)

- **Gate RC11:** test dichiarati tutti passati dall’utente (**466 attesi**; numero effettivo non comunicato). La parità visiva P0 non è ancora approvata.
- **Punti ricerca:** saldo, velocità di produzione e costi ricerca mostrati con due decimali `it-IT` (`N2`); valore interno invariato in millipunti (precisione 1/1000).
- **Acquista ricerca:** corretto il comando che verificava solo la selezione. Ora rifiuta all'interfaccia tecnologie già acquistate, non implementate, prive di prerequisiti/edifici o con saldo insufficiente. Pulsante `IsEnabled` esplicito, grigio in disabled e verde soltanto quando acquistabile; hover ocra e geometria invariati.
- **Test:** 2 nuovi casi Desktop (saldo 99,999 vs costo 100,000, acquisto esatto e stato post-acquisto; stile Avalonia grigio/verde/hover), **468 attesi**, non eseguiti qui per assenza SDK .NET 10. Verificata staticamente struttura XAML e diff circoscritto.
- **Ambito:** solo presentazione/XAML v14, nuovo selettore di stile per il pulsante ricerca, test Desktop e documentazione. Campagna v13, motore, salvataggi SQLite e Scuola/Matite non modificati.
- [Rapporto G1-D28](docs/design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano test](docs/validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](docs/adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).

# Changelog — SimRisorse

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

Il changelog registra **cosa è stato introdotto**, non approva automaticamente gameplay, grafica o valori numerici. Date riferite al **3 novembre 2026**. Il file storico precedente, conservato integralmente in [`docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md`](docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md), contiene i dettagli e i checkpoint delle versioni anteriori; non è la fonte dello stato corrente.

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
- Ultima campagna normale conservata nel ramo sperimentale v60. Cronologia e risultati dettagliati nello [storico integrale](docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md).

## Stato successivo

**Prima:** RC5 `./validate.cmd` e confronto visivo/funzionale con v13. **Poi:** eliminare le disparità rimaste e condividere componenti GUI senza cambiare il layout originale. **Solo dopo:** bilanciamento e approvazione quantitativi E4, migrazione sicura, nuova campagna; quindi navale M5.4 e altre milestone. [Roadmap dettagliata](docs/ROADMAP.md).
