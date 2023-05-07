using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Costruzione
    {
        public string nome = "";
        public string descrizione = "";
        public int quantita = 0;
        public Risorse dotazione = new Risorse();
        public Risorse costo = new Risorse();
        public Risorse produzione = new Risorse();
        public Risorse consumo = new Risorse();
        public Risorse reciclo = new Risorse();
        public Risorse magazzino = new Risorse();

        public DateTime inizio;

        public Costruzione()
        {
            this.inizio = DateTime.Now;
        }

        public int CicliTotali()
        {
            return Convert.ToInt32(System.Math.Round((DateTime.Now - inizio).TotalSeconds));
        }
    }
}
