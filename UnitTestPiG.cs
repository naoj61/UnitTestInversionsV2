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
        /// Vull veure que fan el mogollon de mètodes pig de Productes.
        /// </summary>
        [TestMethod]
        public void TestComparaPig()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

            double tPig = 0;
            double tPig2 = 0;

            Console.WriteLine("Producte\tAny\tPiG\tPiG2");
            foreach (var prod in sessio.Productes.ToList())
            {
                if (prod.Id != 7)
                    continue;

                bool imprimeixTotal = false;
                for (int any = 2010; any < 2021; any++)
                {
                    var pig = prod.pig2TotalTest(any);
                    var pig2 = prod.pig2TotalTest(new DateTime(any, 1, 1), new DateTime(any, 12, 31), any == DateTime.Today.Year);
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
            double pig2Total = 0;
            double pig2Producte = 0;
            double pig2EnCartera = 0;

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

            Assert.IsTrue(Utilitats.SonIguals(pig2Total, 60489.18, 2));
            Assert.IsTrue(Utilitats.SonIguals(pig2Producte, 60486.24, 2));
            Assert.IsTrue(Utilitats.SonIguals(pig2EnCartera, 32204.63, 2));
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
                    var preuCost = producte.costOriginalEnCartera2Test();
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

            double piGActual = 0;

            double importTotalCompresReals = sessio.MovimentsUsuari.Where(w => w._EsCompraReal).Sum(compra => compra.ImportNet);

            double importTotalVendesReals = sessio.MovimentsUsuari.Where(w => w._EsVendaReal).Sum(venda => venda.ImportNet);

            double importTotalDividends = sessio.MovimentsUsuari.Where(w => w._EsDividents).Sum(divident => divident.ImportNet);

            double pigtotal2 = 0;
            double importTotalCarteraActual = 0;
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


        /// <summary>
        /// Comprova el càlcul del preu origen en els traspassos de fons.
        /// </summary>
        [TestMethod]
        public void ComprovaPigDunaVenda()
        {
            InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);
            
            // Biotec Id= 6. PU compra: 274,68 €	Import compra: 6.132,72 €
            var prod = sessio.Productes.Single(w => w.Id == 6);
            var part = prod._Participacions;
            var compresAmbPart = prod.compresAnteriors(DateTime.Now, part); // dws._Participacions - 10);

            double preuCompra = 0;
            foreach (var movimentCompra in compresAmbPart)
            {
                preuCompra += movimentCompra._ParticipacionsDisponibles * movimentCompra._PreuParticipacioOrigenTest;
            }


            double preuCompra2 = prod.costOriginalEnCartera2Test();
            //foreach (var movimentCompra in compresAmbPart)
            //{
            //    var mov = movimentCompra._Moviment;
            //    var aa = mov.DesglosCompres;
            //    foreach (var compOrig in mov.DesglosCompres)
            //    {
            //        preuCompra2 += compOrig._PreuPartOrig * compOrig.ParticipacionsOrig;
            //    }
            //}


            var pigAct = prod.pig2EnCarteraTest();


            var arcelor = sessio.Productes.Single(w => w.Id == 7);
            var venda2019 = sessio.Moviments.Single(w => w.Id == 177);

            var pig2019 = (venda2019.PreuParticipacio - venda2019._PreuCompraParticipacioOrigen) * venda2019.Participacions;

            //var pigTr = venda2019.Prod.pig(DateTime.MinValue, venda2019.Data, true);
            //var pigTr2 = venda2019.Prod.pig(DateTime.MinValue, venda2019.Data, false);

            var pigTrw = 0;


            Debug.WriteLine("\nFinal");
        }


        #endregion *** Test ***

    }
}
