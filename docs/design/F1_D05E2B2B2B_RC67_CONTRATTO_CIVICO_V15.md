# RC67 / F1-D05e.2b.2b.2b — Primo contratto civico v15 autonomo

## Perimetro effettivamente implementato

- Nuovo `Core/Food/V15CityCampaignSnapshot.cs`: record civico v15 con 15 campi del dominio, ma senza coppia versione/regole v14.
- `FromInherited(V14CampaignSnapshot)`: controllo stretto su `FormatVersion` e `RulesVersion`, conversione senza trasformazioni economiche.
- `ToInherited()`: solo per consumer preesistenti; ricostruisce l'intestazione che il DTO legacy richiede, conservando tutte le altre proprietà.
- `V15FoodCampaignCheckpoint.NativeCityCampaign`: proiezione a sola lettura dello stato civico esistente.
- `V15CityCampaignValidator.ValidateNative`: usa il nuovo tipo per verificare direttamente catalogo, lavoratori, ricerca, Scuola, popolazione, tier, cronologia e capienze. `Validate` storico rimane come ponte esplicito.
- `V15CityStorage.Calculate` accetta il nuovo tipo; la capacità Matite riutilizza le regole condivise esistenti.

## Confini non ancora convertiti

`V15FoodCampaignCheckpoint.CityCampaign` rimane `V14CampaignSnapshot`; `V15SqliteFoodSaveStore` legge e scrive ancora lo stesso DTO di raccordo. I tipi interni delle famiglie e dei contatori conservano il nome v14; il vecchio contatore `food-store` può persistere per storia acquisti. Lo slot fittizio del `ProductionPlanner` è locale e disabilitato. **Non si dichiara conclusa la separazione dalla v14.**

## Non regressione

Nessun file sotto `src/SimRisorse.Desktop/` è modificato. La GUI canonica di RC63 rimane valida (bloccati in grigio consultabili; dettagli con icone; disposizione comandi costruzione/lavoratori; crescita sotto menu; Risorse a sette colonne; Scuola/Esercito). Nessuna modifica a DB gen. 6, prezzi, clock, ricette, capacità, salvataggio v14 o progressione economica.

## Roadmap tecnica

1. Validare cumulativamente RC64–RC67 su .NET 10 (797 test attesi).
2. RC68: introdurre portafogli e contatori v15 nativi e convertire i consumer del checkpoint uno per uno.
3. Eliminare lo slot fittizio dal planner senza modificare le ricette.
4. E4a benchmark; E4b crescita costi quasi esponenziale, squadra minima Zecca 10 per edificio.

[ADR 0075](../adr/0075-contratto-civico-v15-proiezione-nativa.md) · [Test](../validation/F1_D05E2B2B2B_RC67_TEST_PLAN.md).
