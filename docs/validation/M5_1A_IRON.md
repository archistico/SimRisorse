# M5.1a — Ferro e carbone integrati

Data: 2026-10-27. **Stato: Verificato su Windows.** `validate.cmd` completo termina con codice **0**. Le regole correnti sono **`civil-growth-6`**; il catalogo ha **15 tipi di edifici e 12 risorse**.

## Decisioni effettivamente ricevute

- Prototipo retro con font monospace apprezzato e accettato come base per proseguire.
- Demolizione **rinviata** a un incremento successivo. Carne già fuori dalla prima campagna per D0.1. M4 può essere chiusa.
- D0.2: per vincere occorre **possedere tutti i 22 tipi** della prima campagna, con il **Castello conclusivo**; gli edifici iniziali contano già.
- D0.3: **una conquista obbligatoria sblocca il Castello**. Identità dell'isola, soldati, costi, durata e fallimenti restano da definire.
- S1: continuare lo scenario e lo slot civile, con conversione e backup dell'originale.
- M5.1a: approvata la **configurazione A** sotto elencata, limitatamente a ferro/carbone.

Catalogo, vittoria e conquista sono scelte progettuali. In questo incremento diventano giocabili soltanto i tre nuovi edifici industriali e le relative risorse; vittoria e missioni non sono ancora implementate.

## Filiera giocabile

| Edificio | ID | Legno / pietra | Addetti | Ricetta per secondo e addetto | Prerequisiti |
| --- | --- | ---: | ---: | --- | --- |
| Miniera di ferro | `iron-mine` | 120 / 80 | 1 | → 2 minerale di ferro | Casa grande |
| Miniera di carbone | `coal-mine` | 130 / 90 | 1 | → 2 carbone | Casa grande |
| Fonderia di ferro | `iron-smelter` | 200 / 150 | 1 | 2 minerale + 1 carbone → 1 ferro | Le due miniere |

Nuove risorse: `iron-ore`, `coal`, `iron`. Capacità base **1.000** ciascuna; ogni Magazzino aggiunge **500**. Nessuna struttura o scorta industriale iniziale. Il trio costa **450 legno / 320 pietra** e richiede tre lavoratori; costruzione e assegnazione sono comandi distinti.

La UI conserva font monospace e controlli esistenti, con sei nuovi pittogrammi originali a celle 8×8, descrizioni e ricette. Prerequisiti mancanti, assenza di personale, input insufficienti e depositi pieni usano la diagnostica esistente. Le dodici risorse scorrono nell'area della tabella, senza spostare i comandi esterni.

## Salvataggi S1

Nuovo passaggio dichiarato **`civil-growth-5 → civil-growth-6`**, concatenabile da `-1…-4`. Il formato SQLite/snapshot resta **v2**. Le tre righe industriali vengono aggiunte a quantità/personale zero; le risorse nuove sono implicitamente zero. I Magazzini già posseduti aumentano anche le nuove capacità.

Tempo frazionario, popolazione, scorte civili e due parti del progresso di crescita si conservano. Il caricamento avviene in pausa e non scrive il file. La prima scrittura crea il backup `<nome>.civil-growth-5.backup.sqlite3` quando il file sostituito è v5; per versioni precedenti conserva il nome della versione originale. Le copie esistenti non si sovrascrivono.

Test su SQLite reale verificano file originale invariato al caricamento, backup ricaricabile, scritture ripetute senza nuove copie inutili e ripresa della produzione a **5,5 secondi**, attraversando esattamente il mezzo secondo successivo. Vecchi checkpoint con scorte industriali positive incompatibili vengono rifiutati mantenendo la partita corrente.

## Validazione eseguita

Run: `.local-cache/m4/runs/4a1f84832c6249ecbd2c7a85ec50d990/`. SDK **10.0.201**, Avalonia **12.1.4**, Microsoft.Data.Sqlite **10.0.14**. Build Release senza avvisi/errori.

Conservate nel run le copie dei nove JSON usati per questa lettura in `report-inputs/`, con `report-inputs.sha256.json`. TRX, render e log nativi rimangono nelle rispettive cartelle dello stesso run; una validazione successiva non sovrascrive queste copie.

| Suite | Superati | Falliti |
| --- | ---: | ---: |
| Core | 67 | 0 |
| Application | 54 | 0 |
| Infrastructure | 28 | 0 |
| Desktop | 42 | 0 |
| **Totale** | **191** | **0** |

I 14 casi aggiuntivi rispetto al gate M4 sono: sei casi applicativi industriali, due ulteriori versioni storiche nella teoria di conversione, due casi SQLite e quattro render/flussi Desktop industriali. Non sono stati aggiunti pacchetti o modificati gli algoritmi del Core.

Il gate include controlli di raggiungibilità dei due cataloghi, dodici cronologie storiche, due nuove cronologie industriali, prova di crescita accelerata corrente, quattro benchmark abitazioni/Pane, pubblicazione e smoke nativi Windows per entrambi gli scenari. Generati **13 PNG**; ispezionati i nuovi layout industriali compatto/standard e la tabella delle risorse compatta. DPI su monitor fisici e altre piattaforme restano fuori da questa verifica.

## Cronologia da nuova partita

La strategia deterministica `industry` usa comandi reali: filiera Cibo al tempo zero, seconda Casa piccola a 61 s, filiera Pane a 600 s, Casa grande a 760 s, trio ferro/carbone a 1.100 s. Non usa dotazioni sintetiche aggiuntive.

| Momento | Misura osservata |
| --- | --- |
| 1.100 s | Trio costruito e assegnato; scorte industriali ancora zero |
| 1.101 s | Prime estrazioni, nessun ferro ancora disponibile |
| **1.102 s** | **1 ferro**, 2 minerale, 3 carbone; popolazione **24** |
| 8 ore | Ferro **1.000**, popolazione **48**; scorte entro capienza |

Il test applicativo porta il ferro a deposito pieno e verifica l'arresto; costruendo e pagando un Magazzino, la fonderia riprende e porta il ferro a **1.001** nel secondo successivo.

Riproduzione dopo il build:

```powershell
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --scenario civil-growth --strategy industry --seconds 1102
```

Output locali: `.local-cache/m4/industry-start.json`, `industry-8-hours.json` e `validation.json`. Gli esperimenti abitativi storici restano esplicitamente v4/v5; il catalogo corrente v6 non li sostituisce.

## Prossimo incremento

M5.1b potrà aggiungere l'oro e misurare la domanda condivisa di carbone, dopo la scelta dei suoi parametri. La configurazione A accettata in questa sessione vale **solo per M5.1a**. Restano sette tipi del catalogo da 22 non implementati e i contratti militari/navali prima delle rispettive fasi. Il Ferro per ora si accumula: Forgia e impiego a valle sono incrementi successivi.
