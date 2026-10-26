# Riferimenti visivi

Nove immagini fornite dall'utente e analizzate il 2026-10-26: quattro screenshot iniziali, due viste delle risorse e tre diagrammi delle filiere. I primi quattro originali sono stati trasferiti dalla cartella `screnshot/`; le cinque nuove immagini sono state aggiunte direttamente a `screenshots/`. Tutte sono conservate senza modifiche.

I nomi dei file e il testo delle schermate rimangono quelli originali. Queste immagini documentano un riferimento di gioco e interfaccia; non sono asset della nuova applicazione né una specifica completa delle meccaniche.

## Inventario

| Immagine | Contenuto |
| --- | --- |
| [Scheda Big House](screenshots/Screenshot_2023-05-05-23-31-13-455_d.android.settlers1.jpg) | Dettaglio di un'abitazione con costo, abitanti, capacità, consumo e produzione |
| [Economia iniziale e abitazioni](screenshots/Screenshot_2023-11-29-06-54-05-770_d.android.settlers1.jpg) | Sede iniziale, legname, pietra, case, depositi e prima alimentazione |
| [Alimentazione e metallurgia](screenshots/Screenshot_2023-11-29-06-54-14-226_d.android.settlers1.jpg) | Mulino, panificio, pozzo, allevamento, depositi alimentari, miniere e fusione |
| [Esercito e isole](screenshots/Screenshot_2023-11-29-06-54-19-657_d.android.settlers1.jpg) | Armi, caserma, castello, cantiere, spedizioni, piantagioni e navi |
| [Risorse — vista completa](screenshots/risorse1.png) | Abitanti, tronchi, tavole, pietra, cibo, carne e pesce: variazione, quantità e capacità |
| [Risorse — dettaglio](screenshots/risorse2.png) | Ingrandimento di abitanti, tronchi, tavole e pietra; non uno stato di gioco indipendente |
| [Filiere civili](screenshots/struttura1_timeofexploration.png) | Materiali, depositi, abitazioni, agricoltura e alimentazione |
| [Monete, soldati e colonie](screenshots/struttura2_timeofexploration.png) | Oro, carbone, ferro, armi, popolazione, navi, spedizioni e commercio |
| [Strutture decorative](screenshots/struttura3_timeofexploration.png) | Tre catene di tre monumenti o attrazioni ciascuna |

L'[analisi di risorse e filiere](RESOURCE_AND_PRODUCTION_CHAINS.md) trascrive le nuove evidenze e distingue collegamenti mostrati, ambiguità e conseguenze per il progetto.

## Evidenze direttamente visibili

- Navigazione principale con Buildings, Resources e Score. I primi quattro screenshot mostrano Buildings; `risorse1.png` e `risorse2.png` mostrano Resources con etichette tedesche.
- Una riga per tipo con icona, nome, quantità e pulsanti meno/più.
- Quantità elevate: 940 Tree Feller, 470 Sawmill e 1.073 Stonecutter nella schermata dell'economia iniziale.
- Case e depositi di più dimensioni; depositi alimentari distinti da quelli generici.
- Presenza di caserma, torre, castello, cantiere, spedizioni, crociate, piantagioni e navi specializzate.
- Alcuni nomi in rosso, comandi disabilitati e la dicitura `Buildings (100%)`, senza una spiegazione del loro significato.
- Icone monocromatiche con tratto disegnato e sfondo scuro.

## Scheda Big House

| Campo mostrato | Valore |
| --- | --- |
| Titolo | Big House (102) |
| Building costs | Stones: 6K; Wood: 4,5K |
| Building resources | Settlers: 30 |
| Recycling resources | Stones: 3K; Wood: 2,2K |
| Store capacity | Settlers: +50 |
| Consumption per minute | Food: 20 |
| Production per minute | Settlers: 0,1 |
| Productivity | 100% |

La descrizione dichiara che l'edificio fornisce 30 abitanti, aumenta limite e crescita della popolazione e richiede cibo periodico. Capacità, popolazione iniziale e crescita sono quindi concetti distinti nella scheda.

I valori sopra sono trascrizioni, non parametri approvati per SimRisorse. L'immagine non basta a dimostrare tutte le regole di applicazione o aggregazione dei tassi.

## Interpretazioni da verificare

- Il rapporto 940:470 suggerisce una relazione tra taglialegna e segherie, ma non dimostra che il rapporto produttivo corretto sia 2:1.
- I primi screenshot non mostravano le ricette. I nuovi diagrammi esplicitano collegamenti come cereali → mulino → farina → panificio, con acqua → pane; quantità e durate restano assenti.
- Recycling resources suggerisce un recupero di materiali, ma non definisce integralmente gli effetti della demolizione su persone e capacità. Il rimborso visualizzato è circa metà del costo, senza dimostrare una percentuale generale esatta.
- Expedition e Crusade compaiono nello stesso elenco degli edifici. Non è possibile stabilire dalle immagini se il numero rappresenti missioni svolte, capacità, obiettivi posseduti o altro.
- Il diagramma delle colonie collega piantagioni, prodotti e navi specializzate alle monete. Restano da verificare costi, tempi e requisiti concreti di spedizioni e conquiste.

Non attribuire al rosso un significato specifico, non interpretare `Buildings (100%)` come prova di completamento e non assumere che tutti i pulsanti meno eseguano la stessa operazione.

## Applicazione al progetto

Confermato dall'utente: gestione tramite elenco, lavoratori e successiva progressione militare con isole. Proposte derivate dall'analisi: dettaglio persistente su desktop, categorie e filtri, flussi netti, motivi di blocco e operazioni multiple.

Il catalogo definitivo, l'aspetto grafico e i valori economici verranno progettati per SimRisorse. Vedere [game design](../GAME_DESIGN.md) e [ADR del catalogo](../adr/0003-versioned-content-catalog.md).
