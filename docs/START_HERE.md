# Ripresa rapida — RC19 / G1-D34

**RC16:** 476 test superati (ultimo gate Windows confermato). **RC17:** 481, **RC18:** 484, non testate. **RC19:** **487 attesi**, da validare su Windows. Migliorati i messaggi nell'Esercito per il blocco «Nuova partita?» e le carenze dei lotti. Include integralmente RC17+RC18. [Stato](CURRENT_STATE.md) · [Piano RC19](validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Ripresa rapida — RC18 / G1-D33

Ultimo test Windows confermato **RC16 (476)**; RC17 non verificata (**481 attesi**); RC18 candidata (**484 attesi**, contiene tutte le modifiche RC17). Eseguire `./validate.cmd` quando possibile; fare backup dei salvataggi prima delle prove GUI. Correzione RC18: messaggio sui posti ricerca insufficienti per lotti +5/+10. [Stato](CURRENT_STATE.md) · [Piano RC18](validation/G1_D33_RC18_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Ripresa corrente — SimRisorse RC17 / G1-D32

**Baseline Windows dichiarata passata:** RC16 con **476 test**. **Candidata corrente:** RC17 (+5 test Desktop, **481 attesi**, da eseguire Windows). [Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Handoff](HANDOFF.md) · [Piano test RC17](validation/G1_D32_RC17_TEST_PLAN.md). La nuova protezione Salva/Carica impedisce che la creazione di una nuova colonia sovrascriva il precedente checkpoint fino al salvataggio manuale. Gate P0 grafico ancora aperto.

---

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
