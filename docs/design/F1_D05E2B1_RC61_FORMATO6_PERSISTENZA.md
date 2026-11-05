# RC61 — F1-D05e.2b.1: rimozione della famiglia legacy dal formato SQLite v15

Data: 5 novembre 2026. Baseline RC60 **validata dall'utente: 756/756**. RC61 è candidata, **761 test attesi**, compilazione e test .NET 10 non eseguiti nel contesto di consegna.

## Modifica concreta

- `V15FoodCampaignCheckpoints.FileVersion` passa da **5 a 6**; `V15SqliteFoodSaveStore` crea database `PRAGMA user_version=6` e verifica anche `snapshot_version=6`.
- `progression_family`: **15 famiglie attive × 3 tier = 45 righe**, non più 16 × 3. `food-store` non è serializzato. Il vincolo SQL `CHECK(family_id <> 'food-store')` ne impedisce il reinserimento con normali istruzioni DML.
- Al caricamento `V15LegacyProducerAdapter.RestoreEmptyAdapterSlot` ricostruisce **soltanto in memoria** un record `food-store` con edifici e lavoratori a zero. Questo è indispensabile finché `V14CampaignSnapshots.Validate` e `V14CampaignSession` sono riutilizzati dai comandi civici v15. Lo slot non genera Cibo, non è acquistabile, non è mostrato e non appartiene più alle famiglie persistite.
- `Save` rifiuta uno snapshot con `food-store` ancora costruito o con lavoratori assegnati. Non perde i dati in modo implicito: usare `V15FoodIndustryCampaignSession.RetireLegacyFoodStore()` **esplicitamente** prima della scrittura. Questa operazione non rimborsa vecchi acquisti.
- Il contatore storico in `progression_acquisitions` **rimane persistito** perché l'adattatore v14 lo richiede e per non perdere la storia degli acquisti. Questa RC **non** dichiara indipendenza completa dal modello v14.
- Nessun cambiamento a `run-game.cmd`, alla UI pixel art (sette colonne Risorse), alle ricette, ai coefficienti economici, al clock o alle capacità. Nessun cambiamento al salvataggio **v14** personale.

## Rottura volontaria dei database sperimentali

Il lettore RC61 non può caricare salvataggi sperimentali `user_version=5`; fallisce con `GameSaveError.UnsupportedFormat` senza sovrascriverli. Non vi è migrazione automatica. La modalità v15 conserva il suo percorso separato; per iniziare una partita RC61, **chiudere il gioco e rinominare/spostare il precedente file v15** (non quello v14) prima di avviare `run-game.cmd --v15`. Non copiare né rinominare il DB v14 al suo posto.

Una conversione opt-in da v14 a gen. 6 può funzionare se l'eventuale `food-store` storico non è occupato; se lo è, fallisce senza scrivere un file destinazione. Nessun tentativo di cancellare automaticamente dati storici o vecchi DB.

## Limite residuo e prossima fase

F1-D05e.2b.2: introdurre un checkpoint e regole civiche nativi v15 per eliminare anche lo slot **in memoria** e il contatore legacy, sostituendo l'adattatore v14. Questa fase deve essere svolta separatamente dalle modifiche della GUI e del bilanciamento. Successivamente E4a benchmark, poi E4b progressione quasi esponenziale e almeno dieci addetti per Zecca.
