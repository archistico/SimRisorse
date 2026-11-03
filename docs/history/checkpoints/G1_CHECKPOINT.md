# SimRisorse — checkpoint G1-D1 (v59 eseguibile invariata)

**3 novembre 2026.** La v59-RC1, `civil-growth-13`, è stata validata su Windows secondo conferma dell'utente. Questo archivio è **solo un consolidamento della progettazione G1**. Contiene l'intero progetto e i test originali ma **non** introduce nuove meccaniche o test da eseguire. Nessun lavoro su flotta, Castello e SQLite è stato sviluppato in questo checkpoint.

## Cosa leggere

1. [Scheda G1](../../design/G1_NAVAL_MISSIONS_DECISION_GATE.md) — costi candidati, N-01…N-07, S-01…S-10, semantica e test.
2. [Checklist G1](../../validation/G1_READY_FOR_IMPLEMENTATION.md) — gate prima del Cantiere e documenti numerici mancanti.
3. [ADR 0017 PROPOSTO](../../adr/0017-naval-mission-boundaries-proposta.md) — motivazioni, alternative, conseguenze.
4. [Registro decisioni](../../AUTHORITY_REGISTRY.md), [roadmap](../../ROADMAP.md), [stato](../../CURRENT_STATE.md) — autorità del checkpoint.

## Numeri da non confondere

- 21 strutture attuali = 20 edifici-obiettivo + 1 Municipio; 16 risorse.
- Futuro Cantiere = 21º edificio-obiettivo; futuro Castello = 22º. Le navi non sono un nuovo tipo di risorsa.
- Benchmark militari controllati validati, ma **senza leggere la fotografia economica reale dopo otto ore non fissiamo 5/10/20 soldati**.
- Il futuro ruleset navale potrebbe essere `civil-growth-14`: attualmente non esiste.

## Differenze rispetto a v59-RC1

Unicamente file Markdown di progettazione, stato, documentazione e handoff. Tutti i file sotto `src/`, `tests/`, `tools/`, `scripts/`, `experiments/` sono byte-identici alla v59-RC1 (vedere `G1_SOURCE_INTEGRITY.json`). Il gate Windows della v59 continua a riferirsi **al codice invariato**, non implica approvazione navale.
