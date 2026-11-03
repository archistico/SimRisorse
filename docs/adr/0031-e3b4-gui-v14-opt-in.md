# ADR 0031 — Interfaccia avanzata v14 in modalità opt-in

- **Stato:** ACCETTATO come architettura temporanea del prototipo; UX e valori di bilanciamento ancora non ratificati.
- **Data:** 03/11/2026.

## Contesto

La sessione avanzata, il replay e SQLite v3 hanno raggiunto il gate RC5 (417 test confermati). Non è prudente modificare la navigazione v13 prima che l'utente abbia validato l'interfaccia e le quantità sperimentali.

## Decisione

1. Opzione esplicita `--experimental-v14`; senza flag restano invariati avvio, GUI e salvataggi v13.
2. La finestra v14 ha un solo proprietario di stato, `V14CampaignSession`, e usa il medesimo orologio Core. Nessuna duplicazione delle formule economiche nella UI, salvo quote di sola lettura dagli helper Core.
3. Il file predefinito v14 è distinto, salvato nel percorso `experiments`; non si esegue alcuna migrazione automatica del file v13. File vecchi o corrotti bloccano il salvataggio, non vengono reinterpretati.
4. Le tecnologie senza effetti si vedono, ma non sono acquistabili; i punti si accumulano e si spendono per decisione del giocatore.
5. Tutte le operazioni UI restano espressioni di comandi Core esistenti, con esito e messaggio mostrati. Chiusura normale = tentativo di salva; uscita senza salvataggio disponibile esplicitamente.

## Conseguenze

La GUI sperimentale è realmente esercitabile senza esporre il gioco attuale a un cambio di ruleset. Introduce una finestra e un viewmodel aggiuntivi, 10 test di regressione e un parametro nel parser desktop; richiede verifica grafica e compilazione su Windows. Valori numerici, navigazione finale e migrazione dei salvataggi sono decisioni future.

[Contratto di implementazione](../design/G1_D16_E3B4_ANTEPRIMA_DESKTOP_V14.md).
