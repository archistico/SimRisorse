# RC88 — Piano di prova: prima produzione Spade e Monete

**Validazione confermata:** RC86.1, 1067 test passati (utente). **RC88 su Windows:** 1103 test eseguiti, 1087 passati e 16 falliti, tutti su `KeyNotFoundException` in `V15IndustrialChainAudit`. **RC88.1 candidata:** 2 test di regressione aggiunti, **1105 cumulativi attesi**. SDK .NET 10 non presente qui.

## Regressione RC88.1 — inventario sparso

- Un `ResourceBundle` può omettere `swords`/`coins` quando sono a zero. I test `[InlineData("V15-I02", "swords")]` e `[InlineData("V15-I03", "coins")]` richiedono che la diagnostica `Analyze` non generi `KeyNotFoundException`, non inventi produzione e mantenga stock finale a zero.
- La validazione del dizionario fisico iniziale **deve rimanere esatta** per tutte le voci presenti: non aggiungere chiavi a zero fittizie e non disabilitare controlli sulle ricevute.

## Gate rapido (obbligatorio)

```powershell
.\validate-f1.cmd
```

Deve compilare senza analizzatori xUnit trattati come errori ed eseguire 1105 test. L'audit RC88 deve generare sette righe a 0 secondi: Forgia (2 × Legacy), Zecca (2 × Legacy), Zecca (3 × MintTen), tutte `timeout`, `Diagnostic=true`, `Verified=false`, `CompletionSeconds=null`. Quattro output JSON/CSV/Markdown, nessun risultato inventato. Gli xUnit devono rifiutare spese gratuite, eventi fuori ordine, stock iniziale alterato, duplicate/milestone senza clock e punti ricerca non pagati.

## Comandi per analisi naturale (opzionali e separati dal gate)

Diagnostica riproducibile:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --audit-v15-industry-chain --max-seconds 0 --output .local-cache/rc88-check
```

Per tentare i sette percorsi per un orizzonte esplicito massimo di 57.600 secondi di simulazione (può richiedere tempo; usare una cartella nuova):

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --audit-v15-industry-chain --max-seconds 57600 --output .local-cache/rc88-full
```

Per analizzare offline i risultati salvati, **senza avviare un nuovo run**:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --audit-v15-industry-chain --input .local-cache/rc88-full/rc88-industry-chain-results.json --output .local-cache/rc88-offline
```

**Controllare:** l'ordine `Estrazione I → Industria I → costruzioni/assegnazioni → output`, la dotazione di due Matite (nessun refill), la separazione del personale 1 vs 10 per modulo, gli input di Forgia/Zecca, la saturazione delle capacità e le ricevute di attesa. Se una strategia non arriva a produrre entro l'orizzonte, riportare l'azione pendente e i deficit: **non** è una prova generale di impossibilità.

## Verifica manuale

Aprire nuova v15 con GUI v14 canonica; partire dalle 2 Matite, Scuola, ricerca, filiere minerarie, Forgia/Zecca, controllare produzione al clock e salvataggio. Nessun redesign di risorse o dettagli edificio. `MintTen` resta una modalità opt-in API, non il default dell'interfaccia.
