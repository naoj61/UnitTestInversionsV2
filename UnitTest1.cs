using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inversions;

namespace UnitTestInversions
{
    [TestClass]
    public class UnitTestInversions
    {
        /// <summary>
        /// Connecta la BD.
        /// </summary>
        /// <param name="usuari"></param>
        /// <returns></returns>
        private static InversionsBDContext ConnectaBd(Usuari.Usuaris usuari)
        {
            InversionsBDContext sessio = new InversionsBDContext();
            sessio.Configuration.AutoDetectChangesEnabled = true; // Si poso true, dona error quan inserto una fila i l'esborro en la mateixa sessió.
            sessio.Configuration.LazyLoadingEnabled = true;

            Usuari.Seleccionat = sessio.Usuaris.Single(s => s.Id == (int) usuari);

            return sessio;
        }


        [TestMethod]
        public void CompresDunaVenda()
        {
            try
            {
                InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

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
                //catch (AssertFailedException) {}
            catch (Exception es)
            {
                System.Diagnostics.Debug.WriteLine("\nError" + es.Message);
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }


        [TestMethod]
        public void CompresOrigDunaVenda()
        {
            try
            {
                InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

                var venda = sessio.Moviments.Single(s => s.Id == 30);

                var compresOrig = venda.__CompresOriginalsAnteriors().ToList();
                var costOrigAntic = venda.Participacions * venda.PreuParticipacioOrigen;
                var costOrigNou = compresOrig.Sum(s => s._ParticipacionsDisponibles*s._Moviment.PreuParticipacio);
                var dif = costOrigAntic - costOrigNou;


                var sumPart = compresOrig.Sum(s => s._ParticipacionsDisponibles);


                 foreach (var movimentCompra in compresOrig)
                {
                    System.Diagnostics.Debug.WriteLine("\nId: {0}; Participacions Disponibles: {1}",
                        movimentCompra._Moviment.Id, movimentCompra._ParticipacionsDisponibles);
                }
                System.Diagnostics.Debug.WriteLine("\n");

                foreach (var movimentCompra in compresOrig)
                {
                    if (movimentCompra._Moviment.Id == 27)
                        Assert.AreEqual(58.502, movimentCompra._ParticipacionsDisponibles, 0.001, "Count incorrecte");
                    else if (movimentCompra._Moviment.Id == 16)
                        Assert.AreEqual(547.645, movimentCompra._ParticipacionsDisponibles, 0.001, "Count incorrecte");
                    else if (movimentCompra._Moviment.Id == 17)
                        Assert.AreEqual(1010.611, movimentCompra._ParticipacionsDisponibles, 0.001, "Count incorrecte");
                    else if (movimentCompra._Moviment.Id == 15)
                        Assert.AreEqual(678.178, movimentCompra._ParticipacionsDisponibles, 0.001, "Count incorrecte");
                    //Assert.AreEqual(678.145, movimentCompra._ParticipacionsDisponibles, 0.001, "Count incorrecte");
                }


                //var prod = venda.Prod;
                //System.Diagnostics.Debug.WriteLine("\nIdProducte: " + prod.Id);
                //System.Diagnostics.Debug.WriteLine("Producte: " + prod._NomProducte);

                //var compres = venda.compresAnteriors().ToArray();

                //if (venda.Id == 100)
                //{
                //    Assert.AreEqual(2, compres.Count(), 0, "Count incorrecte");

                //    comprovaMoviment(compres[0], 29, 1.827);
                //    comprovaMoviment(compres[1], 92, 2877.415);
                //}
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception es)
            {
                System.Diagnostics.Debug.WriteLine("\nError" + es.Message);
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Comprova el càlcul del preu origen en els traspassos de fons.
        /// </summary>
        [TestMethod]
        public void ComprovaCalculPreuOrigen()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            double preuOrig;
            var mov = sessio.Moviments.Single(s => s.Id == 25);
            if (mov.TipusMoviment == TipusMoviment.Compra)
            {
                var venda = mov.MovimentRefVenda;
                preuOrig = venda.Participacions * venda.PreuParticipacioOrigen.Value / mov.Participacions;
                Assert.AreEqual(mov.PreuParticipacioOrigen.GetValueOrDefault(), preuOrig, 0.001, "Preu origen no coincideix");
            }
            else
            {
                var venda = mov;

                var compresVenda = venda.compresAnteriors();
                preuOrig = compresVenda.Sum(movimentCompra => movimentCompra._ParticipacionsDisponibles * movimentCompra._PreuParticipacioOrigen);
                preuOrig = preuOrig / venda.Participacions;
                Assert.AreEqual(venda.PreuParticipacioOrigen.GetValueOrDefault(), preuOrig, 0.001, "Preu origen no coincideix");
            }

            //venda = sessio.Moviments.Single(s => s.Id == 25);
            //venda = sessio.Moviments.Single(s => s.Id == 28);
            //venda = sessio.Moviments.Single(s => s.Id == 30);

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
