# G1-D20 — v60-E3b4-RC4: diagnosi Pane in parità v13/v14

**AGGIORNAMENTO 03/11/2026:** la suite RC4 è stata successivamente dichiarata **passata dall’utente** (443 attesi, conteggio reale non disponibile); resta obbligatorio l’audit visivo. Seguente patch: [G1-D21 RC5](G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md).

**Data:** 3 novembre 2026 · **Stato:** patch P0 **candidata**, non compilata né testata su Windows in questo ambiente. La verifica GUI RC3 resta aperta; RC4 **non la sostituisce**.

## Problema osservato nel codice RC3

La GUI v13 espone `BreadSupplyProjector` e distingue: scorta per il primo arrivo avanzato, insufficiente capacità nominale dei Forni, input mancanti, magazzino pieno, produzione sufficiente e riserva residua. La v14 RC3 sostituiva la diagnosi con una sola frase generica, mostrata **solo dopo** l'arrivo di residenti avanzati (`ShowBreadSupplyStatus => HasAdvancedResidents`). Perciò non era possibile vedere l'avviso del primo arrivo né capire quale collo di bottiglia risolvere.

## Modifica circoscritta

1. L'algoritmo già collaudato `BreadSupplyProjector` rimane invariato nella logica; si aggiunge un overload con scenario, popolazione e forecast, così la v14 può usarlo **senza costruire una `GameSession` v13**.
2. `V14BreadSupplyProjector` adatta letture **non mutanti** dello snapshot v14 (edifici civili + produttori Base/Grande/Avanzato) e il forecast `V14EconomyPreview` a `BuildingView`/`ResourceView`. I lavoratori vengono sommati per famiglia, mantenendo la proiezione reale dell'orologio; i dettagli della catena restano stime nominali, non garanzie di produzione futura.
3. `BreadSupplyStatusFormatter` riusa **testualmente** i messaggi esistenti della v13. Anche `MainViewModel` usa lo stesso formatter: niente divergenza involontaria fra le schermate.
4. `V14PreviewViewModel` mostra la diagnosi quando lo stato non è `Hidden`, **anche prima del primo residente avanzato**. Il refresh mantiene le stesse righe Risorse e la selezione esistente.
5. Nessun cambiamento a `GameSession`, clock v14, catalogo, formule economiche, XAML, salvataggi SQLite v2/v3, percorsi di profilo o contatori di acquisto.

## Test introdotti (quattro, ancora da eseguire)

| Suite | Controllo |
| --- | --- |
| Application | Primo arrivo avanzato senza Pane, anche con Cibo mancante |
| Application | Forno con lavoratori ma senza input vs ricetta alimentata |
| Application | Capacità Pane satura: `StorageFull` e non falsa carenza nominale |
| Desktop | ViewModel v14 ricaricato dal proprio database v3 mostra lo stesso avviso anticipato della v13 |

**Riferimento numerico:** 439 test RC3 attesi + 4 nuovi test = **443 test attesi**, senza esito di discovery/build confermato. L'ultimo gate Windows confermato resta **427/427 RC1**.

## Gate ancora obbligatori

- `./validate.cmd` completo su Windows, con log e numero reale di test. Non concludere dal numero atteso.
- Avvio `./run-game.cmd` e `./run-game.cmd --experimental-v14`; confronto a 1040×740 e 1240×850. Accertare che il testo della filiera non tagli la nota di pausa o il footer. Stessa scorta e stesso contesto di confronto.
- Ripetere l'intera matrice Risorse/Edifici/Esercito/Pane e il test di salvataggi separati della RC3; **nessun gate è considerato chiuso per effetto di questa patch**.
- Non passare a E4 né alle refactor GUI P1 prima di esito positivo o ulteriori patch mirate.

[Roadmap](../ROADMAP.md) · [Matrice parità](../validation/G1_D19_MATRICE_PARITA_GUI.md) · [Piano test RC4](../validation/G1_D20_RC4_TEST_PLAN.md) · [ADR GUI](../adr/0033-preservazione-funzionale-gui-v13-v14.md).
