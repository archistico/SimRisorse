# G1-D14 — E3b/3d: costruzione, upgrade e lavoratori nella sessione v14

**Release candidata:** `v60-E3b3-RC4`. **Ultima validata su Windows dall'utente:** `v60-E3b3-RC3`, 396 test superati. **Test .NET RC4 ancora da eseguire.**

## Perimetro

Questa fase collega gli algoritmi E1/E2 (già testati isolatamente) a `V14CampaignSession`, senza modificare il ruleset giocabile `civil-growth-13`, la GUI, il suo SQLite v2 né lo store v14 v3. Le 16 famiglie produttive usano tre taglie: Base (1 posto), Grande (3 posti), Avanzato (10 posti). Le categorie non produttive rimangono fuori da questa API.

## Nuove API isolate

- `BuildProducer(familyId, tier, quantity)` — verifica quantità, famiglia produttiva, prerequisiti di edificio, tecnologia, costi e disponibilità; addebita per ogni nuovo edificio e aggiorna il contatore storico della famiglia.
- `UpgradeProducer(familyId, from, to)` — passa a un tier superiore (anche Base → Avanzato direttamente), trasferendo soltanto i lavoratori già assegnati; addebita il differenziale calcolato allo stesso contatore e **non** incrementa gli acquisti storici.
- `AssignProducerWorkers(familyId, tier, workers)` — prenota solo abitanti liberi in posti effettivamente disponibili; non crea lavoratori.
- `ReleaseProducerWorkers(familyId, tier, workers)` — restituisce alla popolazione libera lavoratori già assegnati, senza eliminare l'edificio.
- `V14TierConstructionRules` — parametri iniettabili (crescita, moltiplicatore Grande, Avanzato). Default **sperimentali**: 1,15; 2,6; 6,5. Il vincolo di ricerca rimane affidato all'albero proposto E3a.

Ogni comando produce uno snapshot candidato immutabile e lo pubblica soltanto dopo `V14CampaignSnapshots.Validate`. Gli errori ordinari hanno codici dedicati (`MissingPrerequisite`, `MissingResearch`, `InsufficientResources`, `InsufficientPopulation`, `InsufficientWorkplaces`, ecc.). In caso di errore non si modifica alcun dato e non si producono addebiti parziali. Overflow numerici diventano `NumericLimit`.

## Prezzi e conservazione

La formula, **non ancora approvata definitivamente**, è `ceil(costo_base × moltiplicatore_livello × crescita^K)` per ciascuna risorsa, con `K` acquisizioni storiche. Un pacchetto di N costruzioni è la somma delle N quotazioni successive, non N volte la stessa quotazione. Con una Segheria iniziale già acquisita (`K=1`), i prezzi campione sono:

| Azione | Legno lavorato | Pietra | K al termine |
| --- | ---: | ---: | ---: |
| Nuova Segheria Base | 46 | 23 | 2 |
| Nuova Segheria Grande diretta | 120 | 60 | 2 |
| Nuova Segheria Avanzata diretta | 299 | 150 | 2 |
| Miglioramento Base → Grande | 74 | 37 | 1 |
| Miglioramento Grande → Avanzato | 179 | 90 | 1 |
| Miglioramento Base → Avanzato | 253 | 127 | 1 |

Il miglioramento conserva il numero di edifici e i lavoratori già assegnati, mentre i posti aggiuntivi rimangono liberi finché il giocatore non assegna nuovi abitanti. Produzione e stoccaggio vengono già calcolati dall'orologio v14 con la ricetta e le capacità per lavoratore. Nessun moltiplicatore extra alla produttività.

## Compatibilità e limiti

- Nessuna modifica alle classi `GameSession` v13, SQLite v2/v3, UI Avalonia, motore militare o ricette; il nuovo componente è interno al Core v14 isolato.
- Snapshot v14 continuano a rappresentare separatamente Base/Grande/Avanzato, lavoratori e contatori, già leggibili da SQLite v3. Un test di round-trip ripristina una colonia costruita e potenziata e ne confronta un successivo avanzamento di 20 s.
- **Non ancora pronto per il gioco pubblico:** menu costruzione/upgrade, presentazione dei costi, ricerca/Scuola nella GUI, riscontro anti-softlock, bilanciamento e scelta finale dei coefficienti. I valori numerici non diventano decisioni approvate con questa implementazione.
- I prezzi passati restano storia di acquisto e non debito retroattivo; i parametri economici verranno congelati nel ruleset finale prima dell'attivazione/di eventuali migrazioni aggiuntive.
- La sessione deve essere usata serialmente dal chiamante, come `GameSession`; non c'è sincronizzazione concorrente nel prototipo.

## Test e validazione

Nuove suite:

- `tests/SimRisorse.Application.Tests/V14TierSessionTests.cs`: 11 nuovi test su costi, tech, prerequisiti, upgrade diretto/a tappe, multipli, staffing, vincoli demografici, fallimenti atomici, parametri iniettati.
- `tests/SimRisorse.Infrastructure.Tests/V14TierSessionPersistenceTests.cs`: 1 nuovo test di SQLite v3 dopo costruzione, potenziamento, staffing e ripresa produzione.

**Attesi 408 test** (396 della RC3 validata dall'utente + 12 nuovi). Non è stato eseguito .NET in questo ambiente. Dalla root della nuova estrazione su Windows: `./validate.cmd` o `.\validate.cmd` in PowerShell. Eseguire i test su copia; non sostituire il database personale v13.

## Prossimi gate

1. RC4: compilazione e test Windows 408/408, più validatore; correggere eventuali regressioni.
2. E3b/3e: replay multi-azione deterministico dopo SQLite v3 (incluse modifiche in tempo simulato, rollback, equivalenza).
3. E3b/4: GUI pixel art per Scuola, albero, costruzione, livelli, upgrade, staffing.
4. E4: prezzi calibrati, test di raggiungibilità, snapshot storici e attivazione con rollback.
5. Successivamente: G1-N navale e M5.4–M7.
