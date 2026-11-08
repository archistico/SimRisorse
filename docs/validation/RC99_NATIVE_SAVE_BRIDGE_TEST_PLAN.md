# Piano test RC99 — Ponte salvataggi nativi e contenitore composito

**Baseline confermata:** RC95, 1245 test. **RC98:** 1270 attesi, ancora candidati. **RC99:** +12 test Infrastructure, **1282 complessivi attesi**. Non dichiarare un esito verde senza eseguire `.\validate.cmd` su Windows.

## Casi di accettazione

- Importazione di un v15 generation 6 valido in nuovo SQLite sperimentale, confronto dello stato e verifica byte-per-byte dell'originale.
- Rifiuto di input estraneo, sorgente=destinazione, destinazione esistente o cartella inesistente; nessuna sovrascrittura e nessun file temporaneo residuo.
- Export di uno snapshot senza cantiere, che deve essere leggibile dal caricatore normale v15.
- Blocco export con preventivo aperto (anche se ancora senza rata) o materiali versati.
- Completamento finanziato sintetico della Zecca: il nuovo salvataggio nativo contiene **una sola Zecca**, contatore storico coerente e inventario già decurtato, senza secondo addebito. I pagamenti e le ricevute rimangono nel database composito.
- Rifiuto di export verso file esistente, path identici o composito inesistente.
- Conservazione del database composito e pulizia dello staging in caso di successo o rifiuto.

## Gate unico

```powershell
.\validate.cmd
```

Richiede 1282/1282 test e tutti gli audit e screenshot F1 già previsti. Non usare `validate-f1.cmd`. Non includere `.local-cache` o manifest SHA-256 nell'archivio.

## Limiti delle prove

La transazione di export crea un nuovo file e non modifica il salvataggio in uso; **non** implementa ancora una migrazione in-place di una partita con cantiere pendente. I materiali sintetici del fixture RC98 non rappresentano la produzione naturale della città.
