# G1-D9 / E3b/2 — Persistenza SQLite v3 candidata, isolata

**3 novembre 2026. Stato: codice e test scritti, NON eseguiti sul toolchain .NET 10.**

## Baseline e perimetro

- **Ultima base confermata dall'utente con `validate.cmd`: v60-E3b-RC1**, che contiene E1/E2/E3 e lo snapshot v14 isolati. Non confondere questo con una v14 giocabile: la campagna rimane `civil-growth-13`, SQLite v2.
- Questa milestone aggiunge `src/SimRisorse.Infrastructure/Persistence/V14SqliteSaveStore.cs` **senza cambiare** `SqliteGameSaveStore`, `GameSession`, `GameSnapshot`, scenario corrente o interfaccia Avalonia.
- La classe nuova non implementa `IGameSaveStore`: non può essere agganciata per errore ai comandi di salvataggio v13. Ha API distinte `Save`, `Load`, `PreviewMigration` e `SaveMigratedCopy`.

## Schema v3 reale (11 tabelle STRICT)

`PRAGMA application_id=0x53494D52` e `PRAGMA user_version=3`, entrambi obbligatori; intestazione `game_save.snapshot_version=3`, `scenario_id=civil-growth`, `rules_version=civil-growth-14`. 

- `resource_stock`: 16 risorse, quantità in milliunità (vettore sparso).
- `building_stock`: **solo gli edifici non produttivi**, incluso il Municipio, mai i 16 produttori.
- `population_state`: avanzamento crescita e ventesimi di secondo.
- `military_state` + `military_training`: reclute, soldati e progressioni individuali, importati senza perdere ordine e durata.
- `progression_family`: esattamente 16 famiglie per tre righe ciascuna (tier 0/1/2, numero di edifici/lavoratori).
- `progression_acquisitions`: storico degli acquisti monotono per famiglia e per la Scuola.
- `progression_school_state`: numero Scuole, studenti in attesa, ricercatori qualificati e attivi, punti ricerca in millipunti.
- `progression_training`: progressioni degli studenti con indici contigui da zero.
- `progression_technologies`: gli ID acquistati; nessuno sblocco automatico in lettura o scrittura.

La validazione Core `V14CampaignSnapshots.Validate` viene eseguita **prima della scrittura e dopo la lettura**: i soli vincoli SQL non possono garantire l'assenza di doppia occupazione della popolazione, tutti i prerequisiti o le capacità locali.

## Procedura transazionale e protezione S1

1. La richiesta v14 viene validata interamente in memoria: input incompatibile => nessun accesso in scrittura.
2. Se esiste un file alla destinazione, viene riletto e verificato come **v3 valido**; v1/v2, file di formato sconosciuto e v3 corrotti vengono rifiutati **senza sovrascrittura**.
3. Si crea un database v3 completo in una cartella di lavoro nella **stessa directory**, con nome temporaneo univoco. Lo schema e tutte le righe sono scritti in **un'unica transazione SQLite**.
4. Si rilegge il file temporaneo in sola lettura, rieseguendo validazione strutturale e semantica.
5. Se esisteva il v3 precedente, si genera un backup SQLite coerente non sovrascrivibile tramite `VACUUM INTO` (nomi `*.v14.backup*.sqlite3`). Se fallisce, il file precedente rimane intatto.
6. Si promuove il file temporaneo alla destinazione con una sostituzione filesystem nella stessa directory. In caso di errore il precedente non viene volutamente cancellato e il backup è già disponibile.

**Nota tecnica:** il costo I/O di un backup per ogni sovrascrittura è intenzionale in questa fase di sicurezza e andrà riesaminato per i salvataggi frequenti in E3b/3. Non usare questa classe con dati personali prima della validazione Windows.

## Importazione v13

`PreviewMigration(v13Path)` legge usando il **vecchio lettore v2**, verifica lo snapshot con la scenario factory v13 e restituisce il DTO v14 senza scrivere. `SaveMigratedCopy(v13Path)` crea o aggiorna **un altro file con percorso distinto**. La migrazione in-place della v13 è esplicitamente rifiutata; nessun `ALTER TABLE` su un database personale.

La migrazione v13→v14 preserva Municipio, stock, popolazione, tempi, esercito e tutte le costruzioni; inizializza i produttori al livello base, contatori dagli edifici posseduti, Scuola zero, ricerca zero e tecnologie non sbloccate. Non gestisce ancora la promozione di un v3 a formato giocabile.

## Test Infrastructure scritti

- Salvataggio e caricamento v3; il vecchio lettore v2 deve respingerlo.
- Copia migrata di un v13 originale con verifica byte-per-byte del sorgente.
- Rifiuto della conversione sullo stesso percorso e della sovrascrittura del v2.
- Round-trip di ricerca libera, formazione scientifica, tecnologie acquistate e reclute individuali.
- Reiezione di righe tier mancanti, progressioni mancanti, tecnologia ignota e `user_version` incompatibile.
- Input semanticamente invalido e v3 già corrotto non devono distruggere il file precedente.
- Sovrascrittura v3 con backup S1 leggibile e successivi nomi di backup distinti.

I test non sono ancora eseguiti da questo ambiente perché non dispone di .NET SDK 10. Per Windows: estrarre il nuovo ZIP in **cartella nuova**, eseguire `validate.cmd`, e inviare log completo in caso di errori.

## Vincoli ancora aperti / roadmap

- E3b/3: motore `GameSession` v14 con comandi realmente utilizzabili, ricercatori, acquisti, tempo e contabilizzazione unitaria. L'attuale classe è **un prototipo di persistenza isolato**.
- E3b/4: GUI di Scuola/Ricerca e pannello edifici per tier, acquisto potenziamenti e salvataggi v3.
- E4: controlli anti-softlock, benchmark risorse/popolazione, test/rollback su **copie** di salvataggi reali e approvazione quantitativa.
- **Nessun cambio al gameplay `civil-growth-13`, nessuna attivazione del ruleset `civil-growth-14`, nessuna modifica della roadmap navale.**
