# RC85 / E4b.2g — Buffer abitativo naturale e confronti MintTen

## Obiettivo

Studiare se ampliare **per tempo** le abitazioni riduce le attese per assegnare 10 lavoratori alla prima Zecca v15. La strategia è una politica di gioco reale, ma si applica **solo** al benchmark controllato da CLI, non alle sessioni predefinite. Non altera prezzi e capienze.

## Politiche disponibili

| Politica | Interventi abitativi specifici | Regime | Comportamento |
| --- | --- | --- | --- |
| `research-first` | una Casa piccola + una Casa grande nei prerequisiti | Legacy/MintTen | Scuola costruita prima del Pane |
| `bread-first` | una Casa piccola + una Casa grande nei prerequisiti | Legacy/MintTen | Scuola costruita dopo Pane e Carbone |
| **`housing-buffer`** | **due Case piccole in più** rispetto a `bread-first` | Legacy/MintTen | Completati ricerca e filiera del Pane/Carbone, compra le due case, poi procede all'Oro |

Le due case aggiuntive sono azioni `build-small-house` ordinarie, pagate con legname/pietra e soggette a disponibilità e capienza. Non aumentano istantaneamente la popolazione: gli abitanti devono crescere secondo le regole già in vigore. Il numero delle case acquistate **nel run** può essere inferiore al numero pianificato se la prova termina o si blocca prima.

## Comandi

```powershell
# Diagnostica economica breve (non produce dati di fattibilità):
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --max-seconds 0 --include-housing-buffer --compare-legacy --output .local-cache/e4b-rc85-zero

# Rilettura offline, senza clock:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --review-e4b-housing-buffer --input .local-cache/e4b-rc85-zero/e4b-mint-ten-results.json --output .local-cache/e4b-rc85-review

# Run naturale a orizzonte pieno (costoso):
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --include-housing-buffer --compare-legacy --output .local-cache/e4b-rc85-full
```

La CLI preesistente, **senza** `--include-housing-buffer`, mantiene le sole due strategie storiche. `--policy housing-buffer` seleziona solamente la terza. Le cartelle di output devono essere nuove; le ricevute vengono esportate nei consueti file `e4b-mint-ten-*`. Il nuovo confronto offline scrive `e4b-housing-buffer.{json,csv,md}` e conserva l'hash SHA-256 del JSON grezzo.

## Interpretazione rigorosa

- Il report nuovo confronta `housing-buffer` con `bread-first` per regime, usando lo stesso inventario iniziale e lo stesso orizzonte dichiarato, validati dall'audit RC84.
- Case acquistate = numero di eventi `build-small-house/executed` (non case presenti a inizio gioco e non case soltanto preventivate).
- Spese = somma delle ricevute `executed`; includono **tutta** la filiera, senza attribuire ai soli alloggi la spesa totale.
- Delta secondi = `tempo_housing - tempo_bread` e viene valorizzato solo quando entrambi i run sono realmente completati e non abbreviati.
- Una mancata assegnazione può derivare da case, tempi di crescita, razioni, impieghi precedenti, ricerca o materiali: non attribuire una causa univoca senza telemetria.
- Nessun run naturale pieno della RC85 è stato eseguito durante la consegna; i sei run t=0 restano diagnostici.

## Stato

**RC85 candidata.** 20 casi xUnit nuovi; 1058 attesi nel gate cumulativo (RC82 1002 verificati; RC83 18, RC84 18, RC85 20 da verificare). Nessuna modifica al salvataggio, al motore di gioco e all'interfaccia.
