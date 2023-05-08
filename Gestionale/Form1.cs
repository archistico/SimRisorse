using System.Collections.Generic;
using System.Timers;

namespace Gestionale
{
    public partial class Form1 : Form
    {
        public DateTime inizio;
        public List<Costruzione> costruzioni = new List<Costruzione>();
        public Magazzino magazzino = new Magazzino();
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            inizio = DateTime.Now;
            orologio.Start();

            costruzioni.Add(new Comune());
            costruzioni.Add(new Taglialegna());
            costruzioni.Add(new Taglialegna());
            costruzioni.Add(new Segheria());
            //costruzioni.Add(new Tagliapietra());
        }        

        private void CalcoloCiclo()
        {

        }

        private void VisualizzaMagazzino()
        {
            lblMagazzino.Text = String.Empty;
            foreach (Risorsa risorsa in new Risorse().Lista)
            {
                lblMagazzino.AppendText(risorsa.nome + " \t"
                        + magazzino.deposito.getRisorsa(risorsa.nome).quantita + " \t"
                        + magazzino.massimo.getRisorsa(risorsa.nome).quantita + System.Environment.NewLine);
            }
        }

        private void orologio_Tick(object sender, EventArgs e)
        {
            int secondi = Convert.ToInt32(System.Math.Round((DateTime.Now - inizio).TotalSeconds));

            lblSecondiPassati.Text = secondi.ToString();

            magazzino.CalcoloMassimoMagazzino(costruzioni);
            magazzino.CalcoloDepositoCiclo(costruzioni);

            VisualizzaMagazzino();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            costruzioni.Add(new Segheria());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            costruzioni.Add(new Taglialegna());
        }
    }
}