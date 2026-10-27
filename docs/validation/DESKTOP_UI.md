# Verifica dell'interfaccia corrente

Data: **2026-10-27**. Stato: **verificata su Windows**. Questo rapporto aggiorna la verifica della presentazione dopo [M5.1b Oro](M5_1B_GOLD.md); catalogo `civil-growth-7`, parametri economici e formato dei salvataggi restano quelli già adottati.

## Esecuzione di riferimento

Comando: `.\validate.cmd`, terminato con **codice 0**. Run `899f6ff3e8ff41288330c3d26b4c4ea7`, SDK **10.0.201**, Avalonia **12.1.4**, Microsoft.Data.Sqlite **10.0.14**. Build Release con **zero avvisi e zero errori**.

| Suite | Superati | Falliti |
| --- | ---: | ---: |
| Core | 67 | 0 |
| Application | 61 | 0 |
| Infrastructure | 30 | 0 |
| Desktop | 48 | 0 |
| **Totale** | **206** | **0** |

Rispetto al gate Oro da 204 test, due casi Desktop aggiungono il flusso del nuovo layout alle due risoluzioni. Le verifiche già presenti dell'hover sono state aggiornate allo stile ocra, senza aumentare il conteggio.

Completati anche raggiungibilità dei due cataloghi, **16 cronologie** più la prova di crescita accelerata, **quattro benchmark**, pubblicazione e smoke nativi per entrambi gli scenari. Gli smoke verificano salvataggio, caricamento, ripresa della crescita, notifica di sospensione e chiusura usando file privati.

## Interfaccia verificata

- Sidebar da **240 px**, menu Edifici/Risorse con cambio pagina tramite click e pannello Crescita civile.
- Card comuni alle pagine: abitanti/posti, lavoratori liberi, icona degli abitanti e scorte. Aggiornamento dopo costruzioni e assegnazioni; barre contenute nelle card.
- Dettaglio nell'ordine descrizione → costi in elenco → produzione/effetti e consumi. Costi coerenti con +1/+5/+10, produzione per singolo addetto.
- Stesso font, dimensione **12 px** e peso normale per costi e produzione; icone delle risorse da **14 px**.
- Prerequisiti e diagnosi fissi sopra Costruisci; comandi raggiungibili nei layout **1040×740 e 1240×850**.
- Hover dei pulsanti in ocra **`#E6D5AC`**, uguale alla struttura selezionata, e testo scuro **`#263349`**. Verificati i presenter Fluent di Costruisci e Salva, la stabilità geometrica e il grigio disabilitato con il puntatore ancora sul pulsante.
- Copertura dei pittogrammi dei **17 edifici e 14 risorse**; icona aggiuntiva `population`. Il tema mantiene Consolas con i fallback documentati nel [contratto grafico](../PIXEL_ART_UI.md).

Generati **19 PNG** a partire dalle finestre Avalonia dei test headless: 17 render standard elencati dal manifest del validatore e due aggiuntivi, `structure-details-1040.png` e `structure-details-1240.png`. Ispezionati i dettagli, le tabelle industriali e i layout compatto/standard durante gli interventi.

## Artefatti e screenshot nel README

Riepilogo locale: `.local-cache/m4/validation.json`. TRX e immagini: `.local-cache/m4/runs/899f6ff3e8ff41288330c3d26b4c4ea7/test-results/` e `screenshots/`. Sono artefatti locali esclusi da Git.

Il render `structure-details-1240.png` è copiato senza modifiche in [docs/screenshots/colonia.png](../screenshots/colonia.png) e incorporato nel [README principale](../../README.md), subito dopo il titolo. È una scena di test del gioco attuale, con 8 abitanti, 16 posti e Taglialegna selezionato. [Provenienza e aggiornamento](../screenshots/README.md).

## Limiti e revisione documentale

I render headless e gli smoke Windows non verificano DPI fisici differenti, fallback dei font su Linux/macOS, sospensione reale del computer o accessibilità completa. Il gioco non ha ancora militari, navi, missioni, vittoria o classifica implementati.

La revisione documentale successiva conserva i risultati di questa esecuzione; non introduce un nuovo gate applicativo. Per la documentazione si controllano collegamenti locali, ancore, integrità del PNG e coerenza con il riepilogo di validazione.

**Controllo documentale eseguito:** 62 file Markdown, **468 collegamenti locali** con relative ancore validi, nessun errore. Il PNG versionato è identico alla sorgente del run, misura 1240×850 e non è ignorato da Git. Verificata anche la corrispondenza di conteggi, run e numero dei render con `validation.json`. Rapporto locale in `.local-cache/docs-audit.json`.
