# M4.13 — Audit aritmetico degli incentivi delle case

Data: 2026-10-27. **Stato: analisi statica per v27, non benchmark eseguito.** Il più recente gate Windows riferito superato dall'utente è **v24/M4.10** (174 test xUnit attesi); il prototipo grafico v25 e il lettore dei risultati v26 attendono verifica. Nessuna regola, ricetta, migrazione o interfaccia cambia con questo dossier.

## 1. Quesito di bilanciamento

La scelta approvata è che **ogni casa aggiuntiva aumenti la velocità di arrivo degli abitanti**, oltre a offrire posti. Il tetto è **2×**, ossia in condizioni ideali un arrivo ogni **30 secondi di gioco valido** invece di 60. Il sistema conserva la parte frazionaria del progresso e non accumula crescita quando la colonia è bloccata.

Per capire se una Casa grande convenga davvero, occorre distinguere tre benefici diversi:

1. **Velocità**: quanto tempo passa prima del prossimo abitante, finché ci sono risorse e spazio.
2. **Capienza**: quanti abitanti si possono ospitare prima di dover costruire altro.
3. **Sostenibilità**: quanti lavoratori, materiali e filiere alimentari occorrono per alimentare la popolazione ospitata.

Il ragionamento deriva dalle definizioni effettive di [HousingGrowthRules](../../src/SimRisorse.Core/Population/HousingGrowthRules.cs) e [CivilGrowthScenario](../../src/SimRisorse.Application/Scenarios/CivilGrowthScenario.cs), e dal [piano di crescita](../../src/SimRisorse.Core/Population/PopulationPlanner.cs). Non assume una sequenza di acquisto né una simulazione del Core.

## 2. Formula e combinazioni al tetto

Sia `s` il numero di **Case piccole acquistate**, oltre alla prima Casa piccola gratuita, e `g` il numero di **Case grandi**. Il gioco usa interi in ventesimi:

```text
unità di progresso/secondo = min(40, 20 + 3×s + 6×g)
moltiplicatore                = unità/20
posti                        = 8×(1+s) + 32×g
costo case (legno/pietra)    = 80×s + 160×g / 40×s + 80×g
```

La condizione per raggiungere 2× è `3×s + 6×g ≥ 20`; poiché i conteggi sono interi, equivale a **`s + 2×g ≥ 7`**. Ogni Casa grande fornisce quindi lo stesso bonus di **due** Case piccole e costa esattamente quanto due piccole, ma offre **quattro volte** i posti di una piccola.

Queste sono **tutte** le combinazioni con la spesa abitativa minima per raggiungere il tetto (sette unità di costo equivalenti):

| Piccole comprate | Grandi comprate | Case totali (inclusa l'iniziale) | Velocità | Posti complessivi | Legno lavorato | Pietra |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 7 | 0 | 8 | 2× | **64** | 560 | 280 |
| 5 | 1 | 7 | 2× | **80** | 560 | 280 |
| 3 | 2 | 6 | 2× | **96** | 560 | 280 |
| 1 | 3 | 5 | 2× | **112** | 560 | 280 |

Le quattro alternative hanno **la medesima spesa per le sole case**, non il medesimo costo economico totale. Inoltre non sono quattro passaggi di upgrade: la **demolizione non esiste**, dunque una composizione già costruita non può essere convertita gratuitamente in un'altra.

Quattro Case grandi senza altre piccole costano **640 legno lavorato e 320 pietra**; il bonus grezzo sarebbe 2,20× ma viene fermato a **2×**, con **136 posti**. Rispetto a **tre** grandi senza piccole extra, che danno **1,90×** e 104 posti, la quarta grande porta ancora **+0,10×** di velocità effettiva e **32 posti**, ma **0,20× del suo bonus nominale viene perso** a causa del tetto. Il limite va valutato sul totale delle case, non sul singolo edificio isolato.

## 3. Il vero costo: Cibo, Pane e lavoratori

Le condizioni teoriche **a piena occupazione** per le quattro combinazioni sono:

| Piccole extra + grandi | Abitanti ospitabili | Residenti avanzati | Cibo/s richiesto | Pane/s richiesto | Filiere Cibo continue minime | Filiere Pane continue minime |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 7 + 0 | 64 | 0 | 0,640 | 0 | 1 | 0 |
| 5 + 1 | 80 | 32 | 0,800 | 0,480 | 1 | 1 |
| 3 + 2 | 96 | 64 | 0,960 | 0,960 | 1 | 1 |
| 1 + 3 | 112 | 96 | 1,120 | 1,440 | **2** | 1 |

Le cifre derivano da `Cibo = abitanti × 0,010/s` e `Pane = residenti avanzati × 0,015/s`. Una filiera Cibo completa (Pescatore + Deposito alimentare) produce nominalmente **1 Cibo/s**, costa **100 legno + 50 pietra** e impiega **2 lavoratori**. Una filiera Pane completa (2 Fattorie + Pozzo + Mulino + Forno) produce nominalmente **3 Pane/s**, costa **420 legno + 280 pietra** e impiega **5 lavoratori**.

Per rendere esplicito il peso delle filiere, ecco il **costo totale teorico minimo dell'assetto a regime**, sommando case acquistate e filiere alimentari complete necessarie, **senza** detrarre strutture eventualmente già possedute:

| Piccole extra + grandi | Legno totale | Pietra totale | Lavoratori alimentari | Nota |
| --- | ---: | ---: | ---: | --- |
| 7 + 0 | **660** | **330** | 2 | Cibo soltanto |
| 5 + 1 | **1.080** | **610** | 7 | Serve il Pane |
| 3 + 2 | **1.080** | **610** | 7 | Più posti a parità di spesa della riga precedente |
| 1 + 3 | **1.180** | **660** | 9 | Sopra 100 abitanti serve più produzione di Cibo |

**Attenzione ai confini del calcolo:** questi totali escludono Taglialegna, Segheria, Tagliapietre, Magazzini, scorte iniziali, materiali eventualmente già disponibili e tempi in cui le strutture non lavorano. I valori sono capacità *nominali* a flusso continuo; lotti di produzione, serbatoi pieni, indisponibilità di input, ordine di costruzione e personale inizialmente insufficiente possono introdurre pause. In particolare una Casa grande richiede **già un Forno costruito**, e la crescita avanzata necessita di Pane effettivamente disponibile.

La combinazione **3 piccole aggiuntive + 2 grandi** è un riferimento interessante: 96 posti, 2×, una sola filiera Cibo e una Pane nominalmente sufficienti. **Non è ancora dimostrato che sia la strategia più rapida o più conveniente in una partita**: richiede molto più investimento iniziale delle sette Case piccole e lavoratori da liberare per la filiera Pane.

## 4. Saturazione del bonus e progressione successiva

- Oltre il tetto **2×**, acquistare altre case **non accelera** ulteriormente la crescita; aumenta solo la capienza. Questo limite dovrebbe essere leggibile in futuro nell'interfaccia, senza presentare come bonus attivo una percentuale ormai sprecata.
- Quando la capienza è piena, l'accumulatore di crescita si ferma. Aumentare il bonus senza costruire abbastanza posti può quindi non produrre alcun abitante aggiuntivo.
- Il 2× non basta a risolvere una carenza alimentare: Cibo, Pane e magazzino sono vincoli indipendenti.
- Le Case grandi sono efficienti **per posto** (5 legno e 2,5 pietra per posto contro 10 e 5 delle piccole), ma non si devono valutare senza i costi della filiera Pane.
- Le domande future di personale per industria e soldati potrebbero spostare la convenienza delle due politiche: il numero di persone disponibili dipenderà dalla crescita realmente sostenuta, non dalla sola capienza nominale.

## 5. Confronti futuri, senza cambiare le regole

Alla prima occasione di prova Windows:

1. Eseguire il **gate v26** comprensivo di GUI 8-bit e 177 casi xUnit attesi; verificare gli screenshot prima di ulteriori cambiamenti grafici.
2. Usare il [lettore dei benchmark](../validation/M4_BENCHMARK_READER.md) sui quattro JSON reali, tenendo distinti il confronto storico `civil-growth-4` e quello accelerato `civil-growth-5`.
3. Se i risultati suggeriscono una revisione del bilanciamento, progettare **nuovi profili di benchmark** controllati per le quattro combinazioni qui elencate, partendo dalla stessa disponibilità iniziale di materiali e personale. Registrare i tempi per 48/64/80/96/112 abitanti, carenze, secondi con case piene e investimenti effettivi.
4. Osservare separatamente la fase di *avvio* (dove il costo della filiera Pane pesa molto) e quella *a regime* (dove la capienza maggiore può diventare vantaggiosa). Non equiparare una simulazione da una colonia già avviata con il tempo necessario a partire da zero.
5. Solo dopo misure confrontabili proporre un'eventuale nuova versione di regole e relativi test/migrazioni; evitare ritocchi arbitrari a bonus, prezzi o razioni.

## 6. Conclusione e decisioni aperte

**La scelta approvata di dare un vantaggio numerico alle case aggiuntive funziona già sul piano della formula.** Il limite a 2× crea però un secondo tratto di progressione in cui conta soprattutto la capienza e l'alimentazione. Questo è un dato utile per progettare lavoro industriale, caserme e obiettivi di vittoria, senza anticipare una modifica al Core.

**Ancora da decidere con l'utente:** se il tetto 2× sia adeguato anche nelle fasi avanzate; se in futuro la GUI debba indicare esplicitamente il bonus non sfruttato; se Carne diventerà un'alternativa alimentare o vettovagliamento per l'esercito; se demolizione/rimborsi consentiranno la riconversione degli investimenti. Tutte restano ipotesi fino a una decisione esplicita. Nessun numero qui certifica l'obiettivo globale di **8 ore efficienti / 32+ ore inefficienti**.
