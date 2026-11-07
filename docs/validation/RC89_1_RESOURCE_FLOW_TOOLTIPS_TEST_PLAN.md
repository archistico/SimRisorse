# RC89.1 — Piano test tooltip flussi risorse

**Base validata:** RC88.1, 1105 test confermati. **RC89 precedente:** +18 test non confermati. **RC89.1:** +12 test (9 Desktop, 3 Application), **1135 attesi**.

1. `./validate-f1.cmd` su Windows / .NET 10: compilazione con warning come errori, tutte le suite xUnit e gate F1. Non dichiarare superati i test RC89/RC89.1 prima della loro esecuzione.
2. Nella scheda Risorse v15, sorvolare un valore Produzione /s e Consumo /s: verificare fonte, numero di edifici/unità attive, quota di ciascuna e totale uguale alla cella. Verificare nome e valori italiani e il messaggio per flusso zero.
3. Provare filiere con più produttori, output bloccato da spazio/materiali, Deposito alimentare con e senza addetti, consumo abitanti, Pane e Catena carne/orticoltura. Il passaggio di ingredienti al deposito non è consumo e una fonte senza unità attive non produce.
4. Ripetere il sorvolo in v14 e finestra storica. Verificare che click/selezione, scroll, sette colonne, right alignment, ETA e tooltip Capacità siano identici a RC89; niente movimento di elementi durante l'hover.
5. Assegnare/rimuovere lavoratori, avanzare il clock e ricaricare il salvataggio: i tooltip devono aggiornarsi senza mantenere origini del precedente stato, e senza creare risorse o modificare dati salvati.
6. Confrontare i test Core/Application che sommano i contributi con i flussi lordi del clock per Pesce, Cibo pronto, acqua, cereali, ortaggi, bestiame e altre risorse. Non sostituire le metriche con ricette nominali.

**Perimetro:** presentazione e lettura dei dettagli del primo tick; nessuna decisione sull'economia A/B/C. Il checkpoint RC89.1 è candidato finché la suite non viene confermata.
