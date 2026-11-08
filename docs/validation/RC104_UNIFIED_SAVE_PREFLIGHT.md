# Piano di validazione — RC104

**Ultima baseline confermata:** RC95, 1245 test. **RC104 candidata:** 1345 test cumulativi (+15 Infrastructure su RC103). Non considerare verdi i test senza il gate Windows.

## Verifica Windows

Dalla radice del progetto eseguire soltanto:

```powershell
.\validate.cmd
```

Il gate deve riportare **1345 test passati, 0 falliti** e completare render, smoke e audit. In caso di errore conservare le evidenze dell'ultimo run sotto `.local-cache/m4`.

## Controlli specifici

`Rc104UnifiedSavePreflightTests` copre:

- Sorgente mancante, salvataggio v15 nativo e SQLite estraneo, senza alterazioni.
- Composito non registrato, legacy senza identità, preventivo aperto e deposito reale.
- Registro non fornito, registro valido, ramo autorevole attivo, cantiere pendente.
- Backup non autorevole, percorso registro estraneo, registro eliminato, identità non corrispondente e trigger mancante.

## Criteri di accettazione

- Il preflight non scrive inventari né avvia migrazioni, non crea destinazioni e non promuove copie.
- Un ramo non autorevole non risulta pronto alla futura migrazione.
- Una partita con preventivo aperto o rate non può essere esportata in generation 6 come se nulla fosse.
- L'esito positivo di preflight **non** attiva il nuovo salvataggio giocabile né i costi Curve A.
- Conservare il solo `validate.cmd` e non includere `.local-cache`, `validate-f1.cmd` o manifest nel pacchetto.
