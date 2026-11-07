# ADR 0090 — Prova naturale della Zecca MintTen senza modificare la campagna (RC82)

- Data: 06/11/2026
- Stato: **implementazione candidata, test .NET non eseguiti**
- Dipendenze: [ADR 0089](0089-attivazione-opt-in-squadre-zecca-v15-versionata.md), [RC77-M01](../validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md)

## Decisione

Aggiungere un percorso naturale indipendente `E4B-M01` che parte esattamente dalla stessa nuova campagna v15 usata da RC77, seleziona esplicitamente `V15CrewMode.MintTen` e tenta la filiera fino alle prime Monete con la Zecca assegnata **in un'unica operazione di 10 persone**. Nessun grant o inventario artificiale; `Legacy` resta selezionabile per confronto e la vecchia CLI E4a conserva numeri e comportamenti.

Il benchmark condivide il motore di azioni, ricevute, ricerca, clock, scorte e diagnostiche dell'esperimento industriale consolidato. Il confronto non aggira alloggi, popolazione, cibo, lavoratori impegnati, prerequisiti, materie prime, tecnologia o stoccaggio. Per evitare che due regime diversi vengano mescolati, il nuovo `ScenarioId` è `E4B-M01`, distinto da `V15-I03`; il `RulesVersion` deriva realmente da `CrewMode`.

## Perché non attivare già MintTen come default

I 926 test RC78 sono confermati; RC79–RC81 non sono ancora compilati. L'RC81 ha dimostrato le squadre nel codice e nei test *scritti*, non una progressione naturale completa osservata. Cambiare il default senza confrontare i colli di bottiglia sarebbe una modifica di bilanciamento non verificata. Nemmeno la curva economica A o il finanziamento progressivo sono stati approvati.

## Contratti

- `--benchmark-e4b-mint-ten` esporta JSON, CSV e Markdown su **percorso scelto**, impedisce di sovrascrivere file esistenti; `--compare-legacy` usa due politiche per entrambi i regimi, ma non genera differenze di tempo inesistenti.
- `--max-seconds` breve crea solo una *diagnostica*: timeout e assenza di milestone non provano un softlock. L'orizzonte completo può essere raggiunto soltanto quando il runner è stato effettivamente eseguito.
- Qualsiasi successo richiede una ricetta **davvero prodotta** dopo ricerca e 10 addetti assegnati. La partita normale, salvataggi, GUI e profili benchmark RC77 non cambiano.
- Non applichiamo ancora curve A/B/C, prezzo Zecca avanzata, pagamenti a rate, modifiche di produzione delle altre famiglie o migrazione SQLite.

## Gate

[Specifica RC82](../design/E4B_RC82_MINT_TEN_NATURAL_BENCHMARK.md) e [test](../validation/E4B_RC82_TEST_PLAN.md). Il gate F1 esegue la diagnostica istantanea su entrambe le politiche e regimi; resta necessario l'orizzonte integrale e la verifica visiva Windows per considerare dimostrata la progressione naturale.
