# Checkpoint G1-D14 — SimRisorse v60-E3b3-RC4

**Data:** 03/11/2026. **Ultimo gate confermato:** v60-E3b3-RC3, 396 test passati secondo l'utente. **Nuova candidata:** RC4 E3b/3d, 408 test attesi, .NET non eseguito nel container.

## Implementazione

- Nuovi `V14TierConstructionRules`, `V14TierActionFailure/Result` nel Core; configurazione prezzi iniettabile.
- Quattro operazioni in `V14CampaignSession`: costruzione, upgrade, assegnazione, rilascio lavoratori.
- Prezzi derivati da E1, `TieredConstruction` E2, certificazione snapshot v14; requisiti tecnologie e prerequisiti di edifici in sessione.
- 11 test Application e 1 Infrastructure aggiunti, compreso round-trip SQLite v3 e avanzamento.
- Nessuna modifica a v13, desktop, infrastruttura di produzione, database o script; root già pulita e documenti storici conservati.

## Decisioni

Approvato dall'utente: tre livelli 1/3/10, costruzione diretta/upgrade, prezzi crescenti per famiglia come principio, ricerca accumulabile liberamente e spesa volontaria. **Ancora candidati:** coefficiente +15%, tier 2,6/6,5, costi Scuola, ricette e soglie tecnologie.

## Comandi Windows

```powershell
.\validate.cmd
```

## Roadmap

E3b/3d test → E3b/3e replay/salvataggi → E3b/4 GUI → E4 bilanciamento, migrazioni e gate → G1-N/M5.4. [Specifica](../../design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md) · [ADR](../../adr/0029-e3b3d-comandi-tier-sessione-v14-isolati.md).
