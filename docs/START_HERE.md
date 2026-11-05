# Avvio rapido — SimRisorse RC77

**Codice:** RC77, **899/899 test superati su Windows**; gate automatico, smoke nativo esistente, audit alimentari e recupero industriale superati. [Evidenze RC77](validation/E4A_V15_RC77_TEST_PLAN.md). Conservate icone/intestazione HF04 e crescita v15 fino a ×20. Confronto interattivo Windows della GUI ancora da confermare.

Dalla radice su Windows, con .NET SDK 10 installato:

```powershell
.\validate.cmd
.\validate-f1.cmd
.\run-game.cmd
.\run-game.cmd --v14
```

`validate-f1.cmd` esegue a sua volta `validate.cmd`: non è necessario lanciare entrambi. Controlla **899 test**, audit alimentari da 120s, sei esperimenti industriali controllati e catture v15. Le prove sono in `.local-cache/m4/f1-gates/<id>/`, con report industriali in `industrial-recovery/`. Stress combinato 24h opzionale: `./validate-f1.cmd -Combined24Hours`. Non legge né modifica salvataggi personali.

La GUI v15 mostra gli edifici alimentari tramite **Edifici → Filiere alimentari**. Edifici bloccati grigi ma consultabili; produzione, consumi, capacità e costi con icone; pulsanti e sidebar come la v14; Risorse a sette colonne.

**Salvataggi:** v15 in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v15.sqlite3` (SQLite generation 6), v14 salvata separatamente; non rimuovere il salvataggio personale v14. Le vecchie versioni sperimentali v15 possono essere incompatibili: rinominarle a gioco chiuso se impediscono una nuova partita, senza cancellarle.

**Importante:** il comando gate verifica automaticamente il motore e i test UI, ma è ancora necessaria una verifica **visiva** delle schermate. Non dichiarare F1-D05e conclusa senza entrambe.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md) · [Piano RC72](validation/F1_D05E3_RC72_GATE_PLAN.md).

## Benchmark E4a v15

RC77: `dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-recovery --output DIRECTORY_NUOVA` esegue tre confronti controllati da 120s. [Specifica e provenienza](design/E4A_V15_RC77_PRODUZIONE_PERSA_E_RECUPERO.md).

`dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120` produce una diagnostica abbreviata. Il comando senza `--max-seconds` misura gli orizzonti completi quando il gate F1 sarà verde. [Dettagli](../benchmarks/e4a-v15/README.md).

Diagnostica industriale V15-I01 / research-first da 120 secondi già eseguita, con rianalisi coerente; risultati completi ancora `not-run`. [Esiti e comandi](validation/RC76_WINDOWS_VALIDATION.md).
