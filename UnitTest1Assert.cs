using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Comuns;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inversions;
using Microsoft.Win32;

namespace UnitTestInversions
{
    [TestClass]
    public class UnitTest1Assert
    {
        #region *** Test ***

        /// <summary>
        /// .
        /// </summary>
        [TestMethod]
        public void ComparaCompresDesgloçCompres()
        {
            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var sumCompres = sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra && w.TipusMoviment != TipusMoviment.Traspàs).Sum(s => s.Participacions);
            var sumDesgloç = sessio.DesglosCompras.Sum(s => s.Participacions);

            Assert.AreEqual(sumCompres, sumDesgloç, .0001);

            Debug.WriteLine("\nFinal");
        }

        /// <summary>
        /// Comprova que ProvacompresDeLaVenda reparteix correctament.
        /// </summary>
        [TestMethod]
        public void ProvaCompresDeLaVenda()
        {
            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = UnitTest1.ConnectaBd();

            int cont = 0;
            foreach (var usuari in sessio.Usuaris.ToList())
            {
                Usuari.Seleccionat = usuari;

                var vendes = sessio.MovimentsUsuari.Where(w => w.TipusMoviment == TipusMoviment.Venda).ToList();

                foreach (var venda in vendes)
                {
                    List<DesglosCompra> xxx = new List<DesglosCompra>();
                    List<Moviment> compres = venda.compresDeLaVenda3Test().ToList();
                    foreach (var compra in compres)
                    {
                        xxx.AddRange(compra.DesglosCompres.ToList());
                    }

                    var sumaDesgolç = xxx.Sum(s => s._ParticipacionsDisponibles);

                    Assert.AreEqual(sumaDesgolç, venda.Participacions, .0001, "Prod:{0}. VendaId:{1}", venda.Prod, venda.Id);
                    cont++;
                }
            }
            Debug.WriteLine("\nTotal vendes:{0}", cont);
            Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Comprova l'import de Participacions a Moviments amb la suma de Participacions a DesglosCompra.
        /// </summary>
        [TestMethod]
        public void ComprovaDesgloçCompres()
        {
            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            int contErr = 0;
            var compres = sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra).ToList();
            foreach (var compra in compres)
            {
                var sumPartsDesg = compra.DesglosCompres.Sum(s => s.Participacions);
                if (!Utilitats.SonIguals(compra.Participacions, sumPartsDesg, 2))
                {
                    if(contErr==0)
                        Debug.WriteLine("\n");

                    contErr++;
                    Debug.WriteLine("Prod:{3}. Error Id:{0}. sumPartsDesg:{1}. Parts:{2}", compra.Id, sumPartsDesg, compra.Participacions, compra.Prod);
                }
            }
            Debug.WriteLine("\n TotOk:{0}. TotError:{1}.", compres.Count - contErr, contErr);

            Assert.AreEqual(contErr, 0);

            Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Comprova que ProvaParticipacionsDisponiblesCompra reparteix correctament.
        /// </summary>
        [TestMethod]
        public void ProvaParticipacionsDisponiblesCompra()
        {
            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);
            var compra = sessio.Moviments.Single(w => w.Id == 101);

            double partsRest;
            List<DesglosCompra> xxx = compra.DesglosCompres.ToList();

            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(0, 20);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 20, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(partsRest, 0, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(0, 30);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 24.7409, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 5.2591, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(partsRest, 0, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(0, 200);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 24.7409, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 48.378, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 87.6339, .001);
            Assert.AreEqual(partsRest, 39.2472, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(5, 10);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 10, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(partsRest, 0, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(20, 20);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 4.7409, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 15.2591, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(partsRest, 0, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(30, 50);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 43.1189, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 6.8811, .001);
            Assert.AreEqual(partsRest, 0, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(80, 10);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 10, .001);
            Assert.AreEqual(partsRest, 0, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(80, 100);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 80.7528, .001);
            Assert.AreEqual(partsRest, 19.2472, .001);

            DesglosCompra.ResetParticipacionsDisponibles(compra.DesglosCompres);
            partsRest = compra.trobaParticipacionsDisponiblesDesgloçCompraTest(0, 0);
            Assert.AreEqual(xxx[0]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[1]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(xxx[2]._ParticipacionsDisponibles, 0, .001);
            Assert.AreEqual(partsRest, 0, .001);

            Debug.WriteLine("\nFinal");
        }



        /// <summary>
        /// No poden haver data hora minut segon duplicats.
        /// </summary>
        [TestMethod]
        public void ComprovaDatesMovDuplicades()
        {
            try
            {
                InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

                bool totOk = true;

                System.Diagnostics.Debug.WriteLine("********** Inici Comprovació dates **********");
                DateTime dataAnt = DateTime.MinValue;
                foreach (var mov in sessio.Moviments.OrderBy(o => o.Data).ToList())
                {
                    if (dataAnt == mov.Data)
                    {
                        System.Diagnostics.Debug.WriteLine("Data duplicada. {0}. IdMov.{1}", dataAnt, mov.Id);
                        totOk = false;
                    }
                    dataAnt = mov.Data;
                }
                System.Diagnostics.Debug.WriteLine("********** Final Comprovació dates **********");

                Assert.IsTrue(totOk, "Hi ha dates duplicades");
            }
            catch (AssertFailedException)
            {
                // Si no capturo l'error aquí, passaria pel "catch (Exception ex)" i el test acabaria bé.
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Comprova el càlcul del preu origen en els traspassos de fons.
        /// </summary>
        [TestMethod]
        public void ComprovaCalculPreuOrigen()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var thailand = sessio.ProdFons.Single(w => w.Id == 13);

            var pig = thailand.pig2TotalTest(inclouCartera: false);
            var pig2013 = thailand.pig2TotalTest(2013);

            double preuOrig;
            var mov = sessio.Moviments.Single(s => s.Id == 25);
            if (mov.TipusMoviment == TipusMoviment.Compra)
            {
                var venda = mov.MovimentRefVendaN;
                preuOrig = venda.Participacions * venda._PreuParticipacioOrigen.GetValueOrDefault() / mov.Participacions;
                Assert.AreEqual(mov._PreuCompraParticipacioOrigen, preuOrig, 0.001, "Preu origen no coincideix");
            }
            else
            {
                var venda = mov;

                var compresVenda = venda.compresAnteriors();
                preuOrig = compresVenda.Sum(movimentCompra => movimentCompra._ParticipacionsDisponibles * movimentCompra._PreuParticipacioOrigenTest);
                preuOrig = preuOrig / venda.Participacions;
                Assert.AreEqual(venda._PreuCompraParticipacioOrigen, preuOrig, 0.001, "Preu origen no coincideix");
            }

            //venda = sessio.Moviments.Single(s => s.Id == 25);
            //venda = sessio.Moviments.Single(s => s.Id == 28);
            //venda = sessio.Moviments.Single(s => s.Id == 30);

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }




        /// <summary>
        /// Compara els preu originals amb el sistema nou i amb l'antic  a nivell de moviment compra.
        /// </summary>
        [TestMethod]
        public void TestPreuOriginal()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            System.Diagnostics.Debug.WriteLine("********** Inici **********");

            int oks = 0, kos = 0;

            foreach (var compra in sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
            {
                var costTotalOrig = compra.DesglosCompres.Sum(s => s.ParticipacionsOrig * s._PreuParticipacioOrig);
                var preuUnitOrig = Math.Round(costTotalOrig / compra.Participacions, 4);
                var preuOrigAnt = Math.Round(compra._PreuCompraParticipacioOrigen, 4);
                var dif = Math.Round(preuUnitOrig - preuOrigAnt, 2);

                if (dif > 0)
                {
                    Debug.WriteLine("MovId = {0}\tDif = {3}\tpreuUnitOrig = {1}\tpreuOrigAnt = {2}"
                        , compra.Id, preuUnitOrig.ToString("#,##0.00€"), preuOrigAnt.ToString("#,##0.00€"), dif.ToString("#,##0.00€"));
                    kos++;
                }
                else
                {
                    oks++;
                }
            }

            System.Diagnostics.Debug.WriteLine("\nOks={0}. Kos={1}", oks, kos);

            Assert.IsTrue(kos == 0, "\nHi ha compres que no quadren");
            System.Diagnostics.Debug.WriteLine("Final");
        }

        #endregion *** Test ***

    }
}
