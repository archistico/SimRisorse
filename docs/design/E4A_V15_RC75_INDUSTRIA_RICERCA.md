# RC75 / E4a.2b — Benchmark naturali industria e ricerca v15

## Obiettivo e perimetro

Nuovo `V15IndustrialProgressionBenchmark` e analizzatore `V15IndustrialProgressionAnalysis`, indipendenti dai benchmark v14 e dai runner naturali RC73. Non cambiano la simulazione, il bilanciamento o i salvataggi.

Si parte con `CivilGrowthScenario.Create()`, `GameSession.Create()` e la stessa preparazione v15 del runner RC73, **senza aggiungere risorse, edifici, abitanti, tecnologia o personale**. Il laboratorio RC75 è diagnostico: non dimostra che i target siano economicamente raggiungibili, né che una strategia sia ottimale.

## Scenari e politiche

| ID | Obiettivo osservabile | Orizzonte dichiarato |
|---|---|---:|
| V15-I01 | Acquisto di `estrazione-1` tramite Scuola/ricercatore | 21.600 s (6 h) |
| V15-I02 | Prima Spada realmente prodotta dopo `estrazione-1` e `industria-1` | 43.200 s (12 h) |
| V15-I03 | Prima Moneta realmente prodotta dopo `estrazione-1` e `industria-1` | 57.600 s (16 h) |

Politiche dichiarate:
- `research-first`: Pescatore → Deposito → Casa → **costruzione anticipata della Scuola** → filiera Pane/Casa grande → Miniera di carbone → Matite → formazione/ricerca → metalli.
- `bread-first`: stesso avvio, **prima filiera Pane/Casa grande/Carbone**, poi costruzione della Scuola, Matite, formazione/ricerca → metalli.

Per ogni scenario le due politiche partono dal **medesimo checkpoint** e compiono **lo stesso insieme di azioni**, in ordine diverso. Questo è indispensabile per confrontare correttamente i costi e le attese, invece di confrontare programmi con prerequisiti incompleti. Sono script seriali, non un motore di pianificazione automatica: La GUI v15 approvata prevede **2 Matite per studente** (`pencilsPerStudent: 2`); una produzione da 4 Matite costa 1 Legname lavorato + 1 Carbone. Il Carbone richiede Miniera di carbone, a sua volta dipendente da Casa grande, Forno, Mulino e filiera agricola: perciò **non è possibile formare ricercatori prima della filiera Pane/Carbone** anche nella politica `research-first`. Questa politica differisce per l'anticipo dell'investimento della Scuola, non perché possa ignorare un prerequisito.

Le azioni bloccate da prerequisiti mancanti producono `blocked` (della strategia), attese di materiale/popolo/formazione/ricerca producono `waiting` o `timeout` all'orizzonte. Il planner prova acquisti ogni 60 secondi; dopo il completamento della costruzione osserva la produzione ogni secondo.

## Contabilità e risultati

- Ogni evento `executed` contiene l'intero `ResourceBundle` restituito dal comando riuscito, in milliunità per `resource_id` (incluse risorse non industriali e Cibo per iscrizioni); lo stock del gioco non è modificato dall'analizzatore.
- I punti ricerca effettivamente spesi per l'acquisto sono riportati separatamente in *millipunti*, non mescolati ai materiali. Non sono inclusi nei costi delle costruzioni.
- I consumi di lavorazione continui (Carbone, Ferro, ecc.) **non** sono «spese di costruzione»: sono misurabili dagli stock, ma non sommati impropriamente ai costi degli acquisti. Il prossimo incremento dovrà affrontare i costi indiretti reali dell'intera filiera.
- Campioni: popolazione, lavoratori liberi, Scuola, ricercatori formati e attivi, saldo ricerca, tecnologie, scorte fisiche di Spade e Monete.
- Stato: `pass`, `timeout` o `blocked`; `CompletionSeconds` è presente solo se una milestone è stata osservata. Orizzonte abbreviato = `Diagnostic=true`, non prova a orizzonte completo.
- L'analisi accetta dati coerenti, rifiuta duplicazioni/valori negativi e calcola differenze di tempo fra politiche **solo** per completamenti osservati a orizzonte pieno in entrambe.

## Comandi

```powershell
# Diagnostica breve, adatta a controllo iniziale (non è il benchmark finale):
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-industry --scenario V15-I01 --policy research-first --max-seconds 120 --output .local-cache/e4a-v15/industry-diagnostic-001

# Orizzonti completi, da eseguire solo dopo gate F1 e compilazione riusciti:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-industry --output .local-cache/e4a-v15/industry-measurement-001

# Analizza successivamente il JSON senza nuove simulazioni:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4a-v15-industry --input .local-cache/e4a-v15/industry-measurement-001 --output .local-cache/e4a-v15/industry-analysis-001
```

Il runner produce `e4a-v15-industry-results.json`, `...-events.csv`, `...-samples.csv`, `...-analysis.json`, `...-analysis.csv`, `...-analysis.md`. L'analizzatore indipendente genera soltanto gli ultimi tre. La CLI impedisce di sovrascrivere file risultato già esistenti. Non accede ai salvataggi personali.

## Limitazioni consapevoli

1. La politica seriale non esplora in autonomia alternative quando resta senza risorse o persone: `blocked` può essere della politica, non del modello. Per una diagnosi di softlock occorrono politiche di recupero e analisi dei prerequisiti/capienze.
2. L'orizzonte massimo è una decisione sperimentale dichiarata; non è un obiettivo di design approvato.
3. I costi tracciati sono spese **effettive delle azioni**. I costi indiretti di tutte le filiere e la concorrenza continua dei materiali restano E4a.2c.
4. Nessuna misura RC75 è stata eseguita nell'ambiente di consegna: manifest in stato `not-run`, confronto e tempistiche non determinati.
5. Il benchmark costruisce la Zecca **con i prezzi attuali** e senza i requisiti di dieci addetti che verranno studiati nella E4b; non confondere la situazione attuale con l'obiettivo futuro.

## Protezione architetturale

Nessuna modifica ai file Desktop/XAML, al Core, alla persistenza SQLite generation 6 o al modello v14. Il nuovo scenario viene eseguito da Application + CLI. Il gate cumulativo passa a 857 test previsti ma non validati.
