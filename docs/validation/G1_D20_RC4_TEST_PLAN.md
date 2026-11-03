# G1-D20 — Piano prova RC4, diagnostica Pane e parità GUI

**AGGIORNAMENTO 03/11/2026:** l’utente ha successivamente confermato **test RC4 passati** (443 attesi, nessun conteggio `.trx` comunicato); il piano di confronto grafico rimane da eseguire. La versione più recente è RC5: [piano RC5](G1_D21_RC5_TEST_PLAN.md).

**Non eseguito su Windows.** Preparare backup dei file personali prima di un avvio manuale. I test automatici creati sono **quattro**; target indicativo della soluzione: **443**, da verificare mediante test discovery. I precedenti 439 test RC3 non sono confermati.

## 1. Test automatici

In Windows 11 / PowerShell, dalla root estratta:

```powershell
.\validate.cmd
```

Conservare output/.trx, conteggio, warnings/analyzer ed eventuali screenshot. Le nuove suite sono `V14BreadSupplyParityTests` (Application, 3 test) e `V14UiParityRegressionTests.V14FirstAdvancedArrivalShowsTheSameDetailedBreadWarningAsV13` (Desktop, 1 test).

## 2. Riproduzione del Pane nella GUI

Avviare `./run-game.cmd` e `./run-game.cmd --experimental-v14`; mettere in pausa entrambe le partite, senza usare lo stesso salvataggio. Riprodurre per quanto possibile gli stessi stati nelle due campagne e fotografare la sidebar a 1040×740 e 1240×850.

1. **Primo arrivo avanzato:** 8 abitanti, una Casa piccola (8 posti di base), Casa grande costruita dopo i suoi prerequisiti (Forno incluso), zero Pane; atteso avviso «Serve Pane in scorta per il primo residente avanzato» **prima dell'arrivo**. Il Cibo mancante non deve nasconderlo.
2. **Nessun addetto:** almeno un residente avanzato, Forno senza lavoratore; attesa diagnosi di capacità nominale insufficiente e numero di addetti necessari.
3. **Mancano gli input:** Forno con lavoratore assegnato, farina o acqua insufficiente; diagnosi `input insufficienti`, non `Produzione sufficiente`.
4. **Spazio Pane esaurito:** Forno con farina, acqua e lavoratore, stock Pane alla capacità; diagnosi specifica «Forno fermo… spazio… Magazzino», non «capacità nominale».
5. **Filiera alimentata:** Forno con input/spazio/lavoratore; previsione sufficiente; il valore della riserva senza produzione è coerente con scorte e consumo del prossimo secondo.
6. La tabella Risorse continua a mostrare **sette colonne**, con capacità Pane normale e tooltip Municipio/Magazzino/edifici locali. Verificare testo in sidebar e footer senza sovrapposizioni.

## 3. Resto del gate P0 (ancora tutto aperto)

Ripetere tutti i 22 casi della [matrice parità](G1_D19_MATRICE_PARITA_GUI.md): Risorse, Edifici/Costruisci, Esercito, Scuola, Albero tecnologico, pulsanti disabilitati, focus, scroll e selezione. Verificare salvataggi separati e rifiuto del DB errato; nessun test deve utilizzare il profilo SQLite personale come fixture.

Esito manuale di ogni riga: **Pass/Fail/Pending**, con dimensione finestra, passaggi e screenshot. Non dichiarare «GUI validata» finché la matrice non è completa.
