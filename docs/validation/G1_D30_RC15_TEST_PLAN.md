# G1-D30 / RC15 — Piano di validazione Windows

**Base:** RC14 dichiarata verde, 470 test. **Candidata:** RC15, **473 test attesi** (+3 Desktop). La verifica statica Linux non equivale a build o test Windows.

## Gate automatico

1. In Windows 11 con SDK .NET 10, estrarre lo ZIP RC15 in directory nuova, senza mescolare sorgenti precedenti.
2. Dalla root: `./validate.cmd`. Atteso: build tutti i progetti e **473/473 test xUnit**, nessuno ignorato o fallito.
3. Se non compilasse, conservare la prima riga `error CS...`, progetto e file/linea. In caso di test falliti, inviare nome e stack trace, mantenendo RC14 come ultimo gate verde.

## Controlli manuali P0 (in sospeso)

- `./run-game.cmd --experimental-v14` alle dimensioni **1040×740** e **1240×850**.
- **Edifici:** selezionare Municipio, Magazzino e Scuola (anche bloccata, selezionabile). Nel box «Capacità» vedere **1.000 Matite**, **500 Matite**, **50 Matite** con icona coerente; non deve sparire la capacità storica né la riga posti Scuola. Confrontare Risorse→tooltip Matite per coerenza, verificando scorta/spazio.
- **Nuova partita:** cliccare Riprendi, poi «Nuova partita»: la sessione si ferma e **Pausa/Riprendi diventa grigio non utilizzabile** finché è mostrata la conferma. Premere «Annulla»: il pulsante torna utilizzabile e Riprendi continua dalla sessione precedente senza azzeramento; ripetere con «Avvia nuova» e verificare la nuova sessione in pausa.
- **Regressioni da tenere d’occhio:** elenco Edifici Base/Grande/Avanzato, righe bloccate grigie ma selezionabili, costruzioni +1/+5/+10 e lavoratori −1/+1/+5/+10, Esercito, Scuola, Albero, sette colonne Risorse, nessuna oscillazione sull’hover.
- Evitare di sovrascrivere un salvataggio personale: creare backup prima di lanciare l’anteprima; la v14 usa SQLite v3 sperimentale separato.

Non dichiarare chiusa la parità GUI P0 senza il confronto delle 22 voci e le due viewport. Non alterare prezzi/tempi economici senza gate E4.

[Audit UI-01…UI-22](G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md).
