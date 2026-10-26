# Analisi delle risorse e delle filiere

Analisi delle cinque nuove immagini il 2026-10-26. Sono evidenze del gioco di riferimento: non cambiano automaticamente il catalogo o il bilanciamento di SimRisorse. Nessun codice di simulazione è stato modificato per questa analisi.

## Schermate delle risorse

Fonti: [vista completa](screenshots/risorse1.png) e [dettaglio](screenshots/risorse2.png). La seconda riprende i valori delle prime quattro righe della prima, con maggiore leggibilità.

| Etichetta | Significato | Variazione visualizzata | Quantità | Capacità visualizzata |
| --- | --- | --- | --- | --- |
| Siedler | Abitanti | +0.001 | 10 | 110 |
| Baumstämme | Tronchi | ±0 | 100 | 10K |
| Bretter | Tavole / legno lavorato | +40 | 9.1K | 10K |
| Steine | Pietra | +10 | 2.2K | 10K |
| Nahrung | Cibo | +5 | 960 | 10K |
| Fleisch | Carne | ±0 | 12 | 10K |
| Fische | Pesce | +5 | 1.1K | 10K |

Le immagini mostrano una variazione numerica distinta dalla quantità e dal limite. Non mostrano l'unità temporale della variazione: non si può attribuire automaticamente il valore a un secondo, un minuto o un ciclo. Anche l'interpretazione come saldo netto richiede conferma, benché sia coerente con il tipo di schermata.

I valori positivi sono verdi; il saldo nullo e la scorta delle tavole sono gialli. Per le tavole è visibile 9.1K su 10K: il colore può segnalare vicinanza al limite, ma soglia e significato non sono spiegati. Il `19%` della scheda edifici non è definito dalle immagini.

`Siedler 10/110` non distingue persone libere, assegnate o totali. Il tasso frazionario di crescita non dimostra che siano assegnabili frazioni di lavoratore. Il modello del nostro gioco mantiene persone intere; la crescita resta da progettare.

## Materiali e popolazione

Fonte: [diagramma delle filiere civili](screenshots/struttura1_timeofexploration.png).

I collegamenti delle icone mostrano taglialegna → tronchi → segheria → tavole, e tagliapietra → pietra. Nella parte inglese le etichette `Wood` e `Trees` sono incoerenti con alcune icone: per SimRisorse manteniamo la distinzione già chiara tra `logs` (tronchi) e `planks` (tavole), confermata anche dai termini tedeschi della schermata risorse.

Sono presenti tre dimensioni di deposito e tre di abitazione. Le case portano agli abitanti; il cibo è collegato alle case medie e grandi. Non è disegnato lo stesso ingresso per la casa piccola: questo non basta a stabilire tutte le sue eventuali spese o il suo mantenimento.

La scheda Big House già analizzata separa abitanti iniziali, capacità abitativa e crescita. Il nuovo diagramma aggiunge la relazione tra disponibilità alimentare e sviluppo della popolazione, senza fornire formule.

## Alimentazione

Nel diagramma civile sono espliciti questi percorsi:

| Percorso | Collegamento mostrato |
| --- | --- |
| Agricoltura | Fattoria → `Corn` (cereali) |
| Farina | Cereali → mulino → farina |
| Pane | Farina + acqua → panificio → pane |
| Acqua | Pozzo → acqua |
| Pesca | Pescatore → pesce |
| Caccia | Cacciatore → carne |
| Allevamento | Cereali + acqua → allevamento di maiali → carne |
| Cibo | Pane, pesce e carne sono collegati ai depositi alimentari, che portano alla voce `Food` |

`Corn` è il nome del nodo del riferimento: la denominazione esatta da adottare, per esempio cereali o mais, resta una scelta di contenuto.

La funzione dei depositi alimentari richiede attenzione. Il diagramma li inserisce nel percorso verso il cibo, ma non precisa se trasformino, prelevino o aggreghino le scorte, né se pane, pesce e carne siano ingredienti contemporanei o alternative. Non trattarli automaticamente come semplici aumenti di capacità, e non inventare una ricetta che consumi obbligatoriamente tutti e tre.

## Monete, armi e soldati

Fonte: [diagramma economico e militare](screenshots/struttura2_timeofexploration.png).

| Attività | Input mostrati | Output mostrato |
| --- | --- | --- |
| Miniera d'oro | Nessuno specificato dal diagramma | Minerale d'oro |
| Miniera di carbone | Nessuno specificato dal diagramma | Carbone |
| Fusione dell'oro | Minerale d'oro + carbone | Oro |
| Zecca | Oro + carbone | Monete d'oro |
| Miniera di ferro | Nessuno specificato dal diagramma | Minerale di ferro |
| Fusione del ferro | Minerale di ferro + carbone | Ferro |
| Forgia | Ferro + carbone | Spade |
| Caserma | Spade + abitanti | Soldati |
| Cantiere navale | Legno, con icona di tavole, + monete | Nave |

"Nessuno specificato" non significa produzione senza lavoratori o costi: il disegno omette molti dettagli. Il carbone alimenta più lavorazioni, compresa la zecca, ed è quindi un candidato importante per le future prove di contesa tra filiere.

Il collegamento abitanti → caserma → soldati sostiene la proposta di conversione della popolazione in forza militare. Non ne stabilisce rapporti, durata, costi alimentari, recuperabilità o perdite.

Sede iniziale, torre di guardia e castello sono raggruppati come strutture militari. Non sono mostrate frecce tra loro: il disegno non prova una sequenza obbligatoria sede → torre → castello o specifici bonus.

## Esplorazione, colonie e commercio

Il diagramma distingue spedizione, territori stranieri, crociata e colonie. Le frecce descrivono il percorso spedizione → territori scoperti → crociata → colonie.

Dalle colonie partono tre rami:

- piantagione di banane → banane → nave dedicata → monete;
- piantagione di caffè → caffè → nave dedicata → monete;
- piantagione di cotone → cotone → nave dedicata → monete.

Le monete compaiono quindi come risultato sia della zecca sia delle filiere coloniali. Questa è un'informazione nuova rispetto alle sole schermate degli edifici.

Il disegno non precisa se le navi dedicate siano mezzi fisici, rotte o servizi, né se le monete arrivino una volta o periodicamente. Non mostra tutti gli input necessari a spedizioni e crociate: non si deducono automaticamente quantità di navi, soldati, perdite o durate.

## Strutture decorative e progressione avanzata

Fonte: [diagramma delle strutture decorative](screenshots/struttura3_timeofexploration.png). Sono rappresentate tre catene distinte:

| Ramo | Prima struttura | Seconda struttura | Terza struttura |
| --- | --- | --- | --- |
| Monumenti | Arc de Triomphe | Brandenburg Gate | Liberty Statue |
| Torri e architettura | Big Ben | Eiffel Tower | Fernsehturm Berlin |
| Luoghi naturali | Ayers Rock | Niagara Falls | Grand Canyon |

Questi nove elementi possono ispirare traguardi avanzati, ma il diagramma non specifica prezzi, effetti, condizione di vittoria o eventuale assenza di benefici produttivi. L'etichetta "decorative" non basta a decidere le loro regole per SimRisorse.

## Ampiezza del riferimento

Contando una volta sola la miniera di carbone condivisa tra filiere, nei tre diagrammi si distinguono 43 tipi strutturali:

| Gruppo | Tipi |
| --- | --- |
| Materiali | Taglialegna, segheria, tagliapietra |
| Depositi e case | Tre depositi generici e tre abitazioni |
| Alimentazione | Fattoria, mulino, panificio, pozzo, pescatore, cacciatore, allevamento; tre depositi alimentari |
| Metalli e moneta | Miniere di oro, carbone e ferro; fusioni di oro e ferro; zecca |
| Esercito e navigazione | Forgia, caserma, sede iniziale, torre, castello, cantiere |
| Colonie | Tre piantagioni |
| Decorazioni | Nove elementi dei tre rami |

Spedizioni, crociate, territori, soldati e navi non sono inclusi in questo conteggio strutturale. Il conteggio descrive il riferimento, non un catalogo già approvato o implementato.

## Conseguenze per SimRisorse

- M3: mostrare quantità, capacità e flussi con unità temporali esplicite, distinguendo scorte da personale. `ResourceView` M2 contiene quantità e capacità; i flussi vanno ancora aggiunti alle proiezioni definitive.
- M4: definire le filiere alimentari, il ruolo dei depositi, crescita e mantenimento; il Core attuale ha popolazione e capacità fisse.
- M5: distinguere territori scoperti e posseduti, addestramento e missioni; precisare il modello di trasporto e commercio.
- M6: decidere se e come le tre catene decorative partecipino all'obiettivo di possedere tutti gli edifici.

Il catalogo M2 supporta già input e output multipli e contesa deterministica. Non supporta ancora automaticamente effetti di capacità, crescita, rotte o comportamenti alternativi dei depositi alimentari. Questa analisi non amplia le meccaniche implementate e non assegna costi o tempi non visibili nelle immagini.
