# Avvio rapido — SimRisorse RC76-HF04

**Codice:** RC76-HF04, **885/885 test superati su Windows**; gate automatico, smoke nativo esistente e audit v15 superati. [Evidenze HF04](validation/RC76_HF04_ICONS_AND_GROWTH.md). Icone e intestazione aggiornate; crescita v15 fino a ×20. Render ispezionati alle due risoluzioni; confronto interattivo Windows ancora da confermare.

Dalla radice su Windows, con .NET SDK 10 installato:

```powershell
.\validate.cmd
.\validate-f1.cmd
.\run-game.cmd
.\run-game.cmd --v14
```

`validate-f1.cmd` esegue a sua volta `validate.cmd`: non è necessario lanciare entrambi. Esegue anche due audit v15 da 120 secondi (base e combinato) e controlla il conteggio **esatto** degli 885 test. Richiede anche le otto catture v15 di Edifici, Scuola ed Esercito. I file di prova sono in `.local-cache/m4/f1-gates/<id>/`. Per uno stress combinato opzionale di 24h: `./validate-f1.cmd -Combined24Hours`. Non legge né modifica salvataggi personali.

La GUI v15 mostra gli edifici alimentari tramite **Edifici → Filiere alimentari**. Edifici bloccati grigi ma consultabili; produzione, consumi, capacità e costi con icone; pulsanti e sidebar come la v14; Risorse a sette colonne.

**Salvataggi:** v15 in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v15.sqlite3` (SQLite generation 6), v14 salvata separatamente; non rimuovere il salvataggio personale v14. Le vecchie versioni sperimentali v15 possono essere incompatibili: rinominarle a gioco chiuso se impediscono una nuova partita, senza cancellarle.

**Importante:** il comando gate verifica automaticamente il motore e i test UI, ma è ancora necessaria una verifica **visiva** delle schermate. Non dichiarare F1-D05e conclusa senza entrambe.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md) · [Piano RC72](validation/F1_D05E3_RC72_GATE_PLAN.md).

## Benchmark E4a v15

`dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120` produce una diagnostica abbreviata. Il comando senza `--max-seconds` misura gli orizzonti completi quando il gate F1 sarà verde. [Dettagli](../benchmarks/e4a-v15/README.md).

Diagnostica industriale V15-I01 / research-first da 120 secondi già eseguita, con rianalisi coerente; risultati completi ancora `not-run`. [Esiti e comandi](validation/RC76_WINDOWS_VALIDATION.md).
