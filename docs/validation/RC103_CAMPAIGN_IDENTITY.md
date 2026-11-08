# RC103 — Piano di validazione: identità stabile dei compositi

**Baseline confermata:** RC95 con 1245 test. **RC96–RC102:** gate Windows non confermato. **RC103:** +12 test Infrastructure, per **1330 test complessivi attesi**. I nuovi test non sono stati eseguiti con .NET 10 nell'ambiente di preparazione.

## Casi introdotti

1. Una nuova campagna composita ottiene un GUID canonico persistente; una seconda campagna ne ottiene un altro.
2. Backup e ripristino con API SQLite preservano il medesimo ID.
3. Registrazione iniziale e riavvio del gestore mantengono l'identità, l'epoca e il token; i pagamenti continuano a essere idempotenti.
4. Promozione di una copia dello stesso ramo conserva l'ID e rende inutilizzabile il vecchio token.
5. Una campagna indipendente, anche alla stessa revisione, non è promuovibile.
6. Una sostituzione fuori banda del file attivo con un altro composito viene rilevata **prima** della scrittura.
7. Le scritture SQL ordinarie non possono cambiare o eliminare l'ID del composito registrato.
8. La modifica fuori banda della tabella di binding del registro impedisce `Current` e `Apply`.
9. Un composito legacy senza ID resta leggibile e riceve l'identità soltanto alla registrazione; un registro RC101/RC102 privo della nuova tabella viene completato alla riapertura.
10. Ripetere i test RC95–RC102: acquisti, tick, receipt, backup/restore, rifiuto di file estranei, trigger, promozione e rollback.

## Esecuzione su Windows

```powershell
.\validate.cmd
```

**Criterio di accettazione:** 1330 test, zero errori, zero ignorati; gate F1, render della GUI e smoke Windows completi. In caso di fallimento conservare l'ultimo TRX e il rapporto `.local-cache/m4/f1-gates/`; la pulizia non deve cancellare le evidenze del run fallito.

**Limitazione:** il test d'identità non dimostra sicurezza contro sostituzioni deliberate di una copia precedente dello **stesso** ramo. Un unico salvataggio giocabile con controllo di rollback e transizione crash-safe resta una milestone futura.
