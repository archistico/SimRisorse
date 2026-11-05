# RC45-DOC01 — Audit del solo riordino documentale

**Data:** 2026-11-04. **Baseline applicativa:** RC45; 600 test .NET confermati superati dall'utente **prima** di questo intervento. **Nessun test applicativo ripetuto** per RC45-DOC01, poiché non sono stati modificati sorgenti, test, script, XAML né file di progetto.

## Criteri di verifica

1. Copia integrale del pacchetto validato RC45 come punto di partenza; nessun file dell'applicazione rimosso o modificato.
2. Sostituire le concatenazioni dei vecchi aggiornamenti nei documenti operativi con una sola versione coerente.
3. Eliminare soltanto file manifestamente obsoleti **senza riferimenti in altre risorse del repository**. I documenti storici utili a debug, migrazioni, benchmark e decisioni sono conservati.
4. Collegamenti Markdown locali esistenti e nuovi verificati; nessun riferimento rotto.
5. La proposta costi/crew non può essere confusa con codice attivo o con prezzi già approvati.

## Esiti di controllo statico

| Verifica | Esito |
| --- | --- |
| File non documentali variati rispetto a RC45 | **0** |
| File documentali nuovi | 5 (ADR 0056, specifica E4b, matrice prove, registro pulizia, audit) |
| Documenti operativi riallineati | README principale, roadmap, stato, handoff, game design, catalogo, decisioni e avvio |
| File obsoleti rimossi | 9, elencati nel [registro di pulizia](../history/DOCUMENTATION_CLEANUP_RC45_DOC01.md) |
| Revisione delle decisioni storico-ADR | Nessun ADR precedente alterato; indice aggiornato |
| Test .NET e benchmark | Non eseguiti, nessuna modifica di codice |
| Nuovi coefficienti nel gameplay | Nessuno |

**Artefatti storici preservati:** salvataggi e schema SQLite, sorgenti del gioco, test e script, vecchi ADR e prove migrazione, risultati e scenari benchmark, immagini di riferimento, changelog integrale.

Per la parte ancora non automatizzabile di una futura decisione E4b servirà esplicita approvazione dell'utente sui coefficienti, oltre ai test e alle misure. I numeri 100–1.000 Tagliapietre base e 10 lavoratori per modulo Zecca indicano gli obiettivi di progettazione, non un release di gameplay.

[Roadmap](../ROADMAP.md) · [Specifica E4b](../design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md) · [Handoff](../NEW_CHAT_HANDOFF.md).
