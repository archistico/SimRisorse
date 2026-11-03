**Aggiornamento G1-D32 / RC17 (03/11/2026):** RC16 confermata verde dall’utente (**476 test passati**); RC17 candidata **481 attesi**. Decisione di sicurezza [ADR 0039](adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md): durante «Nuova partita?» Salva/Carica disabilitati; dopo Avvia nuova nessun autosalvataggio fino al primo Salva v14 esplicito. Chiudere durante la conferma la annulla e salva la vecchia colonia. Nessun cambiamento ai coefficienti economici o ai database. **P0 grafico ancora aperto.**

# Registro decisioni attive — SimRisorse

**Aggiornamento G1-D29 / RC13 (03/11/2026):** RC12 confermata con test tutti passati dall’utente (468 attesi); roadmap concordata in **sei fasi**: gate P0 GUI, rifinitura Scuola/tecnologie, E4a audit quantitativo, E4b bilanciamento con approvazione, E4c migrazione/rollback, M5.4–M7 navale. [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md). RC13 non introduce regole economiche.

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** RC11 dichiarata verde dall'utente (**466 test attesi**, numero esatto non comunicato). RC12: ricerca a **due decimali**, pulsante «Acquista ricerca» disabilitato e grigio se saldo o prerequisiti insufficienti, verde con hover ocra solo quando acquistabile. Controllo prezzi/saldi sul valore **non arrotondato** in millipunti. **468 test attesi, non eseguiti** in questo ambiente privo di SDK .NET 10. GUI v13, core e SQLite invariati; audit visivo P0 ancora aperto. [Report RC12](design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).


**AGGIORNAMENTO G1-D27 / RC11 (03/11/2026):** l’utente conferma **test RC10 passati (464 attesi)**, senza comunicare un conteggio dettagliato. RC11 riordina il dettaglio Edifici della sola v14 in due righe: **Costruisci | +1 | +5 | +10** e **Lavoratore | +1 | +5 | +10**; il pulsante **−1** rimane compatto accanto alla label Lavoratore per non perdere la funzione di rilascio. Aggiunti comandi atomici per assegnare 5/10 persone, con limiti di popolazione libera e posti di lavoro, stati disabled espliciti e hover già previsto. **466 test attesi** (+2 casi), non eseguiti qui per assenza dell’SDK .NET 10; audit GUI P0 ancora aperto. [Rapporto RC11](design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Piano RC11](validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).


**Aggiornamento operativo:** G1-D25 / 3 novembre 2026 (decisioni UI esplicite sull’aspetto degli edifici bloccati, sulle righe pulsanti e sulla Scuola: [ADR 0034](adr/0034-v14-stati-edifici-e-layout-azioni.md)). **Prevalenza:** esplicite richieste del giocatore e ADR accettati > codice e documentazione di rilascio correnti > analisi/proposte > rapporti storici. Il testo storico precedente è salvato in [`history/operational/AUTHORITY_REGISTRY_PRIMA_DEL_RIORDINO_G1_D19.md`](history/operational/AUTHORITY_REGISTRY_PRIMA_DEL_RIORDINO_G1_D19.md).

**Nuove decisioni RC9:** rimuovere i pulsanti «Esci senza salvare»; mostrare il progresso della Scuola come l'Esercito; formazione richiede **almeno 2 Matite per studente**, ottenute da Legno e Carbone. Il lotto manuale di 4 Matite da 1 Legno lavorato + 1 Carbone e la capienza supplementare della Scuola sono **valori candidati**. [ADR 0035](adr/0035-scuola-formazione-matite-v14.md).

## Requisiti confermati

1. **GUI:** la grafica e le funzioni della **v13 non si ridisegnano né riducono** nella v14. Menu laterale **Edifici, Risorse, Esercito, Scuola, Albero tecnologico**. Le nuove pagine si aggiungono; la tabella Risorse originale a sette colonne, l'Esercito, la crescita, i dettagli e la filiera Pane rimangono completi. **Non introdurre ComboBox di livello** nei dettagli. Vedi [ADR 0032](adr/0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md), [ADR 0033](adr/0033-preservazione-funzionale-gui-v13-v14.md).
2. **Edifici a tre livelli:** Base (1 lavoratore), Grande (3), Avanzato (10), **ognuno direttamente costruibile come edificio distinto**; deve restare possibile potenziare edifici esistenti con un comando separato. Costo di costruzione crescente a ogni nuovo acquisto della famiglia; upgrade non deve generare acquisti fittizi. Le Case e il Magazzino conservano funzioni e categorie proprie.
3. **Stati di costruzione:** prerequisiti mancanti ⇒ edificio **grigio in elenco ma selezionabile**, niente pulsante Costruisci, ma requisiti leggibili; sbloccato senza risorse sufficienti ⇒ pulsante disabilitato con indicazioni; pulsanti +1/+5/+10 coerenti e prezzo cumulativo. Hover non sposta elementi; disabilitato diverso da hover. Nella RC11 la prima riga è **Costruisci | +1 | +5 | +10**; la seconda è **Lavoratore (−1) | +1 | +5 | +10**.
4. **Ricerca:** la Scuola si costruisce dalla scheda Edifici: nella scheda Scuola sono presenti solo azioni studenti/ricercatori, senza pulsante Costruisci. La Scuola forma ricercatori tra gli abitanti esistenti; **i punti ricerca si accumulano liberamente e vengono spesi solo per decisione del giocatore**. Tecnologie prive di beneficio reale non acquistabili.
5. **Capacità e popolazione:** Municipio unico iniziale; magazzino con incremento **uniforme per ogni risorsa**, incluso Pane; edifici produttivi contribuiscono allo stoccaggio delle proprie uscite. Capacità leggibile nel dettaglio edificio e tooltip della tabella Risorse; residenti mai duplicati. La costruzione di una Casa accelera la crescita come previsto dallo scenario.
6. **Militari v13:** una recluta per posto Caserma, 30 s individuali, 5 Spade, soldati che occupano popolazione e consumano razioni, ordini e annullamento. Non semplificare la scheda Esercito nella v14.
7. **Salvataggi:** preservare la partita `civil-growth-13` (SQLite v2); la sperimentale v14 usa SQLite v3 e percorso separato; migrazione solo esplicita con backup e rollback. Salvare/ricaricare deve preservare la simulazione.
8. **Metodo di lavoro:** consegna ZIP integrale a ogni incremento, roadmap/stato/test/ADR aggiornati, root del progetto pulita, preservare storico; non presentare un semplice passaggio xUnit come approvazione della UX.

## Implementazioni candidate, NON decisioni economiche definitive

| Tema | Parametro oggi nel prototipo | Stato |
| --- | --- | --- |
| Crescita costi per nuova costruzione | **15%** per famiglia, con arrotondamento | Candidato da bilanciare |
| Moltiplicatore prezzi livelli | Base 1; Grande 2,6; Avanzato 6,5 | Candidato da approvare |
| Capacità locali di output | +50 / +150 / +500 per produttore | Candidato della nuova economia |
| Scuola | 120 Legno lavorato +80 Pietra, 120 s/formazione, 10 Cibo, 1 posto corso/2 posti ricercatori | Candidato da approvare |
| Matite v14 | **2 per studente confermate**; lotto manuale da 4 con 1 Legno lavorato + 1 Carbone; Scuola +50 capienza | Coefficiente del lotto e capienza candidati |
| Produzione di ricerca | 0,1 punto al secondo per ricercatore attivo | Candidato da approvare |
| Costi sblocco tecnologie | 8 tecnologie produttive prototipo | Candidato da approvare |
| Logistica / Navigazione | Ricerche differite, senza effetto acquistabile | Restano NON acquistabili |
| Regole navali e conquista | Proposte M5.4–M5.7 | Non ancora approvate o implementate |

**Nota:** 1/3/10 posti e la distinzione delle righe edificio sono **requisiti confermati**; i costi, tempi e bonus sono esempi eseguibili ma richiedono un gate decisionale separato. La RC2 grafica **non è stata approvata**.

## Fonti operative

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Handoff](HANDOFF.md) · [Dossier prezzi e ricerca](design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [Dossier tecnologia](design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md).
