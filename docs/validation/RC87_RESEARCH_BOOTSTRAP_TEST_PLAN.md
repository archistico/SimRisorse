# RC87 — Piano di validazione del primo sblocco ricerca

**Baseline confermata:** RC86.1 con 1067 test superati, riferito dall'utente il 06/11/2026.  
**RC87 da validare:** +18 test xUnit Application, **1085 cumulativi attesi**. In questo ambiente manca .NET 10: non attribuire esecuzione alle nuove prove.

## Prove automatiche

1. `./validate-f1.cmd` su Windows con .NET 10. Controllare zero warning trattati come errori, 1085 passati e nessun test skipped, e smoke Desktop.
2. Il benchmark `V15-I01` termina la lista di azioni con `buy-tech-estrazione-1`: non deve acquistare Carbone, Zecca o costruzioni successive. `bread-first` conserva la filiera alimentare aggiuntiva **prima** della Scuola; `V15-I02/03` conservano gli obiettivi industriali completi.
3. L'audit controlla quantitativi effettivi: inventario di 2 Matite del Municipio; prima iscrizione 2 Matite consumate; 150 punti ricerca per `Estrazione I`; nessun grant e nessun refill ai checkpoint.
4. Validare rifiuto di report manipolati: ricevimento Matite non coerente, ricerca pagata con saldo falso, nessuna assegnazione ricercatore, campione finale non sbloccato, partenza alterata, report duplicato.
5. CLI diagnostica nel gate F1:
   ```powershell
   dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --audit-v15-research-bootstrap --max-seconds 0 --output .local-cache/rc87-bootstrap-diagnostic
   ```
   Deve produrre quattro file, due timeout diagnostici, zero completamenti rivendicati e nessuna differenza temporale numerica.

## Benchmark naturale completo (separato dal gate ordinario)

```
dotnet run --project tools/SimRisorse.Simulation -c Release -- --audit-v15-research-bootstrap --output .local-cache/rc87-bootstrap-full
```

Eseguire in directory di output nuova, non sovrascrivere prove precedenti. Controllare JSON e Markdown prima di dichiarare quale strategia raggiunge davvero lo sblocco e in quanti secondi. Timeout **non** equivale a softlock globale; le due strategie non usano gli stessi investimenti nel pane.

## Verifica manuale

Aprire nuova campagna v15: 2 Matite, Scuola sbloccata dopo Casa piccola + Deposito, cibo pronto, iscrizione del primo studente, formazione e assegnazione, accumulo punti, acquisto di Estrazione I. Dopo l'iscrizione le Matite devono essere a zero, non reintegrarsi al load; verificare il tier Base/Grande della Miniera di carbone e le descrizioni GUI. Costi nel dettaglio a icone una sola volta; interfaccia v14 canonica non modificata.
