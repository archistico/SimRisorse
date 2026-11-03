# G1-D35 / RC20 — Piano di validazione Windows

**Baseline Windows confermata dall'utente:** RC16 (476 test). **Candidature cumulative:** RC17 (481), RC18 (484), RC19 (487), **RC20 (490 test attesi)**. Non dichiarare verdi le versioni intermedie: RC20 include tutte le modifiche precedenti.

## Gate automatico

1. Estrarre RC20 in una cartella nuova; non sovrascrivere salvataggi personali.
2. Eseguire `./validate.cmd` dalla root (in PowerShell: `.\validate.cmd`). Attesi **490/490 test**; nessuna compilazione fallita.
3. In particolare controllare la nuova classe `V14ResourcesAuditTests`: 2 viewport + 1 test ViewModel. Se fallisce un controllo geometrico, conservare il TRX con risoluzione e misure riportate dall'asserzione; non correggere a tentativi.
4. Conservare evidenza dei test RC17 sulla protezione del checkpoint: questo gate non è ancora stato provato dall'utente.

## Gate grafico manuale P0

Avviare `.\run-game.cmd --experimental-v14` con **salvataggio di prova**, alle risoluzioni **1040×740** e **1240×850**:

- Aprire **Risorse**: 17 righe in ordine alfabetico italiano; sette colonne invarianti. Verificare la leggibilità dell'ultima intestazione «Si riempie tra» e delle celle, anche quando la scrollbar è visibile.
- Selezionare **Pane** e **Matite**: riga ocra, ancora selezionata dopo tick e cambio temporaneo di scheda; dettagli di capacità sul tooltip; quantità arrotondata all'intero solo in tabella, valore preciso al passaggio del mouse.
- Verificare produzione, consumo, saldo /s e stima `hh:mm:ss`: `—` in assenza di crescita netta; nessun valore invade scrollbar o altre colonne.
- Controllare che la GUI classica `./run-game.cmd` mostri ancora le 16 risorse originali con le colonne e lo stile precedenti.
- Su **copia del database**, verificare RC17: Nuova partita → Avvia nuova → chiusura senza Salva manuale deve conservare il checkpoint precedente.

## Limiti dichiarati

Nessuna modifica a GUI v13/v14, Core, Application, Infrastructure, SQLite, ricette o coefficienti. Sono stati **aggiunti solo test Desktop e documentazione**. Integrità del pacchetto verificabile qui; compilazione xUnit e approvazione grafica soltanto su Windows. Prossimo: [G1-D36 Edifici](../ROADMAP.md).

[Rapporto RC20](../design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Audit completo](G1_D29_AUDIT_GUI_V13_V14.md).
