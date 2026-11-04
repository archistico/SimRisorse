# Piano di validazione RC27 / G1-D41

**Baseline:** RC26 confermata dall’utente (504 test passati). **RC27 candidata: 507 test attesi** (+3 Desktop), compilazione/xUnit locali non disponibili. La validazione RC27 incorpora tutti gli incrementi precedenti.

## Suite automatica Windows

```powershell
.\validate.cmd
```

Risultato richiesto: 507 test superati, nessun errore di build. In caso di errore conservare il primo errore compilatore oppure nome test, messaggio completo e `.trx`; non dichiarare RC27 verificata in assenza del log.

## Audit GUI a due risoluzioni

1. Avviare `./run-game.cmd --experimental-v14` con una copia del salvataggio, impostare **1040×740** e poi **1240×850** a DPI costante.
2. Aprire Edifici; scegliere un produttore, leggere costi e requisiti e scorrere il dettaglio principale. Con diagnosi lunga, scorrere anche il riquadro arancione indipendente.
3. Selezionare un altro edificio: entrambi gli scorrimenti devono azzerarsi, la nuova intestazione deve essere visibile, il nuovo elemento deve risultare selezionato nell’elenco.
4. Tenere selezionato lo stesso edificio e far avanzare il tempo: né il tick né il refresh dei valori devono riportare automaticamente in alto le aree mentre si leggono i dettagli. Provare anche le frecce della tastiera sull’elenco: nessun focus assegnato d’autorità ai pulsanti.
5. Verificare la presenza delle due righe `Costruisci +1/+5/+10` e `Lavoratore −1/+1/+5/+10`, gli upgrade pagabili/non pagabili e la diagnosi alta al massimo 104px, senza cambio di colori e nessun movimento sull'hover.
6. Annotare PASS/FAIL con screenshot delle voci **UI-07, UI-12 e UI-18** della [matrice P0](G1_D39_P0_UI_MATRIX.md), senza dichiararle chiuse per il solo superamento dei test.

**Invarianti:** niente E4, nessun codice Core, nessuna migrazione SQLite, GUI v13 e 7 colonne Risorse intatte.

[Rapporto](../design/G1_D41_RC27_AUDIT_SCORRIMENTO_DETTAGLI.md) · [Roadmap](../ROADMAP.md) · [Handoff](../NEW_CHAT_HANDOFF.md).
