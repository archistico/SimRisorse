# F1-D02 / RC41 — Piano di validazione

**Baseline confermata dall'utente:** RC40, 552/552 test. **RC41 candidata:** 563 test attesi, da eseguire su Windows con .NET 10. Non sono state eseguite misure E4a della nuova economia.

## Test automatici

```powershell
.\validate.cmd
```

I nuovi 11 test `MeatChainFoundationTests` verificano:

1. La Stalla usa entrambi gli input: senza Acqua non consuma Grano né produce Bovini.
2. Il Cacciatore funziona senza input agricoli, ma rispetta il personale e la capienza.
3. Il Macellaio divide i lavoratori tra Bovini e Selvaggina senza contarli due volte.
4. Animali prodotti ora non possono essere macellati nello stesso tick.
5. Carne piena: nessun animale sprecato.
6. Spazio insufficiente per un lotto intero: nessun consumo.
7. Priorità delle due ricette: criterio deterministico, indipendente dall'ordine in cui vengono fornite.
8. Rese parametriche: scenario illustrativo più produttivo e scenario equivalente, senza valori nel catalogo.
9. Rifiuto di ricette duplicate o del mangime privo di Grano/Acqua.
10. Nessun effetto con 0 lavoratori, nessuna risorsa estranea o conteggio lavoratori negativo.
11. Carne grezza distinta da Cibo pronto: deposito e lavoratori di trasformazione necessari.

## Controlli statici e funzionali

- `V14CampaignSession`, `V14SqliteSaveStore`, `GameCatalog`, `V14PreviewWindow`, GUI v13, benchmark E4a: file invariati rispetto a RC40.
- Nessuna comparsa di Bovini/Selvaggina/Carne nella tabella Risorse v14; la filiera è **ancora isolata**.
- La ricetta esistente Pesce → Cibo e la riserva Pane restano identiche.
- Nessuna modifica del numero di abitanti, dei costi, del deposito alimentare vigente o delle tecnologie acquistabili.
- Verificare l'integrità ZIP e i documenti di handoff.

## Gate successivo

F1-D03 modellerà Orto e Frutteto. F1-D04 dovrà collegare la produzione al deposito con un unico flusso di scorte e controllo atomico; F1-D05 dovrà introdurre migrazione/versione di regole e coprire i salvataggi legacy prima dell'attivazione della riforma.

[Rapporto](../design/F1_D02_RC41_FILIERA_CARNE.md) · [Roadmap](../ROADMAP.md)
