# Prova industriale — ipotesi A/B

**Stato: esperimento eseguito, regole industriali non adottate nel gioco.** Il catalogo futuro da 22 tipi è stato scelto dall'utente; prezzi e ricette di questa prova restano le ipotesi del [dossier M5.1](../../docs/design/M5_1_INDUSTRY_PREFEASIBILITY.md).

Questo eseguibile usa Core e Application esistenti per confrontare gli investimenti industriali. Il suo scenario `industry-probe`, con versioni `industry-hypothesis-a-1` / `industry-hypothesis-b-1`, vive soltanto in questa cartella. Non è selezionabile nel desktop e non carica o salva partite. Non aggiunge pacchetti esterni e non appartiene alla soluzione di produzione.

## Esecuzione

Dalla radice del repository:

```powershell
dotnet build experiments/industry-probe/SimRisorse.IndustryProbe.csproj -c Release
dotnet experiments/industry-probe/bin/Release/net10.0/SimRisorse.IndustryProbe.dll
```

L'esecuzione senza argomenti scrive il confronto JSON su stdout e termina con codice 0 se tutti i controlli sono passati. `--help` descrive l'uso; argomenti sconosciuti sono rifiutati con codice 2. Per il restore offline con la cache locale:

```powershell
dotnet restore experiments/industry-probe/SimRisorse.IndustryProbe.csproj --locked-mode --packages .local-cache/nuget --source .local-cache/nuget -p:NuGetAudit=false
dotnet build experiments/industry-probe/SimRisorse.IndustryProbe.csproj --no-restore -c Release
```

La validazione principale `validate.cmd` rimane distinta: non costruisce né esegue automaticamente questa prova.

## Stato iniziale e ipotesi

Colonia sintetica già avviata, derivata dalle regole civili `civil-growth-5`:

- **40 abitanti e 40 posti**, una Casa piccola e una grande. La capienza piena mantiene costante la popolazione.
- Taglialegna, Segheria e Tagliapietra con un lavoratore ciascuno.
- Pescatore e Deposito alimentare con un lavoratore ciascuno; due Fattorie, Pozzo, Mulino e Forno con cinque lavoratori complessivi.
- **10 lavoratori inizialmente assegnati**, 30 liberi; Cibo 100 e Pane 100, tutte le altre scorte a zero.
- Nessun Magazzino iniziale. Capacità industriali ipotetiche 1.000; ogni Magazzino aggiunge 500 anche alle cinque risorse nuove. Il suo costo civile resta 100 legno / 60 pietra.

I costi della colonia iniziale sono esclusi. Non si tratta di una cronologia dalla nuova partita né di una migrazione da un salvataggio civile.

| Edificio | Costo A legno/pietra | Costo B | Ricetta per secondo e lavoratore |
| --- | ---: | ---: | --- |
| Miniera ferro | 120 / 80 | 180 / 120 | → 2 minerale ferro |
| Miniera carbone | 130 / 90 | 195 / 135 | → 2 carbone |
| Fonderia ferro | 200 / 150 | 300 / 225 | 2 minerale ferro + 1 carbone → 1 ferro |
| Miniera oro | 180 / 120 | 270 / 180 | → 1 minerale oro |
| Fonderia oro | 220 / 170 | 330 / 255 | 1 minerale oro + 1 carbone → 1 oro |

Un lavoratore per struttura. Le miniere richiedono la Casa grande; le fonderie le miniere corrispondenti e quella di carbone. Allocazione deterministica: miniere prima delle fonderie, ferro prima dell'oro. Come nel motore civile, l'output del passo non può alimentare un'altra produzione nello stesso passo.

## Otto confronti

Ogni ipotesi A/B viene eseguita con il trio ferro/carbone e con l'intera rete ferro+oro, sotto due politiche:

1. **`whole-budget`:** attendere le risorse per l'intero investimento, poi acquistare e assegnare tutte le strutture. Se il budget supera la capienza, comprare prima un Magazzino appena finanziabile.
2. **`as-affordable`:** comprare e assegnare una struttura appena possibile nell'ordine dichiarato: miniera ferro → miniera carbone → fonderia ferro → eventuale miniera oro → fonderia oro. Le prime produzioni possono partire prima che la rete sia completata.

Ogni profilo dura **3.600 secondi**, seguiti dall'acquisto effettivo di un Magazzino e da un secondo di ripresa. I costi `Financing*Spent` escludono quest'ultimo Magazzino di verifica, che costa ulteriori 100/60; l'evento è comunque presente in `ConstructionEvents` e nel controllo contabile. `ConstructionSecond` indica quando l'intera rete del profilo è costruita; il primo ferro può precederlo negli acquisti progressivi.

## Controlli eseguiti

- Raggiungibilità del catalogo sperimentale, costruzioni e assegnazioni tramite i comandi del Core.
- Per ogni secondo, scorte effettive uguali a scorte precedenti meno input/consumi più output; riconciliazione cumulativa anche dei costi di costruzione.
- Scorte entro la capienza e colonia alimentata, con popolazione e progresso invariati.
- Avvio delle fonderie dopo la disponibilità degli input; saturazione osservata dei metalli e ripresa dopo l'espansione pagata del deposito.
- Ripetizione di ogni caso con passi da **1 s** e **250 ms + 750 ms**: uguaglianza dell'intero rapporto e dello snapshot finale. Il primo quarto di secondo non deve già accreditare produzioni.

Ogni caso riconcilia **3.601 secondi** per entrambe le modalità: otto confronti e sedici esecuzioni, distinti dai test xUnit della soluzione principale. I contatori di blocco possono sovrapporsi quando esistono più cause; `Factories` descrive i 3.600 secondi prima della prova finale di ripresa.

## Risultati misurati il 2026-10-27

| Ipotesi | Profilo | Acquisto | Primo ferro (s) | Primo oro (s) | 100 ferro (s) | 100 oro (s) | Spesa legno/pietra |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| A | Ferro | Budget intero | 453 | — | 552 | — | 450 / 320 |
| A | Ferro | Progressivo | 452 | — | 551 | — | 450 / 320 |
| A | Ferro + oro | Budget intero | 853 | 853 | 952 | 952 | 850 / 610 |
| A | Ferro + oro | Progressivo | 452 | 852 | 551 | 951 | 850 / 610 |
| B | Ferro | Budget intero | 678 | — | 777 | — | 675 / 480 |
| B | Ferro | Progressivo | 677 | — | 776 | — | 675 / 480 |
| B | Ferro + oro | Budget intero | 1.378 | 1.378 | 1.477 | 1.477 | 1.375 / 975 |
| B | Ferro + oro | Progressivo | 677 | 1.277 | 776 | 1.376 | 1.275 / 915 |

Solo il profilo B ferro+oro con budget intero compra il Magazzino di finanziamento, al secondo **101**. Il costo industriale è 1.275/915; la spesa effettiva sale a 1.375/975. Il profilo progressivo evita quel Magazzino: il costo aggregato superiore a 1.000 **non è un ostacolo inevitabile**, perché i singoli acquisti svuotano le scorte.

Con acquisti progressivi, B ritarda di **225 s** il primo ferro e di **425 s** il primo oro rispetto ad A. Non si può ricavare il ritmo della campagna dal solo moltiplicatore 1,5× dei prezzi. Alla fine dell'ora i metalli raggiungono la capienza: 1.000, oppure 1.500 nel solo profilo con Magazzino anticipato. L'espansione finale permette a ogni fonderia costruita di produrre un'altra unità nel secondo successivo.

Artefatto locale: `.local-cache/industry-probe/runs/a419e939ab024a5fa6573135d31dedff/comparison.json`, con `manifest.json` nella stessa cartella: SDK, impronte SHA-256 dei sorgenti/input e riferimento al nuovo gate principale. La build finale è stata rieseguita e la sua uscita coincide con il JSON conservato. Le tabelle riportano misure di questo esperimento; i risultati non scelgono automaticamente A o B.

## Limiti e prossimo passo

La prova usa un ordine di acquisto fisso, non ottimizza gli investimenti. Non comprende il costo di avviamento civile, domanda di metalli a valle, Forgia/Zecca, reclutamento, missioni, migrazioni o una condizione di vittoria. La possibilità di finanziare e saturare queste produzioni in una colonia sintetica non chiude D0.6 per la campagna completa.

Prima dell'implementazione nel desktop restano da scegliere vittoria/isole, contratti militari e navali, politica S1/S2, parametri economici e chiusura progettuale M4. Questa prova rende A/B confrontabili senza adottarne silenziosamente le regole.
