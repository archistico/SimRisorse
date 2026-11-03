# SimRisorse — G1-D3: contratto quantitativo economia, livelli e ricerca (PROPOSTA)

Data: 3 novembre 2026. Base eseguibile: **v59-RC1 / `civil-growth-13`**, `validate.cmd` superato sul Windows dell'utente. **Questo file NON approva le nuove regole** e non modifica il motore. Integra G1-D2 con formule implementabili, esempi verificati, modello di stato, principi di migrazione, vincoli, rischi e criteri di test.

## 0. Baseline effettiva v59

- In `GameSession.Construct` il costo attuale è lineare: `definition.Cost.Scale(count)`. Le costruzioni non hanno una cronologia degli acquisti.
- `BuildingDefinition` specifica costo, posti lavoro, ricetta, prerequisiti, alloggi e stoccaggio; `BuildingState` aggrega conteggio e lavoratori per `BuildingId`.
- `ProductionPlanner` usa una ricetta per lavoratore assegnato, limitata da input e capacità condivisa. Non produce automaticamente più risorse per un edificio più grande.
- Segheria: 40 Legno lavorato + 20 Pietra, 1 posto, 2 Tronchi/s -> 1 Legno lavorato/s e +50 capienza per Legno lavorato. La colonia iniziale contiene già **1 Segheria**, 1 Taglialegna, 1 Tagliapietra e 8 abitanti.
- Municipio unico: +1.000 capienza per ogni risorsa; Magazzino +500 per ciascuna; strutture produttive +50 per risorsa prodotta. 16 risorse, nessun punto ricerca come merce.
- La prima campagna contempla 22 tipi-obiettivo (20 implementati + Cantiere + Castello) e Municipio extra; la Scuola NON è inclusa nei 22.
- Il ruleset attuale è `civil-growth-13` e i salvataggi civili usano `GameSnapshot` versione 2 / SQLite `user_version=2`, con stato militare individuale.

## 1. Aumento prezzi per famiglia — proposta C-01…C-10

Preferenza espressa: escalation **per famiglia**, non un coefficiente globale. `family` è il tipo funzionale (es. `sawmill`), indipendentemente dal livello. Per ogni famiglia `f`, mantenere un contatore persistente monotono `K_f` di edifici **acquisiti**, includendo gli esemplari iniziali e senza decrementare dopo upgrade o futura demolizione.

**Formula del costo di una nuova costruzione** (per ogni risorsa del prezzo):

`P(f,t,r,K) = ceil_unita( B(f,r) × M(t) × G(f)^K )`

- `B(f,r)` = costo base della famiglia per la risorsa r nel catalogo; zero rimane zero.
- `M(t)` = moltiplicatore di costo della taglia, candidato base=1, grande=2,6, avanzata=6,5.
- `G(f)` = fattore di crescita, candidato comune iniziale **1,15**; confronti 1,10 e 1,20 (NON approvati).
- `K` = numero di edifici già acquisiti in quella famiglia, **prima** del prossimo acquisto; l'upgrading non aumenta `K`.
- `ceil_unita(x)` = arrotondamento per eccesso all'unità intera di ciascuna risorsa; evitare `double`, usare aritmetica decimale controllata, limiti di rappresentazione e di calcolo per evitare overflow. Il motore salva le risorse a millesimi, ma i **prezzi** qui sono interi.
- Per ordini da N: sommare **ogni** prezzo `P(K+i)` per `i=0,…,N−1` prima di scalare risorse o conteggi. Ordine e pagamento atomici, nessuna modifica parziale. Una transazione di acquisto fallita non incrementa contatori.
- Municipio fisso: escluso dalla spesa/contatori. Famiglie non produttive (case, magazzini, Caserma, Scuola) usano crescita per famiglia ma **non** necessariamente taglie 1/3/10.

### Sensibilità (Segheria *base*, 40 Legno lavorato + 20 Pietra)

| Numero storico dell'edificio | +10% | +15% | +20% |
|---|---:|---:|---:|
| 1º (`K=0`) | 40 / 20 | 40 / 20 | 40 / 20 |
| 2º (`K=1`) | 44 / 22 | 46 / 23 | 48 / 24 |
| 3º (`K=2`) | 49 / 25 | 53 / 27 | 58 / 29 |
| 4º (`K=3`) | 54 / 27 | 61 / 31 | 70 / 35 |
| 5º (`K=4`) | 59 / 30 | 70 / 35 | 83 / 42 |
| 6º (`K=5`) | 65 / 33 | 81 / 41 | 100 / 50 |
| 10º (`K=9`) | 95 / 48 | 141 / 71 | 207 / 104 |
| 20º (`K=19`) | 245 / 123 | 570 / 285 | 1278 / 639 |

**Raccomandazione provvisoria**: +15% per famiglia, non per tier e non globalmente. È una scelta da simulare con la crescita popolazione/magazzini, non confermata solo dal prezzo del 20º edificio. Per la Segheria il `K` della nuova partita parte da 1, perché una è già presente: il primo acquisto aggiuntivo è il *secondo*, 46/23 nella variante +15%. Non retroaddebitare gli edifici iniziali.

### Condizioni di sicurezza

La progressione deve risultare monotona; controllare valori massimi e tempi di calcolo, in particolare acquisti in massa (1/5/10) e prezzi superiori alla capacità massima delle risorse: evitare deadlock irrisolvibili. La formula deve essere deterministicamente identica dopo pausa/salvataggio/ripristino. UI: mostrare `Costo prossimo +1`, `Costo totale +5/+10`, progressivo famiglia e motivo di blocco. I vecchi pulsanti quantità non possono usare `count × costo della prossima unità`.

## 2. Tre taglie e potenziamenti — proposta L-01…L-09

**Portata del primo incremento:** soli edifici produttivi con ricetta. Per una Segheria:

| Taglia | Posti lavoratore | Consumo Tronchi/s al massimo | Produzione Legno lavorato/s al massimo | Deposito locale prodotto | Costo base prima di crescita |
|---|---:|---:|---:|---:|---|
| Base | 1 | 2 | 1 | +50 | 40 Legno lavorato / 20 Pietra |
| Grande | 3 | 6 | 3 | +150 | 104 Legno lavorato / 52 Pietra |
| Avanzata | 10 | 20 | 10 | +500 | 260 Legno lavorato / 130 Pietra |

Costi dei tier sono **esempi** con moltiplicatori 1, 2,6, 6,5 e stessi materiali del livello base, da bilanciare. A parità di prezzo di riferimento, il costo per posto di lavoro scende passando da base a grande e avanzata; tecnologia e filiera di input devono però essere disponibili. I lavoratori restano assegnati manualmente (nessun trasferimento implicito di abitanti liberi) e la produzione richiede materie prime e stoccaggio per ogni lavoratore effettivo. Si può avere avanzata con 10 posti e 4 addetti: produce come 4 addetti, non come 10.

**Costruzione diretta**: `K_f` aumenta di 1, qualunque sia il tier selezionato. La ricerca deve già avere sbloccato il livello richiesto; il livello base è sempre costruibile sotto i prerequisiti correnti.

**Upgrade sul posto**: sostituisce una unità di livello precedente, senza incrementare `K_f` perché non aggiunge un edificio fisico. Pagare l'**incremento di valore calcolato al contatore corrente**:

`U(f,da,a,r,K) = P(f,a,r,K) - P(f,da,r,K)`

Esempio Segheria, `K=1`, +15%: una nuova base costerebbe 46/23; una nuova grande 120/60; il passaggio **base→grande costa 74 Legno lavorato + 37 Pietra**. Il passaggio grande→avanzata: nuova avanzata 299/150, quindi **179 + 90**. Il valore del vecchio edificio è *credito teorico*, non materiale rimborsato; niente accrediti di risorse. Un upgrade diretto base→avanzata è ammissibile se tutti i prerequisiti sono sbloccati: delta 253/127 al medesimo `K`.

Questa politica significa che migliorare oggi un edificio esistente costa meno che costruire oggi un tier nuovo, ma non garantisce che la spesa totale sostenuta in anni diversi sia uguale a un acquisto diretto: questo è intenzionale e va spiegato. L'upgrade non cambia la produzione per lavoratore né ricetta, salvo esplicita tecnologia futura. Nessun downgrade iniziale. Magazzini e case restano con le taglie/categorie esistenti finché non progettate separatamente. Caserma non passa automaticamente a 3/10 posti: i suoi slot sono governati dalle regole militari v13.

**Aggregazione raccomandata:** `BuildingTierState(family,tier,count,assignedWorkers)` mantenendo aggregazione per famiglia e livello, senza identità individuali in questa prima fase. Per upgrade di una unità, spostare fino a capienza della vecchia unità (minimo necessario a non lasciare assegnazioni eccedenti) nella nuova classe; nessuna persona creata, rilasciata o contata due volte. Il conteggio aggregato non impone un ID univoco di edificio. Contare il possesso di una famiglia quando *qualunque* suo tier ha `count>0`; i prerequisiti storici devono usare la famiglia, non solo il livello base.

**Storage condiviso:** con l'upgrade la capacità locale passa da +50 a +150/+500, non si somma il vecchio bonus al nuovo. Le risorse già immagazzinate restano nel magazzino comune; niente distruzione né trasferimento fisico. Le operazioni che riducono la capacità (futura demolizione) devono respingersi se lo stock non entra più, salvo una politica esplicita futura.

## 3. Scuola, formazione e ricerca — proposta R-01…R-10

**Scuola**: nuovo edificio non produttivo di risorse materiali. Costo candidato: **120 Legno lavorato + 80 Pietra**, prerequisiti: almeno una Casa piccola e un Deposito alimentare. Prezzo per la seconda Scuola soggetto al normale coefficiente famiglia. Lo sblocco della scuola deve essere raggiungibile senza ricerca; non dipende da Zecca o Monete. La Scuola ha **un posto di formazione** e **due posti di ricerca**; scuole aggiuntive moltiplicano in modo prevedibile i posti disponibili.

**Formazione**: un abitante libero diventa allievo ricercatore; tempo proposto **120 secondi simulati** per persona, costo candidato **10 Cibo** all'avvio, consumato atomicamente. Una persona in formazione non è assegnabile come lavoratore né come recluta. Completata la formazione diventa ricercatore qualificato, ma non produce conoscenza senza posto e assegnazione alla Scuola. Un ricercatore in attesa può essere riassegnato al bacino civile **perdendo la qualifica**, in modo da non causare blocchi irreversibili della popolazione; questa politica è ancora da approvare.

**Produzione ricerca**: ogni ricercatore attivo genera **0,1 punti ricerca per secondo simulato** (1 punto ogni 10 secondi), senza consumare una nuova risorsa e senza usare il Magazzino. La popolazione continua a consumare Cibo e, dove applicabile, Pane secondo le regole di alloggio esistenti. La conoscenza è un saldo persistente distinto dalle 16 risorse; scelta candidata: punti globali spendibili, nessun limite artificiale inizialmente. Ricerche sbloccate permanenti per partita; spendere punti e sbloccare è un'operazione atomica. La pausa e l'uscita dal gioco bloccano formazione e ricerca, come il resto della simulazione.

### Albero tecnologico iniziale suggerito

| Tecnologia | Costo candidato (punti) | Prerequisito | Effetto |
|---|---:|---|---|
| Lavorazioni I | 100 | Scuola | Grandi nelle filiere base (Taglialegna/Segheria/Tagliapietra) |
| Lavorazioni II | 350 | Lavorazioni I | Avanzate nelle filiere base |
| Alimentazione I | 100 | Scuola | Grandi nelle prime filiere alimentari |
| Alimentazione II | 350 | Alimentazione I | Avanzate nelle stesse filiere |
| Industria I | 150 | Lavorazioni I | Grandi per estrazione/fusione e Forgia/Zecca |
| Industria II | 450 | Industria I | Avanzate industriali |
| Logistica I | Da fissare | Scuola | Futuri miglioramenti magazzino, senza introdurli implicitamente |
| Navigazione | Da fissare | Ricerca e Zecca | Futuro sblocco Cantiere; regole navali NON ancora approvate |

Tempi orientativi per **100 punti**, senza produzione precedente: 1 ricercatore = 1.000 s (16 min 40 s); 2 ricercatori = 500 s (8 min 20 s). La formazione iniziale non è inclusa. Per 350 punti con 2 ricercatori: 1.750 s (29 min 10 s). Sono tempi nominali, non un benchmark di campagna. Ricerche di settori indipendenti generano differenti strategie; evitare che tutte le ricerche siano obbligatorie per la vittoria.

**Contabilità persone (invariante):** `Popolazione = liberi + lavoratori civili + allievi + ricercatori inattivi + ricercatori attivi + reclute + soldati`. Una persona appare in **una sola** categoria di occupazione. La formazione non aggiunge una persona. In caso di annullamento non si perde popolazione; rimborsi di eventuali costi vanno definiti prima dell'implementazione. Cambi di ruolo non possono duplicare addetti. Gli scienziati mantengono la posizione di residenti e i consumi alimentari. La Caserma v13 conserva costo 260/200, 5 Spade per recluta e addestramento individuale 30 s salvo decisione specifica successiva.

## 4. Migrazioni e retrocompatibilità S1

**Non riscrivere `civil-growth-13`.** Ipotesi di prossimo ruleset: `civil-growth-14` per progressione e ricerca; seguente per navigazione. Lo stato v14 richiederà nuove proprietà nello snapshot. Proposta tecnica: formato snapshot/SQLite **v3** con migrazione transazionale esplicita da v2; gli scenari storici devono continuare a usare i propri formati e benchmark. **Non** alterare retroattivamente i checkpoint di `civil-growth-1`…`-13`.

Stato nuovo persistente minimo:
1. `construction_history(family_id, acquisitions)` monotono per famiglia.
2. `building_tiers(family_id,tier,count,assigned_workers)`; riconciliazione col catalogo e controllo di somme per famiglia.
3. `education_state` (allievi, progressi formativi, ricercatori inattivi/attivi con assegnazione) e capienza scuole.
4. `research_state` (punti disponibili, eventuale progressione) e `research_unlocks` (ID tecnologia univoci e validati).

**Conversione da v13**, solo su copia con backup intatto: tutti gli edifici v13 diventano **livello base**, tranne le categorie esistenti già concettualmente distinte (Casa piccola/grande, Municipio, ecc.), che conservano esattamente tipo e parametri. Per ciascuna famiglia non fissa `K = numero attuale di edifici posseduti` (non esiste demolizione operativa nella baseline); gli edifici iniziali contano in `K`. Non si applica alcun debito retroattivo per costruzioni pagate a prezzo precedente. Le risorse, il Municipio, i bonus capacità, la popolazione, la cronologia della simulazione, le reclute, i soldati e le progressioni militari **non cambiano**. Scuola e ricerche partono da zero. Nessun edificio v13 richiede retroattivamente ricerca per rimanere operativo: la ricerca blocca soltanto **nuove** taglie/gradi (ed eventualmente il futuro Cantiere).

**Sequenza:** 1) leggere/snapshot v13, 2) validarlo con catalogo v13, 3) produrre nuovo snapshot con valori espliciti, 4) validarlo con catalogo v14, 5) backup originale S1, 6) scrittura v3 atomica in nuova transazione, 7) round-trip esatto. Se la migrazione fallisce, non intaccare il file originale; il file va mantenuto e mostrato errore, senza nuovo autosalvataggio sopra la partita incompatibile. Salvataggi sconosciuti/incoerenti: rifiuto esplicito; vietato assumere default che generino risorse o persone.

## 5. Sequenza di implementazione raccomandata

1. **Gate G1-E1** — formula, costo famiglia, parametri e progressivi; modello dati e test per prezzi `+1/+5/+10`, stessa spesa tra lotto e acquisti singoli, no overflow/softlock.
2. **Gate G1-E2** — costi relativi tier, upgrade, migrazioni tipo e storage; test su lavoratori e prerequisiti con tutti i tier.
3. **Gate G1-E3** — Scuola e ricercatori, costo/tempo formazione, punti e albero tecnologie; catene senza cicli.
4. **Release implementativa E1** — motore dei prezzi progressivi e storico, UI costo, salvataggi/migrazione; nessuna ricerca esposta prima della sua implementazione.
5. **Release implementativa E2** — livelli, upgrade, UI e tutti i test; livelli non disponibili al giocatore prima degli sblocchi.
6. **Release implementativa E3** — formazione, ricerca, UI, save v3 e migrazione completa; pubblicare la nuova campagna solo quando il sistema è coerente. L'incremento schema può essere introdotto insieme agli stati minimi e ampliato con tabelle versionate.
7. **Release implementativa E4** — benchmark per 1/3/10, case/Pane, consumo risorse, ricerca e formazione; test playthrough con salvataggi reali e raggiungibilità.
8. **Gate G1-N** — riprezzare Cantiere/navi, verificare 5/10/20 soldati usando i rapporti JSON v59, riapprovare navigazione e isole prima di programmarle.

### Test aggiuntivi obbligatori

- Parità esatta tra ordine da +5 e cinque ordini +1 con materiali sufficienti; acquisto fallito lascia intatti costi e stock.
- Costi scalati per *famiglia*, anche con taglie miste, e contatore non abbassato da upgrade o futura demolizione.
- Nessuna perdita/duplicazione di lavoratori nel passaggio 1→3→10, assenza di produzione con posti non occupati, priorità di produzione stabile con materia prima contesa.
- Riconciliazione stoccaggio pane e altre risorse, edifici fissi invariati, nessun volume di magazzino derivato erroneamente dal tier precedente.
- Formazione e ricerca solo a tempo simulato; nessuna produzione in pausa o fuori gioco, segmentazione del passo equivalente e salvataggio a metà formazione senza duplicati.
- Cicli ricerca/prerequisiti assenti; una famiglia con solo tier avanzato soddisfa il requisito di possedere quella famiglia.
- Salvataggi dalla v13 reale e v41 fork: nessuna reinterpretazione di `military_state`; backup S1, round-trip e migrazione crash-safe.
- Simulazione di vittoria con vincoli reali, no deadlock dovuto a prezzo > capacità, disponibilità di lavoro e Cibo sufficiente; obiettivo storico ~8h vs 32+h **da rivalutare**.

## 6. Decisioni ancora aperte per approvazione

1. **Coeff.** +15% unico in prima versione oppure valori differenziati per famiglia in una seconda fase?
2. **Economia tier.** Accettare 1/2,6/6,5 e capacità 50/150/500 come base di benchmark oppure fissare altri valori?
3. **Ruolo Scuola.** Una Scuola con 1 posto formazione + 2 ricercatori, 120s/10 Cibo e 0,1 punto/s per ricercatore?
4. **Politica ricercatori.** Possibilità di riassegnazione al bacino civile con perdita della qualifica?
5. **Vittoria.** Inserire Scuola tra gli obbligatori: **23 tipi**, 22 precedenti + Scuola, Municipio extra; ogni famiglia conta una volta indipendentemente dal tier?
6. **Ricerca.** Sblocchi di settore e nessuna tecnologia retroattiva per edifici base già esistenti?

Queste cifre sono candidati da misurare, non impostazioni già approvate. `validate.cmd` non è stato rieseguito in G1-D3 perché nessun C#, test o script è cambiato. La sola validazione dei file ZIP non dimostra il raggiungimento delle 8 ore o la bontà dei costi.
