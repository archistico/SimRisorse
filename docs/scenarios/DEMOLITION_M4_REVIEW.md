# M4 — Demolizione: capacità, rimborsi e recuperabilità

Data: 2026-10-27. **Stato: proposta quantitativa e di coerenza; nessuna meccanica approvata o implementata.** Non modificare `civil-growth-4` o i salvataggi sulla base di questa analisi.

**Decisione successiva dell'utente:** demolizione **rinviata a un incremento successivo**, consentendo la chiusura M4. Le regole correnti sono `civil-growth-6` con ferro/carbone; rimborso del 25% e politiche delle capacità qui studiate restano proposte non adottate.

## Perché serve una decisione prima del codice

Le strutture non sono soltanto investimenti: possono ospitare popolazione, impiegare lavoratori, aumentare la capacità delle risorse o sbloccare altre costruzioni. Una demolizione senza condizioni potrebbe lasciare 20 abitanti in 16 posti, far sparire scorte oltre il limite o trattenere lavoratori in edifici inesistenti. Q-08 del game design rimane aperta.

## Politiche candidate

| Politica | Rimborsi | Vantaggi | Svantaggi |
| --- | --- | --- | --- |
| A — Senza rimborso | 0% | Regola semplice e senza possibilità di recuperare risorse | Molto punitiva per errori di costruzione |
| **B — Rimborso parziale** | **25% del costo originale**, arrotondato alla precisione 0,001 | Permette di correggere una strategia perdendo comunque risorse | Serve controllare che il rimborso non superi i nuovi depositi |
| C — Rimborso elevato | 50% | Sperimentazione più libera | Riduce il costo delle scelte e la pressione del bilanciamento |

**Raccomandazione da discutere: B**, non una decisione già presa. Ricostruire dopo aver demolito costa sempre almeno il 75% dei materiali originali: non si creano risorse ripetendo il ciclo.

## Esempi con le regole correnti

| Struttura (1 unità) | Costo: tavole / pietra | Recupero ipotetico al 25% | Capacità o effetto perso |
| --- | --- | --- | --- |
| Casa piccola | 80 / 40 | 20 / 10 | 8 posti |
| Casa grande | 160 / 80 | 40 / 20 | 32 posti |
| Magazzino | 100 / 60 | 25 / 15 | 500 di capacità per ciascuna risorsa |
| Forno | 150 / 100 | 37,5 / 25 | Fino a 3 Pane/s, se assegnato e rifornito |

Il recupero non dipenderebbe da quanto tempo l'edificio ha lavorato. L'eventuale saldo delle scorte cambia solo per il rimborso; la demolizione non consuma Cibo o Pane né anticipa la crescita.

## Invarianti proposte per un'operazione atomica

1. **Quantità positiva e posseduta:** non demolire più edifici di quelli presenti.
2. **Abitazioni:** rifiutare l'operazione se la nuova capacità totale risulta inferiore alla popolazione presente. Nessuno sfratto, decesso o abitante senza posto introdotto implicitamente.
3. **Magazzini:** verificare tutte le risorse rispetto alle capacità *dopo* la demolizione. Rifiutare se le scorte — incluso l'eventuale rimborso — eccedono un limite. Nessuna perdita silenziosa o vendita automatica.
4. **Lavoratori:** se diminuiscono i posti disponibili nel tipo demolito, liberare soltanto i lavoratori eccedenti; gli altri rimangono assegnati. Nessuna modifica alla popolazione.
5. **Prerequisiti:** demolire un prerequisito già usato non cancella gli edifici dipendenti. Le nuove costruzioni tornano invece soggette al prerequisito. Un Forno distrutto può fermare la produzione di Pane e quindi la crescita: va segnalato.
6. **Atomicità:** se uno solo dei controlli fallisce, non cambiano quantità, personale, scorte, capacità, popolazione o progresso.
7. **Pausa e salvataggi:** l'ordine sarebbe consentito in pausa come la costruzione. Uno snapshot registrerebbe semplicemente i nuovi conteggi e lavoratori, senza nuovi campi; l'introduzione della regola di rimborso richiederebbe comunque una versione di regole dedicata.

## Casi che meritano prove automatiche

| Scenario | Esito proposto |
| --- | --- |
| 16 abitanti, due Case piccole, nessuna Casa grande; demolire una Casa piccola | Rifiuto, 8 posti non bastano |
| 16 abitanti, due Case piccole e una Casa grande; demolire una Casa piccola | Ammesso, 40 posti residui |
| 600 Pane conservati con un Magazzino; demolire l'unico Magazzino | Rifiuto, tornerebbe il limite base 100 |
| 90 Pane e recupero di tavole/pietra entro i nuovi limiti; demolire Magazzino | Ammesso, capacità ridotte e nessuna scorta persa |
| Tre edifici con tre lavoratori, demolirne uno e lasciarne due | Un lavoratore liberato, due rimangono |
| Edificio mai costruito, quantità zero o negative, materiali di rimborso oltre capacità | Rifiuto senza mutazioni |
| Demolire l'unico Forno con Casa grande ancora presente | Casa grande invariata; Pane non più prodotto finché il Forno non viene ricostruito |
| Salvare/ricaricare dopo demolizione, anche con crescita a 59 s | Stesse scorte, addetti, popolazione e progresso; ripresa in pausa |

## Rischio strategico e momento di implementazione

Il recupero del 25% aiuta a correggere errori senza rendere gratuite le scelte. Resta però il rischio di **blocco della filiera iniziale** se il giocatore elimina l'ultimo produttore indispensabile e non conserva materiali per ricostruirlo. Le alternative sono: bloccare la demolizione dell'ultima struttura critica, offrire un avviso con conferma oppure accettare la possibilità di una partita bloccata e permettere un riavvio chiaro. **Non scegliere automaticamente**: cambia il livello di difficoltà.

Prima dell'implementazione: (a) superare il gate `civil-growth-4`, (b) confermare rimborso e comportamento degli ultimi produttori, (c) preparare una versione delle regole e test Core/Application/SQLite/Desktop. La futura GUI 8-bit deve mostrare la perdita di capacità e i rimborsi *prima* che l'ordine venga confermato.
