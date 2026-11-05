# RC54 — F1-D05d.4b.1 · Piano di validazione Windows

## Stato e numeri

RC53 **705/705** validati dall'utente. RC54 aggiunge **8 test** in `tests/SimRisorse.Desktop.Tests/V15DesktopIntegrationTests.cs`. **713 test attesi**, senza compilazione/xUnit .NET 10 in questo ambiente. Il numero di test realmente eseguiti fa fede nel TRX, non nella stima.

## Gate automatico

```powershell
.\validate.cmd

dotnet test tests\SimRisorse.Desktop.Tests\SimRisorse.Desktop.Tests.csproj -c Release --filter FullyQualifiedName~V15DesktopIntegrationTests
```

Nuovi test:

1. Opzione `--v15` separata, v14 predefinita, rifiuto delle opzioni conflittuali e del percorso v14 riservato.
2. Sprite pixel art per sei siti e cinque risorse intermedie.
3. Prima partita in pausa, sei siti, tier separati, un Cibo, nessun `food-store` visibile; Deposito bloccato senza Pescatore.
4. Creazione e caricamento file SQLite v15 senza modificare file v14 adiacenti.
5. Nuova partita con conferma e protezione dal salvataggio automatico finché non si salva manualmente.
6. DB v15 corrotto: nessuna riscrittura e salvataggio disabilitato.
7. Finestra Avalonia headless: cinque sezioni menu, sette colonne e selezione coerente.
8. Selezioni Edificio/Risorsa/Tecnologia stabili dopo refresh; il clock in pausa non avanza.

## Gate manuale (solo dopo build verde)

1. Avviare `run-game.cmd`: **deve rimanere la GUI v14 identica**, con le proprie risorse e salvataggio.
2. Avviare `run-game.cmd --v15`: verificare la palette, icone, scroll, menu, costruzioni bloccate senza pulsanti e tier diretti senza ComboBox.
3. Risorse: **sette colonne**, alfabetiche, selezione ocra, colonne numeriche a destra, scrollbar non sovrapposta, quantità arrotondate, tooltip della capienza. Nota: i flussi sono ancora «—» in questa RC.
4. Acquistare Pescatore, poi nuovo Deposito; assegnare personale, avviare e mettere in pausa; osservare la produzione nel tempo, Cibo pronto in un'unica riga e Pane separato.
5. Testare una sessione reale con Stalla, Macellaio, Orto, Frutteto; saturare capacità; verificare che nessuna risorsa sparisca e nessun lavoratore venga contato due volte.
6. Provare Scuola, formazione e ricerca con stock pronti; testare un reclutamento con Caserma; salvare, riavviare e caricare.
7. Confermare Nuova partita e chiudere senza salvare: il vecchio checkpoint v15 deve restare intatto.
8. Verificare caricamento su DB v15 volutamente non valido: non deve venire sovrascritto.

## Rischi ancora aperti

- Compilazione Avalonia/compiled bindings e test headless **non eseguiti** qui.
- Flussi lordi e ETA v15 non disponibili: completare F1-D05d.4b.2 prima della parità completa della tabella Risorse.
- UI v15 in modalità **opt-in**, non ancora modalità predefinita.
- Il catalogo v15 eredita internamente parti del ruleset v14; `food-store` rimane uno slot tecnico.
- Testare salvataggi con percorsi contenenti spazi e caratteri accentati; testare suspend/minimizzazione.
