# M5.1b — Oro e carbone condiviso

Data: 2026-10-27. **Stato: Verificato su Windows.** L'utente ha scelto esplicitamente **configurazione A per M5.1b**, mantenendo i parametri del ferro e la continuità S1 già acquisiti. Regole correnti **`civil-growth-7`**, **17 tipi di edifici / 14 risorse**.

## Parametri adottati

| Edificio | ID | Legno / pietra | Addetti | Ricetta per secondo e addetto | Prerequisiti |
| --- | --- | ---: | ---: | --- | --- |
| Miniera d'oro | `gold-mine` | 180 / 120 | 1 | → 1 minerale d'oro | Casa grande |
| Fonderia d'oro | `gold-smelter` | 220 / 170 | 1 | 1 minerale d'oro + 1 carbone → 1 oro | Miniera d'oro e Miniera carbone |

Le due strutture aggiuntive costano **400 legno / 290 pietra** e impiegano due lavoratori. `gold-ore` e `gold` partono da zero; capacità base 1.000, +500 per Magazzino. La fonderia del ferro conserva la priorità rispetto a quella dell'oro quando il carbone non basta, come approvato nella domanda sui parametri.

Un trio ferro e una filiera oro richiedono complessivamente due unità di carbone/s: una sola miniera da 2/s può sostenerli a regime, dopo l'avvio. Più fonderie cambiano la domanda. Il Core economico esistente continua a gestire input, personale, capienza e priorità; non è stato introdotto un altro pianificatore.

La UI mantiene lo stile retro monospace e aggiunge quattro pittogrammi originali, descrizioni e ricette. I comandi mostrano prerequisiti, input mancanti e depositi pieni; le 14 risorse scorrono nella tabella.

## Compatibilità S1

Nuovo passaggio **v6 → v7** concatenato a quelli esistenti. Miniera e Fonderia d'oro vengono aggiunte con quantità/personale zero; le scorte nuove sono zero. Il formato file/snapshot resta **v2**.

Il test SQLite parte da un vero checkpoint v6 con **ferro già prodotto, carbone, assegnazioni, Magazzino e tempo a 5,5 s**. La conversione conserva ogni risorsa precedente, tutti i conteggi/lavoratori, tempo e frazione della crescita. Il Magazzino esistente porta immediatamente le nuove capacità a **1.500**. Il caricamento non modifica il file; la prima scrittura conserva una copia ricaricabile in `game.civil-growth-6.backup.sqlite3`.

Un checkpoint v6 che dichiara già scorte positive d'oro/minerale d'oro viene rifiutato senza sostituire la sessione corrente. Sono verificate anche conversioni dalle versioni precedenti e ripresa esatta di entrambe le produzioni attraverso un mezzo secondo salvato su SQLite.

I cataloghi v4/v5 dei benchmark e il catalogo v6 del primo ferro restano distinti da quello corrente. L'esperimento A/B rimane basato sulla sua colonia civile v5.

## Verifica eseguita

`validate.cmd` terminato con **codice 0**; build Release con **zero avvisi/errori**. Run `.local-cache/m4/runs/326e28fa96b14ecb83a1dc8d4433d547/`, SDK **10.0.201**, Avalonia **12.1.4**, Microsoft.Data.Sqlite **10.0.14**.

Conservati gli undici JSON di input/risultato in `report-inputs/` dello stesso run, con manifest `report-inputs.sha256.json`. TRX, render e log nativi restano isolati nel run. L'esperimento A/B è stato ricompilato e la sua uscita coincide con il rapporto storico v5.

| Suite | Superati | Falliti |
| --- | ---: | ---: |
| Core | 67 | 0 |
| Application | 61 | 0 |
| Infrastructure | 30 | 0 |
| Desktop | 46 | 0 |
| **Totale** | **204** | **0** |

Tredici casi aggiuntivi rispetto a M5.1a: sei applicativi sull'oro, una versione storica aggiunta alla teoria di conversione, due casi SQLite e quattro casi Desktop. Il gate esegue **16 cronologie** più la prova di crescita corrente, i quattro benchmark storici, pubblicazione e smoke nativi di entrambi gli scenari. Generati **17 render**; controllati i layout oro a 1040×740 e 1240×850. DPI fisici e altre piattaforme restano verifiche di rifinitura.

### Produzione da nuova partita

La strategia `gold` segue quella del ferro: Cibo a tempo zero, seconda Casa piccola a 61 s, Pane a 600 s, Casa grande a 760 s, trio ferro/carbone a 1.100 s. A **1.500 s** acquista e assegna Miniera e Fonderia d'oro.

| Momento | Misura osservata |
| --- | --- |
| 1.500 s | Nuova filiera costruita; oro e minerale d'oro ancora zero |
| 1.501 s | 1 minerale d'oro, nessun oro; il minerale appena estratto non viene usato nello stesso passo |
| **1.502 s** | **1 oro**, 1 minerale d'oro, **401 ferro**, **402 carbone**, popolazione **33** |
| 8 ore | Oro **1.000**, Ferro **1.000**, popolazione **48**, scorte entro capienza |

Con entrambe le fonderie a un addetto il carbone si stabilizza: 2/s estratti, 2/s consumati. La fonderia d'oro si ferma a deposito pieno e riparte dopo un Magazzino pagato, producendo l'unità **1.001** nel secondo successivo.

### Scarsità e ripresa del carbone

Un test distinto usa due fonderie di ferro e una d'oro, materie prime disponibili e carbone iniziale zero. Con una sola miniera le due fonderie di ferro consumano il carbone disponibile per prime; la fonderia d'oro segnala Carbone mancante. Costruendo e assegnando una seconda miniera, l'oro riprende dopo la disponibilità degli output del passo precedente. Questa è una verifica di contesa; il confronto esteso fra strategie appartiene a M5.1c.

## Riproduzione e prossimo incremento

```powershell
dotnet tools/SimRisorse.Simulation/bin/Release/net10.0/SimRisorse.Simulation.dll --scenario civil-growth --strategy gold --seconds 1502
```

Output: `.local-cache/m4/gold-start.json`, `gold-8-hours.json`, `validation.json`; TRX, render e log nativi nella cartella del run. Il confronto esteso di scala/carbone è il prossimo incremento M5.1c. Forgia, Zecca, Spade e Monete non sono ancora implementati e richiedono i rispettivi parametri e contratti d'uso.
