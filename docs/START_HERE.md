# Inizia qui — SimRisorse RC14 / G1-D29-F1

**Ultima suite confermata dall'utente:** RC12, tutti i test passati (**468 attesi**). **RC14** include roadmap in sei fasi, audit GUI iniziale e correzione tooltip Matite; **470 test attesi, da provare**. P0 visivo ancora aperto.

1. [Stato e validazioni](CURRENT_STATE.md)
2. [Roadmap ufficiale](ROADMAP.md)
3. [Handoff completo](HANDOFF.md) / [Prompt per nuova chat](NEW_CHAT_HANDOFF.md)
4. [Audit GUI v13/v14](validation/G1_D29_AUDIT_GUI_V13_V14.md) e [piano RC14](validation/G1_D29_RC14_TEST_PLAN.md)
5. [Decisioni vincolanti](AUTHORITY_REGISTRY.md) e [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md)

Windows 11 con .NET SDK 10:

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --experimental-v14
```

Non sovrascrivere database reali; usare cartelle nuove e backup prima delle prove. [Documentazione](README.md) · [Root README](../README.md).

La **RC13 non compilava su Windows** per 2 CS0136; la RC14 contiene solo il fix dei nomi. [Piano di validazione RC14](validation/G1_D29_RC14_TEST_PLAN.md).
