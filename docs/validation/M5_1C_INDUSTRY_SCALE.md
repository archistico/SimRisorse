# M5.1c — Contesa del Carbone e confronto di scala

Aggiornato il **2026-10-27**. **Benchmark implementato, gate Windows ancora da eseguire per questo incremento.** I numeri della tabella sono **risultati attesi calcolati dalle regole**, non risultati dichiarati di una simulazione .NET già eseguita.

**Nota 2026-10-28:** l'utente ha confermato «test passati» sulla v33, nella quale `validate.cmd` include questo benchmark. Non è stato fornito l'output del gate o il JSON `industry-scale-benchmark.json`: la tabella rimane quindi etichettata **teorica**, senza attribuirle misure eseguite né un'identità di run non documentata.

## Scopo e confini

Misurare l'effetto di una o due Miniere di carbone su gruppi diversi di Fonderie di ferro e Fonderie d'oro, conservando i prezzi e le ricette approvati in M5.1a/M5.1b. La priorità del Ferro sull'Oro è già una regola: questo incremento **non** la cambia.

Il benchmark parte da un **checkpoint sintetico**, non dall'inizio di una campagna. Usa il catalogo eseguibile `civil-growth-7`, istanzia una partita con 40 abitanti, 40 posti (una Casa piccola e una grande), un Forno senza addetti e due Magazzini. Imposta 1.700 Tavole, 1.700 Pietra, 1.000 Minerale di ferro, 1.000 Minerale d'oro, 100 Cibo, 100 Pane e zero Carbone/Ferro/Oro. Costruisce con i **comandi reali di `GameSession`** le Miniere di ferro e d'oro, lasciandole senza addetti, quindi le Miniere di carbone e le Fonderie, assegnandovi i lavoratori.

Questo isolamento permette di misurare la contesa del Carbone senza attribuire all'industria le attese causate dalla catena civile, dai prezzi di una partita nuova o dalla disponibilità dei minerali. Il primo secondo alimenta la scorta di Carbone; le Fonderie iniziano nel secondo successivo, secondo l'ordine effettivo del pianificatore.

## Cinque profili: valori teorici da verificare

Tutti i profili avanzano per **180 secondi**. I costi includono una Miniera di ferro e una d'oro non presidiate, comuni a tutti i profili. `Oro senza Carbone` conta i *secondi-lavoratore* bloccati, non semplicemente i secondi solari.

| Profilo | Fonderie Fe / Au | Miniere carbone | Tavole / Pietra spese | Ferro atteso | Oro atteso | Carbone finale atteso | Oro senza Carbone |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| A — bilanciato | 1 / 1 | 1 | 850 / 610 | 179 | 179 | 2 | 1 |
| B — priorità del Ferro | 2 / 1 | 1 | 1.050 / 760 | 358 | 0 | 2 | 180 |
| C — recupero con seconda miniera | 2 / 1 | 2 | 1.180 / 850 | 358 | 179 | 183 | 1 |
| D — due Fonderie d'oro | 1 / 2 | 1 | 1.070 / 780 | 179 | 179 | 2 | 181 |
| E — quattro Fonderie | 2 / 2 | 2 | 1.400 / 1.020 | 358 | 358 | 4 | 2 |

**Lettura:** con una miniera da 2 Carbone/s, due fonderie di Ferro assorbono tutto il combustibile disponibile. La seconda miniera aumenta il throughput; con tre fonderie attive lascia anche crescere lo stock. Due miniere permettono invece quattro fonderie sempre attive dopo l'avvio.

## Invarianti controllate automaticamente

Il programma di misura `tools/SimRisorse.Simulation/IndustryScaleBenchmark.cs` usa `ForecastEconomy()` per rilevare gli addetti realmente attivi; ricostruisce **indipendentemente** consumi alimentari, input e output delle ricette; avanza la sessione e confronta le scorte per **ciascuno dei 180 secondi**. Controlla inoltre:

1. costo totale di ogni costruzione e capienza dei depositi;
2. popolazione ferma a 40 e assenza di crescita con abitazioni piene;
3. bilancio del Carbone: produzione meno consumo uguale giacenza finale;
4. Ferro/Oro e secondi-lavoratore bloccati secondo la priorità dichiarata;
5. confronto di due replay dello stesso profilo: `180 × 1 s` e `180 × (250 ms + 750 ms)`, con **risultati e snapshot finali identici**.

Il validatore confronta poi anche i cinque risultati con i valori della tabella e salva `industry-scale-benchmark.json` insieme alle evidenze del gate.

## Esecuzione richiesta su Windows

Da cartella principale del progetto:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-industry-scale
.\validate.cmd
```

Il primo comando esegue il singolo benchmark, il secondo l'intera verifica inclusi build, test xUnit, cronologie, benchmark e smoke desktop. **Questi comandi non sono stati eseguiti nell'ambiente di preparazione** perché non è disponibile .NET/PowerShell. I **206/206 test** documentati appartengono al gate Windows precedente dell'archivio locale dell'utente; non dimostrano ancora l'esito di M5.1c.

## Dopo il gate

Se il benchmark e `validate.cmd` passano, M5.1c è chiudibile senza nuove regole, versioni SQLite o mutazioni della GUI. Per M5.2 restano da scegliere **prezzi/ricette di Forgia e Zecca, utilizzo di Spade e Monete**; la misura della sola contesa industriale non approva quelle scelte.
