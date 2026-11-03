# SimRisorse v58-RC5 — disallineamento validatore v9/v13

Data: 3 novembre 2026. Gate: R3, aperto. Base: v58-RC4.

## Riscontro e diagnosi

Il risultato Windows dell'utente arriva a `scripts/validate.ps1`, riga 102, con:

`The industrial rules, construction or workforce differ.`

Il validatore esegue `--scenario civil-growth --strategy industry --seconds 1102`, senza indicare un ruleset specifico. Il simulatore usa `CivilGrowthScenario.Create()` e restituisce il catalogo attuale `civil-growth-13`. La verifica alla riga 101 confrontava invece `RulesVersion` con `civil-growth-9`. Lo stesso disallineamento esiste nei controlli Ferro/Oro e crescita accelerata ereditati dal validatore v57. I benchmark storici con API dedicate (`--benchmark-*`) hanno già factory v4, v5, v7 e v9 proprie e non devono essere reinterpretati come v13.

L'avanzamento dello script fino alla riga 102 implica che build e comando `dotnet test` precedenti non hanno restituito un exit code negativo; il gate complessivo è comunque fallito. Non disponiamo di un rapporto completo della RC4 per certificare il conteggio test.

## Modifica minimale, senza alterare risultati di riferimento

1. `tools/SimRisorse.Simulation/Program.cs`: nuova opzione CLI `--rules historical-v9`, esclusiva di `civil-growth` e delle strategie `bootstrap`, `industry`, `gold`. Usa `CivilGrowthScenario.CreateMintForBenchmarks()`; non cambia la factory corrente o i salvataggi. Rimane `--rules legacy` per i confronti v4.
2. `scripts/validate.ps1`: Ferro/Oro ai tempi 1102 s / 8 h e 1502 s / 8 h più sonda crescita 180 s vengono ora eseguiti con `--rules historical-v9`. Tutte le aspettative congelate su `civil-growth-9` e i relativi quantitativi sono mantenuti.
3. `scripts/validate.ps1`: due sonde sul catalogo giocabile `civil-growth-13` ai tempi 1102 s e 1502 s verificano costruzione e addetti delle fonderie, produzione positiva, scorte entro la capacità, Municipio iniziale presente. I JSON di uscita sono separati: `industry-current.json` e `gold-current.json`.

Non sono stati modificati `src/`, test xUnit, dati iniziali, consumo di Pane, magazzini, reclutamento, Caserma o file SQLite. La correzione preserva la distinzione storica/corrente.

## Procedura di controllo sul Windows dell'utente

Da una **nuova cartella di estrazione**:

```powershell
.\clean.cmd
.\validate.cmd
```

Se si desidera isolare i due regimi di simulazione prima della validazione completa:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --scenario civil-growth --strategy industry --seconds 1102 --rules historical-v9
dotnet run --project tools/SimRisorse.Simulation -c Release -- --scenario civil-growth --strategy industry --seconds 1102
```

Il primo deve dichiarare `RulesVersion: civil-growth-9`, il secondo `RulesVersion: civil-growth-13`; entrambi devono riuscire a costruire una Fonderia di ferro. I test numerici storici e le sonde correnti sono eseguiti automaticamente da `validate.cmd`.

## Limiti

- In questo ambiente il .NET SDK 10 e PowerShell non sono disponibili, quindi **nessuna compilazione o esecuzione RC5 è attestata**.
- Se il validatore supera questo blocco ma fallisce su uno step successivo, leggere l'errore e il JSON della sonda, senza cambiare arbitrariamente i risultati di riferimento.
- R3 richiede ancora il gate integrale, smoke Desktop e verifica migrazioni su **copie** di database.

## Prossimo passaggio

Ottenere un output Windows completo `validate.cmd`. Se tutti i gate passano, verificare il bilanciamento del reclutamento M5.3c. M5.4 Cantiere e flotta resta bloccata fino ad approvazione N-01…N-07.
