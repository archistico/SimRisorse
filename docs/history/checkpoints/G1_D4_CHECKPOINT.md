# SimRisorse — Checkpoint G1-D4

**Consegna:** studio dell'albero tecnologico, non nuova release eseguibile. **Base:** v59-RC1, ruleset `civil-growth-13`, validato sul Windows dell'utente prima dei checkpoint documentali D1-D4.

## File nuovi
- `docs/design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md` — requisiti funzionali, parametri e rischio.
- `docs/design/G1_D4_TECH_TREE_PROPOSTA.json` — albero per validazione documentale, **non caricato dal software**.
- `docs/adr/0020-tech-tree-sector-research-proposta.md` — ADR proposto, in attesa di decisione.
- `docs/validation/G1_D4_AUDIT_DOCUMENTALE.md` — verifiche statiche effettuate.
- `G1_D4_SOURCE_INTEGRITY.json` — prova SHA dei 158 file di codice, test e script invariati.

## Gate rimasti aperti
- Approvare/o correggere numeri, schema della ricerca, prenotazioni dei ricercatori e requisito di Navigazione.
- Leggere `military-balance-8-hours.json` e `military-capacity-benchmark.json` per i limiti navali, appena disponibili.
- Solo dopo approvazione: E1 (prezzi) + E2 (tier) + E3 (ricerca) dietro un catalogo v14 coerente; evitare una release parziale con soli aumenti di prezzo. Test Windows su implementazione futura.

**Esito:** 158 file di implementazione identici; albero senza cicli, copertura di 16 famiglie e simulazione aritmetica dell'upgrade verificati. Nessuna compilazione .NET richiesta per una modifica solamente documentale.
