> **Aggiornamento RC49:** la persistenza dei transiti, animali ed edifici è ora implementata nello [schema 5](F1_D05D2_RC49_CHECKPOINT_FILIERA_SQLITE5.md); la vecchia richiesta di migrare tutti i salvataggi non è più vincolante. Il documento sotto fotografa lo stato storico RC48.

# RC48 / F1-D05d.1 — Coordinamento deterministico delle filiere alimentari

**Data:** 5 novembre 2026. **Stato:** implementato in Core isolato, **test .NET 10 da eseguire**. **RC47 confermata dall'utente: 620 test superati**. RC48 contiene 11 nuovi test e ne attende 631 cumulativi.

## Scopo della consegna

`FoodIndustryTickPlanner` (`src/SimRisorse.Core/Food/FoodIndustryTickPlanner.cs`) compone in **un unico calcolo puro** i tre modelli già presenti: `MeatChainPlanner`, `HorticulturePlanner` e `UnifiedFoodSupply`. Questo riduce il rischio di consumare due volte Acqua, produrre Cibo senza lavoratori, smarrire scorte bloccate per capienza o macellare animali generati nello stesso tick.

La RC48 non altera i parametri attivi, non aggiunge edifici al catalogo della partita, non modifica salvataggi/SQLite né il codice dell'interfaccia. È il **primo incremento interno a F1-D05d**, non la sua conclusione e non un'attivazione giocabile.

## Proprietà esclusive degli inventari

| Inventario | Risorse | Provenienza | Stato in RC48 |
| --- | --- | --- | --- |
| `City` | Risorse urbane incluse Grano, Acqua, Pesce e Pane, **mai Cibo pronto** | Campagna v14/v15 | La città conserva solo beni che il catalogo storico rappresenta |
| `Animals` | `cattle` (Bovini), `game` (Selvaggina) | Stalla e Cacciatore | Necessita nuova persistenza prima dell'attivazione |
| `Transit` | `meat`, `fruit`, `vegetables` | Macellaio, Frutteto, Orto | Conserva eccedenze che il Deposito non può ricevere; necessita nuova persistenza |
| `Depot.RawFood` | `fish`, `bread`, `meat`, `fruit`, `vegetables` | Trasferimento con limiti di capienza | Contratto v15 già presente |
| `Depot.ReadyFood` | `food` | Solo lavorazione del Deposito | Contratto v15 già presente |

**Non unire `Animals` o `Transit` alle risorse cittadine del checkpoint RC47.** `V15FoodCampaignCheckpoints.Validate` richiede esplicitamente che la città non contenga alimenti non posseduti dal deposito; `V15SqliteFoodSaveStore` registra solo la città e i due inventari del deposito. Se lo stato intermedio fosse ignorato, si perderebbero Bovini, Selvaggina e prodotti in attesa; se venisse copiato nella città, il salvataggio sarebbe incompatibile.

## Ordine esatto di un passo

1. **Preflight**: policy canoniche, inventari a proprietario unico, personale non negativo, addetti minori o uguali ai posti effettivi, somma addetti minore o uguale a `Population - BusyOutsideFood`. Il chiamante deve includere nel budget esterno lavoratori delle filiere non alimentari, militari, studenti e ricercatori già impegnati, senza duplicarli.
2. **Allevamento/caccia/macellazione**: Grano e Acqua alimentano la Stalla; i Cacciatori producono Selvaggina; il Macellaio usa soltanto Bovini e Selvaggina disponibili **all'inizio** del passo. I nuovi animali sono macellabili dal passo successivo.
3. **Orto/Frutteto**: lavoratori separati, acqua residua dopo la Stalla; acqua utilizzata una volta sola, priorità della policy, capienze dedicate.
4. **Consegna al Deposito**: Pesce/Pane dalla città e Carne/Frutta/Verdura dal transito, al più fino alla capacità residua; ingredienti eccedenti rimangono al rispettivo proprietario.
5. **Produzione Cibo**: il Deposito processa solo lotti completi che possono essere lavorati dai suoi addetti e contenuti nella capacità di Cibo pronto; rispetta quota protetta di Pane.
6. **Razione atomica**: se mancano Cibo o Pane per la razione avanzata, il pasto è rifiutato senza un consumo parziale; l'esito va ancora collegato alla progressione di crescita della popolazione.

Tutti gli input sono immutabili; l'esito è `FoodIndustryTickResult`, comprensivo di scorte finali e dettagli di ciascuna filiera. Errori di validazione **non causano transizioni parziali**. Il numero di lotti per addetto è quello del modello Core già esistente: nessun coefficiente economico o costo nuovo viene introdotto.

## Limiti e prossima integrazione obbligatoria

- **Persistenza**: il record v15 RC47 non ha stati per nuove strutture, lavoratori né scorte intermedie. Prima di una sessione giocabile occorrono tabelle/tipi versionati e migrazione copy-on-write dal v15 esistente; **non reinterpretare `user_version=4`** con uno schema incompatibile. Rifiutare file futuri sconosciuti. La designazione esatta della successiva versione sarà fissata insieme al contratto SQL.
- **Edifici**: introdurre direttamente Stalla, Cacciatore, Macellaio, Orto e Frutteto, con prerequisiti, costi e slot, più Deposito aggiornato. Il calcolo dei `Workplaces` deve essere **derivato dagli edifici posseduti** e dalle relative dimensioni; non accettare dal client un numero di addetti arbitrario. Nessuna ComboBox per i tier.
- **Simulazione**: collegare il coordinatore al medesimo orologio di crescita, ricerca e industria, garantendo ordine di produzione/consumo e nessuna doppia assegnazione. Salvataggio e resume devono riprodurre lo stesso tick.
- **GUI**: mantenere navigazione e grafica v14, tabella Risorse a sette colonne e ordine alfabetico, diagnostica di capacità e passaggi dei prodotti, senza alterare v13.
- **Attivazione**: solo dopo backup consistente, puntamento esplicito della nuova partita, verifica recovery post-crash e test su copie; non operare sul database reale durante le prove.
- **Economia**: le ricette dei test sono **fixture**, non bilanciamento approvato. E4a misura la nuova filiera; E4b rimane separata e prevede Zecca a 10 addetti per modulo.

[ADR 0059](../adr/0059-confini-produzione-alimentare-e-proprieta-stock.md) · [Piano test](../validation/F1_D05D1_RC48_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
