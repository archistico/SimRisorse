# RC32 / P1-D46 — Piano di validazione

## Stato

RC30: test confermati dall'utente (511 previsti). RC31: 515 test attesi, non verificati su Windows. RC32: **518 test attesi**, non verificati su Windows. La RC32 include tutti gli incrementi di RC31.

## Esecuzione

Da PowerShell nella radice della RC32:

```powershell
.\validate.cmd
.\run-game.cmd
```

## Controlli automatici attesi

- Suite intera: 518 test; nessun errore di compilazione.
- `V14SchoolIndividualProgressTests.DetailShowsBothStudentsAndRefreshesWhenFirstGraduates` — dati dopo Restore e tick.
- `V14SchoolIndividualProgressTests.SchoolPanelBindsIndividualProgressWithoutMovingTrainingActions` — binding headless.
- `SchoolTrainingQueueD46Tests.ParallelSeatsStartWaitingStudentsAfterGraduationWithoutAutoAssigningResearchers` — coda parallela, riserva popolazione e costi.

## Controlli manuali

1. A 1040x740 e 1240x850 entrare in **Scuola**. Nella colonia senza Scuole la nuova area individuale deve essere vuota, senza nascondere i comandi.
2. Con piu' Scuole e studenti attivi devono apparire i progressi dei singoli posti. Il primo coincide con la barra principale.
3. Dopo un tick il tempo restante deve decrescere; quando il primo allievo termina, un elemento in coda entra in formazione.
4. Verificare che `Iscrivi`, `Annulla`, `Assegna`, `Sospendi`, `Torna civile` e il laboratorio Matite mantengano gli stati precedenti.
5. Verificare apertura e ripresa di un salvataggio v14 esistente. Non modificare salvataggi v13.

**Criterio di uscita D46:** suite verde, dati individuali coerenti e nessuna regressione dei comandi. Il passaggio a D47 e' possibile dopo il gate; non dichiarare test eseguiti senza log Windows.
