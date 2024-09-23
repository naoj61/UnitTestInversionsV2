using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inversions;

namespace UnitTestInversions
{
    [TestClass]
    public class UnitTestPiG4
    {
        [TestMethod]
        public void ComprovaDividendsCompra()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 151); // Dividends=0
            var div = compra.dividendsCompra4Test();
            Assert.AreEqual(div, 0); 

            compra = sessio.Moviments.Single(s => s.Id == 166); // Dividends=169,608 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 169.608, .001);

            compra = sessio.Moviments.Single(s => s.Id == 174); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 124.712, .001);

            compra = sessio.Moviments.Single(s => s.Id == 178); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 0, .001);

            compra = sessio.Moviments.Single(s => s.Id == 191); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 75.731, .001);

            compra = sessio.Moviments.Single(s => s.Id == 190); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 22.621, .001);

            compra = sessio.Moviments.Single(s => s.Id == 193); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 106.466, .001);

            compra = sessio.Moviments.Single(s => s.Id == 192); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 122.939, .001);

            compra = sessio.Moviments.Single(s => s.Id == 182); // Dividends=124,712 €
            div = compra.dividendsCompra4Test();
            Assert.AreEqual((double)div, 139.413, .001);
        }

        [TestMethod]
        public void PigTotal()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var movUsuari = sessio.Moviments.Where(mov => mov.UsuariId == Usuari.Seleccionat.Id).ToList();
            var prods = Producte.Tuples.ToList();
            prods = prods.Where(prod => prod is ProdFons).ToList();
            //prods = prods.Where(prod => prod is ProdAccions).ToList();

            movUsuari = movUsuari.Where(mov => prods.Contains(mov.Prod)).ToList();

            var compresTotes = movUsuari.Where(mov => mov._EsCompra).ToList();
            var compresReals = movUsuari.Where(mov => mov._EsCompraReal).ToList();
            var vendesReals = movUsuari.Where(mov => mov._EsVendaReal).ToList();

            //Debug.WriteLine("Prod\tId\tPiG");
            //foreach (var vendaReal in vendesReals.OrderBy(o => o.Prod))
            //{
            //    Debug.WriteLine("{0}\t{1}\t{2}", vendaReal.Prod, vendaReal.Id, vendaReal.pigVenda4Test(true, false, false).ToString("#,###.000"));
            //}
            //Debug.WriteLine("{0}\t{1}\t{2}", "Cartera", "", prods.Sum(producte => producte.pigEnCartera4Test(true, true)).ToString("#,###.000"));

            //Debug.WriteLine("Prod\tId\tPig Orig\tPiG");
            //foreach (Moviment compra in compresTotes)
            //{
            //    var aa = compra.pigCompra4Test(false, true, true);
            //    var bb = compra.pigCompra4Test(false, false, true);
            //    Debug.WriteLine("{3}\t{0}\t{1}\t{2}", compra.Id, aa, bb, compra.Prod);
            //}

            var totalCompres = compresReals.Sum(compra => compra.Participacions * compra.PreuParticipacio);
            var totalVendes = vendesReals.Sum(venda => venda.Participacions * venda.PreuParticipacio);          
            // No conto despeses per que alguns fons en tenen i és un merder.
            var totalDespeses = 0; // movUsuari.Sum(mov => mov.Despeses.GetValueOrDefault());
            var totalEnCartera = prods.Sum(prod=>prod.partsEnCarteraTest() * prod._PreuParticipacioActual);

            var pigTotal = totalEnCartera + totalVendes - totalCompres - totalDespeses;

            var pigVendesReals = vendesReals.Sum(venda => venda.pigVenda4Test(true, false, false));
            var pigEnCartera = prods.Sum(producte => producte.pigEnCartera4Test(true, false));
            
            var pigVendesCart = pigVendesReals + pigEnCartera;

            Assert.AreEqual((double)pigTotal, (double)pigVendesCart, .03);

            decimal pigCompres = compresTotes.Sum(compra => compra.pigCompra4Test(false, false, true));
            decimal pigCompresOrig = compresTotes.Sum(compra => compra.pigCompra4Test(false, true, true));

            var pigTotalX = pigTotal.ToString("#,###.000");
            var pigVendesCartX = pigVendesCart.ToString("#,###.000");
            var pigCompresX = pigCompres.ToString("#,###.000");
            var pigCompresOrigX = pigCompresOrig.ToString("#,###.000");
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
            Assert.AreEqual((double)pigNoOrig, 0, .01);

            pigNoOrigAmbDespeses = prod.pigEnCartera4Test(false, true);
            Assert.AreEqual((double)pigNoOrigAmbDespeses, 0, .01);
        }


        [TestMethod]
        public void PigProd13()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var prod = sessio.ProdFons.Single(s => s.Id == 13);

            var compra = prod.MovimentsProducteUsuari.Single(s => s.Id == 34);
            var venda = prod.MovimentsProducteUsuari.Single(s => s.Id == 42);

            var pigOrig = compra.pigCompra4Test(false, true, false);
            Assert.AreEqual((double)pigOrig, -283.135, .01);

            var pig = compra.pigCompra4Test(false, false, false);
            Assert.AreEqual((double)pig, -659.436, .01);

            var pigVendaOrig = venda.pigVenda4Test(true, false, false);
            Assert.AreEqual((double)pigVendaOrig, -283.135, .01);

            var pigVenda = venda.pigVenda4Test(false, false, false);
            Assert.AreEqual((double)pigVenda, -659.436, .01);
        }

        [TestMethod]
        public void PigProd16()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 101);
            
            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 434.492, .01);

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, 19653.451, .01); 
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

            pigNoOrig = venda.pigVenda4Test(false, true);
            Assert.AreEqual((double)pigNoOrig, 5086.172, .01);


            venda = sessio.Moviments.Single(s => s.Id == 1279);

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
            ModificacioTemporalValoracioActual(sessio, prod, 46.53m);

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
            Assert.AreEqual((double)pigOrig, 1134.474, .01);

            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, 1134.606, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, 495.868, .01);

            var pig = compra.pigCompra4Test(true, false, false);
            Assert.AreEqual((double)pig, 0, .01);
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

            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true);
            Assert.AreEqual((double)pigOrigAmbCartera, 845.651, .01);

            var pigOrig = compra.pigCompra4Test(true, true, false);
            Assert.AreEqual((double)pigOrig, 0, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true);
            Assert.AreEqual((double)pigAmbCartera, 150.905, .01);

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
