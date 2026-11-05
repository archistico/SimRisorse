# ADR 0072 — Canonicalizzazione delle famiglie produttive nel checkpoint v15

**Stato:** accettato per RC64, candidato ai test Windows. **Data:** 2026-11-05. **Decisione precedente:** [ADR 0069](0069-generazione6-senza-famiglia-produttiva-legacy.md). **Vincolo UI:** [ADR 0071](0071-gui-v14-contratto-canonico-v15.md).

## Contesto

SQLite generation 6 non persiste la famiglia `food-store`, ma la rilettura la ricreava a zero lavoratori nel checkpoint in RAM. Dopo l'approvazione grafica RC63 non vogliamo introdurre una seconda interfaccia né nuove regressioni visive.

## Decisione

Il normale stato v15 include 15 famiglie produttive. La proiezione v14 a 16 famiglie esiste esclusivamente durante `V14CampaignSnapshots.Validate`, `V14CampaignSession.Restore` e le diagnosi di retroproiezione; nessuna di queste proiezioni viene assegnata allo stato ufficiale del gioco. Durante il clock, `ProductionPlanner` riceve uno slot aggiuntivo locale inattivo per il catalogo storico. Sul percorso SQLite si caricano direttamente le 15 famiglie archiviate.

La funzione di rimozione agisce **solo** sulla famiglia storica senza edifici e senza lavoratori. Per un produttore ancora occupato il salvataggio continua a rifiutare perdite silenziose e resta necessario il ritiro esplicito. I contatori storici restano temporaneamente preservati perché `V14CampaignSession` li usa nei calcoli della progressione.

## Conseguenze e limiti

- Nessuna modifica allo schema SQL (generation 6), al catalogo v14 o ai salvataggi personali.
- Nuove partite v15, costruzioni, salvataggi e riprese mantengono una collezione a 15 famiglie.
- Non è ancora una riscrittura nativa dei comandi civici; per quella resta F1-D05e.2b.2b, prima dei benchmark E4a.
- I test .NET 10 devono confermare la correttezza del refactoring. Gli audit statici non sostituiscono xUnit.

## Alternative scartate

Sostituire immediatamente `V14CampaignSnapshot` e riscrivere l'intera API civica in un'unica patch: troppa superficie non validata, soprattutto subito dopo il ripristino della UI. Rimuovere d'ufficio uno slot legacy occupato: perdita di lavoratori e edifici senza consenso.
