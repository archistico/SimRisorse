# RC97 — Piano di validazione (candidato)

**Punto di partenza confermato:** RC95 con 1245 test verdi. **RC96:** riscrittura del gate non ancora confermata con test Windows. **RC97:** +12 test, totale previsto **1257**.

## Controllo Windows

Da PowerShell nella radice del repository:

```powershell
.\validate.cmd
```

Non usare `validate-f1.cmd`: non è più distribuito. Il gate deve eseguire compilation Release .NET 10, 1257 xUnit verdi, native smoke, 27 screenshot, benchmark e tutti gli audit precedenti. In particolare, il report `rc97-v15-funding-clock.json` deve attestare `NativeElapsedTicks=10000000`, saldo città/deposito coerente, escrow conservato durante il tick, rimborso, nessuna Zecca concessa e `not-approved`.

## Contratti dei 12 test nuovi

- Application: un secondo reale, deposito intatto, restore del checkpoint composito, rifiuto revisione obsoleta, ledger manomesso, checkpoint RC95 senza campi RC97.
- Infrastructure: riapertura con replay senza doppio tick, secondo tick distinto, collisione ID fra clock e pagamento, revisione obsoleta priva di ricevuta, escrow che sopravvive al riavvio, rimborso idempotente dopo il tick.

## Verifiche supplementari

- Controllare che `.local-cache/m4/runs` e `.local-cache/m4/f1-gates` conservino l'ultima prova (anche in caso di errore), che NuGet e salvataggi dell'utente rimangano intatti.
- Nessun `MANIFEST_XXX_SHA256.txt` dentro lo ZIP, nessun nuovo README/ROADMAP/HANDOFF parallelo, screenshot README originali conservati.
- In caso di errore inviare stack trace completo e risultato TRX del progetto fallito; **non** dichiarare verde il gate sulla sola base dei test.

## Confini

Sandbox isolato: no costi attivi modificati, no GUI, no cantieri nei salvataggi del giocatore, no completamento Zecca. L'avanzamento del clock non autorizza materiali aggiunti artificialmente.
