# ADR 0099 — Albero tecnologico descrittivo e Depositi alimentari di livello superiore

**Data:** 06/11/2026 · **Stato:** incremento RC90 candidato, validazione Windows non eseguita.

## Contesto e decisione

La GUI v15 mostrava lo stato e il costo delle tecnologie, senza il beneficio edilizio. Nella v14 i livelli del vecchio `food-store` esistevano, ma in v15 `food-store` è ritirato e il Deposito alimentare è `food-depot`. Non riattivare il produttore ritirato per correggere il testo.

Aggiungiamo descrizioni tecnologiche calcolate dai tier produttivi **realmente presenti** nella v15, escludendo `food-store`. Un componente esplicito della descrizione e delle regole di costruzione aggiunge i due nuovi livelli del deposito. Le tecnologie non acquistabili dichiarano l'assenza di effetti implementati.

`alimentazione-1` abilita **Deposito alimentare grande**, `alimentazione-2` **Deposito alimentare avanzato**, con prerequisite tecnologico I→II già presente. Nessuna ComboBox: sono due ulteriori righe edilizie selezionabili; quelle bloccate rimangono consultabili e grigie. Il Base resta invariato.

## Effetti economici, produzione e sicurezza

- Base: 1 addetto, 100 Cibo pronto e 50 ingredienti per ciascuna risorsa.
- Grande: 3 addetti, 300 Cibo pronto e 150 ingredienti per risorsa, prezzo Base ×2,6 e progressione famiglia 1,15.
- Avanzato: 10 addetti, 1000 Cibo pronto e 500 ingredienti per risorsa, prezzo Base ×6,5 e progressione famiglia 1,15.
- I tre livelli condividono lo stesso deposito fisico/logico e il medesimo pianificatore: nessuna duplicazione di Cibo, nessun produttore `food-store` aggiuntivo. Le assegnazioni restano **separate per tier**, confrontate con 1/3/10 posti per edificio e con il budget globale degli abitanti.
- I costi dei depositi utilizzano il numero di copie della stessa famiglia nei tre livelli, non un contatore che si azzera cambiando tier. Il valore 2,6/6,5 deriva dai moltiplicatori v14 esistenti: non costituisce approvazione dei coefficienti di bilanciamento futuro.

## Persistenza e compatibilità

Lo schema SQLite v15 rimane **generation 6**: le colonne di `food_industry_site` erano già generiche. Si aggiungono due identità di riga; la lettura accetta file a sei righe (tier nuovi zero) e otto righe, ma rifiuta stati parziali. I tier costruiti richiedono la relativa tecnologia acquistata nel checkpoint. V14 e i salvataggi storici non vengono riscritti. Non aggiungere Matite nei Load.

## Invarianti GUI

Nessun cambio alla tabella a sette colonne, ai tooltip di Produzione/Consumo, alle icone pixel art e alle posizioni di pulsanti e pannelli. Il deposito di livello superiore riutilizza il glifo del deposito Base.

## Validazione

[RC90 test plan](../validation/RC90_DEPOSITI_E_DESCRIZIONI_TECNOLOGIE.md). Previsti 22 casi addizionali; compilazione e test su Windows ancora necessari.
