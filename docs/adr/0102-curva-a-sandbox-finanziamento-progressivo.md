# ADR 0102 — RC92: curva A e finanziamento sperimentale isolato

**Data:** 06/11/2026  
**Stato:** prototipo tecnico da validare; **non** approva i coefficienti economici, non abilita la funzione in partita.

## Problema

La curva A (`ProgressionBase=2`, `CopyGrowth=1.08`, `LargeMultiplier=2.6`, `AdvancedMultiplier=6.5`) produce preventivi Zecca che possono superare la capacità dei depositi fisici. L'acquisto atomico richiede che tutti i materiali coesistano nel magazzino e può rendere impossibile un acquisto che invece sarebbe finanziabile con versamenti successivi. Le analisi RC78–RC89 davano solo quotazioni e numero teorico di rate; non collegavano il contatore delle copie a uno stato transazionale verificabile.

## Decisione sperimentale

1. Introdurre `E4bCurveAFundingSandbox`, **esplicitamente opt-in**, in memoria e separato da gameplay, SQLite e GUI. Il simulatore riusa `E4bEconomicCurveStudy` e `ProgressiveConstructionEscrow` esistenti; non applica alcun prezzo alle partite correnti.
2. Quando si apre un acquisto, il preventivo Curve A per `mint` e tier Base/Grande/Avanzato viene **congelato** rispetto al contatore corrente. Finché esiste un acquisto pendente non è possibile aprirne un secondo; il contatore della famiglia cresce **solo dopo il completamento di tutti i versamenti**.
3. Ogni versamento usa esclusivamente lo stock della sessione, in modo atomico. Deposito parziale, pagamenti doppi/sovrabbondanti, risorse assenti e tentativi fuori sequenza non devono creare edifici o incrementare contatori.
4. La consegna di materiali di prova (`TryDeliverHypothetical`) è marcata come **ipotetica**, è limitata dalla capacità fisica e non simula né misura produzione, trasporti, tempi o popolazione. La prova conta separatamente il materiale ancora in città, il deposito pendente e i costi completati, con conservazione `scorta iniziale + rifornimenti ipotetici = scorta + deposito + spesa completata`.
5. L'annullamento è **solo con rimborso completo e capienza sufficiente**: in assenza di spazio non modifica nulla e non cancella il credito. Il completamento nel sandbox non concede edificio, lavoratori, capienza o ricette nel runtime.
6. Report deterministico (JSON/CSV/MD) per due Zecche consecutive a ogni livello; il gate richiede sei righe riconciliate, ambito dichiarato non giocabile e approvazione economica ancora negata.

## Confini necessari prima del gameplay

Non è ancora definita la persistenza **atomica** del credito e dell'inventario SQLite, la gestione dei rimborsi, né l'interazione fra prerequisiti, acquisti, produzione naturale, risorse accreditate e salvataggi/cambi regime. Il sandbox non costituisce un'alternativa praticabile al motore v15 finché questi requisiti non sono soddisfatti e i prezzi non sono approvati. Un eventuale regime economico giocabile dovrà essere versionato, con test di crash/replay/no-double-spend.

## Verifica

[RC92 — piano test](../validation/RC92_CURVE_A_FUNDING.md). Baseline Windows confermata: RC90.3 **1167** test. RC91.3 **1188** attesi ma gate non confermato. RC92 aggiunge **15 casi xUnit**, totale **1203 attesi**. Senza SDK .NET 10 questo è un incremento candidato, non validato.
