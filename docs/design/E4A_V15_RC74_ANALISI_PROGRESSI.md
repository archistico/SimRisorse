# RC74 / E4a.2a — Analisi dei dati naturali v15

**Stato:** codice e test scritti, **non compilati o eseguiti in questo ambiente**. Ultima RC confermata: RC63 (771/771). RC73: 834 test attesi; RC74: **843** attesi (9 nuovi). Il gate F1 RC72 e i benchmark v15 naturali non hanno ancora evidenza Windows.

## Obiettivo

Trasformare i JSON grezzi `V15NaturalProgressionRun` di RC73 in un confronto verificabile, senza cambiare la simulazione. Nessun parametro economico è stato modificato. La CLI non genera scorte, edifici, popolazione o checkpoint; **legge un file JSON già prodotto** dal benchmark v15 e produce:

- `e4a-v15-analysis.json`: dati strutturati e distinzione fra esito e classificazione;
- `e4a-v15-analysis.csv`: una riga per scenario con eventi, scorte finali e costi tracciati;
- `e4a-v15-analysis.md`: riepilogo leggibile, avvertenze e passi per approfondire.

```powershell
# 1. Solo dopo il gate F1, eseguire gli orizzonti completi:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --output .local-cache/e4a-v15/misura-001

# 2. Analizzare i report senza eseguire di nuovo la simulazione:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4a-v15-natural --input .local-cache/e4a-v15/misura-001 --output .local-cache/e4a-v15/analisi-001

# Può essere passato anche il percorso esatto del file e4a-v15-natural-results.json.
```

L'analisi è deliberatamente *post hoc*: eseguirla su una prova abbreviata va bene per controllarne il formato, ma il report continua a identificarla come `diagnostic-only`. **Non è un benchmark economico confermato**.

## Significato rigoroso delle classificazioni

| Classe | Evidenza | Interpretazione ammessa |
| --- | --- | --- |
| `diagnostic-only` | Limite ridotto rispetto all'orizzonte dichiarato | Diagnostica tecnica, non tempo confrontabile |
| `milestone-observed` | `pass` e evento `milestone` al tempo riportato | Il risultato è stato ottenuto dalla specifica strategia |
| `script-policy-timeout` | Azione ancora da completare a fine orizzonte | La strategia non è arrivata all'obiettivo; **non** prova softlock |
| `production-not-observed-within-horizon` | Azioni completate, produzione non osservata entro il termine | Analizzare ricette/consumi/ritmi prima di concludere |
| `script-policy-blocked` | Il runner segnala un blocco terminale | Prerequisito o limite dello script; non impossibilità universale |

I vincoli `resources`, `population`, `prerequisite-or-technology` e `other-or-unknown` derivano **soltanto dai motivi di errore registrati dal comando**; il tool non attribuisce una causalità economica ulteriore.

## Costi, tempi e attendibilità

- Il CSV RC73 registra **solo** `planks` e `stone` per gli acquisti eseguiti. La RC74 ne somma i milliunits e li dichiara spese *parziali*. **Non** quantifica ancora il costo complessivo degli input avanzati (Oro, Ferro, Carbone ecc.): richiederà un ledger di costo multi-risorsa.
- `WaitingSeconds` misura gli intervalli fra un evento `waiting` e la successiva decisione rilevante, oppure la fine del run. Lo script RC73 prende decisioni ogni **60 secondi**; non è il tempo ottimale di un giocatore umano.
- Gli scenari sono indipendenti, con condizioni iniziali naturali. Il file deve specificare la fixture `fresh-game/no-grants`; altri profili vengono respinti.
- Il postprocessore verifica eventi monotoni, campioni monotoni, orizzonte coerente, marker del completamento e ID univoci. Rifiuta input incompleti o contraddittori, invece di produrre un rapporto falsamente numerico.
- La CLI controlla i nomi dei tre file di output e rifiuta la sovrascrittura, ma non sostituisce un archivio transazionale: in caso di errore disco ispezionare la directory parziale.

## Criteri di uscita

1. Suite cumulativa RC74 verde (**843/843** attesi su Windows), gate F1 completato, conferma grafica della GUI v15.
2. Runner RC73 validato, esecuzioni a orizzonte pieno per V15-N01/N02/N03, con metadati della versione .NET e dei ruleset.
3. Analisi dei risultati con motivi dei blocchi: distinguere limiti della politica seriale, capienza, personale e carenza risorse.
4. Estendere il runner in **E4a.2b** a percorsi avanzati (Scuola, ricerca, Forgia, Zecca) e strategie alternative. La Zecca richiede una misurazione dell'investimento complessivo prima di decidere curve E4b.
5. Non promuovere nessun coefficiente E4b finché la suite e gli esperimenti quantitativi non sono stati confermati.

[Runner RC73](E4A_V15_RC73_NATURAL_RUNNER.md) · [Piano test RC74](../validation/E4A_V15_RC74_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
