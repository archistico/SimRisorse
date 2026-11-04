# RC42 / F1-D03 — Piano di validazione Windows

**Ultima baseline confermata:** RC41 563/563. **RC42 candidata:** 577 test cumulativi (14 nuovi: 2 Infrastructure + 12 Core). Non dichiarare la suite verde fino al risultato sul PC con .NET 10.

## Esecuzione

```powershell
.\validate.cmd
.\run-game.cmd
```

## 1. Fix del salvataggio

- `SaveBeyond100OccupiedBackupSlotsKeepsAllOldBackupsAndValidPrimary`: 100 backup numerati già esistenti; due ulteriori salvataggi; entrambi i backup extra SQLite validi e distinti; backup numerati intatti; checkpoint corrente corretto.
- `InvalidSaveAtBackupCapacityDoesNotCreateExtraBackupOrDamageCheckpoint`: snapshot illegale, database attuale e 100 file preesistenti invariati.
- Test storici di `V14SqliteSaveStoreTests` e salvataggi legacy v13 sempre verdi.
- **Prova manuale suggerita, su copia della cartella salvataggi:** aprire v14, eseguire Salva più volte in una directory che contiene già 100 backup numerati e verificare l'assenza del vecchio errore. Non cancellare o spostare backup reali per creare artificialmente lo scenario.

## 2. Modello ortofrutta (isolato)

Dodici casi `HorticultureFoundationTests`: Orto e Frutteto separati; parallelismo; capacità separate; spazio insufficiente per lotti; Acqua condivisa senza doppio consumo; priorità configurabile; Acqua insufficiente; zero lavoratori; input e policy errati; passaggio grezzo→deposito→Cibo; determinismo e immutabilità.

## 3. Compatibilità

- V14 GUI e v13 legacy non cambiate.
- Nuove filiere **non** ancora disponibili nel catalogo. Nessuna modifica a costi, fabbriche, capacità attuali, salvataggi o versione regole.
- Backup extra illimitati: verificare spazio libero su disco in caso di sessioni con molti autosalvataggi; non esiste una retention automatica.
- F1-D04 e F1-D05 restano necessari prima di usare Frutta, Verdura e Carne nella partita.

[Rapporto tecnico](../design/F1_D03_RC42_ORTOFRUTTA_E_BACKUP_V14.md) · [Roadmap](../ROADMAP.md)
