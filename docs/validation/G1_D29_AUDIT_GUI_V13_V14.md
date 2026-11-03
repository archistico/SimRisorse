# G1-D29 / RC13 — Primo audit di parità v13/v14 (non chiude il gate grafico)

**Data:** 03/11/2026. **Base:** RC12, suite Windows dichiarata passata dall'utente (**468 attesi**). **Controlli D29:** ispezione statica di `MainWindow` e `V14PreviewWindow`, delle proiezioni e dei test; verifica visuale a Windows **non eseguita in questo ambiente**. Nessuna voce qui equivale a un'approvazione visiva.

| ID | Area | Evidenza nel codice / nei test RC12 | Stato D29 e azione successiva |
| --- | --- | --- | --- |
| UI-01 | Navigazione | Sidebar v14 include cinque pagine; `V14NavigationMenu`, `V14Sections`. | Statica presente; confrontare cambio schede, focus e scroll su Windows. |
| UI-02 | Risorse 7 colonne | `V14ResourceTableHeader` e `V14Resources`, test `ClassicResourcePageHasAllSevenOriginalColumnsAndMilitaryActions`. | Copertura headless preesistente; screenshot a entrambe le risoluzioni. |
| UI-03 | Quantità/precisione | `ResourceRow.QuantityRounded`, tooltip `Quantity`. | Statica; numeri e allineamento da confrontare. |
| UI-04 | Capacità reali | `CapacityBreakdown` per risorse standard; **Matite mostravano soltanto bonus generici**, senza conteggi posseduti né spazio libero. | **Difetto accertato; patch RC13 + 2 test**, esecuzione Windows pendente. |
| UI-05 | Flussi e tempi | `ResourceRow.Production`, `Consumption`, `NetRate`, `TimeToFull`. | Statica; testare capacità piena e saldo negativo. |
| UI-06 | Risorse layout | `ResourceNameComparer` it-IT, righe stabili aggiornate `row.Update`. | Statica; verificare selezione ocra, scrollbar e ordinamento visivo. |
| UI-07 | Elenco Edifici | `V14BuildingRow`, `RefreshBuildingEntries`, `SelectedBuilding`. | Statica; testi lunghi e selezione da confrontare. |
| UI-08 | Tre livelli distinti | `V14BuildingEntry` Base/Grande/Avanzato, senza ComboBox livello. | Test preesistenti; confrontare elenco e dettaglio. |
| UI-09 | Blocchi prerequisiti | `RowOpacity`, `CanBuildSelected`, test `LockedBuildingRowsAreMuted...`. | Test preesistenti; visuale da convalidare. |
| UI-10 | Blocchi risorse | `CanAffordSelected/Five/Ten`, `V14BuildSelected.IsEnabled`, test `MissingResourcesMakeBuildButtonVisiblyDisabled...`. | Test preesistenti; verificare colore/hover Windows. |
| UI-11 | Costruzione a lotti | `ProgressiveConstructionPricing`, tooltip +5/+10; test preventivi. | Test preesistenti; evitare lotti parziali. |
| UI-12 | Dettaglio e upgrade | `V14ResourceDetailTemplate`, `ShowUpgradeNext`, `NextUpgradeCost`. | Statica; confrontare icone, capacità e scroll. |
| UI-13 | Pane e diagnosi produzione | `V14BreadSupplyProjector`, `FamilyProductionDiagnosis`, test RC4/RC6. | Test preesistenti; validazione visiva pendente. |
| UI-14 | Popolazione e crescita | `GrowthStatus`, `FoodRequirement`, `BreadRequirement`. | Statica; riprodurre arrivo residente e blocchi. |
| UI-15 | Esercito | `MilitaryTraining*`, `EnlistButton`, `CancelQueueButton`, `DischargeButton`. | Test preesistenti; confrontare tutte le azioni con v13. |
| UI-16 | Scuola | `V14SchoolProgressBar`, `SchoolEnroll...`, `V14CraftPencils`. | Test RC9/RC10; misurare formazione e annullamento in GUI. |
| UI-17 | Albero | `CanBuySelectedTechnology`, saldo millipunti, `V14BuyTechnology`. | Test RC12; verificare blocchi/hover visualmente. |
| UI-18 | Hover e disabled | Stili `primary`, `build`; test headless a due risoluzioni. | Automatizzazione parziale; prova Windows necessaria. |
| UI-19 | Selezione su refresh | Le istanze `ResourceRow` e `V14BuildingEntry` sono riutilizzate; RC13 testa selezione Matite dopo Load. | **Nuovo test RC13**, tick e scroll restano da vedere manualmente. |
| UI-20 | Footer e comandi | `NewGameButton`, `SaveButton`, `LoadButton`, pausa. | Statica; controllare tutti i flussi. |
| UI-21 | Salvataggi | Store separati SQLite v2/v3, test caricamento errato e `CloseWithoutSaving`. | Test preesistenti; smoke test manuale su backup. |
| UI-22 | Invarianti popolazione/scorte | Core/Session validation, test Application/Infrastructure. | Gate xUnit Windows necessario; scenari complessi da provare. |

## Registro difetti e azioni

| ID | Evidenza | Gravità | Intervento | Stato |
| --- | --- | --- | --- | --- |
| D29-01 | Tooltip Matite RC12 elencava bonus fissi «Municipio +1000 / Magazzino +500 / Scuola +50» anche senza possesso, senza mostrare la ripartizione né lo spazio libero. | P2 informativo (UI-04) | Usare il numero reale di Municipi, Magazzini e Scuole, con contributi effettivi e saldo/spazio. | **Corretto nel codice RC13, test scritti ma non ancora eseguiti**. |
| D29-02 | La parità visiva delle 22 voci non è attestata da screenshot o checklist Windows. | P0 validazione | Registrare esito a 1040×740 e 1240×850 e annotare riproduzioni. | **Aperto**; non attribuire un pass automaticamente. |

## Evidenze per la chiusura P0

Compilazione e xUnit Windows verdi; per ciascuna delle 22 voci, esito, scenario riproducibile, dimensione finestra, coppia di screenshot o nota di non applicabilità motivata; nessuna regressione non triagiata. Le novità Scuola/Albero non richiedono una scheda identica in v13, ma non devono far sparire funzioni precedenti. La v13 e i due percorsi SQLite devono restare invariati.

[Matrice originale](G1_D19_MATRICE_PARITA_GUI.md) · [Piano RC13](G1_D29_RC13_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).

## Aggiornamento D30 / RC15 (03/11/2026)

**RC14:** 470 test dichiarati superati dall’utente; D29-01 risolta automaticamente, visuale ancora da verificare.

| ID | Area | Evidenza / regressione | Soluzione RC15 | Gate |
| --- | --- | --- | --- | --- |
| D30-01 | UI-04, UI-12 | Dettaglio Municipio/Magazzino/Scuola non esponeva i propri posti per Matite, ma il tooltip della tabella Risorse sì. | Righe Matite con icona e valori dalla regola di capienza; test Desktop aggiunto. | Candidato; controllo Windows pendente. |
| D30-02 | UI-20 | V14 consentiva Riprendi durante «Nuova partita?»; v13 lo disabilita. | CanExecute e refresh del comando; 2 test Desktop, incluso Avalonia. | Candidato; controllo Windows pendente. |

**Restano aperte:** validazione visiva completa 1040×740 / 1240×850, confronto screenshot e tutte le altre voci non ancora approvate. [Rapporto D30](../design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](G1_D30_RC15_TEST_PLAN.md).


## Aggiornamento D31 / RC16 (03/11/2026)

**RC15:** 473 test attesi, tutti dichiarati passati dall'utente; **RC16:** +3 nuovi test, **476 attesi**, esecuzione Windows pendente.

| ID | Area | Difetto verificato staticamente | Intervento | Gate |
| --- | --- | --- | --- | --- |
| **D31-01** | **UI-20** / UI-09/10/15/16/17 | «Nuova partita?» disabilitava solo Pausa/Riprendi; costruzioni, lavoratori, Scuola, Matite, Esercito e acquisti ricerca restavano operativi. | Guardia unica dei comandi mutanti, senza nascondere righe, dettagli o alterare la sessione; 3 test Desktop aggiunti. | **Candidato RC16**, build e test Windows pendenti; prova visiva manuale in due viewport. |
| **D31-02** | **UI-20/21** | Possibile interazione Salva/Carica durante conferma: comune alla v13, non affrontata in questo incremento. | Verificare flusso e decidere una soluzione coerente con le due GUI; nessuna modifica ai DB in RC16. | **Audit aperto**, non dichiarato risolto. |

[Riproduzione e soluzione D31](../design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](G1_D31_RC16_TEST_PLAN.md).
