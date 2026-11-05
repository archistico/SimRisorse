# RC49 — Piano di verifica F1-D05d.2 (SQLite generation 5)

**5 novembre 2026.** RC47: **620 test confermati dall'utente**. RC48: +11, **631 attesi ma non eseguiti**. RC49: +**17** test `[Fact]` (12 Infrastructure e 5 Core), **648 attesi**; compilazione e test .NET 10 **non eseguiti** in questo ambiente.

## Esecuzione Windows

Aprire PowerShell nella radice del progetto, con SDK **.NET 10** installato:

```powershell
.\validate.cmd
```

Se fallisce, riportare i messaggi completi con progetto/test, senza modificare manualmente i file SQLite reali. Per lanciare isolatamente la verifica del nuovo schema:

```powershell
 dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release --filter FullyQualifiedName~F1D05d2FoodIndustryPersistenceTests
 dotnet test tests/SimRisorse.Core.Tests/SimRisorse.Core.Tests.csproj -c Release --filter FullyQualifiedName~FoodIndustryCampaignStateTests
```

**Nota:** a differenza dei test della GUI v14, questi nuovi test creano database effimeri sotto la directory temporanea. Non devono toccare `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`.

## Verifiche introdotte

- Round-trip esatto al **millesimo** per `cattle`, `game`, `meat`, `fruit`, `vegetables`, città e deposito; presenza delle **sei** righe edifici, intestazione coerente `5/5`.
- Modifica successiva con backup SQLite contenente il precedente stato completo.
- Rifiuto di edificio mancante o ignoto, numero di lavoratori maggiore dei posti, personale combinato maggiore della popolazione disponibile, risorse nello stock errato, tabella mancante e `user_version=4` (incompatibilità deliberata, nessun overwrite silenzioso).
- Divieto di proiettare una campagna con nuove industrie attive nel vecchio snapshot v14.
- Simulazione di continuità: `tick → save → load → tick` uguale al percorso senza salvataggio, con transito non consegnabile conservato.
- Test Core sui posti derivati, sei tipi obbligatori e budget unico della forza lavoro.

## Gate di uscita

- `validate.cmd` passa integralmente su Windows; numero totale di test **da confermare dai log xUnit**, non dal mero conteggio dei `[Fact]` (potrebbero esistere `[Theory]`).
- Aprire la GUI v14 e verificare il comportamento storico di Risorse/Edifici/Scuola, senza nuove ComboBox e senza attivazione di filiere ancora incomplete.
- Controllare su una cartella temporanea un SQLite 5 valido, un 4 rifiutato e la ripresa senza perdita di transito; non cancellare dati personali per testare un formato di sviluppo.
- In assenza di risultati Windows, la milestone resta **candidata**, non validata.

[Contratto](../design/F1_D05D2_RC49_CHECKPOINT_FILIERA_SQLITE5.md) · [ADR 0060](../adr/0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md) · [Stato](../CURRENT_STATE.md).
