# Piano di validazione RC86 — bootstrap Matite, dettagli costo, documenti

**Baseline verificata dall'utente:** RC82/1002 test. RC83–RC85: +56 test non eseguiti in questa sede. RC86 aggiunge 9 test (1067 cumulativi attesi) e modifica asserzioni obsolete del benchmark e della GUI. **Gate RC86 non ancora eseguito**: SDK .NET 10 non presente nell'ambiente di preparazione.

## Test automatici previsti

- `Rc86MunicipalStarterTests`: inizio v15 con 2 Matite, 0 Carbone, 0 studenti e 0 punti ricerca; copertura quantità di un solo studente; assenza di Matite dalla **migrazione**, senza side effects; ordine di acquisto tecnologia rispetto al carbone in tre strategie; inventario iniziale condiviso fra benchmark.
- `V15DesktopIntegrationTests`: la nuova partita mostra 2 Matite, persistono Save/Load, un vecchio salvataggio con 0 Matite non riceve un refill e la scheda edificio usa solo risorse con icone. I test di regressione v14 non pretendono più la riga di testo duplicata.
- `E4aV15NaturalProgressionBenchmarkTests`: confronto scorte iniziali RC73 deve usare **NewCampaign** come il gioco corrente, non un import v14.
- `E4aV15IndustrialProgressionTests`: le policy hanno le stesse azioni obbligatorie ma `Estrazione I` precede la Miniera di carbone; non servono Matite craftate prima della ricerca.

## Gate Windows

```powershell
.\validate-f1.cmd
```

Controllare test totali, skipped, warnings, gate GUI F1, report del clock e diagnostica naturale, con attenzione alla modifica dello stock di partenza. Non ripubblicare i vecchi tempi RC77 come risultati RC86.

## Audit manuale

1. Nuova partita: 2 Matite nel pannello Risorse; deposito Municipio con capienza Matite sufficiente.
2. Produrre Cibo pronto, costruire Scuola, iscrivere uno studente e verificare che scendano a 0; dopo formazione assegnarlo e acquistare `Estrazione I`. Verificare costruibilità della Miniera di carbone e che il laboratorio Matite ora sia usabile se ci sono input.
3. Salvare/ricaricare dopo il consumo: le Matite restano 0; con nuova partita esplicita tornano a 2.
4. Selezionare edifici bloccati/sbloccati in GUI v15 e v14: un solo elenco costo con icone, preventivi 5/10 separati, nessun movimento dei pulsanti e nessuna colonna risorse alterata.
5. `docs/README.md` deve puntare a **un solo** ROADMAP, CURRENT_STATE, AUTHORITY_REGISTRY e NEW_CHAT_HANDOFF; nessun `*_ARCHIVE.md` in `docs/` operativo; integrità ZIP e manifest SHA-256 verificati.

**Criterio di uscita:** xUnit/.NET e verifica visiva Windows completati. Gli audit statici e di archivio non valgono come test superati.
