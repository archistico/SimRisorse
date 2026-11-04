# SimRisorse — P1-D49 / RC35: integrazione Scuola → Ricerca → Edifici

**Data:** 2026-11-04  
**Stato:** test di integrazione predisposti; **compilazione e xUnit non eseguiti in questo ambiente**.  
**Ultimo checkpoint Windows confermato:** RC30, 511 test; RC31–RC34 da verificare cumulativamente con RC35.

## Obiettivo e perimetro

Verificare la catena funzionale completa della v14, non soltanto i singoli servizi:

1. una colonia v13 convertita in memoria alla v14 dispone di una scorta iniziale di test **entro i limiti del Municipio**;
2. acquisto reale di Pescatore e Deposito alimentare, quindi costruzione della Scuola rispettando i prerequisiti;
3. fabbricazione reale delle Matite (1 Legno lavorato + 1 Carbone → 4), iscrizione con consumo di 10 Cibo + 2 Matite e prenotazione di un abitante;
4. avanzamento del tempo di 120 secondi, qualifica, azzeramento del diritto al rimborso Matite per l'allievo diplomato, assegnazione manuale del ricercatore;
5. accumulo di 0,1 punti/s per 999 secondi, acquisto rifiutato con **99,900 punti** e campagna invariata;
6. salvataggio SQLite v3, ricaricamento **in pausa**, un secondo di progresso dopo ripresa fino a **100,000 punti**;
7. acquisto volontario di Lavorazioni I, consumo dei 100 punti, upgrade effettivo Segheria Base → Grande, mantenimento dei lavoratori e del contatore storico;
8. nuovo salvataggio/caricamento, confronto strutturale di risorse, popolazione, formazione, ricerca, tecnologie, edifici e contatori.

Il test percorre il modello di dominio e il reale `V14SqliteSaveStore`, senza introdurre alcun meccanismo di sblocco automatico. Il capitale iniziale aggiunto nella fixture serve a rendere riproducibile l'integrazione: **non è un bonus della partita normale né una proposta di bilanciamento**. I punti e le tecnologie non vengono inseriti dalla fixture.

## Altre regressioni coperte

- Un salvataggio con formazione parziale (45/120 s) non avanza durante la pausa e riprende dal secondo corretto, senza produrre ricerca prima dell'assegnazione di un ricercatore.
- Iscrizione senza Matite, acquisto senza punti e costruzione Grande senza tecnologia sono rifiutati senza modificare il riferimento al checkpoint; la partita resta poi giocabile.
- Il salvataggio non registra tempo trascorso fuori dal gioco; l'utente deve riprendere la simulazione.

## Implementazione

- **Nuovo:** `tests/SimRisorse.Infrastructure.Tests/V14SchoolResearchEndToEndTests.cs` (3 `[Fact]`).
- **Non modificati:** applicazione, interfacce v13/v14, motore, ricette, rapporti costi, schema SQLite, avvio predefinito v14 e slot legacy.
- **Architettura:** il test usa `CivilGrowthScenario`, `V14CampaignSession` e lo store SQLite di infrastruttura; nessun DB esterno e nessuna dipendenza di rete. I file di prova vengono creati sotto una directory temporanea dedicata e cancellati nel `finally`.

## Limiti / rischi aperti

- Il test integrato esegue fino a 1120 secondi di clock simulato: la durata della suite Windows va misurata, senza indebolire gli invarianti in caso di rallentamento.
- Questa prova non certifica la GUI visiva né sostituisce la validazione del singolo controller desktop; RC31–RC34 restano da verificare.
- La semantica dei vecchi salvataggi pre-Matite è già coperta dai test dedicati delle RC9/RC33; questa milestone verifica principalmente la compatibilità degli stati contemporanei.
- Il bilanciamento E4 resta fuori perimetro. Valori 120 s, 10 Cibo, 0,1 punti/s e ricetta delle Matite sono **attuali ma candidati**; il vincolo di almeno 2 Matite per studente rimane confermato.

## Gate e prosecuzione

**RC35:** 536 test cumulativi **attesi** (533 della RC34 + 3 nuovi). Non dichiarare verde in assenza di log Windows.  
**P1-D50:** consolidamento Scuola/Ricerca, inventario delle decisioni confermate/candidate e piano benchmark E4a. Niente nuove ricette o tecnologie senza scelta esplicita.

[Roadmap](../ROADMAP.md) · [Piano di validazione](../validation/P1_D49_RC35_TEST_PLAN.md) · [ADR 0046](../adr/0046-catena-integrata-scuola-ricerca-tier.md)
