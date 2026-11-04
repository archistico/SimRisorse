**G1-D43 / RC29 (04/11/2026):** predisposta la raccolta strutturata di 44 evidenze (22 ID × 1040×740 e 1240×850), con report HTML e controlli di completezza; **0/44 prove visive eseguite, P0 APERTO**. Nessuna riga della matrice è stata dichiarata PASS. Vedere [piano G1-D43](G1_D43_RC29_PIANO_EVIDENZE.md) e [rapporto statico](../design/G1_D43_RC29_AUDIT_VISIVO.md). RC27 verde 507 test; RC28/RC29 hanno 511 test attesi, ancora da eseguire su Windows.

**Aggiornamento G1-D42 / RC28 (04/11/2026):** RC27 confermata verde su Windows (**507 test**). RC28 aggiunge 4 esecuzioni headless di focus/tastiera e stili visivi **soltanto v14**; **511 test attesi, non ancora eseguiti**. Evidenza aggiuntiva per UI-01, UI-06, UI-07, UI-17 e UI-18; **tutte le 22 prove visive restano aperte**. [Rapporto D42](../design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Piano Windows RC28](G1_D42_RC28_TEST_PLAN.md).

**Aggiornamento G1-D41 / RC27, 04/11/2026:** RC26 confermata verde su Windows (**504 test**); RC27 include +3 test sui dettagli Edifici (**507 attesi**, non eseguiti qui). L'evento `SelectionChanged` riporta in alto dettaglio e diagnosi senza modificare il focus o i refresh della stessa selezione. Rafforzate le evidenze automatiche per **UI-07, UI-12, UI-18**; le relative prove visive **rimangono aperte** a 1040×740 e 1240×850. [Rapporto D41](../design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Test](G1_D41_RC27_TEST_PLAN.md).

# G1-D39 / RC24 — Matrice operativa di chiusura P0, GUI v13/v14

**Aggiornamento G1-D40, 04/11/2026:** RC25 confermata verde su Windows (**501 test**); RC26 candidata con +3 test Desktop (**504 attesi, non eseguiti**). La seguente matrice è sempre valida: nessuna delle 22 prove visive P0 è certificata. Le evidenze automatiche UI-05 e UI-14 sono state rafforzate in RC26. La descrizione originale D39 resta storica.

La matrice elenca **22 obblighi**, non 22 risultati già approvati. **A** = codice e test automatici esistenti o aggiunti (per RC21+ esecuzione ancora pendente); **S** = ispezione statica; **V** = osservazione visiva/interattiva Windows ancora da effettuare. Una voce passa a **CHIUSA** soltanto con il test cumulativo verde e una prova Windows registrata alle due dimensioni, o una non-applicabilità motivata. Nessuna voce è qui chiusa visivamente.

| ID | Area | Evidenza automatica / statica (non equivalente a esito Windows) | Prova manuale necessaria | Gate |
| --- | --- | --- | --- | --- |
| UI-01 | Navigazione | A: `V14NavigationSelectionAuditTests` (D38), menu/tab sincronizzati | Tutte le 5 pagine; focus, ritorno, scroll, 1040/1240 | V aperto |
| UI-02 | Risorse: sette colonne | A: `V14ResourcesAuditTests` (D35) | Intestazioni, spazi e ultima colonna con scrollbar | V aperto |
| UI-03 | Numeri/allineamento | A: `V14ResourcesAuditTests`; `ResourceRow` riutilizzato | Sei colonne numeriche a destra, arrotondamento e tooltip di precisione | V aperto |
| UI-04 | Tooltip capacità | A: `V14UiParityRegressionTests` RC13/RC15 | Municipio, Magazzino, Scuola, Matite, Pane, deposito pieno | V aperto |
| UI-05 | Flussi e tempo | A: `V14PopulationAndFlowAuditTests` (RC26) e `ResourceRow` | Produzione, consumo, saldo, stima pieno con saldo ≤ 0 e capacità satura | V aperto |
| UI-06 | Ordinamento e selezione Risorse | A: `V14ResourcesAuditTests` (D35) | 17 risorse italiane; ocra, tooltip, selezione in scroll/tick | V aperto |
| UI-07 | Elenco Edifici | S: `V14BuildingEntry`, `RefreshBuildingEntries` | Testi lunghi, icone, contatori, scroll e riga selezionata | V aperto |
| UI-08 | Livelli distinti | A: `V14UiParityRegressionTests`, `V14BuildingUpgradeAuditTests` | Base/Grande/Avanzato separati; nessuna ComboBox livello | V aperto |
| UI-09 | Requisiti di sblocco | A: `V14UiParityRegressionTests` (RC8) | Riga grigia selezionabile; requisiti visibili; Costruisci assente da bloccati | V aperto |
| UI-10 | Materiali e pulsanti | A: `V14BuildingUpgradeAuditTests` (D36) | Grigio se non pagabile; verde e hover ocra solo se pagabile; niente salto | V aperto |
| UI-11 | Costi lotti | A: test preventivi RC5/RC11 | +1/+5/+10, prezzo progressivo totale e mancanze coerenti | V aperto |
| UI-12 | Dettagli e scorrimento | A: `V14BuildingDetailScrollAuditTests` (D37) | Produzioni/consumi/capacità, diagnosi max 104px, comandi sempre leggibili | V aperto |
| UI-13 | Pane e produzione | A: regressioni RC4/RC6 | Input mancanti, filiera, spazio alimentare, primo residente avanzato | V aperto |
| UI-14 | Popolazione/crescita | A: `V14PopulationAndFlowAuditTests` (RC26), forecast e test sessione | Velocità crescita, capienza case, razioni e blocchi in GUI | V aperto |
| UI-15 | Esercito | A: `V14MilitaryUiParityTests` (D34/D37) | Coda, progresso a 10/30 s, lotti, rimborsi e blocchi | V aperto |
| UI-16 | Scuola | A: test RC9/RC18 e `V14SchoolConfirmationHintsAuditTests` (D39) | Coda 120 s, ricercatori, Matite 2/studente, suggerimenti in conferma | V aperto |
| UI-17 | Albero tecnologico | A: regression RC12 | Saldo 2 decimali, acquisti non pagabili grigi, prerequisiti e effetti | V aperto |
| UI-18 | Hover/disabled/focus | A: test headless RC8/RC21/RC22 | Nessun movimento, hover ocra, grigio disabled, focus a due viewport | V aperto |
| UI-19 | Selezioni al refresh | A: `V14NavigationSelectionAuditTests` (D38) | Edificio/Risorsa/Tecnologia dopo tick, Load e cambi pagina | V aperto |
| UI-20 | Ciclo Nuova partita | A: regressioni RC15/RC16/RC17 | Conferma, Annulla, pausa, comandi disabilitati, chiusura finestra | V aperto |
| UI-21 | Persistenza | A: test Infrastructure/RC17 | Backup del checkpoint, salvataggi separati v2/v3, errore DB non distruttivo | V aperto |
| UI-22 | Invarianti di popolazione | A: test Core/Application e SQLite | Nessuna duplicazione con lavoratori/Scuola/Esercito, nessun addebito a errore | V aperto |

## Checklist di esecuzione ripetibile

1. Su una **copia** della cartella di gioco e dei salvataggi, eseguire `./validate.cmd` dalla RC26. Registrare il riepilogo/TRX: **504 test attesi**. Se fallisce, il gate rimane aperto e va registrato il caso, non attribuito un PASS.
2. Avviare `./run-game.cmd` (v13) e `./run-game.cmd --experimental-v14`, in sessioni separate, senza migrare o sovrascrivere i salvataggi. Alla prima prova v14 **fare backup del DB**.
3. Verificare **1040×740** e **1240×850**, a scala DPI uguale. Per ogni UI-ID segnare data, versione, PASS/FAIL/NA motivata, azioni riprodotte, differenze autorizzate e immagine/evidenza.
4. Utilizzare immagini denominate `UI-XX_1040x740_v13.png`, `UI-XX_1040x740_v14.png`, `UI-XX_1240x850_v13.png`, `UI-XX_1240x850_v14.png` dove comparabili; per Scuola/Albero (solo v14) registrare screenshot v14 e NA motivata per v13.
5. Sulle schermate Edifici, Risorse, Esercito e Scuola ripetere almeno un controllo con **risorse insufficienti** e uno dopo avanzamento di **tempo/salvataggio**; verificare sempre che la selezione resti inalterata.
6. **Non chiudere P0** con la sola esecuzione xUnit o con il solo confronto XAML: tutte le 22 righe devono avere un esito visivo reale o NA motivata; correggere i FAIL e ripetere il test.

## Priorità residue dopo D39

- **G1-D40 (RC26 candidata):** UI-05/UI-14: recuperato lo stato dettagliato della crescita; aggiunti test ETA risorse/case piene/accelerazione. Restano aperte prove visive, consumo reale Cibo/Pane e blocchi in sessione.
- **G1-D41:** UI-07/UI-12/UI-18: audit visuale dei dettagli Edifici, caratteri, scroll/focus; evitare redesign non supportato.
- **Gate finale P0:** prova Windows cumulativa e 22 evidenze eseguite. Solo dopo, Scuola/tecnologie P1, audit economico E4a, decisioni E4b, migrazione E4c, M5.4–M7.

[Audit iniziale](G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md) · [Piano test RC24](G1_D39_RC24_TEST_PLAN.md).
