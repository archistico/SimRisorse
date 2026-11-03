# G1-D16 · E3b/4a — GUI desktop v14 isolata

**Candidato:** v60-E3b4-RC1 · 03/11/2026 · validazione .NET da eseguire.

## Obiettivo e scelta progettuale

La campagna pubblica `civil-growth-13` non deve cambiare finché parametri e migrazioni della v14 non sono approvati. È stata quindi introdotta un'**anteprima opt-in**, in una finestra Avalonia distinta e con un database SQLite separato. Non sono stati modificati `MainWindow.axaml`, `MainViewModel`, `GameSession`, `GameRuntime`, lo store v2 o il formato v3.

## Ingresso / percorso

- `run-game.cmd` → vecchia v13, invariata.
- `run-game.cmd --experimental-v14` → nuova `V14PreviewWindow` e `V14PreviewViewModel` con le regole candidate.
- `--experimental-v14 --save-path PATH` → SQLite v14 personalizzato. Il percorso identico al database v13 standard viene rifiutato già dal parser. Un altro percorso che contenga dati v13 o corrotti viene rifiutato dallo store v14, senza sovrascrittura.
- `--experimental-v14` non accetta `--smoke-test` né `--scenario economy-foundation`.
- Default: `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`. Lo store usa copy-on-write e backup S1 già validati.

## Schermate effettive

1. **Scuola:** costruzione +1, iscrizione/annullamento +1 studente, assegnazione/sospensione ricercatore, ritorno civile, riepilogo scuole/attesa/formazione/qualificati/attivi.
2. **Albero tecnologico:** elenco di tutte le 11 voci, stato acquisto, saldo spendibile, requisiti e acquisto esplicito. Le 3 voci senza effetti restano visibili ma non acquistabili.
3. **Produttori:** 16 famiglie, tre taglie, acquisto, assegnazione e rimozione lavoratori, upgrade diretto tra livelli con quote e prerequisiti.
4. **Civili:** Case, Magazzino e Caserma con costo progressivo; Municipio non costruibile.

La testata mostra tempo, popolazione disponibile, punti e velocità di ricerca, inventario delle 16 risorse. Le liste vengono aggiornate senza ricreare le righe, mantenendo selezioni e scorrimento. Si usano i colori e i bordi netti della v13, in una UI compatta separata.

## Orologio, stato e sicurezza

- La sessione parte/riparte **in pausa**. La GUI usa il `PlayClock` esistente e sospende la partita quando Windows entra in pausa o la finestra è minimizzata; non recupera progressi offline.
- `Save` e chiusura normale persistono v14 sul DB dedicato. `Load` ripristina soltanto snapshot validati. Se il file esistente è illeggibile o di formato diverso, `CanSave=false`; l'utente può uscire esplicitamente senza scrivere.
- Nessuna migrazione automatica di database v13. Le regole Core già esistenti validano ruoli, prerequisiti, ricette, saldo ricerca e transazioni atomiche.
- L'anteprima usa i parametri candidati Scuola (120 Legno lavorato, 80 Pietra, 120 s, 10 Cibo, un posto di formazione, due ricercatori, 0,1 punti/s) e tier (+15%, 1×/2,6×/6,5×). **Non sono un'approvazione implicita**.

## Criteri di uscita e limiti

- Test auto attesi: 10 nuovi Desktop, totale 427; con `validate.cmd` su Windows e prova visuale 1160×820 / 980×670.
- Limiti da auditare: dimensionamento e leggibilità a schermo minimo, navigazione da tastiera, didascalie/localizzazione dei messaggi, accesso definitivo da MainWindow, salvataggi personali v13 e stress economico prolungato. L'anteprima non è ancora attivazione pubblica.
- Non dichiarare compilazione o screenshot Avalonia verificati nell'ambiente privo di .NET.

[ADR 0031](../adr/0031-e3b4-gui-v14-opt-in.md) · [test plan](../validation/V60_E3B4_RC1_TEST_PLAN.md) · [checkpoint](../history/checkpoints/G1_D16_CHECKPOINT.md).
