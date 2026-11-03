# ADR 0019 — Contratto quantitativo costi, livelli e ricerca (PROPOSTA)

Data: 2026-11-03. Stato: **Proposto — non approvato**. Integra ADR 0018 (anch'esso proposto); non lo sostituisce.

## Contesto
La v59 validata ha prezzi lineari per edificio e posti lavoro definiti per tipo. L'utente preferisce escalation dei prezzi per famiglia, livelli base/grande/avanzata costruibili o ottenibili tramite upgrade, Scuola che forma ricercatori e ricerca che sblocca i livelli.

## Scelte quantitative candidate
- Prezzi `ceil(B_r × M_t × 1,15^K)` per famiglia, per risorsa; contatore storico K incrementato solo da nuove costruzioni, con acquisti multipli sommati progressivamente.
- Tier produttivi 1/3/10 lavoratori; moltiplicatori costo 1/2,6/6,5; ricetta per lavoratore invariata, stoccaggio proporzionale ai posti, upgrade costo `P_target(K)-P_current(K)` senza incremento K.
- Scuola 120 Legno lavorato + 80 Pietra, 1 slot di formazione, 2 ricerca; formazione 120 s e 10 Cibo; ricercatori attivi 0,1 punti/s; tech tree per settori.
- Salvataggi v13 invariati, proposta `civil-growth-14`, snapshot/SQLite v3 con backup S1, contatori, tier, personale di ricerca e sblocchi; nessun retrodebito.
- Scuola proposta come 23º tipo-obiettivo; Municipio sempre escluso; tier diversi della stessa famiglia valgono una sola volta.

## Rischi da validare
Esplosione esponenziale dei prezzi, capacità magazzino insufficienti per l'acquisto, blocco del bacino di lavoro, cicli tecnologici, migrazioni incoerenti, bypass via upgrade o acquisti in lotti. Nessuna modifica attuativa senza approvazione e benchmark.

## Decisione
**Non ancora presa.** Il documento di riferimento è `docs/design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md`.
