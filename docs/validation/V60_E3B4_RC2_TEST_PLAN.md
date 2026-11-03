# Validazione v60-E3b4-RC2

**Precedente RC1:** 427 test Windows superati secondo l'utente.

**RC2 candidata:** 430 test attesi; compilazione e xUnit non eseguiti nell'ambiente di preparazione.

## Test automatici

Estrarre il progetto in una nuova cartella Windows 11 ed eseguire:

```powershell
.\validate.cmd
```

La suite `V14PreviewTests` verifica inoltre che il catalogo v14 includa tre righe per ciascuna delle 16 famiglie, con prezzi e capacità propri; che la selezione del livello determini la costruzione; che la ricerca mancante blocchi l'azione senza perdita risorse; che l'upgrade utilizzi comandi diretti senza alterare la selezione; che Scuola e Albero siano presenti; che la sidebar abbia cinque voci e che non esistano `V14BuildTier`, `V14UpgradeFrom` o `V14UpgradeTo` nella UI. Gli altri test v13 rimangono inclusi.

## Controllo visivo e funzionale

1. `\.\run-game.cmd --experimental-v14`; verificare che il layout sia analogo a quello del gioco storico: sidebar scura, lista Edifici e pannello dettagli, icone pixel art e scorte superiori.
2. Sidebar Edifici, Risorse, Esercito, Scuola, Albero tecnologico (non quattro schede orizzontali).
3. Selezionare Segheria, Segheria grande, Segheria avanzata: tre voci, nessuna ComboBox, costi e capacità distinti, pulsante Costruisci +1.
4. Edifici avanzati bloccati indicano chiaramente la ricerca mancante. Il comando non deve creare un edificio o spendere risorse.
5. Provare gli upgrade espliciti dalla Segheria base a Grande o Avanzata, dopo lo sblocco, e la conservazione dei lavoratori.
6. Nella Scuola verificare studenti, posti e ricercatori; nell'Albero il saldo cumulabile e l'acquisto volontario.
7. Salvare e ricaricare: selezione e voci stabili, sessione ripresa in pausa, salvataggio v14 su percorso separato. Avviare normalmente senza flag e verificare la GUI v13 e il suo DB intatti.

**In caso di problemi:** inviare log completi e una schermata della nuova GUI con la voce Segheria grande selezionata.
