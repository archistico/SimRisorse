# RC70 — Piano test su Windows

**Stato:** non eseguito · **Totale atteso: 818 test** (814 RC69 + 4 nuovi: Application 2, Infrastructure 2).

1. Da root del progetto: ` .\validate.cmd `. Registrare log TRX, errori di compilazione, test falliti, ignored e durata. **Se non è verde, RC70 rimane candidata**.
2. Infrastructure `F1D05e2b2eV15SqliteAuditTests`: confronto di dati distribuiti fra città/deposito/animali/transito, frazioni di millesimi, scorte pronte, elapsed tick non intero, progressi, portafogli in ordine invertito e 3 salvataggi consecutivi con backup immutabili.
3. Application `F1D05e2b2eNativePencilCapacityTests`: confronto con il precedente calcolo v14 per 9 configurazioni Magazzini/Scuole; nessuna influenza del vecchio `food-store` sulla capienza v15.
4. Verificare `user_version=6`, 16 tabelle, nessun `food-store` in `progression_family`, eventuale storico in `progression_acquisitions`; mantenere separata la partita v14.
5. UI v15: edifici grigi consultabili, icone costo/produzione/consumo/capacità, comandi nella disposizione RC63, barra popolazione, Risorse sette colonne e Scuola/Esercito senza regressioni.
6. Stress combinato breve: `dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined`. Devono essere zero gli sbilanciamenti e la produzione Cibo da `food-store`; annotare log.

**Limitazioni:** nessun test di spegnimento forzato hardware eseguito; gli audit statici e i test xUnit non provano atomicità sotto qualsiasi filesystem.

[Specifica](../design/F1_D05E2B2B2E_RC70_AUDIT_SQLITE.md) · [Stato](../CURRENT_STATE.md).
