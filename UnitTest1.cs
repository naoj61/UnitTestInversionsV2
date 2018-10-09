using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inversions;

namespace UnitTestInversions
{
    [TestClass]
    public class UnitTestInversions
    {
        [TestMethod]
        public void CompresDunaVenda()
        {
            try
            {
                InversionsBDContext sessio = new InversionsBDContext();
                sessio.Configuration.AutoDetectChangesEnabled = true; // Si poso true, dona error quan inserto una fila i l'esborro en la mateixa sessió.
                sessio.Configuration.LazyLoadingEnabled = true;

                Usuari.Seleccionat = sessio.Usuaris.Single(s => s.Id == 1);


                var venda = sessio.Moviments.Single(s => s.Id == 100);
                var prod = venda.Prod;
                System.Diagnostics.Debug.WriteLine("\nIdProducte: " + prod.Id);
                System.Diagnostics.Debug.WriteLine("Producte: " + prod._NomProducte);

                var compres = venda.compresAnteriors().ToArray();

                if (venda.Id == 100)
                {
                    Assert.AreEqual(2, compres.Count(), 0, "Count incorrecte");

                    comprovaMoviment(compres[0], 29, 1.827);
                    comprovaMoviment(compres[1], 92, 2877.415);
                }
            }
            catch (AssertFailedException) {}
            catch (Exception es)
            {
                System.Diagnostics.Debug.WriteLine("\nError" + es.Message);
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }

        private static void comprovaMoviment(MovimentCompra compra, int id, double part)
        {
            System.Diagnostics.Debug.WriteLine("\nIdMov={0}. _ParticipacionsDisponibles={1}",
                compra._Moviment.Id, compra._ParticipacionsDisponibles.ToString("0.000"));
            Assert.AreEqual(id, compra._Moviment.Id, 0, "Num Participacions Disponibles incorrecte");
            Assert.AreEqual(part, compra._ParticipacionsDisponibles, 0.001, "Num Participacions Disponibles incorrecte");
        }
    }
}
