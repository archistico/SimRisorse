# RC76-HF02 — Esiti Windows e diagnostica E4a (05/11/2026)

> Rapporto storico HF02 con 869 test. La successiva [HF03](RC76_HF03_VISUAL_REGRESSIONS.md) corregge regressioni grafiche e supera il gate con **874 test**, incluse otto catture v15.

Esecuzioni effettuate da Codex nel workspace Windows corrente con .NET SDK **10.0.201**, dopo la correzione del test `FreshCampaignBenchmarkAndLiveSessionAgreeAfterInitialActions`.

## Correzione della regressione

Il test confrontava i 5 lavoratori liberi della partita nuova con un campione del benchmark V15-N01 che aveva già costruito e assegnato Pescatore e Deposito al secondo zero. La sessione esegue ora le stesse quattro azioni prima del confronto: restano **3 lavoratori liberi** in entrambi gli stati. Sono verificati anche eventi eseguiti, istante zero e proiezione del checkpoint aggiornato. Nessuna modifica al comportamento del benchmark o al calcolo del personale.

## Validazione automatica completata

| Verifica | Esito |
| --- | --- |
| `./validate.cmd` | Uscita 0; compilazione Release con 0 errori e 0 avvisi |
| Suite cumulativa | **869/869** superati; 0 falliti, 0 saltati |
| Core / Application / Infrastructure / Desktop | 212 / 339 / 129 / 189 test |
| Simulazioni e benchmark storici di `validate.cmd` | Superati |
| Smoke nativo Windows | Entrambi gli scenari; salvataggio, caricamento, ripresa crescita, sospensione e chiusura |
| `./validate-f1.cmd` | Uscita 0; `status=passed`, contatori TRX verificati |
| Audit v15 baseline e combinato | 120 secondi ciascuno, accettati dal gate |
| Stress combinato 24h | Non eseguito |
| Verifica visiva manuale della GUI | Da confermare; i test e lo smoke non la sostituiscono |

Evidenze locali (cache non inclusa in Git):

- Validazione diretta: `.local-cache/m4/runs/4a80ae353d1a4826acaec808eac4a062/`.
- Gate: `.local-cache/m4/f1-gates/896122fbbe63482ebeda1372b62d6d1e/f1-gate.json`, concluso alle `2026-11-05T21:37:08.9208356Z`.
- TRX del gate: `.local-cache/m4/runs/faf1c0cddfc947a69e9df5f2a380a3b6/test-results/`.

**F1-D05e rimane formalmente aperta per la verifica visiva della GUI.** Il gate automatico è superato; RC63 rimane il riferimento grafico approvato. L'etichetta `milestone` del report automatico contiene ancora il nome storico RC76-HF01: il comando è stato eseguito sul workspace corrente con CityCatalog ripristinato e il test corretto.

## Diagnostica industriale RC76

```powershell
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --benchmark-e4a-v15-industry --scenario V15-I01 --policy research-first --max-seconds 120 --output DIRECTORY_NUOVA
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --analyze-e4a-v15-industry --input DIRECTORY_NUOVA --output DIRECTORY_ANALISI_NUOVA
```

Entrambi i comandi terminano con uscita 0. La rianalisi dei risultati produce **6/6 report identici byte per byte**: analisi JSON/CSV/Markdown e vincoli JSON/CSV/Markdown.

| Dato osservato | Valore |
| --- | --- |
| Scenario / politica | V15-I01 / research-first |
| Regole / provenienza | civil-growth-15 / fresh-game/no-grants |
| Orizzonte dichiarato / effettivo | 21600 / 120 secondi |
| Esito / classificazione | timeout / diagnostic-only |
| Completamento | Non osservato (`null`) |
| Azioni eseguite / attesa materiali | 5 / 120 secondi |
| Spesa effettiva Legname lavorato / Pietra | 192 / 96 unità |
| Lavoratori liberi minimi | 3 |
| Acquisto in attesa a 120s | Scuola; mancano 73 unità di Legname lavorato |
| Campioni di saturazione | 0 |

Output diagnostico: `.local-cache/e4a-v15/rc76-validated-diagnostic-c24f79eb70e34186ac0006242e0ba19f/`.

Output rianalizzato: `.local-cache/e4a-v15/rc76-validated-analysis-6c2268250a2146dba8c51831c794275b/`.

Questi risultati verificano il percorso CLI e la coerenza delle esportazioni. Non misurano il completamento a orizzonte pieno e non dimostrano uno stallo economico. I manifest degli scenari completi restano `not-run`. GUI, prezzi, ricette, personale e SQLite non sono modificati da questa verifica.
