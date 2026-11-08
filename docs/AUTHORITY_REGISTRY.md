> **RC105 candidata — ADR 0116:** SQLite generation 7 *sperimentale*: tabelle città, finanziamento, ricevute e identità nello stesso file; creazione solo dal ramo autorevole verificato, file finale non autorevole e non apribile nel gioco. [ADR 0116](adr/0116-prototipo-sqlite-generation7-citta-e-cantieri.md).

> **RC104 candidata — ADR 0115:** il preflight della futura migrazione verifica ramo autorevole, identità, ricevute e cantieri pendenti; non crea un salvataggio giocabile unico e non autorizza la Curva A in partita. [ADR 0115](adr/0115-preflight-migrazione-salvataggio-unico-e-conservazione-cantieri.md).

> **RC103 (fase precedente) — ADR 0114:** il composito e il registro d'autorità sperimentali conservano lo stesso `campaign_id`; backup e restore lo mantengono, una campagna indipendente non può essere promossa, e tre nuovi trigger proteggono l'identità registrata. Non è ancora un salvataggio giocabile unico. [ADR 0114](adr/0114-identita-stabile-campagna-composita-e-autorita.md).

> **RC102, base storica — ADR 0113:** i compositi registrati hanno un vincolo SQLite persistente che blocca gli accessi diretti ordinari allo stato, alle ricevute e al legame d'autorità. Gli archivi sperimentali mai registrati rimangono scrivibili; questo non è un meccanismo contro la manipolazione intenzionale dello schema o il cambio di host. [ADR 0113](adr/0113-fencing-obbligatorio-scritture-composito-registrato.md).

# Registro unico delle decisioni — RC105

**Baseline RC95:** 1245 test completamente confermati. **RC96–RC105:** gate F1 non ancora convalidato. **RC105:** +15 test Infrastructure, **1360 cumulativi attesi**; nel secondo tentativo Windows del 08/11 la compilazione è riuscita e **1359/1360** test sono passati; il test RC98 sulla rimozione del prerequisito Zecca è stato corretto nel secondo hotfix e attende nuova esecuzione. **Dotazione Municipio v15 approvata:** 100 Carbone e 2 Matite alla creazione della nuova partita, nel normale inventario, senza refill ai caricamenti. [ADR 0105](adr/0105-riserva-carbone-municipio-avvio-v15.md).

**ADR 0106 (solo sperimentale):** il nuovo contenitore SQLite compone in un commit una copia nativa della città v15 e il deposito Curve A, con revisioni e ricevute idempotenti. È un database autonomo **non giocabile**: non autorizza il clock, la costruzione o la modifica del salvataggio normale. [ADR 0106](adr/0106-contenitore-sqlite-composito-v15-finanziamento-isolato.md).

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
| RC98: completamento sandbox | Solo nel contenitore composito sperimentale, una Zecca completamente finanziata diventa un edificio del checkpoint laboratorio; saldo `Committed`, ricevute, tier, contatore e capienza sincronizzati senza doppia spesa. Non è attivo in partita. | [ADR 0109](adr/0109-completamento-atomico-zecca-finanziata-in-sandbox.md) |
| RC95: contenitore composito | Il BLOB del salvataggio nativo v15 e il finanziamento vengono scritti nello stesso commit nel **laboratorio autonomo**, non nel salvataggio giocabile; niente completamenti o rifornimenti ipotetici | [ADR 0106](adr/0106-contenitore-sqlite-composito-v15-finanziamento-isolato.md) |
| Finanziamento Curve A RC93 | Persistenza idempotente **solo nel sandbox separato**: transazione unica stock/escrow/ricevuta, replay e revisione; non attiva nel gameplay | [ADR 0103](adr/0103-persistenza-sandbox-finanziamento-atomica-idempotente.md) |
| Salvataggi | Slot v14 personali distinti, checkpoint v15 in SQLite generation 6; possibile rottura controllata dei **vecchi salvataggi sperimentali**, con avviso; non sovrascrivere in modo inatteso | [SAVE_FORMAT](SAVE_FORMAT.md) |
| Spedizioni | Esplorazione distinta dalla conquista; conquista deterministica, perdite iniziali non irreversibili | [ADR 0017](adr/0017-naval-mission-boundaries-proposta.md) |
| Progetto | ZIP completo + test, ADR e documentazione aggiornati a ogni incremento, con stato dei test sincero | [DEVELOPMENT](DEVELOPMENT.md) |

## Decisioni NON approvate o ancora da misurare

Curve di costo A/B/C e coefficienti esatti; moduli multipli/quantità di personale degli altri edifici avanzati; pagamenti frazionati nel gameplay; costi dei magazzini e limiti di capienza per la progressione avanzata; attivazione predefinita `MintTen`; durata della campagna completa. Le analisi RC78–RC89 non equivalgono ad approvazione numerica; il preflight RC89 è un criterio di revisione della capienza, non un assenso alle curve.

## Punteggio e classifiche (pianificato)

Definire formula e classifica locale **dopo** il bilanciamento, la progressione naturale e il consolidamento del gameplay. **Classifica online tramite API come una delle ultime funzionalità**, con controlli server e privacy; non esiste ancora un algoritmo o una API approvati. [Piano](design/PUNTEGGIO_CLASSIFICA.md).

## Regola documentale

Esistono **solo** questo registro, `CURRENT_STATE.md`, `ROADMAP.md` e `NEW_CHAT_HANDOFF.md` come documenti operativi dei rispettivi tipi. I vecchi `*_ARCHIVE.md` e i doppi handoff sono stati rimossi. Usare ADR e `CHANGELOG.md` per ricostruire le decisioni storiche, senza ripubblicare file chiamati “corrente”.
