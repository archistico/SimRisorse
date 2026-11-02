# Prova isolata M5.2 — Forgia, Zecca e Carbone

Preparata il **2026-10-27**, **eseguita il 2026-11-02**. **Esperimento isolato**, separato dal catalogo del gioco. Il contratto A è stato approvato il 28/10; B resta un confronto sperimentale. Forgia è ora nel gioco v8, Zecca resta futura. Il [dossier M5.2](../../docs/design/M5_2_FORGE_MINT_PREFEASIBILITY.md) conserva le scelte e i limiti delle misure.

## Che cosa misura

Il progetto legge il catalogo **storico** `civil-growth-7` tramite `CreateGoldForBenchmarks()` e crea in memoria lo scenario `forge-mint-probe` con Forgia/Zecca, Spade/Monete per i confronti A/B. Questo mantiene riproducibile l’esperimento anche dopo l’introduzione di Forgia nel gioco v8. Non apre né migra salvataggi personali; usa i normali comandi di `GameSession`.

Tre configurazioni vengono ripetute per prezzi **A** e **B** (sei profili):

| Configurazione | Carbone | Personale | Orizzonte | Spade attese | Monete attese | Primo prodotto | Obiettivo 240/360 |
| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: |
| `staged` | 1 miniera | Fonderie fino a 121 s; poi addetti spostati a Forgia/Zecca | 241 s | 240 | 360 | Secondo 122 | Secondo **241** |
| `parallel` | 2 miniere | Due fonderie, Forgia e Zecca insieme | 122 s | 240 | 360 | Secondo 3 | Secondo **122** |
| `always-staffed` | 1 miniera | Due fonderie, Forgia e Zecca insieme | 180 s | 0 | 0 | Nessuno entro 180 s | Non misurato |

La **v34** aggiunge anche **sei controlli di confine** (tre casi per prezzo A/B), sempre sul catalogo ipotetico e con edifici acquistati tramite `GameSession`:

| Controllo | Scorte sintetiche e capienza | Forecast atteso | Dopo 1 secondo |
| --- | --- | --- | --- |
| `storage-near-full` | Spade **1.999/2.000**, Monete **1.998/2.000** | Forgia e Zecca bloccate: manca spazio per i rispettivi lotti **2/3** | Scorte inalterate; nessun input consumato |
| `storage-exact-fit` | Spade **1.998/2.000**, Monete **1.997/2.000** | Entrambe attive: i due lotti entrano esattamente | Spade **2.000**, Monete **2.000**, consumo di 2 Carbone |
| `coal-priority` | 2 Carbone, 1 Ferro e 1 Oro iniziali; una miniera di Carbone, due fonderie e due trasformatori presidiati | Le fonderie prendono i 2 Carbone, Forgia/Zecca segnalano **Carbone mancante** | 0 Spade/Monete; 2 Ferro, 2 Oro e 2 Carbone |

La capienza di **2.000** deriva dal magazzino base ipotetico di 1.000 per risorsa **più due Magazzini da 500** già presenti nel checkpoint; questi controlli traducono F8/F9 in prove eseguibili e **non** sostituiscono i successivi test da nuova partita.

Ipotesi sintetiche: **40 abitanti e 40 posti**, 1.000 unità di ciascun minerale già disponibili, Miniere di ferro e oro acquistate ma non presidiate, 1.000 Cibo e Pane, due Magazzini, 1.700 Tavole e Pietra iniziali. Tutti gli edifici industriali sono **acquistati attraverso le azioni reali**; il costo include anche le Miniere di ferro e oro non presidiate. I tempi non comprendono costruzione, raccolta, percorso da nuova partita o conseguenze militari. La priorità delle nuove strutture (**100/101, dopo le fonderie 90/91**) è un'ipotesi, non una regola attuale.

| Prezzi | Una miniera: Tavole/Pietra | Due miniere: Tavole/Pietra |
| --- | ---: | ---: |
| A: Forgia 240/180; Zecca 210/160 | 1.300 / 950 | 1.430 / 1.040 |
| B: Forgia 360/270; Zecca 315/240 | 1.525 / 1.120 | 1.655 / 1.210 |

## Garanzie della prova

- Ricostruisce il consumo di Cibo/Pane e gli input/output delle ricette dal forecast e **riconcilia ogni secondo con lo stato reale**.
- Controlla acquisti, lavoro disponibile, capienza, popolazione ferma a 40 e bilancio del Carbone.
- Esegue ciascuno dei sei profili con passi da **1 secondo** e con coppie **250 ms + 750 ms**, confrontando risultati e snapshot finali.
- Ripete anche i sei controlli di confine con passi interi e frazionari, verificando l'indivisibilità dei lotti, il blocco visibile nel forecast e la precedenza delle fonderie con Carbone scarso.
- Confronta il risultato simulato con gli oracoli aritmetici attesi, compresi i 241/122 s. Se qualcosa diverge, segnala errore ed esce con codice 1.
- Non cambia i benchmark storici, la soluzione principale né il validatore `validate.cmd`.

## Come eseguire in seguito su Windows

```powershell
dotnet run --project experiments/forge-mint-probe/SimRisorse.ForgeMintProbe.csproj -c Release
```

Output: un JSON con i sei profili nella proprietà `Profiles` e sei verifiche nella proprietà `Guards`, stato `ISOLATED_EXPERIMENT` e indicatori dei tempi, delle scorte e dei costi. Il comando è separato dalla validazione ordinaria, che continua a misurare esclusivamente regole già presenti nel gioco.

**Verificato il 02/11:** comando terminato con codice 0; sei profili A/B e sei guardie completati, ogni caso confrontato a passi interi/frazionari con snapshot uguali. Confermati **241 s a fasi / 122 s paralleli** per 240 Spade +360 Monete. Output `.local-cache/m4/forge-mint-probe.log`. Sono misure su colonia sintetica con minerali precaricati e acquisti finanziati, non tempi dalla nuova partita né chiusura di M5.2c.

## Dopo la conferma delle scelte

Forgia/Spade sono ora verificate nel gioco v8. Il prossimo passo è Zecca/Monete in v9 secondo il contratto già approvato; poi M5.2c confronterà le filiere complete e una cronologia dalla nuova partita. L’esperimento non viene aggiunto al gate ordinario.