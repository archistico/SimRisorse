# RC77 / E4a.3 — Verifiche e risultati controllati

## Nuove regressioni

`E4aV15IndustrialRecoveryTests`: **14 casi** nel progetto Application. Coprono Forgia/Zecca sature, recupero con Magazzino pagato, assegnazione reale di due addetti al Carbone, vincoli misti non attribuiti due volte, esecuzione parziale dei lavoratori, diagnostica senza alterare il tick, rifiuto di dati non misurati o intervalli multipli, diagnostica zero su tre scenari, determinismo/esportazioni e argomenti invalidi.

```powershell
dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj --no-restore -c Release --filter FullyQualifiedName~E4aV15IndustrialRecoveryTests
.\validate-f1.cmd
```

Suite cumulativa: **899 casi** (885 HF04 + 14 RC77). Il gate richiede TRX, smoke esistente, screenshot, audit alimentari e la nuova CLI su tutti i sei run completi. Verifica durata/provenienza, pasti, azioni riuscite, categorie riconciliate e presenza dei quattro report. La lettura JSON tiene conto del comportamento di Windows PowerShell 5.1.

**Esito effettivo:** `validate-f1.cmd` completato con uscita 0 su Windows/.NET SDK 10.0.201, **899/899 superati**, nessun fallito o saltato, build con 0 errori e 0 avvisi. Core 212, Application 364, Infrastructure 129, Desktop 194. Smoke nativo esistente, audit alimentari e nuovo benchmark controllato superati.

Gate: `.local-cache/m4/f1-gates/3764e1684674430f9a600e49b18059af/f1-gate.json`. TRX: `.local-cache/m4/runs/6cade1068b1647aa83c48fcbbcbf45ec/test-results/`. È una verifica tecnica RC77; non promuove automaticamente la GUI/F1.

## Misure eseguite su Windows

Sono stati eseguiti i sei run a 120 secondi, poi ripetuti. **4/4 file identici byte per byte**: JSON, campioni CSV, azioni CSV e Markdown. Cinque chiamate CLI invalide rifiutate; un tentativo di riscrivere la directory esistente è stato rifiutato senza modificare i quattro file.

| Scenario | Politica | Prima produzione | Produzione lorda in 120s | Spesa dell'intervento |
| --- | --- | --- | --- | --- |
| V15-R01 | baseline | Non osservata | 0 Spade | 0 |
| V15-R01 | build-warehouse | 1s | **240 Spade** | 100 Legno lavorato + 60 Pietra |
| V15-R02 | baseline | Non osservata | 0 Monete | 0 |
| V15-R02 | build-warehouse | 1s | **360 Monete** | 100 Legno lavorato + 60 Pietra |
| V15-R03 | baseline | Non osservata per Forgia/Zecca | 0 Spade, 0 Monete | 0 |
| V15-R03 | assign-coal-workers | 2s | **238 Spade, 357 Monete** | 0 materiali; 2 abitanti assegnati |

Le baseline sature registrano rispettivamente 240 Spade e 360 Monete non prodotte **per solo spazio**, senza distruzione delle scorte iniziali. La baseline Carbone registra invece mancati input; l'intervento conserva il primo secondo di avvio senza Carbone disponibile. I pasti sono serviti per tutti i 120 secondi di ogni run; nessun inventario fisico squilibrato.

Evidenze locali:

- `.local-cache/e4a-v15/rc77-recovery-98be31fe7c6e495298560bfcd4efa56f/`.
- Replay: `.local-cache/e4a-v15/rc77-replay-269cdcfe8c8f4d4c8081a249ef3495bb/`.
- Il gate conserva inoltre i propri report nuovi in `f1-gates/<id>/industrial-recovery/`.

Gli inventari, gli edifici e i 40 residenti iniziali sono grants dichiarati: i numeri non sono tempi di progressione naturale né giustificano automaticamente nuove formule economiche. I benchmark naturali completi restano `not-run`. Il confronto interattivo della GUI e lo stress 24h restano separati.
