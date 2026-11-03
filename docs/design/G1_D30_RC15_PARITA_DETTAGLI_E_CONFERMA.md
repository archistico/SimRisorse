# G1-D30 / RC15 — Audit di parità: capienza Matite nei dettagli e conferma nuova partita

**Data:** 03/11/2026. **Base confermata:** RC14 / G1-D29-F1, 470 test dichiarati tutti passati dall’utente. **Candidata:** RC15 / G1-D30, 473 test attesi; esecuzione .NET 10 e prova visuale Windows ancora pendenti.

## Risultati dell’ispezione (UI-04, UI-12, UI-20)

1. **D30-01 (Edifici, P2 informativo):** la v14 calcolava lo stoccaggio Matite di Municipio (+1.000), Magazzino (+500) e Scuola (+50) ma il dettaglio «Capacità» di questi edifici non lo elencava. Il tooltip in Risorse era stato corretto in RC13, quindi le due viste mostravano informazioni discordanti.
   - Correzione: aggiunta riga con icona `pencils` nel medesimo elenco `CapacityLines` degli altri elementi. Per Municipio/Magazzino l’importo viene letto da `StoragePerBuilding["planks"]`, ossia la regola stessa da cui `V14SchoolSupplies.Capacity` ricava la capacità Matite; per Scuola la costante v14 `SchoolPencilStorage` viene centralizzata nel modulo esistente e usata dalla formula Core. **Formula numerica invariata** (moltiplicazione decimale esplicita).
   - Invariati: capienze effettive, inventario, produzione, ricette, edificio e GUI v13.
2. **D30-02 (Nuova partita, P2 controllo):** la v13 disabilita Pausa/Riprendi finché la finestra di conferma nuova partita è aperta; la v14 lasciava il comando eseguibile e permetteva di riprendere la simulazione durante la conferma.
   - Correzione: predicato `CanExecute = !_newGameConfirmation` sul comando v14. `RefreshParity()` notifica l’abilitazione dopo apertura/cancellazione/conferma. Nessun cambiamento al comportamento esistente «Annulla» (la partita resta in pausa fino a esplicito Riprendi), coerente con v13.

## Prove aggiunte

- `BuildingDetailsShowPencilStorageFromActualV14CapacityRules`: Municipio, Magazzino, Scuola e permanenza delle righe precedenti.
- `NewGameConfirmationKeepsPauseDisabledUntilResolved`: pausa, conferma, invocazione diretta del comando, annullamento e ripresa.
- `PauseButtonVisuallyDisablesDuringNewGameConfirmation`: stato `IsEffectivelyEnabled` del controllo Avalonia.

**Nessuna certificazione del confronto visivo 1040×740 / 1240×850:** gli screenshot e l’intera matrice UI-01…UI-22 restano aperti. I tre test nuovi non sostituiscono tale verifica. Il bilanciamento E4 e il navale sono sospesi.

[Roadmap](../ROADMAP.md) · [Audit D29 aggiornato](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano RC15](../validation/G1_D30_RC15_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md).
