# M5.3a/b — Caserma, reclute e soldati

Aggiornato il **2026-11-03**, checkpoint **v41**. Stato: **implementato, da verificare su Windows**. Contratto e motivazioni nell'[ADR 0017](../adr/0017-barracks-recruitment-and-v9-save-upgrade.md).

## Perché non è ancora "Verificato"

L'ambiente di preparazione non aveva accesso a un SDK .NET né a NuGet: il codice è stato scritto e rivisto senza compilazione. Nessun conteggio di questo documento è un esito eseguito. Il gate è:

```powershell
.\validate.cmd
```

## Regole introdotte — `civil-growth-10`

| Elemento | Valore |
| --- | --- |
| Caserma | 260 Legno lavorato / 200 Pietra, 0 addetti, prerequisito Forgia |
| Costo per recluta | 5 Spade, spese all'arruolamento |
| Addestramento | 1 recluta alla volta per Caserma, 30 secondi validi |
| Alloggio e razioni | Reclute e soldati restano residenti: case, Cibo e Pane come ogni abitante |
| Annullamento | Persona e Spade restituite; prima la coda, poi chi ha meno progresso (tempo perso) |
| Congedo, perdite | Non presenti; rinviati alle missioni |

Bilancio controllato in Core, Application, simulatore e validatore: `abitanti = impiegati + liberi + reclute + soldati`.

## Test attesi

| Suite | Prima | Nuovi | Attesi |
| --- | ---: | ---: | ---: |
| Core | 67 | 10 | 77 |
| Application | 97 | 28 | 125 |
| Infrastructure | 35 | 8 | 43 |
| Desktop | 61 | 2 | 63 |
| **Totale** | **260** | **48** | **308** |

- **Core** (`MilitaryRulesTests`): parametri non validi, scenari incoerenti, esercito vuoto, scenario senza reclutamento.
- **Application** (`BarracksRecruitmentTests`): prezzo e prerequisito; Spade e persone scalate una volta; rifiuti senza mutazioni; lavoratori mai arruolati; 1 soldato ogni 30 s; Caserme in parallelo e nuova Caserma che prende la coda; replay 1 s / 250+750 ms / passo unico; checkpoint a 29,5 s con un solo soldato; annullamento e capienza; modello di lettura e previsione del completamento confrontata con la simulazione (quattro casi); conversione v9→v10 e rifiuti; otto checkpoint militari incoerenti; catalogo v9 fissato.
- **Infrastructure**: round trip SQLite frazionario con coda/posti/soldati; file v9 reale convertito con backup; cinque corruzioni delle tabelle militari; salvataggio civile successivo che rimuove le righe militari.
- **Desktop**: scheda Caserma a 1040×740 e 1240×850 (costruzione, arruolamento ×5, avanzamento, annullamento, visibilità dei pulsanti, icone) con render `barracks-buildings*.png`.

## Simulatore e validatore

- Nuova strategia `military`: percorso `crafting`, poi dalla soglia 240 Spade/360 Monete (2.320 s) acquista la Caserma appena possibile e arruola fino a 20 reclute. I tempi di acquisto e arruolamento **sono misurati**, non fissati.
- `validate.ps1`: cronologia `military-8-hours` (20 soldati, coda vuota, una Caserma senza addetti, bilancio della popolazione), controlli delle regole aggiornati a `civil-growth-10`, due render aggiuntivi richiesti (33 + 2 dettagli).

## Da registrare dopo il gate

- Esito `validate.cmd`, ID della run e conteggi TRX.
- Istante d'acquisto della Caserma e del ventesimo soldato in `military-8-hours.json`.
- Ispezione dei due render Caserma.
- Poi M5.3c: confronto di contingenti piccoli/grandi e reclutamento precoce/tardivo sul lavoro civile e sulle razioni.
