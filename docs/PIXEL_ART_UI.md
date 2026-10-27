# Interfaccia retro 8-bit

Aggiornata il **2026-10-27**. La base grafica è **implementata, verificata su Windows e approvata dall'utente**. La finestra Avalonia usa pittogrammi originali, monospace e pannelli compatti. [Ultima verifica](validation/DESKTOP_UI.md), [decisione stilistica](adr/0012-retro-8bit-desktop-visual-language.md).

![Interfaccia corrente della colonia](screenshots/colonia.png)

## Obiettivo

Far leggere subito costi, produzione, lavoratori e motivi di blocco. Il colore accompagna il testo; non sostituisce quantità, unità o spiegazioni. Le strutture si gestiscono da elenco, senza posizionamento su una mappa.

La grafica è sviluppata progressivamente con i contenuti. Le sezioni militari, navali, isole e obiettivi arriveranno con i rispettivi incrementi; non devono apparire come funzioni già disponibili.

## Layout attuale

- **Sidebar da 240 px:** marchio, menu Edifici/Risorse e riquadro Crescita civile con progresso e fabbisogni. Il contenuto lungo può scorrere.
- **Testata compatta:** titolo, cronometro, stato e pausa sulla stessa riga.
- **Card comuni alle due pagine:** abitanti/posti, lavoratori liberi e tre scorte principali. Quando ci sono residenti avanzati, il Pane entra nel riepilogo. Le barre di capienza si adattano alla larghezza disponibile.
- **Edifici:** elenco con icona, quantità, personale e stato; dettaglio a destra.
- **Risorse:** tabella con quantità, capacità, produzione, consumo e saldo al secondo. Scorrimento interno alla finestra; nomi leggibili anche nel layout minimo.
- **Footer:** messaggi, stato dei salvataggi, nuova partita, caricamento e salvataggio.

Le dimensioni verificate sono **1040×740** e **1240×850**. Non ridurre i pulsanti o nascondere le diagnosi per guadagnare spazio.

## Dettaglio della struttura

Ordine di lettura:

1. Nome e descrizione.
2. **Costi di costruzione in elenco**, una riga per risorsa, con icona e quantità.
3. **Produzione per addetto e secondo**, con icona della risorsa; per case e magazzini si mostra l'effetto dell'edificio.
4. Eventuali consumi della ricetta.

I costi seguono la quantità scelta (**+1/+5/+10**); la produzione descrive sempre il singolo addetto. Costi e produzione usano lo stesso modello `ResourceDetailTemplate`: testo normale da **12 px**, icona da **14 px**, distanza icona/testo di 7 px.

Diagnosi, quantità, Costruisci e assegnazioni restano nel fondo fisso. Prerequisiti mancanti e altri avvisi sono visibili sopra i comandi senza scorrere il dettaglio; la descrizione può scorrere quando necessario.

## Tipografia e pittogrammi

Unica famiglia condivisa `ContentControlThemeFontFamily` in `App.axaml`: **Consolas**, poi **DejaVu Sans Mono, Liberation Mono, Courier New**. Titoli, cifre, tabelle e controlli Fluent usano questa risorsa. Sono font di sistema; non viene distribuito un font aggiuntivo.

`PixelGlyph` disegna matrici originali **8×8** con rettangoli. Sono coperti **17 edifici, 14 risorse e l'indicatore degli abitanti**. Le chiavi seguono gli ID del catalogo, più `population` per la card; la grafica resta nel Desktop.

| Utilizzo | Dimensione del controllo |
| --- | ---: |
| Riepilogo risorse e abitanti | 16 × 16 px |
| Tabella Risorse | 20 × 20 px |
| Elenco edifici | 24 × 24 px |
| Costi e produzione nel dettaglio | 14 × 14 px |

I pennelli della palette sono immutabili, per consentire l'inizializzazione del tipo anche fuori dal thread UI. I pittogrammi sono codice del progetto; non derivano dagli sprite delle immagini di riferimento. Nessuna dipendenza grafica esterna.

## Palette e stati dei controlli

| Elemento | Colore |
| --- | --- |
| Sfondo principale | `#EAE8DC` |
| Sidebar, testo principale e contorni | `#263349` |
| Riquadro Crescita | `#303B50` |
| Card ed elenco | `#FFFDF4` |
| Dettaglio struttura | `#DFE7E3` |
| Struttura selezionata e hover pulsanti | **`#E6D5AC`**, risorsa condivisa `SelectionBackground` |
| Pulsante primario e menu attivo | `#176858` |
| Costruisci disabilitato / testo | `#B8C2BC` / `#5D6C64` |

L'hover dei pulsanti abilitati usa l'ocra della selezione con testo scuro `#263349` e bordo `#9A6947`. Vale per primari e secondari, compreso il presenter del tema Fluent. I disabilitati restano grigi anche con il puntatore fermo sul controllo. La pressione ha uno stato distinto dall'hover.

**Geometria stabile:** hover, pressione, focus e disabilitazione non cambiano dimensioni, padding o spessore dei bordi. Il pulsante Costruisci mantiene un bordo da 1 px.

## Diagnostica della crescita e della produzione

Le formule e le previsioni provengono da Application. La GUI non ricalcola ricette, priorità, capacità o razioni.

La crescita mostra Cibo, Pane dei residenti avanzati e blocchi. La diagnosi Pane distingue capacità nominale insufficiente, input mancanti, scorte quasi piene e autonomia senza nuove forniture; la descrizione deve restare utile anche senza colori. Se mancano Cibo e Pane insieme, entrambe le necessità sono segnalate.

Le stime nominali della filiera Pane non dimostrano la disponibilità di edifici, addetti o materiali: vedere il [modello di capacità](scenarios/BREAD_CHAIN_CAPACITY.md). Per miniere e fonderie sono esposti prerequisiti, input mancanti, spazio e priorità del Ferro sul Carbone.

## Verifica e rifiniture successive

Il gate corrente comprende **206 test**, di cui **48 Desktop**, con render, click sul menu, aggiornamento delle card, ordini multipli, diagnosi fisse e controlli geometrici. I test di hover controllano i colori effettivi del presenter di Costruisci e Salva, il contrasto e il passaggio allo stato disabilitato. [Evidenze](validation/DESKTOP_UI.md).

Restano da ampliare le prove su DPI fisici, fallback dei font, Linux/macOS, tastiera e accessibilità. Animazioni e nuovi asset si valutano dopo la leggibilità; eventuali bitmap future richiederanno interpolazione adatta al pixel art. Regole economiche e compatibilità dei salvataggi rimangono indipendenti dalle rifiniture visive.
