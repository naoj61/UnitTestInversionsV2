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
    public class UnitTest1
    {
        #region *** Modifiquen dades ***

        #region *** Executar un cop sobre la BD de la versió: 1.9.5.3 ***

        //[TestMethod]
        public void ModificaEstruturaTaulaMoviments()
        {
            var sessio = new InversionsBDContext();

            sessio.Database.ExecuteSqlCommand("EXEC sp_RENAME 'DesglosCompres.RefCompraId' , 'MovCompraId', 'COLUMN'");
            sessio.Database.ExecuteSqlCommand("EXEC sp_RENAME 'DesglosCompres.RefCompraOrigId' , 'MovCompraOrigId', 'COLUMN'");
            
            sessio.Database.ExecuteSqlCommand("ALTER TABLE [Moviments] DROP CONSTRAINT [FK_ProducteMoviment]");
            sessio.Database.ExecuteSqlCommand("DROP INDEX [Moviments].[IX_FK_ProducteMoviment]");
            sessio.Database.ExecuteSqlCommand("ALTER TABLE [Moviments] DROP COLUMN [ProducteTraspasId]");
        }

        /// <summary>
        /// En els trapassos, informa RefTraspasId(MovimentRefTraspasId) de les vendes.
        /// </summary>
        //[TestMethod]
        public void CreaMovimentRefVendaIdEnVendesTraspassades()
        {
            int cont = 0;
            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    System.Diagnostics.Debug.WriteLine("********** Inici **********");

                    foreach (var movCompraTraspas in conn.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra && w.RefTraspasId != null))
                    {
                        var movVendaTraspas = conn.Moviments.Single(w => w.Id == movCompraTraspas.RefTraspasId);
                        movVendaTraspas.RefTraspasId = movCompraTraspas.Id;
                        conn.SaveChanges();
                        cont++;
                    }

                    dbContextTransaction.Commit();
                }
            }

            System.Diagnostics.Debug.WriteLine("\nFinal. Files modificades: {0}", cont);
        }

        /// <summary>
        /// Esborra la taula "DesglosCompres" i la crea de nou.
        /// </summary>
        [TestMethod]
        public void GeneraDesgloçCompres()
        {
            return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar la taula.

            var sessio = new InversionsBDContext();

            sessio.Database.ExecuteSqlCommand("TRUNCATE TABLE [DesglosCompres]");

            using (var conn = new InversionsBDContext())
            {
                Debug.WriteLine("********** Inici **********");

                foreach (var usu in sessio.Usuaris)
                {
                    // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
                    Usuari.Seleccionat = usu;

                    using (var dbContextTransaction = conn.Database.BeginTransaction())
                    {
                        Debug.WriteLine("\nCompres reals");
                        
                        var movsUsuari = conn.Moviments.Where(w => w.UsuariId == usu.Id).ToList();

                        var compresReals = movsUsuari.Where(w => w._EsCompraReal).OrderBy(o => o.Data);
                        foreach (var co in compresReals)
                        {
                            Debug.WriteLine("co.Id = {0}", co.Id);

                            co.desgloçarCompra(conn, co.RefTraspas);
                        }

                        Debug.WriteLine("\nCompres traspassos");

                        var compresTrasp = movsUsuari.Where(w => w._EsCompra && w._EsTraspas).OrderBy(o => o.Data);
                        foreach (var co in compresTrasp)
                        {
                            Debug.WriteLine("co.Id = {0}", co.Id);

                            co.desgloçarCompra(conn, co.RefTraspas);
                        }

                         //conn.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }

        #endregion *** Executar un cop sobre la BD de la versió: 1.9.5.3 ***

        /// <summary>
        /// Elimina camp PreuParticipacioOrigen de la taula Moviments
        /// </summary>
        [TestMethod]
        public void ModificaEstruturaTaulaMoviments2()
        {
            var sessio = new InversionsBDContext();
            try
            {
                sessio.Database.ExecuteSqlCommand("ALTER TABLE [Moviments] DROP COLUMN [PreuParticipacioOrigen]");
                Debug.WriteLine("Alter Table Fet!!!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Alter Table Error: {0}", ex.Message));
            }
            try
            {
                sessio.Database.ExecuteSqlCommand("EXEC sp_RENAME 'Moviments.MovimentRefVendaId' , 'RefTraspasId', 'COLUMN'");
                Debug.WriteLine("Rename Table Fet!!!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Rename Error: {0}", ex.Message));
            }
        }


        /// <summary>
        /// Esborra la taula "DesglosCompres" i la crea de nou.
        /// </summary>
        [TestMethod]
        public void GeneraDesgloçCompresMovId26()
        {
            return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

            using (var conn = new InversionsBDContext())
            {
                Debug.WriteLine("********** Inici **********");
                // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
                Usuari.Seleccionat = conn.Usuaris.Single(s => s.Id == (int) Usuari.Usuaris.Joan);

                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    var filesDesglosCompres = conn.Database.ExecuteSqlCommand("DELETE from [DesglosCompres] where [MovCompraId] >= 92");

                    var compra = conn.Moviments.Single(s => s.Id == 92);
                    compra.desgloçarCompra(conn, compra.RefTraspas);

                    dbContextTransaction.Commit();
                }
            }

            Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Fa un traspàs de fons.
        /// </summary>
        //[TestMethod]
        public void TraspasFons()
        {
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

           //InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            const int idDel = 1208;

            Producte prodVenda;
            Producte prodCompra;
            DateTime dataVenda = DateTime.Now;
            double participacionsVenda;
            double participacionsCompra;
            double preuParticipacioVenda;
            const string descripcio = null;

            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    var filesDesglosCompres = conn.Database.ExecuteSqlCommand("DELETE from [DesglosCompres] where [MovCompraId] >= " + idDel);
                    var filesMoviments = conn.Database.ExecuteSqlCommand("DELETE from [Moviments] where[Id] >= " + idDel);
                    //dbContextTransaction.Commit();

                    Debug.WriteLine("********** Inici **********");

                    Usuari.Seleccionat = conn.Usuaris.Single(s => s.Id == (int)Usuari.Usuaris.Joan);


                    // De Global a Optimal
                    prodVenda = conn.ProdFons.Single(w => w.Id == 27);
                    prodCompra = conn.ProdFons.Single(w => w.Id == 1);

                    dataVenda = DateTime.Now;
                    preuParticipacioVenda = 1800;
                    participacionsVenda = prodVenda._Participacions;
                    participacionsCompra = 15000;
                    prodVenda.desaTraspasTest(conn, dataVenda, participacionsVenda, preuParticipacioVenda, descripcio, dataVenda.AddSeconds(1)
                        , prodCompra, participacionsCompra);


                    //// De Atkien a Global
                    //prodVenda = conn.ProdFons.Single(w => w.Id == 16);
                    //prodCompra = conn.ProdFons.Single(w => w.Id == 27);
                    

                    //dataVenda = DateTime.Now.AddDays(-1);
                    //preuParticipacioVenda = 375.5500;
                    //participacionsVenda = 80.7528;
                    //participacionsCompra = 1778.6900;
                    //prodVenda.desaTraspas(conn, dataVenda, participacionsVenda, preuParticipacioVenda, descripcio, dataVenda.AddSeconds(1)
                    //    , prodCompra, participacionsCompra);


                    //dataVenda = DateTime.Now;
                    //preuParticipacioVenda = 380;
                    //participacionsVenda = 80;
                    //participacionsCompra = 1770;
                    //prodVenda.desaTraspas(conn, dataVenda, participacionsVenda, preuParticipacioVenda, descripcio, dataVenda.AddSeconds(1)
                    //    , prodCompra, participacionsCompra);


                    dbContextTransaction.Commit();
                }
            }

            Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// 
        /// </summary>
        //[TestMethod]
        public void ProvaTraspasMesCompra()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            var prodOrig = sessio.Productes.Single(w => w.Id == 6);
            var prodDest = sessio.Productes.Single(w => w.Id == 1);
            var ccc1 = prodDest.MovimentsProducteUsuari;
            //var ccc = prodDest.Moviments.Where(w => w.UsuariId == Usuari.Seleccionat.Id);

            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    var data = DateTime.Now.AddMinutes(-10);
                    prodOrig.desaTraspasTest(conn, data, 20, 600, String.Empty, data, prodDest, 20);

                    //conn.SaveChanges();
                    dbContextTransaction.Commit();
                }

                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    var prod = conn.Productes.Single(w => w.Id == 7);
                    var data = DateTime.Now;
                    prod.desaCompra(conn, data, data.TimeOfDay, 100, 22, 1, 0, String.Empty);

                    //conn.SaveChanges();
                    dbContextTransaction.Commit();
                }
            }

            Debug.WriteLine("\n*** Fi Ok ***");
        }


        /// <summary>
        /// Esborra un traspàs.
        /// </summary>
        [TestMethod]
        public void EsborraTraspas()
        {
            const int idTraspasVenda = 194;

            using (var conn = new InversionsBDContext())
            {
                Debug.WriteLine("********** Inici **********");
                // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
                Usuari.Seleccionat = conn.Usuaris.Single(s => s.Id == (int)Usuari.Usuaris.Joan);

                var traspasVenda = conn.Moviments.Single(s => s.Id == idTraspasVenda);

                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    var delete = String.Format("DELETE from [DesglosCompres] where [MovCompraId] = {0}", traspasVenda._MovimentRefCompra.Id);
                    conn.Database.ExecuteSqlCommand(delete);
                    
                    delete = String.Format("DELETE from [Moviments] where [Id] >= {0} AND [Id] <= {1}", idTraspasVenda, traspasVenda._MovimentRefCompra.Id);
                    var filesDesglosCompres = conn.Database.ExecuteSqlCommand(delete);

                    if (filesDesglosCompres != 2)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception();
                    }

                    dbContextTransaction.Commit();
                }
            }

            Debug.WriteLine("\nFinal");
        }

        #endregion *** Modifiquen dades ***


        #region *** Test ***


        [TestMethod]
        public void xx()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            foreach (var producte in sessio.Productes)
            {
                var aaX = producte.costOriginalEnCartera4X();
                var aa = producte.costOriginalEnCartera4();
                Assert.AreEqual(aaX, aa);
            }

            //var prod = sessio.Productes.Single(w => w.Id == 5);
            //var data = new DateTime(2015, 1, 1);


            //var aaC = prod.compresDePartipacionsEnData(data).ToList();
            //var bbC = prod.compresDePartipacions2X(data).ToList();

            //Debug.WriteLine("\n'aaC'");
            //foreach (var compraExt in aaC)
            //{
            //    Debug.WriteLine("{0}. Parts Util:{1}. Parts Ocup: {2}",
            //        compraExt.ToString(), compraExt._PartsUtilitzades, compraExt._PartsOcupades);
            //}

            //Debug.WriteLine("\n'bbC'");
            //foreach (var compra in bbC)
            //{
            //    Debug.WriteLine("{0}. Parts Util:{1}. Parts Ocup: {2}",
            //        compra.ToString(), compra._ParticipacionsUtilitzades, compra._ParticipacionsOcupades);
            //}

            //var aa = prod.desglosCompresDePartipacionsEnData(data).ToList();
            //var bb = prod.desglosDeCompres(data).ToList();

            //Debug.WriteLine("\n'aa'");
            //foreach (var desglosCompraExt in aa)
            //{
            //    Debug.WriteLine("{0}. Parts Util:{1}. Parts Ocup: {2}",
            //        desglosCompraExt.ToString(), desglosCompraExt._PartsUtilitzades, desglosCompraExt._PartsOcupades);
            //}

            //Debug.WriteLine("\n'bb'");
            //foreach (var desglosCompra in bb)
            //{
            //    Debug.WriteLine("{0}. Parts Util:{1}. Parts Ocup: {2}",
            //        desglosCompra.ToString(), desglosCompra._ParticipacionsUtilitzades, desglosCompra._ParticipacionsOcupades);
            //}
        }

        [TestMethod]
        public void PigVendesAny()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            
            double pigTot = 0;

            for (int any = 2013; any < 2022; any++)
            {
                var vendes = sessio.MovimentsUsuari.Where(w => w._EsVendaReal && w.Data.Year == any);

                double pig = 0;

                foreach (var venda in vendes)
                {
                    pig += venda.pig2VendaTest(true);
                }

                var dividents = sessio.MovimentsUsuari.Where(w => w._EsDividents && w.Data.Year == any).Sum(s=>s._ImportBrut);

                pigTot += pig + dividents;

                Debug.WriteLine("Any: {0}. PiG Net: {1}. Dividents: {2}", any, pig.ToString("C2"), dividents.ToString("C2"));
            }
            Debug.WriteLine("PiG Total: {0}.", pigTot.ToString("C2"));
        }


        //[TestMethod]
        //public void PigDefinitiu()
        //{
        //    InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

        //    foreach (var producte in sessio.Productes)
        //    {
        //        var pig2Tot = producte.pig2TotalTest();
        //        var pigDef = producte.pigDefinitiu();
        //        //Assert.AreEqual(pig2Tot, pigDef, .05);
        //    }

        //    var arcelor = sessio.ProdAccions.Single(w => w.Id == 7);

        //    var pig1 = arcelor.pigDefinitiu(new DateTime(2021, 12, 28), inclouEnCartera: true);
        //    var pig2 = arcelor.pigDefinitiu(new DateTime(2021, 12, 28), inclouEnCartera: false);
        //    var pigTot = arcelor.pigDefinitiu();
        //}





        [TestMethod]
        public void DividendsCompra()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            
            /*
                166	Compra	169,61 €
                174	Compra	124,71 €
                191	Compra	98,35 €
                182	Compra	139,41 €
                192	Compra	122,94 €
                193	Compra	106,47 €
             */

            var co = sessio.Moviments.Single(w => w.Id == 166).dividentsDeLaCompraTest();
            Assert.AreEqual(169.61, co, .05);

            co = sessio.Moviments.Single(w => w.Id == 174).dividentsDeLaCompraTest();
            Assert.AreEqual(124.71, co, .05);

            co = sessio.Moviments.Single(w => w.Id == 191).dividentsDeLaCompraTest();
            Assert.AreEqual(98.35, co, .05);

            co = sessio.Moviments.Single(w => w.Id == 182).dividentsDeLaCompraTest();
            Assert.AreEqual(139.41, co, .05);

            co = sessio.Moviments.Single(w => w.Id == 192).dividentsDeLaCompraTest();
            Assert.AreEqual(122.94, co, .05);

            co = sessio.Moviments.Single(w => w.Id == 193).dividentsDeLaCompraTest();
            Assert.AreEqual(106.47, co, .001);
        }


        [TestMethod]
        public void ProvesDesgloçarCompra92()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var co = sessio.Moviments.Single(w => w.Id == 92);
            
            co.desgloçarCompra(null, co.RefTraspas);
        }


        [TestMethod]
        public void ComprovaPartsOrig()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            int iguals = 0;
            int diferents = 0;
            foreach (var desg in sessio.DesglosCompras)
            {
                if(desg.MovCompra.DesglosCompres.Count == 1)
                    continue;
                if (Utilitats.SonIguals(desg.ParticipacionsOrig, desg.MovCompraOrig.Participacions))
                    iguals++;
                else
                    diferents++;
            }

            //Assert.AreEqual(12950.6981, tot, 3);
        }


        [TestMethod]
        public void PigProshares_2015_2016()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var proshares = sessio.ProdAccions.Single(w => w.Id == 17);
            var pig2015 = proshares.pig2CarteraTest(2015, true, true);
            var pig2016 = proshares.pig2CarteraTest(2016, true, true);
            var pig2017 = proshares.pig2CarteraTest(2017, true, true);

            //Assert.AreEqual(12950.6981, tot, 3);
        }

        [TestMethod]
        public void PiG_2021()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var pigAcc = Producte.Pig2CarteraTest(Producte.TipusProducte.Accions, null, 2022, true, true);
            var pigRF = Producte.Pig2CarteraTest(Producte.TipusProducte.Fons,  TipusFons.RF, 2022, true, true);
            var pigRV = Producte.Pig2CarteraTest(Producte.TipusProducte.Fons,  TipusFons.RV, 2022, true, true);
            var pigFons = Producte.Pig2CarteraTest(Producte.TipusProducte.Fons,  TipusFons.Tots, 2022, true, true);
            var pigTot = Producte.Pig2CarteraTest(Producte.TipusProducte.Tots,  null, 2022, true, true);

            Assert.AreEqual(pigRF + pigRV, pigFons, 3);
            Assert.AreEqual(pigRF + pigRV + pigAcc, pigTot, 3);
            Assert.AreEqual(pigFons + pigAcc, pigTot, 3);

        }

        [TestMethod]
        public void ComprovaPiGArcelor_2021()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var arcelor = sessio.ProdAccions.Single(w => w.Id == 7);
            var pigArcelor = arcelor.pig3TotalTest(2021, false, false);

            var c191 = sessio.Moviments.Single(s => s.Id == 191).pigDeLaCompraEsElBoooooTest(false, true, 2021, true, false);
            var c182 = sessio.Moviments.Single(s => s.Id == 182).pigDeLaCompraEsElBoooooTest(false, true, 2021, true, false);
            var tot = c191 + c182;

            Assert.AreEqual(pigArcelor, tot, 3);
            Assert.AreEqual(12950.6981, tot, 3);
        }

        [TestMethod]
        public void ComprovaPiGTelefonica_2015()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var telefonica = sessio.ProdAccions.Single(w => w.Id == 9);
            var pigTel = telefonica.pig3TotalTest(2015, false, false);

            var c77 = sessio.Moviments.Single(s => s.Id == 77).pigDeLaCompraEsElBoooooTest(false, true, 2015, true, false);
            var c85 = sessio.Moviments.Single(s => s.Id == 85).pigDeLaCompraEsElBoooooTest(false, true, 2015, true, false);
            var c99 = sessio.Moviments.Single(s => s.Id == 99).pigDeLaCompraEsElBoooooTest(false, true, 2015, true, false);
            var c106 = sessio.Moviments.Single(s => s.Id == 106).pigDeLaCompraEsElBoooooTest(false, true, 2015, true, false);
            var tot = c77 + c85 + c99 + c106;

            Assert.AreEqual(pigTel, tot, 5);
            Assert.AreEqual(9418.22, tot, 3);
        }

        [TestMethod]
        public void ComprovaCompresRealsAmbVendesEnDiferentsAnys()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            
            var compraArcelor = sessio.Moviments.Single(w => w.Id == 166);
            var venda = compraArcelor.pigDeLaCompraEsElBoooooTest(false, true, null, true, false);
            var venda2018 = compraArcelor.pigDeLaCompraEsElBoooooTest(false, true, 2018, true, false);
            var venda2019 = compraArcelor.pigDeLaCompraEsElBoooooTest(false, true, 2019, true, false);

            Assert.AreEqual(venda, venda2018 + venda2019, 5);
        }

        /// <summary>
        /// 28/05/2021. 
        /// </summary>
        [TestMethod]
        public void ComprovaSplitContraSplit()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var data = DateTime.Now;
            var prodsAccions = sessio.ProdAccions.ToList();

            var arcelor = sessio.ProdAccions.Single(w => w.Id == 7);
            var movs = arcelor.compresDePartipacionsTest(DateTime.Now);
            var preuPartAct = arcelor._PreuParticipacioActual;

            Debug.WriteLine("Prod\tImport");
            foreach (var prod in prodsAccions)
            {
                var partsEnCartera = prod.numParticipacionsEnDataTest(data);

                double importBrut = partsEnCartera * prod._PreuParticipacioActual;
                if(importBrut>0)
                    Debug.WriteLine("{0}\t{1}", prod, importBrut.ToString("#,###.000", CultureInfo.CurrentCulture));
            }
        }


        /// <summary>
        /// 28/05/2021. 
        /// </summary>
        [TestMethod]
        public void ParticipacionsOriginalsEnCartera()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var prodsFons = sessio.ProdFons.ToList();
            Dictionary<Producte, double[]> totProdsOrig = new Dictionary<Producte, double[]>();
            Dictionary<Producte, double[]> totProdsAct = new Dictionary<Producte, double[]>();
            var data = DateTime.Now;

            foreach (var prod in prodsFons)
            {
                var partsEnCartera = prod.numParticipacionsEnDataTest(data);
                if (partsEnCartera > 0)
                {
                    var compres = prod.compresDePartipacionsTest(data, partsEnCartera);
                    foreach (var compra in compres)
                    {
                        foreach (var desglosCompra in compra.DesglosCompres.Where(w => w._ParticipacionsUtilitzades > 0))
                        {
                            var prodOrig = desglosCompra.MovCompraOrig.Prod;
                            if (!totProdsOrig.ContainsKey(prodOrig))
                                totProdsOrig.Add(prodOrig, new double[] { 0, 0 });

                            totProdsOrig[prodOrig][0] += desglosCompra._ParticipacionsUtilitzadesOrig;
                            totProdsOrig[prodOrig][1] += desglosCompra._ParticipacionsUtilitzadesOrig * desglosCompra._PreuParticipacioOrig;

                            var prodAct = desglosCompra.MovCompra.Prod;
                            if (!totProdsAct.ContainsKey(prodAct))
                                totProdsAct.Add(prodAct, new double[] { 0, 0 });

                            totProdsAct[prodAct][0] += desglosCompra._ParticipacionsUtilitzades;
                            totProdsAct[prodAct][1] += desglosCompra._ParticipacionsUtilitzades * prod._PreuParticipacioActual;
                        }
                    }
                }
            }

            Debug.WriteLine("Prod Orig\tParts\tImport");
            foreach (KeyValuePair<Producte, double[]> totProd in totProdsOrig.OrderBy(o => o.Key._NomProducte))
            {
                var prod = totProd.Key;
                var parts = totProd.Value[0];
                var importC = totProd.Value[1];

                Debug.WriteLine("{0}\t{1}\t{2}", prod, parts.ToString("#,###.000", CultureInfo.CurrentCulture), importC.ToString("#,###.00", CultureInfo.CurrentCulture));
            }


            Debug.WriteLine("\n\nProd Act\tParts\tImport");
            foreach (KeyValuePair<Producte, double[]> totProd in totProdsAct.OrderBy(o => o.Key._NomProducte))
            {
                var prod = totProd.Key;
                var parts = totProd.Value[0];
                var importC = totProd.Value[1];

                Debug.WriteLine("{0}\t{1}\t{2}", prod, parts.ToString("#,###.000", CultureInfo.CurrentCulture), importC.ToString("#,###.00", CultureInfo.CurrentCulture));
            }
        }


        /// <summary>
        /// 12/05/2021. Compara compresAnteriors4Test amb compresAnteriors3Test.
        /// </summary>
        [TestMethod]
        public void ComprovaVendesDeLaCompra()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            double participEnCartera;

            //var compres = sessio.MovimentsUsuari.Where(w => w._EsCompra);
            //foreach (var compra in compres)
            //{
            //    var vendes = compra.vendesDeLaCompraTest();
            //}

            var compra = sessio.Moviments.Single(w => w.Id == 182);
            var vendesCompra = compra.vendesDeLaCompraTest();
            var partsCart = compra.Participacions - vendesCompra.Sum(s => s._ParticipacionsUtilitzades);
        }


        /// <summary>
        /// 27/04/2021
        /// </summary>
        [TestMethod]
        public void pig2TotalTest()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            const int any = 2018;
            double pigTot = 0;
            Debug.WriteLine("\n");
            var vendes = sessio.MovimentsUsuari.Where(w => w._EsVendaReal && w.Data.Year == any).ToList();
            foreach (var venda in vendes)
            {
                Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", venda.Prod, venda.Id, venda.Data.ToShortDateString()
                    , venda.Participacions, venda.PreuParticipacio);

                var xx = venda.compresDeLaVenda4Test().ToList();
                var impO = xx.Sum(s => s.DesglosCompres.Sum(ss => ss._ParticipacionsUtilitzadesOrig * ss._PreuParticipacioOrig));
                //var impOX = xx.Sum(s => s.DesglosCompres.Sum(ss => ss._ParticipacionsDisponiblesXOrig * ss._PreuParticipacioOrig));
                var pUtil = xx.Sum(s => s._ParticipacionsUtilitzades);
                //var pDisp = xx.Sum(s => s._ParticipacionsDisponiblesX);
                var pUtilDesg = xx.Sum(s => s.DesglosCompres.Sum(ss => ss._ParticipacionsUtilitzadesOrig));
                //var pDispDesg = xx.Sum(s => s.DesglosCompres.Sum(ss => ss._ParticipacionsDisponiblesXOrig));

                var pigVenda = venda._ImportBrut - impO;
                pigTot += pigVenda;
            }
            Debug.WriteLine("\n");

            double impVendes = vendes.Sum(venda => venda._ImportBrut);
            double impCompres = vendes.Sum(venda => venda.calculaImportCompraOrigen3(true, true));
            var pigs = impVendes - impCompres;

            var pig = Producte.Pig2(Producte.TipusProducte.Tots, any, false, false);

            Debug.WriteLine(String.Format("PiG trib:{0}", pig.ToString("#,##0.00")));

            Debug.WriteLine("\n*** Fi Ok ***");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void PiGNou()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            double pigTotalEnCartera = 0;
            foreach (var prod in sessio.Productes)
            {
                var valorAct = prod._ValorActualEnCartera;

                Moviment venda = new Moviment();
                venda.Prod = prod;
                venda.Data = DateTime.Now;
                venda.TipusMoviment = TipusMoviment.Venda;
                venda.Participacions = prod._Participacions;

                var costOrig = venda.calculaImportCompraOrigen3(true, true);

                var piG = valorAct - costOrig;

                pigTotalEnCartera += piG;
            }

            Debug.WriteLine("\nPiG total en cartera:{0}", pigTotalEnCartera);

            double pigTotalVenut = 0;
            foreach (var venda in sessio.MovimentsUsuari.Where(w => w._EsVendaReal))
            {
                var costOrig = venda.calculaImportCompraOrigen3(true, true);

                var piG = venda._ImportBrut - costOrig;

                pigTotalVenut += piG;
            }

            Debug.WriteLine("PiG total venut:{0}", pigTotalVenut);

            Debug.WriteLine("PiG total:{0}", pigTotalEnCartera + pigTotalVenut);

            Debug.WriteLine("\n*** Fi Ok ***");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void VendesDeCompra()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            double pigTotal = 0;
            Moviment compra;
            double piG;
            const double preuPartsEnCartera = 24.71;

            Debug.WriteLine("preuPartsEnCartera: {0}.\n", preuPartsEnCartera);

            compra = sessio.Moviments.Single(w => w.Id == 166);
            piG = compra.pigDeLaCompraTest(preuPartsEnCartera);
            Debug.WriteLine("Id: {0}. PiG: {1}.", compra.Id, piG);
            if (preuPartsEnCartera == 21)
                Assert.AreEqual(-12314.272, piG, "PiG incorrecte");
            pigTotal += piG;

            compra = sessio.Moviments.Single(w => w.Id == 174);
            piG = compra.pigDeLaCompraTest(preuPartsEnCartera);
            Debug.WriteLine("Id: {0}. PiG: {1}.", compra.Id, piG);
            if (preuPartsEnCartera == 21)
                Assert.AreEqual(-1287.6, piG, "PiG incorrecte");
            pigTotal += piG;

            compra = sessio.Moviments.Single(w => w.Id == 178);
            piG = compra.pigDeLaCompraTest(preuPartsEnCartera);
            Debug.WriteLine("Id: {0}. PiG: {1}.", compra.Id, piG);
            if (preuPartsEnCartera == 21)
                Assert.AreEqual(3377.56, piG, "PiG incorrecte");
            pigTotal += piG;

            compra = sessio.Moviments.Single(w => w.Id == 182);
            piG = compra.pigDeLaCompraTest(preuPartsEnCartera);
            Debug.WriteLine("Id: {0}. PiG: {1}.", compra.Id, piG);
            if (preuPartsEnCartera == 21)
                Assert.AreEqual(11578.545, piG, "PiG incorrecte");
            pigTotal += piG;

            if (preuPartsEnCartera == 21)
                Assert.AreEqual(1354.233, Math.Round(pigTotal, 3), "PiG total incorrecte");

            Debug.WriteLine("\n*** Fi Ok ***");

            /*
             * preuPartsEnCartera: 24.71.

                Id: 166. PiG: -1450.848.
                Id: 174. PiG: 712.64.
                Id: 178. PiG: 4495.712.
                Id: 182. PiG: 17143.545.

                *** Fi Ok ***
             */
        }

        /// <summary>
        /// Comprova que el desgloç d'unes compres determinades son els esperats.
        /// </summary>
        [TestMethod]
        public void TestDesgloçCompres()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var compraT = sessio.Moviments.Single(w => w.Id == 41);

            Assert.AreEqual(1, compraT.DesglosCompres.Count(), "Número de files desgloç incorrecte");

            foreach (var desglosCompra in compraT.DesglosCompres)
            {
                if (desglosCompra.MovCompraOrigId == 7)
                {
                    //MovId=41. MovOrigId=7. ParticipacionsDelMoviment=0.347200000000015. ParticipacionsDelMovimentOrigen=2.55975192993167
                    Assert.AreEqual(174.229, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(15, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else
                {
                    throw new AssertFailedException(String.Format("Aquesta fila no correspon{0}. ", desglosCompra.ToString()));
                }
            }


            compraT = sessio.Moviments.Single(w => w.Id == 92);

            Assert.AreEqual(4, compraT.DesglosCompres.Count(), "Número de files desgloç incorrecte");

            foreach (var desglosCompra in compraT.DesglosCompres)
            {
                if (desglosCompra.MovCompraOrigId == 15)
                {
                    //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                    Assert.AreEqual(443.134, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(679.64, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.MovCompraOrigId == 16)
                {
                    //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                    Assert.AreEqual(866.4984, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(547.6451, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.MovCompraOrigId == 17)
                {
                    //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                    Assert.AreEqual(1598.8742, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(1010.611, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.MovCompraOrigId == 27)
                {
                    //Id=44. MovId=31. MovOrigId=27. ParticipacionsDelMoviment=1.23029999999997. ParticipacionsDelMovimentOrigen=11.1955917196529
                    Assert.AreEqual(13.8534, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(11.1956, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else
                {
                    throw new AssertFailedException(String.Format("Aquesta fila no correspon{0}. ", desglosCompra.ToString()));
                }
            }


            var compres = sessio.Moviments.Single(w => w.Id == 91).compresDeLaVenda4Test().ToList();

            Assert.AreEqual(5, compres.Count(), "Número de files desgloç incorrecte");

            foreach (var compra in compres)
            {
                foreach (var desglosCompra in compra.DesglosCompres)
                {
                    if (desglosCompra.MovCompraId == 26 && desglosCompra.MovCompraOrigId == 17)
                    {
                        //Id=24. MovId=26. MovOrigId=17. ParticipacionsDelMoviment=0.347200000000015. ParticipacionsDelMovimentOrigen=2.55975192993167
                        Assert.AreEqual(0.3472, Math.Round(desglosCompra.Participacions, 4));
                        Assert.AreEqual(2.5598, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                    }
                    else if (desglosCompra.MovCompra.Id == 31 && desglosCompra.MovCompraOrig.Id == 15)
                    {
                        //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                        Assert.AreEqual(39.354, Math.Round(desglosCompra.Participacions, 4));
                        Assert.AreEqual(679.64, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                    }
                    else if (desglosCompra.MovCompra.Id == 31 && desglosCompra.MovCompraOrig.Id == 16)
                    {
                        //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                        Assert.AreEqual(76.9523, Math.Round(desglosCompra.Participacions, 4));
                        Assert.AreEqual(547.6451, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                    }
                    else if (desglosCompra.MovCompra.Id == 31 && desglosCompra.MovCompraOrig.Id == 17)
                    {
                        //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                        Assert.AreEqual(141.6462, Math.Round(desglosCompra.Participacions, 4));
                        Assert.AreEqual(1008.0512, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                    }
                    else if (desglosCompra.MovCompra.Id == 31 && desglosCompra.MovCompraOrig.Id == 27)
                    {
                        //Id=44. MovId=31. MovOrigId=27. ParticipacionsDelMoviment=1.23029999999997. ParticipacionsDelMovimentOrigen=11.1955917196529
                        Assert.AreEqual(1.2303, Math.Round(desglosCompra.Participacions, 4));
                        Assert.AreEqual(11.1956, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                    }
                    else
                    {
                        throw new AssertFailedException(String.Format("Aquesta fila no correspon{0}-{1}. ", compra, desglosCompra));
                    } 
                }
            }

            System.Diagnostics.Debug.WriteLine("*** Fi Ok ***");
        }


        /// <summary>
        /// Comprova que el desgloç d'unes compres determinades son els esperats.
        /// </summary>
        [TestMethod]
        public void ProvesVaries()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            

            var compraT = sessio.Moviments.Single(w => w.Id == 41);
            var vendaT = sessio.Moviments.Single(w => w.Id == compraT.RefTraspasId);
            var compra = sessio.Moviments.Single(w => w.Id == 1);
            var venda = sessio.Moviments.Single(w => w.Id == 4);

            var t0 = compraT.RefTraspas1.Any();
            var t1 = vendaT.RefTraspas1.Any();
            var t2 = compra.RefTraspas1.Any();
            var t3 = venda.RefTraspas1.Any();

            var x0 = compraT.RefTraspas;
            var x1 = vendaT.RefTraspas;
            var x2 = compra.RefTraspas;
            var x3 = venda.RefTraspas;

            System.Diagnostics.Debug.WriteLine("*** Fi Ok ***");
        }

        #endregion *** Test ***


        #region *** Mètodes ***

        internal static InversionsBDContext ConnectaBd()
        {
            InversionsBDContext sessio = new InversionsBDContext();
            sessio.Configuration.AutoDetectChangesEnabled = false; // Si poso true, dona error quan inserto una fila i l'esborro en la mateixa sessió.
            sessio.Configuration.LazyLoadingEnabled = true;

            return sessio;
        }

        /// <summary>
        /// Connecta la BD.
        /// </summary>
        /// <param name="usuari"></param>
        /// <returns></returns>
        internal static InversionsBDContext ConnectaBd(Usuari.Usuaris usuari)
        {
            InversionsBDContext sessio = ConnectaBd();
            Usuari.Seleccionat = sessio.Usuaris.Single(s => s.Id == (int)usuari);

            return sessio;
        }

        internal static InversionsBDContext ConnectaBd(Usuari usuari)
        {
            InversionsBDContext sessio = ConnectaBd();
            Usuari.Seleccionat = usuari;

            return sessio;
        }

        #endregion *** Mètodes ***
    }
}
