# ADR 0056 — Progressione economica avanzata e soglie minime di personale

**Data:** 2026-11-04  
**Stato:** **principio approvato dall'utente; progettazione tecnica e coefficienti proposti**, non implementati.

## Contesto

La Zecca Base richiede oggi soltanto 210 Legno lavorato e 160 Pietra, contro 30 e 10 del Tagliapietre. La crescita del 15% sulle copie successive non crea da sola il salto di difficoltà desiderato fra famiglie produttive di epoche diverse. Il limite di posti 1/3/10 nel sistema tier impedisce inoltre la regola desiderata di dieci persone per avviare una Zecca.

## Decisione di principio

- La progressione dalla colonia iniziale all'industria avanzata deve diventare **molto più onerosa**, fino a un andamento quasi esponenziale calibrato sulla raggiungibilità effettiva.
- La **prima Zecca** deve costituire un investimento di ordine comparabile a circa **100–1.000 Tagliapietre base**; il significato dell'equivalenza e il prezzo reale verranno definiti con benchmark e approvazione specifica.
- È richiesta una **squadra minima di 10 lavoratori per modulo produttivo della Zecca**: due unità simultaneamente operative richiedono 20 addetti, senza doppia assegnazione.
- Il livello Base/Grande/Avanzato rimane una scelta esplicita di edificio, mai una ComboBox; i relativi posti possono essere specifici per famiglia, evitando di imporre un limite di 1 addetto alla Zecca Base.
- **Non si autorizza in questo ADR alcun nuovo prezzo, fattore di crescita, nuova ricetta o migrazione dei salvataggi.**

## Alternative da confrontare prima del codice

1. Un moltiplicatore statico enorme della Zecca: facile da fare, ma rischia di creare un blocco insolubile e materiali insufficienti.
2. Costi per complessità/fase + crescita delle copie e tier **(candidato prioritario)**: distingue la prima Zecca dalle copie successive e rende visibile la pianificazione economica.
3. Solo prerequisiti tecnologici: da soli non risolvono il prezzo basso; possono completare, ma non sostituire la progressione economica.
4. Produzione per singolo lavoratore (attuale) contro moduli da squadre minime: le soglie per modulo corrispondono alla richiesta; i dettagli tier vanno verificati su benchmark.

## Conseguenze e rischi

Richiede cambiamenti a catalogo/costi, rappresentazione e validazione delle capienze per famiglia, planner, comandi lavoro, proiezioni GUI, benchmark, salvataggi e conversioni di regole. Una regola di costo irraggiungibile o un limite di stoccaggio troppo basso può rendere impossibile progredire; un conteggio scorretto delle squadre può consumare input senza produrre output. Le prove automatiche, i benchmark naturali e il rollback sono gate vincolanti.

## Sequenza

1. Completare F1-D05b/d e ottenere il nuovo benchmark alimentare stabile.
2. E4a: misurare costi attuali, produttività e vincoli di popolazione/capacità.
3. E4b-P01: approvare formule, fasce di prezzo e parametri per famiglia/tier.
4. E4b-P02/P03: implementare costi e moduli minimi con versionamento e test.
5. E4b-P04: GUI, diagnostica e benchmark comparati; E4c: migrazione e stabilizzazione.

[Specifica dettagliata](../design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md) · [Matrice di test](../validation/E4B_P00_COSTI_SQUADRE_TEST_MATRIX.md) · [Roadmap](../ROADMAP.md).
