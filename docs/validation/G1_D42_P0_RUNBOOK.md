# G1-D42 — Runbook di verifica e chiusura del gate P0

**Baseline:** RC27, 507 test dichiarati passati su Windows il 04/11/2026. **Nessun esito visivo delle 22 voci è automaticamente confermato.** Questo runbook non costituisce una verifica eseguita.

## Ambiente e preparazione sicura

1. Estrarre il progetto **in una nuova cartella**, con .NET 10; registrare versione Windows, DPI/scaling, build e data.
2. Eseguire `./validate.cmd`; conservare esito completo e `.trx`. La base RC27 conta **507 test confermati dall'utente**; il numero potrà aumentare nelle successive release.
3. Fare backup di ogni salvataggio da utilizzare. Usare soltanto copie e partite dimostrative; **non** avviare la migrazione v13→v14 su dati personali.
4. Avviare `./run-game.cmd` per v13 e `./run-game.cmd --experimental-v14` per v14 **separatamente**.
5. Ripetere con **1040×740** e **1240×850** a DPI uguale; riportare eventuale differenza di scaling. Non utilizzare screenshot di altre versioni come se fossero RC27.

## Esecuzione delle 22 voci, nell'ordine consigliato

| Giro | ID | Prove pratiche |
| --- | --- | --- |
| A: navigazione | UI-01, UI-18, UI-19 | Cinque pagine, TAB/Shift+TAB, frecce, Enter/Spazio; focus e selezione mantenuti dopo tick e riapertura scheda |
| B: Risorse | UI-02…UI-06 | Sette colonne, scrollbar, numeri destra, 17 risorse, tooltip Matite/Pane, saldo e stima pieno, selezione ocra |
| C: Edifici | UI-07…UI-13 | Bloccati grigi selezionabili; tier separati; costo lotti +1/+5/+10; costruzione, lavoratori, upgrade, diagnosi, scroll e Pane |
| D: abitanti e militare | UI-14, UI-15, UI-22 | Crescita/razioni, capacità case, reclute a 10/30 s, rimborso, residenti non duplicati |
| E: scuola e ricerca | UI-16, UI-17 | Studenti, coda/progresso, 2 Matite, assegnazione ricercatori, prezzi ricerca a 2 decimali, blocchi corretti |
| F: sessione | UI-20, UI-21 | Nuova partita/Annulla, pulsanti sospesi, salvataggio solo esplicito prima di autosave, copia checkpoint e reload |

**Scenari obbligatori trasversali:** almeno un edificio bloccato e uno costruibile, magazzino pieno, risorse insufficienti, lavoro/abitanti insufficienti, due durate di tick, salvataggio/riapertura, finestra di Nuova partita. Ripetere i flussi con testo lungo e scrolling senza salto di layout.

## Modulo esito per ciascuna voce

```text
UI-ID:
Versione: SimRisorse RC__ / G1-D__
Data e testatore:
Viewport: 1040x740 / 1240x850
DPI/scaling:
Salvataggio di prova: (ID fittizio, nessun dato personale)
Passi effettuati:
Esito: PASS | FAIL | NA motivata | NON ESEGUITA
Differenza v13-v14 e motivazione:
Rif. screenshot o video:
Rif. test automatico/TRX:
Difetto/issue (se FAIL):
Correzione e nuova verifica (se presente):
```

**Nomi screenshot consigliati:** `UI-XX_1040x740_v13.png`, `UI-XX_1040x740_v14.png`, `UI-XX_1240x850_v13.png`, `UI-XX_1240x850_v14.png`; per Scuola e Albero è sufficiente v14 + NA motivata per v13.

## Chiusura del gate

- Tutti i 22 ID: **PASS o NA motivata**, nessun FAIL o NON ESEGUITA.
- Due viewport controllate, differenze autorizzate registrate, anomalie bloccanti risolte.
- Suite automatica verde sullo stesso candidato cumulativo, TRX conservato.
- Prova di protezione dei salvataggi documentata su copia (nuova partita, Annulla, Salva manuale, chiusura e riapertura).
- Conferma visiva esplicita dell'utente; solo allora aggiornare P0 a **CHIUSO** nel registro.

[Roadmap](../ROADMAP.md) · [Matrice completa UI-01…UI-22](G1_D39_P0_UI_MATRIX.md) · [Piano RC27](G1_D41_RC27_TEST_PLAN.md).
