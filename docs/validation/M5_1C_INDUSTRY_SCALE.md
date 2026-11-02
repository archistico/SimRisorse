# M5.1c — Contesa del Carbone e confronto di scala

Aggiornato il **2026-11-02**. **Verificato su Windows:** cinque profili con bilancio per secondo e replay interi/frazionari confermati nel gate baseline da **211/211** (run `11a1fbf5ceff4a6a87fd52bd1126979a`) e nel gate M5.2a da **235/235** (run `909f039588b94b69b80b8d9e9d9bcd26`). Le quantità attese della tabella sono state confermate. Il catalogo v7 resta fissato con `CreateGoldForBenchmarks()`; [report corrente](M5_2A_FORGE.md).

**Nota 2026-10-28:** l'utente ha confermato «test passati» sulla v33, nella quale `validate.cmd` include questo benchmark. Non è stato fornito l'output del gate o il JSON `industry-scale-benchmark.json`: la tabella rimane quindi etichettata **teorica**, senza attribuirle misure eseguite né un'identità di run non documentata.

## Scopo e confini

Misurare l'effetto di una o due Miniere di carbone su gruppi diversi di Fonderie di ferro e Fonderie d'oro, conservando i prezzi e le ricette approvati in M5.1a/M5.1b. La priorità del Ferro sull'Oro è già una regola: questo incremento **non** la cambia.

Il benchmark parte da un **checkpoint sintetico**, non dall'inizio di una campagna. Usa il catalogo eseguibile `civil-growth-7`, istanzia una partita con 40 abitanti, 40 posti (una Casa piccola e una grande), un Forno senza addetti e due Magazzini. Imposta 1.700 Tavole, 1.700 Pietra, 1.000 Minerale di ferro, 1.000 Minerale d'oro, 100 Cibo, 100 Pane e zero Carbone/Ferro/Oro. Costruisce con i **comandi reali di `GameSession`** le Miniere di ferro e d'oro, lasciandole senza addetti, quindi le Miniere di carbone e le Fonderie, assegnandovi i lavoratori.

Questo isolamento permette di misurare la contesa del Carbone senza attribuire all'industria le attese causate dalla catena civile, dai prezzi di una partita nuova o dalla disponibilità dei minerali. Il primo secondo alimenta la scorta di Carbone; le Fonderie iniziano nel secondo successivo, secondo l'ordine effettivo del pianificatore.

## Cinque profili: valori attesi confermati il 02/11

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

Il primo comando esegue il singolo benchmark, il secondo l'intera verifica inclusi build, test xUnit, cronologie, benchmark e smoke desktop. Il gate è stato **eseguito e ispezionato il 02/11** con M5.1c, prima sulla baseline da 211 test, poi su M5.2a da 235 test. Gli esiti da 206 test e la conferma utente v33 restano evidenze storiche distinte.

## Dopo il gate

**M5.1c Verificata** senza cambiare le regole industriali. Il contratto A Forgia/Zecca è stato approvato separatamente dall'utente il 28/10; Forgia/Spade è ora verificata in v8. Il prossimo incremento è Zecca/Monete in v9; le quantità per reclutamento e navi restano da scegliere nelle fasi successive.
