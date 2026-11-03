# G1 — Checklist di uscita prima di M5.4a

**Stato:** proposta progettuale (v59-G1-D1); **nessun nuovo codice o risultato di test** in questo checkpoint. Il test `validate.cmd` della v59-RC1 è stato confermato superato dall'utente.

| Gate | Prova richiesta | Stato |
| --- | --- | --- |
| G1-00 | Baseline Windows v59-RC1 verde | **Confermato dall'utente** |
| G1-01 | Dati della simulazione reale militare a 8 h esaminati | **In attesa dei file JSON** |
| G1-02 | N-01…N-07 approvate o rettificate con decisione esplicita | **In discussione** |
| G1-03 | S-01…S-10 approvate o rettificate con decisione esplicita | **In discussione** |
| G1-04 | Soglia soldati, tempi e ricompense della prima isola | **Non fissati** |
| G1-05 | Nessun ciclo fra Zecca, Cantiere, prima nave, conquista e Castello | **Disegno senza ciclo apparente; da misurare** |
| G1-06 | Piano Core/Application/SQLite/GUI + matrice regressione | **Predisposto; non implementato** |
| G1-07 | Backup salvataggi v13 e compatibilità storica prima del ruleset nuovo | **Da realizzare in M5.4b** |

Per ricevere soltanto i risultati utili, copiare dalla **root della versione v59-RC1 effettivamente validata** questi file:

```powershell
Get-Item '.local-cache\m4\military-balance-8-hours.json', '.local-cache\m4\military-capacity-benchmark.json' | Select-Object FullName, Length, LastWriteTime
```

Se mancano, rieseguire `./validate.cmd` su quella versione. Non è necessario condividere il DB della partita personale.

Una volta approvate N/S, implementare **solo M5.4a** (senza missioni premature), con nuovo checkpoint ZIP e gate di test autonomo. La v59-G1-D1 non è una release di gameplay.
