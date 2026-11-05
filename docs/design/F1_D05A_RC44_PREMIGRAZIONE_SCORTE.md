# F1-D05a / RC44 — Contratto di pre-migrazione alimentare (4 novembre 2026)

## Stato e motivo

RC42 è confermata dall'utente (577 test); **RC43 / F1-D04 è candidata** (590 test attesi, non ancora validati su Windows). RC44 aggiunge **10 test** (600 attesi complessivi), non eseguiti qui. La GUI v14 è la modalità ordinaria; v13 è legacy con database distinto.

Il modello F1-D04 separa scorte di ingredienti nel Deposito e Cibo pronto. La v14 attuale salva invece `food`, `fish` e `bread` in `resource_stock`, assieme a tutte le altre risorse. Collegare subito il nuovo tick potrebbe quindi duplicare Cibo oppure perderlo a causa della capienza del nuovo edificio. Prima serve una mappatura **esatta, riproducibile e reversibile**.

## Implementazione RC44

- `src/SimRisorse.Core/Food/V14FoodStockMigration.cs` introduce un piano immutabile, **non persistito**, per trasferire `food` in `FoodDepotStock.ReadyFood` e `fish`, `bread`, `meat`, `fruit`, `vegetables` in `FoodDepotStock.RawFood`. Le altre risorse restano nella città.
- Il piano conserva il riferimento al checkpoint sorgente v14 e ricostruisce il `ResourceBundle` originale **senza perdere un millesimo**. La ricostruzione rifiuta duplicazioni, aggiunte e perdite di scorte.
- `AssessCapacity` calcola gli eventuali deficit per singolo alimento e Cibo pronto. **Non taglia scorte storiche oltre capacità.** Il futuro caricatore dovrà tollerare tali scorte o non attivare la migrazione.
- `V14SqliteSaveStore.PreviewFoodMigration(policy)` usa il caricamento v14 **già validato** in modalità SQLite read-only, senza scrivere, copiare, promuovere versioni o generare backup. `Save`, `Load`, percorso, schema e user_version della v14 restano invariati.
- I parametri delle ricette nei test sono fixture unitarie; nessuna conversione viene eseguita durante la migrazione (in particolare, Pane **non** diventa Cibo pronto).

## Invarianti verificabili

1. Fonte accettata: esclusivamente snapshot `civil-growth-14`, formato 3; non si può reinterpretare un futuro formato 4 come v14.
2. `food` ha un solo proprietario: deposito Cibo pronto. Pane/Pesce e altri alimenti hanno un solo proprietario: deposito ingredienti. Cibo non resta in parallelo nell'inventario cittadino.
3. Riconversione esatta al vecchio inventario; planks, stone, strumenti, Matite, valuta e altri dati non vengono modificati.
4. Le scorte non vengono convertite, consumate o limitate durante il passaggio; frazioni al millesimo integre; stock pari a zero resta zero.
5. Il piano è di sola lettura: non introduce alcuna attivazione parziale nella sessione e non fa cambiamenti a file v13/v14.
6. L'overcapacity va segnalata, non eliminata; il deposito deve poter partire con il Cibo originale (es. i 30 iniziali) anche se la futura capacità ordinaria sarebbe inferiore.

## Vincoli che **non sono** ancora implementati

Questo non è il salvataggio v15, né la migrazione in produzione. Mancano: formato SQLite nuovo con inventario deposito persistente e versione regole nuova, copia/backup verificato + attivazione atomica + recupero in caso di errore, nuovi edifici/risorse e tier, integrazione dei tick, consumi e GUI. Non richiedere al giocatore di cancellare o spostare i file attuali.

## Continuazione F1-D05

- **D05b:** specifica ed implementazione di un file **nuovo e separato** per il salvataggio v15, con inventario deposito e registri nuovi. Guardrail su file preesistente, supporto overflow storico, test corruzione/duplicazione; nessun overwrite della v14.
- **D05c:** migrazione concreta copy-on-write della v14 nella v15 con checksum/round-trip e rollback. Non sovrascrivere la sorgente; nessun doppio consumo del Pane.
- **D05d:** cataloghi, Stalla/Cacciatore/Macellaio, Orto/Frutteto, personale nel Deposito, tick e popolazione, UI v14. Attivare tutto insieme quando i gate precedenti sono verdi; i coefficienti E4b restano non approvati.
- **E4a rinnovata:** misurare realmente le nuove filiere prima di E4b.

[Roadmap](../ROADMAP.md) · [Piano test](../validation/F1_D05A_RC44_TEST_PLAN.md) · [ADR 0055](../adr/0055-preflight-migrazione-alimentare-immutabile.md)
