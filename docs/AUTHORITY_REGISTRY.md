# Registro unico delle decisioni — RC94.2

**Baseline RC93.1:** 1220 test confermati superati dall'utente. **RC94.1:** 1233 eseguiti, 1228 passati, 5 falliti per un test RC81 ancorato a 0 Carbone iniziale. **RC94.2:** test corretto, 1233 attesi; gate Windows da rieseguire. **Dotazione Municipio v15 approvata:** 100 Carbone e 2 Matite alla creazione della nuova partita, nel normale inventario, senza refill ai caricamenti. [ADR 0105](adr/0105-riserva-carbone-municipio-avvio-v15.md).

**ADR 0104 (solo sperimentale):** è autorizzato il checkpoint composito **shadow v15 + escrow Curve A** in memoria per dimostrare conservazione dei versamenti rispetto alla vera capacità/contatori della città. Non autorizza salvataggi modificati, prezzi Curve A in partita, completamento reale della Zecca, nuove interazioni GUI o riempimenti artificiali. `Readiness` è soltanto diagnostica. [ADR 0104](adr/0104-checkpoint-composito-v15-escrow-curva-a-sandbox.md).

**RC93.1 convalidata:** SQLite separato per il vecchio sandbox, transazioni idempotenti e replay; non va mai interpretato come database v15 giocabile. [ADR 0103](adr/0103-persistenza-sandbox-finanziamento-atomica-idempotente.md).

---

## Ordine di autorevolezza

1. Istruzioni più recenti e approvazioni esplicite dell'utente.
2. ADR accettati più recenti (un ADR nuovo può superare quello precedente sullo stesso punto).
3. Questi quattro documenti correnti: [CURRENT_STATE](CURRENT_STATE.md), [ROADMAP](ROADMAP.md), **AUTHORITY_REGISTRY**, [NEW_CHAT_HANDOFF](NEW_CHAT_HANDOFF.md).
4. Stato dei sorgenti e verifiche effettivamente eseguite (non attribuire esecuzione a una semplice asserzione di codice).
5. Analisi, stime, note e release storiche, validi solo nel proprio contesto.

## Contratti approvati / da preservare

| Tema | Regola confermata | Evidenza |
| --- | --- | --- |
| GUI | La v14 validata è il **modello canonico** della v15, senza sostituirla o semplificarla | [ADR 0071](adr/0071-gui-v14-contratto-canonico-v15.md), [PIXEL_ART_UI](PIXEL_ART_UI.md) |
| Edifici | Bloccati in grigio ma selezionabili; livelli Base/Grande/Avanzato come voci autonome, mai ComboBox; pulsanti storici; dettaglio con icone costo, capacità, input, output | [PIXEL_ART_UI](PIXEL_ART_UI.md) |
| Costo nel dettaglio | Un solo elenco con icone sotto “COSTO DEL PROSSIMO EDIFICIO”; **non ripetere** la stessa quantità come testo | RC86, [ADR 0094](adr/0094-starter-matite-e-documentazione-unica.md) |
| Risorse | Sette colonne, ordine alfabetico, saldo e ETA, numeri allineati, quantità intere, capacità e tooltip; **sorvolo su Produzione/Consumo con fonti reali e totali /s** | [ADR 0066](adr/0066-flussi-v15-misurati-dal-clock-e-eta-locale.md), [ADR 0098](adr/0098-tooltip-flussi-risorse-per-fonte.md) |
| Municipio | Capacità minima e risorse iniziali; **nelle nuove v15 anche 100 Carbone e 2 Matite una tantum**, per avviare Scuola e laboratorio Matite; nessun refill al caricamento | [ADR 0105](adr/0105-riserva-carbone-municipio-avvio-v15.md) |
| Ricerca | Scuola, formazione studenti con **2 Matite per iscrizione**, ricercatori assegnati e punti ricerca accumulabili, spesi a scelta; evitare prerequisiti circolari | [ADR 0035](adr/0035-scuola-formazione-matite-v14.md), [ADR 0021](adr/0021-ricerca-saldo-libero-spesa-su-scelta.md) |
| Misure prima ricerca | `V15-I01` termina su `Estrazione I`: non richiedere miniera/carbone dopo lo sblocco; solo ricevute reali possono provare la milestone; i timeout diagnostici non sono prove di blocco strutturale | [ADR 0095](adr/0095-ricerca-iniziale-milestone-e-ricevute-v15.md) |
| Prove industriali | Forgia/Zecca: uno `pass` del benchmark richiede ricevute dei comandi, ricerche pagate e output positivo osservato dal clock; un timeout non prova un softlock | [ADR 0096](adr/0096-audit-filiera-industriale-ricevute-clock-v15.md) |
| Alimentazione | Cibo pronto solo dal Deposito. **Alimentazione I** sblocca Deposito alimentare **Grande** (3 posti e 3× capacità); **Alimentazione II** sblocca Deposito **Avanzato** (10 posti e 10× capacità). Tier autonomi v15, costi progressivi normali, personale e stoccaggio riconciliati | [ADR 0099](adr/0099-tecnologie-e-depositi-alimentari-per-tier.md) |
| Popolazione | Ogni nuova casa accelera gli arrivi, moltiplicatore fino a ×20; lavoratori, studenti e militari non duplicati | [ADR 0013](adr/0013-accelerated-housing-and-fractional-growth.md) e HF04 in `CHANGELOG.md` |
| Zecca | **10 lavoratori per modulo** come obiettivo approvato; implementazione `MintTen` nella v15 ancora opt-in e distinta dal default | [ADR 0089](adr/0089-attivazione-opt-in-squadre-zecca-v15-versionata.md) |
| Economia | Progressione quasi esponenziale molto impegnativa; Zecca equivalente a investimento di circa 100–1000 Tagliapietre come criterio qualitativo, non prezzo applicato | [ADR 0056](adr/0056-progressione-tardiva-e-squadre-minime-proposta.md) |
| Preflight RC89 | Verificare prima/seconda Zecca, capacità iniziale, percorso Magazzini e rate teoriche **prima** di cambiare il prezzo attivo; primo Base come riferimento 100–1000; ogni tempo importato RC88 misura solo i prezzi correnti | [ADR 0097](adr/0097-verifica-pre-attivazione-costi-zecca-e-capienza.md) |
| Finanziamento Curve A RC93 | Persistenza idempotente **solo nel sandbox separato**: transazione unica stock/escrow/ricevuta, replay e revisione; non attiva nel gameplay | [ADR 0103](adr/0103-persistenza-sandbox-finanziamento-atomica-idempotente.md) |
| Salvataggi | Slot v14 personali distinti, checkpoint v15 in SQLite generation 6; possibile rottura controllata dei **vecchi salvataggi sperimentali**, con avviso; non sovrascrivere in modo inatteso | [SAVE_FORMAT](SAVE_FORMAT.md) |
| Spedizioni | Esplorazione distinta dalla conquista; conquista deterministica, perdite iniziali non irreversibili | [ADR 0017](adr/0017-naval-mission-boundaries-proposta.md) |
| Progetto | ZIP completo + test, ADR e documentazione aggiornati a ogni incremento, con stato dei test sincero | [DEVELOPMENT](DEVELOPMENT.md) |

## Decisioni NON approvate o ancora da misurare

Curve di costo A/B/C e coefficienti esatti; moduli multipli/quantità di personale degli altri edifici avanzati; pagamenti frazionati nel gameplay; costi dei magazzini e limiti di capienza per la progressione avanzata; attivazione predefinita `MintTen`; durata della campagna completa. Le analisi RC78–RC89 non equivalgono ad approvazione numerica; il preflight RC89 è un criterio di revisione della capienza, non un assenso alle curve.

## Regola documentale

Esistono **solo** questo registro, `CURRENT_STATE.md`, `ROADMAP.md` e `NEW_CHAT_HANDOFF.md` come documenti operativi dei rispettivi tipi. I vecchi `*_ARCHIVE.md` e i doppi handoff sono stati rimossi. Usare ADR e `CHANGELOG.md` per ricostruire le decisioni storiche, senza ripubblicare file chiamati “corrente”.
