# ADR 0071 — L'interfaccia v14 è il contratto canonico anche per v15

- **Data:** 05/11/2026
- **Stato:** adottata in RC63, verifica applicativa Windows pendente
- **Contesto:** RC62 rendeva la v15 predefinita, ma la nuova finestra aveva perso scelte di layout e interazione precedentemente validate nella v14. L'utente ha esplicitamente rifiutato di ripetere le decisioni di design da zero.

## Decisione vincolante

1. **Nessuna sostituzione del layout v14** per introdurre una versione economica successiva. Nuovi modelli e comandi devono essere adattati alla stessa presentazione, non il contrario.
2. Le righe di edifici **bloccati** restano grigie, selezionabili e senza azione Costruisci. La diagnosi elenca prerequisiti e scorte mancanti.
3. Il dettaglio mostra costi, produzione, consumi e capacità come **righe con icone delle risorse**. Rispettare tipografia, spaziature e area di diagnosi della v14.
4. I pulsanti **Costruisci +1/+5/+10** e **Lavoratore −1/+1/+5/+10** conservano posizione, quote progressive, abilitazione e visibilità. I livelli Base/Grande/Avanzato sono sempre voci autonome; nessuna ComboBox nel dettaglio.
5. Sidebar: crescita del prossimo abitante (valore, percentuale, fabbisogni Cibo e Pane), cards, clock e navigazione a cinque sezioni.
6. Scheda Risorse: sette colonne, ordinamento alfabetico, quantità arrotondate, saldo, ETA e tooltip di capienza; stessi principi di focus e selezione.
7. Schede Scuola, Esercito, Ricerca: non perdere indicazioni di posti, code, progressi, materiali, limiti e controlli già validati.
8. **Ogni evoluzione della GUI** deve confrontarsi con il contratto v14 (test automatici e verifica visiva). Modifiche intenzionali al contratto richiedono approvazione specifica e ADR, non possono essere implicite.

## Implementazione RC63

- Riportati nella v15 i template di sidebar, card popolazione, edifici, risorse, scuola e militari dalla finestra v14, con binding su ViewModel v15.
- Presenter v15 traduce catalogo, ricette, costi, capienze e progressi dei checkpoint in righe con icone e quote del motore reale.
- Aggiunti test di parità (`V15V14UiParityTests`). È un primo vincolo; per evitare future divergenze valutare un successivo **componente UI condiviso** quando la suite e la resa grafica saranno pienamente validate, senza cambiare l'aspetto.

## Non obiettivi

- Nessun cambiamento alle formule produttive o prezzi E4b, al formato SQLite generation 6 o alla GUI/salvataggio v14.
- La rimozione strutturale del vecchio checkpoint v14 resta in F1-D05e.2b.2.
