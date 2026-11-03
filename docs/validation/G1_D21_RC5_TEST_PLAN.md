# G1-D21 — Piano di validazione RC5 (Windows)

**Stato: da eseguire.** RC4 confermata verde dall'utente; RC5 candidata, **447 test attesi**, senza compilazione o verifica runtime locale. Nessun controllo grafico automatico può sostituire l'approvazione visiva.

## Test automatici

Estrarre lo ZIP completo in una directory nuova, con SDK .NET 10 e connessione o cache NuGet compatibile. In PowerShell dalla root:

```powershell
.\validate.cmd
```

Controllare build, test xUnit e simulazioni del validatore; conservare riepilogo `.trx`, conteggio totale e warning. In caso di errori non procedere a E4.

## Prova operativa comparativa

1. Avviare `./run-game.cmd` (v13) e `./run-game.cmd --experimental-v14` dopo aver salvato una copia di sicurezza dei database personali. Mettere in pausa, riprodurre screenshot a 1040×740 e 1240×850.
2. **Edifici Base:** Segheria: leggere costi +1/+5/+10. Confrontare +5/+10 con somme dei singoli acquisti **a prezzo crescente e arrotondato per singolo**; non fare semplicemente 5× o 10× il prossimo edificio. Tooltip e testo visibile devono coincidere.
3. **Disponibilità:** se +5/+10 non pagabili, le azioni restano **disabilitate** e le righe del preventivo mostrano materiali posseduti/richiesti; se manca la ricerca per Grande, **niente pulsante Costruisci** ma spiegazione. Il prezzo non deve addebitare risorse al semplice click disabilitato.
4. **Refresh:** acquistare +1, controllare nuovo preventivo e contatore; mantenere la selezione. Avanzare il tempo, osservare aggiornamento delle disponibilità. Ripetere per Scuola e edifici civili quando i prerequisiti sono disponibili.
5. **Upgrade:** con edificio base costruito e tecnologia appropriata sbloccata, spostare scorte sotto la soglia necessaria. Il pulsante di upgrade deve essere **visibile ma disabilitato** e il costo evidenziare il deficit. Senza ricerca, nessun upgrade visibile e testo requisiti leggibile. Verificare percorso diretto all'Avanzato solo con le tecnologie necessarie.
6. **Parità storica:** tabella Risorse con sette colonne e tooltip, Esercito completo, Crescita/Pane, niente ComboBox dei livelli, Scuola e Albero nel menu, hover senza movimento, scroll completo, salvataggi v13/v14 separati, chiusura di un DB incompatibile sempre possibile.

## Chiusura del gate

- [ ] Build/test/validator Windows verdi, conteggio confermato (attesi 447).
- [ ] Scenari RC5 superati su 1040×740 e 1240×850.
- [ ] Le 22 righe della matrice parità P0 hanno esito documentato con screenshot.
- [ ] Nessuna regressione introdotta nella GUI v13.

[Rapporto RC5](../design/G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md) · [Matrice P0](G1_D19_MATRICE_PARITA_GUI.md).
