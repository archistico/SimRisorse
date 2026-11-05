# ADR 0064 — RC53: Pesce e Pane possono attendere in città prima del Deposito

- **Data:** 5 novembre 2026
- **Stato:** adottato tecnicamente, implementato in RC53; validazione Windows pendente.
- **Ambito:** checkpoint v15 e contratti di inventario; sostituisce solo la parte troppo restrittiva delle verifiche di proprietà introdotte in RC46/RC49. Non cambia l'ADR 0063 sul Cibo pronto.

## Contesto

La RC52 compilava e avviava 703 test, di cui 695 superati e 8 falliti. I test del clock e della proiezione delle risorse dimostrano che la produzione del Pescatore e del Forno inserisce inizialmente **Pesce e Pane nell'inventario cittadino**. La consegna al nuovo Deposito avviene successivamente, tenendo conto della capienza. Non è una duplicazione: scorta cittadina e scorta grezza del Deposito sono due porzioni distinte della stessa risorsa, che la GUI somma senza confonderle.

Il precedente controllo `CityCampaign.Resources.Amounts.Keys.Any(edibleIds.Contains)` vietava erroneamente qualunque Pesce o Pane in città, rendendo impossibile rappresentare correttamente materiale non ancora consegnato o rimasto fuori dal Deposito pieno.

## Decisione

1. **Cibo pronto (`food`)**: esclusivamente `Depot.ReadyFood`; vietato in `CityCampaign.Resources` e `Depot.RawFood`. La Scuola e la popolazione usano lo stock pronto del Deposito.
2. **Pesce e Pane (`fish`, `bread`)**: possono esistere in `CityCampaign.Resources` (non consegnati) e `Depot.RawFood` (consegnati). Sono quantità fisiche diverse; la proiezione le somma una sola volta per contenitore.
3. **Bovini e Selvaggina**: solo `Industry.Animals`. **Carne, Frutta e Verdura**: solo `Industry.Transit` prima della consegna e `Depot.RawFood` dopo. Non sono consentiti nella città.
4. **Capienze**: produzione rispetta gli spazi ancora liberi e preserva materiali non consegnati. Scorte già eccedenti le nuove capacità non vengono tagliate.
5. Le fixture devono modificare `ProductiveFamilies` per Pescatore, Fattoria e Pozzo e non soltanto `OtherBuildings`. I worker devono risultare validi *prima* di testare il rifiuto di assegnazioni aggiuntive.

## Conseguenze e controllo

- `V15FoodCampaignCheckpoints.Validate` distingue Cibo pronto dagli ingredienti in attesa, lasciando invariato il controllo di proprietà esclusiva di animali e intermedi.
- `V15SqliteFoodSaveStore` usa sempre la generazione **5**; nessuna migrazione aggiuntiva è necessaria perché l'inventario cittadino già serializza Pesce e Pane.
- Il test storico che rifiutava Pesce in città viene corretto per verificare il divieto effettivo di **Cibo pronto** nella città.
- Due regressioni nuove coprono aggregazione visiva e round-trip SQLite di Pesce/Pane in entrambi i contenitori.
- RC53 attende `validate.cmd`: **705 test attesi** (703 precedenti + 2), non dichiarati verdi.
- La GUI pixel art v14 e il salvataggio personale non sono modificati.

[Stato](../CURRENT_STATE.md) · [Piano test RC53](../validation/F1_D05D4A_RC53_REGRESSIONI_TEST_PLAN.md) · [Rapporto](../design/F1_D05D4A_RC53_FIX_REGRESSIONI.md).
