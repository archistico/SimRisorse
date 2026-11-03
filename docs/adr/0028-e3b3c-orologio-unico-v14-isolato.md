# ADR 0028 — Orologio unico isolato per la sessione v14

**Stato:** implementazione sperimentale, in attesa di validazione Windows; **non** ratifica parametri economici. 3 novembre 2026.

## Contesto

La sessione sperimentale v14 gestiva Scuola e tecnologie ma non il tempo. Un secondo motore economico indipendente rischierebbe differenze rispetto al comportamento già convalidato in `GameSession` v13, in particolare fra produzione e fabbisogno alimentare.

## Decisione tecnica

- Aggiungere un adattatore `V14CampaignClock` al Core, e delegargli l'avanzamento da `V14CampaignSession.Advance`.
- Usare i pianificatori civili e il motore militare esistenti, senza copiare le ricette.
- Ordinare deterministicamente per secondo produzione/consumo, crescita, militari, Scuola/ricerca.
- Calcolare il nuovo snapshot **prima** del commit, validandolo come transizione unica.
- Conservare i tick parziali, applicare pausa e limite massimo come nella v13, ripristinare in pausa.
- Nessuna tecnologia si sblocca per il solo avanzamento del tempo: il saldo ricerca è spendibile soltanto su comando.

## Alternative scartate

- Motore nuovo duplicato per i tier: aumenta il rischio di divergenza da v13.
- Accumulo ricerca con `double`: compromette riproducibilità e salvataggi; usiamo millipunti interi.
- Commit dopo ogni secondo: un errore al secondo N lascerebbe una sessione parzialmente aggiornata.

## Conseguenze e gate

Il costo computazionale resta proporzionale ai secondi simulati, come nella v13. Servono test di equivalenza a grana diversa, pausa, overflow e replay SQLite; la misurazione su 8–24 ore avverrà nei benchmark successivi. Non cambiano GUI, store v2 e ruleset giocabile.

Dettaglio: [G1-D12](../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md).
