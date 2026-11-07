# RC94.2 — Hotfix asserzioni consumo Carbone Zecca

**Stato:** candidata, 07/11/2026. **Totale previsto:** 1233 test, invariato rispetto alla RC94.1. **Baseline integralmente confermata:** RC93.1, 1220 test.

## Evidenza Windows ricevuta

La suite RC94.1 ha eseguito 1233 test, **1228 superati e 5 falliti**, tutti nella teoria `Rc81LiveMintCrewTests.LiveClockConsumesInputsOnlyForCompletedMintTeams` (`staff` 0, 9, 10, 19, 20). La differenza osservata per il Carbone è esattamente **+100** rispetto all'asserzione storica: expected 6/5/4, actual 106/105/104. Questo è coerente con le 100 unità aggiunte al Municipio in RC94.1. Non rappresenta una prova di doppia produzione o doppio consumo.

## Modifica applicata

Nel file `tests/SimRisorse.Application.Tests/Rc81LiveMintCrewTests.cs`, il test non confronta più lo stock finale assoluto di Carbone con `6 - expectedModules`. Ora verifica per **Oro e Carbone** la differenza `stockIniziale - stockFinale = expectedModules`, senza cambiare il test delle squadre 0/9/10/19/20, delle Monete o del regime MintTen. Nessun codice di produzione è stato modificato.

## Verifica richiesta su Windows

```powershell
.\validate-f1.cmd
```

Criteri di uscita: compilazione verde, **1233 test passati / 0 falliti**, gate F1 completo inclusi report e screenshot. Non considerare la RC94.2 validata finché non arriva la conferma Windows. In caso di altri fallimenti, conservare il log integrale e distinguere i problemi reali del motore dai fixture/test ancorati a vecchie scorte.

## Perimetro e compatibilità

Non cambiano: Municipio (100 Carbone e 2 Matite solo per nuove partite), salvataggi SQLite, economia, produzione, GUI pixel art, filtri, finanziamento sandbox RC94. Nessuna modifica a `.csproj` o a dipendenze. L'asserzione parametrizzata esistente resta una singola teoria con cinque casi, quindi il conteggio xUnit non cambia.
