# G1-D44 / RC30 — Integrità delle evidenze visive P0

**Data:** 04/11/2026. **Ambito:** strumenti di audit, senza modifiche al gameplay o alla GUI.

## Stato

- **Baseline confermata:** RC27, 507 test xUnit superati (utente).
- **Candidata cumulativa:** RC28/RC29/RC30, **511 test .NET attesi, non ancora eseguiti su Windows**.
- **Evidenze reali raccolte qui:** 0/44. **P0 aperto**. Non ci sono FAIL visivi utilizzabili per attuare correzioni applicative D44.

## Difetti verificati nel codice dello strumento D43

1. Nella versione precedente l'esistenza di `UI-XX_...png` era sufficiente a dichiarare la prova presente. Un file vuoto, corrotto o non PNG poteva far apparire una riga `PASS` completa.
2. Il contatore delle evidenze poteva indicare "complete" anche con **44 `NA`** provvisti dei soli campi descrittivi, anziché 44 prove positive.

## Correzioni RC30

- `scripts/p0-ui-evidence.ps1`: lettura/decodifica dei PNG usando `System.Drawing` su Windows; verifica formato PNG e compatibilità delle dimensioni con la viewport dichiarata (tolleranza 16 px); file mancanti o illeggibili producono `EVIDENZA_MANCANTE` e una motivazione nel report.
- Le immagini non valide non sono incorporate nel confronto HTML; compare la ragione dell'esclusione.
- `NA` è ora una **eccezione da approvare manualmente** e impedisce lo stato automatico di 44/44 prove positive. La v13 non applicabile per Scuola/Tecnologie resta gestita come eccezione di *screenshot*, non come `NA` per l'intera voce v14.
- Nuova azione `-Action Verify`: uscita `0` solo con **44 righe PASS complete** e immagini leggibili; uscita `2` se il registro è incompleto, ha FAIL, NA o prove mancanti. Anche uscita `0` **non autorizza a chiudere P0** senza suite .NET verde e approvazione visiva dell'utente.
- `-Action Report` continua a generare HTML anche con registro incompleto e non sovrascrive `results.csv`; `-Action Init` continua a preservare l'eventuale CSV esistente.
- `scripts/test-p0-ui-evidence.ps1`: test indipendenti per `Init`, idempotenza, registri incompleti, 44 `NA`, PNG corrotto e 44 `PASS` con PNG sintetici. Le immagini sintetiche vengono create **solo in una directory temporanea** e non costituiscono evidenza della GUI.

## Criteri e limiti

La verifica tecnica non autentica il contenuto di uno screenshot e non determina se la GUI sia corretta. DPI, finestra e stato della partita devono essere controllati dall'operatore. Una cattura valida può contenere la schermata sbagliata: **la revisione resta umana**.

La roadmap G1-D44 sulle anomalie grafiche resta **aperta e in attesa delle prove D43**. Il passo successivo richiede prima validazione cumulativa RC30 e auto-test PowerShell, poi le 44 prove D43. Non promuovere a D45/P1/E4 senza gate P0.

[Procedura aggiornata](../validation/G1_D43_RC29_PIANO_EVIDENZE.md) · [Piano test RC30](../validation/G1_D44_RC30_TEST_PLAN.md) · [ADR 0041](../adr/0041-gate-evidenze-visive-e-NA.md).
