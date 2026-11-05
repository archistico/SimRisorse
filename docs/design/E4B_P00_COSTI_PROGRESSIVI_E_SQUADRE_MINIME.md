# E4b-P00 — Edifici costosi, crescita quasi esponenziale e squadre minime

**Stato:** principio di design approvato dall'utente il 04/11/2026; **specifica preliminare**, non implementata. I nuovi prezzi, le curve numeriche e i parametri per singola famiglia **non sono approvati**. Documento di riferimento per gli incrementi successivi a F1-D05d, non autorizza modifiche immediate al bilanciamento.

## 1. Perché cambiare

SimRisorse attualmente rende troppo facile raggiungere strutture industriali di alto livello. Il problema non è solo il requisito di una filiera precedente: costruire la prima Zecca costa `210 planks + 160 stone`, mentre il primo Tagliapietre costa `30 planks + 10 stone` (sorgente: `src/SimRisorse.Application/Scenarios/CivilGrowthScenario.cs` e `EconomyFoundationScenario.cs`). Sono appena **7× le tavole e 16× la pietra**, nell'ordine di grandezza sbagliato per un edificio di fine progressione.

La prima Zecca oggi ha un solo posto di lavoro nel livello Base. La progressione v14 applica un aumento per acquisto all'interno della famiglia (`growthFactor = 1,15`), ma questo rende costose **le copie successive**, senza rendere necessariamente costosa **la prima struttura avanzata**. Moltiplicatori correnti per tier: Base `1`, Grande `2,6`, Avanzato `6,5`; posti: `1 / 3 / 10`. Non modificare queste costanti prima di una proposta misurata.

## 2. Obiettivi approvati

1. **Difficoltà crescente sostanziale**: passare da strumenti di base a industria avanzata deve richiedere crescita della popolazione, filiere sostenibili, capacità di stoccaggio e investimenti rilevanti.
2. **Quasi esponenziale nella progressione**: la distanza economica fra epoche/famiglie avanzate deve essere di ordini di grandezza, ma non rendere la partita irraggiungibile o dipendente da un ciclo impossibile.
3. **Zecca come caso guida**: costo percepito equivalente all'investimento necessario per circa **100–1.000 Tagliapietre** di riferimento. È un intervallo-obiettivo, **non un prezzo finale già deciso**.
4. **Soglie minime di produzione per struttura attiva**: una Zecca richiede almeno **10 lavoratori** prima di generare la prima Moneta; due Zecche richiedono 20 lavoratori per produrre contemporaneamente al ritmo completo di due unità equivalenti.
5. **Coerenza per famiglia e tier**: le regole di squadra e capienza vanno esplicitate, testate e mostrate chiaramente nella GUI; nessun lavoratore può sostenere contemporaneamente due unità produttive.
6. **Preservazione dei salvataggi**: nuove regole versionate, con checkpoint e percorso di migrazione/rollback; evitare di invalidare personale già assegnato o costi già sostenuti.

## 3. Interpretazione dell'equivalenza con 100–1.000 Tagliapietre

Per un benchmark leggibile, **senza applicare l'attuale +15% ai 100–1.000 acquisti ipotetici**, si usa come punto di confronto il costo-base unitario del Tagliapietre:

| Equivalenza illustrativa | Legno lavorato | Pietra |
| --- | ---: | ---: |
| 1 Tagliapietre | 30 | 10 |
| 100 Tagliapietre | 3.000 | 1.000 |
| 1.000 Tagliapietre | 30.000 | 10.000 |
| Zecca attuale | 210 | 160 |

**Non** significa che la Zecca debba costare soltanto questi due materiali, né che il prezzo debba essere fissato subito a 30.000/10.000. Materie lavorate, metalli, tecnologie, infrastrutture e personale impegnato devono partecipare al costo totale. Il benchmark misura il peso dell'investimento in risorse equivalenti e tempo produttivo; per il valore finale serve una simulazione completa.

La somma realmente pagata per costruire 100 Tagliapietre con fattore 1,15 è un'altra grandezza e aumenta rapidamente: **non** va confusa con il benchmark di equivalenza; non usarla come moltiplicatore diretto e incontrollato.

## 4. Architettura dei prezzi — proposta da simulare

Separare quattro cause di costo:

- **Costo d'ingresso della famiglia** (`B_f,r`, vettore per materiale `r`), differenziato fra costruzione iniziale, filiera intermedia e industria finale.
- **Indice di complessità/progressione** (`A_f`), calibrato sulla posizione nell'albero produttivo e tecnologico; forte crescita del costo della *prima* costruzione avanzata.
- **Moltiplicatore del livello** (`T_f,t`) per Base, Grande, Avanzato, con upgrade calcolati coerentemente.
- **Crescita delle copie nella stessa famiglia** (`G_f^n`) e dello storico acquisizioni; nessun reset gratuito attraverso upgrade o demolizioni.

Formula **da validare**, non contrattuale:

`Costo(f, tier, n, risorsa) = ceil(B[f,risorsa] × A[f] × T[f,tier] × G[f]^n)`.

A fianco possono esistere **prerequisiti di investimento** (tecnologie, numero di lavoratori/popolazione, materiali di altri rami, infrastrutture), separati dai materiali consumati. La prima Zecca non va ottenuta moltiplicando solo il costo di Legno/Pietra, trascurando la filiera dell'Oro.

### Cose da evitare

- Curve troppo ripide che causino overflow `decimal`, valori irraggiungibili, costi con int impossibili da visualizzare o magazzini insufficienti per *qualunque* pagamento.
- Prezzi che diventano più convenienti comprando e potenziando in un certo ordine, o azzerando contatori tramite demolizione/migrazione.
- Aumentare il costo in modo tale che non si possa costruire l'edificio che produrrebbe le risorse necessarie a pagarlo (deadlock duro).
- Obbligare ad accumulare una scorta maggiore della massima capacità realizzabile con le strutture esistenti (servono magazzini scalabili e coerenti).
- Azzerare le risorse già spese in vecchi salvataggi: decidere una politica per edifici posseduti, non imporre differenze retroattive senza migrazione.

## 5. Squadre minime e produzione — proposta

### Regola per la Zecca

- **Dimensione minima della squadra**: `10` persone per unità produttiva operativa.
- `0–9` lavoratori assegnati alla Zecca: produzione Monete = **zero**, nessun consumo di Oro/Carbone da quella unità; mostrare quanti mancano.
- `10–19` lavoratori su **due Zecche Base**: **una sola** unità operativa (se le altre condizioni sono soddisfatte), non il doppio della produzione.
- `20` lavoratori su due Zecche Base: **due** unità operative e potenzialmente doppia produzione, se input e deposito/output sono sufficienti.
- Squadre parziali possono rimanere assegnate in attesa: la UI deve rendere evidente che non producono, così non sembrano risorse scomparse. La riallocazione deve essere deterministica e non contare due volte i lavoratori.

**Importante:** la produzione attuale v14 scala quasi sempre con i singoli lavoratori. Questo comportamento va cambiato *per le sole famiglie configurate*, non indiscriminatamente per Pescatori, Tagliapietre, Scuola ecc.

### Coerenza dei livelli

I posti `1 / 3 / 10` attuali sono **incompatibili** con un minimo di 10 alla Zecca Base. Proposta tecnica: per ogni famiglia dichiarare `CrewSize` e `TeamsPerBuildingTier`, distinguendo **moduli produttivi** dai posti fisici. Per una Zecca, da valutare:

| Livello | Moduli massimi per edificio | Personale minimo per primo modulo | Personale per piena operatività |
| --- | ---: | ---: | ---: |
| Base | 1 | 10 | 10 |
| Grande | 3 | 10 | 30 |
| Avanzato | 10 | 10 | 100 |

Per altre famiglie le squadre possono restare da un lavoratore. I moltiplicatori `1/3/10` diventerebbero **moduli di capacità**, non il limite letterale di persone per una Zecca. Questo schema è **candidato**, non ancora deciso per tutte le famiglie; approvare il significato di produzione e addetti prima dell'implementazione.

`ModuliAttivi = min(ModuliPosseduti, floor(LavoratoriAssegnati / CrewSize))` **solo dopo** aver controllato popolazione disponibile e limiti tier. La produzione finale sarà ulteriormente limitata da Oro, Carbone, capacità di stoccaggio e priorità del planner. Un modulo incompleto non consuma input.

### UI proposta

Nel box dettaglio, senza ComboBox e con gli stessi controlli pixel-art: `Zecche 2`, `Addetti 19/20 richiesti per due unità`, `Operative 1/2`, `Mancanti 1`, `Collo di bottiglia: personale`, `Monete/s nominali/effettive`. La riga Costruisci rimane separata dalla riga Lavoratore, con `+1/+5/+10` coerenti. I numeri di team e i costi dei lotti sono calcolati sui valori effettivi.

## 6. Effetti da verificare sull'intera economia

- **Popolazione**: arrivare a 10/20/30/100 addetti industriali richiede Case, crescita, alimentazione e personale libero. Non sottrarre implicitamente personale a Scuola, Esercito, attività esistenti.
- **Alimentazione F1**: quando sarà attiva, un'industria pesante deve competere con agricoltura, panificazione, allevamento, Cibo e trasporto nel deposito, senza rendere ingiocabile l'avvio.
- **Energia/materie**: la Zecca consuma Oro e Carbone, quindi devono essere misurate anche le capacità delle Miniere e della Fonderia, non solo l'organico della Zecca.
- **Ricerca**: i livelli Grande/Avanzato devono richiedere scelte tecnologiche realmente giustificate dai benefici.
- **Magazzini**: la capacità disponibile deve precedere e sostenere le richieste di accumulo; costi altissimi senza magazzini adeguati causano stalli strutturali.
- **Rapporto costi/rendimento**: una filiera di fine gioco deve essere lenta da raggiungere ma utile, non un costo enorme per produzione simbolica.

## 7. Gate e criteri per approvare i numeri

1. **Baseline stabile:** validare la filiera alimentare F1 e la migrazione v15 prima di proporre prezzi definitivi. Il vecchio benchmark `natural-reference-v1` resta storico e va rieseguito sotto la nuova versione.
2. **Rapporto di fattibilità:** per ogni scenario/strategia confrontare tempi e risorse per primo Tagliapietre, prima Scuola, ricerca, filiere Alimentazione, Forgia, prima Zecca, Zecca duplicata, struttura navale futura.
3. **Confronti di scenario:** prezzo Zecca equivalente a 100, 300 e 1.000 Tagliapietre base; mantenere costi materiali distinti e verificare accessibilità, spazio magazzini, popolazione e colli di bottiglia.
4. **Test squadre:** `0, 9, 10, 19, 20` addetti, 1/2 Zecche, livelli Base/Grande/Avanzato, input esauriti, storage pieno, salvaripresa.
5. **Determinismo:** stessa strategia e seed/snapshot devono generare stessi eventi e stessi KPI; nessuna perdita di risorse per operazioni rifiutate.
6. **Migrazione:** test con salvataggi v14 e v15 e casi di lavoratori eccedenti/precedentemente assegnati; protezione checkpoint, niente cancellazioni.
7. **Approvazione finale dei valori:** solo dopo il rapporto E4a e una proposta E4b comparativa. Senza approvazione restano in produzione i coefficienti esistenti.

Vedere [roadmap](../ROADMAP.md), [matrice di verifica](../validation/E4B_P00_COSTI_SQUADRE_TEST_MATRIX.md) e [ADR 0056](../adr/0056-progressione-tardiva-e-squadre-minime-proposta.md).
