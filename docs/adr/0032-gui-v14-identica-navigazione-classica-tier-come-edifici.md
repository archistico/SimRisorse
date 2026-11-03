# ADR 0032 — Presentazione v14 nel layout originale; dimensione = voce edificio

**Data:** 3 novembre 2026. **Decisione UI approvata dall'utente.**

## Motivazione

L'anteprima `v60-E3b4-RC1` ha superato i test di Windows (427/427) ma la GUI non rispetta la presentazione pixel art storica. Il selettore ComboBox di livello nel dettaglio edificio è respinto dall'utente.

## Decisione

1. La navigazione rimane **nella barra laterale sinistra** come nell'interfaccia storica: **Edifici, Risorse, Esercito, Scuola, Albero tecnologico**. Scuola e Albero tecnologico sono voci aggiuntive, non schede orizzontali sostitutive.
2. Il workspace mantiene **elenco edifici a sinistra, dettaglio selezionato a destra**, icone pixel art, palette, stile dei bordi, capacità e produzione per risorsa, barra superiore con abitanti e materiali.
3. **Il livello costituisce una voce edificio autonoma:** per ogni famiglia produttiva `X`, `X grande`, `X avanzato/a` sono tre righe nel catalogo. La loro identità è `(familyId, ProductionTier)`; il database e il modello Domain restano per famiglia e livello, senza creare identità persistenti spurie.
4. Selezionando un edificio si conoscono immediatamente **prezzo del suo livello, numero edifici, lavoratori, ricetta, capacità e prerequisiti**. Il pulsante **Costruisci +1** costruisce direttamente il livello selezionato; **nessuna ComboBox** per taglia o trasformazione compare nei dettagli.
5. Il potenziamento di un edificio già posseduto resta possibile tramite **pulsanti espliciti**: `Base → Grande`, `Base → Avanzato`, `Grande → Avanzato`. Un upgrade conserva i lavoratori assegnati e non incrementa lo storico degli acquisti, secondo i contratti precedenti.
6. Scuola è visibile anche come struttura nell'elenco Edifici per permettere la costruzione coerentemente con gli altri edifici. La voce Scuola a sinistra gestisce iscrizioni e ricercatori. Albero tecnologico gestisce il saldo accumulato e gli sblocchi **volontari**.
7. Non si modifica `MainWindow`, la GUI della v13, il ruleset v13, SQLite v2 o SQLite v3. La v14 conserva attivazione opt-in `--experimental-v14` e salvataggio separato: cambia solo il layout della finestra sperimentale e il viewmodel di presentazione.
8. Un unico layout non significa una doppia economia: tutte le operazioni v14 continuano a richiamare `V14CampaignSession`. I coefficienti economici restano proposte di bilanciamento.

## Conseguenze e vincoli

- 16 famiglie x 3 livelli = 48 voci produttive; più 5 voci storiche non produttive (Municipio incluso) e Scuola = **54 righe**. Tutte condividono lo stesso ordinamento per famiglia e il medesimo dettaglio strutturale.
- Le tecnologie non sbloccate sono presenti, ma segnalate come bloccate con la causa; non si introduce una ricerca automatica.
- L'Esercito v14 resta **in sola lettura** nella RC2, fino al completamento delle azioni militari della nuova GUI; il ruleset v13 conserva tutte le sue funzionalità.
- I dettagli della vecchia UI v13 non vengono alterati per evitare regressioni. Per la RC2 è necessaria una verifica visiva sul PC Windows, oltre ai test Headless.

## Alternative respinte

- Cambiare il livello tramite ComboBox nel dettaglio selezionato.
- Spostare Edifici o Scuola in una struttura di tab orizzontali indipendenti.
- Modificare la GUI v13 prima del completamento e della validazione del ruleset v14.

## Verifiche

`V14PreviewTests`: catalogo con 54 voci, livelli distinti e costi/capacità separati, costruzione diretta, navigazione sidebar 5 sezioni, nessuna ComboBox di livello, isolamento salvataggi, selezione stabile, test di upgrade. **Scritti ma non eseguiti in questo ambiente**.
