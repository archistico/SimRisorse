# G1-D10 — v60-E3b2-RC3

**Base Windows validata:** v60-E3b2-RC2, `validate.cmd` interamente superato secondo l'utente.

**Nuovo lavoro:** rafforzamento integrità SQLite v3 del numero studenti e del caricamento di una coda troncata, tre test xUnit, ADR 0026 e piano E3b/3. **Validazione Windows RC3 ancora da fare.**

- Store SQLite v14 isolato, `civil-growth-13` invariato.
- Gli archivi sperimentali v3 di RC1/RC2 non sono caricati silenziosamente; il checkpoint v13 sorgente originale non viene mai modificato.
- Nessuna modifica a `GameSession`, GUI, salari, razioni, popolazione o produzione.
- Prossimo: E3b/3a sessione v14 isolata e relativo restore, dopo gate automatico.

Rapporto: [validazione](../../validation/V60_E3B2_RC3_SQLITE_INTEGRITY.md). Piano: [E3b/3](../../design/G1_D10_E3B3_SESSION_PLAN.md).
