# Documentazione di SimRisorse

Aggiornata il **2026-10-28**. Stato corrente: **M4 chiusa, M5.1a Ferro e M5.1b Oro verificate nel progetto locale**, regole `civil-growth-7`, 17 edifici e 14 risorse. Test **v33 confermati passati dall'utente**; output del gate M5.1c non acquisito separatamente. Ultimo rapporto dettagliato: **206/206 test**; v33 con due test Desktop ha **208 casi attesi**, v35 con un caso aggiuntivo ne ha **209**, v36 con due casi UI sulle scrollbar e sulla selezione ne ha **211, ancora da verificare**. **Il contratto M5.2 A è approvato nel design, non implementato.** Il [README principale](../README.md) presenta il gioco e l'avvio rapido.

## Documenti correnti

| Documento | Quando leggerlo |
| --- | --- |
| [Handoff](HANDOFF.md) | Riprendere il lavoro: stato verificato, scelte ricevute e prossimo incremento |
| [Roadmap](ROADMAP.md) | Consultare milestone, dipendenze e criteri di completamento |
| [Game design](GAME_DESIGN.md) | Distinguere regole confermate e proposte |
| [Architettura](ARCHITECTURE.md) | Comprendere confini e dipendenze del codice |
| [Registro ADR](adr/README.md) | Consultare le decisioni tecniche e le motivazioni |
| [Sviluppo](DEVELOPMENT.md) | Eseguire build, test, simulazioni e benchmark |
| [Salvataggi](SAVE_FORMAT.md) | Gestire formato, conversioni, backup e recupero |
| [Interfaccia retro](PIXEL_ART_UI.md) | Consultare layout, font, icone e stati dei controlli |
| [Screenshot del progetto](screenshots/README.md) | Conoscere provenienza e aggiornamento dell'immagine nel README |

Le pagine operative descrivono il presente. Le misure dei singoli gate nei rapporti seguenti conservano data, conteggi e limiti dell'esecuzione originale.

## Decisioni di campagna e contenuti futuri

| Documento | Stato e contenuto |
| --- | --- |
| [Prima campagna](design/FIRST_CAMPAIGN_PROPOSAL.md) | Catalogo scelto di 22 tipi, vittoria e ruolo della conquista; cinque tipi ancora futuri |
| [Scheda D0](design/D0_DECISION_BOARD.md) | Scelte già ricevute e contratti ancora da definire |
| [Dossier preparatorio D0](design/PRE_M5_DECISION_GATE.md) | Inventario dei 43 riferimenti e ragionamento sulle alternative |
| [M5.1 Industria](design/M5_1_INDUSTRY_PREFEASIBILITY.md) | Studio iniziale; Ferro e Oro implementati, benchmark scala/contesa preparato |
| [M5.2 Forgia e Zecca](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) | **Contratto A approvato il 28/10**, non ancora implementato; confronti A/B storici, carbone a fasi/parallelo, 241/122 s teorici, test F1–F10 e salvataggi |
| [M5.3 Esercito](design/M5_3_ARMY_PREFEASIBILITY.md) | Ipotesi su reclute, soldati, addestramento e conservazione della popolazione |
| [M5.4 Navigazione](design/M5_4_NAVAL_PREFEASIBILITY.md) | Ipotesi su Cantiere, navi, trasporti e primo viaggio |
| [Esperimento industriale A/B](../experiments/industry-probe/README.md) | Prova separata su colonia sintetica v5, senza cambiare i parametri del gioco |
| [Esperimento M5.2 Forgia/Zecca](../experiments/forge-mint-probe/README.md) | Sei profili A/B e sei controlli F8/F9 su capienza/priorità, codice sperimentale **ancora da eseguire** |

## Scenari e modelli economici

- [Economy Foundation](scenarios/ECONOMY_FOUNDATION.md): ciclo economico di riferimento del Core.
- [Crescita civile](scenarios/CIVIL_GROWTH.md): abitazioni, crescita, Cibo e Pane; [bozza originaria](scenarios/CIVIL_GROWTH_DRAFT.md).
- [Case grandi e Pane](scenarios/ADVANCED_HOUSING_PROPOSAL.md): proposta storica che precede l'implementazione.
- [Confronto abitazioni](scenarios/HOUSING_BENCHMARK.md) e [audit degli incentivi](scenarios/HOUSING_INCENTIVE_AUDIT.md).
- [Scala della filiera Pane](scenarios/BREAD_SCALE_BENCHMARK.md), [alta densità](scenarios/BREAD_HIGH_DENSITY_BENCHMARK.md) e [capacità nominale](scenarios/BREAD_CHAIN_CAPACITY.md).
- [Carne](scenarios/MEAT_DESIGN_REVIEW.md): fuori dalla prima campagna.
- [Demolizione](scenarios/DEMOLITION_M4_REVIEW.md): studio conservato; implementazione rinviata e rimborso non scelto.
- [Prova economica M1](scenarios/ECONOMY_PROBE.md): scenario dell'esperimento iniziale.

## Verifiche

| Rapporto | Evidenza |
| --- | --- |
| [Interfaccia corrente](validation/DESKTOP_UI.md) | Ultimo gate da **206 test**, layout, icone, font e hover ocra |
| [M5.1c Carbone/scala](validation/M5_1C_INDUSTRY_SCALE.md) | Protocollo di cinque profili e valori teorici; v33 dichiarata passata dall'utente, log dettagliato non acquisito |
| [M5.1b Oro](validation/M5_1B_GOLD.md) | Gate di integrazione da 204 test, conversione v6→v7, priorità del Carbone e primo Oro |
| [M5.1a Ferro](validation/M5_1A_IRON.md) | Gate da 191 test, prima industria e conversione S1 |
| [Risultati misurati M4](validation/M4_MEASURED_RESULTS.md) | Quattro benchmark eseguiti e render ispezionati |
| [Lettore dei benchmark](validation/M4_BENCHMARK_READER.md) | Riprodurre il rapporto dai JSON conservati |
| [Cronologia M4](validation/M4_CIVIL_GROWTH.md) | Evoluzione e verifiche della crescita civile |
| [M3 Desktop](validation/M3_DESKTOP.md) | Prima partita salvabile e avvio nativo |
| [Migrazione Avalonia 12](validation/AVALONIA_12.md) | Baseline dei pacchetti e controllo delle dipendenze alla migrazione |
| [M2 Core](validation/M2_CORE.md) | Motore autonomo e simulazioni lunghe |
| [M1 Stack](validation/M1_STACK_PROBE.md) | Prova iniziale dello stack desktop |

## Riferimenti e storico

- [Immagini del gioco di riferimento](references/README.md) e [analisi delle filiere](references/RESOURCE_AND_PRODUCTION_CHAINS.md): materiale di studio distinto dagli screenshot dell'applicazione.
- [Changelog](../CHANGELOG.md): modifiche realizzate, ordinate per incremento.
- [Handoff storico](HANDOFF_HISTORY.md): punti di ripresa precedenti, con le limitazioni note in quel momento.
