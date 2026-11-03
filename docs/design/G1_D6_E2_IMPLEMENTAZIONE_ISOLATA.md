# SimRisorse — G1-D6 / E2: livelli produttivi, upgrade e proiezione economica

**3 novembre 2026 · v60-E2-RC1, candidato da validare su Windows.** Questo incremento estende E1, ma **non è collegato** alla partita corrente `civil-growth-13`, ai salvataggi, ai cataloghi storici né alla UI. Nessun coefficiente nuovo è dichiarato approvato: 1 / 2,6 / 6,5 e +15% restano esempi sperimentali. La decisione già confermata dall'utente è il saldo di ricerca libero e spendibile quando vuole.

## Implementato nel Core (isolato)

1. **`TieredProductionFamily` e `TieredProductionCatalog`**: Base/Grande/Avanzata, rispettivamente 1/3/10 posti. Mapping esplicito di **16 famiglie produttive** verso i rami `lavorazioni`, `alimentazione`, `estrazione`, `industria`; case, Municipio, Magazzino e Caserma restano esclusi. I livelli superiori richiedono gli ID tecnologia pertinenti; Avanzata richiede entrambi I e II, anche per un acquisto diretto. Cataloghi storici non ottengono famiglie inesistenti. Il rapporto costi è fornito dall'esterno, non imposto al gameplay.
2. **`TieredBuildingPortfolio`**: quantità ed assegnazioni per livello, modello aggregato e immutabile. Importa uno stato precedente di produttore come livello Base senza duplicare lavoratori. Non attribuisce automaticamente i posti liberi a persone. Il miglioramento di un edificio trasferisce deterministicamente **fino a 1/3 lavoratori** già assegnati al livello di origine, senza richiedere ID individuali, e preserva il totale dei lavoratori. Nessun downgrade.
3. **`TieredConstruction`**: acquisto di nuovi livelli e miglioramento Base→Grande→Avanzato, incluso Base→Avanzato. Usa i prezzi progressivi già realizzati in E1, controlla disponibilità della ricerca (solo preflight; lo sblocco tecnologico non è qui implementato), fondi e presenza dell'edificio da aggiornare. Risorse, storico acquisizioni e portfolio sono restituiti **insieme soltanto se l'operazione riesce**. Il miglioramento non aumenta il contatore familiare.
4. **`TieredEconomyProjection`**: per una simulazione isolata unisce `BuildingState` non modificati e famiglie a livelli, converte i soli lavoratori aggregati al `ProductionPlanner` **già esistente**, e computa la capacità locale moltiplicando il deposito base per la somma dei posti 1/3/10. Non introduce nuova ricetta o bonus per lavoratore; il contributo non richiede che i posti siano occupati. Rifiuta doppia rappresentazione degli edifici, riferimenti sconosciuti e scorte superiori alla capienza. **NON** è ancora un nuovo motore di avanzamento: non comprende consumo popolazione, avanzamento temporale, reclute o crescita.

## Limiti voluti e punti di integrazione E3

- I tier NON sono acquistabili nella campagna: la costruzione ufficiale rimane `GameSession.Construct` con prezzo lineare. Nessun `civil-growth-14` o v3 del salvataggio è stato introdotto; mai caricare partite reali con il codice non validato.
- L'insieme delle tecnologie sbloccate è passato a E2 come parametro di preflight. Formazione, Scuola, produzione ricerca e acquisto tecnologie verranno implementati in E3. Non presentare come approvati l'albero completo o i costi candidati.
- Per l'attivazione in E3 occorre un aggregato persistente con `ConstructionAcquisitionHistory`, portfolio per tier, persone disponibili, tecnologie e saldo ricerca; migrazione con backup S1, validazioni contro costi/stoccaggi e snapshot round-trip. Non basta copiare i tier in `GameSnapshot` v2.
- La regola deterministica di distribuzione lavoratori di un singolo upgrade è una **politica candidata**; la UI dovrà esplicitare quali persone sono trasferite da una taglia aggregata. L'alternativa futura è dare ID individuali agli edifici, più costosa in termini di migrazione e UI.
- I test di simulazione qui non dimostrano che i prezzi sono raggiungibili partendo da otto abitanti: anti-softlock e bilanciamento end-to-end restano E4.
- Eventuali overflow o quote impossibili generano eccezioni nel modello puro; in E3 dovranno essere mappati a errori di comando senza mutazione, con preflight di prerequisiti, capacità e popolazione.

## Test aggiunti

- **Core**: configurazione 1/3/10; sblocchi; migrazione in Base; assegnazioni; upgrade diretto e per tappe; conservazione lavoratori; prezzi e contatori E1; controlli atomici; capacità locale +50/+150/+500; produzione limitata da input, lavoratori e deposito; collisioni tra ricette; reject di duplicati; sessione legacy intatta.
- **Application**: copertura delle 16 famiglie effettive di `CivilGrowthScenario.Create()`, esclusione di case/Municipio/Caserma/Magazzino e compatibilità con il catalogo economico storico di tre produttori.

**Validazione:** sorgenti verificati staticamente, archivio ZIP e manifest controllati. **SDK .NET 10 assente in ambiente:** i nuovi test NON sono stati eseguiti. La v59-RC1 resta ultima baseline con `validate.cmd` superato dall'utente. Alla prima occasione estrarre il nuovo ZIP in cartella separata, poi `./validate.cmd` da Windows; raccogliere output, senza promuovere la v60 a release stabile prima del gate.
