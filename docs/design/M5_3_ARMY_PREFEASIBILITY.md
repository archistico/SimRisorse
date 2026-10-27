# M5.3 — Caserma, reclutamento e addestramento: prefattibilità

Data: 2026-10-27, allineamento M4.14/v28. **Stato: studio preparatorio, non approvato e non implementato.** La base eseguibile attuale è **`civil-growth-5`**: il gate v24/M4.10 è riferito superato dall'utente (174 casi xUnit attesi); il prototipo 8-bit v25 e gli incrementi v26–v28 attendono la verifica Windows (177 casi attesi). Si applicano le decisioni della [scheda D0](D0_DECISION_BOARD.md) **solo dopo approvazione**. Questo dossier **non** cambia catalogo, comandi, risorse, popolazione, SQLite o regole.

## 1. Fonte e confine del requisito

Il [diagramma militare originale](../references/RESOURCE_AND_PRODUCTION_CHAINS.md) collega **Spade + abitanti → Caserma → Soldati**. Non specifica il rapporto fra persone e spade, il prezzo della Caserma, i tempi di addestramento, il mantenimento, gli alloggi, le perdite o le condizioni di congedo. Caserma, sede iniziale, torre e castello sono disegnati, ma **non esiste un ordine obbligatorio documentato** tra questi ultimi edifici.

La visione generale prevede un bacino comune di popolazione e un esercito impiegabile nelle isole, con **tempo di gioco**, salvataggi, interfaccia a schede e stile finale pixel art 8-bit. M5.3 deve fermarsi alla formazione di soldati disponibili; navi e spedizioni appartengono a **M5.4–M5.5**. Anche Spade e Monete dipendono dalle decisioni **D0.8** e dalle filiere M5.1–M5.2, tuttora proposte.

## 2. Invariante fondamentale: nessuna persona duplicata

Il modello candidato distingue quattro stati mutuamente esclusivi della popolazione:

`Popolazione totale = Civili impiegati + Civili liberi + Reclute in addestramento + Soldati disponibili (incluse le truppe impegnate, se ancora residenti)`.

Per esempio, con **220 persone** e **15 civili impiegati**, si potrebbero avere **180 civili liberi, 10 reclute e 15 soldati**: `15 + 180 + 10 + 15 = 220`. È una **prova contabile**, non lo stato di una partita né un valore di bilanciamento approvato. La popolazione di M4 non distingue ancora reclute o soldati: lo scenario attuale non deve essere reinterpretato retroattivamente.

| Transizione candidata | Civili liberi | Reclute | Soldati | Popolazione totale |
| --- | ---: | ---: | ---: | ---: |
| Arruola una persona disponibile | −1 | +1 | 0 | Invariata |
| Addestramento completato | 0 | −1 | +1 | Invariata |
| Annulla reclutamento (se consentito) | +1 | −1 | 0 | Invariata |
| Congedo (se consentito) | +1 | 0 | −1 | Invariata |
| Assegna un civile libero a una produzione | −1 | 0 | 0 | Invariata; +1 impiegato |

**Caso bloccante:** le perdite permanenti in missione riducono la popolazione totale e interagiscono con i posti abitativi, la razione Cibo/Pane, la crescita e il possibile recupero dei posti di lavoro. M5.3 non può introdurre decessi o consumo militare di cibo per deduzione. Occorre una scelta D0.4 e una regola versionata, poi testarla separatamente.

## 3. Tre modelli di reclutamento da confrontare

| Modello | Comportamento | Vantaggio | Rischio |
| --- | --- | --- | --- |
| **A. Immediato** | Civili liberi e dotazione vengono trasformati subito in soldati | Minima complessità e pochi nuovi stati | Caserma poco più che un pulsante; tempo di gioco scarsamente rilevante |
| **B. Addestramento a coda** | Arruolamento prenota persone/materiali; la Caserma forma soldati dopo tempo attivo | Scelte di investimento e tempo visibili | Cancellazioni e ricariche richiedono un contratto preciso |
| **C. Addestramento per posti di Caserma** | Come B, ma il numero di reclute in formazione simultanea dipende dalle Caserme | Scala produttiva significativa e specializzazione | Stato più articolato, rischio di code bloccate e gestione complessa |

**Preferenza tecnica da sottoporre all'utente: B come primo incremento M5.3**, lasciando C come ampliamento opzionale se la scala delle partite lo giustificherà. Non è una decisione adottata. Numeri come *1 spada per soldato*, *60 secondi di addestramento* o *una recluta per caserma* **non sono fissati** dal riferimento e non entrano nel codice senza una scelta esplicita.

### Sottofasi realizzabili dopo il gate D0

| Sottofase | Confine | Gate |
| --- | --- | --- |
| **M5.3a** | Caserma e comando atomico di arruolamento, con popolazione condivisa | Persone e Spade scalate una sola volta; nessuna duplicazione |
| **M5.3b** | Addestramento sul tempo simulato, code e blocchi leggibili | Passi grandi/piccoli, pausa, ripresa e salvataggio equivalenti |
| **M5.3c** | Bilanciamento di contingenti piccoli/grandi e integrazione preparatoria con le missioni | Impatto misurato sul lavoro civile e sulle razioni; nessuna spedizione implementata in anticipo |

Le sottofasi sono una proposta di implementazione, non meccaniche autorizzate. Anche la quantità delle Spade e il loro destino (consumo al reclutamento o equipaggiamento persistente) restano a D0.8.

## 4. Decisioni da chiudere prima di scrivere la prima classe militare

| ID | Domanda precisa | Perché blocca l'implementazione |
| --- | --- | --- |
| **D0.4a** | Un soldato continua a occupare un posto abitativo civile? | Determina capienza e regole di crescita |
| **D0.4b** | Soldati/reclute consumano Cibo e Pane come gli altri residenti, oppure una razione distinta? | Cambia il bilancio delle filiere e il modello di popolazione avanzata |
| **D0.4c** | Reclutamento immediato, coda temporizzata o posti simultanei di Caserma? | Definisce stato persistente e progressione del tempo |
| **D0.4d** | Si possono annullare l'addestramento e congedare soldati? Che cosa viene restituito? | Serve rollback economico e contabile atomico |
| **D0.4e** | Le missioni possono causare perdite permanenti? | Richiede regole di diminuzione della popolazione e recupero |
| **D0.8a** | Le Spade si consumano all'arruolamento oppure restano associate ai soldati? | Modello di risorsa contro inventario/equipaggiamento |
| **D0.4f** | Qual è il ruolo della Caserma: sblocco, capacità di formazione o entrambe? | Costo, prerequisiti, utile strategico e catalogo |

Finché non sono risolte, **non è corretto scegliere un costo o una durata “plausibile” e codificarli**. Le risposte possono essere accompagnate da due benchmark alternativi, ma restano scelte dell'utente.

## 5. Contratto tecnico da rispettare in una futura implementazione

- **Core:** comandi di arruolamento espliciti, controlli di popolazione disponibile, eventuali Spade e capacità; tutte le modifiche applicate insieme o nessuna. Non trasformare un soldato in un lavoratore assegnato senza prima congedarlo.
- **Tempo:** addestramento avanzato solo da secondi di gioco validi; nessun progresso fuori dal gioco o in pausa. Lo stesso intervallo totale, suddiviso diversamente, produce lo stesso esito. Trattare con attenzione completamenti esatti e frazioni di secondo.
- **Application:** stato di lettura tipizzato con persone libere, impiegate, reclute, soldati, tempo residuo e motivazione di eventuale comando disabilitato. Niente formule duplicate nella GUI.
- **Persistenza:** nuova versione delle regole e migrazione dichiarata dagli snapshot civili; caricare una vecchia partita con zero reclute/soldati solo se questa politica è stata approvata. Conservare tempi parziali senza duplicare i completamenti.
- **GUI:** una scheda Caserma 8-bit con conteggi, costo per ordine, numero di reclute, avanzamento e messaggio di blocco immediatamente leggibile. Nessun posizionamento su mappa; dimensione del pulsante stabile fra normale, hover, premuto e disabilitato.
- **Missioni successive:** distinzione futura tra soldati disponibili e impegnati in una spedizione; impedire l'impiego simultaneo in più missioni. M5.3 non simula ancora combattimento, navi o isole.

## 6. Matrice di verifica proposta

| Prova futura | Risultato atteso |
| --- | --- |
| Arruolamento di N civili liberi, materiali sufficienti | Esattamente N reclute; totale persone invariato |
| N maggiore dei liberi, Spade mancanti o Caserma assente | Rifiuto senza mutazioni parziali |
| Lavoratori già assegnati | Non vengono reclutati implicitamente |
| Fine addestramento su confine di secondo | Una sola transizione recluta → soldato |
| `Advance(600s)` contro 600 × `Advance(1s)` | Stato finale identico |
| Pausa, chiusura e riapertura nel mezzo di un addestramento | Nessun avanzamento offline, tempo residuo preservato |
| Salvataggio/ricarica subito prima del completamento | Nessun doppio soldato né spesa duplicata |
| Congedo/annullamento, se approvati | Popolazione e dotazione riconciliate |
| Scenario civile M4 caricato nella nuova versione | Migrazione esplicita, backup e bilancio invariato |
| 1040×740 e 1240×850, tastiera e mouse | Conteggi e blocchi visibili; nessun salto di layout sui comandi |

**Benchmark futuro:** confrontare più ordini piccoli contro un ordine grande e la scelta di reclutare presto o tardi, registrando popolazione disponibile, produzione civile persa, Pane/Cibo, Spade impiegate e durata in tempo simulato. Non attribuire un tempo-obiettivo all'addestramento prima delle decisioni D0.4.

## 7. Conclusione e collegamento con la roadmap

Il passaggio più delicato non è la Caserma come edificio, ma la **conservazione delle persone** mentre cambiano ruolo. Questo dossier definisce l'invariante e le prove necessarie; lascia deliberatamente aperti prezzi, durate, razioni, perdite, armi e capienza. Il suo uso corretto è sostenere la discussione **D0.4/D0.8** dopo il gate tecnico M4, poi implementare M5.3 soltanto dopo M5.1–M5.2 approvate e validate. Le missioni appartengono a **M5.5** e le isole a **M5.6**.
