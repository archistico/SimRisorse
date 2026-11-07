# ADR 0092 — RC84: evidenze naturali MintTen separate dalle ipotesi economiche

- Data: 06/11/2026
- Stato: **proposta implementata / release candidata, non validata .NET**
- Ambito: Application/Benchmarks, CLI, test, gate e documentazione. Nessuna modifica a gioco, GUI o SQLite.
- Dipendenze: ADR 0089 (opt-in MintTen), 0090 (benchmark naturale), 0091 (rischio seconda Zecca).

## Problema

RC83 proietta costi e capacità delle curve candidate A/B/C su una o due Zecche, mentre RC82 simula `MintTen` con i prezzi **attualmente attivi**. Non è corretto concludere che una curva sia fattibile, o che un magazzino provochi uno stallo, solo confrontando queste due fonti. Senza correlazione dei contratti di personale e del medesimo inventario iniziale si rischiano confronti invalidi.

## Decisione

Introduciamo `E4bMintNaturalFeasibilityAudit.Analyze` e CLI di sola lettura `--analyze-e4b-mint-ten --input ... [--output ...]`.

1. Validare i ledger naturali RC82 mediante l'analizzatore industriale esistente: ordine degli eventi, ricevute, timeout, milestone, duplicati e orizzonte.
2. Accettare solo i due regimi persistiti, con scenari esatti `E4B-M01/MintTen` e `V15-I03/Legacy`. Verificare obiettivo identico, regole iniziali, inventario fisico, scenario base e orizzonte dichiarato. Richiedere almeno una prova `MintTen`.
3. Una milestone deve avere costruzione della Zecca, assegnazione reale della squadra corretta ed evento produttivo. Non trattare test diagnostici a 0–N secondi come misura integrale.
4. Raccogliere l'istante di costruzione/assegnazione/produzione, minimi di lavoratori liberi, attese, deficit quantitativi osservati, capacità satura **come possibile pressione**, senza equipararla al blocco di produzione.
5. Affiancare i **nove casi di RC83** come `EconomicHypotheses` esplicitamente `not-applied`, con condizioni di personale, capienza e prezzo; non assegnare loro tempi, liquidità o successo derivati dai run ai prezzi legacy.
6. I tre file di output JSON/CSV/Markdown non sovrascrivono risultati esistenti; l'input viene verificato con hash SHA-256 nell'output. La CLI non avanza il clock, non apre e non altera salvataggi personali.
7. Il gate F1 crea una diagnostica RC82 a secondo 0 e ne verifica la rianalisi RC84: 4 osservazioni **tutte diagnostiche**, 0 milestone, 9 ipotesi. Il gate NON esegue gli orizzonti integrali da 57.600 secondi.

## Interpretazione e limiti

Un timeout della politica scripted non dimostra un softlock globale. L'ultimo evento di attesa non è una prova di causa unica e gli eventi deduplicati non danno durate precise. La telemetria magazzino indica saturazione fisica, non prova perdita di produzione; questa richiede diagnostica del clock. Un confronto `MintTen`/`Legacy` controlla il numero di persone con ricette e prezzi legacy; non verifica la curva A/B/C.

## Alternativa scartata

Attivare direttamente curva A e squadre in nuove partite per misurare il tempo: non sostenibile senza aver deciso pagamenti oltre capienza, rischio sui magazzini e regole salvate; inoltre altererebbe il confronto naturale.

## Uscita dalla fase RC84

Verificare RC83+RC84 con 1038 test nel gate; poi eseguire due run naturali MintTen integrali con e senza Legacy (`--compare-legacy`) e rianalizzarli, confrontando nello stesso orizzonte e sullo stesso inventario. Se una strategia fallisce, esaminare gli ultimi eventi e una strategia alternativa **prima** di decidere modifiche a prezzi o stoccaggio. Approvare ufficialmente A/B/C e la politica di finanziamento prima del gameplay versionato.
