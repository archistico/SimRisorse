# P1-D50 — Piano di validazione RC36

**Stato:** non eseguito qui; SDK .NET 10 assente. Ultimo checkpoint confermato dall'utente RC30 (511 test). RC36 contiene tutti gli incrementi RC31–RC35.

## Passi automatici Windows

Da una cartella nuova, estratta dall'archivio integrale RC36:

```powershell
.\validate.cmd
```

**Atteso: 538 test totali**; 536 in RC35 + 2 in `V14SchoolPencilPersistenceTests.cs`. Se il numero effettivo differisce, annotare risultati e spiegazione, non dichiarare il gate chiuso. Il test end-to-end RC35, il ripristino dei salvataggi e l'avvio predefinito v14 sono compresi nel totale.

## Test manuali di accettazione

1. Aprire `./run-game.cmd`: verificare v14 predefinita, senza etichetta anteprima. `--legacy-v13` rimane opt-in; non aprire i salvataggi reali per le prove di migrazione.
2. Su una **copia** di un salvataggio v14, produrre Matite, iscrivere uno studente e verificare 10 Cibo + 2 Matite scalati una sola volta.
3. Sospendere/riprendere la formazione: progresso esatto, posto liberato alla qualifica, nessuna ricerca prima dell'assegnazione.
4. Assegnare un ricercatore, accumulare punti, acquistare una tecnologia con saldo sufficiente e verificare lo sblocco Grande/Avanzato previsto.
5. Salvare e caricare: inventario Matite, stato degli studenti, saldo ricerca, tecnologie e edifici invariati; sessione caricata in pausa.
6. Verificare annullamento e rimborsi, in particolare i vecchi iscritti senza Matite pagate, utilizzando **soltanto fixture di test**.
7. Tentare iscrizione senza Matite e acquisto di ricerca con punti insufficienti: nessuna variazione parziale.

## Due test nuovi

- `PrePencilStudentInOldSqliteCanBeCancelledWithoutInventingPencils`: pre-Matite SQLite con studente reale in formazione, lettura senza colonna `pencil_paid_students`, annullamento con rimborso Cibo soltanto, e successivo salvataggio.
- `InvalidPencilRefundLedgerCannotOverwriteExistingCheckpoint`: snapshot invalido rifiutato, database valido invariato e ricaricabile.

## Criteri di uscita

- Tutti i test superati, nessuna compilazione fallita.
- Integrità SQLite e separazione v13/v14 confermate.
- Nessuna regressione osservata nei flussi Scuola e Albero tecnologico.
- Se non è possibile testare, P1 rimane **implementata ma in attesa di validazione**; E4a è solo predisposta, non misurata.

[Rapporto D50](../design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md) · [Protocollo E4a](../design/E4A_PROTOCOLLO_BENCHMARK.md) · [Roadmap](../ROADMAP.md)
