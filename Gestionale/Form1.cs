using System.Timers;

namespace Gestionale
{
    public partial class Form1 : Form
    {
        public DateTime inizio;
        public Costruzione[] costruzioni = new Costruzione[5];
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            inizio = DateTime.Now;
            orologio.Start();

            costruzioni[0] = new Costruzione { nome = "Comune", quantita = 1 };
            costruzioni[1] = new Costruzione { nome = "Taglialegna", quantita = 1 };

            costruzioni[1].produzione.legno.quantita = 1;

        }

        private void orologio_Tick(object sender, EventArgs e)
        {
            DateTime ora = DateTime.Now;
            int secondi = Convert.ToInt32(System.Math.Round((ora - inizio).TotalSeconds));

            lblSecondiPassati.Text = secondi.ToString();            
        }
    }
}