# Nuova chat — SimRisorse RC14 / G1-D29-F1

**Allegare lo ZIP integrale RC14** e leggere, nell'ordine: [CURRENT_STATE](CURRENT_STATE.md), [ROADMAP](ROADMAP.md), [HANDOFF](HANDOFF.md), [audit UI](validation/G1_D29_AUDIT_GUI_V13_V14.md) e [piano test RC14](validation/G1_D29_RC14_TEST_PLAN.md).

## Prompt di ripresa

> Continua SimRisorse da **RC14 / G1-D29-F1**. La RC12 è la **ultima versione con test dichiarati passati dall'utente** (468 attesi). La RC13 ha introdotto roadmap in sei fasi, audit UI-01…UI-22 e correzione tooltip Matite con 2 test aggiunti, ma la sua build Windows **è fallita per 2 CS0136** (`owned`, `free` in `V14PreviewViewModel.Parity.cs`): nessun test RC13 eseguito. La RC14 rinomina soltanto i due identificatori locali nel ramo Matite, mantenendo invariato il comportamento. **470 test attesi in RC14, da eseguire su Windows**; non dichiarare green prima del log. La GUI P0 non è approvata.
>
> **Priorità P0:** completare parità GUI classica v13 / v14 con verifica visuale 1040×740 e 1240×850 e test, correggere difetti reali; poi Scuola/tecnologie, E4a audit numerico, E4b bilanciamento approvato, E4c migrazione con backup, solo infine M5.4–M7 navale.
>
> Preservare GUI v13, Risorse a sette colonne, Edifici Base/Grande/Avanzato separati, pulsanti Costruisci +1/+5/+10 e Lavoratore −1/+1/+5/+10, Esercito completo, Scuola e Matite (minimo 2/studente), acquisti tecnologia con guard esatta; SQLite v2/v3 separati. Niente redesign, niente nuove meccaniche o approvazione implicita dei numeri economici.
>
> **A ogni avanzamento:** ZIP completo di progetto/codice/test, roadmap, changelog, handoff, piano validazione; non inventare esiti xUnit.

[Fix CS0136 RC14](design/G1_D29_RC14_FIX_CS0136.md) · [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md) · [Registro decisioni](AUTHORITY_REGISTRY.md) · [Report D29](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md).
