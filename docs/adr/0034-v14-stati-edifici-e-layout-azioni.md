# ADR 0034 — Separare disponibilità, selezione e visualizzazione dei comandi Edifici v14

- **Data:** 2026-11-03
- **Stato:** Accettato come requisito UI espresso dall'utente; implementazione RC8 da validare su Windows
- **Integra:** [ADR 0032](0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md), [ADR 0033](0033-preservazione-funzionale-gui-v13-v14.md)

## Contesto

Il giocatore non riesce a distinguere nell'elenco un edificio bloccato da uno acquistabile. Inoltre il testo «Costruisci» è compresso in un terzo del pannello dettagli e il colore del pulsante disabilitato non è inequivocabile. La costruzione Scuola era duplicata fra due tab, creando due percorsi UX per la stessa operazione.

## Decisione

- **Blocchi da prerequisiti:** il contenuto della riga appare grigio, ma il `ListBoxItem` rimane selezionabile e mostra nel dettaglio le condizioni mancanti. Il comando di costruzione è assente, non soltanto disabilitato.
- **Blocchi da risorse:** il pulsante resta visibile, ma usa `IsEnabled=false` in modo esplicito, oltre a `CanExecute=false`; lo stato è grigio, non verde. Gli stati hover e disabled restano distinti senza spostare geometria.
- **Geometria:** «Costruisci +1» sta da solo su tutta la prima riga; «−1 lavoratore», «+1 lavoratore», «+5», «+10» condividono una seconda riga. Layout v14 adattato per non tagliare etichette al viewport minimo.
- **Scuola:** la costruzione avviene in Edifici, non nel tab Scuola; quest'ultimo conserva la formazione e la gestione dei ricercatori.
- Nessuna modifica a GUI v13, gameplay, prezzi o database. I parametri economici restano candidati.

## Alternative scartate

Disabilitare le intere righe: impedisce lettura dei requisiti. Nascondere il pulsante per materiali mancanti: confonde il giocatore con un blocco tecnologico. Usare una ComboBox di quantità o di livello: già respinta. Conservare un secondo pulsante Costruisci in Scuola: duplica il flusso operativo.

## Verifica

Test headless sulle due dimensioni, stato enabled/disabled, ordine pulsanti e assenza del comando nella Scuola; audit visivo Windows obbligatorio. [Rapporto RC8](../design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md) · [Piano test](../validation/G1_D24_RC8_TEST_PLAN.md).
