# M4.8 — Stress della filiera Pane oltre la soglia della strategia snella

Data: 2026-10-27. **Stato: gate M4.8 riferito superato dall'utente insieme ai test di v21** (codice implementato in v18). Il relativo JSON grezzo non è qui disponibile, quindi non sono pubblicati valori effettivi di crescita o carenze. La GUI provvisoria cambia solo in v22, dopo questo gate; lo stress non modifica `civil-growth-4`, prezzi, ricette, snapshot o salvataggi.

## Domanda da verificare

La quarta strategia `lean-chain` aggiunge una sola Fattoria alla filiera iniziale, più un secondo Pozzo, Mulino e Forno. La capacità media teorica massima è **4,5 Pane/s**, limitata dal Grano. Nel confronto a 220 abitanti, dopo 30 minuti si può arrivare soltanto a 250 residenti (242 avanzati, 3,630 Pane/s): quel benchmark misura risparmio di investimento nel medio periodo, **non** la soglia in cui la soluzione snella diventa insufficiente.

Per isolare quella soglia, un secondo profilo parte oltre **308 abitanti** (300 avanzati × 0,015 = 4,5 Pane/s). Con 320 abitanti, di cui 312 avanzati, il fabbisogno iniziale è **4,680 Pane/s**. Se si raggiungono i 350 abitanti, diventa **5,130 Pane/s**. La seconda filiera completa ha limite nominale medio di **6 Pane/s**, ma deve sostenere davvero l'alternanza dei lotti di Farina e la capacità della scorta.

### Limite quantitativo dimostrabile anche senza eseguire .NET

Per i 1.800 secondi del benchmark, **ipotizzando assenza di carenze e nessuna diminuzione di popolazione**, la richiesta di Pane non può essere inferiore a **312 × 0,015 × 1.800 = 8.424 unità**. La crescita può soltanto aumentarla. Le scorte iniziali sono 100 Pane e **zero** Grano/Farina, quindi una stima *ottimistica* ignora avviamento, colli di bottiglia dei lotti e deposito pieno:

| Strategia | Fattorie totali | Grano massimo in 1.800 s | Pane massimo da quel Grano | Con 100 Pane iniziali | Confronto con domanda minima 8.424 |
| --- | ---: | ---: | ---: | ---: | --- |
| Una filiera | 2 | 3.600 | 5.400 | 5.500 | Insufficiente per almeno 2.924 unità |
| Snella | 3 | 5.400 | 8.100 | 8.200 | **Insufficiente per almeno 224 unità** |
| Due complete | 4 | 7.200 | 10.800 | 10.900 | Potenzialmente sufficiente, non garantito |

Il calcolo usa **2 Grano → 1 Farina → 3 Pane** e il massimo di 1 Grano/s per Fattoria; non presuppone che tutta la Farina venga davvero prodotta o trasformata. **Conclusione rigorosa:** in questo profilo `one-chain` e `lean-chain` non possono sostenere la razione completa *senza interruzioni* per tutti i 1.800 secondi. È dunque fondato attendersi `MissingBreadSeconds > 0`, e nessun risultato del benchmark può trasformare il limite nominale della snella in una fornitura continua sufficiente a 320 abitanti.

**Cosa resta da leggere dai report già validati dall'utente:** secondi di carenza, abitanti finali, tempi per 340/350, scorte e produttività effettiva; la disponibilità teorica di 10.900 Pane non dimostra l'assenza di blocchi per la filiera completa. In particolare, il confronto relativo fra popolazione finale snella e completa è una **previsione**: occorre leggere il JSON effettivo per quantificarla.

## Configurazione controllata

| Elemento | Quantità o politica |
| --- | --- |
| Popolazione iniziale | **320** (312 avanzati) |
| Posti | 1 Casa piccola + **11 Case grandi** = **360 posti** |
| Alimentazione comune | **4 Pescatori + 4 Depositi alimentari**, tutti assegnati; capacità nominale Cibo **4/s** |
| Filiera Pane iniziale | 2 Fattorie + Pozzo + Mulino + Forno, **5 lavoratori** |
| Risorse iniziali | 1.000 legno lavorato, 1.000 pietra, 1.000 Cibo e **100 Pane**; altri input Pane a zero |
| Simulazione | **1.800 secondi**, crescita al massimo fino a **350** abitanti |
| Strategia di acquisto | Stesse quattro politiche e stessi costi del [benchmark 220 abitanti](BREAD_SCALE_BENCHMARK.md) |

Le risorse e gli edifici del punto di partenza sono **sintetici e identici** fra politiche. I potenziamenti sono acquistati tramite i normali comandi del Core; il benchmark **non** misura il tempo necessario a creare la colonia da zero e non inserisce strutture “gratuitamente” durante la partita.

## Politiche e ipotesi

| Strategia | Addetti e materiali aggiuntivi | Limite teorico Pane/s | Ipotesi da verificare |
| --- | --- | ---: | --- |
| `one-chain` | Nessuno | 3 | Numerosi blocchi Pane |
| `lean-chain` | +4; 360 legno lavorato / 250 pietra | 4,5 | Crescita migliore della singola, ma ancora con carenze |
| `two-chains` | +5; 420 / 280 | 6 | Può raggiungere 350 abitanti nei 1.800 secondi |
| `second-chain-late` | +5; 420 / 280 al secondo 900 | 3 → 6 | Recupero parziale dopo l'investimento |

**Attenzione:** i valori 3/4,5/6 sono limiti nominali medi, non produzioni continue garantite. Il Forno opera per lotti di 3 Pane; il Mulino consuma 2 Grano per lotto. La scorta piena, il ritmo effettivo della produzione e la priorità delle ricette possono produrre risultati diversi. I risultati sopra sono ipotesi e vengono confrontati con il JSON reale **solo dopo il gate Windows**.

## Output e validazione

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-bread-high-density
```

`validate.cmd` esegue questo profilo dopo quello a 220 abitanti e salva **`bread-high-density-benchmark.json`**, con percorso anche in `validation.json`. Il report espone gli stessi campi del confronto M4.7, più i tempi per raggiungere **340 e 350 abitanti**. Le soglie precedenti di 240 e 250 sono già soddisfatte nel punto di partenza, quindi vengono registrate a **0 secondi** in questo profilo e non sono metriche di progressione.

Il validatore controlla:

1. stessa versione delle regole, popolazione iniziale, alloggi da 360 posti e quattro politiche distinte;
2. assenza di blocchi Cibo o case piene nel periodo osservato;
3. acquisti reali con costi e lavoratori corretti per ciascuna politica;
4. **1.800 secondi riconciliati** per ciascuna politica, Pane effettivamente prodotto/consumato e bilancio cumulato delle scorte;
5. ordine di crescita atteso **singola < snella < completa**, carenze di Pane nella snella e arrivo a **350 abitanti** con la completa.

L'ultimo punto è un'**aspettativa testabile**, non un esito già osservato. Se fallisce, bisogna analizzare `bread-high-density-benchmark.json` prima di decidere se correggere una condizione del test, un errore del motore o il giudizio di bilanciamento. Il benchmark è un esperimento isolato: non prova che la filiera completa sia sempre l'investimento migliore e non autorizza modifiche economiche automatiche.

## Posizione nella roadmap

È un passo tecnico di **M4.8**, riferito superato dall'utente insieme a v21. L'uscita di M4 richiede di leggere i due report di scala quando disponibili, confermare l'assenza di regressioni nella GUI v22 e classificare esplicitamente Carne e demolizione come attività rinviate o approvate. La GUI definitiva rimane **pixel art retro 8-bit essenziale** e sarà prototipata dopo la stabilizzazione delle regole civili.
