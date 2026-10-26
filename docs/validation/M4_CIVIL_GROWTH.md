# M4 — Primo incremento di crescita civile

Data: 2026-10-26, aggiornato il 2026-10-27. Esito: **case, crescita, filiere del pesce e del pane e magazzino espandibile implementati e verificati su Windows**. M4 rimane in corso per le ulteriori fasi civili.

Baseline invariata: .NET 10, Avalonia 12.1.4, Microsoft.Data.Sqlite 10.0.14; xUnit 3 per Desktop e 2 per gli altri test. Core e Application mantengono dipendenze solo dal framework e dai riferimenti interni. Nessun nuovo pacchetto.

## Risultati

Build Release senza errori o avvisi, restore con lock file, **117 test superati**:

| Progetto | Test |
| --- | --- |
| Core | 49 |
| Application | 24 |
| Infrastructure | 21 |
| Desktop | 23 |

I nuovi controlli coprono:

- Arrivo soltanto al sessantesimo secondo valido, consumo dal passo successivo e persone intere.
- Progresso a 59,5 secondi salvato, pausa senza avanzamento e arrivo dopo altri 0,5 secondi.
- Case piene senza accumulo di arrivi; più case non moltiplicano il ritmo.
- Cibo prenotato prima delle ricette; nessuna razione parziale e nessun riuso immediato dello spazio liberato.
- Carenza che conserva persone e progresso; cibo nuovo utilizzabile dal passo successivo.
- Snapshot invalidi, capacità abitativa fuori limite e acquisti falliti senza mutazioni parziali.
- Proiezioni confrontate con le variazioni effettive di tutte le risorse per 181 secondi, inclusi due arrivi.
- Ripresa e rollback su SQLite reale v2: popolazione, crescita, tempo frazionario, scorte ed edifici rimangono coerenti.
- Stato di crescita mancante, negativo o oltre i limiti tecnici rifiutato; errore applicativo senza sostituire la partita corrente.
- File v1 e v2 che non si sovrascrivono, in entrambe le direzioni; fixture M3 ancora caricabile senza modificarla.
- UI con case, posti, cibo, crescita e carenze; nuova partita nello scenario corrente e percorsi personali separati.

## Simulazioni e desktop

| Cronologia | Durata | Esito |
| --- | --- | --- |
| Foundation, nessun ordine | 60 s, 8 h, 32 h | Timeline precedente conservata e risorse entro capacità |
| Civil, bootstrap | 121 s | 9 abitanti, progresso 0, 140,32 cibo |
| Civil, recupero | 662 s | 14 abitanti, progresso 0, produzione alimentare recuperata |
| Civil, bootstrap | 8 h e 32 h | 16 abitanti, nessuna crescita oltre i posti e scorte nei limiti |

Otto render: viste Edifici/Risorse per i due scenari, a 1240×850 e 1040×740. Ispezionate le nuove viste; controllati i comandi primari e il contenimento della barra di crescita nella colonna laterale. Le schede mostrano tre risorse in evidenza; la tabella Risorse conserva l'elenco completo. I saldi negativi hanno segno esplicito e colore distinto.

Pubblicazione Windows e ciclo nativo completati per entrambi gli scenari. Per Civil: costruzione della casa, progresso 59 s, salvataggio, nuova partita, ricaricamento e singolo arrivo dopo 1 s. Per entrambi: notifica Win32 di sospensione alla sola finestra e chiusura ordinata con salvataggio. File privati di verifica, nessun utilizzo dei salvataggi personali.

## Secondo incremento: magazzino e `civil-growth-2` (2026-10-27)

Decisione nell'[ADR 0010](../adr/0010-storage-capacity-and-rules-conversion.md). Build Release senza errori o avvisi, restore offline con lock file, **128 test superati** (+11):

| Progetto | Test |
| --- | --- |
| Core | 54 |
| Application | 27 |
| Infrastructure | 22 |
| Desktop | 25 |

Nuovi controlli:

- Produzione ferma a capacità piena, ripresa dal passo successivo alla costruzione del magazzino e nuovo arresto al nuovo limite.
- Ripristino verificato rispetto alla capacità degli edifici salvati; overflow della capacità rifiutato come dato invalido.
- Acquisto oltre il limite numerico fallito con `NumericLimit` senza mutazioni; scorte iniziali entro la capacità degli edifici iniziali; effetti su risorse sconosciute rifiutati dal catalogo.
- Scenario civile: capacità 1.500 su tutte le cinque risorse dopo un magazzino, saldi previsti uguali alla variazione effettiva nel secondo successivo.
- Conversione di un checkpoint `civil-growth-1` con progresso a 59,5 s: tempo, scorte e crescita invariati, magazzino a zero, ripresa in pausa. Regole sconosciute, scenario diverso e checkpoint v1 incoerente rifiutati senza sostituire la partita.
- SQLite reale: copia `<nome>.<regole>.backup.sqlite3` creata solo quando cambiano scenario o regole, mai sovrascritta (suffisso `-2`), nome ripulito da caratteri non sicuri, contenuto ricaricabile; nessuna copia per generazioni di file diverse.
- Desktop: messaggio di conversione all'avvio, copia creata al primo salvataggio, descrizione/effetto/stato del magazzino e capacità 1.500 nella vista risorse.

Simulazioni: le sette precedenti con gli stessi esiti e la nuova cronologia `storage` (1260 s): 16 abitanti, capacità 1.500 per ogni risorsa e pietra a 1.000 dopo la ripresa. Otto render rigenerati; nella vista compatta l'elenco di sette strutture scorre e i comandi primari restano visibili. Pubblicazione e ciclo nativo Windows superati per entrambi gli scenari.

## Terzo incremento: filiera del pane e `civil-growth-3` (2026-10-27)

Build Release senza errori o avvisi, restore offline con lock file, **132 test superati** (+4):

| Progetto | Test |
| --- | --- |
| Core | 54 |
| Application | 30 |
| Infrastructure | 22 |
| Desktop | 26 |

Nessun cambiamento al Core: fattoria, pozzo, mulino e forno sono definizioni di catalogo. Nuovi controlli:

- Prerequisiti mulino→fattoria e forno→mulino+pozzo, costi insufficienti senza mutazioni.
- Cronologia completa dal tempo zero: a 600 s risorse e persone libere esatte; mulino attivo dal secondo passo, forno dal terzo; saldi previsti uguali alla variazione effettiva per dieci secondi; a regime +4 cibo/s e grano, farina, acqua costanti.
- Conversione concatenata da `civil-growth-1` e da `civil-growth-2`, con tutte le nuove strutture a zero; rifiuto dei checkpoint che dichiarano regole precedenti ma contengono già le nuove strutture.
- Copia di sicurezza e messaggio desktop aggiornati alla nuova versione; ricette e prerequisiti mancanti del forno visibili nella UI.
- Vista Risorse: con otto risorse la tabella scorre dentro la finestra nella dimensione compatta; intestazione e nota restano visibili (verificato dal test di render e ispezionando il PNG).

Simulazioni: le otto precedenti con gli stessi esiti e la nuova cronologia `bread` (660 s): 16 abitanti, 4 cibo/s, forno attivo. Pubblicazione e ciclo nativo Windows superati per entrambi gli scenari.

## Analisi di raggiungibilità del catalogo (2026-10-27)

Build Release senza errori o avvisi, **140 test superati** (+8): Core 59, Application 33, Infrastructure 22, Desktop 26. Nessun cambiamento a regole, contenuti o salvataggi.

- Core: catalogo di riferimento completo; costo non ottenibile che blocca la struttura e, a cascata, i dipendenti; dotazione iniziale che paga un costo ma non rende rinnovabile un input; costi e ricette oltre la capacità, risolti da un magazzino raggiungibile; struttura già posseduta raggiungibile anche con costo non ottenibile.
- Application: Foundation e Civil Growth hanno tutte le strutture raggiungibili e tutte le risorse rinnovabili; le conversioni dichiarate formano una catena continua fino alle regole correnti e aggiungono solo strutture del catalogo.
- Validazione: `--check-catalog` per entrambi gli scenari, con report `catalog-<scenario>.json`; le nove simulazioni, gli otto render e il ciclo nativo Windows restano superati.

## Riproduzione

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
.\run-game.cmd
.\run-game.cmd --scenario economy-foundation
```

Il validatore scrive in `.local-cache/m4/`: `validation.json`, otto PNG, nove JSON di simulazione, due report del catalogo, pacchetto e risultati/log/database isolati per esecuzione sotto `runs/<id>/`. La cache NuGet rimane quella del workspace. I report precedenti restano nelle cartelle M1–M3.

## Limiti

Le filiere dimostrano raggiungibilità e recupero per le quattro cronologie, non ottimalità matematica o bilanciamento sulle otto ore; con i valori attuali il pane non è necessario per nutrire la popolazione raggiungibile. Non sono implementati carne, demolizione, esercito, isole o classifica.

Nessuna migrazione di formato; due conversioni di regole dichiarate e concatenabili; la copia di sicurezza non è gestibile dalla UI; scelta dello scenario tramite argomento di avvio, non ancora tramite schermata dedicata. Linux/macOS, sospensione fisica del computer, spegnimento improvviso e distribuzione self-contained restano da verificare. Audit CLI non completato per TLS nell'ambiente; nessuna dipendenza è cambiata rispetto al [controllo puntuale della migrazione Avalonia](AVALONIA_12.md).
