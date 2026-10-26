# Handoff

Ultimo aggiornamento: 2026-10-26. Questo documento descrive il punto operativo da cui continuare; visione e regole complete sono nel [game design](GAME_DESIGN.md).

## Stato attuale

- M0–M3 sono completate; il primo incremento M4 è implementato su Avalonia 12.1.4 e verificato con 117 test. Sono giocabili case, crescita degli abitanti, pescatore e pesce→cibo, con salvataggio del progresso parziale. M4 resta in corso per magazzini espandibili e ulteriori filiere civili.
- La bozza Windows Forms e i relativi test sono in [legacy/winforms](../legacy/winforms/README.md), con contenuto originale conservato.
- Le nove immagini di riferimento sono in [docs/references/screenshots](references/screenshots/), indicizzate nel [documento dei riferimenti](references/README.md). Le cinque nuove immagini di risorse e strutture hanno un'[analisi dedicata](references/RESOURCE_AND_PRODUCTION_CHAINS.md).
- La prova è in [experiments/stack-probe](../experiments/stack-probe/README.md): quattro progetti, interfaccia Avalonia, modello ridotto, salvataggio SQLite e 23 test. La sua soluzione non è quella definitiva.
- La [soluzione definitiva](../SimRisorse.slnx) contiene Core, Application, Infrastructure, Desktop, Simulation e quattro progetti di test: nove progetti senza riferimenti a bozza o esperimento.
- Il simulatore esegue lo scenario economico senza finestre; non è ancora uno strumento completo di ricerca delle strategie o bilanciamento.
- Il nuovo default desktop è `civil-growth-1`, sei tipi di struttura, cinque risorse e otto abitanti iniziali che possono crescere. Foundation rimane eseguibile con `--scenario economy-foundation`. Catalogo completo, vittoria, classifica, esercito e isole non sono implementati. Non sono stati creati commit o release in questo incremento.

## Vincoli da mantenere

1. Il giocatore vuole costruire tutti i tipi di edificio; la formulazione precisa della vittoria resta aperta.
2. La gestione avviene tramite elenco, senza collocazione degli edifici su una mappa.
3. Lavoratori, soldati, castelli e missioni sulle isole appartengono alla visione.
4. Il gioco si ferma in pausa e quando non si gioca; non esiste produzione offline.
5. La classifica misura il tempo di gioco; otto ore efficienti e oltre 32 meno efficienti sono obiettivi di bilanciamento.
6. Le immagini sono riferimenti: valori, colori e significato dei pulsanti non sono automaticamente regole approvate.

## Decisioni architetturali

- Accettati: monolite modulare, funzionalità verticali e Core indipendente; tempo esplicito e riproducibilità; catalogo versionato; modelli espliciti per popolazione e progressione militare.
- **Adottato dopo la prova M1:** C# su .NET 10, Avalonia/MVVM e SQLite tramite Microsoft.Data.Sqlite, come registrato nell'[ADR 0005](adr/0005-desktop-stack-candidate.md). Windows è la prima piattaforma verificata; Linux/macOS restano da provare.
- Core e Application usano solo framework e riferimenti interni. La baseline corrente usa Avalonia 12.1.4, incluso headless, Microsoft.Data.Sqlite 10.0.14 e xUnit 3.2.2 per i soli test Desktop. La baseline storica M3 e la prova M1 conservano il proprio significato nei rispettivi report.
- L'[ADR 0006](adr/0006-economic-step-and-resource-precision.md) fissa millesimi di risorsa, passo di un secondo e prenotazioni per priorità. Nuovi edifici e risorse sono definizioni, non nuovi enum o sottoclassi.
- L'[ADR 0007](adr/0007-desktop-persistence-and-lifecycle.md) fissa persistenza, gestione del tempo reale e chiusura. Formato SQLite e limiti sono in [SAVE_FORMAT.md](SAVE_FORMAT.md).
- L'[ADR 0008](adr/0008-avalonia-12.md) sostituisce la sola baseline dei pacchetti dell'ADR 0007, su richiesta dell'utente. Migrazione e aggiornamento SQLite sono completati; nessun cambiamento a snapshot v1 o regole Economy Foundation.
- L'[ADR 0009](adr/0009-civil-growth-and-save-generations.md) introduce crescita civile, prenotazione del cibo e snapshot/file v2, mantenendo il vecchio scenario v1 in uno slot distinto. "Nuova partita" conserva lo scenario corrente.
- Il contenuto di tutti gli ADR è nell'[indice](adr/README.md). "Accettato" non significa "implementato".

## Prossimo lavoro: completare M4

Lo [scenario civile eseguibile](scenarios/CIVIL_GROWTH.md) conserva la base economica e aggiunge casa piccola con 8 posti, un arrivo ogni 60 secondi nutriti con spazio libero e consumo di 0,010 cibo per abitante/s. Fame, case piene e pausa fermano il progresso; nessun decesso o malus produttivo. I valori sono adottati per questo incremento e restano provvisori, senza estenderli implicitamente al mantenimento militare.

Le nuove schermate di risorse e i diagrammi precisano filiere civili, alimentari e militari. La UI M3 mostra già flussi previsti `/s`; i valori del gioco di riferimento non hanno invece un'unità temporale dimostrata. I diagrammi non forniscono costi, durate o ricette numeriche: le nuove famiglie richiedono una scheda di scenario esplicita.

La [proposta originaria](scenarios/CIVIL_GROWTH_DRAFT.md) rimane un documento storico. Le cronologie eseguibili sono nel simulatore: `bootstrap` avvia pescatore/deposito al tempo zero e casa al secondo 61, con primo arrivo al 121; `recovery` parte dalla casa, esaurisce il cibo, poi riavvia la filiera al secondo 600 e ha un nuovo arrivo al 662. I risultati sono verificati, non dimostrazioni di ottimalità.

Passi successivi:

- Introdurre capacità di magazzino espandibile nel catalogo, nella sessione e nelle previsioni; definire costi e limiti prima dell'algoritmo.
- Ampliare la progressione con grano, acqua, farina e pane, specificando costi, ricette, prerequisiti e modalità delle trasformazioni alimentari. Il riferimento non dimostra una ricetta obbligatoria con pane, carne e pesce insieme.
- Aggiornare la versione delle regole al prossimo cambio di contenuto e decidere la compatibilità prima di sostituire `civil-growth-1`.
- Decidere demolizione e conseguenze su personale, capacità e scorte (Q-08) prima di abilitarla nella UI.
- Ampliare le cronologie riproducibili e i controlli di avviabilità/recupero, mantenendo i test di tempo, contesa, pause e salvataggi.
- Ripristinare l'audit CLI quando l'accesso NuGet .NET funziona; conservare il controllo puntuale e le evidenze di aggiornamento già prodotti, senza aggirare TLS.

Esercito, navigazione e isole rimangono M5; i nove elementi decorativi sono riferimenti per la progressione avanzata, non contenuti già approvati o richiesti per la prima estensione civile.

## Avvio e verifiche correnti

```powershell
.\validate.cmd -OfflinePackageSource .local-cache/nuget
.\run-game.cmd
```

Build Release senza errori o avvisi; **117 test superati**: 49 Core, 24 Application, 21 Infrastructure, 23 Desktop. Verificate otto immagini, sette simulazioni fino a 32 ore, pubblicazione e avvio nativo Windows per entrambi gli scenari. Il ciclo civile riprende il progresso a 59 secondi e genera esattamente un arrivo dopo 1 secondo. Entrambi verificano sospensione notificata alla sola finestra e chiusura ordinata. Fixture M3 ancora compatibile; SQLite v2 verificato anche con errore di scrittura indotto. Dettagli nel [report M4](validation/M4_CIVIL_GROWTH.md).

La sessione parte/riprende in pausa. Il file personale civile è `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`; Foundation mantiene `game.sqlite3`, accessibile con `run-game.cmd --scenario economy-foundation`. Checkpoint ogni minuto, manuali e alla chiusura; nessuna produzione offline. La chiusura fallita conserva la finestra e offre uscita esplicita senza salvare. File incompatibili all'avvio non vengono sostituiti da autosalvataggi della sessione iniziale di ripiego.

Artefatti correnti in `.local-cache/m4/`: `validation.json`, otto PNG, pubblicazione, sette JSON delle simulazioni e risultati/log/database sotto `runs/<id>/`. Validazione e `run-game.cmd --smoke-test` non usano file personali. Gli artefatti precedenti rimangono nelle cartelle M1–M3.

L'audit CLI è stato ritentato ma non completato per errore TLS. Un confronto puntuale con i dati ufficiali VulnerabilityInfo di NuGet ha invece esaminato le 63 coppie pacchetto/versione dei lock della soluzione definitiva: dopo l'aggiornamento SQLite nessuna corrisponde agli intervalli segnalati. Dati, report prima/dopo e limite del controllo sono in `.local-cache/avalonia12-audit/` e nel report della migrazione. Pacchetti scaricati via HTTPS standard in `.local-cache/avalonia12-feed/`, restore da sorgenti locali, nessuna modifica globale.

Linux/macOS, self-contained, spegnimento improvviso e migrazioni di formato non sono verificati. La CI resta da introdurre.

## API e limiti correnti

- `GameRuntime` possiede una sola sessione e serializza modifiche e letture. Gli handler delle funzionalità sono in `Application/Features/`.
- `GameSession` è un aggregato a singolo proprietario; può essere usato direttamente nei test, ma i chiamanti concorrenti passano dal runtime.
- Acquisti e assegnazioni hanno quantità intere e sono atomici. Gli errori attesi sono codici `GameError`.
- `ResourceAmount` rappresenta 0,001 unità su `long`; conversioni con più decimali vengono rifiutate, non arrotondate.
- Il pianificatore usa input e spazio iniziali, priorità crescente e identificatore ordinale; le previsioni usano la stessa contesa. Le attività successive possono restare senza input.
- Passo economico di un secondo, tempo residuo conservato in tick. Massimo 24 ore per singola chiamata di avanzamento; durata totale non limitata a 24 ore.
- Il Core valida snapshot e riprende in pausa. `GameSession.Population` è la popolazione effettiva; `GameScenario.Population` è la dotazione iniziale. La capacità abitativa dipende dagli edifici; la capacità delle risorse è ancora fissa nello scenario.
- `ForecastEconomy()` comprende attività delle strutture e piano civile. La proiezione include consumo effettivamente prenotabile e fabbisogno alimentare distinto, così una razione mancante non appare consumata.
- Foundation emette snapshot v1 senza crescita; Civil emette v2 con `GrowthProgressSeconds` obbligatorio, validato rispetto all'intervallo. Il caricamento di regole diverse non cambia automaticamente lo scenario.
- Catalogo in memoria: riferimenti e cicli edilizi controllati, nessun fingerprint automatico e nessuna prova generale di raggiungibilità economica.

## Verifiche precedenti M2 e comandi

Build Release: **zero errori e avvisi**. **45 test superati**: 36 Core e 9 Application/architettura. Verificate simulazioni di 60 secondi, 8 e 32 ore e isolamento delle configurazioni legacy/M1. Dettagli nel [report M2](validation/M2_CORE.md).

Controlli finali: 22 documenti con 102 collegamenti locali validi, riferimenti dei cinque progetti e lock file coerenti. Contenuto di 26 file storici confrontato con Git normalizzando i fine riga; SHA-256 dei quattro screenshot invariato. Verificato anche il comando `dotnet run` documentato per lo scenario di 60 secondi.

L'audit online è stato tentato ma non completato per errore TLS dell'ambiente. Nessuna verifica dei certificati è stata disattivata. La validazione funzionale offline è completa; i pacchetti esterni dei test e il runtime non sono dichiarati verificati dall'audit.

```powershell
.\validate.cmd -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --hours 8
```

Artefatti in `.local-cache/m2/`: TRX e JSON delle simulazioni. Core e Application non hanno pacchetti esterni; i test usano versioni centralizzate e lock file. La CI resta da introdurre.

## Decisioni di prodotto ancora aperte

Consultare Q-01–Q-10 nel [game design](GAME_DESIGN.md). In particolare non assumere già approvati: possesso simultaneo di tutti gli edifici, isole obbligatorie per vincere, popolazione civile/militare condivisa, consumo alimentare, demolizione con rimborso, missioni deterministiche o classifica pubblica.

## Pulizia effettuata

| Prima | Adesso |
| --- | --- |
| `Gestionale.sln` | `legacy/winforms/Gestionale.sln` |
| `Gestionale/` | `legacy/winforms/Gestionale/` |
| `TestProject1/` | `legacy/winforms/TestProject1/` |
| `screnshot/` | `docs/references/screenshots/` |
| `.vs/`, `bin/`, `obj/` e preferenze locali | `.local-cache/`, esclusa da Git |
| Cartelle vuote rimaste dall'analisi Graphify | `.local-cache/graphify-out/` |

La pulizia delle cache è reversibile. La cancellazione ricorsiva è stata rifiutata dal controllo automatico dei permessi; i file sono stati spostati all'interno del repository, non eliminati. I vecchi output di build non devono essere usati per verificare il nuovo percorso della soluzione: si rigenerano con un build esplicito quando necessario.

## Verifiche precedenti M1 e comandi

La validazione finale ha prodotto zero errori e avvisi di build, **23 test superati su 23**, due immagini headless e una pubblicazione framework-dependent avviata su Windows con codice 0. Il [report M1](validation/M1_STACK_PROBE.md) registra dettagli e limiti.

Controlli finali del repository: 19 documenti con 81 collegamenti locali validi, riferimenti dei quattro progetti della prova validi, cinque ADR accettati e integrità SHA-256 dei 30 file preservati confermata. Anche il comando di avvio interattivo è stato eseguito nella modalità smoke con esito positivo.

I test coprono ricette, carenze, capacità, conservazione del personale, tempo frazionario, ripetibilità su otto ore, cronometro a 32 ore, round trip SQLite, rollback di una scrittura fallita, dati incoerenti e UI reattiva durante I/O. Il test di otto ore non verifica la durata del gioco completo.

Riproduzione offline dalla radice, con pacchetti già presenti nella cache indicata:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-stack-probe.ps1 -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
```

Dopo la validazione, avvio interattivo con `scripts/run-stack-probe.cmd`. Artefatti in `.local-cache/stack-probe/`; salvataggio personale in `%LOCALAPPDATA%/SimRisorse/StackProbe/save.sqlite3`. Database dei test separati e temporanei.

Non verificati: Linux/macOS, audit online, pacchetto self-contained, gestione completa della sospensione del sistema, compatibilità futura dei salvataggi e bilanciamento del catalogo completo. Lo stack è tecnicamente adottato, non certificato su tutte le piattaforme.

## Verifiche precedenti M0

Verifiche eseguite con esito positivo il 2026-10-26:

- Confronto SHA-256 di 30 file preservati: soluzione, sorgenti, test, risorse dei form, attributi Git, licenza e quattro immagini. Il solo `.gitignore` è stato modificato intenzionalmente.
- Risoluzione dei due progetti nella soluzione legacy e del riferimento dal progetto di test all'applicazione.
- Controllo di 16 documenti Markdown: 58 collegamenti locali validi; nessuno spazio finale o terminatore mancante.
- Al completamento M0 gli ADR erano quattro accettati e stack proposto; al completamento M1 l'ADR 0005 è passato ad accettato con evidenze.
- `git diff --check` senza errori e `git check-ignore` con esito atteso per cache, dati locali e cache Graphify.

I file trasferiti appaiono come eliminazioni dai vecchi percorsi e file nuovi nei nuovi percorsi nello stato Git non in staging; gli hash confermano che il loro contenuto è invariato. Non è stato eseguito staging.

Durante M0 non furono eseguiti build e test .NET. Le verifiche M1 riguardano l'esperimento, non i due test storici della bozza: questi ultimi verificano assegnazioni e nomi, non la simulazione.

## Problemi della bozza da non trasferire

- Il tempo viene letto direttamente in `Costruzione.CicliTotali()`.
- I pulsanti aggiungono edifici senza addebitare costi.
- `Magazzino.CalcoloDepositoCiclo()` ricalcola totali dalla creazione e può accreditare produzione senza input sufficienti.
- La capacità viene calcolata ma non applicata come limite coerente ai flussi.
- I campi pubblici permettono modifiche arbitrarie allo stato.

Questi punti sono osservazioni sul codice, non correzioni già eseguite.

## Aggiornamento alla prossima sessione

Registrare cosa è cambiato, verifiche effettivamente eseguite, questioni ancora aperte e prossimo passo. Conservare nel changelog le modifiche concluse e nella roadmap quelle future.
