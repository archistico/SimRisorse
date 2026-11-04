# ADR 0040 — Roadmap post-RC27, gate sequenziali e disciplina dei rilasci

**Data:** 2026-11-04  
**Stato:** adottata come piano operativo; le singole scelte di gameplay rimangono da deliberare.

## Contesto

La RC27 è stata dichiarata validata dall'utente (507 test), ma il confronto reale GUI v13/v14 sulle 22 voci e due risoluzioni non risulta ancora raccolto. Proseguire direttamente al bilanciamento rischierebbe regressioni o decisioni prive di misure.

## Decisione

1. Conservare RC27 come baseline applicativa; una revisione **solo documentale** non cambia il numero RC né il conteggio test.
2. Pianificare G1-D42…D45 (focus, evidenze P0, correzioni documentate, sign-off) prima di P1-D46…D50 (Scuola/Matite/tecnologie).
3. Separare E4a **misurazione**, E4b **modifica autorizzata** ed E4c **migrazione sicura**.
4. Avviare navale, esplorazione e conquista solo dopo il consolidamento v14 e nuove decisioni specifiche.
5. Non dichiarare il gate visivo chiuso perché i test unitari sono verdi: servono 22 PASS/NA motivati, prova in due viewport e approvazione.
6. Preservare GUI v13, salvataggi e regole già approvate; usare sempre archivio integrale per avanzamenti di codice, con documentazione, test e handoff.

## Conseguenze

Le prossime attività sono ordinate e verificabili. Le decisioni sui coefficienti economici, la ricetta Matite, la produzione automatica e i benefici delle tecnologie restano aperte; non vengono approvate da questo ADR. La roadmap canonica è `docs/ROADMAP.md`; lo storico è in `docs/history/operational/ROADMAP_G1_D41_RC27_ARCHIVE.md`.

[Roadmap](../ROADMAP.md) · [Matrice](../validation/G1_D39_P0_UI_MATRIX.md) · [Runbook](../validation/G1_D42_P0_RUNBOOK.md).
