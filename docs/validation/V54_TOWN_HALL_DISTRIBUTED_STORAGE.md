# SimRisorse v54 — Municipio e stoccaggio distribuito

**28 ottobre 2026 — Implementazione preparata, gate Windows pendente.** Regole attive `civil-growth-12`. L'ambiente di preparazione non dispone di `dotnet`: non attribuire a questo pacchetto alcuna compilazione o test già superati.

## Contratto approvato

- All'avvio ci sono **8 abitanti, 120 Tavole, 80 Pietra, 30 Cibo**, un Taglialegna, una Segheria, un Tagliapietra, una Casa piccola e **un Municipio**. L'assegnazione di beni avviene una sola volta, nella creazione dello scenario.
- Il **Municipio è unico, fisso, non costruibile** e offre +1.000 per ciascuna delle 16 risorse. Base invisibile `GameScenario.Capacity=0`; nessun inventario privato.
- Ogni produttore offre +50 **per ogni risorsa prodotta**; Casa piccola +10 Cibo/+10 Pane, Casa grande +25 Cibo/+25 Pane, Caserma +20 Spade; il Magazzino continua a offrire +500 a ogni risorsa. I bonus sono cumulativi per conteggio degli edifici.
- Municipio è **fuori dal conteggio dei 22 edifici-obiettivo** della prima campagna. Ci sono ora 21 strutture nel catalogo, di cui 20 appartenenti ai 22 obiettivi (mancano Cantiere e Castello).
- La tabella Risorse resta ordinata alfabeticamente, con quantità arrotondata all'unità; il tooltip della Capacità mostra Municipio, Magazzini e singoli produttori/abitazioni pertinenti. La capacità compare nel dettaglio Edifici usando le icone e la tipografia del dettaglio produttivo. Aggiungere stoccaggio a un produttore non deve nasconderne lo stato di attività.
- `civil-growth-11 → civil-growth-12`: il Municipio viene aggiunto in numero **1**, operai 0, senza consegnare nuove risorse. Scorte, tempo, popolazione, edifici precedenti e stato militare restano invariati. `military_state` obbligatorio in v10/v11/v12; backup SQLite **S1** prima del primo salvataggio con nuove regole. Schema e snapshot rimangono v2. Le factory storiche v10/v11 non devono cambiare.

## Valori verificabili a partita appena creata

| Risorsa | Capacità attesa | Motivo |
|---|---:|---|
| Tronchi | 1.050 | Municipio 1.000 + Taglialegna 50 |
| Tavole | 1.050 | Municipio 1.000 + Segheria 50 |
| Pietra | 1.050 | Municipio 1.000 + Tagliapietra 50 |
| Cibo | 1.010 | Municipio 1.000 + Casa piccola 10 |
| Pane | 1.010 | Municipio 1.000 + Casa piccola 10 |
| Altre undici | 1.000 | Municipio 1.000 |

Esempio: **2 Forni + 1 Magazzino + 2 Case piccole** comportano capienza Pane **1.620**, contando anche il Municipio: 1.000 + 100 + 500 + 20.

## Validazione Windows

Nella cartella principale del pacchetto eseguire:

```bat
.\validate.cmd
```

Risultato atteso: restore, build Release e **287 test xUnit** (nessun errore; conteggio da verificare nel log). Non è ancora un risultato confermato.

Controllare poi in desktop:

1. **Nuova partita:** Municipio in cima alla lista, conteggio 1, capacità +1.000 per ogni risorsa; nessun bottone di costruzione disponibile per lui. Le risorse di partenza sono esattamente 120 Tavole, 80 Pietra e 30 Cibo.
2. **Risorse:** tavole/tronchi/pietra a 1.050, Cibo/Pane a 1.010; tooltip di Pane indica Municipio e Casa piccola, oltre al Magazzino da 0×500. Dopo un Magazzino, Pane 1.510 e Tavole 1.550.
3. **Edifici:** dettaglio Forno mostra Pane +50 con sua icona 14 px e testo 12 px; Case mostrano anche Cibo e Pane, Caserma anche Spade; le etichette di produzione/consumo restano corrette. Un Forno fermo per ingredienti o spazio deve ancora visualizzare il motivo.
4. **Salvataggio storico v11:** caricare un checkpoint reale dopo averne conservato una copia; verificare Municipio1, stocks invariati, nessun secondo bonus di risorse e copia `*.civil-growth-11.backup.sqlite3` al primo salvataggio. Riavviare e assicurarsi che il Municipio rimanga uno solo.
5. **Interfaccia 1040×740 e 1240×850:** riga Capacità e scrollbar non si sovrappongono; navigazione, selezione ocra e scheda Esercito funzionano come in v53.

Se `validate.cmd` fallisce, trasmettere **il primo errore di compilazione oppure il primo test fallito** con nome, messaggio e stack. Non aggiornare ulteriormente le regole prima di correggere il gate.

## Roadmap

Dopo validazione v54, riprendere la scelta non ancora approvata per Cantiere/Flotta: una sua futura versione di regole sarà **`civil-growth-13`**, non v12. Nessuna nuova meccanica navale è inclusa nel pacchetto v54.
