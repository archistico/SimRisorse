# G1-D12 — checkpoint v60-E3b3-RC2

- Ultima versione confermata con `validate.cmd` su Windows: **v60-E3b3-RC1**, 387 test attesi, tutti passati secondo l'utente.
- Nuova candidata: **E3b/3c — tempo unico v14 isolato**, 9 test nuovi, 396 attesi; non compilato né testato .NET nell'ambiente di produzione ZIP.
- Nuovo `V14CampaignClock` con produzione, razioni, crescita, militari, Scuola e saldo ricerca coordinati per secondo; `V14CampaignSession` supporta pausa/ripresa e Advance atomico.
- Restano invariati `GameSession` v13, `SqliteGameSaveStore` v2, GUI Avalonia e script.
- Prossimo passo dopo gate Windows: **E3b/3d**, costruzione e upgrade dei livelli nella sessione; poi replay, GUI e bilanciamento.
- Non sono approvati nuovi numeri di costo, tassi o tempi tecnologici; la decisione confermata è l'accumulo libero/spesa volontaria dei punti ricerca.

[Dettaglio](../../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md) · [ADR 0028](../../adr/0028-e3b3c-orologio-unico-v14-isolato.md).
