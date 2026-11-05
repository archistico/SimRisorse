# E4b-P00 — Piano di misurazione e gate per nuovi costi e squadre

**Stato aggiornato RC78:** baseline naturale RC77-M01 misurata, tre curve analitiche confrontate e **926/926 test verificati**. [Risultati](E4B_RC78_CURVE_COMPARISON.md) · [Prove](E4B_RC78_TEST_PLAN.md). Coefficienti e moduli tier candidati, non applicati. Le proiezioni delle squadre non equivalgono a test di clock/save/load con le nuove regole.

| ID | Scenario / prova | Condizione di uscita | Implementazione |
| --- | --- | --- | --- |
| K01 | Prima Zecca con prezzi correnti e progressione naturale | Tempo, materie, output e bottleneck misurati | RC77-M01: I03 completo per entrambe le politiche |
| K02 | Equivalenza costo 100/300/1.000 Tagliapietre base | Confronto investimenti reali, niente overflow | RC78: confronto analitico, nessun tempo di nuova campagna |
| K03 | Crescita copie 1–10, tier Base/Grande/Avanzato | Costi monotoni e prezzi lotti corretti | RC78: preventivi e test delle tre proposte; regole live future |
| K04 | Esecuzione con disponibilità 0/9/10 addetti per 1 Zecca | Zero sotto 10; produzione da 10, se input e spazio adeguati | Da implementare |
| K05 | Esecuzione con 10/19/20 addetti per 2 Zecche | Rispettivamente 1/1/2 moduli attivi, a parità di input | Da implementare |
| K06 | Zecca Grande e Avanzata | Moduli, personale, throughput e upgrade coerenti; nessuna doppia assegnazione | Da decidere e implementare |
| K07 | Scorte Oro e Carbone insufficienti | Nessun consumo da moduli bloccati; diagnosi esatta | Da implementare |
| K08 | Monete o materiali da costruzione oltre capacità | Produzione bloccata correttamente, ampliamento magazzini raggiungibile | Da implementare |
| K09 | Arruolamento, Scuola, popolazione e Zecca in concorrenza | Nessun lavoratore condiviso o negativo | Da implementare |
| K10 | Save/load e migrazione lavoratori in v14/v15 | Invarianti preservati; backup sorgente intatti | Da implementare |
| K11 | Prime filiere, cibo, case, industria, navale | Non esistono stalli inevitabili prima delle nuove soglie | Benchmark E4a |
| K12 | GUI v14: -1/+1/+5/+10, costi e stati grigi | Azioni coerenti con `CanExecute`, messaggi e hover stabili | Da implementare |
| K13 | Ripetizione deterministica e export CSV/JSON | Stessi checkpoint → stesse metriche | Benchmark E4a |
| K14 | Ridimensionamento in seguito a coefficienti approvati | Vecchie partite salvabili/recuperabili, controllo versioni | Gate E4c |

## KPI obbligatori

- Tempo simulato e tempo reale fino alla prima costruzione per ogni tappa;
- legno, pietra, materiali avanzati e investimento totale consumato;
- popolazione totale, disponibile e lavoratori impegnati/inattivi;
- numero di moduli produttivi e resa nominale/effettiva per edificio;
- durata e causa di blocchi per input, personale e capienza;
- saldo di Cibo, Pane, riserva avanzata e rischi di collasso alimentare;
- ROI temporale (tempo per recuperare investimento con produzione realmente vendibile/utilizzabile), dove pertinente;
- fallimenti espliciti `blocked/timeout` non convertiti in esiti `pass`.

## Regole per i risultati

Non fissare alcun tempo desiderato senza dati. Non utilizzare fixture con risorse donate come prova di raggiungibilità nella campagna naturale. Pubblicare insieme assunzioni, versione regole, configurazione, seed, input, output e confronto fra esecuzioni. Le curve numeriche vengono scelte e approvate **dopo** il rapporto; i benchmark sono evidenza, non approvazione dei valori.

[Specifica economica](../design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md) · [Roadmap](../ROADMAP.md).
