# RC81 — Piano di validazione runtime e persistenza Zecca a squadre

**Stato:** candidata; l'ambiente di preparazione non possiede SDK .NET 10, compilazione ed esecuzione xUnit **non svolte**. Ultima suite confermata: RC78 **926/926**.

| Release | Nuovi test | Totale cumulativo atteso | Stato |
| --- | ---: | ---: | --- |
| RC78 | — | 926 | verificata dall'utente |
| RC79 | +17 | 943 | da eseguire |
| RC80 | +22 | 965 | da eseguire |
| **RC81** | **+20** (6 Core, 10 Application, 4 Infrastructure) | **985** | da eseguire |

## Comando obbligatorio su Windows

```powershell
.\validate-f1.cmd
```

Usare report TRX fresco e gate in `.local-cache/m4/f1-gates/`, senza dichiarare superamento prima di vedere **985/985**, zero errori, zero test ignorati e smoke Desktop riuscito. Non usare `-ExpectedTests` per forzare un numero inferiore. Eseguire inoltre confronto manuale della GUI con la v14 canonica.

## Copertura specifica aggiunta

- **Core**: capacità per tutti i tier, indipendenza tra posti lavorativi e moduli, upgrade che non inventa persone, `CrewSize=1` storico invariato, nuova versione delle regole e rifiuto di valori non validi. Il test RC80 9+9 tra tier è ora effettivamente rispettato dal planner grazie al parametro `staffedModules`.
- **Application**: clock v15 con 0/9/10/19/20 addetti su due Zecche, output e consumi effettivi, azioni atomiche per capienza insufficiente, `EnableMintCrews`, vecchio regime, pausa e checkpoint invalido.
- **Infrastructure**: persistenza `Legacy`/`MintTen` nel campo `rules_version`, nessuna differenza nella generation 6, ripresa dopo un tick di campagna con dieci persone alla Zecca, rifiuto versione ignota.

## Controlli manuali ancora dovuti

- Sessioni reali dopo `EnableMintCrews` con altre industrie che competono per il Carbone; checkpoint in pausa e dopo ripresa; due/tre Zecche.
- GUI con personale 9/10, `Workforce`/`Summary`, input e output reali, pulsanti nello stesso posto, disabilitato e hover corretti, risorse a sette colonne.
- Soglia di capacità Monete piena, ricerca per i tier Grande/Avanzato, diverse quantità di Magazzini, upgrade con personale preassegnato, salvataggio e riapertura sullo stesso percorso.
- Distinguere nuovi benchmark naturali `MintTen` dai run storici `Legacy`. Non attribuire prestazioni o raggiungibilità ai test controllati.

Non sono stati creati TRX né nuove evidenze di esecuzione. I controlli statici, elencati in `RC81_STATIC_AUDIT.md`, non dimostrano che i test siano passati.
