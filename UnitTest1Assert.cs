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
        /// Comprova que les compres reals de fons, coincideixen amb la cartera actual més les vendes reals
        /// </summary>
        //[TestMethod]
        //public void ComprovaCompresOrig()
        //{
        //    InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

        //    List<Moviment> compresRealsAmbCartera = new List<Moviment>();
        //    List<ProdFons> fonsAmbCartera = new List<ProdFons>();
        //    foreach (var prod in sessio.ProdFons)
        //    {
        //        if (prod._Participacions <= 0)
        //            continue;

        //        fonsAmbCartera.Add(prod);

        //        foreach (var compraAmbCartera in prod.compresDePartipacionsTest(DateTime.Now))
        //        {
        //            foreach (var desglosCompra in compraAmbCartera.DesglosCompres)
        //            {
        //                if (!compresRealsAmbCartera.Contains(desglosCompra.MovCompraOrig))
        //                    compresRealsAmbCartera.Add(desglosCompra.MovCompraOrig);
        //            }
        //        }
        //    }

        //    foreach (var compraReal in compresRealsAmbCartera)
        //    {
        //        var id = compraReal.Id;

        //        double partsComprades = sessio.Moviments.Single(w => w.Id == id).Participacions;

        //        double partsVenudes = 0;
        //        foreach (var venda in sessio.MovimentsUsuari.ToList().Where(w => w.Prod is ProdFons && w._EsVendaReal))
        //        {
        //            var compresDeLaVenda = venda.compresDeLaVenda4Test();
        //            foreach (var compra in compresDeLaVenda)
        //            {
        //                foreach (var desglosCompra in compra.DesglosCompres)
        //                {
        //                    if (desglosCompra.MovCompraOrigId == id)
        //                        if (venda._EsVendaReal)
        //                            partsVenudes += desglosCompra._ParticipacionsUtilitzadesOrig;
        //                }
        //            }
        //        }

        //        double partsEnCartera = 0;

        //        foreach (var prod in fonsAmbCartera)
        //        {
        //            if (prod._ValorActualEnCartera > 0)
        //            {
        //                foreach (var compra in prod.compresDePartipacionsTest(DateTime.Now))
        //                {
        //                    foreach (var desglosCompra in compra.DesglosCompres)
        //                    {
        //                        if (desglosCompra.MovCompraOrigId == id)
        //                            partsEnCartera += desglosCompra._ParticipacionsUtilitzadesOrig;
        //                    }
        //                }
        //            }
        //        }

        //        if (Utilitats.ComparaNumeros(partsComprades, partsEnCartera + partsVenudes) != 0)
        //        {
        //            var dif = partsComprades - partsVenudes - partsEnCartera;
        //            Debug.WriteLine("\nNo cuadra. Prod: {0}. Comprades: {1}. Venudes: {2}. En cartera: {3}. Dif: {4}. Preu cost: {5}."
        //                , compraReal.Prod, partsComprades, partsVenudes, partsEnCartera, dif, dif * compraReal.PreuParticipacio);
        //        }

        //        Assert.AreEqual(partsComprades, partsEnCartera + partsVenudes, 2
        //            , "No cuadra. Prod: {0}. Comprades: {1}. Venudes: {2}. En cartera: {3}"
        //            , compraReal.Prod, partsComprades, partsVenudes, partsEnCartera);
        //    }
        //}


        /// <summary>
        /// Valida que 
        /// Si son accions les participacions a Moviments siguin les mateixes que les de DesglosCompres.
        /// Si son fons, que les partipipacions orig en cartera més les vendes reals corresponguin a les compres originals
        /// </summary>
        //[TestMethod]
        //public void comprovaVendesDeLaCompra()
        //{
        //    InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

        //    // *** Valida accions ***

        //    Debug.WriteLine("\nAccions");
        //    Debug.WriteLine("Prod\tParts Mov\tParts Desg\tDif");

        //    // Compto les participacions de les compres reals de totes les accions.
        //    var prodsAccionsAmbPartsEncartera = sessio.ProdAccions.ToList().Where(w => w._Participacions > 0);
        //    foreach (var prod in prodsAccionsAmbPartsEncartera)
        //    {
        //        var partsMov = prod.MovimentsProducteUsuari.Where(w => w._EsCompra).Sum(s => s.Participacions);
        //        var partsDesg = prod.MovimentsProducteUsuari.Sum(s => s.DesglosCompres.Sum(s2 => s2.Participacions));
        //        var dif = Math.Round(partsMov - partsDesg, 3);

        //        if (!Utilitats.EsZero(dif))
        //            Debug.WriteLine("{0}\t{1}\t{2}\t{3}", prod
        //                , partsMov.ToString(CultureInfo.CurrentCulture)
        //                , partsDesg.ToString(CultureInfo.CurrentCulture)
        //                , dif.ToString(CultureInfo.CurrentCulture));

        //        Assert.IsTrue(Math.Abs(dif) < 5, "\nProd: {0}.", prod);
        //        Assert.AreEqual(partsMov, partsDesg, 5, "\nProd: {0}.", prod);
        //    }


        //    // *** Valida fons ***

        //    Dictionary<Moviment, double> compresOrigAmbPartsEnCartera = new Dictionary<Moviment, double>();

        //    // Compto les participacions de vemde reals del tots els fons.
        //    var vendesReals = sessio.MovimentsUsuari.Where(w => w.Prod is ProdFons && w._EsVendaReal).ToList();
        //    foreach (var venda in vendesReals)
        //    {
        //        var compresDeLaVenda = venda.compresDeLaVenda4Test();
        //        foreach (var compra in compresDeLaVenda)
        //        {
        //            foreach (var desglosCompra in compra.DesglosCompres)
        //            {
        //                if (compresOrigAmbPartsEnCartera.ContainsKey(desglosCompra.MovCompraOrig))
        //                    compresOrigAmbPartsEnCartera[desglosCompra.MovCompraOrig] += desglosCompra._ParticipacionsUtilitzadesOrig;
        //                else
        //                    compresOrigAmbPartsEnCartera[desglosCompra.MovCompraOrig] = desglosCompra._ParticipacionsUtilitzadesOrig;
        //            }
        //        }
        //    }

        //    // Compto les participacions de les compres reals de tots els fons.
        //    var prodsFonsAmbPartsEncartera = sessio.ProdFons.ToList().Where(w => w._Participacions > 0);
        //    foreach (var prod in prodsFonsAmbPartsEncartera)
        //    {
        //        var compresDeLaVenda = prod.compresDePartipacionsTest(DateTime.Now, prod._Participacions);
        //        foreach (var compra in compresDeLaVenda)
        //        {
        //            foreach (var desglosCompra in compra.DesglosCompres)
        //            {
        //                if (compresOrigAmbPartsEnCartera.ContainsKey(desglosCompra.MovCompraOrig))
        //                    compresOrigAmbPartsEnCartera[desglosCompra.MovCompraOrig] += desglosCompra._ParticipacionsUtilitzadesOrig;
        //                else
        //                    compresOrigAmbPartsEnCartera[desglosCompra.MovCompraOrig] = desglosCompra._ParticipacionsUtilitzadesOrig;
        //            }
        //        }
        //    }

        //    Debug.WriteLine("\nFons");
        //    Debug.WriteLine("Prod\tId Orig\tParts Orig\tAcumulat Traspas\tDif");
        //    foreach (var compraOrig in compresOrigAmbPartsEnCartera.OrderBy(o => o.Key.Id))
        //    {
        //        var compra = compraOrig.Key;

        //        var partsOrig = Math.Round(compra.Participacions, 4);
        //        var parts = Math.Round(compraOrig.Value, 4);
        //        var dif = Math.Round(partsOrig - parts, 4);

        //        if (!Utilitats.EsZero(dif))
        //            Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", compra.Prod, compra.Id
        //                , partsOrig.ToString(CultureInfo.CurrentCulture)
        //                , parts.ToString(CultureInfo.CurrentCulture)
        //                , dif.ToString(CultureInfo.CurrentCulture));

        //        Assert.IsTrue(Math.Abs(dif) < 2, "\nProd: {0}.", "\nProd: {0}. Id Mov: {1}.", compra.Prod, compra.Id);
        //        //Assert.AreEqual(partsOrig, parts, .01, "\nProd: {0}. Id Mov: {1}.", compra.Prod, compra.Id);
        //    }

        //    Debug.WriteLine("\n*** Fi Ok ***");
        //}



        /// <summary>
        /// Suma de compres reals ha de ser igual a la suma de les vendes reals mes les participacions en cartera.
        /// </summary>
        //[TestMethod]
        //public void ComprovaPartsOrig()
        //{
           
        //    // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
        //    InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

        //    var totalPartsCompresReals = sessio.MovimentsUsuari.Where(w => w._EsCompraReal).Sum(s => s.Participacions);
        //    var totalPartsVendesReals = sessio.MovimentsUsuari.Where(w => w._EsVendaReal).Sum(s => s.Participacions);
        //    double totalPartsEnCartera = 0;

        //    foreach (var producte in sessio.Productes)
        //    {
        //        if(producte._Participacions == 0)
        //            continue;
                
        //        var compresAnt = producte.compresDePartipacionsTest(DateTime.Now, producte._Participacions);

        //        foreach (var compra in compresAnt)
        //        {
        //            foreach (var desglosCompra in compra.DesglosCompres)
        //            {
        //                totalPartsEnCartera += desglosCompra._ParticipacionsUtilitzadesOrig;
        //            }
        //        }
        //    }

        //    var total = totalPartsCompresReals - totalPartsVendesReals - totalPartsEnCartera;

        //    Debug.WriteLine(String.Format("Total parts compres reals:\t{0}", totalPartsCompresReals.ToString("####.000", CultureInfo.CurrentCulture)));
        //    Debug.WriteLine(String.Format("Total parts vendes reals:\t{0}", totalPartsVendesReals.ToString("####.000", CultureInfo.CurrentCulture)));
        //    Debug.WriteLine(String.Format("Total parts en cartera:\t{0}", totalPartsEnCartera.ToString("####.000", CultureInfo.CurrentCulture)));
        //    Debug.WriteLine(String.Format("Total parts:\t{0}", total.ToString("####.000", CultureInfo.CurrentCulture)));

        //}

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
            decimal costOrigEnCart;
            decimal importSelf;
            decimal total = 0;

            Debug.WriteLine("\nProd:\tSelf:\tAquí:\tDif:");

            importSelf = 777.35M;
            prod = sessio.Productes.Single(s => s.Id == 5);
            costOrigEnCart = prod.costOriginalEnCartera3Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000",CultureInfo.CurrentCulture), prod);

            importSelf = 6132.72M;
            prod = sessio.Productes.Single(s => s.Id == 6);
            costOrigEnCart = prod.costOriginalEnCartera3Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 17922.04M;
            prod = sessio.Productes.Single(s => s.Id == 16);
            costOrigEnCart = prod.costOriginalEnCartera3Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 1588.63M;
            prod = sessio.Productes.Single(s => s.Id == 19);
            costOrigEnCart = prod.costOriginalEnCartera3Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 27338.26M;
            prod = sessio.Productes.Single(s => s.Id == 27);
            costOrigEnCart = prod.costOriginalEnCartera3Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 26084.35M;
            prod = sessio.Productes.Single(s => s.Id == 7);
            costOrigEnCart = prod.costOriginalEnCartera3Test();
            total += costOrigEnCart;
            //Assert.AreEqual(importSelf, costOrigEnCart, .0001);
            Debug.WriteLine("{3}\t{0}\t{1}\t{2}", importSelf.ToString("####.000", CultureInfo.CurrentCulture), costOrigEnCart.ToString("####.000", CultureInfo.CurrentCulture), (importSelf - costOrigEnCart).ToString("####.000", CultureInfo.CurrentCulture), prod);

            importSelf = 79843.35M;
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
            var sumDesgloç = sessio.DesglosCompres.Sum(s => s.Participacions);

            Assert.AreEqual((double) sumCompres, (double) sumDesgloç, .0001);

            Debug.WriteLine("\nFinal");
        }

        /// <summary>
        /// Comprova que ProvacompresDeLaVenda reparteix correctament.
        /// </summary>
        //[TestMethod]
        //public void ProvaCompresDeLaVenda()
        //{
        //    // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
        //    InversionsBDContext sessio = UnitTest1.ConnectaBd();

        //    int cont = 0;
        //    foreach (var usuari in sessio.Usuaris.ToList())
        //    {
        //        Usuari.Seleccionat = usuari;

        //        var vendes = sessio.MovimentsUsuari.Where(w => w.TipusMoviment == TipusMoviment.Venda).ToList();

        //        foreach (var venda in vendes)
        //        {
        //            List<DesglosCompra> xxx = new List<DesglosCompra>();
        //            List<Moviment> compres = venda.compresDeLaVenda4Test().ToList();
        //            foreach (var compra in compres)
        //            {
        //                xxx.AddRange(compra.DesglosCompres.ToList());
        //            }

        //            var sumaDesgolç = xxx.Sum(s => s._ParticipacionsUtilitzades);

        //            Assert.AreEqual(sumaDesgolç, venda.Participacions, .0001, "Prod:{0}. VendaId:{1}", venda.Prod, venda.Id);
        //            cont++;
        //        }
        //    }
        //    Debug.WriteLine("\nTotal vendes:{0}", cont);
        //    Debug.WriteLine("\nFinal");
        //}


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
                if (!Utilitats.SonIguals(compra.Participacions, sumPartsDesg))
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
        //[TestMethod]
        //public void TestPreuOriginal()
        //{
        //    InversionsBDContext sessio = UnitTest1.ConnectaBd(Usuari.Usuaris.Joan);

        //    Debug.WriteLine("********** Inici **********");

        //    int oks = 0, kos = 0;

        //    foreach (var compra in sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
        //    {
        //        var costTotalOrig = compra.DesglosCompres.Sum(s => s.ParticipacionsOrig * s._PreuParticipacioOrig);
        //        var preuUnitOrig = Math.Round(costTotalOrig / compra.Participacions, 4);
        //        var preuOrigAnt = Math.Round(compra.calculaImportCompraOrigen3(false, false) / compra.Participacions, 4);
        //        var dif = Math.Round(preuUnitOrig - preuOrigAnt, 2);

        //        if (dif > 0)
        //        {
        //            Debug.WriteLine("MovId = {0}\tDif = {3}\tpreuUnitOrig = {1}\tpreuOrigAnt = {2}"
        //                , compra.Id, preuUnitOrig.ToString("#,##0.00€"), preuOrigAnt.ToString("#,##0.00€"), dif.ToString("#,##0.00€"));
        //            kos++;
        //        }
        //        else
        //        {
        //            oks++;
        //        }
        //    }

        //    Debug.WriteLine("\nOks={0}. Kos={1}", oks, kos);

        //    Assert.IsTrue(kos == 0, "\nHi ha compres que no quadren");
        //    Debug.WriteLine("Final");
        //}

        #endregion *** Test ***

    }
}
