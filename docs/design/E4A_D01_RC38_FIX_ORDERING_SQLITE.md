# RC38 / E4a-D01-F1 — Fix confronto checkpoint SQLite

**Data:** 2026-11-04  
**Origine:** RC37 / E4a-D01  
**Classificazione:** falso negativo in un test d'integrazione; nessuna modifica al gioco.

## Evidenza Windows ricevuta

`validate.ps1` ha completato la compilazione e avviato i test; risultato **540 superati su 541**. Unico caso fallito:
`SimRisorse.Infrastructure.Tests.V14SchoolResearchEndToEndTests.PencilsToStudentToResearchToLargeTierSurvivesTwoSqliteRoundTrips`.
Il confronto in `AssertSameCampaign` segnala `OtherBuildings` diversi alla posizione 0: contenuto equivalente, ma ordine differente (`town-hall` nella snapshot della sessione, `barracks` in quella restituita da SQLite).

## Analisi e soluzione

`OtherBuildings`, `ProductiveFamilies`, `PurchaseCounters` e `UnlockedTechnologyIds` sono collezioni identificate da una chiave, **non sequenze ordinate**. Sono ora normalizzate per rispettiva identità con `StringComparer.Ordinal` prima del confronto. I record vengono confrontati integralmente: una quantità, un ID, un assegnato o un contatore difforme continuano a far fallire il test. `SchoolResearch.TrainingProgressSeconds` resta confrontato per posizione, poiché rappresenta la progressione individuale degli studenti e l'ordine è significativo.

La soluzione riprende quella già adottata in `V14SqliteSaveStoreTests.Equivalent` e `V14ReplayPersistenceTests.Equivalent`. Non viene alterato l'ordine esposto dal dominio né l'ordinamento SQLite e non vengono apportate modifiche alle query o allo schema.

## Ambito e verifiche

- Un solo file sorgente modificato: `tests/SimRisorse.Infrastructure.Tests/V14SchoolResearchEndToEndTests.cs`.
- Zero modifiche ai sorgenti applicativi, alle due GUI, ai costi, al runner E4a o ai salvataggi.
- Numero di test invariato: **541 attesi**.
- Verifiche statiche disponibili in questa consegna: confronto RC37→RC38, integrità ZIP, parsing dei file strutturati, controllo dei link Markdown.
- **Non è disponibile l'SDK .NET 10 in questo ambiente**: la RC38 deve ancora essere eseguita su Windows. Non attribuire un esito verde.

## Prossimi passi

Validare RC38 con `./validate.cmd`. Dopo il successo della suite, eseguire due run separate di `--benchmark-e4a-school` seguendo il piano di test RC37. Non segnare scenari misurati prima di eseguirli; E4b resta sospeso finché E4a non produce dati e i nuovi coefficienti non sono approvati.
