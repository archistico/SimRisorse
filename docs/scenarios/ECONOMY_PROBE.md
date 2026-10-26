# Scenario economico della prova tecnica M1

Stato: regole operative provvisorie per la sola prova tecnica, definite il 2026-10-26. Non costituiscono il catalogo finale e non modificano l'obiettivo di otto ore del gioco completo.

## Identità e dotazione

- Scenario: `economy-probe`.
- Regole: `probe-1`; formato del salvataggio: `1`.
- Otto abitanti, senza crescita o mantenimento alimentare in questa prova.
- Scorte iniziali: 0 tronchi, 120 legno lavorato, 80 pietra.
- Capacità: 1.000 unità per ciascuna risorsa.
- Un edificio per tipo, ciascuno con un lavoratore assegnato; cinque persone disponibili.
- Nuove partite e caricamenti iniziano in pausa.

## Catalogo ridotto

| Tipo | Costo di un nuovo edificio | Posti per edificio | Ricetta per lavoratore e secondo |
| --- | --- | --- | --- |
| Taglialegna | 20 legno, 10 pietra | 1 | Produce 2 tronchi |
| Segheria | 40 legno, 20 pietra | 1 | Consuma 2 tronchi, produce 1 legno |
| Tagliapietra | 30 legno, 10 pietra | 1 | Produce 1 pietra |

I tre edifici iniziali fanno parte della dotazione, non vengono pagati. L'acquisto è immediato, paga l'intero costo o non modifica nulla e crea un posto di lavoro vuoto. Nessun lavoratore viene assegnato automaticamente ai nuovi edifici.

L'assegnazione è manuale, una persona per comando, limitata dai posti e dagli abitanti disponibili. La produttività dipende dai lavoratori assegnati; edifici vuoti non producono. Nessuna demolizione nella prova.

## Avanzamento e materiali

- Velocità desktop: 1 secondo reale = 1 secondo di gioco mentre la partita è attiva.
- Passo economico: 1 secondo; il tempo residuo inferiore al secondo viene conservato anche nel salvataggio.
- Quantità di risorse intere rappresentate con `long`; persone e quantità di edifici intere. Questa scelta non decide la precisione del futuro Core.
- Le lavorazioni usano esclusivamente gli input presenti all'inizio del passo. Gli output diventano disponibili nel passo successivo.
- Ogni lavoratore esegue una ricetta completa. Con input insufficienti eseguono la ricetta solo i lavoratori servibili; nessuna frazione di ricetta.
- A magazzino pieno la produzione corrispondente si ferma e non consuma input inutilmente. Anche lo spazio viene valutato all'inizio del passo: quello liberato dal consumo sarà disponibile al passo successivo.
- Ordine di prenotazione deterministico: taglialegna, segheria, tagliapietra. Nel catalogo ridotto una sola ricetta consuma tronchi; la politica definitiva di concorrenza tra filiere rimane aperta.
- Assegnazioni e acquisti sono consentiti in pausa, senza avanzare il tempo. Si applicano dopo gli eventuali passi già maturati.

La minimizzazione ferma la prova. La perdita del focus non la ferma. Un intervallo del timer superiore a due secondi viene trattato come interruzione/sospensione: pausa senza recuperarlo. È una protezione semplice della prova, da sostituire con una gestione completa degli eventi di sospensione nel desktop definitivo.

## Esempi verificabili

Con dotazione iniziale e un lavoratore per tipo, senza ordini:

| Tempo simulato | Tronchi | Legno | Pietra |
| --- | --- | --- | --- |
| 0 secondi | 0 | 120 | 80 |
| 1 secondo | 2 | 120 | 81 |
| 2 secondi | 2 | 121 | 82 |
| 60 secondi | 2 | 179 | 140 |

Due avanzamenti da mezzo secondo equivalgono a uno da un secondo. I materiali prodotti dal taglialegna al primo passo non alimentano la segheria nello stesso passo.

Esempi di errore: comprare una segheria senza 40 legno e 20 pietra lascia tutto invariato; assegnare una persona oltre i posti o oltre gli otto abitanti non modifica gli assegnati.

## Salvataggi e verifica dello stack

La prova conserva un singolo slot SQLite con metadati e stato serializzato, scritti nella stessa transazione. L'errore sulla seconda scrittura deve annullare anche la prima. Al caricamento si controllano formato, regole, quantità e assegnazioni prima di sostituire la partita attiva.

La UI presenta elenco, dettaglio, pausa, assegnazioni, costruzione, salvataggio e caricamento. La prova è in [experiments/stack-probe](../../experiments/stack-probe/README.md), separata dalla futura soluzione definitiva.

## Questioni risolte solo per questa prova

Q-03: acquisti immediati. Q-04: velocità 1:1 e ordini in pausa. Q-06: assegnazioni manuali con ricette complete per lavoratore. Q-07: scorte iniziali al passo, blocco per input/spazio e priorità stabile. Le regole generali restano nel [game design](../GAME_DESIGN.md) e saranno aggiornate dopo la valutazione del prototipo.
