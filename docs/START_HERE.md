# Inizia qui — SimRisorse RC16 / G1-D31

**Baseline con test confermati:** RC15, **473 test attesi** dichiarati superati dall'utente. **Nuova RC16 candidata:** **476 test attesi**; la compilazione e xUnit non sono stati eseguiti nell'ambiente di confezionamento. Il gate grafico P0 è ancora aperto.

1. [Stato e verifiche](CURRENT_STATE.md)
2. [Roadmap approvata in sei fasi](ROADMAP.md)
3. [Handoff](HANDOFF.md) e [prompt per nuova chat](NEW_CHAT_HANDOFF.md)
4. [Audit UI-01…UI-22](validation/G1_D29_AUDIT_GUI_V13_V14.md)
5. [Report e test RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano Windows](validation/G1_D31_RC16_TEST_PLAN.md)
6. [Decisioni](AUTHORITY_REGISTRY.md) e [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md)

Da Windows 11 con SDK .NET 10, su cartella nuova e con backup dei salvataggi:

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --experimental-v14
```

Non sovrascrivere un database personale. [Documentazione](README.md) · [README generale](../README.md).
