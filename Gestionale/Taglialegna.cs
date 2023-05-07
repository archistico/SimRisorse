using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Taglialegna : Costruzione
    {
        public Taglialegna() : base() 
        {
            this.quantita = 1;
            this.nome = "Taglialegna";
            this.descrizione = "Casa del taglialegna, produce alberi";
            this.costo.legno.quantita = 100;
            this.costo.pietra.quantita = 50;
            this.costo.persone.quantita = 1;

            this.produzione.alberi.quantita = 50;
        }
    }
}
