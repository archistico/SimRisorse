# P1-D47 / RC33 — Audit della filiera Matite

**Data:** 2026-11-04  
**Stato:** implementato; compilazione, xUnit e verifica visiva Windows pendenti.  
**Baseline:** RC30, 511 test confermati. RC31 e RC32 non ancora validate separatamente.

## Perimetro e regole correnti

- **Laboratorio Scuola:** un lotto trasforma **1 Legno lavorato + 1 Carbone in 4 Matite**. Lotti selezionabili: +1 / +5 / +10.
- **Iscrizione:** attualmente **10 Cibo + 2 Matite per studente**, pagati al momento dell'iscrizione. Il requisito minimo di 2 Matite/studente è confermato; gli altri coefficienti attendono E4b.
- **Stoccaggio Matite:** Municipio +1.000, Magazzino +500, Scuola +50, secondo le regole v14 già introdotte. Nessuna modifica alle capacità.
- **Annullamento:** rimborso del Cibo e soltanto delle Matite realmente pagate; le iscrizioni anteriori a RC9 non producono rimborsi fittizi.
- **Persistenza:** lo stock di Matite rimane nella tabella risorse v14; il contatore `PencilPaidStudents` non cambia schema. Slot v13 e v14 sempre separati.

## Audit e intervento RC33

La v14 controllava la possibilità di produrre in due punti indipendenti: `SchoolCanCraft` nel ViewModel e `CraftPencils` nella sessione. Anche se i controlli attuali erano equivalenti per gli stati normali, una futura modifica avrebbe potuto lasciare il pulsante attivo a fronte di un rifiuto del comando.

1. Introdotto `V14CampaignSession.CheckPencilCraft(batches)` **senza effetti collaterali**, usando esattamente la stessa valutazione del comando `CraftPencils` (priorità di errore: quantità → Scuola → materiali → spazio → limiti numerici).
2. `SchoolCanCraft` si affida al controllo della sessione e non riproduce più una propria versione delle condizioni di produzione.
3. `SchoolCraftHint` elenca le carenze effettive di **entrambi** gli input per il lotto +1/+5/+10, con disponibilità, fabbisogno e differenza; in caso di deposito insufficiente indica spazio libero e spazio richiesto.
4. L'esecuzione rifiutata non spende input, non crea Matite e non sostituisce lo snapshot; un lotto è tutto-o-niente.
5. Nessuna modifica XAML, GUI v13, bilanciamento, durata di formazione, motore dei tick o schema SQLite.

## Copertura test aggiunta

**Core/Application (4):** verifica sola lettura e allineamento con comando; rifiuto atomico del lotto +5 senza Carbone sufficiente; rifiuto per spazio e successiva produzione del lotto +1; scenario misto con uno studente legacy e uno nuovo, annullamento dal più recente e rimborsi senza creazione di Matite.

**Desktop (2):** il suggerimento del lotto +5 riporta entrambe le carenze, pulsante disabilitato; deposito con 10 posti liberi rifiuta +20 Matite ma accetta +4, aggiornando scorte e stato del pulsante.

**Totale atteso:** 518 RC32 + 6 nuovi test = **524 test RC33**. Questi test non sono stati eseguiti in questo ambiente. Non attribuire loro esito PASS finché non verrà fornito il log Windows.

## Scelta sulla produzione futura (non attuata)

| Alternativa | Vantaggi | Svantaggi / prerequisiti | Decisione |
| --- | --- | --- | --- |
| **A. Laboratorio manuale nella Scuola** | Semplice, controllabile, non introduce nuove filiere | Molti clic e minore automatismo in una colonia grande | **Attuale, conservata** |
| B. Produzione automatica nella Scuola | Elimina i clic ripetuti, integra il ritmo produttivo | Richiede una regola esplicita per lavoratori, frequenza, input, capienza e priorità | Da valutare dopo E4a |
| C. Edificio dedicato (es. Cartiera) | Filiera riconoscibile, possibile progressione tecnologica | Nuovi costi, dipendenze e test di migrazione, rischio di stalli | Da valutare in una futura espansione |

**Nessun automatismo o nuovo edificio è stato introdotto.** La scelta fra B e C non viene anticipata senza confronto sui dati economici e approvazione.

## Rischi e limiti residui

- La contabilità degli studenti con Matite pagate è aggregata (non per identificativo allievo): si basa sull'ordine già stabilito di completamento e annullamento. La RC33 ne verifica anche un caso misto; un'eventuale futura modifica all'ordine richiederà un riesame della persistenza.
- La verifica statica non sostituisce l'esecuzione xUnit né la prova UI a 1040×740 e 1240×850.
- P1-D48 può cominciare a livello di analisi, ma RC33 resta candidata fino a validazione cumulativa.

[Roadmap](../ROADMAP.md) · [Piano di validazione](../validation/P1_D47_RC33_TEST_PLAN.md) · [ADR 0044](../adr/0044-unica-verifica-laboratorio-matite.md)
