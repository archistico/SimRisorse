# Inizia qui — SimRisorse RC31 / G1-D45

**RC30:** 511 test .NET attesi dichiarati superati dall'utente. **RC31:** 515 test attesi, ancora da eseguire su Windows. La v14 è stata approvata per l'utilizzo quotidiano ed è ora **la modalità predefinita**. L'archivio delle 44 prove visive P0 rimane senza screenshot: non è una verifica dichiarata completata.

## Avvio rapido (Windows / .NET 10)

```powershell
.\validate.cmd
.\run-game.cmd                  # v14 ordinaria
.\run-game.cmd --legacy-v13     # v13 per compatibilità
```

Per una vecchia scorciatoia `--experimental-v14` rimane supportato; `--v14` è il nuovo alias esplicito. L'avvio normale non cambia né sposta il salvataggio v14, che rimane in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`. La v13 conserva uno slot distinto, `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`. Non copiare un DB v13 nel percorso v14; non esiste una migrazione automatica. Eseguire copie di sicurezza a gioco chiuso.

## Documenti in ordine di lettura

1. [Stato corrente](CURRENT_STATE.md) e [roadmap](ROADMAP.md)
2. [Nuovo handoff](NEW_CHAT_HANDOFF.md) e [handoff completo](HANDOFF.md)
3. [ADR 0042: decisione sulla promozione](adr/0042-promozione-v14-e-conservazione-slot.md)
4. [Rapporto G1-D45](design/G1_D45_RC31_PROMOZIONE_V14.md) e [piano di validazione](validation/G1_D45_RC31_TEST_PLAN.md)
5. [Formato dei salvataggi](SAVE_FORMAT.md) e [registro delle decisioni](AUTHORITY_REGISTRY.md)

## Prossima milestone

**P1-D46: Scuola** — audit iscrizione, progressione individuale, annullamento e rimborsi. D47 Matite; D48 tecnologie; D49 integrazione; D50 gate. Poi E4a misurazione, E4b bilanciamento approvato, E4c migrazione controllata, M5.4–M7.
