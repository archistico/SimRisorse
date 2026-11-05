# ADR 0063 — Comandi v15 e proiezione canonica del Deposito

**5 novembre 2026 · Accettata per RC52, validazione .NET 10 pendente**

## Contesto

Con RC51 il clock non produce più Cibo attraverso `food-store`, ma il catalogo v14 contiene ancora quel produttore e la vecchia Scuola sottrae Cibo dalle risorse della città. Richiamare i comandi v14 direttamente sulla nuova partita rischia di duplicare lavoratori o di scrivere Cibo nel contenitore sbagliato.

## Decisione

- Un unico proprietario della campagna v15 (`V15FoodIndustryCampaignSession`) controlla i comandi di costruzione, lavoratori, Scuola, ricerca e reclutamento. I comandi storici non avanzano il tempo; i risultati vengono validati e inglobati nel checkpoint v15 soltanto se riusciti.
- Per costruzioni civili e tier usiamo un adattatore temporaneo alle regole `V14CampaignSession`, non due sessioni attive. La forza lavoro alimentare è conteggiata nelle verifiche dell'azione.
- La Scuola usa il Cibo fisicamente contenuto nel Deposito: quantità temporanea soltanto per invocare le regole pure di formazione e **mai** conservata nella città. La restituzione del Cibo è vincolata alla capacità del Deposito, non alla capacità storica del Cibo cittadino.
- Il nuovo Deposito (`food-depot`) sostituisce il prerequisito `food-store` per la Scuola nella v15. Catalogo e comandi pubblici v15 non rendono più acquistabile il vecchio produttore. Un comando esplicito consente di ritirare strutture e personale residui, senza rimborsare materiali o eliminare altre risorse.
- La proiezione delle risorse e degli edifici per la futura GUI è distinta dal catalogo v14 e conserva i tre tier come voci individuali.
- Lo schema SQLite rimane generation 5; non si introduce un obbligo di migrazione dei database di sviluppo. GUI e percorso di salvataggio v14 rimangono indipendenti.

## Alternative respinte

Riutilizzare direttamente `V14CampaignSession.EnrollStudents` e `CancelStudents`: farebbe pagare Cibo dalla città, violando la proprietà esclusiva del Deposito. Riattivare entrambi i produttori per non cambiare i comandi: duplicherebbe la filiera. Rimuovere subito `food-store` dallo snapshot v14 ereditato: invaliderebbe famiglie, contatori, tecnologie e test di compatibilità senza disporre ancora del nuovo tipo di checkpoint.

## Conseguenze e limiti

È un consolidamento **headless e di transizione**: `food-store` resta nei dati interni storici non giocabili; il nuovo catalogo proietta soltanto gli edifici realmente selezionabili. Le tecnologie ereditate che lo menzionano saranno riorganizzate nella successiva pulizia della struttura v15. Per aprire la GUI v15 servono ancora integrazione e regressioni Avalonia D05d.4b. Non si anticipa il bilanciamento E4b.

[Specifica RC52](../design/F1_D05D4A_RC52_COMANDI_CATALOGO_V15.md) · [Piano test](../validation/F1_D05D4A_RC52_TEST_PLAN.md).
