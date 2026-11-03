# SimRisorse — G1-D2: economia progressiva, livelli degli edifici e ricerca (PROPOSTA)

**03/11/2026 — solo progettazione.** Baseline v59-RC1, `civil-growth-13`, `validate.cmd` superato sul PC dell’utente. Questo documento non approva né implementa regole; modifica le priorità della roadmap proposta. I documenti G1-D1 su flotta e spedizioni rimangono proposte non approvate. `civil-growth-14` non è ancora assegnato a nessuna release eseguibile.

## Motivo del cambiamento

La campagna attuale può evolvere troppo rapidamente perché i costi per un medesimo tipo di edificio sono costanti e tutte le unità produttive sono di dimensione unica. Si propone un secondo asse di progressione: (1) costo crescente per nuovi esemplari, (2) taglie base/grande/avanzata, (3) ricerca e ricercatori che aprono le taglie e le tecnologie. Riferimento concettuale GridPower: crescita del costo per upgrade e sblocchi tramite ricerca; non importare le sue cifre o sistemi in SimRisorse.

## Baseline reale, verificata staticamente nel codice v59

- `BuildingDefinition`: `Cost`, `WorkplacesPerBuilding`, `Recipe`, `Prerequisites`, `HousingPerBuilding`, `StoragePerBuilding` sono definiti per tipo, senza tier dinamico.
- `BuildingState`: `BuildingId`, `Count`, `AssignedWorkers`, aggregati per tipo.
- `GameSession.Construct` usa `definition.Cost.Scale(count)`; la formula è quindi lineare e non distingue gli acquisti successivi.
- `ProductionPlanner` applica la ricetta per ciascun lavoratore effettivamente assegnato. Servono input e capacità sufficienti.
- I dati militari v13: Caserma 260 Legno lavorato + 200 Pietra, recluta 5 Spade, un posto e 30 s per addestramento. Non cambiare senza un'ulteriore decisione.
- 20 tipi-obiettivo implementati, Cantiere e Castello ancora futuri, più Municipio iniziale fuori dai 22 obiettivi; 16 risorse. Non è presente un motore di ricerca.

## A. Costo crescente (da approvare)

Preferenza: escalation **per famiglia** di edificio; una Segheria aggiuntiva non influenza il costo del primo Pozzo. Evitare escalation globale comune a tipi differenti. Formula candidata a confronto:

`nextCost(family) = ceilByResource(baseCost(tier) * pow(growthFactor(family), familyPurchaseIndex))`.

- Confrontare coefficienti 1.10, 1.15, 1.20; nessuno dei tre è approvato.
- Il `familyPurchaseIndex` deve essere storico persistente e monotono anche con demolizione e conversioni; edifici iniziali vanno trattati esplicitamente.
- Acquisti multipli devono calcolare la **somma progressiva** dei singoli costi (non `count * costoUltimo`) e operare atomicamente, senza spese parziali.
- Soglie, cap di quantità, overflow, arrotondamento delle risorse e capacità storage devono essere verificati.
- I livelli nella stessa famiglia condividono il contatore di acquisti; upgrade non deve generare vantaggi elusivi. La politica precisa sul conteggio upgrade è ancora aperta.
- Nei salvataggi migrati da v13 occorre definire se il contatore parta da `edificiPosseduti` o debba compensare i costi storici; non imporre ricostruzione retroattiva dei costi.

## B. Taglie per edifici produttivi (da approvare)

Esempio Segheria: base 1 posto, grande 3 posti, avanzata 10 posti. Con ricetta v13 per lavoratore di 2 Tronchi -> 1 Legno lavorato/s, capacità teoriche: 1/3/10 di output e 2/6/20 input/s, solo con lavoratori/risorse/magazzini sufficienti.

- Distinguere **tipo/famiglia** (Segheria) da **taglia** (base/grande/avanzata). I tre livelli non devono gonfiare automaticamente il numero dei tipi richiesti per la vittoria.
- Approfondire gli upgrade sul posto, con conservazione dei lavoratori occupati e senza perdita/duplicazione di merci e capacità.
- Prezzi e capacità non automaticamente x3/x10: gli edifici maggiori devono avere economie di scala ragionevoli ma richiedere risorse tecnologiche / investimenti.
- Non tutti i 22 tipi richiedono necessariamente tre taglie: lo studio parte dalle filiere produttive; case, magazzini, caserma, scuola, Cantiere vanno considerati singolarmente.
- Modello dati: preservare aggregazione per family+tier quando possibile, identificare con stabilità ogni edificio se necessario per un futuro upgrade individuale. La scelta impatta profondamente salvataggi e UI.

## C. Scuola e ricerca (da approvare)

- La Scuola deve formare **abitanti esistenti** come ricercatori, non creare persone ex novo. Durante la formazione la persona non è libera né disponibile per altre assegnazioni.
- I ricercatori attivi, assegnati a posti di ricerca, generano un valore di **conoscenza/punti ricerca** nel tempo simulato. I punti ricerca sono un progresso persistente dedicato, non una normale merce nel Magazzino, salvo decisione diversa.
- Sblocchi permanenti per partita e di gruppo, ad esempio «Lavorazioni grandi», «Meccanizzazione», «Logistica», «Navigazione». Richiedono punti ricerca e prerequisiti; costi/durate da bilanciare.
- Assicurare che non si possa usare un abitante simultaneamente come lavoratore, recluta, soldato o ricercatore. Razioni, alloggi, crescita e pausa continuano a rispettare il motore v13.
- La Scuola deve essere raggiungibile dalla colonia iniziale senza richiedere una tecnologia che essa stessa deve sbloccare. Verificare catalogo e ciclo delle dipendenze.
- Scelta ancora aperta: una sola struttura Scuola che forma e impiega ricercatori, oppure Scuola formativa + edificio di ricerca separato (sconsigliato per il primo incremento per complessità).

## D. Vittoria e numero di edifici (da decidere esplicitamente)

Il contratto attuale richiede **22 tipi-obiettivo**, con Municipio extra. Se la Scuola è costruibile e obbligatoria, proposta coerente: portare gli obiettivi a **23 tipi** (i 22 esistenti + Scuola), Municipio sempre fuori conteggio. Le tre taglie della Segheria contano come un solo tipo. Non modificare tacitamente il numero 22; l'utente deve approvarlo.

## E. Impatto sui salvataggi e versioni

- `civil-growth-13` e vecchi benchmark restano immutati. La prossima versione economica potrebbe essere `civil-growth-14`; il Cantiere successivo passerebbe a `civil-growth-15` salvo nuova decisione.
- Versionare contratto di catalogo, contatori storici di acquisto, edifici per tier, progressi ricerca, ricercatori e sblocchi.
- Migrazione su copie con backup S1; ripristino esatto di risorse, edifici, personale, pop, progressi militari e deposito. Le tecnologie già implicite in edifici esistenti v13 richiedono una regola di compatibilità esplicita.
- Non incrementare il ruleset e non attribuire sblocchi ai vecchi save finché il contratto non è approvato.

## F. Nuova roadmap proposta (senza anticipare implementazione)

1. **G1-E1** Approvare formula costo, perimetro famiglie, contatori e acquisto multiplo.
2. **G1-E2** Approvare livelli 1/3/10, upgrade, costi e capacità per famiglia.
3. **G1-E3** Approvare scuola, modello ricercatori, conoscenza, tech tree e obiettivi vittoria.
4. **M5.4-E1** Implementare i costi progressivi, prezzo prossimo acquisto, atomicità e test.
5. **M5.4-E2** Implementare livelli/taglie e upgrade, UI e benchmark di filiera.
6. **M5.4-E3** Implementare ricerca, Scuola, ruoli popolazione, sblocchi, SQLite, UI e test.
7. **M5.4-E4** Bilanciare campagna reale, inclusa raggiungibilità militare, obiettivo temporale e costo opportunità.
8. **G1-N** Rifissare costi e prerequisiti di Cantiere, nave e conquista sulla nuova economia; riprendere le milestone navali M5.5+ con rinumerazione da confermare.

## G. Test indispensabili

1. Costo N-esimo acquisto, rounding e overflow; batch equivalente alla somma di acquisti singoli, operazione atomica.
2. Persistenza del contatore nel salvataggio, anche quando un edificio viene convertito/upgradato; anti-exploit demolizione.
3. Produzione base/grande/avanzata in funzione degli assegnati 0..1/3/10 e delle materie prime disponibili.
4. Aggiornamento della capacità deposito e nessuna perdita silenziosa dopo upgrade/caricamento.
5. Ricercatori conservano popolazione: `abitanti = liberi + lavoratori + reclute + soldati + ricercatori + tirocinanti` (categorie esclusive da definire nel modello).
6. Ricerca avanza solo con tempo simulato, non in pausa o a programma chiuso; unlock persistenti e non duplicati.
7. Salvataggi v13 riproducibili senza ricarico illegale dei costi; vecchi benchmark congelati.
8. Simulatore di strategia efficiente e lenta per completabilità e durata di campagna: gli obiettivi ~8/32+ ore vanno riesaminati, non considerati automaticamente validi dopo la riforma.

## Stato

**PROPOSTA, NON APPROVATA.** Nessun file C#, test o script di gioco viene modificato in G1-D2. I valori 1/3/10 derivano dall'idea dell'utente; tutti gli altri coefficienti, livelli tecnologici e percorsi sono esempi o raccomandazioni per la discussione.
