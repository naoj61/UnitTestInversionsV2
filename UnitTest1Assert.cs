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
        /// Suma de compres reals ha de ser igual a la suma de les vendes reals mes les participacions en cartera.
        /// </summary>
        [TestMethod]
        public void ComprovaPartsOrig()
        {
           
            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var totalPartsCompresReals = sessio.MovimentsUsuari.Where(w => w._EsCompraReal).Sum(s => s.Participacions);
            var totalPartsVendesReals = sessio.MovimentsUsuari.Where(w => w._EsVendaReal).Sum(s => s.Participacions);
            double totalPartsEnCartera = 0;

            foreach (var producte in sessio.Productes)
            {
                if(producte._Participacions == 0)
                    continue;
                
                var compresAnt = producte.compresAnteriors3Test(DateTime.Now, producte._Participacions);

                foreach (var compra in compresAnt)
                {
                    foreach (var desglosCompra in compra.DesglosCompres)
                    {
                        totalPartsEnCartera += desglosCompra._ParticipacionsDisponiblesOrig;
                    }
                }
            }

            var total = totalPartsCompresReals - totalPartsVendesReals - totalPartsEnCartera;

            Debug.WriteLine(String.Format("Total parts compres reals:\t{0}", totalPartsCompresReals.ToString("####.000", CultureInfo.CurrentCulture)));
            Debug.WriteLine(String.Format("Total parts vendes reals:\t{0}", totalPartsVendesReals.ToString("####.000", CultureInfo.CurrentCulture)));
            Debug.WriteLine(String.Format("Total parts en cartera:\t{0}", totalPartsEnCartera.ToString("####.000", CultureInfo.CurrentCulture)));
            Debug.WriteLine(String.Format("Total parts:\t{0}", total.ToString("####.000", CultureInfo.CurrentCulture)));

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void ComparaPreuCompraOrigAmbSelfBank()
        {
            /*
              							Import compra orig
                 5-DWS Deutschland LC			  777.35 €
                 6-Candriam Eqs L Biotech		 6132.72 €
                16-DWS Aktien Strategie			17922.04 €
                19-Trea NB Capital Plus		 	 1588.63 €
                27-Fidelity Global Technology	27338.26 €
                 7-Arcelor Mittal Steel         26084,35 €
                Total                           79843,35 €
             */

            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            Producte prod;
            double costOrigEnCart;
            double importSelf;
            double total = 0;

            Debug.WriteLine("\nProd:\tSelf:\tAquí:\tDif:");

            importSelf = 777.35;
            prod = sessio.Productes.Single(s => s.Id == 5);
            costOrigEnCart = prod.costOriginalEnCartera2Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000",CultureInfo.CurrentCulture), prod);

            importSelf = 6132.72;
            prod = sessio.Productes.Single(s => s.Id == 6);
            costOrigEnCart = prod.costOriginalEnCartera2Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 17922.04;
            prod = sessio.Productes.Single(s => s.Id == 16);
            costOrigEnCart = prod.costOriginalEnCartera2Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 1588.63;
            prod = sessio.Productes.Single(s => s.Id == 19);
            costOrigEnCart = prod.costOriginalEnCartera2Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 27338.26;
            prod = sessio.Productes.Single(s => s.Id == 27);
            costOrigEnCart = prod.costOriginalEnCartera2Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 26084.35;
            prod = sessio.Productes.Single(s => s.Id == 7);
            costOrigEnCart = prod.costOriginalEnCartera2Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 79843.35;
            //Assert.AreEqual(importSelf, total, .0001);
            Debug.WriteLine("Total\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), total.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - total).ToString("####.000", CultureInfo.CurrentCulture));

            Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// 
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
        /// Compara els preu originals amb el sistema nou i amb l'antic  a nivell de moviment compra.
        /// </summary>
        [TestMethod]
        public void TestPreuOriginal()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            Debug.WriteLine("********** Inici **********");

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

            Debug.WriteLine("\nOks={0}. Kos={1}", oks, kos);

            Assert.IsTrue(kos == 0, "\nHi ha compres que no quadren");
            Debug.WriteLine("Final");
        }

        #endregion *** Test ***

    }
}
