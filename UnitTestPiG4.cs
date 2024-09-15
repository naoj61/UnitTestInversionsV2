using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public class UnitTestPiG4
    {

        [TestMethod]
        public void PigTotal()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);
            
            var movUsuari = sessio.Moviments.Where(mov => mov.UsuariId == Usuari.Seleccionat.Id).ToList();

            var compres = movUsuari.Where(mov => mov._EsCompraReal).ToList();
            var vendes = movUsuari.Where(mov => mov._EsVendaReal).ToList();
            var dividends = movUsuari.Where(mov => mov._EsDividents).ToList();

            var totalCompres = compres.Sum(compra => compra.Participacions * compra.PreuParticipacio);
            var totalVendes = vendes.Sum(venda => venda.Participacions * venda.PreuParticipacio);
            var totalDespeses = movUsuari.Sum(mov => mov.Despeses.GetValueOrDefault());
            
            var totalEnCartera = Enumerable.Sum(sessio.Productes, producte => producte.partsEnCarteraTest() * producte._PreuParticipacioActual);

            decimal pig = (totalEnCartera + totalVendes) - (totalCompres + totalDespeses);

            var pigVendesReals = vendes.Sum(venda => venda.pigVenda4Test(true, true, false));
            var pigEnCartera = Enumerable.Sum(sessio.Productes, producte => producte.pigEnCartera4Test(true, true));
         
            var xx = pig.ToString("#,###.##");
            var yy = (pigEnCartera + pigVendesReals).ToString("#,###.##");
        }


        [TestMethod]
        public void PigTotesLesVendes()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var prod = sessio.ProdAccions.Single(s => s.Id == 7); // Arcelor

            var vendes = sessio.Moviments.Where(mov => mov.TipusMoviment == TipusMoviment.Venda).ToList();
            vendes = vendes.Where(venda => venda.Prod == prod).ToList();
            
            int compt = 0;
            foreach (var venda in vendes)
            {
                if (venda.Prod is ProdFons && !venda._EsVendaReal)
                    continue;
                compt++;
                var aa = venda.pigVenda4Test(true, true);
                aa = venda.pigVenda4Test(true, false);
                aa = venda.pigVenda4Test(false, true);
                //aa = venda.pigVenda4Test(false, false);
            }
        }


        [TestMethod]
        public void PigProdsEnCarteraActual()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            // ***** Data 14/09/2024. Prods Fons: 27, 28, 1042, 1043. Prods Acc: 7, 1044.

            // *** 27-Global Technology Fund A-acc-eur
            var prod = sessio.Productes.Single(s => s.Id == 27);

            ModificacioTemporalValoracioActual(sessio, prod, 47.53m);

            var pigNoOrig = prod.pigEnCartera4Test(false, false);
            Assert.AreEqual((double)pigNoOrig, 4283.997, .01);

            var pigOrig = prod.pigEnCartera4Test(true, false);
            Assert.AreEqual((double)pigOrig, 14587.662, .01);


            // *** 28-Euro Fund A-2 Acc
            prod = sessio.Productes.Single(s => s.Id == 28);

            ModificacioTemporalValoracioActual(sessio, prod, 460.314m);

            pigNoOrig = prod.pigEnCartera4Test(false, false);
            Assert.AreEqual((double)pigNoOrig, 4075.19, .01);


            // *** 1042-DWS Floating Rate Notes LC
            prod = sessio.Productes.Single(s => s.Id == 1042);

            ModificacioTemporalValoracioActual(sessio, prod, 89.31m);

            pigNoOrig = prod.pigEnCartera4Test(false, false);
            Assert.AreEqual((double)pigNoOrig, 439.509, .01);


            // *** 1043-Global Technology A-Acc-USD H
            prod = sessio.Productes.Single(s => s.Id == 1043);

            ModificacioTemporalValoracioActual(sessio, prod, 45.5792m);

            pigNoOrig = prod.pigEnCartera4Test(false, false);
            Assert.AreEqual((double)pigNoOrig, -49.602, .01);


            // *** Arcelor Self
            prod = sessio.Productes.Single(s => s.Id == 7);

            ModificacioTemporalValoracioActual(sessio, prod, 20.51m);

            pigNoOrig = prod.pigEnCartera4Test(false, false);
            Assert.AreEqual((double)pigNoOrig, 27, .01);

            var pigNoOrigAmbDespeses = prod.pigEnCartera4Test(false, true);
            Assert.AreEqual((double)pigNoOrigAmbDespeses, 19.72, .01);


            // *** Arcelor Trade
            prod = sessio.Productes.Single(s => s.Id == 1044);

            ModificacioTemporalValoracioActual(sessio, prod, 20.514m);

            pigNoOrig = prod.pigEnCartera4Test(false, false);
            Assert.AreEqual((double)pigNoOrig, 614, .01);

            pigNoOrigAmbDespeses = prod.pigEnCartera4Test(false, true);
            Assert.AreEqual((double)pigNoOrigAmbDespeses, 613, .01);
        }

        #region ***** Producte 27 *****
       
        [TestMethod]
        public void PigVendesProd27()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var venda = sessio.Moviments.Single(s => s.Id == 1251);

            var pigOrig = venda.pigVenda4Test(true, true);
            Assert.AreEqual((double)pigOrig, 6715.381, .01);

            var pigNoOrig = venda.pigVenda4Test(false, true);
            Assert.AreEqual((double)pigNoOrig, 4654.800, .01);


            venda = sessio.Moviments.Single(s => s.Id == 198);

            pigOrig = venda.pigVenda4Test(true, true);
            Assert.AreEqual((double)pigOrig, 0, .01);

            pigNoOrig = venda.pigVenda4Test(false, true);
            Assert.AreEqual((double)pigNoOrig, 5086.172, .01);


            venda = sessio.Moviments.Single(s => s.Id == 1279);

            pigOrig = venda.pigVenda4Test(true, true);
            Assert.AreEqual((double)pigOrig, 0, .01);

            pigNoOrig = venda.pigVenda4Test(false, true);
            Assert.AreEqual((double)pigNoOrig, 635, .01);
        }

        [TestMethod]
        public void PigCompra173()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 173);

            // Poso el valor actuar de la participacio a 46,53 €
            ModificacioTemporalValoracioActual(sessio, compra.Prod, 46.53m);


            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, 12409.021, .01); // Valor per PU actual = 46,53 €

            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 6715.381, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, 32050.46, .01); // Valor per PU actual = 46,53 €

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, 28023.202, .01);

            var pigEncarteraCompra = pigAmbCartera - pig;
            Assert.AreEqual((double)pigEncarteraCompra, 4027.26, .01); // Valor per PU actual = 46,53 €

            var pigOrigEncarteraCompra = pigOrigAmbCartera - pigOrig;
            Assert.AreEqual((double)pigOrigEncarteraCompra, 5693.64, .01); // Valor per PU actual = 46,53 €

            //var pigEncarteraProducte = compra.Prod.pigEnData4Test(DateTime.Now, true, true, null, null);
            //Assert.AreEqual((double)pigEncarteraProducte, 13883.191, .01); // Valor per PU actual = 46,53 €
        }

        [TestMethod]
        public void PigProd27()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var prod = sessio.ProdFons.Single(s => s.Id == 27);

            // Poso el valor actuar de la participacio a 46,53 €
            var valoracio = sessio.Valoracio.ToList().Where(val => val.Prod == prod).OrderBy(val => val.Data).Last();
            valoracio.PreuParticipacio = 46.53m;

            var pigOrigAmbCartera = prod.pigEnCartera4Test(true, true);
            var pigOrigAmbCartera2 = prod.pigHistoric4Test(true, true, DateTime.Now);

            /*
            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, 34994.61, .01); // Valor per PU actual = 46,53 €

            var pigAmbCartera = compra.pigCompra4Test(true, false, true, true);
            Assert.AreEqual((double)pigAmbCartera, 32050.46, .01); // Valor per PU actual = 46,53 €

            var pigOrig = compra.pigCompra4Test(true, true, false, true);
            Assert.AreEqual((double)pigOrig, 29300.972, .01);

            var pig = compra.pigCompra4Test(true, false, false, false);
            Assert.AreEqual((double)pig, 28023.202, .01);

            var pigEncarteraCompra = pigAmbCartera - pig;
            Assert.AreEqual((double)pigEncarteraCompra, 4027.26, .01); // Valor per PU actual = 46,53 €

            var pigOrigEncarteraCompra = pigOrigAmbCartera - pigOrig;
            Assert.AreEqual((double)pigOrigEncarteraCompra, 5693.64, .01); // Valor per PU actual = 46,53 €

            var pigEncarteraProducte = compra.Prod.pigEnData4Test(DateTime.Now, true, true, null, null);
            Assert.AreEqual((double)pigEncarteraProducte, 13883.191, .01); // Valor per PU actual = 46,53 €
             */
        }
        
        #endregion ***** Producte 27 *****       
 

        #region ***** Producte 29 *****

        [TestMethod]
        public void PigVendesProd29()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var venda = sessio.Moviments.Single(s => s.Id == 1229);

            var pigOrig = venda.pigVenda4Test(true, true);
            Assert.AreEqual((double)pigOrig, 1362.648, .01);

            var pigNoOrig = venda.pigVenda4Test(false, true);
            Assert.AreEqual((double)pigNoOrig, -386.89, .01);
        }

        [TestMethod]
        public void PigCompra199()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 199);

            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, 1362.648, .01);

            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 1362.648, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, -386.89, .01);

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, -386.89, .01);
        }

        #endregion ***** Producte 29 *****


        #region ***** Producte 28 *****

        [TestMethod]
        public void PigVendesProd28()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var venda = sessio.Moviments.Single(s => s.Id == 1278);

            var pigOrig = venda.pigVenda4Test(true, true);
            Assert.AreEqual((double)pigOrig, 1836.64, .01);

            var pigNoOrig = venda.pigVenda4Test(false, true);
            Assert.AreEqual((double)pigNoOrig, 421.92, .01);
        }

        [TestMethod]
        public void PigCompra1239()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 1239);

            ModificacioTemporalValoracioActual(sessio, compra.Prod, 460.150m);

            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 1362.648, .01);

            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, -1, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, 624.12, .01);

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, -386.89, .01);
        }

        [TestMethod]
        public void PigCompra217()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 217);

            ModificacioTemporalValoracioActual(sessio, compra.Prod, 460.150m);

            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, 7903.788, .001);

            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 0.135, .001);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, 624.118, .001);

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, 0, .001);
        }

        [TestMethod]
        public void PigCompra1241()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 1241);

            ModificacioTemporalValoracioActual(sessio, compra.Prod, 460.150m);

            //var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true, true);
            //Assert.AreEqual((double)pigOrigAmbCartera, 845.6485, .01);

            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 0, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, 92.19, .01);

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, 0, .01);
        }

        #endregion ***** Producte 28 *****

        private static void ModificacioTemporalValoracioActual(InversionsBDContext sessio, Producte producte, decimal preuPart)
        {
            var valoracio = sessio.Valoracio.ToList().Where(val => val.Prod == producte).OrderBy(val => val.Data).Last();
            valoracio.PreuParticipacio = preuPart;
        }
    }
}
