# M4 — Proposta di crescita civile

Stato: **proposta storica precedente al primo incremento M4**. Il proseguimento dello sviluppo ha adottato questi valori provvisori nel nuovo [scenario Civil Growth](CIVIL_GROWTH.md); consultare quella scheda per il comportamento eseguibile e l'[ADR 0009](../adr/0009-civil-growth-and-save-generations.md) per la compatibilità. Il testo seguente conserva la proposta originaria e non certifica un bilanciamento definitivo.

## Primo ciclo proposto

```text
tronchi → legno lavorato ─┬→ nuove abitazioni → posti per abitanti
pietra ──────────────────┤
                         └→ pescatore → pesce → deposito alimentare → cibo
                                                                     ↓
                                                    abitanti nutriti + posti liberi
                                                                     ↓
                                                        nuovi lavoratori disponibili
```

Il giocatore sceglie fra espandere i materiali, preparare il cibo o anticipare le abitazioni. Un edificio vuoto non aumenta la produzione e una casa da sola non genera persone.

Il primo incremento usa solo pesce come alimento trasformabile. Carne e pane arriveranno come filiere ulteriori con ricette esplicite. La scelta non attribuisce al deposito del gioco di riferimento una ricetta che gli screenshot non dimostrano.

## Valori candidati per il primo incremento

Unità e produzione rimangono quelle del Core: millesimi di risorsa, passo di un secondo, persone intere e ordini immediati applicabili in pausa. I valori servono alle prove, non al bilanciamento finale di otto ore.

| Voce | Valore proposto |
| --- | --- |
| Scenario distinto | `civil-growth`, con propria versione delle regole |
| Dotazione economica | 8 abitanti; strutture, personale, materiali e capacità di Economy Foundation |
| Nuove scorte | 0 pesce e 30 cibo, capacità 1.000 ciascuno |
| Abitazioni iniziali | 1 casa piccola, capacità 8 abitanti |
| Casa piccola aggiuntiva | 80 legno lavorato + 40 pietra; +8 posti abitativi; nessun lavoratore |
| Pescatore | 40 legno lavorato + 20 pietra; 1 posto di lavoro; 1 pesce per lavoratore/s; nessun prerequisito |
| Deposito alimentare | 60 legno lavorato + 30 pietra; richiede almeno 1 pescatore; 1 posto di lavoro; 1 pesce → 1 cibo per lavoratore/s |
| Consumo civile | 0,010 cibo per abitante/s, anche per gli abitanti non assegnati |
| Crescita | +1 abitante ogni 60 secondi nutriti con posto abitativo libero, per insediamento |
| Effetto di una carenza | Crescita sospesa; nessun decesso e nessuna riduzione della produzione esistente in questo primo incremento |

I nuovi edifici produttivi seguono i materiali nella priorità del pianificatore: pescatore 40, deposito alimentare 50. Come nelle altre filiere, il pesce prodotto nel secondo corrente diventa utilizzabile dal secondo successivo. La casa non impone una filiera alimentare per essere costruita; il cibo è necessario per la crescita.

Non introdurre crescita per ogni casa: nella proposta le abitazioni aumentano la capacità, mentre il ritmo degli arrivi è unico. In questo modo costruire molte case non moltiplica implicitamente anche il ritmo della popolazione.

## Ordine del secondo economico

1. Leggere popolazione, capacità e scorte all'inizio del passo.
2. Calcolare il fabbisogno alimentare della popolazione iniziale del passo. Se il cibo è sufficiente, prenotare e consumare il fabbisogno intero; altrimenti non consumare una razione parziale e segnalare carenza.
3. Pianificare le ricette con le prenotazioni e i limiti di inizio passo. Lo spazio liberato dal consumo alimentare diventa disponibile solo al passo successivo, come gli altri consumi.
4. Se il fabbisogno è stato soddisfatto ed esiste posto abitativo, avanzare di un secondo l'accumulo di crescita. A 60 secondi aggiungere una persona e riportare l'accumulo a zero.
5. La nuova persona è disponibile alla fine del passo e consuma dal passo successivo. Gli ordini successivi possono assegnarla.

In pausa, a case piene o durante una carenza, il progresso parziale già maturato rimane fermo. Non si accumulano arrivi oltre il limite e non si recuperano secondi di assenza. La previsione della UI deve usare lo stesso piano del motore, includendo il consumo della popolazione.

## Possibilità di partire e recuperare

Con i 120 legno lavorato e 80 pietra iniziali si possono costruire subito un pescatore e un deposito: costo complessivo 100 e 50. Dei cinque abitanti liberi, due bastano per avviarli senza fermare la produzione di materiali.

Otto abitanti consumano 0,080 cibo/s. Le 30 unità iniziali coprono 375 secondi senza rifornimenti. Un lavoratore per ciascuno dei due edifici alimentari porta a regime 1 cibo/s, sufficiente per i primi 16 abitanti, lasciando la scelta su quando costruire la seconda casa.

Spendere prima tutto in case può ritardare il cibo, ma la carenza proposta non elimina i lavoratori necessari a recuperare. Si possono riassegnare persone e ricostituire le scorte con la produzione di base. Evitare costi iniziali in cibo per pescatore e deposito: creerebbero un possibile blocco circolare.

Questi conti sono controlli aritmetici della proposta; non sono ancora simulazioni automatiche dello scenario o prova di una strategia ottimale.

## Confini e compatibilità da implementare

- Core: effetto abitativo nelle definizioni, popolazione modificabile solo tramite regole, stato esplicito della crescita e pianificazione condivisa di cibo/produzione. Nessun orologio o SQL.
- Application: mantenere costruzione e assegnazioni come funzionalità verticali; esporre capacità abitativa, fabbisogno, stato della crescita, progresso e motivo del blocco nelle proiezioni.
- Infrastructure: salvare popolazione effettiva e secondi parziali della crescita. Definire prima il nuovo contratto persistente; il solo numero degli abitanti non basta per una ripresa esatta.
- Desktop: mostrare abitanti/posti, lavoratori liberi, cibo previsto e motivo di crescita ferma. Case e produttori restano nell'elenco.
- Compatibilità: conservare scenario e fixture M3. Non caricare un checkpoint `economy-foundation-1` applicandogli le nuove formule. Prima di cambiare lo scenario predefinito, scegliere un percorso esplicito di ripresa del vecchio scenario oppure un nuovo slot separato; nessuna conversione silenziosa.

La scelta del nuovo formato e del percorso di compatibilità va registrata in un ADR all'implementazione. Questo documento non cambia il formato v1 già usato dall'applicazione.

## Prove richieste per il primo incremento

- A 59 secondi validi nessun nuovo abitante; al sessantesimo esattamente uno, senza superare i posti disponibili.
- Ripresa con accumulo a 59 secondi, poi un secondo: un solo arrivo; pausa e chiusura non lo anticipano.
- Fabbisogno calcolato sugli abitanti prima dell'arrivo; quantità frazionarie esatte e nessuna scorta negativa.
- Cibo insufficiente: crescita ferma e motivazione leggibile; rifornimento e ripresa senza perdere il progresso parziale.
- Case piene: nessuna crescita anticipata; costruzione aggiuntiva sblocca il conteggio dal passo successivo.
- Produzione alimentare, consumo civile e capienza usano le medesime regole nel forecast e nell'avanzamento.
- Intera filiera avviabile dalla dotazione iniziale, compresa una cronologia con investimento iniziale inefficiente e successivo recupero.
- Salvataggio e ricaricamento conservano stato e tempo frazionario; un file del vecchio scenario non viene reinterpretato o sovrascritto automaticamente.

## Estensioni successive di M4

Una volta verificato il primo ciclo: capacità di magazzino espandibile, campi/mulino/pozzo/forno e ulteriori alternative alimentari. Ogni aggiunta richiede costi, ricette, prerequisiti e una cronologia di raggiungibilità.

Demolizione e rimborsi restano Q-08; soldati e bacino di reclutamento restano M5. La presente proposta non introduce penalità irreversibili, vittoria o classifica.
