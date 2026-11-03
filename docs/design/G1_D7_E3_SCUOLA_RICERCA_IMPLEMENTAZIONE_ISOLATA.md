# SimRisorse — G1-D7 / E3a: Scuola, ricercatori e acquisto volontario delle tecnologie

**3 novembre 2026 — v60-E3-RC1. Stato: prototipo Core + test scritti, NON compilato né validato su Windows.**

## Fonte di verità

- **Ultimo codice giocabile validato:** v59-RC1, `civil-growth-13`, validatore completo superato secondo l'utente.
- **Ramo v60:** E1, E2 ed E3 sono prototipi Core, **non collegati** a `GameSession`, `GameSnapshot` v2, SQLite, desktop o simulazione ufficiale.
- **Scelta vincolante:** i punti ricerca si accumulano senza progetto selezionato; il giocatore spende **quando vuole** (ADR 0021 accettato).
- **Proposte NON approvate:** costi della Scuola e formazione, 1 posto di formazione/Scuola, 2 postazioni ricercatori/Scuola, 120 secondi, 10 Cibo, 0,1 punti/s, prezzi delle otto tecnologie, politica rimborso formazione, ritorno al lavoro civile. Queste regole sono parametri/candidati di prova, non numeri live.

## Sviluppato nel Core

1. **`SchoolResearchRules`**: tutte le tariffe e capacità sono parametri immutabili; costo formazione espresso in una risorsa del catalogo, per il prototipo `food`. Nessuna creazione automatica di abitanti.
2. **`SchoolResearchState`**: studenti in coda, progressi individuali per posto, ricercatori qualificati e ricercatori attivi. `ReservedPopulation = attesa + formazione + qualificati`; gli attivi sono *parte* dei qualificati e non vengono contati due volte. Posti di formazione e posti ricerca sono distinti. Progressi ordinati dal più avanzato al meno avanzato.
3. **`SchoolResearch.Enroll`**: prenotazione di abitanti liberi, con controllo su lavoratori civili, esercito e ricerca preesistente; pagamento Cibo per tutti i nuovi studenti, stato restituito solo su successo, coda deterministica. La scuola non genera popolazione.
4. **`SchoolResearch.Advance`**: avanzamento per secondi di simulazione, formazione parallela per posto e sequenziale in coda. I neoqualificati restano inattivi sino ad assegnazione **volontaria**. `ResearchLedger` accumula punti solo con ricercatori attivi, a tariffa parametrica; niente ricerca automatica.
5. **`SchoolResearch.Assign/Unassign/ReturnToCivilian`**: occupazione posti ricerca distinta dalla qualifica; il rientro civile richiede un ricercatore qualificato inattivo e ne rimuove la qualifica, senza creare abitanti. Un ritorno in ricerca richiederebbe nuova formazione: **politica candidata, non gameplay approvato**.
6. **`SchoolResearch.CancelTraining`**: cancellazione prima dalla coda e poi dai posti meno avanzati; rimborso integrale del Cibo **soltanto se c'è capacità di stoccaggio**. L'operazione non produce effetti parziali. Politica esplicitamente *candidata*; da discutere prima del collegamento alla GUI.
7. **`TechnologyDefinition` / `TechnologyCatalog`**: grafo verificato senza prerequisiti mancanti o cicli; otto tecnologie produttive candidate e tre voci rinviate (Logistica I/II, Navigazione). La loro disponibilità nel catalogo sperimentale non le rende acquistabili nel gioco corrente. Famiglie sbloccate coerenti con le 16 mappate in E2.
8. **`ResearchLedger` / `TechnologyPurchases`**: saldo e tecnologie in forma immutabile, acquisto esplicito e atomico, no duplicati, verifica di saldo/prerequisiti/edifici; tecnologia non implementata sempre non acquistabile. Gli ID tecnologia sbloccati si passano direttamente al `TieredProductionFamily.IsUnlocked` della E2.
9. **`ResearchPrototypeCheckpoint`**: DTO *solo del prototipo*, tag `research-prototype-e3-1`, saldo, tecnologie, coda, progressi, qualificati e attivi. Restore con validazione di tecnologia, posti e popolazione totale; round-trip JSON nel test. **Non** è `GameSnapshot` v3, non è una nuova tabella SQLite, non autorizza migrazione dei salvataggi personali.

## Correzione proattiva del ramo E2

In `Construction/TieredProductionFamily.cs` la factory `TieredProductionCatalog.FromCatalog` usava `ImmutableArray.Builder.MoveToImmutable()` su un builder a capacità variabile: per il catalogo storico con 3 produttori (capacità interna tipicamente 4) può lanciare `InvalidOperationException`. È stato sostituito con `ToImmutable()`, che conserva gli elementi indipendentemente dalla capacità. Il test Application E2 `HistoricalCatalogOnlyProvidesExistingProductiveFamilies` copre questo caso ed era già presente, **ma non è stato ancora eseguito**. I restanti 170 file tecnici preesistenti non sono cambiati. Il nuovo E3 usa `ToImmutable()` anche quando acquisisce le liste dei prerequisiti.

## Garanzie e limiti

- Un abitante non può essere contemporaneamente lavoratore, militare o ricercatore se i controlli di popolazione sono forniti con i conteggi corretti. **Il futuro aggregato `GameSession` dovrà diventare fonte unica di questi numeri**; qui sono parametri dei servizi puri.
- La produzione per ricercatore non fa parte di `ResourceBundle` e non usa la capacità dei Magazzini.
- Un acquisto di tecnologia fallito restituisce esattamente lo stesso `ResearchLedger` in memoria; non spende punti.
- La formazione usa il clock di simulazione, non il tempo reale; la pausa e il caricamento della partita saranno gestiti dall'integrazione futura.
- La Scuola **non è ancora un edificio catalogato**, non esistono UI Scuola/Ricerca, nuovo ruleset `civil-growth-14`, snapshot v3 o migrazione S1. Non attribuire al prototipo queste funzionalità.
- È ancora aperta la ripartizione definitiva del lavoro E3b (GameSession/SQLite/UI) rispetto a E4 (attivazione simultanea di prezzi+upgrade+ricerca). La cosa fondamentale è non attivare i costi crescenti **prima** delle alternative di upgrade e ricerca.

## Test aggiunti (NON eseguiti qui)

- **`TechnologyPurchasesTests`**: saldo libero, scelta volontaria, indipendenza rami, prerequisiti, costo, acquisto non ripetuto, ricerche rinviate, prerequisiti di edificio, integrità del DAG, ripristino invalido, copertura 16 famiglie, collegamento E2 e percorso completo dalla Scuola al livello Grande.
- **`SchoolResearchTests`**: consumo Cibo, posti separati, disponibilità popolazione, coda e progresso temporale, assegnazione, rientro civile, annullamento con capienza, allineamento temporale, JSON checkpoint, input invalidi e non duplicazione abitanti.

**Gate:** non esiste .NET SDK 10 in questo ambiente. Nessuna dichiarazione di compilazione riuscita. Quando possibile, estrarre lo ZIP in una nuova cartella e lanciare `validate.cmd`; acquisire il log completo. Prima di collegare SQLite/UI, servono test di migrazione **su copie** e una decisione sui numeri proposti.

## Prossimi incrementi

1. **E3b progettazione integrazione:** contratto `civil-growth-14`, Scuola a catalogo, persistenza versionata/snapshot futuro, conservazione del personale e backup S1. Non eseguire migrazioni reali su DB dell'utente senza gate.
2. **E4 integrazione e bilanciamento:** accendere insieme prezzi, taglie, formazione e acquisto tecnologie; test di raggiungibilità dalla colonia iniziale, prezzi compatibili con capacità stoccaggio, controllo 23 obiettivi inclusa Scuola, regressione interfaccia e campagna militare.
3. **G1 navale:** riesaminare costi del Cantiere nel nuovo mercato, poi M5.4–M5.7.
