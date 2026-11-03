# Nuova chat — SimRisorse RC16 / G1-D31

**Allegare il progetto integrale RC16.** Leggere nell'ordine: [CURRENT_STATE](CURRENT_STATE.md), [ROADMAP](ROADMAP.md), [HANDOFF](HANDOFF.md), [audit UI-01…UI-22](validation/G1_D29_AUDIT_GUI_V13_V14.md), [piano RC16](validation/G1_D31_RC16_TEST_PLAN.md) e [rapporto D31](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md).

## Prompt di ripresa

> Continua SimRisorse dal checkpoint **RC16 / G1-D31**. **RC15 è l’ultimo gate Windows dichiarato superato (473 test attesi)**. RC16 aggiunge **3 test Desktop (476 attesi)**, non ancora compilati o eseguiti in Linux per assenza .NET SDK 10. Non dichiarare RC16 verde finché l’utente non conferma `./validate.cmd`.
>
> **Correzione RC16:** durante la finestra «Nuova partita?» la v14 lasciava attivi comandi mutanti eccetto Pausa. Ora la proprietà `CanMutateColony` blocca costruzioni, personale, upgrade, Scuola, Matite, Esercito e acquisto ricerca; la visibilità dei dettagli rimane invariata. «Annulla» ripristina la possibilità di giocare. Il comportamento Salva/Carica durante tale conferma è una **questione aperta UI-20/UI-21**.
>
> La roadmap approvata resta in **sei fasi**: 1) parità GUI v13/v14 e audit UI-01…UI-22; 2) Scuola/tecnologie; 3) E4a benchmark economia; 4) E4b bilanciamento con approvazione; 5) E4c migrazione v14 non distruttiva; 6) M5.4–M7 navi, esplorazione separata dalla conquista e completamento. Il **gate P0 grafico è aperto**, necessita controllo Windows 1040×740 / 1240×850; niente E4/navale anticipati.
>
> Preservare GUI v13, SQLite v2/v3 separati, tabella Risorse 7 colonne, Edifici Base/Grande/Avanzato come righe selezionabili anche se bloccate, Costruisci +1/+5/+10, Lavoratore −1/+1/+5/+10, Esercito, Scuola e 2 Matite/studente, ricerca con due decimali e verifiche reali. Nessun redesign involontario o cambio dei costi/tempi.
>
> Ogni versione: ZIP integrale, test, changelog, roadmap, handoff, piano Windows, ADR se servono nuove scelte. Non includere DB personali. Registrare separatamente test statici vs test .NET e risultato visivo reale.

[Stato corrente](CURRENT_STATE.md) · [Audit G1-D31](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Roadmap](ROADMAP.md).
