# G1-D22 — RC6: piano di prova diagnostica produttiva

**Stato: da eseguire su Windows**. SDK .NET 10, cache NuGet e ambiente grafico non disponibili nella sessione di confezionamento. La conferma test di RC4 non si estende a RC5/RC6.

## 1. Suite automatica

In una **cartella pulita** dopo aver estratto lo ZIP RC6:

```powershell
.\validate.cmd
```

**Obiettivo atteso:** 451 test (447 previsti in RC5 + 4 nuovi in RC6); non sostituire il dato reale con questa stima. Salvare `.trx`, riepilogo, eventuali warning/errori e codice di uscita. In caso di test falliti, correggere la regressione prima di E4.

## 2. Riscontri funzionali RC6

1. In v14 selezionare la **Segheria Base** appena iniziata la partita, senza Tronchi: il box diagnosi deve indicare mancanza di **Tronchi**, filiera non produttiva e quantità degli addetti che possono lavorare; la struttura rimane costruita.
2. Aggiungere Tronchi, ma esaurire la capacità di **Legno lavorato**: leggere produzione bloccata, quantità attuale, capacità, posti liberi, spazio necessario per lotto e suggerimento Magazzino (+500 per risorsa).
3. Con un **Forno** assegnato, Farina/Acqua presenti e capienza Pane piena: messaggio di **Spazio insufficiente per Pane**; non proporre il Deposito alimentare come contenitore del Pane, spiegare Forno/Magazzino/Municipio.
4. Ridurre contemporaneamente le scorte per **Costruisci +1** e gli input di una filiera già attiva: mostrare **entrambi** i problemi, non far sparire la diagnosi produttiva sotto quella del costo.
5. Avere livelli Base e Grande della stessa famiglia con lavoratori: la misura `runnable/assigned` deve essere etichettata **di filiera / tutti i livelli**, non inventare l’attività di un singolo livello.
6. Con zero addetti sul livello selezionato: avviso per assegnare personale; nessuna costruzione o addebito automatico.
7. Dopo un tick, cambio scheda e riapertura, cambiano i numeri correttamente senza perdere la selezione; il pannello diagnostico non copre i pulsanti.

## 3. Parità v13/v14 e sicurezza

Ripetere la [matrice di parità a 22 punti](G1_D19_MATRICE_PARITA_GUI.md), soprattutto Risorse (sette colonne, hover/tooltip), Esercito, build bloccati, +5/+10 e upgrade RC5, filiera Pane RC4, zoom/DPI, scroll e layout **1040×740 e 1240×850**. Nessuna modifica attesa nella GUI v13 e nei DB personali. Usare `./run-game.cmd` e `./run-game.cmd --experimental-v14` con copie di sicurezza dei salvataggi.

## Gate di chiusura

- [ ] Suite RC5+RC6 verde, conteggio reale comunicato.
- [ ] Scenari RC6 superati.
- [ ] Matrice P0 verificata su Windows con screenshot quando pertinenti.
- [ ] Nessuna regressione nel gioco normale v13; salvataggi isolati.

[Rapporto D22](../design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md) · [Handoff](../HANDOFF.md).
