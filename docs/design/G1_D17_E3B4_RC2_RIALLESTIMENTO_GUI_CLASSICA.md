# G1-D17 — GUI v14 riallineata al layout storico

## Stato e obiettivo

Base Windows **v60-E3b4-RC1 validata, 427 test passati comunicati dall'utente**. La RC2 risponde alla richiesta di ripristinare la presentazione della GUI pixel art esistente; non cambia la simulazione né i database.

## Cosa è stato realizzato

- `Views/V14PreviewWindow.axaml`: ricalca la struttura dell'originale `MainWindow.axaml` — colonna laterale da 240px con 5 voci, header, riepilogo abitanti, tre scorte in evidenza, area liste/dettagli, footer salvataggi.
- Unica sezione Edifici: tutte le 48 declinazioni produttive hanno un proprio nome e una propria riga, con conteggio per livello, personale `assegnati/posti`, requisiti, costo e ricetta. Aggiunte Scuola e le 5 voci storiche non produttive.
- Pannello dettagli compatto, icone `PixelGlyph`, righe per produzione, consumo e stoccaggio. Le capacità base/grande/avanzato sono rispettivamente `+50/+150/+500` **per risorsa prodotta**; l'output/consumo rimane basato sul lavoratore effettivamente assegnato.
- Le azioni Costruisci/Potenzia/Assegna/Libera agiscono sulla selezione **famiglia+taglia**, senza ComboBox.
- Scuola e Albero tecnologico nel menu laterale; Esercito rimane visibile con riepilogo v14 (azioni complete ancora future). Risorse usa la tabella tradizionale e la capacità condivisa.
- Tutti i row object sono persistenti nei refresh, così la selezione non cambia quando scorre l'orologio. Save e Load SQLite v3 rimangono isolati dalla v13.

## Cosa NON è modificato

Il gameplay `civil-growth-13`, `MainWindow`, `MainViewModel`, Core, Application, Infrastructure e gli schemi/backup SQLite. In modalità normale l'utente vede la stessa GUI già validata.

## Test RC2

- Test Windows confermati precedente RC1: **427**.
- RC2 introduce **3 test Desktop aggiuntivi** e sostituisce il test Headless che verificava la vecchia disposizione, per un totale atteso di **430 test**.
- Da eseguire su Windows: `\.\validate.cmd` [usare la sintassi PowerShell `./validate.cmd` o `.\validate.cmd`].
- Verifiche visive manuali: navigazione a 1240x850 e 1040x740, scrollbar lista edifici di 54 voci, costi e icone, colori selezione, assenza assoluta di ComboBox nel dettaglio v14, prezzi/capacità corretti, sospensione, carica/salva e anteprima separata.

## Rischi aperti

La GUI v14 in questa RC2 riprende la composizione principale e il design storico, ma non è ancora una sostituzione definitiva di `MainWindow`. L'Esercito v14 è temporaneamente in sola lettura. Un eventuale riuso XAML strutturale fra MainWindow e Preview verrà valutato dopo l'approvazione visiva, senza intervenire ora sulla GUI v13 stabilizzata.

## Decisione

[ADR 0032](../adr/0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md).
