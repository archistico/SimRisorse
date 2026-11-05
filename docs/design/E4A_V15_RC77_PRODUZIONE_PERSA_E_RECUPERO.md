# RC77 / E4a.3 — Produzione non realizzata e recupero controllato

## Contratto

Misurare l'output nominale degli addetti assegnati e l'output effettivo delle filiere metallurgiche, distinguendo capacità, input e vincoli misti. Confrontare interventi reali su checkpoint iniziali identici, senza cambiare prezzi, ricette, personale, GUI o SQLite.

La revisione riprende dalla HF04 verificata con 885 test. I nuovi strumenti sono indipendenti dalla promozione grafica F1: il confronto interattivo Windows rimane pendente e non viene implicitamente dichiarato superato.

## Misura al confine del clock

`V15UnifiedCampaignClock.Plan(..., includeProductionDiagnostics: true)` espone opzionalmente `FirstSecondBuildings`: i report dello stesso planner che ha eseguito il primo secondo. Gli ID di input mancanti e output senza spazio vengono raccolti soltanto quando richiesti. Senza richiesta il campo resta `null`, cioè **non misurato**, e il percorso normale mantiene lo stesso comportamento.

Il benchmark avanza esattamente un secondo alla volta e riconcilia tutti gli inventari fisici con produzione e consumo lordi. Non deduce la produzione dalla differenza di scorta e non considera i trasferimenti come nuova produzione.

Per ogni famiglia/risorsa:

```text
nominale = addetti assegnati × output della ricetta per addetto
prodotto = addetti effettivamente eseguiti × output della ricetta
non realizzato = nominale − prodotto
```

Il nominale non è il massimo degli edifici posseduti e non include posti senza personale. L'output non realizzato non è scorta distrutta. Le categorie non si sovrappongono:

| Categoria | Condizione |
| --- | --- |
| Solo spazio | `StorageFull`, senza `MissingInputs` |
| Solo input | `MissingInputs`, senza `StorageFull` |
| Misto | Entrambi i vincoli; nessuna attribuzione esclusiva |
| Altro | Deficit residuo senza i due flag; riportato esplicitamente |

Un secondo vincolato può avere addetti parzialmente attivi. I contatori di secondi non significano che tutta la struttura sia rimasta ferma. I valori quantitativi restano al millesimo.

## Esperimenti dichiarati prima dell'esecuzione

Versione: `v15-controlled-industrial-recovery-1`. Orizzonte: **120 secondi**. Durate 0–119 sono diagnostiche, oltre 120 vengono rifiutate. Tre scenari, due politiche per scenario:

| Scenario | Stato iniziale controllato | Confronto |
| --- | --- | --- |
| V15-R01 | Scorta Spade alla capacità, Forgia con un addetto e input disponibili | Nessuna azione / un Magazzino acquistato a t=0 |
| V15-R02 | Scorta Monete alla capacità, Zecca con un addetto e input disponibili | Nessuna azione / un Magazzino acquistato a t=0 |
| V15-R03 | Un addetto al Carbone, due fonderie e Forgia/Zecca concorrenti; Carbone iniziale zero | Nessuna azione / due abitanti liberi assegnati alle miniere già possedute a t=0 |

La fixture fornisce e dichiara nei checkpoint JSON **40 abitanti, cinque case piccole, tre miniere di Carbone, altri siti industriali e scorte finanziate**, compresi 100 Cibo pronto. Non misura il tempo o il costo per ottenere questi asset giocando. Cinque case sono piene: la crescita ×20 approvata non modifica la popolazione del confronto. La razione di 0,4 Cibo/s è coperta per tutto l'orizzonte.

L'intervento usa `BuildCityBuilding` oppure `AssignCityProducerWorkers`; i materiali effettivamente pagati e l'esito sono registrati. Entrambe le politiche ricevono lo stesso checkpoint immutabile; nessuna riceve ulteriori grants durante il run. Nessun salvataggio personale viene aperto.

## Output e limiti

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-recovery --output DIRECTORY_NUOVA
```

Opzionali: `--scenario V15-R01|V15-R02|V15-R03`, `--max-seconds 0..120`. L'output contiene:

- `e4a-v15-recovery-results.json`: checkpoint iniziali/finali, metadati, azioni, campioni e totali.
- `e4a-v15-recovery-samples.csv`: un campione per secondo/famiglia, nominale, produzione, deficit classificati e ID vincolanti.
- `e4a-v15-recovery-actions.csv`: azioni e pagamenti effettivi.
- `e4a-v15-recovery.md`: risultati leggibili, unità e limiti.

La console emette un riepilogo JSON compatto. File esistenti vengono rifiutati. A zero secondi non vengono eseguite azioni né inventati campioni.

Questi esperimenti verificano il recupero **controllato**, non completamenti naturali o softlock del gioco. I manifest naturali RC73/RC75 restano distinti. Prima di E4b occorrono misure naturali a orizzonte pieno e valutazione delle strategie, senza dedurre nuovi prezzi da questi buffer sintetici.

[Risultati e verifiche](../validation/E4A_V15_RC77_TEST_PLAN.md) · [ADR 0085](../adr/0085-misure-produzione-non-realizzata-e-recupero-controllato.md).
