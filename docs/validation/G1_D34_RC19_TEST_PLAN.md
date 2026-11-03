# G1-D34 / RC19 — Piano di validazione Windows

**Baseline confermata:** RC16 con 476 test superati. **Da validare assieme:** RC17 (481 attesi), RC18 (484 attesi), RC19 (**487 attesi**). RC19 contiene tutte le modifiche precedenti: non serve eseguire le suite intermedie.

## Test automatici

1. Estrarre lo **ZIP RC19 in una nuova cartella**; eseguire backup di ogni salvataggio prima di usare la GUI sperimentale.
2. Eseguire ` .\validate.cmd` dalla root del progetto. Risultato atteso: **487/487 test**, nessun errore di compilazione o test fallito/ignorato.
3. Controllare in particolare `V14MilitaryUiParityTests` (3 casi), `ResearchSeatHintUsesTheSelectedBatchSizeAndAgreesWithCommandState` (RC18) e i test RC17 sulla protezione del checkpoint.
4. In caso di errore, conservare primo errore del compilatore/xUnit e file TRX. Nessuna versione successiva alla RC16 va dichiarata superata senza questa conferma.

## Test manuali senza modificare le partite personali

Aprire ` .\run-game.cmd --experimental-v14` usando una partita di prova, alle risoluzioni **1040×740** e **1240×850**.

- **Esercito, nessuna Caserma:** il suggerimento invita a costruire prima la Caserma. Non deve proporre un arruolamento eseguibile.
- **Esercito, Caserma e cinque Spade in partita di prova:** con `+1` e abitanti liberi il tasto Arruola è attivo e il costo indica «1 recluta»; con `+5` deve essere disabilitato e il testo deve indicare Spade disponibili e necessarie, non un generico errore.
- **Nuova partita:** con azione militare normalmente disponibile, aprire la conferma. Arruola e Annulla reclute restano bloccati; testo dei suggerimenti e limiti segnala la sospensione. Con «Annulla» si ripristinano gli stati originali.
- **Rimborso reclute:** dopo un arruolamento valido, annullare dalla coda e verificare che le Spade ritornino una volta sola. Se manca spazio in magazzino, l'annullamento resta bloccato e mostra la capienza disponibile.
- **Risorse/Edifici:** verificare che la tabella Risorse abbia sempre sette colonne, le righe bloccate restino selezionabili e le due righe di pulsanti costruzione/lavoratori conservino layout, disabled e hover ocra. Queste aree **non sono state modificate in RC19**, quindi la verifica è di non-regressione.
- **Salvataggi RC17:** ripetere su una copia il ciclo Nuova partita → Avvia nuova → chiusura senza Salva manuale, verificando la permanenza del checkpoint precedente. È un gate indipendente non ancora confermato.

## Confini del rilascio

Solo presentazione v14 (testo), 3 test Desktop e documentazione. Nessuna modifica a XAML, Core, Application, Infrastructure, GUI v13, database, catalogo, ricette o prezzo. **P0 grafico aperto, E4 sospeso.**

[Rapporto](../design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Roadmap](../ROADMAP.md) · [Audit completo](G1_D29_AUDIT_GUI_V13_V14.md).
