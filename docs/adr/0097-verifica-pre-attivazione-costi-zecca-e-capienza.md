# ADR 0097 — Gate di pre-attivazione per i costi della Zecca

**Data:** 06/11/2026  
**Stato:** contratto RC89 candidato, approvazione dei prezzi ancora sospesa.

## Contesto

La RC88.1 ha superato **1105 test su Windows**, confermati dall'utente. Le curve A/B/C delle RC78–RC83 hanno una contabilità numerica deterministica, ma non sono applicate alla partita. Il prezzo di una Zecca equivalente a circa 100–1000 Tagliapietre è un **obiettivo di difficoltà**, non un coefficiente già approvato. La capacità potrebbe essere insufficiente per un pagamento atomico, anche qualora i materiali siano producibili.

## Decisione RC89

1. Produrre una matrice **3 curve × 3 tier** che riporti i preventivi per la prima e la seconda Zecca e per un lotto atomico di due, con contatore progressivo riconciliato.
2. Evidenziare, **risorsa per risorsa**, quando il preventivo eccede la capacità iniziale. Una chiave assente nel `ResourceBundle` significa **zero**, senza eccezioni.
3. Per ciascun pagamento mantenere distinti: capacità iniziale, esito del percorso limitato di Magazzini, numero minimo di rate necessario con capacità iniziale. Quest'ultimo è un limite teorico: il modello escrow RC79 **non è attivo** e non produce materiali.
4. Applicare l'obiettivo 100–1000 Tagliapietre **alla prima Zecca Base**, non automaticamente alle versioni Grande/Avanzata. Gli equivalenti sono stimati sul lavoro dei materiali del preventivo diretto, non misurati in ore di gioco.
5. Distinguere tre priorità di revisione: `recalibrate-base-price`, `review-progressive-funding`, `needs-natural-playtest`. Nessuna priorità equivale ad approvazione per la produzione.
6. Consentire importazione facoltativa dei report industriali RC88. La convalida deve usare **lo stesso audit delle ricevute** e distinguere run diagnostici, run verificati e run MintTen verificati. Non attribuire mai alle curve ipotetiche i risultati misurati con i prezzi correnti.
7. Esporre `--study-e4b-price-readiness` con output JSON, CSV, Markdown e provenienza SHA-256; bloccare la sovrascrittura dei file già presenti; aggiungere test e gate F1.

## Fuori perimetro

Nessuna modifica a catalogo prezzi attivo, curve approvate, formazione, produzione, GUI v14/v15, regime `Legacy`/`MintTen`, SQLite o salvataggi. Nessuna prova di raggiungibilità globale: il percorso simulato con soli Magazzini assume materiali producibili e termina dopo un limite finito. Un arresto in quel percorso **non prova un softlock universale**.

## Gate e decisioni future

RC89 aggiunge **18 test** a RC88.1: totale **1123 attesi**, non ancora eseguiti su .NET 10 in questo ambiente. Prima di attivare la curva economica servono: scelta numerica, pagamenti sostenibili, compatibilità capienza, traiettorie naturali MintTen con le regole nuove e test della persistenza. Non trattare un report a zero secondi come benchmark completato.
