# Gate v53 — capacità uniforme e dettagli degli edifici

**28/10/2026 — Preparato, ancora da eseguire su Windows.** Si tratta di una modifica delle regole economiche e dell'interfaccia, non del Cantiere o delle navi. Baseline `civil-growth-11`, 20 edifici e 16 risorse, **280 test xUnit attesi** (276 preesistenti e 4 nuovi casi); nessun SDK .NET disponibile nell'ambiente di preparazione.

## Contratto funzionale

1. **Capacità base:** ogni risorsa nel nuovo catalogo, compreso il Pane, dispone di **1.000 unità**. Ogni Magazzino aggiunge esattamente **500 unità a ciascuna risorsa**. Il Deposito alimentare continua a trasformare Pesce in Cibo e non incrementa i depositi.
2. **Lotti indivisibili:** il Forno continua a richiedere **tre posti liberi per ogni addetto attivo** per produrre 3 Pane; Forgia e Zecca richiedono rispettivamente 2 e 3 posti. Il messaggio di diagnosi del blocco include quantità, capienza, spazio libero e soluzione.
3. **Tooltip Capacità:** il valore nella riga Risorse espone, al sorvolo, `quantità esatta / capacità`, spazio libero, `capacità iniziale + numero Magazzini × bonus`, la soluzione per aumentare la capienza e — per Pane, Spade e Monete — il lotto del produttore. Il valore rimane numerico nella tabella. Non è più visualizzata la spiegazione persistente sotto l'elenco.
4. **Dettaglio Edifici:** la riga «CAPACITÀ PER EDIFICIO» ha icona pixel 14×14, testo normale 12 px e lo stesso template di costo/produzione/consumo. Il Magazzino mostra +500 per ogni risorsa, le Case +8/+32 posti abitativi, la Caserma +2 posti di addestramento; gli altri edifici indicano «Nessuna capacità aggiuntiva». Il precedente campo `Recipe` resta stabile per la compatibilità dei test e degli altri consumatori.
5. **Salvataggi:** `civil-growth-10 → civil-growth-11` aggiorna solo la regola della capienza. Non cambia scorte, case, operai, militari, coda/addestramento, tempo o progresso frazionario. Il primo salvataggio dopo la conversione crea il backup S1 della v10. Il catalogo storico v10 mantiene Pane base 100. Il file e lo snapshot rimangono **formato 2**, con `military_state` obbligatorio sia in v10 sia in v11; una versione sconosciuta non viene accettata come militare.
6. **Flotta:** tutte le proposte N-01…N-07 restano non approvate. Se approvate, l'incremento navale userà la **v12** e dovrà aggiungere un proprio stato persistente, indipendente dalla modifica v11 della capienza.

## Verifica esecutiva

Sul PC Windows, dall'ultima cartella sorgente estratta, eseguire:

```powershell
.\validate.cmd
```

Attesi **0 errori di compilazione**, **280/280 test riusciti**, controllo dei cataloghi, timeline storiche congelate, benchmark e smoke. Non si può dichiarare il gate superato finché non viene riportato un log di esecuzione valido.

Verificare inoltre manualmente a **1040×740** e **1240×850**:

- Risorse → Pane: **1.000** nella colonna Capacità; dopo un Magazzino **1.500** come le altre risorse; tooltip con formula `1.000 iniziale + 1 Magazzino × 500` e indicazione dei 3 posti liberi per lotto.
- Risorse: scrollbar verticale affiancata a «Si riempie tra», nessuna sovrapposizione; selezione della riga ocra ancora funzionante e riquadro sotto la tabella assente.
- Edifici → Magazzino, Casa piccola, Casa grande, Caserma, Forno e Deposito alimentare: riga Capacità visibile, con icona e stessa dimensione del testo di produzione/costi; distinzione corretta fra stoccaggio, alloggi, formazione e assenza di bonus.
- Nuova partita e caricamento **di un salvataggio v10 preesistente**: gli oggetti, le reclute e i progressi rimangono, la capienza del Pane cresce senza cambiare la sua quantità. Dopo il primo salvataggio v11 la copia `.civil-growth-10.backup.sqlite3` esiste ed è riapribile come checkpoint storico.

Se fallisce un test, riportare la **prima eccezione completa con nome del test e riga**, oltre al riepilogo `dotnet` (inclusi gli output dei benchmark se il blocco è successivo ai test).
