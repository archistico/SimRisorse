# ADR 0068 — Stress combinato prima della rottura strutturale v15

- **Stato:** accettato per RC59 / F1-D05e.2a
- **Data:** 5 novembre 2026
- **Contesto:** RC58 validata con 744 test. L'audit reale di 24 ore ha validato il percorso Pescatore→Deposito, non le nuove filiere tutte simultaneamente. È autorizzata la rottura dei database di sviluppo, ma non dei salvataggi v14 personali.

## Decisione

1. Introdurre una fixture sintetica condivisa fra test e strumento diagnostico: 24 residenti, 22 lavoratori assegnati (3 iniziali + 13 alimentari storici + 6 nuovi; correzione RC60 rispetto ai 19 originariamente documentati), filiere di animali, orticoltura, pane e Pesce attive nello stesso clock.
2. Riconciliare a ogni secondo le quantità fisiche al millesimo fra città, animali, transito e Deposito, confrontandole con i flussi emessi dal clock. Una discordanza interrompe l'audit.
3. Mantenere intatto il vecchio comando `--audit-v15`, aggiungendo `--profile combined`, opzionale e limitato a 48 ore simulate.
4. Non cambiare in questa release le regole economiche, la GUI e il layout SQLite. La rimozione della famiglia `food-store` e del suo contatore dal formato v15 è una milestone separata (**D05e.2b**) con propri test di compatibilità/transizione e possibile rottura esplicita dei soli database sperimentali.

## Alternative scartate

- Rimuovere il vecchio `food-store` e contemporaneamente introdurre uno stress di tutte le filiere: aumenterebbe le cause possibili di regressione senza un gate intermedio.
- Estendere soltanto le 24 ore del precedente audit senza cambiare la fixture: non validerebbe Cacciatore, Macellaio, Orto e Frutteto.
- Dichiarare uno stress finanziato equivalente al benchmark economico naturale E4a: falso, perché questo scenario include risorse e costruzioni preimpostate.

## Conseguenze e verifica

11 nuovi test, totale atteso RC59 **755**; nessuna build .NET effettuata durante la preparazione dell'archivio. Dopo il gate Windows, D05e.2b potrà modificare il checkpoint v15 con una baseline multi-filiera già verificata. [Specifiche](../design/F1_D05E2A_RC59_STRESS_FILIERE.md) · [Piano](../validation/F1_D05E2A_RC59_TEST_PLAN.md).


## Rettifica RC60 — personale iniziale della fixture

La RC59 ha eseguito **754/755 test**: il solo fallimento era l'asserzione che stimava 5 abitanti liberi. I **tre lavoratori già presenti** nella fondazione iniziale non erano inclusi nella contabilità descritta. La fixture occupa dunque **22 persone su 24**, lasciandone **2 libere**. RC60 corregge la verifica, aggiunge una regressione e rettifica i riferimenti storici; l'architettura e le scelte registrate nell'ADR rimangono invariate. [Analisi](../design/F1_D05E2A_RC60_FIX_PERSONALE.md).
