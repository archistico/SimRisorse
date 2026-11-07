# ADR 0087 — Finanziamento costruzioni progressive: escrow isolato

- Data: 06/11/2026.
- Stato: **prototipo analitico RC79 / E4b.2a; meccanica NON attivata**.
- Autorità: continua ADR 0086, non approva nessuna delle curve A/B/C né i moduli dei tier.

## Contesto

Le curve RC78 alzano l'investimento dei finali di filiera a centinaia di Tagliapietre equivalenti, ma la capacità del Magazzino cresce linearmente mentre il suo prezzo cresce progressivamente. In un percorso dichiarato, le nuove unità di Magazzino diventano non finanziabili, impedendo pagamenti atomici elevati prima ancora di verificare quanto tempo serve a produrre i materiali. Separare la difficoltà *economica* dalla impossibilità *contabile* di tenere una quantità superiore al deposito è indispensabile per evitare false conclusioni di softlock.

## Decisione di sviluppo

Creare un **motore puro di deposito a rate** (`ProgressiveConstructionEscrow`) indipendente da sessione e salvataggio. Conserva la quotazione integrale e quanto effettivamente pagato; nessun valore compare due volte nell'inventario. Un deposito può essere rifiutato senza perdere materiali, e il rimborso non avviene quando non c'è capacità sufficiente. Un edificio non è erogato dal motore: ogni eventuale integrazione futura richiederà transazione, cancellazione, UI e salvataggio versionato.

Costruire uno studio aggiuntivo delle tre curve che confronta:

1. **Lotto atomico**: la quotazione totale deve entrare in un unico pagamento; è la modalità oggi esistente.
2. **Copie sequenziali**: ciascuna copia usa il contatore progressivo e paga separatamente; il prezzo dell'ultima copia è il massimo.
3. **Escrow rateale ipotetico**: si separa il pagamento dal completamento della stessa costruzione; il totale e il contatore non vengono scontati. Il numero minimo delle rate è vincolato al *vettore* di capacità disponibile, non a un saldo monetario fittizio.

Le rate si suddividono al millesimo, riconciliano esattamente il totale senza allocare liste gigantesche e non possono richiedere capacità più grande di quella dichiarata. Lo studio calcola **27 confronti** (3 curve × 3 tier × 1/5/10 copie). JSON/CSV/Markdown separati con nuova CLI, preservando il formato e il replay RC78.

## Limiti ed eventuale scelta futura

Il calcolo verifica esclusivamente la **fattibilità dimensionale dei pagamenti**, non l'effettiva disponibilità delle risorse, il tempo per produrle, i requisiti di filiera o un percorso naturale percorribile. Un costo altissimo resta un costo altissimo anche quando rateizzabile. Non confondere escrow con uno sconto, un grant o una produzione anticipata.

Prima di installare la funzione nel gioco: decidere se la rateizzazione è desiderata, se applicarla anche ai Magazzini e ai lotti, se rendere la costruzione cancellabile, come gestire rimborsi senza capienza e se la GUI v14 canonica può ospitare i comandi. Definire identità univoca del progetto, snapshot atomico per quote+depositi, prezzo congelato alla creazione, convalida anti-doppia-spesa, compatibilità di sessione/SQLite e test concorrenza/ripresa; solo allora intervenire sulla v15. Non modificare il DB personale v14.

[Risultati da produrre](../validation/E4B_RC79_FINANCING_TEST_PLAN.md) · [Specifica](../design/E4B_RC79_FINANCING_FEASIBILITY.md) · [ADR precedente](0086-confronto-curve-economiche-isolato.md).
