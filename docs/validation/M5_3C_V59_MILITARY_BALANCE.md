> **ESITO SUCCESSIVO, 03/11/2026:** l'utente ha confermato **tutti i test di v59-RC1 superati** con `validate.cmd` su Windows. È chiusa la validazione tecnica di questo gate, **non** l'analisi delle misure economiche reali: sono ancora da acquisire `military-balance-8-hours.json` e `military-capacity-benchmark.json`. I paragrafi sottostanti descrivono il protocollo alla data di consegna della RC1.

# M5.3c — misurare esercito, formazione e costo-opportunità (v59-RC1)

**Checkpoint:** 3 novembre 2026. **Baseline:** v58-RC5 `civil-growth-13`. L'utente ha dichiarato superato integralmente `validate.cmd` per RC5; non è disponibile in questa consegna il log numerico delle misure. **v59-RC1 non è ancora validata su Windows.**

## Obiettivo e perimetro

Misurare il reclutamento **prima di fissare i requisiti delle isole**, mantenendo invariati motore, catalogo, costi, capacità, SQLite, GUI e dati dei salvataggi. M5.3c non implementa navi, viaggio, combattimento o nuove risorse.

La regola corrente è **1 recluta per posto/Caserma, 30 secondi per recluta, 5 Spade/persona**, con costo **260 Legno lavorato e 200 Pietra per Caserma**. Reclute e soldati sono già parte della popolazione residente, non sono lavoratori disponibili e non consumano razioni aggiuntive rispetto agli abitanti. Il modello di addestramento è deterministico.

## Due prove che non devono essere confuse

| Prova | Cosa misura | Cosa non misura |
| --- | --- | --- |
| **Controllata** `--benchmark-military-capacity` | Posti, throughput, costo esatto delle Spade, numero di persone temporaneamente sottratte al lavoro, effetto di 1/2/4 Caserme | Tempo per produrre Spade, costruire, far crescere residenti o mantenere il Pane |
| **Campagna reale** `--scenario civil-growth --strategy military --hours 8` | Risultato dopo 8 ore simulando gli acquisti e le risorse di partenza del ruleset v13 | Tempo minimo dimostrato per **tutti** i contingenti 5/10/20, scenario di conquista (non implementato) |

Il primo benchmark usa **60 abitanti**, **15 lavoratori già assegnati** e una dotazione di Spade sufficiente ad arruolare subito. È un ambiente di test isolato, **non** una partita reale e **non** una proposta di scorte iniziali. Nessun edificio ulteriore o popolazione viene creato artificialmente nello scenario di campagna.

## Valori teorici del contratto v13

| Caserme | Obiettivo | Spade | Risorse per le sole Caserme (Tavole / Pietra) | Tempo formazione con risorse pronte |
| ---: | ---: | ---: | ---: | ---: |
| 1 | 5 | 25 | 260 / 200 | 150 s |
| 1 | 10 | 50 | 260 / 200 | 300 s |
| 1 | 20 | 100 | 260 / 200 | 600 s |
| 2 | 5 | 25 | 520 / 400 | 90 s |
| 2 | 10 | 50 | 520 / 400 | 150 s |
| 2 | 20 | 100 | 520 / 400 | 300 s |
| 4 | 5 | 25 | 1040 / 800 | 60 s |
| 4 | 10 | 50 | 1040 / 800 | 90 s |
| 4 | 20 | 100 | 1040 / 800 | 150 s |

Formula con tutte le reclute già arruolate al tempo zero:

`durata = ceil(reclute / (caserme * postiPerCaserma)) * secondiPerRecluta`

La prima recluta pronta arriva dopo 30 s in tutti i profili. I costi non comprendono Forgia, miniera, fonderie, abitazioni, Cibo, Pane e i tempi di produzione degli edifici precedenti. Per 20 soldati sono necessari almeno 20 cittadini non già impiegati, oltre ai lavoratori che devono tenere attive le filiere.

## Riproduzione su Windows

Dalla root della **v59-RC1 estratta in una directory nuova**:

```powershell
.\validate.cmd

dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --benchmark-military-capacity

dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --scenario civil-growth --strategy military --hours 8
```

Il validatore scrive automaticamente nella `.local-cache/m4/`:

- `military-capacity-benchmark.json` — nove profili controllati con assert su costi, tempi e persone.
- `military-8-hours.json` — stato completo della strategia reale già esistente.
- `military-balance-8-hours.json` — riepilogo leggibile di soldati, code, popolazione, lavoratori disponibili, scorte e razioni.
- `validation.json` — percorsi dei report e risultato complessivo, se il validatore arriva alla fine.

**Non inserire nel repository un risultato simulato inventato.** Solo gli output ottenuti dal PC Windows possono essere riportati come misure reali. Il validatore continua a proteggere i vecchi benchmark v9 e verifica `civil-growth-13` separatamente.

## Criteri di uscita M5.3c

1. Il validatore completo deve essere verde, compresi i **9 nuovi casi parametrici Core**, il benchmark controllato e il probe reale di otto ore.
2. Esaminare i valori effettivi di `military-balance-8-hours.json`, non soltanto il suo esito: soldati pronti, Spade residue, disponibilità civili, blocchi di crescita, Cibo e Pane consumati.
3. Se necessario, aggiungere una seconda fase sperimentale con obiettivi 5/10/20 a partire dalla colonia reale, rilevando **quando** ogni contingente diventa disponibile, non solo la fotografia delle otto ore.
4. Soltanto dopo questi dati scegliere i requisiti militari dell'isola obbligatoria, rispettando l'obiettivo complessivo di campagna di circa otto ore per un giocatore efficiente (obiettivo non ancora dimostrato).
5. Prima di utilizzare le partite personali, provare conversioni di salvataggi reali su copie e conservare sempre il backup esterno: il successo del validatore verifica i salvataggi sintetici, non prova ogni DB storico esistente.

## Esito attuale

**M5.3c strumentata, non ancora chiusa.** Nessun numero misurato sul nuovo codice v59-RC1; test e benchmark da eseguire su Windows. Le scelte per flotta, numero di isole, soldati richiesti e durate dei viaggi restano proposte e sono materia di G1.
