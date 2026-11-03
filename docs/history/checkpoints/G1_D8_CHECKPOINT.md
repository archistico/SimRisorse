# SimRisorse — G1-D8 / v60-E3b-RC1

**Implementazione tecnica isolata, senza test .NET eseguiti.** Base ZIP: v60-E3-RC1. Ultima versione confermata verde dall'utente: v59-RC1.

## Incremento

- Nuovo `src/SimRisorse.Core/Sessions/V14CampaignSnapshot.cs`: contratto completo di uno snapshot candidato `civil-growth-14` (formato 3) con produttori 1/3/10, storico per famiglia, Scuola, ricercatori, punti ricerca e tecnologia.
- Conversione `MigrateFromV13` valida lo snapshot originale attraverso il motore v13 e porta le 16 famiglie al livello base, senza cambiare risorse, Municipio, lavoratori, soldati, tempo o popolazione.
- Validazione v14 indipendente: cataloghi completi, ricerca richiesta per tier, popolazione non duplicabile, limiti alloggi, capacità magazzini, storico monotono e ruoli della Scuola.
- Nuovi casi xUnit `tests/SimRisorse.Application.Tests/V14CampaignMigrationTests.cs` (da eseguire su Windows).
- Schema SQLite v3 e strategia backup S1 **progettati soltanto**; nessun database modificato.

**Nessuna modifica ai file esistenti in `src/`, `tests/`, `tools/`, `scripts/` o `experiments/`.** I due nuovi file tecnici sono aggiuntivi e il gioco v13 continua a usare il proprio snapshot v2.

## Limiti e attività future

E3b/2 persistenza reale su copie; E3b/3 sessione v14 con Scuola e comandi; E3b/4 GUI; E4 prove di compatibilità, performance e raggiungibilità. Costi +15%, livelli 1/3/10, scuola e tecnologie sono proposte numeriche, non nuove regole approvate.

Documenti: [specifica E3b](../../design/G1_D8_E3B_CONTRATTO_SNAPSHOT_MIGRAZIONI.md) e [ADR 0024](../../adr/0024-v14-snapshot-migration-boundaries-proposta.md).
