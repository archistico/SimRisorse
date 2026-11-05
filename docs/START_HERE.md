# Avvio rapido — SimRisorse RC78

**Codice:** RC78, **926/926 test superati su Windows**; gate automatico, smoke nativo esistente, audit alimentari, recupero industriale e studio delle curve superati. [Evidenze RC78](validation/E4B_RC78_TEST_PLAN.md). Conservate icone/intestazione HF04 e crescita v15 fino a ×20. Confronto interattivo Windows della GUI ancora da confermare.

Dalla radice su Windows, con .NET SDK 10 installato:

```powershell
.\validate.cmd
.\validate-f1.cmd
.\run-game.cmd
.\run-game.cmd --v14
```

`validate-f1.cmd` esegue a sua volta `validate.cmd`: non è necessario lanciare entrambi. Controlla **926 test**, audit alimentari da 120s, sei esperimenti industriali controllati, studio delle tre curve e catture v15. Le prove sono in `.local-cache/m4/f1-gates/<id>/`, con report industriali in `industrial-recovery/` e studio in `economic-curves/`. Stress combinato 24h opzionale: `./validate-f1.cmd -Combined24Hours`. Non legge né modifica salvataggi personali.

RC78 confronta proposte, senza applicare coefficienti o squadre: [rapporto](validation/E4B_RC78_CURVE_COMPARISON.md). Prossima decisione: curva e moduli tier prima dell'implementazione versionata RC79+.

La GUI v15 mostra gli edifici alimentari tramite **Edifici → Filiere alimentari**. Edifici bloccati grigi ma consultabili; produzione, consumi, capacità e costi con icone; pulsanti e sidebar come la v14; Risorse a sette colonne.

**Salvataggi:** v15 in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v15.sqlite3` (SQLite generation 6), v14 salvata separatamente; non rimuovere il salvataggio personale v14. Le vecchie versioni sperimentali v15 possono essere incompatibili: rinominarle a gioco chiuso se impediscono una nuova partita, senza cancellarle.

**Importante:** il comando gate verifica automaticamente il motore e i test UI, ma è ancora necessaria una verifica **visiva** delle schermate. Non dichiarare F1-D05e conclusa senza entrambe.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md) · [Piano RC72](validation/F1_D05E3_RC72_GATE_PLAN.md).

## Benchmark E4a v15

Le misure naturali complete **RC77-M01 sono eseguite**: 9/9 obiettivi raggiunti, 21/21 confronti di file identici. [Risultati, limiti e riproduzione](validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md).

RC77: `dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-recovery --output DIRECTORY_NUOVA` esegue tre confronti controllati da 120s. [Specifica e provenienza](design/E4A_V15_RC77_PRODUZIONE_PERSA_E_RECUPERO.md).

`dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120` produce una diagnostica abbreviata. Il comando senza `--max-seconds` misura gli orizzonti completi quando il gate F1 sarà verde. [Dettagli](../benchmarks/e4a-v15/README.md).

La diagnostica industriale da 120s della HF02 rimane [documentata come storico](validation/RC76_WINDOWS_VALIDATION.md); i manifest correnti riportano invece le misure integrali RC77-M01.
