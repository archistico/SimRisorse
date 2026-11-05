# ADR 0067 — Riconciliazione fisica degli stock v15 e stress separato

- **Data:** 5 novembre 2026
- **Stato:** implementato nella RC57 candidata; test Windows .NET 10 pendenti
- **Contesto:** RC56 ha **731 test confermati dall’utente**. Il clock espone i flussi lordi per il primo secondo, ma non esiste ancora un audit indipendente utilizzabile su durate prolungate. F1-D05e deve proteggere i passaggi città → transito → Deposito, la ripresa SQLite e il consumo delle razioni.

## Decisione

1. **Ledger fisico, non nuovi rendimenti.** `V15StockAuditor.PhysicalTotals` somma una sola volta città, Bovini/Selvaggina, transito, ingredienti depositati e Cibo pronto; l’eventuale duplicazione non viene mascherata. `ReconcileSecond` verifica `stock finale = stock iniziale + produzione lorda - consumo lordo` **per ogni risorsa al millesimo** usando i flussi del clock. Restituisce differenze esplicite, non modifica il salvataggio e non inventa ricette.
2. **Un secondo, non un’intera finestra.** Il piano RC56 registra i flussi lordi del **primo** secondo: confrontarli con un avanzamento di due ore sarebbe sbagliato. Il verificatore rifiuta avanzamenti che non rappresentano esattamente un secondo; i test e il runner eseguono una riconciliazione ad ogni tick, senza aggiungere un accumulatore persistente o rallentare la GUI normale.
3. **Stress opt-in.** `tools/SimRisorse.Simulation --audit-v15` può verificare fino a **48 ore simulate**, ripartendo da una fixture di laboratorio riproducibile. Non scrive né legge salvataggi personali, non altera l’avvio grafico. È **fuori da `validate.cmd`** perché la durata computazionale dipende dalla macchina. I test SQLite riproducono separatamente una serie di riavvii e confrontano lo stato con l’esecuzione ininterrotta.
4. **Una RC per volta.** Questa è **F1-D05e.1**; lo schema v15 generation 5 continua a contenere uno slot tecnico `food-store` inattivo, perché lo snapshot v14 ereditato e la sua validazione lo richiedono. La rimozione fisica richiede un **nuovo checkpoint indipendente** e una decisione esplicita in D05e.2. In sviluppo si può interrompere la compatibilità dei DB sperimentali (ADR 0060), ma non si cancellano o sovrascrivono i salvataggi personali.
5. **Nessun cambiamento di economia.** Ricette, prezzi +15% e requisiti del personale restano invariati. E4a e poi E4b rimangono le milestone di bilanciamento; Zecca di costo 100–1000 Tagliapietre e squadre minime di 10 lavoratori **non** sono implementate nella RC57.

## Criteri di accettazione

- `validate.cmd` su Windows: **743 test attesi** (731 validati RC56 + 12 nuovi casi).
- Ledger bilanciato ad ogni tick su Pesce/Cibo, trasferimenti senza lavoratori, animali, orticoltura, capienza pronta piena e scorte in eccesso conservate.
- Avanzamento continuo vs spezzato con frazioni di secondo e SQLite restart: stessa popolazione, scorte, progresso, militari e ricerca.
- `--audit-v15 --hours 24` e, facoltativamente, `--hours 48` completano **senza divergenze** e senza accesso al DB della GUI. Non indicare questi stress come eseguiti finché non risultano lanciati sulla macchina Windows.

[Specifica](../design/F1_D05E1_RC57_RICONCILIAZIONE_STRESS.md) · [Test](../validation/F1_D05E1_RC57_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md).


## Addendum RC58 — confronto logico nei test di persistenza (5 novembre 2026)

Il collaudo RC57 ha rilevato 2 fallimenti xUnit su 743: gli edifici per chiave avevano gli stessi valori ma un ordine diverso dopo `ORDER BY building_id` in SQLite. I test RC58 normalizzano il confronto per ID, **senza modificare la persistenza e senza attenuare gli invarianti degli stock**. Anche famiglie, contatori e ID tecnologia vengono confrontati per chiave; i progressi individuali, invece, restano posizionali. **744 casi attesi, da convalidare su Windows**. [Rapporto RC58](../design/F1_D05E1_RC58_FIX_ORDINE_CHECKPOINT.md).
