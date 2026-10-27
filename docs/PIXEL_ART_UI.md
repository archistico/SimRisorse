# Direzione grafica — pixel art retro 8-bit essenziale

Decisione confermata il 2026-10-27 dall'utente. **Stato: primo prototipo implementato in v25/M4.11, non ancora validato su Windows.** La finestra Avalonia attuale rimane una base funzionale: pittogrammi e pannelli retro sono sperimentali; non cambiano motore, ricette, bilanciamento o salvataggi.

## Identità e priorità

- Stile **retro 8-bit essenziale**: icone quadrate, contorni netti, pochi colori ben differenziati e pannelli discreti, senza texture complesse o decorazioni isometriche obbligatorie.
- Il valore principale della schermata rimane **capire subito cosa produce, cosa manca e cosa conviene costruire**. La grafica non deve nascondere quantità, unità `/s`, assegnazioni e costi.
- Edifici selezionati da **elenco o schede per categoria**: nessun posizionamento su una mappa. Un futuro panorama dell'isola, se approvato separatamente, sarà decorativo e non vincolerà le regole del gioco.
- Evitare la copia di icone o sprite dagli screenshot di riferimento: servono risorse grafiche originali o con licenza compatibile e attribuzione verificata.
- **Stabilità geometrica degli stati:** hover, pressione, focus e disabilitazione non devono cambiare dimensioni, padding o spessore dei bordi di pulsanti e schede. Cambiano soltanto colore, contrasto e altri elementi che non spostano i vicini. Il difetto osservato in v22 sul pulsante Costruisci viene coperto da un test geometrico v23.

## Vocabolario visivo candidato

| Elemento | Dimensione base proposta | Utilizzo |
| --- | --- | --- |
| Risorsa | 16 × 16 pixel | Barra delle scorte, costi, input e output; ingrandimento intero 2× o 3× |
| Edificio | 32 × 32 pixel | Elenco costruzioni; ingrandimento fino a 64 × 64 o 96 × 96 nel dettaglio |
| Indicatori di stato | 8 × 8 o 16 × 16 pixel | Attivo, senza lavoratori, mancano input, deposito pieno |
| Bordi dei pannelli | Moduli a multipli di 8 pixel | Griglie, pulsanti e schede con spaziatura regolare |

Le dimensioni sono **ipotesi da verificare con un prototipo**, non vincoli delle regole. Preferire PNG con trasparenza e scala a multipli interi; per gli sprite pixelati impostare l'interpolazione bitmap di Avalonia a `None` e controllare monitor con DPI differenti. Titoli e numeri devono essere leggibili: un font bitmap è candidato per intestazioni grandi, mentre testo lungo e tabelle possono usare un carattere da interfaccia ordinario.

Palette candidata, da validare con sprite e contrasti reali: sfondo blu notte `#1B1B2F`, pannelli ardesia `#303B50`, testo crema `#F2D49B`, funzionamento verde `#6AA85C`, avvisi corallo `#D66A57`, selezione azzurro `#66BED0`. Il colore non basta a spiegare un errore: usare sempre un'etichetta o una descrizione.

## Struttura delle schermate

1. **Sempre visibile:** abitanti/posti, lavoratori liberi, tempo di gioco, pausa e salvataggio; scorte principali con variazione netta `/s`.
2. **Edifici:** categorie, icona, quantità costruite, lavoratori impiegati, output del prossimo secondo e motivo di inattività. Acquisto e assegnazioni multipli accessibili senza aprire altre finestre.
3. **Dettaglio edificio:** figura ingrandita, costo totale per `+1/+5/+10`, prerequisiti, posti di lavoro, input/output per lavoratore, benefici di abitazione o magazzino e avvisi precisi.
4. **Risorse:** tutte le nove risorse M4 con quantità, capacità, produzione, consumo e saldo; nessuna informazione affidata esclusivamente a icone.
5. **Crescita M4:** capacità Case piccole/grandi, residenti avanzati, Cibo/Pane, progresso e differenza fra input del Forno mancanti e deposito pieno. Non perdere la diagnosi dello sblocco 16→17 abitanti.
6. **Estensioni future:** spazi di navigazione per Esercito, Navi, Isole e Obiettivi, disabilitati o assenti fino a M5/M6; non rappresentarli come funzionalità già presenti.

### Miglioria ergonomica già introdotta nella GUI provvisoria (v22)

Il prototipo definitivo 8-bit non è ancora sviluppato, ma la segnalazione dell'utente ha introdotto **tre requisiti pratici da preservare**:

- Il pulsante **Costruisci** deve mostrare stati nettamente diversi: disponibile, hover, premuto e disabilitato; lo stato disabilitato non deve sembrare un semplice hover.
- Il messaggio **«Per costruire servono prima…»** (e le altre diagnosi di blocco) deve stare in un riquadro visibile **senza scorrere il dettaglio**, immediatamente sopra i comandi di acquisto. La descrizione dell'edificio può invece scorrere.
- Margini, card, elenco e footer devono essere abbastanza compatti per lasciare posto a informazioni future, senza nascondere testo o ridurre i pulsanti a bersagli difficili da usare.

L'implementazione Avalonia v22 applica questi criteri alla **GUI attuale**, non alla futura palette pixel art. Due test geometrici a 1040×740 e 1240×850 e un test dello stato hover/disabilitato sono predisposti, **non ancora eseguiti su Windows**. Ripetere gli stessi controlli sul prototipo 8-bit.

## Identità delle risorse grafiche

Usare gli identificatori del catalogo come chiavi stabili per la scelta dello sprite, indipendentemente dai nomi italiani visibili. Prime nove icone: `logs`, `planks`, `stone`, `fish`, `food`, `grain`, `water`, `flour`, `bread`. Per gli edifici: prima prova su `small-house`, `large-house`, `farm`, `mill`, `bakery` e un produttore di Cibo. Separare un eventuale atlas grafico dai contenuti `GameCatalog`: le immagini non appartengono a Core o allo snapshot.

Per il Pane avanzato la UI riceve già `GameView.BreadSupply` da Application, con stati `Hidden`, `FirstAdvancedArrivalNeedsStock`, `NominalCapacityInsufficient`, `SufficientNextSecond`, `MissingInputs`, `StorageFull` e `LimitedNextSecond`. Ogni stato potrà avere un piccolo simbolo pixel art più una descrizione; la GUI non deve ripetere il calcolo. Anche quando mancano contemporaneamente Cibo e Pane prima del primo arrivo avanzato, l'avviso sul Pane resta visibile.

La proiezione espone inoltre domanda, **capacità nominale dei lavoratori assegnati**, **produzione realmente prevista nel prossimo secondo**, deficit nominale e, quando esiste una sola ricetta produttrice di Pane, numero minimo/addizionale di lavoratori necessari. La futura GUI 8-bit potrà affiancare una cifra a un'icona di carenza senza reinterpretare ricette e produttività. Il numero di lavoratori è una stima **nominale**: non dimostra che ci siano edifici/posti di lavoro disponibili né che Farina, Acqua e deposito sostengano la produzione. Prima del primo abitante avanzato la domanda attuale è zero e queste stime sono intenzionalmente nascoste.

Per la **filiera completa**, `BreadSupply.ChainCapacity` fornisce anche gli anelli produttivi Grano, Acqua, Farina e Pane, con fabbisogno, capacità nominale e deficit di ciascuno, più il limite teorico medio della catena. I quattro indicatori possono diventare piccole schede 8-bit; vanno distinti dal forecast effettivo del prossimo secondo. Il [modello e i limiti](scenarios/BREAD_CHAIN_CAPACITY.md) sono documentati senza anticipare la nuova grafica.

## Prototipo e criteri di accettazione

- **v25/M4.11:** la schermata civile impiega `PixelGlyph`, un insieme originale di 21 pittogrammi codificati in matrici 8×8 e disegnati con quadrati a scala intera nei controlli da 16/20/24 pixel. Copre **12 edifici e 9 risorse** tramite gli ID di catalogo, senza bitmap esterne o estensioni Avalonia. La palette di partenza è più netta (blu notte, crema, azzurro, verdi e ocra); testi e numeri rimangono affiancati ai simboli.
- Nella UI il pittogramma dell'edificio sostituisce le iniziali decorative. Gli ID e i dati sono invariati; liste, colonne delle risorse, diagnosi fissa e pulsante Costruisci restano gli stessi componenti funzionali. Non è ancora stato approvato un set artistico definitivo.
- Preparare **una sola schermata rappresentativa** con Casa grande, Forno, lavoratori, Cibo e Pane prima di produrre tutti gli sprite.
- Testare almeno 1240×850 e 1040×740, oltre al ridimensionamento e alle impostazioni DPI comuni su Windows: nessun testo sovrapposto, tutti i comandi principali raggiungibili, icone nitide.
- Verificare tre stati distinti: produzione regolare; Forno senza Farina/Acqua; Forno fermo a deposito Pane pieno. Il giocatore deve riconoscerli anche senza colori.
- Aggiungere animazioni solo dopo aver verificato chiarezza e prestazioni; niente movimento obbligatorio per capire i valori economici.
- Mantenere invariate API Application, regole economiche e compatibilità dei salvataggi durante il rifacimento estetico.

**Sequenza di sviluppo:** l'utente ha riferito superati i gate fino a **v24/M4.10**; il **prototipo v25 deve ancora essere compilato, renderizzato e verificato su Windows**. Durante **M5** aggiungere per gruppi industria, esercito, navi e isole; durante **M6** provare il flusso completo e gli obiettivi; in **M7** rifinire asset e accessibilità. La grafica viene sviluppata **progressivamente**. Le fasi e i gate sono nella [roadmap operativa](ROADMAP.md); l'[ADR 0012](adr/0012-retro-8bit-desktop-visual-language.md) registra la scelta stilistica già confermata.
