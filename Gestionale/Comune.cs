using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Comune : Costruzione
    {
        public Comune() : base()
        {
            this.quantita = 1;
            this.nome = "Comune";
            this.descrizione = "Casa comunale in cui tutto si decide";
            this.costo.persone.quantita = 1;

            for (int i = 0; i < this.magazzino.ListaPersone.Count; i++)
            {
                this.magazzino.ListaPersone[i].quantita = 100;
            }

            for (int i = 0; i < this.magazzino.ListaRisorse.Count; i++)
            {
                this.magazzino.ListaRisorse[i].quantita = 5000;
            }
        }
    }
}
