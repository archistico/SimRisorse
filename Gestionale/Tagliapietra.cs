using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Tagliapietra : Costruzione
    {
        public Tagliapietra() : base()
        {
            this.quantita = 1;
            this.nome = "Tagliapietra";
            this.descrizione = "Casa del tagliapietra, produce pietre";
            this.costo.legno.quantita = 200;
            this.costo.pietra.quantita = 200;
            this.costo.persone.quantita = 1;

            this.produzione.pietra.quantita = 50;
        }
    }
}
