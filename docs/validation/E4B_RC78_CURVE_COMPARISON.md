# RC78 / E4b.1 — Confronto delle tre curve

Studio analitico eseguito sulla baseline misurata RC77-M01. **Raccomandazione: A come punto di partenza del prossimo bilanciamento**, perché rientra nell'obiettivo iniziale con minore pressione sullo stoccaggio. Nessun coefficiente o requisito di personale è stato installato nel gioco. La raccomandazione non approva i numeri né dimostra che la nuova campagna sia percorribile.

## Prima Zecca Base

| Curva | Pagamento diretto della Zecca | Filiera edilizia senza Magazzini | Magazzini Base richiesti | Filiera compresi Magazzini | Equivalenti di lavoro Tagliapietre, senza / con Magazzini |
| --- | --- | --- | ---: | --- | ---: |
| A | 3.360 tavole + 2.560 pietre | 7.797 tavole + 5.634 pietre | 5 | 8.385 tavole + 5.987 pietre | 303,26 / 325,10 |
| B | 7.143 tavole + 5.442 pietre + 40 Ferro + 20 Oro | 21.947 tavole + 15.918 pietre + 40 Ferro + 20 Oro | 13 | 24.405 tavole + 17.394 pietre + 40 Ferro + 20 Oro | 856,60 / 947,91 |
| C | 11.161 tavole + 8.504 pietre + 120 Ferro + 80 Oro | 24.434 tavole + 18.055 pietre + 120 Ferro + 80 Oro | 21 | 36.326 tavole + 25.193 pietre + 120 Ferro + 80 Oro | 963,19 / 1.404,93 |

La filiera conta prerequisiti una sola volta, include Scuola, alimentazione, abitazioni e supporto industriale per una Zecca da 10 addetti; sottrae gli edifici iniziali, ma non le risorse iniziali. Per B/C include i produttori del Ferro necessario al pagamento. Non comprende Forgia o Esercito non necessari al percorso della Zecca. Le voci sono investimento edilizio lordo: non sono il consumo totale di una partita.

Il riferimento è il lavoro materiale nominale per 30 tavole + 10 pietre = **70 secondi-lavoratore**. La filiera A richiede 21.228 secondi-lavoratore di materiali senza Magazzini. Il confronto non somma pietre, metalli e tavole in una valuta arbitraria. I rapporti di singoli materiali e gli interi vettori sono disponibili nel JSON.

B si avvicina al limite alto del criterio 100–1.000 già per l'ingresso; C lo supera includendo i Magazzini. La curva A lascia più margine per formazione, consumo alimentare, crescita e futura infrastruttura. L'obiettivo non viene esteso automaticamente ai tier superiori: A Avanzata completa richiede già circa 2.662 equivalenti senza Magazzini, da ricalibrare o accettare esplicitamente dopo la simulazione.

## Stoccaggio, lotti e upgrade

| Curva | Zecca Base: lotto 5, tavole | Lotto 5 finanziabile nel percorso Magazzini | Lotto 10, tavole | Lotto 10 finanziabile |
| --- | ---: | --- | ---: | --- |
| A | 19.714 | Sì, 38 Magazzini | 48.679 | No |
| B | 43.612 | No | 113.846 | No |
| C | 75.254 | No | 226.615 | No |

Acquistare singolarmente e pagare un lotto atomico sono operazioni finanziariamente diverse, pur avendo gli stessi prezzi sommati e lo stesso storico finale. Anche A richiede un disegno di stoccaggio migliore per mantenere sempre utilizzabile `+10` sugli edifici finali.

| Curva | Prima Avanzata diretta, tavole | Base→Grande→Avanzata, tavole totali | Massimo pagamento del percorso, tavole | Diretto / upgrade finanziabili |
| --- | ---: | ---: | ---: | --- |
| A | 21.840 | 23.319 | 14.153 | Sì / Sì |
| B | 46.430 | 50.358 | 30.644 | No / Sì |
| C | 111.610 | 126.677 | 89.846 | No / No |

B dimostra perché un pagamento diretto impossibile nel percorso non prova che siano impossibili gli upgrade: questi frazionano la spesa e arrivano entro la capacità di 60 Magazzini. Il percorso C si ferma a 38 Magazzini: capacità tavole 20.050, Magazzino successivo 20.255 tavole. Per B il limite del percorso è 61 Magazzini, capacità tavole 31.550. Lo spazio accessorio dei produttori e altre strategie non sono modellati: nessuno di questi esiti viene chiamato stallo globale.

I percorsi di upgrade a contatore fisso telescopizzano; dopo l'acquisto iniziale con incremento dello storico il totale reale non è più conveniente dell'acquisto diretto. Verificato per tutte e tre le proposte. Non è una prova generale contro arbitraggio da demolizione/migrazione futura.

## Personale e ricerca

| Configurazione candidata | Moduli | Addetti Zecca | Abitanti necessari compreso supporto | Posti abitativi del percorso |
| --- | ---: | ---: | ---: | ---: |
| Una Base | 1 | 10 | 23 | 48 |
| Due Base | 2 | 20 | 36 | 48 |
| Una Grande | 3 | 30 | 49 | 80 |
| Una Avanzata | 10 | 100 | 143 | 144 |
| Due Avanzate | 20 | 200 | 279 | 304 |

I totali comprendono alimentazione/pane a regime, supporto Oro/Carbone, produttori iniziali e un ricercatore; nessun abitante è contato due volte. Non c'è personale libero di riserva. Per pagare i metalli B/C si possono usare temporaneamente lavoratori del supporto; l'ordine e la sostenibilità di questa riallocazione non sono simulati.

Con due Zecche Base, **0/9/10/19/20 assegnati → 0/0/1/1/2 moduli**; input o spazio insufficienti azzerano i moduli non eseguibili e il relativo consumo. Gli addetti incompleti restano riservati. Sono test di una proiezione isolata, non test del clock o dei salvataggi con squadre nuove. I moduli tier 1/3/10 richiedono ancora decisione.

Ricerca proposta mantenuta a 350 punti Base/Grande e 800 Avanzato, pari a 3500/8000 secondi-lavoratore teorici a 0,1 punti/s, esclusa formazione. Nessun ROI temporale, tempo di arrivo o consumo alimentare complessivo è misurato per le proposte.

## Collegamento alle misure naturali

Importati e validati i sei abbinamenti industriali RC77-M01, senza grants né orizzonti ridotti. Con regole correnti: Estrazione I 2520s/2580s, prime Spade/Monete 4563s/4623s (`bread-first`/`research-first`), 3540s di attesa ricerca per industria, 120s formazione, 900/960s materiali. Il ramo I03 paga 1573 tavole/1046 pietre, inclusa una tavola di crafting, più 1 Carbone, 10 Cibo, 2 Matite e 350 punti ricerca. Il suo personale Zecca attuale resta uno: non confrontare i tempi come se avesse già la squadra da dieci.

## Decisione successiva

Scegliere una curva e il significato dei moduli prima di applicare prezzi o squadre. Suggerita A, con verifica della crescita delle copie e dello stoccaggio a lungo termine; B è un'alternativa più onerosa che impone subito il ramo Ferro; C richiede una revisione dello stoccaggio già dal tier Grande. Non combinare silenziosamente parametri di curve diverse.

Seguono implementazione versionata, scenari naturali della curva scelta, concorrenza del personale, salvataggio/ripresa e migrazione. Il confronto interattivo Windows richiesto per promuovere formalmente F1 rimane distinto.

[Metodo](../design/E4B_RC78_THREE_CURVES_STUDY.md) · [Prove e artefatti](E4B_RC78_TEST_PLAN.md).
