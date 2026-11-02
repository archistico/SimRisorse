# M5.2a — Forgia e Spade

**Verificato su Windows il 2026-11-02**, checkpoint **v37**, regole **`civil-growth-8`**. Catalogo: **18 tipi di edifici / 15 risorse**. Contratto A approvato dall'utente il 28/10; la Zecca segue nell'incremento M5.2b.

## Regole implementate

| Proprietà | Valore |
| --- | --- |
| Edificio / risorsa | `forge` Forgia / `swords` Spade |
| Costruzione | 240 Legno lavorato +180 Pietra |
| Prerequisito | Possesso di una Fonderia di ferro |
| Addetti | 1 per edificio |
| Ricetta per addetto/s | 1 Ferro +1 Carbone → 2 Spade |
| Priorità | 100, dopo Fonderia ferro 90 e Fonderia oro 91 |
| Scorta iniziale | Zero |
| Capacità | 1.000 +500 per Magazzino |

Il pianificatore esistente esegue ricette intere. Con Spade a 999/1.000 la Forgia non produce e non consuma Ferro/Carbone; con 998/1.000 entra un lotto esatto. I materiali prodotti diventano disponibili soltanto dal secondo successivo. Senza scorte pregresse, una filiera ferro/carbone e Forgia produce il primo lotto nel terzo passo economico.

Le Spade sono scorte per il futuro arruolamento. Non sono stati introdotti soldati, costi di reclutamento, Monete o pagamenti.

## Gate eseguiti e artefatti

Comando: `validate.cmd`, restore online con lock, SDK **10.0.201**, Avalonia **12.1.4**, Microsoft.Data.Sqlite **10.0.14**. Build Release: **zero avvisi / zero errori**.

| Gate | Run | Test |
| --- | --- | ---: |
| Baseline v36/M5.1c, prima di Forgia | `11a1fbf5ceff4a6a87fd52bd1126979a` | **211/211** |
| M5.2a Forgia | `909f039588b94b69b80b8d9e9d9bcd26` | **235/235** |

| Suite M5.2a | Superati |
| --- | ---: |
| Core | 67 |
| Application | 79 |
| Infrastructure | 32 |
| Desktop | 57 |

Il gate comprende raggiungibilità dei due cataloghi, **18 cronologie più crescita accelerata**, cinque benchmark e pubblicazione/smoke nativo Windows per entrambi gli scenari: salvataggio, caricamento, ripresa della crescita, sospensione e chiusura. Salvataggi di prova isolati da quelli personali.

Output locale: `.local-cache/m4/validation.json`, `forge-validation.log`, `forge-start.json`, `forge-8-hours.json`, `industry-scale-benchmark.json`; TRX e PNG sotto `.local-cache/m4/runs/909f039588b94b69b80b8d9e9d9bcd26/`. Sono artefatti locali ignorati da Git, non allegati al repository.

## Cronologia dalla nuova partita

`--scenario civil-growth --strategy forge` segue la strategia civile/ferro esistente: Cibo dall'avvio, Casa piccola a 61 s, filiera Pane a 600 s, Casa grande a 760 s, industria ferro a 1.100 s. A **1.400 s** acquista e assegna un addetto alla Forgia. Prezzi e assegnazioni passano attraverso i comandi reali.

| Misura | A 1.401 s | A 8 ore |
| --- | ---: | ---: |
| Spade | **2** | **1.000** |
| Ferro | 299 | 1.000 |
| Carbone | 301 | 1.000 |
| Legno lavorato | 70 | 1.000 |
| Pietra | 531 | 1.000 |
| Popolazione | 31 | 48 |

Questa è una cronologia di riferimento, non una strategia ottima o la durata della campagna completa. Non costruisce la filiera dell'Oro e non misura la futura contesa con la Zecca.

## Persistenza e regressioni

- Conversioni concatenate **v1–v7→v8**, con Forgia a quantità/personale zero e Spade zero. Capacità dei Magazzini già posseduti applicata anche alle Spade.
- Test con database SQLite reale v7: conservazione delle scorte frazionarie di Ferro/Oro/Carbone, popolazione, lavoratori, edifici, tempo e frazione della crescita; caricamento in pausa senza scrittura e backup originale prima del primo salvataggio v8. Salvataggio successivo senza duplicare il backup.
- Round trip di Spade frazionarie e addetto Forgia a metà secondo; ripresa esatta dopo altri 500 ms e nessuna produzione in pausa.
- Rifiuto di checkpoint antecedenti che dichiarano Spade positive o righe Forgia incompatibili; la partita corrente resta preservata.
- Test di primo lotto, Carbone prenotato dalle due fonderie, carenze Ferro/Carbone diagnosticate, capienza intera, ripresa dopo acquisto Magazzino, assenza di addetto, pausa e replay interi/frazionari.

SQLite/snapshot **v2**, slot civile e politica S1 invariati. [ADR 0015](../adr/0015-forge-swords-and-v7-save-upgrade.md).

## Interfaccia e correzioni della baseline

Descrizione Forgia, costo, ricetta, input e diagnosi usano la presentazione esistente. Aggiunte icone originali 8×8 di Forgia/Spade; Spade si inserisce nell'ordine alfabetico italiano della tabella Risorse.

**23 PNG generati**: 21 richiesti dal manifest più due dettagli. Ispezionati tutti i quattro nuovi render: `forge-buildings[-compact].png` e `forge-resources[-compact].png`, a **1040×740 / 1240×850**. Verificati ricetta, icone, controlli, colonne e margine delle scrollbar; tabella Spade a +2/s e riempimento indicativo 00:08:19 con 2/1.000.

La prima esecuzione sulla baseline ha trovato pennelli statici mutabili in `ResourceRow`, inizializzati fuori dal dispatcher e poi usati nel rendering UI. Sono ora immutabili. Un altro test leggeva il font da una riga costi rimossa dalla finestra al cambio selezione: confronta ora la riga corrente. Dopo queste correzioni il gate baseline passa **211/211**, prima dell'aggiunta delle nuove regole.

## Benchmark storici ed esperimento separato

I cataloghi v4/v5/v6 restano immutati. `CreateGoldForBenchmarks()` fissa **v7** per M5.1c e ForgeMintProbe: aggiungere Forgia al gioco non cambia il confronto industriale già definito e non duplica gli ID dell'esperimento.

L'esperimento separato è stato eseguito con `dotnet run --project experiments/forge-mint-probe/SimRisorse.ForgeMintProbe.csproj -c Release`, codice di uscita **0**: sei profili A/B più sei guardie F8/F9, ripetuti a passi interi/frazionari con confronto snapshot. Confermati **241 s a fasi / 122 s con due miniere** per 240 Spade +360 Monete. Risultati in `.local-cache/m4/forge-mint-probe.log`.

Sono misure su una colonia sintetica v7 con minerali precaricati e acquisti finanziati. Non sono una prova di Zecca nel catalogo giocabile né il completamento di M5.2c.

## Prossimo incremento e limiti

**M5.2b:** Zecca/Monete secondo il contratto A già approvato, con regole **`civil-growth-9`** e conversione `-8→-9`. Successivamente M5.2c confronterà le due politiche nel gioco corrente, includendo una cronologia dalla nuova partita.

Restano da scegliere quantità di Spade per recluta e Monete per nave nelle fasi militari/navali. Altre piattaforme, DPI fisici e accessibilità non sono coperti da questo gate Windows. Nessuna valutazione del divertimento o certificazione delle otto ore fino alla vittoria.
