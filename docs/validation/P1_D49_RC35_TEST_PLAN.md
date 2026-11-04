# Piano di validazione — RC35 / P1-D49

**Stato:** suite da eseguire su Windows con SDK .NET 10. Non eseguita in questo ambiente.

## 1. Gate automatico cumulativo

Da una **cartella RC35 estratta integralmente**:

```powershell
.\validate.cmd
```

**Atteso: 536/536 test superati**, senza errori di compilazione o warning introdotti dal nuovo test. Il conteggio comprende RC31–RC34; non occorre provarle separatamente.

Controllare nel risultato TRX le tre prove:

- `V14SchoolResearchEndToEndTests.PencilsToStudentToResearchToLargeTierSurvivesTwoSqliteRoundTrips`
- `V14SchoolResearchEndToEndTests.PartialTrainingSavedPausedDoesNotGraduateOrAccrueResearchUntilResumed`
- `V14SchoolResearchEndToEndTests.InsufficientSuppliesAndResearchNeverPartiallyChangeCampaign`

Se un test non supera il gate, conservare **nome del test, testo errore, file e numero di riga, output di `validate.cmd`**. Non modificare costi/rate/ricette per farlo passare; verificare prima fixture e invarianti.

## 2. Verifica manuale della catena (su copia di partita v14)

1. Avviare `run-game.cmd`: si apre la **v14**; la v13 è disponibile solo con `--legacy-v13`, su percorso separato.
2. Costruire una Scuola dopo i prerequisiti e fabbricare le Matite dal laboratorio. Controllare i preventivi di produzione e la disponibilità dei materiali per +1, +5 e +10.
3. Iscrivere uno studente: decremento immediato di 10 Cibo e 2 Matite; il residente è riservato alla formazione.
4. Controllare progresso individuale, pausa e ripresa, conclusione dopo 120 s; l'allievo diventa ricercatore qualificato, **non automaticamente attivo**.
5. Assegnare il ricercatore, osservare il saldo di ricerca a due decimali. Verificare che la disponibilità dell'acquisto non dipenda dall'arrotondamento visivo.
6. Acquistare Lavorazioni I quando il saldo reale raggiunge il costo. Verificare che l'acquisto debiti 100 punti e renda costruibile/potenziabile la Segheria Grande.
7. Potenziare una Segheria Base, verificando gli occupati, il nuovo tier e il preventivo, poi **Salva**, chiudi e riapri. Verificare gli stessi dati, senza ricerca prodotta a gioco chiuso.
8. Provare un'azione rifiutata (risorse insufficienti / ricerca non sbloccata): nessun consumo parziale. Non usare il salvataggio personale originale per prove distruttive.

## 3. Limiti

Non sono stati eseguiti qui test .NET o osservazioni della GUI Windows. I test P1-D49 verificano il flusso e la persistenza, non certificano autonomamente leggibilità/focus. Nessuna modifica al DB o alle ricette è richiesta da questa RC.

[Rapporto D49](../design/P1_D49_RC35_INTEGRAZIONE_PROGRESSIVA.md) · [Roadmap](../ROADMAP.md)
