# G1-D40 — RC26: audit UI-05/UI-14, flussi e crescita popolazione

**Data:** 04/11/2026. **Baseline confermata dall’utente:** RC25, 501 test superati. **RC26 candidata:** 504 test attesi (+3 Desktop); compilazione e xUnit non eseguiti nel confezionamento per assenza dell’SDK .NET 10. **Gate P0 visivo ancora aperto.**

## Problema riproducibile individuato

Nella schermata v13, `MainViewModel.GrowthActiveStatus()` mostra il progresso reale in ventesimi di secondo; se la velocità di crescita è accelerata, mostra anche il moltiplicatore e i secondi stimati al prossimo abitante. Nella RC25 v14 il testo utilizzava solo `ProgressSeconds` (intero) e il moltiplicatore, **omettendo il resto frazionario e il tempo residuo**. La barra percentuale v14 utilizzava già il resto corretto, creando una divergenza fra testo e barra.

## Correzione

Soltanto `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Parity.cs` è modificato. Il nuovo `GrowthActiveStatus()` adotta la stessa presentazione della GUI classica: progressione in ventesimi, accelerazione `×N,NN` e tempo `~Ns` calcolato con `Ceiling` su unità intere. Se la velocità è normale (×1) preserva la forma breve classica. `GrowthStatus` continua a dare precedenza a **Cibo mancante**, **Pane mancante** e **abitazioni piene**. Non vengono modificati la crescita, gli avanzamenti, gli intervalli, i costi o i salvataggi.

## Copertura nuova

`tests/SimRisorse.Desktop.Tests/V14PopulationAndFlowAuditTests.cs` aggiunge tre test:

1. Colonia iniziale: 8/8 abitanti, case piene, avanzamento nullo.
2. Una seconda casa piccola porta la velocità a ×1,15; `~53s` prima del tick, `1,15 / 60 s` e `~52s` dopo 1 s, barra fra 1,91% e 1,92%, nessun residente anticipato.
3. `ResourceRow.TimeToFull` usa il **saldo netto** e distingue pieno `00:00:00`, capacità assente o saldo nullo/negativo `—`, tempi sotto un minuto e `01:00:00`.

Questi test **non sostituiscono** la verifica delle razioni Cibo/Pane, il confronto visivo v13/v14 e il controllo reale della tabella; tali gate restano aperti.

## Invarianti

- Nessuna modifica a v13, motore Core/Application, formati SQLite v2/v3, configurazioni produttive e XAML.
- Non sono cambiati i coefficienti E4; la ricetta Matite e il costo di due Matite per studente restano come in RC25.
- Nessuna dichiarazione di 504 test superati prima di `validate.cmd` su Windows.

[Roadmap](../ROADMAP.md) · [Matrice P0](../validation/G1_D39_P0_UI_MATRIX.md) · [Piano RC26](../validation/G1_D40_RC26_TEST_PLAN.md).
