# RC42 / F1-D03 — Ortofrutta e correzione del salvataggio v14

Data: 2026-11-04. **Ultima suite confermata dall'utente:** RC41, 563 test superati. **RC42 candidata:** 577 test attesi, .NET 10 non disponibile in questa sessione.

## A. Bug prioritario: backup SQLite esauriti

### Causa riprodotta

`V14SqliteSaveStore.BackupExisting()` prova 100 nomi (`.v14.backup.sqlite3`, `.v14.backup-2.sqlite3` ... `.v14.backup-100.sqlite3`) e prima della RC42 interrompe `Save()` quando tutti sono occupati, anche se il database principale è integro. Questo colpisce una partita normale con frequenti salvataggi automatici; non indica da solo un database corrotto.

### Correzione adottata

- I primi 100 nomi e il comportamento storico restano **identici**.
- Dopo l'ultimo slot si usa `*.v14.backup-extra-YYYYMMDDTHHMMSSfffZ-GUID.sqlite3`, unico e mai riutilizzato.
- Anche il backup aggiuntivo usa **`VACUUM INTO`**: viene generata una copia coerente del precedente SQLite prima della promozione del nuovo checkpoint.
- Nessun backup vecchio viene cancellato, ruotato o sovrascritto. Le copie legacy v13 restano separate.
- `Save()` continua a validare il checkpoint nuovo e quello esistente; non promuove mai il nuovo file se il backup fallisce.
- **Conseguenza dichiarata:** lo spazio disco occupato dai backup cresce nel tempo. Una futura retention configurabile o un archivio manuale richiederà una decisione esplicita; non è presente pulizia automatica.
- I test coprono 100 slot pieni, due backup extra consecutivi con contenuto coerente, assenza di sovrascrittura e mancata creazione di backup per salvataggi invalidi.

### Uso e recupero

Il percorso ordinario del salvataggio v14, salvo opzioni personalizzate, è `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`; i backup numerati sono nella stessa cartella. Non eliminare file `.sqlite3` per risolvere il blocco senza averli copiati su un supporto di backup. Con RC42 un normale Salva nella v14 deve riprendere a funzionare senza rinominare né rimuovere i 100 file esistenti. Non modificare direttamente la directory dei salvataggi durante l'esecuzione del gioco.

## B. Orto e Frutteto, due filiere distinte

Nuovo modulo isolato `src/SimRisorse.Core/Food/HorticultureModel.cs`:

- `HorticultureCropRecipe`: identificativo prodotto, resa per lotto, quantità opzionale di Acqua per lotto e priorità.
- `HorticulturePolicy`: Orto → `vegetables` (Verdura), Frutteto → `fruit` (Frutta), Acqua condivisa (`water`) solo se configurata.
- `HorticulturePlanner.PlanTick`: lavoratori indipendenti, lotti interi, capienza indipendente per Frutta e Verdura, eventuale precedenza sulla risorsa Acqua condivisa, rifiuto input estranei, nessuna modifica allo stock originale.
- I prodotti sono **alimenti grezzi**: per diventare Cibo utilizzabile necessitano di deposito e lavoratori. Il test di integrazione passa dagli output del planner al `FoodDepotPlanner.Receive` e `Plan`.
- I coefficienti nei test sono esempi di comportamento, **non valori di bilanciamento approvati**. Non si impone un consumo d'Acqua alle filiere finché non sarà deliberato.

**Non ancora attivo nella partita:** non registrati Orto, Frutteto, Frutta e Verdura nel catalogo, nella GUI o nello schema SQLite. Il gioco corrente continua a utilizzare le vecchie risorse e ricette. L'attivazione completa richiede il deposito unificato F1-D04 e l'integrazione/migrazione F1-D05.

## C. Perimetro e rischi

Sono cambiati il solo store SQLite v14, due file di test e i nuovi modello/test Core, più documentazione. `V14CampaignSession`, la GUI v14, la GUI v13, i cataloghi giocabili, le ricette correnti, il formato SQLite, le tecnologie e il motore E4a restano invariati. La correzione backup è immediatamente efficace; l'ortofrutta resta un modello di dominio isolato.

[Validazione RC42](../validation/F1_D03_RC42_TEST_PLAN.md) · [Roadmap](../ROADMAP.md) · [ADR backup](../adr/0052-backup-v14-oltre-cento-slot.md) · [ADR ortofrutta](../adr/0053-orto-frutteto-modello-isolato.md)
