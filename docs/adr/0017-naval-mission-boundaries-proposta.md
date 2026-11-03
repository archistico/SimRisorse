# ADR 0017 — Perimetro di flotta, missioni e conquista deterministica

- **Data:** 2026-11-03
- **Stato:** **PROPOSTO — NON ACCETTATO**
- **Milestone:** G1; prerequisito di M5.4–M5.6

## Contesto

La v59-RC1 `civil-growth-13` è validata sul PC Windows secondo l'utente. Ha 16 risorse, 20 edifici-obiettivo + Municipio e un esercito aggregato. Restano due edifici-obiettivo (Cantiere e Castello) e il percorso navale necessario alla conquista obbligatoria. Le ipotesi v55 N-01…N-07 non sono ancora autorizzate e usano numeri di ruleset storici.

## Proposta da discutere

1. Navi individuali persistenti, con ID stabili e capacità comune, distinte dalle 16 risorse economiche. Acquisto atomico previa esistenza del Cantiere.
2. Esplorazione separata dalla conquista; missioni versionate con viaggio andata/ritorno e tempo esclusivamente simulato.
3. Conquista deterministica con prerequisiti visibili prima della partenza, senza perdita irreversibile iniziale.
4. Flotta e soldati riservati alla missione fino al ritorno; soldati ancora residenti e consumatori di razioni ordinarie. Riserve militari aggregate, senza ID individuali dei soldati.
5. Un'isola obbligatoria iniziale per sbloccare il Castello. Schema tecnico estensibile, nuove isole facoltative non necessarie prima del primo completamento.
6. Salvataggi e conversioni di scenario compatibili, senza risorse o navi aggiunte implicitamente.

## Alternativa considerata

Navi monouso come risorsa quantificata; un solo tipo di missione che esplora e conquista; battaglia casuale con perdite. È più facile ridurre la complessità iniziale, ma il costo di ogni viaggio e i fallimenti casuali possono bloccare la progressione e rendere meno chiaro l'investimento nel Cantiere. **Nessuna alternativa è stata definitivamente respinta**; il confronto non equivale ad approvazione.

## Dati aperti

Prezzi N-02/N-03, capienza N-04, simultaneità, ritorni, durate, ricompense, numero di soldati della prima conquista. Leggere i JSON effettivi prodotti dal validatore v59 prima di ratificare le soglie quantitative.

## Conseguenze tecniche se approvato

Nuovo ruleset futuro (presumibilmente `civil-growth-14`), entità nave/missione/isola nel Core, comandi Application atomici, persistenza SQLite, read-model/UI, test di invarianti, tempo e rollback. Nessuna modifica all'esercito storico o alle factory precedenti. Il Castello resta milestone separata.

**Fonte integrale:** [G1 — contratto navale e missioni](../design/G1_NAVAL_MISSIONS_DECISION_GATE.md).
