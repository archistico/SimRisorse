# v60-E3b3-RC3 — Correzione fixture dell'orologio v14

**Stato:** candidato di correzione, non compilato/testato con .NET in questo ambiente. 3 novembre 2026.

## Risultato Windows ricevuto

`validate.cmd` su v60-E3b3-RC2: 396 test eseguiti, 389 superati, 7 falliti; tutti i fallimenti in `V14CampaignClockTests` attraverso `Prepared()` con eccezione di convalida della popolazione al ripristino, prima dell'inizio del clock. Infrastructure e Desktop superati. La v60-E3b3-RC1 rimane l'ultima candidata integralmente confermata dall'utente come verde sul suo Windows.

## Analisi del problema

La fixture sintetica parte dalla colonia v13, che ha **8 abitanti** e **3 lavoratori produttivi iniziali** (Taglialegna, Segheria, Tagliapietra). Poi assegna:

| Prenotazione | Abitanti |
| --- | ---: |
| Tre lavoratori della colonia iniziale | 3 |
| Pescatore + Deposito alimentare, 1 addetto ciascuno | 2 |
| Recluta in addestramento | 1 |
| Scuola: 1 in attesa + 1 in addestramento + 2 ricercatori qualificati | 4 |
| **Totale** | **10** |

I due ricercatori attivi sono un **sottoinsieme dei due qualificati** e non vanno sommati altre due volte. Due Case piccole offrono 16 posti, sufficienti a ospitare 10 abitanti.

`V14CampaignSnapshots.Validate(...)` ha rifiutato correttamente `10 > 8`; sarebbe un errore indebolire il controllo sui lavoratori/abitanti.

## Modifiche

Unico file tecnico modificato: `tests/SimRisorse.Application.Tests/V14CampaignClockTests.cs`.

1. In `Prepared()` la popolazione della fixture è **10** e il commento ne documenta la composizione.
2. Aggiunta `Assert.Equal(0, prepared.AvailablePopulation)` alla fixture: le dieci persone sono tutte riservate, senza doppie assegnazioni.
3. Nel test dell'impianto avanzato con 10 lavoratori la popolazione passa da 12 a **13**, perché restano 3 lavoratori produttivi iniziali. Ricercatori e militari sono azzerati soltanto per quello scenario.
4. Nessuna modifica al motore, al validatore, al ruleset v13, all'interfaccia, alla persistenza o ai salvataggi.

I **396 test** restano quelli della RC2: nessuna nuova classe o metodo di test.

## Controlli effettuati

- Ripercorso staticamente il conteggio della popolazione e la capienza abitativa (16).
- Isolata la modifica tecnica alla sola fixture/Application test.
- Verificata l'integrità dello ZIP e la presenza del progetto completo.
- **Non** disponibili in questo ambiente compilatore e runner .NET 10: nessuna dichiarazione di superamento dei test RC3.

## Prossimo gate

Eseguire `./validate.cmd` sulla **RC3 estratta in una cartella nuova**: target **396 test verdi** e successivo completamento dei benchmark. Solo dopo iniziare E3b/3d (comandi di costruzione, upgrade e assegnazione lavoratori). Evitare prove sui database personali: il formato v14 è ancora isolato.
