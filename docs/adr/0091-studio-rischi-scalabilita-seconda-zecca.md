# ADR 0091 — Studio congiunto del costo della seconda Zecca, squadre e magazzini

- **Data:** 06/11/2026
- **Stato:** Accettato per lo **studio isolato**; decisione su prezzi, curva di crescita, escrow e default **non presa**.
- **Milestone:** RC83 / E4b.2e.

## Contesto

L'utente ha confermato superata l'intera suite RC82 (1002 test). La modalità MintTen è disponibile solo opt-in; esiste un benchmark naturale senza grant, ma non sono state riportate misure complete MintTen a orizzonte pieno. Lo studio RC78 mostra prezzi progressivi e stima il fabbisogno complessivo; RC79 confronta le modalità di pagamento; RC80/81 definiscono la squadra minima della Zecca. Mancava un confronto diretto fra **una e due Zecche** che non confondesse la quotazione del singolo edificio con l'investimento nella filiera e nella popolazione.

## Decisione

Introdurre `E4bMintScaleRiskStudy` in Application, completamente separato dalle partite, con nove confronti: curve A/B/C × tier Base/Grande/Avanzato. Ogni confronto contiene:

1. investimenti filiera per una e due Zecche con costi, edifici e budget demografici;
2. costo *marginale* della seconda Zecca e delta dell'intera filiera, in risorse e edifici;
3. lavoratori della Zecca 10/30/100 → 20/60/200 per piena produzione, senza riciclare quelli della prima;
4. controllo della capacità del solo percorso di crescita Magazzini dello studio: pagamento della seconda Zecca e acquisto atomico di entrambe;
5. numero **minimo teorico** di rate pagabili con la capacità iniziale, senza presumere disponibilità dei materiali;
6. comparazione del **solo prezzo diretto** di ciascuna Zecca in equivalenti di lavoro materiale per il primo Tagliapietre e fascia indicativa 100–1000.

La metrica Tagliapietre misura lavoro materiale equivalente ricorsivo delle ricette, **non** costo monetario, ritorno economico, tempo di gioco né costo di tutti i prerequisiti. La stima alimentare/demografica è un budget statico. Il limite 512 Magazzini riguarda il percorso modellato, non la giocabilità universale. Qualsiasi stallo osservato in queste proiezioni è solo una criticità da approfondire nei run naturali.

## Alternative non scelte

- **Attivare subito curva A:** prematuro senza misure naturali MintTen e politica di finanziamento; rischio blocco di capienza.
- **Promuovere di default MintTen:** richiede progressione osservata, compatibilità dati di sviluppo e decisione esplicita sulle nuove partite.
- **Escrow live:** modello RC79 valido come prototipo, ma manca il commit transazionale con produzione, annullamento e salvataggio in partita.
- **Rivedere GUI v14:** non necessario per lo studio, vietato sostituire il layout approvato.

## Validazione e conseguenze

18 test RC83, comando `--study-e4b-mint-scale`, JSON+CSV+Markdown, gate F1 portato a **1020**. Il codice gira solo su catalogo sintetico di studio e non accede ai DB personali. La RC83 non cambia Core, Infrastructure, Desktop, schema SQLite, regole attive, prezzi, capacità né velocità di crescita ×20.

**Criterio di passaggio a RC84:** suite cumulativa verde; run naturale completo `MintTen` per entrambe le politiche e confronto `Legacy` con provenienza e file reali; classificazione separata di stalli di script, produzione, alimentazione, capienza e prezzo. Solo dopo scegliere se introdurre la curva A, politica Magazzini/escrow e default nuove campagne.
