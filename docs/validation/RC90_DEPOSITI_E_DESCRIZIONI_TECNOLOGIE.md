## RC90.2 — Hotfix fixture capienza città (da validare)

**Run Windows RC90.1:** 248 Core superati, 557 Application con 8 fallimenti, 136 Infrastructure superati, 216 Desktop superati. Totale 1149 passati, 8 falliti su 1157. Tutti gli otto fallimenti sono in `Rc90FoodDepotTiersTests.Setup()`: le 10.000 unità di `planks` e `stone` per ciascuna risorsa superavano la capacità reale cittadina. **Non modificare il validatore per far passare il fixture.**

Il nuovo fixture aggiunge 800 `planks` e 450 `stone` alle scorte (totali 920/530), entro il limite del Municipio di 1000 ciascuna; la somma copre le quote per un deposito Grande e uno Avanzato. L'asserzione di capienza usa `V15CityStorage.Calculate` come il gioco. Eseguire `./validate-f1.cmd` da Windows (.NET 10): gate atteso **1157/1157**. Verificare specialmente gli 8 test RC90 e gli altri 1149 già passati: qui nessuna esecuzione xUnit possibile. Nessun cambio a schema SQLite, motoristica o GUI.

---

## RC90.1 — Hotfix compilazione (storico, compilazione riuscita)

Nel primo tentativo RC90 il compilatore Windows ha segnalato `CS1503` in `FoodIndustryCampaignState.cs` per passaggio di popolazione `long` a `Validate(int, long)`. RC90.1 allinea la firma a `Validate(long, long)`. Il test Core `WorkplacesAreDerivedFromSixOwnedBaseBuildingCounts` è stato esteso per usare `int.MaxValue + 1L`, senza aumentare il numero di test. **Eseguire `./validate-f1.cmd` (1157 attesi) e verificare anche il round-trip a 6/8 righe del deposito.** Nessun esito .NET 10 ottenuto nell’ambiente di preparazione.

# RC90 — Piano di validazione tecnologie e Depositi alimentari

**Baseline confermata:** RC89.1 = 1135/1135. **Candidato RC90:** +22, obiettivo 1157/1157.

1. `./validate-f1.cmd` su Windows con .NET 10 SDK. Il gate deve riportare nessun fallimento/ignorato. Non dichiarare successo prima dell'esecuzione.
2. Albero tecnologico v15: selezionare le 8 tecnologie acquistabili e vedere i nomi degli edifici Grande/Avanzato effettivi. Controllare nomi della Zecca, miniere, Forno e Deposito; su Logistica/Navigazione vedere che la ricerca non è implementata.
3. Selezionare Deposito Grande/Avanzato ancora bloccati: devono restare grigi ma consultabili, con icona, costi e capacità nel dettaglio; il testo mostra la ricerca mancante anche quando manca Pescatore. Nessuna ComboBox. Nella tabella Risorse devono rimanere sette colonne e i tooltip RC89.1.
4. Eseguire acquisto reale `alimentazione-1`: `food-depot-large` costruibile con materiali disponibili, `food-depot-advanced` ancora bloccato. Acquistare `alimentazione-2`: costruire il livello Avanzato e verificare le quote progressive per famiglia e il consumo dei materiali.
5. Verificare posti 1/3/10 e assegnazioni distinte per livello; 3 lavoratori assegnati al Grande non occupano quelli del Base. Il clock consuma ingredienti e produce Cibo da moduli assegnati; sommare capacità senza raddoppiare lo stock.
6. Roundtrip SQLite generation 6 con 8 righe `food_industry_site`: mantenere conteggi, lavoratori, tecnologie e risorse. Verificare caricamento file a 6 righe (livelli superiori azzerati), rifiuto di 7 righe e di tier acquistati senza ricerca.
7. Confrontare la GUI pixel art v15 con la v14 canonica: nessun cambiamento a scrollbar, pulsanti, dettagli, filtri salvo le due righe aggiuntive e le nuove descrizioni.

**Consegna:** RC90 è candidata finché non viene eseguita la verifica Windows. Le curve A/B/C e il regime MintTen predefinito restano fuori ambito.
