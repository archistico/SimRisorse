# RC72 — Contratto tecnico gate finale F1-D05e

L'incremento RC72 **non cambia** modelli, regole di produzione, SQLite generation 6, file Desktop/XAML, script `validate.cmd` né salvataggio personale v14. Introduce test di interazione del clock e della persistenza, più un controllo della validazione cumulativa automatizzato.

Il comando `validate-f1.cmd` invoca la validazione esistente, ne legge il `validation.json` fresco e i relativi TRX, controlla la somma degli esiti e verifica che non siano stati saltati test. Usa gli output JSON di `SimRisorse.Simulation.dll` su scenari sintetici, non importa partite personali e non modifica la GUI. L'audit combinato richiede produzione positiva per tutte le sette fasi monitorate e riconciliazione senza differenze. L'eventuale esito negativo mantiene il gate chiuso.

La condizione grafica richiede test automatici **e** riscontro visivo umano. Lo stress sintetico è progettato per misurare invarianti, non la velocità economica di una partita naturale; per questa servirà E4a. Il test combinato utilizza input iniziali finanziati esplicitamente.

[Validazione](../validation/F1_D05E3_RC72_GATE_PLAN.md) · [ADR 0080](../adr/0080-gate-f1-d05e-e-evidenza-riproducibile.md) · [Roadmap](../ROADMAP.md).
