# SimRisorse — G1-D4: albero tecnologico e gameplay della ricerca (PROPOSTA)

**Data:** 3 novembre 2026. **Base valida:** v59-RC1, `civil-growth-13`, `validate.cmd` passato su Windows secondo l'utente. **Non è codice implementato:** nessuna cifra, milestone o sblocco è stato formalmente approvato. Questo documento approfondisce e integra G1-D3, non supera i contratti proposti ADR 0018/0019.

## 1. Obiettivo progettuale

Il giocatore deve poter progredire lungo strade diverse senza che la ricerca diventi una seconda tassa lineare. La progressione si fonda su tre scelte interdipendenti: **acquistare un'altra struttura** (prezzo per famiglia crescente), **potenziare quella già posseduta** (3 o 10 lavoratori per esemplare), oppure **deviare lavoratori alla Scuola** e investire conoscenza in una filiera. Gli edifici base e i rifornimenti minimi devono restare sempre raggiungibili; nessuna tecnologia sblocca retroattivamente un edificio base v13 già operativo.

## 2. Insieme reale degli edifici e perimetro dei livelli

Le **16 famiglie produttive** in `CivilGrowthScenario` + `EconomyFoundationScenario` sono qui ripartite senza doppioni:

| Settore | Famiglie tecniche | Grandi | Avanzate |
|---|---|---|---|
| Lavorazioni | `tree-feller`, `sawmill`, `stonecutter` | Lavorazioni I | Lavorazioni II |
| Alimentazione | `fisher`, `food-store`, `farm`, `well`, `mill`, `bakery` | Alimentazione I | Alimentazione II |
| Estrazione | `iron-mine`, `coal-mine`, `gold-mine` | Estrazione I | Estrazione II |
| Industria | `iron-smelter`, `gold-smelter`, `forge`, `mint` | Industria I | Industria II |

**Esclusi dai tier produttivi nella prima implementazione:** `small-house`, `large-house` (due categorie civili da non fondere), `warehouse`, `barracks`, `town-hall` (fisso). `school` è proposta nuova, `shipyard` e `castle` sono futuri. Le strutture senza ricetta non devono acquisire posti lavoro fittizi perché appartengono a una famiglia.

Ogni famiglia produttiva conserva **ricetta per lavoratore**, priorità, prerequisiti e risorsa di output. Taglie proposte: 1, 3 e 10 posti lavoro; capacità locale proposta 50/150/500 per output. Risorse condivise come oggi, Pane senza eccezioni.

## 3. Catalogo tecnologico candidato — 8 ricerche attuabili nella prima economia

| Tecnologia (ID) | Punti | Richiede | Sblocca |
|---|---:|---|---|
| Lavorazioni I (`lavorazioni-1`) | 100 | Solo punti ricerca | Taglie grandi di Taglialegna, Segheria, Tagliapietra |
| Lavorazioni II (`lavorazioni-2`) | 350 | Lavorazioni I | Taglie avanzate delle stesse tre famiglie |
| Alimentazione I (`alimentazione-1`) | 100 | Solo punti ricerca | Taglie grandi di Pescatore, Deposito alimentare, Fattoria, Pozzo, Mulino, Forno |
| Alimentazione II (`alimentazione-2`) | 350 | Alimentazione I | Taglie avanzate delle stesse sei famiglie |
| Estrazione I (`estrazione-1`) | 150 | Solo punti ricerca | Taglie grandi delle tre miniere |
| Estrazione II (`estrazione-2`) | 400 | Estrazione I | Taglie avanzate delle tre miniere |
| Industria I (`industria-1`) | 200 | Estrazione I | Taglie grandi di Fonderia di ferro, Fonderia d'oro, Forgia, Zecca |
| Industria II (`industria-2`) | 450 | Industria I | Taglie avanzate delle stesse quattro famiglie |

**Per la futura fase navale, NON acquistabile nella v14 economica:** Navigazione (`navigazione-1`), candidato 250 punti, prerequisiti `lavorazioni-1` **e** presenza di una Zecca; potrà sbloccare il Cantiere quando quel contenuto sarà implementato. L'obiettivo del primo Cantiere resterà soggetto a prezzo e bilancio da approvare con i JSON militari.

**Riservate, NON acquistabili perché prive ancora di un effetto attuabile:** Logistica I (120) e Logistica II (360). Niente voce finta che sottragga punti senza ottenere vantaggi. Le future migliorie al Magazzino richiedono contratto economico e test dedicati.

Gli ID esatti e lo stato delle proposte sono anche in `G1_D4_TECH_TREE_PROPOSTA.json` (dato di progetto, **NON** catalogo caricato nel gioco).

## 4. Scuola: personale, formazione e saldo ricerca

**Candidati ereditati da G1-D3:** Scuola 120 Legno lavorato e 80 Pietra, accessibile con almeno una Casa piccola e un Deposito alimentare; 1 formazione simultanea per Scuola, 2 ricercatori attivi per Scuola; formazione di 120 secondi di simulazione, pagamento atomico di 10 Cibo all'ingresso; ciascun ricercatore attivo genera 0,1 punti ricerca/s. Nessuna conoscenza occupa Magazzino. Scuole addizionali moltiplicano gli slot; il numero di ricercatori qualificati può superare gli slot attivi ma non produce punti extra.

**Modello di stato proposto (aggregato, evitando ID individuali in questa release):** allievi per Scuola/somma con tempi residui individuali, `QualifiedResearcherCount`, `AssignedResearcherCount`, `ResearchPoints` decimale preciso e `UnlockedTechnologyIds`. `AssignedResearcherCount` non può superare `2×SchoolCount`; `StudentsInTraining ≤ SchoolCount`; i ricercatori occupati non sono disponibili per lavoro civile o reclutamento.

La Scuola non genera abitanti: richiede persone già esistenti. I ricercatori inattivi **restano qualificati e non lavorano**; l'eventuale comando *Ritorna lavoratore* restituisce la persona al bacino civile e perde la qualifica, con conferma e senza rimborso formazione. Questo è un candidato da confermare, perché tenere una qualifica mentre si occupano lavori comuni richiederebbe identificare persone specializzate nella nuova contabilità aggregata.

**Punti globali spendibili, non progetto automatico:** si accumulano mentre ci sono ricercatori attivi e si consumano soltanto al click *Ricerca* su una tecnologia scelta. Lo sblocco è istantaneo e permanente nella partita; si possono accumulare punti senza aver scelto una tecnologia. Nessun ordine automatico di ricerche, nessun saldo negativo, nessuno sblocco duplicato. Punti residui conservati. Ricerca, formazione e progresso congelati con pausa/uscita, su tempo simulato senza wall-clock.

### Esempi temporali nominali

| Ricercatori attivi | Punti/s | Ricerca da 100 | Ricerca da 350 |
|---:|---:|---:|---:|
| 1 | 0,1 | 16 min 40 s | 58 min 20 s |
| 2 | 0,2 | 8 min 20 s | 29 min 10 s |
| 4 (almeno 2 Scuole) | 0,4 | 4 min 10 s | 14 min 35 s |

Formazione, costruzione, crescita popolazione e consumi sono **esclusi da questi tempi**. Con una Scuola a un solo posto formazione, avere 2 ricercatori formati da zero richiede almeno 4 minuti, quindi i tempi si sommano; nel frattempo la Scuola sottrae almeno una persona alla produzione.

## 5. Strategia e completabilità

- **Percorso materie prime:** scegliere Lavorazioni I/II per ridurre il numero di Segherie e Tagliapietra comprate a costo crescente.
- **Percorso alimentare:** scegliere Alimentazione I/II per ampliare la capacità di nutrire una popolazione più numerosa e liberar posti lavorativi nel settore agricolo.
- **Percorso industria:** prima Estrazione I, poi Industria I, per rendere conveniente il rifornimento di Spade/Monete con meno edifici avanzati. Le ricerche di livello II richiedono i rispettivi I.
- **Nessuna ricerca obbligatoria oggi** per costruire edifici base, Zecca, Caserma o mantenere salvataggi v13. Quando la Navigazione diventerà reale, solo `Lavorazioni I` + `Navigazione` saranno il percorso ricerca minimo per sbloccare il Cantiere (oltre al requisito della Zecca), salvo successiva decisione.
- Non imporre di completare l'intero albero tecnologico per vincere: chiedere la Scuola tra i 23 tipi-obiettivo proposti significa **possedere la Scuola**, non sbloccare ogni ricerca. I livelli avanzati non sono obiettivi obbligatori.
- Il costo dei tier è una scelta distinta dal prezzo delle tecnologie: costo di costruzione a risorse e spesa dei punti non devono confondersi. Non introdurre bonus di produzione per lavoratore né ricompense casuali senza nuovo ADR.
- Prima di autorizzare una release giocabile, simulare il percorso da 8 abitanti e scorte iniziali della v13; verificare anche uno scenario che investe in alimentazione prima dell'industria e uno che investe in lavorazioni prima della crescita militare. L'obiettivo storico ~8h/32+h va rimisurato.

## 6. UI compatta e leggibile (senza nuova mappa)

Una scheda **Ricerca** in stile pixel art 8-bit, nascosta nei ruleset storici, mostra saldo punti, produzione/s, ricercatori formati / attivi / allievi, slot Scuola e tecnologie per settore. Ogni tecnologia visualizza costo, prerequisiti, famiglie sbloccate e stato `Non disponibile` / `Disponibile` / `Ricercata`. La scelta di un edificio nel tab Edifici visualizza costo della taglia richiesta (incl. incremento famiglia), posti occupati/massimi, produzione/consumo e azioni `Costruisci base`, `Costruisci grande`, `Costruisci avanzata`, `Migliora` quando permesse. Nessun pulsante deve muovere il layout in hover; tooltip della Capacità conserva contributi Municipio/Magazzini/edifici per output.

## 7. Persistenza e migrazione con salvaguardie

Per `civil-growth-14` (**nome proposto**): snapshot versionato con `construction_history`, `building_tiers`, `education_state`/progressi, `research_balance` e `research_unlocks`. Validazione integrale all'ingresso, prima dell'UI. È vietato creare ricercatori da un caricamento v13 o ritoccare Spade, addestramento, razioni e capienze esistenti.

Migrazione v13→v14: **tutti i produttori esistenti come Base**, `K` storico iniziale pari agli esemplari posseduti (ove non fissi); immobili non produttivi invariati; Scuola zero, tecnologie sbloccate zero, saldo punti zero. Non introdurre automaticamente la Scuola per guadagnare il nuovo obiettivo. Nessun debito retroattivo. Backup originale S1 prima della scrittura v3; transazione atomica, round-trip, refusal per ruleset sconosciuti. I salvataggi della v41 con differente schema militare continuano a passare attraverso la compatibilità esistente v13: nessuna reinvenzione del formato `military_state`.

## 8. Contratto anti-regressione e piano a fasi

**Regole matematiche:** `P(r,t,K)=ceil(B_r×M_t×1,15^K)` per ogni materiale; `M_t∈{1;2,6;6,5}`. Per una promozione `t→u`, **differenza di due prezzi arrotondati allo stesso K**, `P(r,u,K)-P(r,t,K)`; ciò garantisce che upgrade a tappe e diretto coincidano quando K non cambia. `K` cresce per **nuove costruzioni** (anche direttamente Grande/Avanzata), non per potenziamento. Acquisti +5/+10 equivalenti a 5/10 acquisti progressivi +1. Se la transazione non è fattibile, **nessuna** risorsa, popolazione, cronologia o contatore cambia.

**Gate d'implementazione:**

1. **E1 (nucleo prezzi):** modello costi/counter con test unitari su prezzi, arrotondamento, `+1/+5/+10`, overflow e blocchi; feature nuova isolata dal ruleset v13.
2. **E2 (livelli):** modello tier, upgrade con conservazione lavoratori/storage, catalogo 16 famiglie, test contesa risorse e prerequisiti; livelli non acquistabili finché la ricerca non è implementata.
3. **E3 (Scuola/ricerca):** modello formazione, saldo e tech tree, UI e salvataggi con migrazione v13→v14; solo ora esporre campagna v14 giocabile.
4. **E4 (benchmark/regressioni):** strategie di gioco non identiche, analisi anti-softlock tra capacità e prezzi, salvataggio durante formazione, completabilità e compatibilità storica. Il gate di rilascio resta chiuso finché non passa `validate.cmd` Windows e almeno un caricamento su una copia reale.
5. **G1-N / M5.4:** solo dopo E4, rivedere e implementare navigazione, flotta, esplorazione, conquista e Castello; i relativi costi navali precedenti sono da ricalcolare.

**Test specifici:** DAG senza cicli, copertura esatta delle 16 famiglie, tutti gli ID riferiti al catalogo; impossibilità di comprare tech differita; conoscenza prodotta a passi 1s vs passi grandi identica; nessun doppio conteggio delle persone; Scuola costruibile senza tech; tech acquistabile una volta e senza punti negativi; zero cambi su salvataggi v13; blocco costruzione se costo > capacità recuperabile nella campagna; capienza/consumo coerenti dopo upgrade, pause e restart.

## 9. Decisioni ancora aperte — non inferire un consenso da «continua»

- Accettare i **punti globali spendibili** con sblocco immediato al click, oppure preferire progetti con ricerca progressiva dedicata?
- Scuola come base da 1 posto formazione + 2 ricerca, ricercatori formati con 120s/10 Cibo, 0,1 punti/s: **numeri candidati**.
- La qualifica viene persa solo con comando esplicito *Ritorna lavoratore* o resta persistente anche quando la persona passa ad altri ruoli? La seconda scelta richiederebbe più modello dati.
- Confermare le **8 tecnologie iniziali** e Navigazione/Logistica riservate, con i costi qui riportati; valutare prerequisito Estrazione I per Industria I.
- Progressione 23 tipi-obiettivo, Scuola inclusa, Municipio extra, Castello sbloccato da conquista; costo crescente unico +15% o personalizzato per famiglia.

**Stato finale:** documento progettuale pronto alla discussione; non costituisce autorizzazione ad alterare `civil-growth-13`, creare tabelle SQLite v3 o introdurre l'interfaccia Ricerca.
