using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.Core;
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
    public class UnitTestPiG
    {

        #region *** Test ***


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void PigDeUnProducteAny()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            decimal pig;
            decimal pigTot = 0;
            Producte prod;
            const int any = 2019;

            Debug.WriteLine("\nPiG any: {0}\n", any);

            // Id: 7-Arcelor
            prod = sessio.Productes.Single(w => w.Id == 7);
            pig = Math.Round(prod.pigEnAny4Test(any, false, false), 3);
            pigTot += pig;
            Debug.WriteLine("Prod: {0}\tPiG:\t{1}", prod, pig.ToString(CultureInfo.CurrentCulture));

            // Id: 15-Templeton Emerging Mkts Sm Cos N Acc $
            prod = sessio.Productes.Single(w => w.Id == 15);
            pig = Math.Round(prod.pigEnAny4Test(any, false, false), 3);
            pigTot += pig;
            Debug.WriteLine("Prod: {0}\tPiG:\t{1}", prod, pig.ToString(CultureInfo.CurrentCulture));

            // Id: 3-Asian Smaller Companies Fund
            prod = sessio.Productes.Single(w => w.Id == 3);
            pig = Math.Round(prod.pigEnAny4Test(any, false, false), 3);
            pigTot += pig;
            Debug.WriteLine("Prod: {0}\tPiG:\t{1}", prod, pig.ToString(CultureInfo.CurrentCulture));

            // Id: 4-Thailand A-USD
            prod = sessio.Productes.Single(w => w.Id == 4);
            pig = Math.Round(prod.pigEnAny4Test(any, false, false), 3);
            pigTot += pig;
            Debug.WriteLine("Prod: {0}\tPiG:\t{1}", prod, pig.ToString(CultureInfo.CurrentCulture));

            Debug.WriteLine("PiG Total:\t{0}{1}", "",pigTot.ToString(CultureInfo.CurrentCulture));
        }


        /// <summary>
        /// Vull veure que fan el mogollon de mètodes pig de Productes.
        /// </summary>
        [TestMethod]
        public void TestComparaPig()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            decimal tPig = 0;
            decimal tPig2 = 0;

            Console.WriteLine("Producte\tAny\tPiG\tPiG2");
            foreach (var prod in sessio.Productes.ToList())
            {
                if (prod.Id != 7)
                    continue;

                bool imprimeixTotal = false;
                for (int any = 2010; any < 2021; any++)
                {
                    var pig = 0; // prod.pig3TotalTest((uint)any, true, false);
                    var pig2 = prod.pig2TotalTest(new DateTime(any, 1, 1), new DateTime(any, 12, 31), any == DateTime.Today.Year, false);
                    tPig += pig;
                    tPig2 += pig2;
                    if (!Utilitats.EsZero(pig) || !Utilitats.EsZero(pig2))
                    {
                        Console.WriteLine("{0}-{1}\t{2}\t{3}\t{4}", prod.Id, prod._NomProducte, any,
                            pig.ToString("#,##0.00 €"),
                            pig2.ToString("#,##0.00 €"));
                        
                        imprimeixTotal = true;
                    }
                }
                
                var xxx = prod.pig2ProducteTest();

                if (imprimeixTotal)
                {
                    Console.WriteLine("Total\t\t{0}\t{1}", tPig.ToString("#,##0.00 €"), tPig2.ToString("#,##0.00 €"));
                    Console.WriteLine("");
                }

                tPig = 0;
                tPig2 = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestPigOrigenCartera()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            Console.WriteLine("Producte\tPiG Total\tPiG Producte\tPiG En cartera");

            var data = new DateTime(2020, 07, 11).AddMilliseconds(-1);
            decimal pig2Total = 0;
            decimal pig2Producte = 0;
            decimal pig2EnCartera = 0;

            foreach (var prod in sessio.Productes.ToList())
            {
                //if (prod.Id != 9)
                //    continue;

                if (!prod.MovimentsProducteUsuari.Any())
                    continue;

                var pTot = prod.pig2TotalTest(dataHoraFinal: data);
                var pProd = prod.pig2ProducteTest(dataHoraFinal: data);
                var pCar = prod.pig2EnCarteraTest(dataHoraFinal: data);

                pig2Total += pTot;
                pig2Producte += pProd;
                pig2EnCartera += pCar;

                Console.WriteLine("{0}-{1}\t{2}\t{3}\t{4}",
                    prod.Id, prod._NomProducte,
                    pTot.ToString("#,##0.00 €"),
                    pProd.ToString("#,##0.00 €"),
                    pCar.ToString("#,##0.00 €"));
            }

            Console.WriteLine("\nPiG Total:\t{0}\t{1}\t{2}",
                pig2Total.ToString("#,##0.00 €"),
                pig2Producte.ToString("#,##0.00 €"),
                pig2EnCartera.ToString("#,##0.00 €"));

            //Assert.IsTrue(Utilitats.SonIguals(pig2Total, 60489.18, 2));
            //Assert.IsTrue(Utilitats.SonIguals(pig2Producte, 60486.24, 2));
            //Assert.IsTrue(Utilitats.SonIguals(pig2EnCartera, 32204.63, 2));

            Assert.IsTrue(pig2Total == 60489.18M);
            Assert.IsTrue(pig2Producte == 60486.24M);
            Assert.IsTrue(pig2EnCartera == 32204.63M);
        }


        /// <summary>
        /// Comprova el càlcul del preu origen en els traspassos de fons.
        /// </summary>
        [TestMethod]
        public void ComprovaDiferentsMetodesPig()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            Console.WriteLine("Fons\ttPreu Desgloç<\tValor actual\tPiG");

            foreach (var producte in sessio.Productes.Where(p=>p is ProdFons))
            {
                if (producte.numParticipacionsEnDataTest(DateTime.Now) > 0)
                //if (producte.Id == 27)
                {
                    var preuCost = producte.costOriginalEnCartera3Test();
                    var valorAct = producte.valorEnCartera();
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}",
                        producte._NomProducte,
                        preuCost.ToString("#,##0.00€"),
                        valorAct.ToString("#,##0.00€"),
                        (valorAct-preuCost).ToString("#,##0.00€"));
                }
            }

            Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Comprova el càlcul del preu origen en els traspassos de fons.
        /// </summary>
        [TestMethod]
        public void CalculaPiGDeTot()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            decimal piGActual = 0;

            var importTotalCompresReals = Moviment.MovimentsUsuari.Where(w => w._EsCompraReal).Sum(compra => compra._ImportNet);

            var importTotalVendesReals = Moviment.MovimentsUsuari.Where(w => w._EsVendaReal).Sum(venda => venda._ImportNet);

            var importTotalDividends = Moviment.MovimentsUsuari.Where(w => w._EsDividents).Sum(divident => divident._ImportNet);

            decimal pigtotal2 = 0;
            decimal importTotalCarteraActual = 0;
            foreach (var prod in sessio.Productes)
            {
                if (prod._Participacions > 0)
                {
                    Debug.WriteLine("Producte: {0}. Valor actual en cartera: {1}", prod._NomProducte, prod._ValorActualEnCartera.ToString("C2"));
                    importTotalCarteraActual += prod._ValorActualEnCartera;
                }
                pigtotal2 += prod.pig2TotalTest();
            }

            piGActual = importTotalVendesReals + importTotalCarteraActual + importTotalDividends - importTotalCompresReals;

            Debug.WriteLine(String.Format("ImportTotalCompresReals: {0}", importTotalCompresReals.ToString("C2")));
            Debug.WriteLine(String.Format("ImportTotalVendesReals: {0}", importTotalVendesReals.ToString("C2")));
            Debug.WriteLine(String.Format("ImportTotalCarteraActual: {0}", importTotalCarteraActual.ToString("C2")));
            Debug.WriteLine(String.Format("PiG total: {0}", piGActual.ToString("C2")));
            Debug.WriteLine(String.Format("PiG total 2: {0}", pigtotal2.ToString("C2")));
        }
        
        #endregion *** Test ***

    }
}
