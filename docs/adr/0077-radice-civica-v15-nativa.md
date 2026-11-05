# ADR 0077 — Radice civica nativa nel checkpoint v15

**Data:** 05/11/2026 · **Stato:** adottato per lo sviluppo RC69, **validazione .NET pendente** · **Riguarda:** F1-D05e.2b.2b.2d

## Decisione

`V15FoodCampaignCheckpoint.CityCampaign` conserva direttamente un `V15CityCampaignSnapshot` con `V15ProductiveFamilySnapshot` e `V15PurchaseCounter`. Questo è il **solo stato civico** della partita. `NativeCityCampaign` rimane un alias di lettura della medesima istanza, non una seconda copia.

Il validatore, i comandi civici e il clock operano sui dati v15. La persistenza SQLite generation 6 ricostruisce direttamente i record v15; il formato su disco non cambia e perciò non si incrementa `PRAGMA user_version`.

## Confini storici

`FromInherited` / `ToInherited` conservano un punto di accesso controllato per le fixture, l'importazione v14 esplicita e l'anteprima storica. Per le fixture esistenti è disponibile una conversione verificata dal DTO v14 al v15, che rifiuta format/rules incompatibili. Non introdurre una conversione automatica da v15 a v14: quella è esplicita con `ToInherited()`.

La famiglia `food-store` occupata resta in memoria durante l'importazione finché non viene ritirata: nessuna perdita automatica di edifici, lavoratori o storia acquisti. Il writer SQLite la rifiuta se occupata; la esclude solo quando vuota.

## Alternative escluse

- Mantenere due proprietà autorevoli v14/v15 sincronizzate: rischio desincronizzazione e doppio stato.
- Incrementare schema SQLite per il solo cambio di tipo C#: nessuna differenza nel contratto persistito.
- Eliminare tutti gli adattatori del planner nello stesso incremento: aumenterebbe il rischio di regressioni nella produzione.

## Criteri di validazione

8 nuovi test, 814 cumulativi attesi; prova della radice nativa, identità dell'alias, storico acquisti, rifiuto di costruzioni senza mutazione, isolamento dello slot `food-store`, round-trip SQLite e generation 6 invariata. **Compilazione e xUnit non sono stati eseguiti in questa consegna.** GUI, XAML, asset, tariffe, ricette e v14 non modificati.

**Prossima fase:** RC70 per verifica della persistenza e degli ultimi adapter; RC71 per rimuovere lo slot temporaneo dal planner.

[Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md) · [ADR 0076](0076-portafogli-contatori-v15-nativi.md).
