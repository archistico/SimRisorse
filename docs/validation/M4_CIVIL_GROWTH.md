# M4 — Primo incremento di crescita civile

Data: 2026-10-26, aggiornato il 2026-10-27. **Ultimo esito verificato: gate Windows completo superato, 177/177 test**, dopo la correzione della palette condivisa di `PixelGlyph`. Quattro TRX e nove render ispezionati; lettore dei quattro benchmark eseguito con input conservati. Il [rapporto misurato M4](M4_MEASURED_RESULTS.md) contiene risultati, provenienza e limiti. M4 rimane in corso per le scelte progettuali residue. Le sezioni seguenti conservano la cronologia degli incrementi e gli stati delle verifiche al momento della loro preparazione.

Baseline invariata: .NET 10, Avalonia 12.1.4, Microsoft.Data.Sqlite 10.0.14; xUnit 3 per Desktop e 2 per gli altri test. Core e Application mantengono dipendenze solo dal framework e dai riferimenti interni. Nessun nuovo pacchetto.

## Risultati

Build Release senza errori o avvisi, restore con lock file, **117 test superati**:

| Progetto | Test |
| --- | --- |
| Core | 49 |
| Application | 24 |
| Infrastructure | 21 |
| Desktop | 23 |

I nuovi controlli coprono:

- Arrivo soltanto al sessantesimo secondo valido, consumo dal passo successivo e persone intere.
- Progresso a 59,5 secondi salvato, pausa senza avanzamento e arrivo dopo altri 0,5 secondi.
- Case piene senza accumulo di arrivi; più case non moltiplicano il ritmo.
- Cibo prenotato prima delle ricette; nessuna razione parziale e nessun riuso immediato dello spazio liberato.
- Carenza che conserva persone e progresso; cibo nuovo utilizzabile dal passo successivo.
- Snapshot invalidi, capacità abitativa fuori limite e acquisti falliti senza mutazioni parziali.
- Proiezioni confrontate con le variazioni effettive di tutte le risorse per 181 secondi, inclusi due arrivi.
- Ripresa e rollback su SQLite reale v2: popolazione, crescita, tempo frazionario, scorte ed edifici rimangono coerenti.
- Stato di crescita mancante, negativo o oltre i limiti tecnici rifiutato; errore applicativo senza sostituire la partita corrente.
- File v1 e v2 che non si sovrascrivono, in entrambe le direzioni; fixture M3 ancora caricabile senza modificarla.
- UI con case, posti, cibo, crescita e carenze; nuova partita nello scenario corrente e percorsi personali separati.

## Simulazioni e desktop

| Cronologia | Durata | Esito |
| --- | --- | --- |
| Foundation, nessun ordine | 60 s, 8 h, 32 h | Timeline precedente conservata e risorse entro capacità |
| Civil, bootstrap | 121 s | 9 abitanti, progresso 0, 140,32 cibo |
| Civil, recupero | 662 s | 14 abitanti, progresso 0, produzione alimentare recuperata |
| Civil, bootstrap | 8 h e 32 h | 16 abitanti, nessuna crescita oltre i posti e scorte nei limiti |

Otto render: viste Edifici/Risorse per i due scenari, a 1240×850 e 1040×740. Ispezionate le nuove viste; controllati i comandi primari e il contenimento della barra di crescita nella colonna laterale. Le schede mostrano tre risorse in evidenza; la tabella Risorse conserva l'elenco completo. I saldi negativi hanno segno esplicito e colore distinto.

Pubblicazione Windows e ciclo nativo completati per entrambi gli scenari. Per Civil: costruzione della casa, progresso 59 s, salvataggio, nuova partita, ricaricamento e singolo arrivo dopo 1 s. Per entrambi: notifica Win32 di sospensione alla sola finestra e chiusura ordinata con salvataggio. File privati di verifica, nessun utilizzo dei salvataggi personali.

## Secondo incremento: magazzino e `civil-growth-2` (2026-10-27)

Decisione nell'[ADR 0010](../adr/0010-storage-capacity-and-rules-conversion.md). Build Release senza errori o avvisi, restore offline con lock file, **128 test superati** (+11):

| Progetto | Test |
| --- | --- |
| Core | 54 |
| Application | 27 |
| Infrastructure | 22 |
| Desktop | 25 |

Nuovi controlli:

- Produzione ferma a capacità piena, ripresa dal passo successivo alla costruzione del magazzino e nuovo arresto al nuovo limite.
- Ripristino verificato rispetto alla capacità degli edifici salvati; overflow della capacità rifiutato come dato invalido.
- Acquisto oltre il limite numerico fallito con `NumericLimit` senza mutazioni; scorte iniziali entro la capacità degli edifici iniziali; effetti su risorse sconosciute rifiutati dal catalogo.
- Scenario civile: capacità 1.500 su tutte le cinque risorse dopo un magazzino, saldi previsti uguali alla variazione effettiva nel secondo successivo.
- Conversione di un checkpoint `civil-growth-1` con progresso a 59,5 s: tempo, scorte e crescita invariati, magazzino a zero, ripresa in pausa. Regole sconosciute, scenario diverso e checkpoint v1 incoerente rifiutati senza sostituire la partita.
- SQLite reale: copia `<nome>.<regole>.backup.sqlite3` creata solo quando cambiano scenario o regole, mai sovrascritta (suffisso `-2`), nome ripulito da caratteri non sicuri, contenuto ricaricabile; nessuna copia per generazioni di file diverse.
- Desktop: messaggio di conversione all'avvio, copia creata al primo salvataggio, descrizione/effetto/stato del magazzino e capacità 1.500 nella vista risorse.

Simulazioni: le sette precedenti con gli stessi esiti e la nuova cronologia `storage` (1260 s): 16 abitanti, capacità 1.500 per ogni risorsa e pietra a 1.000 dopo la ripresa. Otto render rigenerati; nella vista compatta l'elenco di sette strutture scorre e i comandi primari restano visibili. Pubblicazione e ciclo nativo Windows superati per entrambi gli scenari.

## Terzo incremento: filiera del pane e `civil-growth-3` (2026-10-27)

Build Release senza errori o avvisi, restore offline con lock file, **132 test superati** (+4):

| Progetto | Test |
| --- | --- |
| Core | 54 |
| Application | 30 |
| Infrastructure | 22 |
| Desktop | 26 |

Nessun cambiamento al Core: fattoria, pozzo, mulino e forno sono definizioni di catalogo. Nuovi controlli:

- Prerequisiti mulino→fattoria e forno→mulino+pozzo, costi insufficienti senza mutazioni.
- Cronologia completa dal tempo zero: a 600 s risorse e persone libere esatte; mulino attivo dal secondo passo, forno dal terzo; saldi previsti uguali alla variazione effettiva per dieci secondi; a regime +4 cibo/s e grano, farina, acqua costanti.
- Conversione concatenata da `civil-growth-1` e da `civil-growth-2`, con tutte le nuove strutture a zero; rifiuto dei checkpoint che dichiarano regole precedenti ma contengono già le nuove strutture.
- Copia di sicurezza e messaggio desktop aggiornati alla nuova versione; ricette e prerequisiti mancanti del forno visibili nella UI.
- Vista Risorse: con otto risorse la tabella scorre dentro la finestra nella dimensione compatta; intestazione e nota restano visibili (verificato dal test di render e ispezionando il PNG).

Simulazioni: le otto precedenti con gli stessi esiti e la nuova cronologia `bread` (660 s): 16 abitanti, 4 cibo/s, forno attivo. Pubblicazione e ciclo nativo Windows superati per entrambi gli scenari.

## Analisi di raggiungibilità del catalogo (2026-10-27)

Build Release senza errori o avvisi, **140 test superati** (+8): Core 59, Application 33, Infrastructure 22, Desktop 26. Nessun cambiamento a regole, contenuti o salvataggi.

- Core: catalogo di riferimento completo; costo non ottenibile che blocca la struttura e, a cascata, i dipendenti; dotazione iniziale che paga un costo ma non rende rinnovabile un input; costi e ricette oltre la capacità, risolti da un magazzino raggiungibile; struttura già posseduta raggiungibile anche con costo non ottenibile.
- Application: Foundation e Civil Growth hanno tutte le strutture raggiungibili e tutte le risorse rinnovabili; le conversioni dichiarate formano una catena continua fino alle regole correnti e aggiungono solo strutture del catalogo.
- Validazione: `--check-catalog` per entrambi gli scenari, con report `catalog-<scenario>.json`; le nove simulazioni, gli otto render e il ciclo nativo Windows restano superati.

## Incremento successivo: diagnostica delle produzioni (in attesa di gate)

Il checkpoint verificato resta quello dei **140 test**. Sono stati preparati tre nuovi test (uno Core, uno Application, uno Desktop), oltre ad asserzioni più precise sui test di contesa già presenti. Il pianificatore indica quali input limitano la produzione e quali output non trovano spazio, calcolandoli secondo le priorità e le prenotazioni effettive. La UI li mostra nel dettaglio Edifici, senza toccare il comportamento delle ricette o il salvataggio. In questa sessione non è disponibile `dotnet`, quindi non sono dichiarati test, build o render superati per questo incremento.

Gate originariamente previsto per la sola diagnostica: **143 test** attesi. Poiché è stato aggiunto anche l'incremento successivo prima della validazione, il gate corrente deve considerare l'intera suite descritta sotto. Ispezionare comunque la vista Edifici a entrambe le risoluzioni, con particolare attenzione alla diagnostica nel pannello laterale.

## Quarto incremento: Casa grande e Pane come razione distinta (`civil-growth-4`, gate in attesa)

L'utente ha approvato la [scheda quantitativa](../scenarios/ADVANCED_HOUSING_PROPOSAL.md). Il codice implementa:

- Casa grande: +32 posti per 160 legno lavorato + 80 pietra, richiede Forno costruito e non occupa lavoratori.
- Forno: 1 Farina + 1 Acqua → 3 Pane/s (invece di 3 Cibo/s); il Cibo comune resta prodotto dal Deposito alimentare. Capacità Pane di base 100, con +500 per Magazzino.
- Popolazione: 0,010 Cibo/persona/s per tutti, più 0,015 Pane/persona/s oltre la capacità delle Case piccole. Il difetto di Pane blocca la crescita senza arrestare il consumo disponibile di Cibo; nessuna perdita di persone, pausa/offline senza consumo. Il passaggio 16→17 richiede scorta Pane; il nuovo arrivato inizia a consumarla dal passo successivo.
- Conversione `civil-growth-3 → -4` concatenata da `-1/-2`; nuova struttura a zero, Pane a zero e scorta Cibo pregressa invariata, perché non si conosce la fonte. Snapshot e file SQLite v2 invariati, copia di sicurezza prevista al primo salvataggio delle regole convertite.

Test aggiunti ma **non eseguiti**: sei test Core (razioni, sospensione/ripresa, ampliamento Case piccole, Pane pieno, regole invalide e mancato consumo di Pane senza Cibo); un'integrazione Application con progressione 16→17 e flussi previsti; un test SQLite reale con conversione e backup anche con Forno già impiegato; un test Desktop per fabbisogno Pane, più casi di conversione nelle teorie esistenti. La diagnostica della sezione precedente aggiunge tre test ancora non eseguiti. Conteggio **atteso 153 casi** (140 + 3 diagnostica + 10 avanzati, inclusa una nuova riga teorica), da confermare con risultati TRX; non dichiarare un conteggio superato prima del gate.

Le simulazioni ora predisposte in `scripts/validate.ps1` sono **dodici** (le nove storiche più `civil-bread-start` a 610 s, `civil-advanced` a 880 s e `civil-advanced-8-hours`). Attesi dalla regola, non ancora osservati con .NET:

| Cronologia | Momento | Atteso | Motivo |
| --- | --- | --- | --- |
| `bread` | 610 s | Forno attivo, 3 Pane/s | Filiera completa |
| `bread` | 660 s | Pane 99/100, Forno bloccato dalla capacità | Output a lotti di 3 unità |
| `advanced` | 880 s | 18 abitanti, 2 avanzati; 48 posti; 0,030 Pane/s | Casa grande al 760° secondo, crescita dopo 60+60 s |
| `advanced` | 8 ore | 48 abitanti, crescita ferma per case piene | Una filiera supera il consumo di Pane, salvo limiti inattesi |

È predisposto anche il confronto `--benchmark-housing` fra tre strategie abitative su otto ore, con registrazione di `housing-benchmark.json` e controlli sui tempi delle soglie 48/100/136 e sui materiali. La [scheda dedicata](../scenarios/HOUSING_BENCHMARK.md) distingue risultati teorici e dati da misurare. Questo confronto non aggiunge un test xUnit, non è stato eseguito e non completa M4.

### Rafforzamento del replay e del salvataggio avanzato (2026-10-27)

Sono stati aggiunti **tre test non eseguiti**:

- Application: la medesima sequenza di scelte (Casa piccola al secondo 61, filiera Pane al 600, Casa grande al 760, seconda Casa grande al 3000) viene riprodotta sia con passi da un secondo sia con intervalli da 137 ms fino a 91 secondi. Gli snapshot devono coincidere su tempo, crescita, popolazione, scorte e strutture dopo 879,5 s, 880 s, 3000 s e 7200 s; attesi 18 abitanti al secondo 880 e 80 dopo due ore.
- Application: ripristino da snapshot a **879,5 secondi**, quando i 17 abitanti hanno accumulato 59 secondi validi per il prossimo arrivo. La sessione deve restare immobile durante una pausa lunga e poi coincidere con quella continua dopo altri 0,5 secondi e fino a due ore.
- Infrastructure: lo stesso confine frazionario verificato su **SQLite reale**, conservando Pane, Cibo, lavoro assegnato al Forno, Casa grande e progresso della crescita. Dopo il caricamento la partita riprende in pausa; la successiva evoluzione deve rimanere identica alla partita originale.

Il conteggio xUnit passa da **153 previsti a 156 previsti**. Resta confermato soltanto il checkpoint precedente di **140 test superati**; per questi casi non si attribuiscono esiti senza compilazione, TRX e simulazioni .NET. Non sono cambiati né `civil-growth-4` né lo snapshot v2.

### Riconciliazione economica e stress del Forno (2026-10-27)

- Il confronto fra le tre strategie controlla ora **a ogni secondo** che i consumi previsti, le ricette e i lavoratori attivi producano esattamente le scorte ottenute, e che l'avanzamento della crescita sia quello previsto. Il JSON aggiunge `ReconciledSeconds`; la validazione richiede **28.800** per strategia. Questo controllo integrato nel benchmark non aggiunge test xUnit.
- Un nuovo test Core a 220 abitanti (212 avanzati) e otto Case grandi confronta un Forno, due Forni e la costruzione del secondo dopo 15 minuti. Il limite teorico di un Forno è 200 residenti avanzati nutriti senza interruzioni. La prova richiede 250 abitanti per i due Forni fin dall'inizio e un miglioramento misurabile con il rinforzo tardivo.
- Il [modello aritmetico indipendente](../scenarios/HOUSING_BENCHMARK.md) prevede 246/249/250 abitanti rispettivamente per un Forno, secondo tardivo e due immediati. **Numeri da confermare con l'esecuzione del Core .NET.**

Conteggio xUnit **atteso 157** (+1 Core rispetto ai 156 predisposti); checkpoint effettivo fermo a **140 superati**. Regole `civil-growth-4` e formato SQLite v2 invariati.

### Diagnostica della sostenibilità del Pane nella UI (2026-10-27)

La sidebar Crescita mostra, quando ci sono residenti avanzati, un testo che distingue capacità nominale dei Forni insufficiente, mancanza di input, Forno fermo perché il deposito è quasi pieno e produzione effettiva sufficiente nel prossimo secondo. L'autonomia teorica della scorta **senza produzione** è calcolata sulla domanda attuale e mostrata in ore/minuti/secondi. Non è una proiezione della durata reale se cambiano forniture e popolazione. Il [documento dello scenario](../scenarios/CIVIL_GROWTH.md) ne precisa l'interpretazione.

È stato aggiunto **un test Desktop** con Pane a capacità massima e Forno assegnato: deve indicare la limitazione dello spazio, non una capacità nominale insufficiente. È stato rafforzato il test Desktop con nove residenti avanzati e 20 Pane, che richiede un'indicazione di **2 min 28 s** senza produzione. Il nuovo test verifica il posizionamento dei testi nella sidebar a **1040×740** e salva `civil-advanced-bread-compact.png`. Tutto resta in attesa di esecuzione e ispezione visuale del nono render.

Conteggio xUnit **atteso 158** (+1 Desktop rispetto ai 157 predisposti); checkpoint effettivo sempre **140 superati**. Motore economico, regole `civil-growth-4` e snapshot v2 invariati.

### Ripresa distinta da carenza di Pane e Cibo (2026-10-27)

Un nuovo test Desktop copre il punto in cui 16 abitanti dispongono di una Casa grande, ma ancora zero residenti avanzati, 59 secondi di progresso e Pane a zero. Verifica che **la diagnosi Pane sia visibile anche prima del primo arrivo avanzato** e che:

1. il primo passo produce 3 Pane, senza avanzare crescita né consumare Pane;
2. al secondo passo la scorta già disponibile permette l'arrivo del diciassettesimo abitante, conservando il Pane intero;
3. soltanto il terzo passo consuma 0,015 Pane per il nuovo residente.

Un nuovo test Application carica una colonia con 17 abitanti, 59 secondi validi, Cibo a zero, 1 Pesce e 1 Pane. Un Deposito alimentare già operativo trasforma il Pesce in Cibo al primo passo, senza consumare Pane e senza completare la crescita; soltanto al secondo passo vengono consumati 0,170 Cibo e 0,135 Pane e arriva il diciottesimo abitante. Il test confronta fabbisogni, scorte, blocchi e progresso tramite i modelli Application.

Il totale raggiunge **160 casi xUnit previsti** (+1 Desktop e +1 Application rispetto ai 158 precedenti). Nessuno di questi due nuovi casi è stato eseguito con .NET; rimangono **140 test effettivamente superati** al checkpoint `civil-growth-3`. Non cambiano regole o formato di salvataggio.

### Diagnostica Pane indipendente dalla GUI e carenze simultanee (2026-10-27)

La valutazione della sostenibilità del Pane è stata trasferita dal ViewModel Avalonia al modello di lettura di Application: `GameView.BreadSupply` ora contiene uno stato tipizzato (`BreadSupplyState`) e i secondi interi di autonomia della scorta al consumo corrente, quando ci sono residenti avanzati. Il desktop traduce questi valori nei testi già previsti; per il giocatore non cambiano le unità, le ricette o la regola di crescita. Questa separazione è utile anche alla futura GUI pixel art retro 8-bit, che potrà associare icone differenti agli stessi stati senza ricalcolare la simulazione.

Corretto un caso di informazione incompleta: **se prima del primo arrivo avanzato mancano insieme Cibo e Pane**, la crescita rimane bloccata prioritariamente per `MissingFood`, ma l'interfaccia mostra anche l'esigenza di Pane per il successivo abitante. Quando il Pane viene aggiunto, l'avviso supplementare scompare; non si anticipano razioni né arrivi.

Due nuovi test Application predisposti e **non eseguiti** verificano: (1) carenze simultanee alla soglia del primo residente avanzato e scomparsa dell'avviso dopo il rifornimento di Pane; (2) distinzione tra input mancanti, produzione nominalmente sufficiente e deposito pieno, con autonomia verificabile. Rafforzati tre test Desktop esistenti sui corrispondenti stati tipizzati. Conteggio xUnit **atteso: 162 casi** (+2 Application), ancora da confermare con TRX. Ultimo checkpoint effettivamente confermato: **140 test superati**. `civil-growth-4`, snapshot v2 e schema SQLite invariati.

### Gate precedente confermato dall'utente e nuova prova di scala (2026-10-27)

L'utente riferisce **"test e validazione passati"** per il pacchetto precedente. Registriamo dunque il gate `civil-growth-4` come **superato secondo verifica dell'utente**, comprendendo i 162 casi predisposti e il validatore generale. In questa sessione non sono disponibili il report TRX, `validation.json`, i PNG o il risultato numerico del benchmark: la conferma viene attribuita all'utente, mentre l'ultimo report dettagliato ispezionato localmente resta quello dei 140 test precedenti.

Con il gate precedente confermato, è stato aggiunto `--benchmark-bread-scale` ([specifica](../scenarios/BREAD_SCALE_BENCHMARK.md)): tre colonie sintetiche identiche da 220 abitanti, una o due filiere Pane complete, aggiunta tardiva della seconda al minuto 15, monitoraggio di 1.800 secondi e riconciliazione di ogni ricetta e razione. Il nuovo comando è integrato nel validatore e produce `bread-scale-benchmark.json`. L'utente ha successivamente riferito **"test passati"**, confermando anche questo incremento; il JSON numerico non è tuttavia disponibile per una lettura indipendente. Suite xUnit invariata rispetto ai 162 casi del gate precedente; nessuna modifica a `civil-growth-4` o al formato di salvataggio.

### Deficit nominale Pane e personale minimo (nuovo gate, 2026-10-27)

`BreadSupplyView` espone ora, oltre allo stato e all'autonomia teorica della scorta, il fabbisogno `RequiredPerSecond`, la produzione `NominalProductionPerSecond` di tutti i lavoratori assegnati, la `NextSecondProductionPerSecond` realmente prevista, `NominalShortfallPerSecond` e, se esiste un solo tipo di produttore di Pane, `MinimumAssignedWorkers` e `AdditionalAssignedWorkers`. Questi ultimi misurano la **capacità nominale**, non la sostenibilità degli input né la disponibilità di posti di lavoro.

Verifica attesa a **220 abitanti, 212 avanzati**: con un Forno assegnato la domanda è 3,180 Pane/s, nominale 3 Pane/s, deficit 0,180 Pane/s, minimo due addetti, quindi ne manca uno. Con due Forni ma zero Farina/Acqua, la capacità nominale diventa 6 Pane/s e il deficit nominale si azzera, ma la previsione reale resta zero e la diagnosi corretta è `MissingInputs`. Il test previene una falsa dichiarazione di sostenibilità basata solo sugli addetti.

Un nuovo test Application e asserzioni aggiuntive nei test Application/Desktop, compreso il controllo geometrico della sidebar a 1040×740, portano la suite a **163 casi xUnit attesi**, **non ancora eseguiti** qui per assenza di .NET. Nessuna variazione alle formule, alle versioni delle regole, allo schema SQLite o alla futura direzione grafica.

### Limite teorico medio dell'intera filiera (incremento corrente, 2026-10-27)

Oltre alla sola capacità nominale dei Forni, `BreadSupply.ChainCapacity` ora risale le ricette di Farina, Acqua e Grano, espone fabbisogno e lavoratori per anello e trova il minimo fra gli equivalenti di Pane/s. Una filiera completa da cinque lavoratori, con 220 abitanti, ha limite medio teorico **3 Pane/s** contro una domanda di **3,180 Pane/s**. Aggiungere soltanto un secondo Forno aumenta la capacità del Forno a 6, ma lascia l'intera filiera limitata a 3. Con una sola Fattoria il Grano porta il limite a **1,5 Pane/s**; con due filiere complete si raggiunge il limite teorico di **6 Pane/s**, anche se la produzione effettiva del passo può essere zero per mancanza di ingredienti.

Queste situazioni sono aggiunte come asserzioni al test Application già presente a 220 abitanti. Nessun nuovo metodo `[Fact]`, quindi il conteggio rimane **163 casi attesi**, ancora da verificare. Il Core non viene modificato e `civil-growth-4` resta la versione corrente. La [scheda di bilanciamento](../scenarios/BREAD_CHAIN_CAPACITY.md) presenta anche il potenziamento candidato a **quattro addetti** (+360 legno lavorato / +250 pietra, limite medio teorico 4,5 Pane/s), più economico del raddoppio completo ma non ancora misurato tramite il motore in esecuzione. Il limite è un **bound teorico**, non un nuovo forecast di produzione continua e non una promessa di assenza di carenze.

### M4.7 — Quarta strategia di scala: filiera snella (implementata, non validata)

- `--benchmark-bread-scale` contiene ora **quattro politiche**, compresa `lean-chain`: una Fattoria anziché due nel potenziamento, più Pozzo, Mulino e Forno come nell'espansione completa. Gli acquisti e le assegnazioni passano dai comandi del Core, non da modifiche dello snapshot in corsa.
- Il validatore richiede **360 legno lavorato, 250 pietra e quattro lavoratori aggiuntivi**, contro 420/280 e cinque per l'altra espansione. Non sono nuovi prezzi: si sommano i costi già previsti dal catalogo corrente.
- Per ogni politica vengono salvati i **secondi-lavoratore** di Fattorie, Pozzi, Mulini e Forni, il Pane effettivamente prodotto/consumato, le scorte finali della catena e i precedenti indicatori di crescita. Ogni strategia deve riconciliare i **1.800 secondi** e l'intero bilancio cumulato del Pane.
- Il gate verifica che la snella non peggiori la crescita o i secondi di carenza Pane rispetto alla filiera singola, e che la popolazione finale non superi quella delle due filiere complete. L'eventuale arrivo a 250 persone **non è ancora dichiarato osservato**.
- **Nessun caso xUnit aggiunto:** la suite rimane a **163 casi attesi**, cui si aggiungono i controlli del benchmark eseguiti dallo script di validazione. Nessun cambiamento a `civil-growth-4`, file SQLite, GUI o salvataggi.

### Gate v17 riferito superato e nuova prova M4.8 (2026-10-27)

L'utente ha riferito **«test passati» per v17**. Registriamo quindi il gate della diagnostica Forno/filiera e del confronto snello da 220 abitanti come **superato secondo verifica dell'utente**. Non sono stati allegati TRX, `validation.json` o JSON di benchmark: non conosciamo il numero numerico di arrivi per ciascuna strategia. I documenti storici sopra mantengono lo stato *non verificato* che era corretto al momento della loro stesura.

Con M4.8 è ora implementato **`--benchmark-bread-high-density`** ([specifica](../scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md)): colonia sintetica iniziale da 320 abitanti, 360 posti, quattro filiere Cibo e 1.800 secondi per ognuna delle quattro strategie Pane. Il validatore salva un terzo JSON, `bread-high-density-benchmark.json`, e verifica il bilancio di Pane, i materiali/lavoratori aggiuntivi e la crescita attesa oltre la soglia teorica della filiera snella. **Questo incremento non è ancora stato eseguito in .NET.** La suite xUnit non cambia: **163 casi attesi**.

**Nuovo gate Windows:** eseguire `./validate.cmd` in PowerShell nella radice del progetto (oppure `./validate.cmd -OfflinePackageSource .local-cache/nuget` con cache completa). Verificare compilazione senza avvisi, suite completa inclusi i replay, la diagnostica Pane e le carenze simultanee, raggiungibilità dei due cataloghi, tutte le dodici cronologie, **tre benchmark** (abitazioni, Pane da 220 e Pane da 320 abitanti), **nove PNG** (otto viste precedenti e lo stato Pane a 1040×740), riavvio e backup SQLite. Controllare che la vista compatta permetta di raggiungere Casa grande e Pane e che la nuova diagnosi nella sidebar non sovrapponga gli elementi. I PNG devono essere presenti nella cartella isolata dell'esecuzione corrente, non recuperati da prove storiche. Registrare gli esiti effettivi prima di cambiare lo stato da "gate in attesa".

### Gate M4.8 confermato dall'utente e intervento ergonomico v22 (2026-10-27)

L'utente ha successivamente riferito **«test passati» per v21**, che include il codice M4.8 introdotto in v18. Il gate di esecuzione dello stress da 320 abitanti si considera **superato in base alla sua verifica**, fermo restando che qui non sono allegati i JSON dei tre benchmark, i TRX e le immagini. Non si deducono popolazioni finali, tempi di arrivo a 340/350 o secondi esatti di carenza senza leggere i report effettivi. Le righe storiche precedenti mantengono il loro stato al tempo della stesura.

Il **checkpoint v22**, ancora da eseguire, modifica solo la GUI Avalonia provvisoria:

1. `BuildButton` differenzia cromaticamente `:pointerover`, `:pressed` e `:disabled`; il disabilitato appare grigio, l'hover di un comando eseguibile è verde più luminoso con bordo.
2. `BuildingDiagnosisPanel` con il testo `Per costruire servono prima: ...` viene reso **fisso nel pannello azioni** anziché collocato nello `ScrollViewer` di descrizione/ricetta. È quindi possibile leggere il messaggio completo senza scorrere il dettaglio.
3. Sono stati ridotti gli ingombri di testata, sidebar, cards, lista degli edifici, dettaglio, Risorse e pulsanti. Due nuovi casi Desktop coprono il testo di prerequisito a **1040×740 e 1240×850**; un terzo confronta i colori del pulsante normale, in hover e disabilitato.

**Conteggio atteso: 166 casi xUnit (163 precedenti + 3 Desktop)**. Senza SDK .NET locale, build, nuovo gate e geometrie non sono ancora dichiarati superati. Su Windows eseguire `./validate.cmd`, verificare le nove immagini esistenti e ispezionare visivamente sia il pulsante sotto il puntatore sia i messaggi di prerequisito su Forno, Casa grande e altri edifici bloccati. I test automatici geometrico/cromatico riducono il rischio, ma non sostituiscono una verifica visiva del layout. Nessun cambiamento a regole, prezzi, cataloghi, filiere, SQLite o formato dei salvataggi.

### Gate GUI v22 confermato e regressione hover v23 (2026-10-27)

L'utente ha successivamente riferito **«test passati» per v22**. Il checkpoint della compattazione viene quindi registrato come **superato secondo la verifica dell'utente** (166 casi previsti); non sono allegati TRX o PNG per una verifica autonoma. Durante l'uso ha però osservato un difetto visuale: entrando con il mouse su **Costruisci**, gli elementi precedenti si spostavano leggermente verso l'alto.

Il codice v22 applicava al pulsante un bordo di **2 pixel in hover** e **1 pixel in disabilitato**; variando lo spessore, il layout poteva ricalcolare l'altezza del controllo. In **v23** `Button.primary.build` definisce un bordo **costante di 1 pixel** per ogni stato, cambiando soltanto i colori durante l'hover. Il test Desktop viene esteso per confrontare *prima/dopo hover* e *dopo esaurimento delle risorse* posizione e dimensione del pulsante, del costo e della selezione quantità alle due risoluzioni **1040×740 e 1240×850**.

Un caso test singolo diventa due righe della teoria, quindi **167 casi xUnit attesi** (166 confermati dall'utente per v22 + 1 nuovo caso). Questo incremento non è stato compilato né eseguito in questa sessione priva di SDK .NET. Richiede `./validate.cmd` su Windows e osservazione del pulsante mentre il mouse entra/esce e dopo l'acquisto; il precedente gate v22 non è invalidato retroattivamente, ma il difetto segnala una regressione da correggere. Nessun effetto su regole, Core, salvataggi o risultati dei benchmark.

### Conferma v23 e nuovo gate v24 — Accelerazione abitativa (`civil-growth-5`)

L'utente ha riferito **«funziona, test passati»** per la correzione hover v23; il checkpoint precedente da **167 casi attesi** risulta quindi superato secondo la sua verifica. TRX/PNG non sono allegati.

In **v24/M4.10** sono implementati +15% per ogni Casa piccola oltre la prima, +30% per Casa grande, limite 2×. La crescita è conservata come **secondi interi + resto in ventesimi**, aggiornati nel solo tempo di gioco valido; il contatore non viene azzerato dalla costruzione di nuovi edifici e trattiene l'eccedenza dopo una nascita. L'interfaccia mostra velocità e tempo approssimativo al prossimo abitante.

**Nuovi controlli predisposti** (+7 xUnit, totale **174 attesi**):

1. Application: bonus piccolo, bonus grande, tetto 2×, cambi di ritmo e salvataggi fra passi, blocchi per carenza di cibo o abitazioni piene.
2. Infrastructure: caricamento di una partita `civil-growth-4` con progresso 59 s, copia di sicurezza, conversione `-5`, nuovo arrivo e persistenza della frazione; caricamento di SQLite v2 storico senza tabella dei resti.
3. Desktop: rappresentazione di ×1,15, countdown e frazione esatta prima/dopo un arrivo.

Il validatore conserva le **12 cronologie storiche** e i **3 benchmark M4 di riferimento** tramite regole legacy, aggiunge un caso attivo a `civil-growth-5` e un **quarto benchmark abitativo accelerato** che confronta gli stessi piani di acquisto, le stesse spese e i tempi per 48/100/136 abitanti. Le variazioni fra regole nuove e storiche sono esplicite e non vengono nascoste modificando i risultati passati.

**Gate v24:** l'utente conferma passati test e validazione Windows, **174 casi previsti**, ma non ha allegato TRX e JSON dei benchmark; non attribuire loro valori inventati. **Gate v29 da eseguire** (comprende il prototipo v25 invariato e il lettore v26): `validate.cmd`, compilazione Release, **177 test xUnit attesi**, simulazioni, JSON storici e nuovi, smoke nativo di `civil-growth-5` e `economy-foundation`, render a entrambe le risoluzioni. Controllare in particolare i pittogrammi originali, i pannelli quadrati, l'assenza di sovrapposizioni e il comportamento di Costruisci.

## Riproduzione del checkpoint e del nuovo gate

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
.\run-game.cmd
.\run-game.cmd --scenario economy-foundation
```

Il validatore scrive in `.local-cache/m4/`: `validation.json`, dodici JSON storici di simulazione e una cronologia accelerata, due report del catalogo, **quattro benchmark JSON** e pacchetto. I **nove PNG** sono generati in `runs/<id>/screenshots/`, accanto a risultati/log/database isolati della stessa esecuzione. La cache NuGet rimane quella del workspace e i report precedenti non vengono cancellati.

Il nuovo [lettore M4.12](M4_BENCHMARK_READER.md), avviabile con `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1`, trasforma i quattro JSON disponibili in un confronto Markdown delle soglie raggiunte e dei secondi risparmiati, oltre alla filiera Pane a 220 e 320 residenti. Il comando è **facoltativo, autonomo e senza effetti sulla partita**; senza gli output Windows non dispone di alcun risultato numerico da dichiarare.

L'[audit M4.13 delle abitazioni](../scenarios/HOUSING_INCENTIVE_AUDIT.md) verifica invece una proprietà della **formula stessa**, indipendente dai benchmark: quattro combinazioni raggiungono 2× spendendo gli stessi 560 legno lavorato / 280 pietra in sole case, ma sostengono da 64 a 112 abitanti con fabbisogni alimentari diversi. Questa è un'analisi teorica, non un esito osservato della simulazione.

## Limiti

Le filiere del checkpoint precedente dimostrano raggiungibilità e recupero, non ottimalità matematica o bilanciamento sulle otto ore. L'utente ha confermato i gate fino a **v24** su Windows, ma i risultati numerici dei benchmark non sono disponibili per l'analisi indipendente; **v25–v29 rimangono da verificare**, incluso il lettore PowerShell. Non sono implementati carne, demolizione, esercito, navi, isole o classifica.

Nessuna migrazione di formato; **quattro conversioni di regole dichiarate e concatenabili**; la copia di sicurezza non è gestibile dalla UI; scelta dello scenario tramite argomento di avvio, non ancora tramite schermata dedicata. Linux/macOS, sospensione fisica del computer, spegnimento improvviso e distribuzione self-contained restano da verificare. Audit CLI non completato per TLS nell'ambiente; nessuna dipendenza è cambiata rispetto al [controllo puntuale della migrazione Avalonia](AVALONIA_12.md).
