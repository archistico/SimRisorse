# E4a-D02 / RC39 — Benchmark naturali dalla colonia iniziale

**Data:** 2026-11-04. **Stato:** runner e test implementati; benchmark ancora da eseguire e verificare su Windows. **Baseline confermata:** RC38, 541 test passati secondo l'utente. **RC39:** 545 test attesi, non eseguiti qui.

## Obiettivo e portata

Introdotto `E4aNaturalBenchmark` per **E4A-01…E4A-05**. Tutte le prove partono da `CivilGrowthScenario.Create()` e `GameSession.Create(scenario).Snapshot()` convertito dalla sessione v14 con `V14CampaignSession.FromV13`. Nessuna risorsa, abitante, tecnologia o punto ricerca viene aggiunto al checkpoint. L'inventario originale (8 abitanti, 120 Legno lavorato, 80 Pietra, 30 Cibo, zero Carbone e Matite) viene conservato nel risultato prima della prima azione.

La **politica di riferimento `natural-reference-v1`** è deterministica e *non ottimizza* la strategia di gioco. Segue la sequenza costruzione Pescatore/Deposito alimentare, lavoratori, seconda Casa piccola, Fattoria/Pozzo/Mulino/Forno e relativi lavoratori, Casa grande, Miniera di carbone, Scuola, Matite, iscrizione, formazione, ricercatore, 100 punti, Lavorazioni I, upgrade Segheria Grande. Il runner prova immediatamente ogni operazione nell'ordine; quando mancano risorse o persone attende 60 secondi simulati e riprova. Non salta prerequisiti né crea stock. I cinque nomi di strategia nel manifest sono mantenuti, ma **tutti condividono temporaneamente la medesima policy**: non sono ancora cinque ottimizzazioni alternative.

| ID | Obiettivo verificato | Orizzonte massimo simulato |
| --- | --- | ---: |
| E4A-01 | Deposito operativo e Cibo prodotto dopo l'avvio | 14.400 s |
| E4A-02 | Pane presente, Casa grande e residente oltre la capienza delle Case piccole | 28.800 s |
| E4A-03 | Scuola e prima iscrizione reale | 28.800 s |
| E4A-04 | Acquisto effettivo della tecnologia Lavorazioni I | 43.200 s |
| E4A-05 | Segheria Grande effettivamente potenziata | 57.600 s |

**La presenza di un edificio non viene scambiata per produzione.** E4A-01 richiede un campione dopo il primo tick con Cibo superiore ai 30 iniziali; E4A-02 richiede Pane reale, una Casa grande e popolazione superiore alla capienza di base. E4A-03 richiede uno studente realmente iscritto. E4A-04/05 controllano gli sblocchi e i livelli nello snapshot.

## Dati esportati e limiti

- Serie temporali ogni 60 secondi: popolazione, disponibili, case, scuole, studenti in coda/formazione, ricercatori, Legno lavorato, Pietra, Cibo, Pane, Carbone, Matite e millipunti ricerca.
- Registro eventi con secondo simulato, fase, esito `executed`/`waiting`/`blocked` e motivo del rifiuto. L'evento `waiting` viene annotato al cambio di motivo, non ripetuto ogni minuto.
- `pass` soltanto per obiettivo osservato; `timeout` se la scadenza arriva prima; `blocked` per azioni non recuperabili. Il rapporto contiene un motivo e *non inventa* un tempo di completamento.
- Output JSON con eventi e campioni, CSV con campioni in formato `InvariantCulture`; non contiene fittizi hash build o checkpoint SQLite. Gli scenari non scrivono mai i salvataggi reali.
- I numeri finali saranno confrontabili solo dopo esecuzione su Windows; il manifest resta `not-run` fino a quel momento. Un fallimento del runner o un timeout **non dimostra automaticamente impossibilità di gioco**: potrebbe indicare una scelta debole della policy.

## Avvio e ripetibilità

```powershell
.\validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --scenario E4A-01 --output .local-cache/e4a/natural-01-run1
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --scenario E4A-01 --output .local-cache/e4a/natural-01-run2
# Per tutti e cinque, senza il filtro --scenario:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-natural --output .local-cache/e4a/natural-all-run1
```

Usare cartelle nuove: il tool rifiuta di sovrascrivere `e4a-natural-results.json` e `e4a-natural-results.csv`. Un exit code 1 può indicare anche `timeout`/`blocked`: leggere l'output e non cancellare le evidenze. Non confrontare i soli tempi di una policy con un obiettivo di durata totale del gioco.

## Invarianti

In RC39 sono modificati soltanto un nuovo benchmark in Application, il comando CLI, quattro test e documentazione. **Nessuna modifica a** Core, GUI v13/v14, contenuto economico, SQLite, salvataggi o parametri E4b. Le prove controllate E4A-06/07 restano separate, così da non attribuire al gioco naturale i loro fondi aggiuntivi.

[Protocollo E4a](E4A_PROTOCOLLO_BENCHMARK.md) · [Piano test RC39](../validation/E4A_D02_RC39_TEST_PLAN.md) · [ADR 0049](../adr/0049-benchmark-naturali-politica-esplicita.md)
