# G1-D27 / RC11 — Layout dei comandi Costruisci e Lavoratore

**Baseline:** v60-E3b4-RC10, test dichiarati passati dall'utente (464 attesi). **RC11 candidata:** 466 test attesi, non eseguiti per assenza SDK .NET 10.

## Richiesta e comportamento implementato

Il box Edifici v14 ora mostra le operazioni in due righe omogenee, senza ambiguità tra lotti:

```text
Costruisci   [+1]    [+5]    [+10]
Lavoratore [-1] [+1]    [+5]    [+10]
```

`−1` è un comando compatto accanto alla label «Lavoratore», **non uno dei tre lotti positivi**; conserva il rilascio del personale già esistente. Le tre colonne a destra hanno larghezza uniforme e si mantengono su singola riga alla dimensione minima della finestra. I blocchi dei prerequisiti lasciano l'edificio selezionabile, ma nascondono la riga Costruisci; i materiali insufficienti disabilitano il singolo lotto (colore grigio) senza nascondere il prezzo/diagnosi. Tutti i comandi di costruzione conservano hover ocra e bordo di spessore costante.

## Implementazione

- `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml`: sostituita la vecchia riga mista con `V14BuildingConstructionActions` e `V14BuildingWorkerActions`. `V14BuildSelected` contiene solo `+1`; `V14BuildFive` e `V14BuildTen` restano costruttivi. Aggiunti `V14AssignFiveWorkers`, `V14AssignTenWorkers`.
- `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Buildings.cs`: aggiunti `CanAssignSelectedOne/Five/Ten`, guard per popolazione libera e numero postazioni, e comando batch che sfrutta la transazione della sessione esistente. Refresh dopo selezione e cambiamenti simulazione.
- `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.cs`: inclusi i due nuovi comandi nel circuito comune di refresh.
- `tests/SimRisorse.Desktop.Tests/V14UiParityRegressionTests.cs`: test del layout a 1040×740 e 1240×850, dei binding e stati disabilitati, hover senza movimento, assegnazione atomica di cinque addetti e protezione contro lotto impossibile di dieci.

**Non modificati:** `MainWindow.axaml`, Core, Application produttiva, SQLite, v13 e risorse/Matite.

## Esito e prossimi controlli

- Ispezione statica XAML/JSON, diff e ZIP da eseguire prima della consegna.
- **Compilazione/xUnit Windows da eseguire:** `./validate.cmd`, **466 test attesi**.
- Verifica manuale: grafica in modalità `--experimental-v14`, righe non sovrapposte a 1040×740 e 1240×850; pulsanti grigi quando non utilizzabili; costo cumulativo +5/+10; lavoratori su livelli Base/Grande/Avanzato; assenza di cambi alla pagina Risorse.

[ADR 0036](../adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md) · [Piano](../validation/G1_D27_RC11_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
