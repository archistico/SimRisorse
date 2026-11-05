# Piano di validazione RC65 — comandi civici nativi v15

**Ultimo test confermato:** 771/771 RC63. **RC64:** 777 previsti, non ancora confermati. **RC65:** 6 nuovi test Application + 1 Infrastructure = **784 attesi**. Nessuna compilazione .NET eseguita qui.

## Comandi

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --v14
# opzionale: test di stress integrato per 120 s
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
```

## Nuovi casi

1. Due Magazzini: costi progressivi crescenti, nessun doppio addebito, contatore incrementato.
2. ID ritirato `food-store`, edificio fisso e quantità invalide: nessuna mutazione di stato, codici di errore corretti.
3. Pescatore e personale: nessuna assegnazione oltre capienza, rifiuto del rilascio eccessivo, rilascio valido.
4. Upgrade Pescatore: non consentito senza ricerca; poi livello Grande, cronologia invariata.
5. Scuola e Matite: prerequisito nuovo Deposito, costo laboratorio e scorte fisicamente separate.
6. Tecnologia: secondo acquisto non addebita punti e lascia lo snapshot identico.
7. Round-trip SQLite generation 6: costruzioni, personale e contatori identici dopo la rilettura, 15 famiglie senza `food-store`.

## Regressione obbligatoria

- Tutti i vecchi 777 test della RC64, inclusi GUI e generazione 6.
- Parità visiva v14/v15: edifici grigi se bloccati, dettaglio iconico, pulsanti nella stessa posizione, barra crescita sotto il menu, Risorse a 7 colonne, Scuola/Esercito.
- Non modificare il DB v14 personale. Non dichiarare che il totale di 784 sia superato finché la validazione Windows non è confermata.
