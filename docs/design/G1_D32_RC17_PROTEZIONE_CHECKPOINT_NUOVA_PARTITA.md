# G1-D32 / RC17 — Protezione del checkpoint durante Nuova partita

**Data:** 03/11/2026. **Baseline Windows confermata dall’utente:** RC16, **476 test superati**. **Candidata RC17:** +5 test Desktop, **481 attesi**, non eseguiti in questo ambiente perché manca .NET SDK 10.

## Difetto accertato (UI-20/UI-21)

La conferma «Nuova partita?» era già protetta per i comandi economici, ma `Salva` e `Carica` continuavano a operare. Ancora più grave: il testo della v14 prometteva che il checkpoint precedente sarebbe rimasto intatto «finché non premi Salva», mentre il timer di autosalvataggio (1 minuto) o il salvataggio alla chiusura potevano salvare la colonia appena creata **senza che il giocatore avesse premuto Salva**.

## Intervento RC17

1. Durante «Nuova partita?» sono disabilitati **Nuova partita**, **Salva v14** e **Carica v14**; `Save()` e `Load()` respingono anche le invocazioni dirette, preservando il checkpoint. I pulsanti **Avvia nuova** e **Annulla** restano disponibili.
2. **Annulla** conserva la sessione precedente in pausa e ripristina le operazioni consentite.
3. **Avvia nuova** crea una sessione v14 in pausa, **sospende ogni autosalvataggio** e lascia disponibile il salvataggio esplicito. Il vecchio checkpoint resta intatto finché il nuovo non viene salvato intenzionalmente.
4. Solo un **Salva v14 riuscito** riattiva l'autosalvataggio periodico e alla chiusura. Anche un **Carica v14 riuscito** ripristina l'autosalvataggio della sessione caricata.
5. Se la finestra viene chiusa **mentre è ancora aperta la conferma**, la chiusura equivale ad **Annulla** e applica il normale salvataggio della partita precedente. Se la conferma è già stata accettata ma la nuova partita non è stata ancora salvata, la chiusura **non sovrascrive** il checkpoint precedente.
6. Un file v14 incompatibile o corrotto continua a non essere sovrascritto: la protezione `_saveAllowed` rimane distinta dall'autosalvataggio volontariamente sospeso.

**Ambito:** solo `V14PreviewViewModel.cs`, `V14PreviewWindow.axaml.cs`, test Desktop e documentazione. Nessun cambio a GUI v13, file XAML, Core, Application, Infrastructure, SQLite, schema DB, costi o tempi.

## Test previsti

- Comandi Salva/Carica/Nuova partita disabilitati durante la conferma; `Save()`/`Load()` diretti non scrivono né mutano la colonia; Annulla ripristina.
- Su checkpoint con 100 punti ricerca, Avvia nuova lascia il file precedente intatto; Salva manuale scrive il nuovo e riattiva l'autosalvataggio.
- Avalonia headless: `V14Save` e `V14Load` visibilmente disabilitati durante la conferma, abilitati dopo Annulla.
- Chiusura con conferma ancora aperta: viene preservata e salvata la partita precedente.
- Chiusura dopo Avvia nuova senza Salva: il checkpoint precedente rimane invariato.

**Gate:** compilazione e 481 test su Windows, prova manuale dei flussi, controllo visivo 1040×740 / 1240×850. Il gate P0 completo della GUI resta aperto.

[Piano test RC17](../validation/G1_D32_RC17_TEST_PLAN.md) · [ADR 0039](../adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md) · [Roadmap](../ROADMAP.md) · [Matrice audit](../validation/G1_D29_AUDIT_GUI_V13_V14.md).
