# RC95 — Piano test contenitore transazionale v15 + finanziamento

**Baseline confermata dall'utente:** RC94.2, 1233 test superati. **Nuovi:** 12 in `Rc95CompositeFundingSqliteTests.cs`, totale previsto **1245**. Nessuna compilazione .NET 10 eseguita nell'ambiente di preparazione.

## Controlli mirati

1. Creazione e riapertura del contenitore separato, stato v15 valido, risorse e popolazione mantenute.
2. `BeginMint` e `Deposit` addebitano una sola volta lo stock v15 e mantengono escrow, preventivo e revisione dopo restart.
3. Replay di `requestId` identico e identità del payload; collisione con un pagamento diverso senza addebito.
4. Revisione obsoleta rifiutata; richieste respinte non consumano `requestId`.
5. Annullamento con rimborso una sola volta, nessuna Zecca acquisita.
6. Tentativi di `DeliverHypothetical` e `Complete` non ammessi per questa integrazione.
7. Rifiuto dei database v15 normali e degli store isolati RC93, senza scritture al file estraneo.
8. Corruzione dell'escrow o dell'immagine v15 causa errore, senza applicazione di nuove richieste.
9. Creazione della stessa sessione per due volte vietata.

## Esecuzione

```powershell
.\validate-f1.cmd
```

Eseguire dalla radice del progetto in PowerShell.

Il gate deve confermare **1245 passati, 0 falliti**, oltre agli screenshot delle viste Edifici. La sola presenza del file SQLite non autorizza costruzioni né modifica il salvataggio del giocatore.
