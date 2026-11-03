# G1-D23 — Piano di validazione RC7 (correzione test Avalonia)

**Stato: da eseguire su Windows.** Nessuna compilazione né test .NET in fase di confezionamento. Il test nella RC6 è fallito per il confronto con `IsEnabled` invece di `IsEffectivelyEnabled`.

## 1. Suite automatica

Estrarre lo **ZIP RC7 completo** in una cartella nuova, poi:

```powershell
.\validate.cmd
```

**Risultato atteso (non dichiarato superato):** **451/451**. Il run RC6 precedente è documentato come **450/451**; il solo fallimento noto era `UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled` in `V14UiParityRegressionTests.cs:191`. Conservare il riepilogo, `.trx` e warning.

## 2. Verifica mirata

`tests/SimRisorse.Desktop.Tests/V14UiParityRegressionTests.cs`, test `UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled`:

- La Segheria Base esiste e sono sbloccate entrambe le tecnologie; le scorte sono vuote.
- Gli upgrade Base→Grande e Base→Avanzato rimangono **visibili**.
- Le quote mostrano **«Mancano:»** e non indicano ricerca ancora necessaria.
- I due pulsanti possiedono gli esatti comandi del ViewModel.
- `CanExecute(null) == false` e **`IsEffectivelyEnabled == false`** per entrambi.
- Il comando non deve eseguire acquisti senza scorte; nessun cambio nei salvataggi.

Riferimento al comportamento di Avalonia: [Button.cs nel tag 12.1.4](https://github.com/AvaloniaUI/Avalonia/blob/12.1.4/src/Avalonia.Controls/Button.cs).

## 3. Verifiche funzionali e P0

Ripetere i [casi RC6](G1_D22_RC6_TEST_PLAN.md) per Pane, input, output saturo, diagnosi e lotti; verificare [RC5](G1_D21_RC5_TEST_PLAN.md) per upgrade e costi progressivi. Completare le 22 prove [della matrice GUI](G1_D19_MATRICE_PARITA_GUI.md), con screenshot a **1040×740** e **1240×850**. Confrontare la v13 (`.\run-game.cmd`) con la v14 isolata (`.\run-game.cmd --experimental-v14`); salvare un backup dei profili prima delle prove. Non migrare/sovrascrivere il salvataggio v13.

## Gate di chiusura

- [ ] Test automatici **451/451** confermati con riepilogo Windows.
- [ ] Verifica visiva e comportamentale pulsanti upgrade: visibili, ma non attivabili senza materiali.
- [ ] Parità Risorse, Edifici, Esercito, Scuola, Albero tecnologico; nessuna regressione nel gioco v13.
- [ ] Salvataggi dei due profili isolati e chiusura sicura v14.
- [ ] E4 sospeso fino ad approvazione esplicita dei gate.

[Rapporto tecnico D23](../design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md) · [Handoff](../HANDOFF.md).
