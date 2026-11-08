# RC102 — Piano di validazione della protezione sui compositi registrati

**Baseline confermata:** RC95, 1245 test. **RC101:** 1306 test previsti. **RC102:** +12 test Infrastructure, **1318 attesi**. I test delle release RC96–RC102 e il gate F1 non sono ancora confermati su Windows.

## Casi essenziali

1. Registrazione iniziale: il composito preesistente viene legato al percorso del registro; le scritture `Apply` e `AdvanceClock` dirette falliscono senza cambiare revisione o inventario.
2. La barriera SQLite è effettiva anche per comandi SQL ordinari che tentano UPDATE su città, INSERT nelle ricevute e UPDATE/DELETE sul legame d'autorità.
3. Il registro autorevole può ancora applicare finanziamenti e tick, con idempotenza e revisione; le copie non registrate continuano ad essere usabili soltanto nel laboratorio storico.
4. Backup e restore copiano anche la barriera. La promozione di copia identica incrementa l'epoca, lascia non scrivibile direttamente l'origine e permette scritture registrate soltanto sulla destinazione.
5. Un registro copiato in un diverso percorso, o un secondo registro indipendente, non assume l'autorità del file già protetto. Le ricevute deliberatamente alterate continuano a causare rifiuto della promozione.
6. Verificare la compatibilità con RC95–RC101, tutti gli audit F1, render, smoke e assenza di file cache e manifest nell'archivio.

## Comando Windows

```powershell
.\validate.cmd
```

**Accettazione:** 1318 test, 0 falliti, 0 ignorati, gate F1 verde. In questo ambiente non è disponibile .NET 10: i risultati non sono anticipati. In caso di errore conservare i TRX e lo screenshot dell'ultima esecuzione nella `.local-cache` locale, non distribuita.

## Rollback

Il file giocabile non è toccato. Prima di registrare un composito già utilizzato, creare una copia di sicurezza verificata; l'installazione dei trigger modifica intenzionalmente il **database sperimentale**, e il rollback a software RC101 non lo rende nuovamente scrivibile in modalità diretta. Non rimuovere i trigger per aggirare la protezione. Nessuna migrazione automatica del salvataggio v15 normale.
