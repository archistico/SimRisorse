**Aggiornamento E4a-D01 / RC37 (non eseguita):** è disponibile il primo runner per gli scenari controllati E4A-06/07 (`dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-001`). Esporta serie temporali JSON/CSV solo quando viene eseguito. **Non è una misura di raggiungibilità naturale**; i 10 stati nel manifest restano `not-run` fino a conferma. [Rapporto e fixture](E4A_D01_RC37_BENCHMARK_SCUOLE.md).

# E4a — Protocollo di analisi economica, prima del bilanciamento

**Preparato in RC36 / P1-D50. NON ancora eseguito: nessun tempo misurato.**

## Scopo

Individuare stalli inevitabili e trade-off nella v14 ordinaria: sviluppo civile e Pane, accesso alla Scuola, consumo Matite, crescita della ricerca, sblocco tier Grande/Avanzato, disponibilità di manodopera, stoccaggio e uso del Carbone concorrente con l'industria. Gli esiti numerici saranno proposte per **E4b**, non modifiche automatiche al gioco.

## Protocollo riproducibile

1. Prima eseguire il gate cumulativo RC36 (`./validate.cmd`, 538 test attesi); annotare versione, hash dell'archivio e ruleset `civil-growth-14`.
2. Ogni scenario del [CSV canonico](../../benchmarks/e4a/scenarios.csv) comincia da checkpoint nuovo isolato. Non utilizzare i file reali in `%LOCALAPPDATA%`.
3. Nelle prove `natural`, usare esclusivamente stock/popolo/costruzioni iniziali del gioco e comandi raggiungibili, senza modifiche dirette allo snapshot. Nelle prove `controlled`, riportare **tutte** le aggiunte alla fixture: vanno usate per isolare un singolo collo di bottiglia, non per dimostrare raggiungibilità.
4. Avanzare soltanto il clock simulato mediante `V14CampaignSession.Advance`; registrare ogni acquisto, assegnazione e tick. Durante la pausa il tempo non progredisce.
5. Raccogliere i dati a ogni milestone e periodicamente (intervallo operativo proposto 60 s simulati; per durate lunghe scegliere batch entro `MaximumAdvance`). Ogni misura ha un `elapsed_seconds` e un identificativo del checkpoint.
6. Fissare prima della prima esecuzione la strategia (`manual-reference`, `growth-first`, `research-first`, `storage-first`), il limite di tempo e le condizioni di stop. Ripetere ogni strategia con le stesse azioni; un nuovo algoritmo di scelta viene versionato a parte.
7. Riportare anche esiti negativi: `blocked` per prerequisiti/risorse impossibili, `timeout` oltre l'orizzonte, `not-applicable` per funzionalità future. Mai compilare un tempo inventato.

## Misure necessarie

- Tempo simulato al primo deposito alimentare, primo Pane, prima Scuola, primo studente qualificato, primo ricercatore assegnato, primo acquisto tecnologico, prima costruzione tier Grande e Avanzato.
- Stock minimo/massimo e tempo con magazzino pieno per Legno lavorato, Carbone, Cibo, Pane, Matite.
- Produzione, consumo e saldo netto di ciascuna risorsa; tempo di inattività per input mancanti o capacità esaurita.
- Popolazione totale, disponibile, lavoratori, studenti, qualificati, ricercatori attivi e reclute/soldati per ogni snapshot significativo.
- Ricerca accumulata/spesa (millipunti); sblocchi e prerequisiti; contatori di acquisto e costo dei lotti.
- Eventi di blocco con codice del comando e motivo; per ogni run `pass` / `blocked` / `timeout`.

## Confronti

- Una Scuola contro due: la formazione parallela dimezza le code solo se popolazione e Matite lo permettono; è **ipotesi da misurare**, non risultato.
- Produzione Matite manuale rispetto al fabbisogno dell'industria del Carbone: quale filiera resta a corto di input e per quanto tempo?
- Magazzino iniziale contro Magazzini aggiuntivi: il rendimento marginale dello stoccaggio è sufficiente a giustificare il costo?
- Acquisto diretto del livello Grande contro upgrade da Base: costi cumulativi e personale disponibile, con identico capitale iniziale.
- Edilizia abitativa avanzata/Panificazione contro ricerca: percorsi alternativi e ricadute sulla crescita degli abitanti.

## Interpretazione e limiti

- **Un test di integrazione finanziato non dimostra** che la Scuola sia raggiungibile da una partita naturale senza interventi o in un certo tempo.
- I 120 s di formazione + 1000 s per 100 punti con un ricercatore a 0,1 punti/s sono una relazione aritmetica delle regole attuali; **non sono il tempo reale per costruire la Scuola**, reperire input o formare il primo ricercatore.
- Il gioco completo richiederà ancora Cantiere, navi, conquista e Castello: la durata obiettivo di 8 ore non può essere certificata dalla sola v14 civile.
- I risultati E4a dovranno includere versione, fixture, strategia, log delle azioni, seed (se esiste casualità), output e motivo dell'eventuale mancato completamento.

[Registro scenari](../../benchmarks/e4a/scenarios.csv) · [Template risultati](../../benchmarks/e4a/results-template.csv) · [Consolidamento P1-D50](P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md)
