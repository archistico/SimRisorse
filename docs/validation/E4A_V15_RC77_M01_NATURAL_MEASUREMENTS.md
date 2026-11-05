# RC77-M01 — Misure naturali complete v15 (05/11/2026)

## Esecuzione e provenienza

Eseguiti i tre scenari alimentari RC73 e i sei abbinamenti industria/politica RC75 sul codice RC77 con crescita HF04 fino a ×20. Tutti partono dalla partita nuova v15: **8 abitanti, 120 Legno lavorato, 80 Pietra e 30 Cibo**, senza ulteriori risorse, edifici, persone o tecnologie concessi.

Non è stato passato `--max-seconds`: ogni run dispone dell'intero orizzonte dichiarato. Come previsto dal contratto, il runner termina alla milestone osservata; non continua a simulare fino alla scadenza dopo averla raggiunta. Le decisioni dello script avvengono ogni 60 secondi durante le attese, con osservazione al secondo della prima produzione dopo le costruzioni.

**Risultato:** 9/9 milestone osservate, 0 timeout, 0 blocchi, 0 run diagnostici. Entrambe le CLI e gli analizzatori completano con uscita 0.

Checkpoint del codice: RC77; .NET SDK **10.0.201**. Workspace con modifiche non committate: il solo HEAD Git non identifica l'eseguibile. `provenance.json` registra HEAD/stato dirty, SHA-256 di **144 file sorgente/configurazione** e delle assembly Core/Application/Simulation realmente eseguite. Non sono cambiati codice, formule o salvataggi durante questa misura.

## Tempi osservati

| Scenario | Milestone | Politica | Orizzonte disponibile | Tempo osservato |
| --- | --- | --- | ---: | ---: |
| V15-N01 | Primo nuovo Cibo prodotto dal Deposito | natural-reference-1 | 4h | **1s** |
| V15-N02 | Prima produzione di Pane | natural-reference-1 | 8h | **8m 01s** (481s) |
| V15-N03 | Prima Selvaggina prodotta | natural-reference-1 | 4h | **1s** |
| V15-I01 | Estrazione I acquistata | research-first | 6h | **43m** (2580s) |
| V15-I01 | Estrazione I acquistata | bread-first | 6h | **42m** (2520s) |
| V15-I02 | Prima produzione di Spade dopo Industria I | research-first | 12h | **1h 17m 03s** (4623s) |
| V15-I02 | Prima produzione di Spade dopo Industria I | bread-first | 12h | **1h 16m 03s** (4563s) |
| V15-I03 | Prima produzione di Monete dopo Industria I | research-first | 16h | **1h 17m 03s** (4623s) |
| V15-I03 | Prima produzione di Monete dopo Industria I | bread-first | 16h | **1h 16m 03s** (4563s) |

`bread-first` precede `research-first` di **60 secondi** in tutti e tre i confronti, con gli stessi materiali pagati. È una differenza fra queste due politiche seriali, pari a un intervallo decisionale, non una dimostrazione della strategia ottimale.

## Spese effettivamente registrate

Valori convertiti dai millesimi dei JSON alle unità di gioco. Le righe industriali sono uguali nelle due politiche dello stesso scenario.

| Scenario | Legno lavorato | Pietra | Carbone | Cibo | Matite | Punti ricerca spesi |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| V15-N01 | 100 | 50 | Non tracciato | Non tracciato | Non tracciato | Non tracciato |
| V15-N02 | 552 | 346 | Non tracciato | Non tracciato | Non tracciato | Non tracciato |
| V15-N03 | 70 | 35 | Non tracciato | Non tracciato | Non tracciato | Non tracciato |
| V15-I01 | 963 | 596 | 1 | 10 | 2 | 150 |
| V15-I02 | 1523 | 1006 | 1 | 10 | 2 | 350 |
| V15-I03 | 1573 | 1046 | 1 | 10 | 2 | 350 |

Il log RC73 registra soltanto Legno/Pietra: gli altri valori mancanti non sono zeri misurati. Il ledger RC75 registra i pagamenti delle azioni (compresi crafting e iscrizione), non tutti i consumi continui di ricette e pasti. Risorse diverse e punti ricerca non devono essere sommati come se fossero una moneta unica. Le cifre non sono una valutazione completa del costo indiretto della campagna.

## Vincoli e differenze fra politiche

| Scenario | Politica | Attesa materiali | Formazione | Attesa punti ricerca | Produzione finale |
| --- | --- | ---: | ---: | ---: | ---: |
| V15-I01 | research-first | 960s | 120s | 1500s | 0s: milestone di ricerca |
| V15-I01 | bread-first | 900s | 120s | 1500s | 0s: milestone di ricerca |
| V15-I02/I03 | research-first | 960s | 120s | 3540s | 3s |
| V15-I02/I03 | bread-first | 900s | 120s | 3540s | 3s |

La Scuola viene costruita a 240s in `research-first` e a 900s in `bread-first`. Eppure il ricercatore è assegnato rispettivamente a **1080s** e **1020s**: costruire prima la Scuola non evita i prerequisiti Pane/Casa grande/Carbone/Matite. La prima politica paga l'investimento prima della filiera e, in questa sequenza, ritarda di un minuto l'inizio della ricerca.

L'attesa dei punti ricerca è il contributo più lungo nei percorsi industriali. I 3540s includono il campionamento decisionale; non sono un nuovo coefficiente della ricerca.

I percorsi industriali terminano con **48 abitanti**: 37 liberi alla prima tecnologia, 34 liberi alle prime Spade/Monete. Il minimo campionato è 3 in `research-first` e 2 in `bread-first`; non sono registrate attese per personale in questi run.

L'analisi RC76 registra 27–95 osservazioni di saturazione per abbinamento. Sono coppie risorsa/campione, non secondi di arresto e non una misura causale dell'output perso. RC77 quantifica il recupero in esperimenti separati; non va trasferito automaticamente a questi run naturali.

## Verifiche di riproducibilità

- Ripetuti tutti i nove run.
- Confrontati **12/12 file** di risultati/eventi/campioni/analisi industriali delle due esecuzioni: identici byte per byte.
- Rigenerata l'analisi alimentare dal replay: **3/3 file identici**.
- Rianalizzato indipendentemente il JSON industriale: **6/6 file identici**.
- Totale: **21/21 confronti identici**. Verificati provenienza senza grants, orizzonti integrali, scorte iniziali canoniche e una milestone registrata all'istante dichiarato per ogni run.

Non è stata ripetuta la suite per una modifica al solo materiale di misura/documentazione. L'ultimo gate del codice invariato rimane **899/899**, SDK 10.0.201, con audit RC77 superati: `.local-cache/m4/f1-gates/3764e1684674430f9a600e49b18059af/f1-gate.json`.

## Evidenze e riproduzione

Directory della misura: `.local-cache/e4a-v15/rc77-natural-1793917768540/`.

- `provenance.json`: identificazione del codice/assembly e configurazione.
- `measurement-validation.json`: controlli eseguiti e confronti dei file.
- `food/` e `food-analysis/`: risultati alimentari e analisi.
- `industry/` e `industry-reanalysis/`: risultati industriali, costi e vincoli.
- `replay/`: seconda esecuzione indipendente.

```powershell
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --benchmark-e4a-v15-natural --output DIRECTORY_NUOVA_FOOD
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --analyze-e4a-v15-natural --input DIRECTORY_NUOVA_FOOD --output DIRECTORY_NUOVA_ANALISI
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --benchmark-e4a-v15-industry --output DIRECTORY_NUOVA_INDUSTRY
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --analyze-e4a-v15-industry --input DIRECTORY_NUOVA_INDUSTRY --output DIRECTORY_NUOVA_RIANALISI
```

## Implicazioni per il prossimo passo

Le milestone dichiarate sono raggiungibili con le regole attuali e queste politiche; non è dimostrata la durata di una campagna completa. I target storici di 8h/32h non sono validati da un primo lotto di Spade/Monete.

La Zecca corrente lavora ancora con un addetto: il futuro requisito di **10 addetti per unità** e il suo impatto devono essere studiati, non considerati già implementati. Prima di modificare i prezzi, RC78/E4b deve confrontare le tre curve candidate con costo diretto/indiretto, domanda di personale, ricerca, storage e possibilità di recupero. I coefficienti richiedono la decisione prevista dalla roadmap.

Il confronto interattivo Windows della GUI resta pendente e non è sostituito da queste misure indipendenti.
