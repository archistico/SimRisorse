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
            costruzioni.Add(new Segheria());
            costruzioni.Add(new Tagliapietra());
        }

        private void CalcoloMagazzino()
        {
            Magazzino magazzinoAttuale = new Magazzino();

            foreach (Costruzione costruzione in this.costruzioni)
            {
                foreach (Risorsa risorsa in costruzione.magazzino.Lista)
                {
                    magazzinoAttuale.massimo.getRisorsa(risorsa.nome).quantita += risorsa.quantita;
                }
            }

            magazzino = magazzinoAttuale;
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
            DateTime ora = DateTime.Now;
            int secondi = Convert.ToInt32(System.Math.Round((ora - inizio).TotalSeconds));

            lblSecondiPassati.Text = secondi.ToString();

            CalcoloMagazzino();
            CalcoloCiclo();

            VisualizzaMagazzino();
        }
    }
}