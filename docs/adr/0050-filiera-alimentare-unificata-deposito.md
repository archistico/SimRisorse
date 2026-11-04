# ADR 0050 — Deposito alimentare unificato e filiere alternative

Data: 2026-11-04. Stato: **approvata come direzione progettuale; implementazione gameplay differita a F1-D02–D05**.

## Contesto

Il sistema vigente trasforma solo Pesce in Cibo nel Deposito, lascia il Pane nella scorta cittadina e consuma Cibo/Pane direttamente da quella scorta. L'utente richiede Carne ottenuta da Stalla o Caccia, Frutta e Verdura, oltre a Pesce e Pane; tutti gli alimenti devono transitare per un vero Deposito alimentare.

## Decisione

- Introdurre un inventario alimentare distinto con materie prime (`fish`, `bread`, `meat`, `fruit`, `vegetables`) e Cibo pronto.
- Solo una trasformazione nel Deposito alimentare, limitata da lavoratori/capacità, produce Cibo ordinario. Non usare il prodotto grezzo come Cibo.
- Proteggere una quota di Pane per la razione aggiuntiva delle abitazioni avanzate; convertire in Cibo soltanto l'eccedenza. Mai doppia contabilizzazione.
- Conservare due fonti di carne non indistinte: Stalla (Grano+Acqua → Bovini) e Cacciatore (Selvaggina), entrambe lavorate dal Macellaio con ricette alternative; non aggiungere combobox per le ricette.
- Tenere Orto e Frutteto come edifici distinti; il vantaggio dell'allevamento verrà bilanciato in base alla resa **netta dell'intera filiera**.
- Garantire la sopravvivenza della dotazione iniziale tramite una riserva di Cibo pronto nel Municipio; nuovi alimenti saranno immagazzinati e convertiti soltanto attraverso il Deposito.
- Attivare il modello con versione di regole e migrazione SQLite esplicite, senza cancellare scorte storiche né scrivere nello slot legacy v13.
- Non scegliere ora costi, produzioni, priorità definitive o orizzonti di riserva: richiedono misurazione E4a e decisione E4b.

## Conseguenze

F1-D01 sviluppa un calcolatore puro e testato, **non ancora cablato alla sessione**; i salvataggi RC39 e la partita esistente sono immutati. F1-D02 caccia/allevamento, F1-D03 ortofrutta, F1-D04 deposito+consumi, F1-D05 migrazione/consolidamento. Gli scenari E4a originali rimangono storici e le misure della nuova economia ripartono dopo F1.

[Modello F1-D01](../design/F1_D01_RC40_MODELLO_ALIMENTARE.md) · [Piano di validazione](../validation/F1_D01_RC40_TEST_PLAN.md) · [Roadmap](../ROADMAP.md)
