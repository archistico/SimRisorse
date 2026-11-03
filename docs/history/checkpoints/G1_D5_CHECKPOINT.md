# SimRisorse — checkpoint G1-D5 / v60-E1-RC1

**Stato:** incremento tecnico isolato, **non validato su Windows**; baseline operativa resta v59-RC1 `civil-growth-13`, con `validate.cmd` superato dall'utente.

- **Decisione accettata:** punti ricerca accumulati senza progetto in corso e spesi dal giocatore a scelta (ADR 0021).
- **Implementato ma NON attivato:** formule parametrizzate per prezzi crescenti, quotazioni di upgrade, contatori storici per famiglia, pagamento immutabile; contabilizzazione dei punti di ricerca.
- **Test nuovi:** `ProgressiveConstructionPricingTests` e `ResearchAccountingTests`, ancora da compilare ed eseguire.
- **Non implementato:** collegamento a `GameSession`, GUI, tier, Scuola, tecnologie sbloccate, migrazione SQLite, `civil-growth-14`, flotta. Valori numerici rimangono ipotesi.
- **Prossimo gate:** .NET 10 Windows con `./validate.cmd`, poi E2 senza modificare ancora la campagna giocabile.

Leggere [dettaglio](../../design/G1_D5_E1_IMPLEMENTAZIONE_ISOLATA.md) e [ROADMAP](../../ROADMAP.md).
