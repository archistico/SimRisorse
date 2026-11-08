# Piano di validazione RC98 — Completamento atomico Zecca

**Baseline confermata:** RC95, 1245 test. **RC96–RC97:** ancora da confermare. **RC98:** 13 nuovi test (7 Application + 6 Infrastructure), **1270 attesi**.

## Funzionalità da provare

- Versamento incompleto: `Complete` rifiutato, nessun contatore/edificio creato.
- Primo completamento: un edificio nel tier acquistato, contatore acquisti +1; inventario dopo il deposito **identico** prima e dopo il completamento.
- Seconda Zecca: preventivo Curve A ricalcolato con epoca storica successiva, ricevute pregresse intatte.
- Ricerca del tier e prerequisiti verificati **all'ultimo momento**: se scompaiono, nessun completamento.
- Registro `Committed` separato da `Escrowed` e verificato contro la somma delle ricevute/conto storico e conteggi del portafoglio.
- Commit e caricamento SQLite: città/finanziamento/ricevuta/revisione in una sola transazione; riavvio, replay identico, conflitto chiave, revisione vecchia e richiesta rifiutata.
- Tick nativo dopo il completamento; magazzino e stock riconciliati.

## Esecuzione

```powershell
.\validate.cmd
```

Il gate esegue anche `--study-e4b-v15-mint-commit` e deve produrre `rc98-v15-mint-commit.json` nella cartella dell'ultima validazione. **La CLI usa materiale sintetico solo per dimostrare l'atomicità**, non per simulare la crescita naturale.

## Criteri di accettazione

1270 test superati, nessun warning trattato come errore, render grafici attesi presenti, gate F1 superato, PNG e TRX dell'ultima esecuzione conservati. Prima della conferma Windows, classificare la RC98 **candidata**.

## Audit statico locale

Verificare l'assenza di `validate-f1.cmd`, file `MANIFEST_*_SHA256.txt` e copie operative di `ROADMAP`, `CURRENT_STATE`, `AUTHORITY_REGISTRY`, `NEW_CHAT_HANDOFF`. README mantiene gli screenshot autentici. Non modificare il gameplay del giocatore.
