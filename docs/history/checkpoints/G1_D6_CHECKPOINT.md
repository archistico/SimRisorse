# Checkpoint G1-D6 / SimRisorse v60-E2-RC1

**Stato: candidato E2 non validato.** Unisce E1 (prezzi e ricerca libera, isolati) ed E2 (livelli 1/3/10, costruzione e upgrade, proiezione su ricette originali) nella stessa base integrale. Nessuna attivazione nella campagna `civil-growth-13`; nessuna migrazione SQLite, nessuna modifica alla UI e ai benchmark.

- **Ultima baseline verificata su Windows:** v59-RC1 (`validate.cmd` passato secondo l'utente).
- **E1:** codice presente, test ancora da eseguire sul PC dell'utente.
- **E2:** 4 nuovi file di Core, 2 classi di test; test non eseguiti qui.
- **Scelta approvata:** ricerca accumulata liberamente con spesa volontaria.
- **Valori sperimentali:** crescita 15%, taglie 1/3/10, prezzi relativi 1/2,6/6,5, capacità locali 50/150/500. Nessuna approvazione implicita.

**Prossimo:** E3 in ramo separato: Scuola, ricercatori, ricerca effettiva, salvataggio e UI; prima di attivare l'economia nuova, gate di validazione/test e benchmark anti-softlock. Dopo E4, rivedere G1 navale.

Leggere [dettagli E2](../../design/G1_D6_E2_IMPLEMENTAZIONE_ISOLATA.md), [ADR 0022](../../adr/0022-e2-tiered-productive-buildings-prototype.md) e [roadmap](../../ROADMAP.md).
