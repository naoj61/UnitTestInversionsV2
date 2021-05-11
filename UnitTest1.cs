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
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar la taula.

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
                        foreach (var co in conn.Moviments
                            .Where(w => w.UsuariId == usu.Id && w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
                        {
                            Debug.WriteLine("co.Id = {0}", co.Id);

                            //if(co.Id == 101)
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
        [
            TestMethod]
        public void GeneraDesgloçCompresMovId26()
        {
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

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

            System.Diagnostics.Debug.WriteLine("\nFinal");
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
                    prodVenda.desaTraspas(conn, dataVenda, participacionsVenda, preuParticipacioVenda, descripcio, dataVenda.AddSeconds(1)
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
                    prodOrig.desaTraspas(conn, data, 20, 600, String.Empty, data, prodDest, 20);

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

        #endregion *** Modifiquen dades ***


        #region *** Test ***

        /// <summary>
        /// 27/04/2021
        /// </summary>
        [TestMethod]
        public void xxx()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            double importTot = 0;
            double partsTot = 0;

            Debug.WriteLine("\nProd\tId\tParts\tImport");
            foreach (var prodFons in sessio.ProdFons)
            {
                if (prodFons.Id != 3)
                    continue;

                prodFons.resetParticipacionsDisponibles();

                var compres = prodFons.MovimentsProducteUsuari.Where(w => w._EsCompraReal).ToList();
                
                foreach (var compra in compres)
                {
                    try
                    {
                        var parts = compra.partsEnCarteraCompra();
                        var import = parts * compra.PreuParticipacio;
                        if (!Utilitats.EsZero(import))
                        {
                            importTot += import;
                            partsTot += parts;
                            Debug.WriteLine("{0}-{1}\t{2}\t{3}\t{4}", prodFons.Id, prodFons, compra.Id, parts.ToString(CultureInfo.CurrentCulture), import.ToString(CultureInfo.CurrentCulture)); 
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

            }
            Debug.WriteLine("\nParts: {0}. Import total: {1}", partsTot.ToString(CultureInfo.CurrentCulture), importTot.ToString(CultureInfo.CurrentCulture));

            Debug.WriteLine("\n*** Fi Ok ***");
        }


        /// <summary>
        /// 27/04/2021
        /// </summary>
        [TestMethod]
        public void pig2TotalTest()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            const int any = 2018;

            Debug.WriteLine("\n");
            var vendes = sessio.MovimentsUsuari.Where(w => w._EsVendaReal && w.Data.Year == any).ToList();
            foreach (var venda in vendes)
            {
                Debug.WriteLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}", venda.Prod, venda.Id, venda.Data.ToShortDateString(), venda.Participacions, venda.PreuParticipacio));
            }
            Debug.WriteLine("\n");

            double impVendes = vendes.Sum(venda => venda.ImportBrut);
            double impCompres = vendes.Sum(venda => venda.calculaImportCompraOrigen3(true, true));
            var pigs = impVendes - impCompres;

            var pig = Producte.Pig2(Producte.TipusProducte.Tots, any, false);

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

                var piG = venda.ImportBrut - costOrig;

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


            var compres = sessio.Moviments.Single(w => w.Id == 91).compresDeLaVenda3Test().ToList();

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
