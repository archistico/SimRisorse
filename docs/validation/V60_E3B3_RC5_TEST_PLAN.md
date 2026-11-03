# Validazione v60-E3b3-RC5 — Piano

**Baseline Windows confermata:** RC4 `validate.cmd` 408/408. **Nuovo incremento:** 7 test Application + 2 Infrastructure = **417 test attesi**, senza test locali .NET nel container.

## Nuovi test Application

- Primo acquisto Casa con storico corretto e senza crescita popolazione istantanea.
- Acquisto 2 Case: prezzi scalari successivi = transazione multipla.
- Insufficienza materiali: nessun acquisto parziale o modifica del contatore.
- Validazioni negative: ID sconosciuto, Municipio fisso, produttore via comando sbagliato, prerequisiti e quantità.
- Capacità condivisa Magazzino e capienza delle Case anche nel validatore snapshot.
- Prezzi civili con fattore configurato a 1, senza toccare v13.
- Caserma dopo Forgia: contatore, edificio e stato militare coerenti.

## Nuovi test Infrastructure

- Replay comandi civili + Scuola + formazione + ricerca + tier + upgrade + clock; sessione continua e checkpoint SQLite v3 a metà, comparate su tutte le proprietà.
- Ripresa da SQLite con prezzo Casa progressivo e errori senza commit.

## Esecuzione Windows

```powershell
.\validate.cmd
```

Oltre ai 417 test, il validatore esegue i controlli supplementari già previsti. Non usare DB personali; usare la cartella estratta dello ZIP. Se si verificano errori, comunicare almeno la prima eccezione completa della suite che fallisce.

## Verifiche non equivalenti ai test Windows

Solo parsing XML/JSON, archiviazione ZIP, differenze su file tecnici e controlli dei link documentali sono eseguibili in questo ambiente. Non dichiarare la RC5 compilata o testata senza il log Windows.
