# E4a-D01 / RC37 — Primo benchmark riproducibile sulla Scuola

**Stato:** implementato, **NON compilato/eseguito in questo ambiente**. Non esistono risultati misurati con RC37 fino all'esecuzione reale del comando. **Baseline confermata da Windows:** RC30 (511 test). RC31–RC37 hanno bisogno di validazione cumulativa (541 test attesi).

## Obiettivo e perimetro

L'esecutore `E4aSchoolBenchmark` confronta gli scenari **E4A-06** (una Scuola) ed **E4A-07** (due Scuole), lasciando inalterati costo della Scuola, produzione delle Matite, numero di posti, formazione e ricerca. Questo è **un benchmark controllato**, non un percorso di campagna naturale.

### Fixture dichiarata, uguale per entrambi

1. Si crea una campagna `CivilGrowthScenario.Create()` e se ne ricava lo snapshot v14 iniziale tramite la migrazione ordinaria *in memoria*, senza leggere/scrivere salvataggi reali.
2. Si aggiungono soltanto **600 Legno lavorato**, **400 Pietra** e **10 Carbone**, rispettando le capacità iniziali. Non si inventano residenti, ricercatori, punti ricerca, edifici già costruiti o tecnologie.
3. Mediante comandi normali di sessione si costruiscono Pescatore Base, Deposito alimentare Base e **una o due Scuole**; si fabbricano **4 Matite** usando **1 Legno lavorato e 1 Carbone**; si iscrivono due studenti pagando **10 Cibo + 2 Matite** a testa.
4. Si avanza il clock v14 reale di **30 secondi per campione**, fino alla qualifica di due studenti o al limite di **1.800 secondi**. I ricercatori qualificati non vengono assegnati automaticamente; il saldo ricerca deve rimanere tracciato.
5. Se un comando di preparazione o un tick fallisce, il benchmark termina **con errore** ed espone il comando: non dichiara un `pass` fittizio.

**Comparabilità:** entrambe le prove partono dalla stessa dotazione iniziale e dalle stesse iscrizioni. La seconda Scuola comporta il suo costo aggiuntivo effettivo; pertanto il confronto isola la *capacità di formazione*, non un ipotetico beneficio economico netto. La produzione di Cibo è lasciata senza lavoratori: è uno scenario controllato sulla coda della Scuola, non una strategia autosostenibile.

## Risultati prodotti (solo dopo l'esecuzione)

- `e4a-school-results.json`: per ciascuno scenario il ruleset, la fixture, tutti i comandi di preparazione, la serie cronologica, il tempo del primo qualificato, il tempo di qualifica di entrambi e l'esito `pass`/`timeout`.
- `e4a-school-results.csv`: un campione per riga, numeri con formato invariato, stock reali di Cibo/Legno lavorato/Pietra/Carbone/Matite, popolazione totale/disponibile, in attesa/in formazione/qualificati/attivi e millipunti ricerca.
- Entrambi gli output sono **misure del clock di gioco**, non secondi di tempo reale, non stime di performance della CPU. Se manca l'esecuzione non si devono attribuire numeri alla RC37.
- Non si scrivono `build_sha256` né `checkpoint_id` inesistenti; il benchmark resta in memoria e non crea checkpoint SQLite. Il responsabile della campagna di test registra separatamente l'hash ZIP/commit e il percorso della run.

**Contratto operativo:** lo strumento non sovrascrive file di misura esistenti; usare una directory distinta per ogni run. `.local-cache` è ignorata da Git e non deve finire negli archivi di distribuzione. Il registro canonico `benchmarks/e4a/scenarios.csv` resta `not-run` fino alla validazione effettiva.

## Esecuzione Windows (.NET 10)

```powershell
.\validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-001
```

Verificare i due output, ripetere con `school-run-002`, confrontare gli output normalizzati, conservare hash del progetto e log. Il comando senza `--output` stampa solo JSON su stdout.

## Limiti dichiarati e attività successive

- **E4a-D02:** implementare strategie *naturali* E4A-01…05 senza iniezione di risorse, con log dei tentativi falliti e timeout; nessuna simulazione anticipata inventata.
- **E4a-D03:** misurare contesa Carbone e magazzini E4A-08/09; niente assunzioni sul tempo di inattività.
- **E4a-D04:** confronto con salva/riprendi SQLite per E4A-10 e report quantitativo; distinguere throughput, tempi di accesso e bilancio dei materiali.
- **E4b:** proporre cambi di costo/ricetta solo dopo dati realmente raccolti e autorizzazione dell'utente.

[Protocollo E4a](E4A_PROTOCOLLO_BENCHMARK.md) · [Registro dei 10 scenari](../../benchmarks/e4a/scenarios.csv) · [Piano di test](../validation/E4A_D01_RC37_TEST_PLAN.md) · [ADR 0048](../adr/0048-benchmark-scuole-controllato.md)
