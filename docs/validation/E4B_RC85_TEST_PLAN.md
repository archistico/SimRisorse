# RC85 — piano di verifica delle strategie abitative per Zecca

## Versioni e verità dei test

- **Ultima conferma reale dell'utente:** RC82 **1002/1002 superati**.
- **Non verificati qui:** RC83 +18, RC84 +18, RC85 +20, quindi **1058/1058 test previsti** con `validate-f1.cmd`; l'SDK .NET 10 non è disponibile nel container.
- Gate statico/archivio: non equivalgono a build, xUnit o f1-gate verde.

## Regressioni aggiunte (20 casi xUnit)

1. I due regimi (`MintTen`, `Legacy`) pianificano **due costruzioni** aggiuntive prima della filiera Oro, senza modificare le tappe comuni.
2. L'assegnazione finale della Zecca `MintTen` resta atomica a 10 lavoratori e non viene scambiata con `staff-mint`.
3. I benchmark Scuola e Forgia non accettano la nuova policy; le due policy storiche conservano i loro passi.
4. A t=0 esito diagnostico, nessun completamento inventato; JSON originale e originale Legacy restano versionati e senza grant.
5. Analisi di un regime o dei due regimi; controllo `bread-first` obbligatorio e `MintTen` necessario.
6. Gli analizzatori RC75/76/84 riconoscono la nuova policy soltanto per lo scenario Zecca.
7. I run con versione sconosciuta, inventario diverso, milestone fittizia o input vuoto sono respinti.
8. Export CSV e Markdown distinguono l'assenza di prova e non valorizzano tempi fittizi.
9. Replay JSON, determinismo, risorse non negative e partenza naturale senza mutazioni.

## Gate F1

`validate-f1.cmd` chiama `scripts/validate-f1-gate.ps1` con **1058 test attesi**. Conserva tutti i gate RC72–RC84; in più invoca `--benchmark-e4b-mint-ten --max-seconds 0 --include-housing-buffer --compare-legacy`, verifica **sei** run (`3 politiche × 2 regimi`) diagnostici e confronta il file grezzo con `--review-e4b-housing-buffer`. Il rapporto deve contenere **due coppie** e **zero confronti di tempo completi**, nonché SHA-256 e tre file di output. I test Desktop e lo smoke Windows rimangono obbligatori.

## Run integrali da eseguire quando disponibile

1. `validate-f1.cmd`, raccogliere TRX, report F1 e screenshot manuali della GUI v14/v15.
2. Run completo con `--include-housing-buffer --compare-legacy --output CARTELLA_NUOVA` e nessun `--max-seconds`. Annotare gli eventuali timeout senza tradurli in softlock.
3. Audit RC84 sul JSON delle sei osservazioni, quindi confronto RC85 offline. Confrontare `research-first`, `bread-first` e `housing-buffer` senza confondere le ipotesi di prezzo A/B/C con prezzi misurati.
4. Se la variante abitativa attende risorse, confrontare la capacità dei magazzini e le ricevute di costruzione. Se attende lavoratori, esaminare gli abitanti liberi e i tempi demografici; nessuna modifica ai prezzi o alle ricette prima di decidere la politica di design.

## Vincoli immutati

Nessun file sotto Core/Desktop/Infrastructure cambia in RC85. La GUI canonica, i 7 campi Risorse, edifici bloccati consultabili, pulsanti, ricerca, capacità e icone, crescita ×20, SQLite generation 6, salvataggi personali e costi legacy devono restare invariati. La nuova strategia non viene installata nella sessione utente.
