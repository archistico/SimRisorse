# G1-D23 / RC7 — Audit del test Avalonia 12: abilitazione reale pulsanti upgrade

**Data:** 3 novembre 2026  
**Pacchetto:** `v60-E3b4-RC7`  
**Ambito:** esclusivamente `tests/SimRisorse.Desktop.Tests/V14UiParityRegressionTests.cs` e documentazione. **Nessuna variazione a codice eseguibile.**

## Evidenza Windows ricevuta

```
SimRisorse.Desktop.Tests.V14UiParityRegressionTests.
  UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled [FAIL]
V14UiParityRegressionTests.cs:191  Assert.False(nextButton.IsEnabled)
Riepilogo: 451 test; 450 superati; 1 fallito; 0 ignorati.
```

L'utente ha fornito il riepilogo test e il percorso del `.trx` locale. Il percorso locale non è incluso nell'archivio: non si dichiarano ispezioni del `.trx` stesso.

## Diagnosi tecnica

Nel caso riprodotto, la ricerca è sbloccata ma le risorse sono vuote. **Prima** dell'asserzione che fallisce il test aveva già controllato:

1. `ShowUpgradeNext` e `ShowUpgradeDirectlyToAdvanced` veri: pulsanti visibili.
2. `CanUpgradeNext` e `CanUpgradeDirectlyToAdvanced` falsi.
3. `UpgradeSelectedCommand.CanExecute(null)` e `UpgradeSelectedToAdvancedCommand.CanExecute(null)` falsi.
4. Quote con «Mancano:», nessun prerequisito di ricerca residuo.
5. Pulsanti `V14UpgradeNext` e `V14UpgradeAdvanced` effettivamente visibili nella finestra headless.

In Avalonia **12.1.4** il controllo `Button` ridefinisce:

```csharp
protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;
```

Dunque `Button.IsEnabled` non riflette necessariamente la disabilitazione operativa del comando. **La proprietà da verificare nella UI è `Button.IsEffectivelyEnabled`**, che considera `IsEnabledCore` (e gli eventuali genitori disabilitati). Il messaggio «Expected: False / Actual: True» su `IsEnabled` non dimostra che il pulsante sia cliccabile: il test interroga un livello d'astrazione errato.

Riferimenti upstream consultati: [sorgente Avalonia tag 12.1.4](https://github.com/AvaloniaUI/Avalonia/blob/12.1.4/src/Avalonia.Controls/Button.cs), [API `IsEnabledCore`](https://api-docs.avaloniaui.net/docs/P_Avalonia_Controls_Button_IsEnabledCore).

## Correzione

Sostituite le due chiamate:

```csharp
Assert.False(nextButton.IsEnabled);
Assert.False(directButton.IsEnabled);
```

con verifiche più forti:

```csharp
Assert.Same(view.UpgradeSelectedCommand, nextButton.Command);
Assert.Same(view.UpgradeSelectedToAdvancedCommand, directButton.Command);
Assert.False(nextButton.IsEffectivelyEnabled);
Assert.False(directButton.IsEffectivelyEnabled);
```

Non è stato aggiunto né eliminato alcun caso xUnit. **451 test attesi** nella RC7. Nessuna decisione di gioco è cambiata, pertanto non occorre un nuovo ADR. Non modificati i flag di `CanExecute`, la logica dei costi, il XAML né le tabelle Risorse.

## Stato e limiti

- **RC6:** suite eseguita su Windows, **450/451**, gate automatico fallito.
- **RC7:** test modificato, **non eseguito qui** perché manca .NET SDK; servono log Windows e confronto grafico.
- **P0 grafico:** 22 verifiche manuali v13/v14 ancora da approvare, indipendentemente dall'eventuale suite verde.
- **E4 e migrazione:** rimangono sospesi.

[Piano di validazione RC7](../validation/G1_D23_RC7_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).
