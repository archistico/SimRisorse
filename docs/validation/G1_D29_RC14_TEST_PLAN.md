# Piano di validazione — RC14 / G1-D29-F1

**Causa RC13:** due errori C# `CS0136` in `V14PreviewViewModel.Parity.cs` impedivano la build. La suite RC13 **non è stata eseguita**. Baseline verde confermata: **RC12 (468 test attesi)**.

## Test automatico Windows (obbligatorio)

1. Estrarre lo ZIP RC14 in una cartella **nuova**. Eseguire `./validate.cmd` da PowerShell e riportare build, totale test, errori e percorso TRX.
2. La build di `SimRisorse.Desktop` deve terminare senza CS0136 e l'intera soluzione deve compilare.
3. Eseguire i **470 test attesi**, compresi i **2 nuovi test Desktop RC13** per la capacità Matite: `PencilCapacityTooltipShowsOwnedBuildingsAndAvailableSpace`, `PencilCapacityTooltipRecalculatesForWarehousesAndSchoolsWithoutLosingResourceSelection`.
4. In caso di nuovo errore, riportare il primo errore e non dichiarare verde la versione.

## Controlli UI P0 (ancora separati)

Con i salvataggi protetti e le modalità v13/v14 su profili diversi, verificare tooltip Matite su Municipio iniziale, Magazzini e Scuola, selezione persistente e elenco Risorse a sette colonne. Completare audit grafico UI-01…UI-22 a **1040×740** e **1240×850**. Test automatici superati non equivalgono ad approvazione visiva.

**Regola di avanzamento:** se RC14 compila e tutti i test sono verdi, riprendere **G1-D30** dal piano di audit; non modificare costi/tempi E4 senza deliberazione.
