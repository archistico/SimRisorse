# E4b / RC80 — Squadre minime con il planner reale, senza attivazione

## Contratto di calcolo

Proposta: Zecca con `CrewSize=10`; i tier rappresentano moduli produttivi 1/3/10, con 10/30/100 posti per edificio. Il numero di addetti *assegnati* è una quantità di abitanti; quello dei moduli *operativi* è un'altra quantità.

- `CrewTeamPortfolio` è un modello immutabile autonomo; calcola assegnati, moduli, squadre complete, addetti impegnati in squadre incomplete, assunzioni mancanti per il prossimo modulo. `Build`, `Assign` e `Release` validano capacità e disponibilità, senza mutare l'istanza originaria.
- `CrewProductionProjection` costruisce il roster per il planner: **numero moduli** come `BuildingState.Count`, **numero addetti** come `BuildingState.AssignedWorkers`; rifiuta duplicati, famiglie non produttive, posti eccedenti e popolazione insufficiente.
- Il parametro `crewSizes` del `ProductionPlanner` è usato SOLO dall'API precedente. Per tutte le chiamate v14/v15 preesistenti il default resta un addetto per esecuzione della ricetta.
- Le risorse sono spese per il numero effettivo di **moduli eseguiti**, al netto dei vincoli di input e storage del planner condiviso. Nessun consumo di un modulo incompleto. Competizione fra produttori secondo priorità originale.

## Casi controllati

| Scenario | Edifici e tier | Addetti | Moduli potenzialmente attivi | Monete teoriche per tick con input/spazio sufficienti |
| --- | --- | ---: | ---: | ---: |
| two-base-0 | 2 Base | 0 | 0 | 0 |
| two-base-9 | 2 Base | 9 | 0 | 0 |
| two-base-10 | 2 Base | 10 | 1 | 3 |
| two-base-19 | 2 Base | 19 | 1 | 3 |
| two-base-20 | 2 Base | 20 | 2 | 6 |
| large-29 | 1 Grande | 29 | 2 | 6 |
| large-30 | 1 Grande | 30 | 3 | 9 |
| advanced-100 | 1 Avanzato | 100 | 10 | 30 |
| mint-no-gold | 1 Base | 10 | 1 | 0 |
| coin-storage-full | 1 Base | 10 | 1 | 0 |

Il laboratorio usa `V15BenchmarkFixture.Create().Baseline.Catalog` e quindi la vera ricetta Zecca (1 Oro + 1 Carbone → 3 Monete per modulo e secondo), disabilitando le assegnazioni alle altre famiglie nel roster di prova. Per lotti ordinari concede **20 Oro e 20 Carbone**, spazio 1000 per ogni risorsa e 200 residenti teorici. Non confondere con materiali guadagnati in partita. La proiezione dello stesso secondo serve a verificare solo contabilità produttiva e diagnosi, non il clock alimentare.

## Riproducibilità

```powershell
 dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-crews --output .local-cache/e4b-v15/rc80-crews-001
```

La CLI crea `e4b-crews.json`, `.csv`, `.md` soltanto se nessuno esiste già. Senza `--output` stampa un riepilogo JSON. `validate-f1.cmd` include questo comando fra i gate, senza aprire SQLite.

## Passi dopo RC80

Validare la candidata (965 test previsti), decidere parametri di tier/prezzo A/B/C e flusso di pagamento; solo successivamente preparare un unico percorso giocabile v15 con comandi di assegnazione/upgrade, proiezione GUI, snapshot SQLite e test round-trip/pausa. L'economia deve superare nuovi run *naturali* completi e verifiche di capacità; il test di produzione su un singolo tick non dimostra raggiungibilità.

[ADR](../adr/0088-proiezione-squadre-minime-v15-isolata.md) · [Test](../validation/E4B_RC80_CREW_TEST_PLAN.md).
