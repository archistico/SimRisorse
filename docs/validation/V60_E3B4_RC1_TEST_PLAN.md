# Piano test · v60-E3b4-RC1

**Baseline confermata su Windows:** RC5, 417 test passati. **Nuovo incremento:** 10 test Desktop addizionali; **427 test attesi**. Non compilati qui per mancanza di .NET 10.

## Comando

```powershell
.\validate.cmd
```

I 10 casi Desktop verificano:

1. Il flag v14 non compare nell'avvio ordinario, richiede lo scenario civile, ha un percorso differente e vieta il percorso v13 standard.
2. Prima apertura in pausa: 16 famiglie, 11 tecnologie, nessuna ricerca automatica, nessun file scritto.
3. Tentativo senza prerequisiti Scuola e acquisto di Logistica differita: stato e scorte invariati.
4. Salvataggio/ricarica della simulazione e conservazione delle selezioni nel viewmodel.
5. Costruzione produttiva Base, assegnazione/rilascio, upgrade senza ricerca bloccato, prezzo e persistenza.
6. Costruzione Casa piccola: crescita capacità senza creare immediatamente abitanti e incremento del prezzo successivo.
7. V13 esistente in posizione v14: il file non viene sovrascritto.
8. Database v13 separato: i byte restano identici dopo il salvataggio v14.
9. Headless Avalonia: quattro schede e comandi effettivamente visibili.
10. Chiusura normale della finestra headless: scrittura esclusiva del database v14 e ripartenza in pausa.

## Checklist grafica manuale

- Avviare `.\run-game.cmd --experimental-v14` e verificare layout 1160×820, minimo 980×670, assenza sovrapposizioni e tooltip/messaggi leggibili.
- Aprire le quattro schede, selezionare un produttore a metà lista, far scorrere il gioco: selezione e scrollbar non devono saltare.
- Provare costruzione di un primo Pescatore e del Deposito alimentare, accumulare materiali per Scuola, iscrivere studenti, attendere diplomi, assegnare ricercatori, accumulare punti senza progetto attivo e acquistare una ricerca quando sufficienti.
- Sbloccare un livello Grande, costruire e potenziare una Segheria; controllare la preservazione dei lavoratori e il prezzo mostrato.
- Provare pausa/minimizzazione/sospensione, salvataggio, ricarica e chiusura. Lo stato deve ripartire in pausa e non maturare durante la chiusura.
- Avviare `run-game.cmd` senza argomenti e verificare che tutti i menu e salvataggi v13 siano quelli precedenti.
- Evitare test su database personali; usare directory nuove e copie.

**Condizioni di completamento:** 427/427 test automatici, smoke GUI confermato, confronto dati, eventuali warning analizzati. Il passaggio ad E3b/4b è consentito per sistemare la GUI; l'attivazione definitiva della v14 richiede E4 e approvazione dei numeri.
