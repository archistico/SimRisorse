# G1-D24 — Test plan RC8: edifici grigi, comandi e Scuola

**Stato:** da eseguire su Windows 11 con SDK .NET 10; **456 test attesi**, non ancora confermati. RC8 comprende RC5/RC6 e la correzione RC7. Ultimo run riportato: **RC6 450/451**, un fallimento nell'asserzione `IsEnabled` ora sostituita nella RC7; non chiamare verde RC7/RC8 senza nuovo log.

## 1. Automatici

Da **nuova** cartella estratta:

```powershell
.\validate.cmd
```

Verificare **456/456**, `.trx` e warning. Regressioni nuove in `V14UiParityRegressionTests`: `LockedBuildingRowsAreMutedButStillExposePrerequisitesInDetails`, `ConstructionIsFullWidthAndFourSecondaryActionsShareTheirOwnRow`, `MissingResourcesMakeBuildButtonVisiblyDisabledWithoutHidingTheDetails`, `V14BuildHoverIsDistinctAndDoesNotMoveTheActions` (×2 viewport). Test della Scuola aggiornato in `V14PreviewTests.ExperimentalWindowUsesOriginalSidebarAndBuildingDetailsWithoutTierComboboxes`.

## 2. Manuali — doppia risoluzione

Avviare `./run-game.cmd --experimental-v14`, prima a **1040×740**, poi a **1240×850**; conservare backup del DB v14 e non modificare il DB v13. Per parità avviare anche `./run-game.cmd`.

| Prova | Comportamento richiesto | Esito |
| --- | --- | --- |
| Edificio non sbloccato | Riga chiaramente grigia, selezionabile al clic; nome, costi, prerequisiti e dettagli leggibili; nessun Costruisci | [ ] |
| Edificio sbloccato, scorte insufficienti | Riga non grigiata; «Costruisci +1» visibile **grigio e non cliccabile**; diagnosi delle risorse mancanti | [ ] |
| Edificio pagabile | «Costruisci +1» grande, interamente leggibile, verde e cliccabile | [ ] |
| Hover attivo | Sfondo ocra/contrasto testo leggibile, nessun movimento della riga né dei comandi | [ ] |
| Hover disabilitato | Rimane grigio, non assume il colore del pulsante attivo | [ ] |
| Righe di azione | Prima riga: soltanto «Costruisci +1»; seconda riga: «−1 lavoratore», «+1 lavoratore», «+5», «+10» senza tagli o sovrapposizioni | [ ] |
| Lotti +5/+10 | Tooltip e preventivo cumulativo; disabilitati senza risorse, riabilitati quando pagabili | [ ] |
| Cambio selezione / tick | Opacità e stati dei comandi aggiornati; selezione rimane corretta | [ ] |
| Scuola | **Nessun pulsante Costruisci** nella scheda Scuola; cinque operazioni studenti/ricercatori conservate | [ ] |
| Costruzione Scuola | Riga presente in Edifici, dettagli/costi/prerequisiti consultabili e costruzione disponibile dopo sblocco | [ ] |
| Risorse / Esercito / v13 | Layout e azioni invariati; sette colonne, scroll e hover Risorse intatti | [ ] |

**Chiusura P0:** dopo suite verde, confronto visivo v13/v14, test su salvataggi v2/v3 e [matrice generale](G1_D19_MATRICE_PARITA_GUI.md). Non iniziare E4 prima della parità approvata.

[Rapporto D24](../design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md) · [Piano RC7](G1_D23_RC7_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
