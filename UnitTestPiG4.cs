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
    public class UnitTestPiG4
    {
        [TestMethod]
        public void ComprovaPigVendesProd27()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var venda = sessio.Moviments.Single(s => s.Id == 1251);

            // Poso el valor actuar de la participacio a 46,53 €
            var valoracio = sessio.Valoracio.ToList().Where(val => val.Prod == venda.Prod).OrderBy(val => val.Data).Last();
            valoracio.PreuParticipacio = 46.53m;

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
        public void ComprovaPigCompra173()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var compra = sessio.Moviments.Single(s => s.Id == 173);

            // Poso el valor actuar de la participacio a 46,53 €
            var valoracio = sessio.Valoracio.ToList().Where(val => val.Prod == compra.Prod).OrderBy(val => val.Data).Last();
            valoracio.PreuParticipacio = 46.53m;


            var pigOrigAmbCartera = compra.pigCompra4Test(true, true, true, true);
            //Assert.AreEqual((double)pigOrigAmbCartera, 34994.61, .01); // Valor per PU actual = 46,53 €
            Assert.AreEqual((double)pigOrigAmbCartera, 12409.021, .01); // Valor per PU actual = 46,53 €

            var pigOrig = compra.pigCompra4Test(true, true, false, true);
            //Assert.AreEqual((double)pigOrig, 29300.972, .01);
            Assert.AreEqual((double)pigOrig, 6715.381, .01);

            var pigAmbCartera = compra.pigCompra4Test(true, false, true, true);
            Assert.AreEqual((double)pigAmbCartera, 32050.46, .01); // Valor per PU actual = 46,53 €

            var pig = compra.pigCompra4Test(true, false, false, false);
            Assert.AreEqual((double)pig, 28023.202, .01);

            var pigEncarteraCompra = pigAmbCartera - pig;
            Assert.AreEqual((double)pigEncarteraCompra, 4027.26, .01); // Valor per PU actual = 46,53 €

            var pigOrigEncarteraCompra = pigOrigAmbCartera - pigOrig;
            Assert.AreEqual((double)pigOrigEncarteraCompra, 5693.64, .01); // Valor per PU actual = 46,53 €

            var pigEncarteraProducte = compra.Prod.pigEnData4Test(DateTime.Now, true, true, null, null);
            Assert.AreEqual((double)pigEncarteraProducte, 13883.191, .01); // Valor per PU actual = 46,53 €
        }

        [TestMethod]
        public void ComprovaPigProd27()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            var prod = sessio.ProdFons.Single(s => s.Id == 27);

            // Poso el valor actuar de la participacio a 46,53 €
            var valoracio = sessio.Valoracio.ToList().Where(val => val.Prod == prod).OrderBy(val => val.Data).Last();
            valoracio.PreuParticipacio = 46.53m;

            var pigOrigAmbCartera = prod.pigActual4Test(true, true);
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
    }
}
