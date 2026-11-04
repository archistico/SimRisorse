# Piano validazione RC26 / G1-D40

**Ultima versione confermata:** RC25, **501 test superati dichiarati dall’utente**. **Nuova candidata:** RC26, **504 test attesi**, +3 test Desktop. Il gate grafico P0 rimane aperto.

## Verifica automatica Windows

Nella cartella estratta della RC26:

```powershell
.\validate.cmd
```

Raccogliere riepilogo e, in caso di errore, percorso `.trx`, primo errore e nome del test. Se fallisce la compilazione, segnalarlo separatamente dall’esecuzione dei test. **Non** ripetere prove su RC21–RC25: RC26 è cumulativa.

## Verifica visiva/funzionale manuale

1. Avviare `./run-game.cmd` e `./run-game.cmd --experimental-v14` usando copie dei salvataggi e viewport **1040×740**, **1240×850**.
2. All'inizio la v14 mostra «Case piene» con 8/8 abitanti e barra di crescita a zero; costruire una seconda Casa piccola.
3. Prima di avviare il tempo, verificare la velocità `×1,15` e `~53s`; dopo 1 secondo controllare `1,15 / 60 s` e `~52s`. Il testo non deve discordare dalla barra percentuale.
4. Osservare la tabella Risorse: `si riempie tra` usa il saldo prodotto − consumato; con saldo zero o negativo non promette una data di riempimento; a deposito pieno `00:00:00`. Ricontrollare scrollbar e allineamenti delle sette colonne.
5. Costruire e avviare una filiera alimentare nella copia di prova; verificare lo stato di crescita con Cibo insufficiente e prima dell’arrivo del primo residente avanzato con Pane insufficiente. Queste verifiche sono ancora **manuali**: RC26 non introduce una copertura automatica esaustiva dei blocchi.
6. Registrare PASS/FAIL delle righe **UI-05** e **UI-14** della matrice P0, con screenshot alle due dimensioni; non chiudere P0 finché tutte le 22 voci non hanno prova reale.

## Criteri di superamento

- 504 test passati su Windows e nessun errore di compilazione.
- Confronto v13/v14 confermato visivamente per UI-05 e UI-14.
- Nessuna variazione del gameplay E4, della v13 o dei salvataggi v2/v3.

[Rapporto D40](../design/G1_D40_RC26_FLUSSI_POPOLAZIONE.md) · [Roadmap](../ROADMAP.md) · [Matrice](G1_D39_P0_UI_MATRIX.md).
