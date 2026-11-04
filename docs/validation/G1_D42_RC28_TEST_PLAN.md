# G1-D42 / RC28 — Piano di validazione Windows

**Versione:** RC28 (incremento di codice successivo alla RC27). **Baseline RC27:** 507 test confermati superati dall'utente. **Nuovi casi:** 2 teorie × 2 dimensioni = **4**, totale **511 attesi**; cifra da confermare dal log `validate.cmd`.

## Validazione automatica

1. Estrarre RC28 in **cartella nuova** evitando di sovrascrivere salvataggi o DB personali.
2. Eseguire `\.\validate.cmd` (Windows: `.\validate.cmd`) dalla root del progetto; conservare TRX.
3. In particolare, verificare `V14KeyboardFocusAuditTests` (2 viewport per menu e Risorse) e tutte le regressioni RC27 su scorrimento del dettaglio.
4. Registrare eventuali CSxxxx, asserzioni fallite, ambiente e DPI prima di proseguire con il gate.

## Controllo visivo/interattivo, necessario ma distinto da xUnit

- Lanciare `.\run-game.cmd --experimental-v14` su **copia della partita**.
- A 1040×740 e 1240×850, selezionare **Edifici**, poi **Risorse** e **Albero tecnologico** con mouse e tastiera. Verificare bordo ocra visibile con focus, sfondo selezionato invariato, nessuna variazione di posizione al cambio focus.
- Dal menu laterale usare le frecce su/giù: la pagina deve seguire. Usare TAB/Shift+TAB per passare a comandi/righe; annotare eventuali salti, focus su elementi nascosti o non raggiungibili.
- In Edifici selezionare un edificio bloccato con frecce: dettagli consultabili e costruzione disabilitata. Poi selezionare un edificio costruibile, scorrere la diagnostica e cambiare selezione: verificare i reset RC27 **solo** al cambio.
- Su pulsanti disabilitati, provare Enter/Spazio: nessuna modifica a risorse, personale o partita. Sui pulsanti abilitati verificare comportamento e hover; non usare dati personali.
- Registrare screenshot e PASS/FAIL/NA per **UI-01, UI-07, UI-09, UI-10, UI-12, UI-18, UI-19** nella [matrice P0](G1_D39_P0_UI_MATRIX.md). Restanti 22 controlli nel D43.

Non dichiarare P0 chiuso finché non saranno registrate tutte le 22 prove alle due dimensioni. Il prossimo passo **G1-D43** esegue il confronto visivo completo; G1-D44 corregge le sole regressioni effettivamente dimostrate.

[Rapporto D42](../design/G1_D42_RC28_FOCUS_TASTIERA.md) · [Roadmap](../ROADMAP.md) · [Runbook](G1_D42_P0_RUNBOOK.md).
