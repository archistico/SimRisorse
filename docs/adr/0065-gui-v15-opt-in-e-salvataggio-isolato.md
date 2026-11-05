# ADR 0065 — Attivazione graduale della GUI v15 senza mutare la GUI v14

- **Data:** 5 novembre 2026
- **Stato:** Accettata per RC54 / F1-D05d.4b.1, candidata al gate Windows
- **Contesto:** RC53 è validata con 705 test; il Core v15 contiene clock, catalogo, comandi e SQLite ma la UI giocabile è ancora la v14.

## Decisione

1. Aggiungere l'opzione esplicita `--v15`; la modalità default resta v14 fino a quando la v15 supera build, test e parità GUI. Non reintrodurre un nuovo tema o una UI sperimentale in sostituzione della classica.
2. Creare un presenter v15 e una finestra v15 che riprendano la stessa struttura a cinque sezioni, lo stile pixel art e le sette colonne Risorse. I livelli Base/Grande/Avanzato rimangono **righe distinte**, mai un ComboBox nell'edificio.
3. I comandi passano esclusivamente da `V15FoodIndustryCampaignSession`; quote e controlli dei prerequisiti usano sessioni temporanee e non mutano il checkpoint attivo. Il vecchio `food-store` non viene mostrato.
4. Avviare e salvare in un percorso v15 distinto, vietando la selezione del percorso riservato v14; nuovo gioco non cancella automaticamente il precedente e un DB corrotto non abilita il salvataggio.
5. **Non inventare la contabilità dei flussi:** le colonne lorde/ETA restano con «—» fino alla proiezione contabile deterministica del clock v15, da realizzare nella tranche 4b.2.

## Motivi

Incremento testabile che consente prove grafiche e di salvataggio delle nuove filiere senza regressioni automatiche sulla v14 già confermata. Salvaguarda le decisioni UI precedenti e usa un solo motore economico.

## Conseguenze

Un nuovo ramo di presentazione Desktop richiede test di parità e manutenzione transitoria fino alla promozione della v15. Lo schema SQLite resta generation 5. L'isolamento del vecchio save è una precauzione operativa, non un obbligo di compatibilità con altri DB sperimentali (ADR 0060). Nessun benchmark E4a o bilanciamento E4b anticipato.

## Gate

`validate.cmd` su Windows e successiva prova visiva. Promuovere la v15 solo dopo la 4b.2 e la fase D05e. Documentare i test reali separatamente da quelli stimati.

## Errata RC55 — 5 novembre 2026

La prima validazione Windows di RC54 ha rivelato due errori di compilazione `CS0103` in `V15PreviewViewModel` perché il sorgente non importava `SimRisorse.Application.Persistence`, namespace che espone `GameSaveException` utilizzata dai percorsi Save/Load. RC55 aggiunge l'import mancante **senza modificare la decisione di isolamento del salvataggio**. Il gate previsto resta di 713 test dopo build; RC53 è l'ultimo checkpoint validato (705/705). Questa errata non introduce un nuovo ADR perché non modifica decisioni architetturali.
