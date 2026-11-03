# ADR 0039 — Nuova partita v14: checkpoint esplicito e autosalvataggio sicuro

**Stato:** accettato come protezione della progressione e del dato, con implementazione candidata RC17. **Data:** 03/11/2026.

## Contesto

Il testo della conferma della v14 dice che il checkpoint esistente non viene sovrascritto fino al click manuale su Salva. Ma un salvataggio periodico o in chiusura poteva sostituirlo appena confermata una nuova partita. Durante la conferma erano inoltre permessi Salva/Carica, con rischio di annullare o sovrascrivere progressi non intenzionalmente.

## Scelta

- La conferma di nuova partita è **atomica dal punto di vista UX**: si può solo confermare o annullare, non salvare/caricare. Le API dei pulsanti e i metodi pubblici riflettono il medesimo blocco.
- Dopo la conferma, la nuova partita resta **solo in memoria** finché il giocatore non preme **Salva v14**. L’autosalvataggio rimane disabilitato e non sostituisce il precedente checkpoint nel frattempo.
- Il **primo Save riuscito** della nuova sessione riattiva autosalvataggio e salvataggio alla chiusura; caricamento riuscito lo riattiva ugualmente.
- Chiudere durante la conferma equivale ad annullarla; la colonia precedente segue il consueto salvataggio di chiusura. Un checkpoint corrotto/incompatibile non può essere sovrascritto automaticamente.
- Per la GUI v13 non si altera il comportamento in questa milestone: la sicurezza aggiuntiva è confinata alla preview v14, con un audit cross-versione ancora previsto.

## Conseguenze

La protezione esige un booleano distinto per **permesso di salvataggio manuale** e **autosalvataggio consentito**. È una scelta di tutela dati, **non** una variazione della simulazione, dei formati dei salvataggi o dei valori di gameplay. Test automatici e prova su Windows sono necessari prima di considerare RC17 validata.

[Rapporto](../design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Test](../validation/G1_D32_RC17_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
