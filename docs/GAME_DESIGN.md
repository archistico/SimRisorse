# SimRisorse — game design e direzione di sviluppo

## Obiettivo

Gestionale locale nel quale si sviluppano produzione, popolazione, alimentazione, ricerca, industria e capacità militare. Interfaccia **pixel art retro 8-bit essenziale, basata su elenchi e schede**, senza mappa di collocazione degli edifici. In futuro il percorso comprende flotta, esplorazione, conquista e Castello, prima della condizione di vittoria.

## Situazione reale del gioco

La **RC76-HF04** include una campagna v15 con Deposito alimentare a cinque ingredienti, edifici produttivi con tier Base/Grande/Avanzato, Scuola, ricerca, militari e SQLite generation 6. La GUI conserva il layout canonico con le modifiche HF04 richieste dall'utente. **885/885 test verificati**, gate automatico superato; confronto interattivo Windows ancora da confermare. [Esiti HF04](validation/RC76_HF04_ICONS_AND_GROWTH.md).

## Regole già stabilite

- Popolazione e personale condividono il budget: lavoratori, studenti, ricercatori e militari non possono essere conteggiati due volte. In v15 ogni casa piccola oltre la prima aggiunge +15% e ogni casa grande +30% alla crescita, fino a **×20**; al massimo un abitante ogni 3 secondi, con alloggi e alimentazione sufficienti.
- Municipio come punto di avvio e capacità minima, edifici e magazzini contribuiscono allo stoccaggio; dettagli consultabili in GUI.
- Cibo pronto creato dal **Deposito alimentare**, con Pesce, Pane, Carne, Frutta e Verdura. Pane sostiene gli abitanti avanzati; consumi alimentari atomici.
- Ricerca libera: punti accumulati, tecnologie acquistate volontariamente e livelli edilizi direttamente costruibili o migliorabili.
- GUI v14 **vincolante**; tutte le funzioni future devono adattarsi a quella presentazione senza reintrodurre la GUI experimental semplificata.

## Obiettivi approvati, parametri in studio

- Progressione **molto più impegnativa**, quasi esponenziale per complessità: la prima Zecca non deve essere accessibile con sole 210 unità Legno lavorato e 160 Pietra.
- Investimento di una Zecca da confrontare con circa **100–1.000 Tagliapietre base equivalenti**; criterio qualitativo di investimento complessivo, non prezzo definitivo.
- **Ogni Zecca necessita di 10 lavoratori per iniziare a produrre**; due Zecche per il pieno regime richiedono 20. Il modello dei tier e la capienza lavoratori vanno adattati e benchmarkati.
- Prima i benchmark v15 E4a, **poi** proposta quantitativa e implementazione E4b. Evitare softlock dovuti a storage, personale, input e ricerca.
- Esplorazione distinta dalla conquista; conquista deterministica e inizialmente senza perdite irreversibili; navale e Castello soltanto dopo il consolidamento economico.

Le durate indicative di **8 ore** per una partita efficiente e **32+ ore** per una molto inefficiente sono obiettivi di design storici, **non misure validate**.

[Roadmap completa](ROADMAP.md) · [Specifiche E4b](design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md) · [Decisioni](AUTHORITY_REGISTRY.md).
