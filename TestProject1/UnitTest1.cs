using Gestionale;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRisorsa()
        {
            Risorsa r = new Risorsa();
            r.nome = "Nome";
            r.descrizione = "Descrizione";
            r.quantita = 100;

            Assert.That(r.nome, Is.EqualTo("Nome"));
            Assert.That(r.descrizione, Is.EqualTo("Descrizione"));
            Assert.That(r.quantita, Is.EqualTo(100));

        }

        [Test]
        public void TestRisorse()
        {
            Risorse r = new Risorse();

            Assert.That(r.ListaRisorse[0].nome, Is.EqualTo("Alberi"));
            Assert.That(r.ListaRisorse[1].nome, Is.EqualTo("Legno"));
            Assert.That(r.ListaRisorse[2].nome, Is.EqualTo("Pietra"));
        }
    }
}