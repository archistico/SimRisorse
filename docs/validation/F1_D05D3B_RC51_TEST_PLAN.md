# RC51 — Piano di validazione del clock v15

**Non eseguito**: l'ambiente di generazione non dispone di `dotnet`/SDK .NET 10. RC47 620 test passati dall'utente. RC48 +11, RC49 +17, RC50 +19, RC51 +16: **683 attesi**, inclusi tutti gli incrementi non ancora verificati. I conteggi `[Fact]` non sostituiscono il totale reale del runner.

## Windows: comandi

Nella cartella del progetto estratto:

```powershell
.\validate.cmd

dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d3bUnifiedCampaignClockTests
dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d3bUnifiedCampaignPersistenceTests
```

## Matrice di controllo

| Area | Aspettativa |
| --- | --- |
| Cibo pronto | assente dalla città, prodotto solo dal `food-depot` anche con legacy `food-store` costruito e dotato di addetti |
| Ricette legacy | Pescatore alimenta Deposito, nessuna trasformazione alternativa fish→food |
| Senza addetti al Deposito | accumulo materia grezza; nessuna conversione in Cibo |
| Residenti avanzati | Pane e Cibo scalati insieme, senza consumo parziale |
| Fame | nessuna crescita, ma clock Scuola e militari indipendenti proseguono |
| Secondi parziali | 900ms+100ms produce esattamente come 1 secondo; niente produzione anticipata |
| Secondi continui/spezzati | stesso stato finale di scorte, crescita, ricerca, esercito, checkpoint e tempo |
| Errore regole | policy errata o elapsed non valido non mutano la sessione |
| Salvataggi | tick→save→load→tick equivalente a tick continuo, senza perdita animali/transito/staff |
| GUI v14 | nessuna modifica a XAML, ViewModel, command router, salvataggio utilizzato |

## Condizioni prima di D05d.4

I test v14 e v15 devono passare insieme. Controllare su Windows tutte le warning-as-errors, riproducibilità del save/load e interazioni del catalogo storico con le tecnologie. Non eseguire prove di nuovi formati direttamente su `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`; usare DB temporanei per i test.

[Specifica](../design/F1_D05D3B_RC51_CLOCK_UNIFICATO.md) · [Roadmap](../ROADMAP.md).
