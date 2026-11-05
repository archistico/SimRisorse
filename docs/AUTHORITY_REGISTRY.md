> **RC78 / ADR 0086:** studio isolato concluso, tre curve confrontate, **926/926 test verdi** e replay di cinque output identici. La raccomandazione A è una proposta: coefficienti e moduli tier non sono approvati né applicati. [Confronto](validation/E4B_RC78_CURVE_COMPARISON.md), [decisione tecnica](adr/0086-confronto-curve-economiche-isolato.md), [verifiche](validation/E4B_RC78_TEST_PLAN.md). Servono scelta dei parametri e implementazione versionata prima delle nuove misure naturali.

> **RC77-M01:** misure naturali complete eseguite: 9/9 obiettivi osservati, senza grants, con 21/21 confronti di file identici. [Rapporto e limiti](validation/E4A_V15_RC77_M01_NATURAL_MEASUREMENTS.md). Questi dati forniscono la baseline E4b ma non approvano nuovi coefficienti né dimostrano una durata di campagna completa.

> **RC77 / ADR 0085:** produzione non realizzata misurata dal clock e recupero controllato con grants dichiarati, senza modificare regole o GUI. **899/899 test verificati** e sei run completi misurati; [decisione](adr/0085-misure-produzione-non-realizzata-e-recupero-controllato.md), [evidenze](validation/E4A_V15_RC77_TEST_PLAN.md). Il confronto interattivo F1 rimane distinto.

> **Richiesta utente HF04:** crescita v15 fino a ×20, nuove icone Bovini/Pane/Pescatore/Tagliapietre, riepilogo Abitanti/Cibo/Legno lavorato/Pietra, pulsanti Carica/Salva senza suffisso. Questi cambiamenti sono autorizzati e aggiornano il riferimento grafico e il limite ×2 precedenti. **885/885 test verificati nella HF04**; [dettagli ed evidenze](validation/RC76_HF04_ICONS_AND_GROWTH.md).

> **Storico RC76 / ADR 0084:** gli ammanchi dei benchmark e le saturazioni sono osservazioni, non diagnosi di stallo strutturale. 867 test erano attesi nella consegna originale. [Dettaglio](adr/0084-telemetria-vincoli-benchmark-v15-senza-diagnosi-automatica-stallo.md).

> **RC75 / ADR 0083:** benchmark avanzati naturali con due politiche; il monitoraggio di costi materiali e punti ricerca non modifica i coefficienti economici. 857 test cumulativi attesi, non eseguiti. [Dettaglio](adr/0083-benchmark-industria-ricerca-v15-senza-grants.md).

> **RC72 / ADR 0080:** gate finale F1 preparato ma non eseguito; 826 test cumulativi attesi. La GUI approvata RC63 resta canonica e deve essere verificata visivamente prima di chiudere F1. [Dettaglio](adr/0080-gate-f1-d05e-e-evidenza-riproducibile.md).

> **Aggiornamento tecnico RC71:** il planner v15 non richiede un `BuildingState` fittizio per `food-store` (vedere [ADR 0079](adr/0079-planner-v15-senza-slot-produttore-ritirato.md)); nessuna nuova decisione grafica o di bilanciamento. RC71 822 test attesi, RC63 ultimo gate verificato.

# Registro delle decisioni vigenti — 05/11/2026

## Precedenza

1. Richieste e approvazioni esplicite dell'utente più recenti.
2. ADR accettati, con i successivi che aggiornano quelli precedenti sul medesimo punto.
3. [Stato](CURRENT_STATE.md), [roadmap](ROADMAP.md) e [handoff](NEW_CHAT_HANDOFF.md) attuali.
4. Codice e prove effettivamente eseguite; le versioni candidate non equivalgono a checkpoint confermati.
5. Studi, valori dimostrativi, screenshot e note storiche.

## Vincoli confermati

| Area | Regola | Riferimento |
| --- | --- | --- |
| GUI | Interfaccia v14 è **riferimento canonico** della v15, non semplificarla | [ADR 0071](adr/0071-gui-v14-contratto-canonico-v15.md) |
| Edifici | Bloccati in grigio consultabili, dettaglio con icone, Base/Grande/Avanzato come edifici autonomi, pulsanti e layout validati | [PIXEL_ART_UI](PIXEL_ART_UI.md) |
| Risorse | Sette colonne, ordine alfabetico, numeri a destra, quantità arrotondate, tooltip capienza, saldo ed ETA | [ADR 0066](adr/0066-flussi-v15-misurati-dal-clock-e-eta-locale.md) |
| Popolazione | Crescita visibile sotto il menu; nessun doppio conteggio di lavoratori, studenti o militari | [ADR 0071](adr/0071-gui-v14-contratto-canonico-v15.md) |
| Alimentazione | Deposito esclusivo per Cibo pronto; cinque ingredienti, Pane avanzato, scorte con proprietari distinti | [ADR 0063](adr/0063-comandi-v15-cibo-esclusivo-e-catalogo-di-presentazione.md) |
| Partite | DB personale v14 distinto e preservato; DB sperimentali v15 possono rompere compatibilità | [ADR 0060](adr/0060-salvataggi-di-sviluppo-e-checkpoint-filiera.md), [ADR 0069](adr/0069-generazione6-senza-famiglia-produttiva-legacy.md) |
| Ricerca | Scuola, formazione individuale, Matite, punti ricerca accumulabili e spesa volontaria | [ADR 0021](adr/0021-ricerca-saldo-libero-spesa-su-scelta.md) |
| Sviluppo | A ogni incremento, documentazione/roadmap/handoff/test e ZIP integrale; mai dichiarare un test non eseguito come superato | [guida](DEVELOPMENT.md) |

## Direzione approvata, **numeri non ancora determinati**

- Progressione economica assai più impegnativa, approssimativamente esponenziale per era/complessità (non solo per copie acquistate). La prima Zecca va confrontata con l'investimento-base di **100–1.000 Tagliapietre**: criterio di confronto, **non prezzo già approvato**.
- **Zecca: 10 lavoratori per unità produttiva**; due unità al pieno regime richiedono 20 addetti. Gli attuali tier con `1/3/10` posti non soddisfano il requisito: distinguere posti da moduli e squadre, verificare sul benchmark prima di fissare parametri. [Specifica E4b](design/E4B_P00_COSTI_PROGRESSIVI_E_SQUADRE_MINIME.md).
- Costruzione navale, esplorazione distinta dalla conquista, conquista deterministica, nessuna perdita irreversibile iniziale: pianificate in M5.4–M5.7, non ancora implementate in v15.

## Decisioni sospese

Coefficienti dei prezzi; costi della prima Zecca per ogni risorsa; personale per tier; criteri temporali di progressione; ricette/effetti avanzati non specificati; risultato della rimozione del contatore storico `food-store`; strategia dettagliata per flotta/isole/vittoria. Sono da misurare o decidere negli appositi gate, **non dedurre** dagli ADR di proposta.

**Stato di prova:** RC63 771/771 confermati; RC70 818 previsti, da validare. Modello nativo del checkpoint introdotto, senza modifica della GUI.

- **ADR 0081**: benchmark naturale v15 separato dal benchmark v14 e dagli stress sintetici; esiti osservati, policy versione e nessun finanziamento artificiale ([testo](adr/0081-runner-naturale-v15-separato-dai-benchmark-v14.md)).

- **ADR 0082:** report natural v15 interpretati solo in base a prove osservate. Timeout e blocked della policy non dimostrano softlock; costi RC73 parziali planks/stone. [Decisione](adr/0082-analisi-basata-su-evidenze-benchmark-v15.md).
