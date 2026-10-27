# M5.1 — Studio di prefattibilità delle prime filiere industriali

Data: 2026-10-27. **Configurazioni A di M5.1a Ferro e M5.1b Oro scelte separatamente e verificate**, corrente `civil-growth-7`, 17 edifici / 14 risorse e **204/204 test**. M4 chiusa e continuità S1 acquisita. [Report Oro](../validation/M5_1B_GOLD.md), [report Ferro](../validation/M5_1A_IRON.md). I cinque edifici della tabella A sono ora adottati; **B e filiere a valle restano proposte**. La [prova A/B](../../experiments/industry-probe/README.md) conserva la baseline civile v5 e il suo output storico.

## Obiettivo concreto

Preparare il primo passaggio dalla produzione civile all'industria, evitando di sviluppare contemporaneamente miniera, moneta, armi, caserma e isole. I [diagrammi di riferimento](../references/RESOURCE_AND_PRODUCTION_CHAINS.md) mostrano:

- Miniera di ferro → minerale di ferro; minerale + carbone → fusione → ferro.
- Miniera di carbone → carbone, necessario anche ad altre lavorazioni.
- Miniera d'oro → minerale d'oro; minerale + carbone → fusione → oro.
- Successivamente: ferro + carbone → armi; oro + carbone → monete.

Il disegno indica le *relazioni*, **non** quantità per secondo, prezzi, lavoratori, capacità o tempi: i numeri qui sotto sono un **set di prova candidato** per una simulazione futura, non dati presi dal riferimento né una modifica al motore attuale.

## Suddivisione per incrementi piccoli

| Incremento | Contenuti da proporre | Esito tecnico cercato | Attendere prima di implementare |
| --- | --- | --- | --- |
| **M5.1a — Ferro e carbone** | 3 edifici: miniera ferro, miniera carbone, fonderia ferro; 3 risorse: minerale ferro, carbone, ferro | Una filiera chiusa che rispetta input, deposito e lavoratori | Catalogo approvato; politica per salvataggi/regole |
| **M5.1b — Oro** | 2 edifici: miniera oro, fonderia oro; 2 risorse: minerale oro, oro | Due fonderie competono per il carbone | M5.1a validata |
| **M5.1c — Scala e contesa** | Nessun edificio indispensabile in più; benchmark con più miniere/fonderie | Limiti teorici contro resa reale e magazzini pieni | M5.1b validata |
| **M5.2 — Utilizzo industriale** | Forgia, Zecca, eventualmente Monete e Spade come nuovi prodotti | Le risorse industriali servono a un obiettivo; confronto fra contesa e produzione a fasi | Ruoli e costi militari/navali approvati in D0; [prefattibilità dedicata](M5_2_FORGE_MINT_PREFEASIBILITY.md) |

**Perché separare M5.1a da M5.1b:** se il ferro non produce, si cerca il problema in tre edifici e tre risorse; la competizione per il carbone entra solo dopo aver verificato le regole di base. È un ordine di lavoro proposto, non una nuova meccanica.

Progressione ora implementata: **12 edifici / 9 risorse** in M4, **15 / 12** in M5.1a e **17 / 14** in M5.1b. Forgia e Zecca sono fasi successive con parametri ancora da scegliere.

## Ipotesi numerica A — Piccola filiera misurabile

L'unità è **una risorsa per secondo per lavoratore** salvo indicazione diversa. Il costo proposto è in **legno lavorato / pietra** per una struttura; gli edifici producono solo con **1 lavoratore assegnato** e sono acquistabili con i comandi esistenti. I valori non vanno copiati in `CivilGrowthScenario` senza approvazione.

| Edificio candidato | Costo proposto | Ricetta per lavoratore e secondo | Addetti | Prerequisito candidato |
| --- | ---: | --- | ---: | --- |
| Miniera di ferro | **120 / 80** | → 2 minerale ferro | 1 | Casa grande |
| Miniera di carbone | **130 / 90** | → 2 carbone | 1 | Casa grande |
| Fonderia di ferro | **200 / 150** | 2 minerale ferro + 1 carbone → 1 ferro | 1 | Miniera ferro + miniera carbone |
| Miniera d'oro | **180 / 120** | → 1 minerale oro | 1 | Casa grande |
| Fonderia d'oro | **220 / 170** | 1 minerale oro + 1 carbone → 1 oro | 1 | Miniera oro + miniera carbone |

**I costi, le ricette e i prerequisiti dei cinque edifici A sono scelti e implementati.** Le scelte di Ferro e Oro sono distinte; B non è stata adottata. Il ferro ha priorità con carbone scarso.

### Capacità teoriche, costi e personale

| Configurazione A | Acquisto iniziale aggiuntivo | Lavoratori | Capacità teorica massima |
| --- | ---: | ---: | --- |
| 1 miniera ferro + 1 miniera carbone + 1 fonderia ferro | **450 legno / 320 pietra** | **3** | 1 ferro/s, con fino a 1 carbone/s residuo |
| Configurazione sopra + 1 miniera oro + 1 fonderia oro | **850 legno / 610 pietra** totali | **5** | 1 ferro/s + 1 oro/s; carbone interamente impegnato |
| Configurazione precedente + secondo trio ferro/carbone/fonderia | **1.300 legno / 930 pietra** totali | **8** | 2 ferro/s + 1 oro/s, teoricamente; 1 carbone/s residuo |

Le somme sono verificate dalla tabella dei cinque prezzi proposti: primo trio 120+130+200 = **450**, 80+90+150 = **320**; seconda filiera oro +180+220 = **850** e +120+170 = **610**; secondo trio +450/320 = **1.300/930**. Un prezzo ipotetico non è un costo di gioco finché non viene approvato.

Con una sola miniera di carbone da 2/s, ferro e oro da una fonderia ciascuno usano insieme **2 carbone/s**. Se una delle due fonderie ha maggiore priorità, a scorte basse può ricevere carbone per prima. Il totale nominale non garantisce che entrambe lavorino a ogni passo: come per il Pane, la **produzione del passo** usa input disponibili all'inizio del passo, e le uscite sono utilizzabili solo nel passo successivo. Vanno misurati avviamento, priorità, saturazione delle scorte e attività reale.

### Ipotesi B — Filiera più costosa, senza cambiare le ricette

Per evitare di scegliere prezzi arbitrari senza criterio, il confronto alternativo B applica un **moltiplicatore indicativo 1,5×** soltanto ai *costi* delle cinque strutture A; i rendimenti e il personale restano gli stessi. Il trio ferro costerebbe **675 legno / 480 pietra**; ferro+oro **1.275 / 915**. Questo non crea maggiore complessità nel motore, ma rallenta l'acquisto con costi di materiali civili superiori. Il rapporto temporale non può essere dedotto dai soli prezzi: servono cronologie e stock iniziali realistici.

**Criterio per scegliere A o B dopo D0:** prima misurare il tempo per finanziare il trio ferro dopo lo sblocco, poi la differenza fra strategia con una sola miniera di carbone e strategia che ne costruisce una seconda, infine verificare se il costo del primo metallo compete davvero con case, Cibo e Pane. Non decidere usando soltanto il numero di edifici.

## Deposito e capacità: problema da non rimandare

Oggi il catalogo civile ha capacità di base pari a **1.000 unità** per le risorse correnti, salvo **Pane 100**; ogni Magazzino aggiunge **500** per ciascuna risorsa. L'estensione industriale deve scegliere esplicitamente:

1. Capacità di base delle cinque nuove risorse. **Candidato semplice: 1.000 ciascuna**, come molte delle risorse civili attuali, solo a titolo di prova; il riferimento grafico non ne stabilisce una quantità.
2. Se il Magazzino esistente aggiunge automaticamente 500 anche alle risorse nuove: **proposta sì**, perché mantiene un solo modello di deposito, ma altera il contenuto del catalogo e richiede test di compatibilità.
3. Se esistano davvero depositi di taglia media/grande separati: il riferimento ne mostra tre livelli, ma **non è approvato** il loro ingresso nel gioco; non inventare livelli per aggirare il limite delle scorte.
4. Effetto delle scorte iniziali a zero: le miniere riforniscono le fonderie dal **secondo successivo**. Un benchmark deve esplicitare il tempo di avviamento e non presentare la capacità nominale come produzione immediata.

Con capacità base 1.000 e output massimo 2/s da una miniera, il deposito relativo si riempirebbe da zero in **500 secondi** senza consumo (ipotizzando nessun altro produttore/consumatore). Questo è soltanto un **tempo aritmetico**, non una simulazione della partita. La GUI 8-bit dovrà esporre stock, spazio e motivo di fermo, senza ripetere il pianificatore del Core.

## Versione delle regole e salvataggi: decisione prima del codice

**S1 scelta dall'utente e verificata in M5.1a:** `civil-growth-6`, snapshot SQLite **v2**, nuove risorse e righe industriali a zero. Conversione esplicita da `-5`, concatenata da versioni precedenti, con backup prima della prima scrittura. La tabella conserva le due alternative originarie:

| Opzione | Pro | Contro | Stato |
| --- | --- | --- | --- |
| **S1. Evolvere lo scenario `civil-growth`** con una nuova versione di regole e nuovi contenuti | Conserva l'ID di scenario e il percorso di ripresa; conversione M4 → M5 relativamente lineare | Nome “civile” meno rappresentativo del gioco completo; conversione da valutare per tutti i vecchi checkpoint | **Preferenza tecnica da discutere** |
| **S2. Nuovo scenario campagna** con importazione esplicita di partite civili | Nome e identità finali più appropriati; il civile può restare dimostrativo | Nuovo slot e trasformazione tra scenari, UI e salvataggi più complessi | Alternativa da valutare |

S2 non è stata scelta. La compatibilità effettiva S1 è documentata nel [formato dei salvataggi](../SAVE_FORMAT.md) e nell'[ADR 0014](../adr/0014-first-iron-industry-and-civil-save-upgrade.md); nuovi stati militari/navali richiederanno verifiche proprie.

## Casi di prova e gate per ogni micro-incremento

### M5.1a — Ferro

- Catalogo e prerequisiti raggiungibili; miniera e fonderia non formano cicli né necessitano di ferro per costruire il primo ferro.
- Acquisto con risorse insufficienti: nessun cambio allo stato.
- Personale insufficiente: produzione nulla senza consumare input.
- Scorte iniziali zero: la fonderia non usa il minerale prodotto nello stesso secondo.
- 1.800 secondi di benchmark: scorte e produzioni riconciliate secondo per secondo, nessuna risorsa negativa e capacità mai oltre il limite.
- Round trip SQLite e migrazioni da `civil-growth-1` / `-2` / `-3` / `-4` / `-5`, originali protetti, nessuna produzione offline. L'eventuale `-6` dovrà essere approvato prima di esistere.
- UI: M5.1a introduce esattamente **3 nuovi tipi di edificio e 3 risorse**, con nomi, costo, produzione e fermo leggibili. Niente schermate di missione anticipate.

### M5.1b — Oro

- Due nuove strutture, due nuove risorse; domande di personale e prerequisiti coerenti.
- Contesa sul carbone da 2/s: ferro e oro mantengono output previsto secondo la priorità già esplicita del catalogo; modificare le priorità è una nuova decisione, non un effetto accidentale dell'ordine dei file.
- Aumentare l'estrazione di carbone deve rimuovere il collo di bottiglia previsto, salvo altri limiti identificati.
- Ripresa a scorte parziali e deposito pieno, salvataggi e diagnostica integri.

### M5.1c — Bilanciamento

| Misura | Perché serve |
| --- | --- |
| Secondi per primo ferro e primo oro effettivi | Controlla l'avviamento delle catene |
| Ferro/s, oro/s e carbone/s misurati | Distingue limite teorico e produzione reale |
| Secondi di blocco per input / magazzino | Diagnosi dei colli di bottiglia |
| Costo per unità/s realmente ottenuta | Confronta acquisto di una seconda miniera rispetto a una seconda fonderia |
| Lavoratori totali e liberi, Case e Pane | Verifica che l'industria non blocchi la crescita civile |
| Secondi necessari a finanziare i costi A e B | Consente una scelta quantitativa dei prezzi |

**Gate di uscita M5.1:** almeno una sequenza di estrazione/fusione da risorse civili è riproducibile; nessun sblocco dipende da una risorsa non ancora disponibile; salvataggi migrati e testati; valori economici fissati *dopo* un confronto misurato. La prova separata A/B fornisce misure da una colonia sintetica; **le prove del gioco integrato e delle migrazioni restano future**. L'esperimento non chiude M4, D0 o il gate M5.1.

## Prima misura A/B effettiva

La [prova industriale](../../experiments/industry-probe/README.md) parte da 40 abitanti già alloggiati e alimentati, produzioni civili predisposte, materiali e metalli a zero. Sotto acquisti progressivi il primo ferro arriva a **452 s per A** e **677 s per B**; aggiungendo la filiera oro, il primo oro arriva a **852/1.277 s**. Si tratta di tempi successivi all'avvio sintetico, senza il costo di preparazione civile.

La politica di accumulare tutto il budget, invece, porta il primo ferro/oro B a **1.378 s**, con un Magazzino acquistato a 101 s e spesa totale **1.375/975**. L'acquisto progressivo finanzia le medesime cinque strutture a **1.275/915** senza quel Magazzino: **un costo totale oltre 1.000 non implica che ogni politica debba accumularlo simultaneamente**. I risultati della prova misurano strategie dichiarate, non un ottimo né la durata della campagna.

## Handoff e ordine operativo

1. **Evidenza acquisita:** benchmark M4 letti e gate tecnico superato; prova industriale A/B separata eseguita, senza nuove regole nel desktop.
2. **M4 chiusa:** prototipo approvato, demolizione rinviata, Carne fuori dalla prima campagna.
3. **M5.1a verificata:** configurazione A e S1 scelte; primo ferro da nuova partita a 1.102 s, backup e ripresa esatti.
4. **M5.1b verificata:** parametri A dell'Oro adottati nelle regole v7, primo Oro da nuova partita a 1.502 s. Prossimo incremento M5.1c per scala/contesa; contratti Spade/Monete e militari/navali prima delle rispettive fasi.
