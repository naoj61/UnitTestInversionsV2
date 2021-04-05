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
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

            var sessio = new InversionsBDContext();

            sessio.Database.ExecuteSqlCommand("EXEC sp_RENAME 'DesglosCompres.RefCompraId' , 'MovCompraId', 'COLUMN'");
            sessio.Database.ExecuteSqlCommand("EXEC sp_RENAME 'DesglosCompres.RefCompraOrigId' , 'MovCompraOrigId', 'COLUMN'");
            
            sessio.Database.ExecuteSqlCommand("ALTER TABLE [Moviments] DROP CONSTRAINT [FK_ProducteMoviment]");
            sessio.Database.ExecuteSqlCommand("DROP INDEX [Moviments].[IX_FK_ProducteMoviment]");
            sessio.Database.ExecuteSqlCommand("ALTER TABLE [Moviments] DROP COLUMN [ProducteTraspasId]");
        }

        /// <summary>
        /// En els trapassos, informa MovimentRefVendaId(MovimentRefTraspasId) de les vendes.
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

                    foreach (var movCompraTraspas in conn.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra && w.MovimentRefVendaId != null))
                    {
                        var movVendaTraspas = conn.Moviments.Single(w => w.Id == movCompraTraspas.MovimentRefVendaId);
                        movVendaTraspas.MovimentRefVendaId = movCompraTraspas.Id;
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
        //[TestMethod]
        public void GeneraDesgloçCompres()
        {
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

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
                            co.desgloçarCompra(conn);
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
        /// Fa un traspàs de fons.
        /// </summary>
        //[TestMethod]
        public void TraspasFons()
        {
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            const int idDel = 168;
            var files = sessio.Database.ExecuteSqlCommand("DELETE from [DesglosCompres] where [MovCompraId] > " + idDel);
            files = sessio.Database.ExecuteSqlCommand("DELETE from [Moviments] where[Id] > " + idDel);

            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    System.Diagnostics.Debug.WriteLine("********** Inici **********");

                    //var prodVenda = conn.ProdFons.Single(w => w.Id == 16);
                    //var prodCompra = conn.ProdFons.Single(w => w.Id == 11);
                    //var dataVenda = new DateTime(2017, 11, 6, 11, 30, 00); // 06/11/2017 11:30:00
                    //const double participacionsVenda = 2500;
                    //const double preuParticipacioVenda = 10.08;
                    //const string descripcio = "";
                    //const double participacionsCompra = 57.3069;

                    var prodVenda = conn.ProdFons.Single(w => w.Id == 16);
                    var prodCompra = conn.ProdFons.Single(w => w.Id == 11);
                    var dataVenda = DateTime.Now;

                    const double participacionsVenda = 60;
                    const double preuParticipacioVenda = 400;
                    const string descripcio = null;
                    const double participacionsCompra = 1000;

                    prodVenda.desaTraspas(conn, dataVenda, participacionsVenda, preuParticipacioVenda, descripcio, dataVenda.AddSeconds(1)
                        , prodCompra, participacionsCompra);

                    //prodVenda.desaCompra(conn, dataVenda, DateTime.Now.TimeOfDay, participacionsVenda, preuParticipacioVenda, 1, 0, descripcio
                    //    , false, false);

                    dbContextTransaction.Commit();
                }
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Les accions sempre han de tenir el preu oigen igual al real.
        /// </summary>
        [TestMethod]
        public void IgualaPreuOrigenAmbRealEnAccions()
        {
            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    var movsAccions = conn.Moviments.Where(w => w.Prod is ProdAccions).ToList();

                    foreach (var movsAccio in movsAccions)
                    {
                        if (movsAccio.PreuParticipacio != movsAccio._PreuCompraParticipacioOrigen)
                        {
                            movsAccio._PreuCompraParticipacioOrigen = movsAccio.PreuParticipacio;
                            conn.Moviments.AddOrUpdate(movsAccio);
                            conn.SaveChanges();
                        }
                    }

                    dbContextTransaction.Commit();
                }
            }

            Debug.WriteLine("*** Fi Ok ***");
        }

        #endregion *** Modifiquen dades ***



        #region *** Test ***


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void PreuPartOrigTest()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var compres = sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra).ToList();

            int contErrors = 0;
            foreach (var compra in compres)
            {
                var preuOrig = Math.Round(compra._PreuParticipacioOrigen2Test, 4);
                var preuOrigAnt = Math.Round(compra._PreuCompraParticipacioOrigen.GetValueOrDefault(compra.PreuParticipacio), 4);

                if (Math.Abs(preuOrig - preuOrigAnt) > 1)
                {
                    contErrors++;
                    Debug.WriteLine("Preus no coincideixen Id:{0}\tpreuOrig:{1}\tpreuOrigAnt:{2}\tDif:{3}\tNom Prod:{4}."
                        , compra.Id, preuOrig, preuOrigAnt, Math.Round(preuOrig - preuOrigAnt, 4), compra.Prod._NomProducte);
                }
            }

            Debug.WriteLine("Nom Ok:{0}. Num errors:{1}", compres.Count - contErrors, contErrors);

            //Moviment mov = sessio.Moviments.Single(w => w.Id == 171);

            //var xx = mov._PreuParticipacioOrigen2Test;

            Debug.WriteLine("\n*** Fi Ok ***");
        }


        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void ProvesCompresDeLaVenda()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            Moviment venda = sessio.Moviments.Single(w => w.Id == 179);

            var xx = venda.compresDeLaVendaTest();

            Debug.WriteLine("\n*** Fi Ok ***");
        }

        /// <summary>
        /// Comprova que les accions tenen el PreuParticipacio i el PreuParticipacioOrigen, iguals.
        /// </summary>
        [TestMethod]
        public void TestPreusOrigenEnAccions()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            var movsAccions = sessio.Moviments.Where(w => w.Prod is ProdAccions).ToList();

            var llistaErrors = new List<Moviment>();
            foreach (var movsAccio in movsAccions)
            {
                if (movsAccio.PreuParticipacio != movsAccio._PreuCompraParticipacioOrigen)
                {
                    if (movsAccio.PreuParticipacio - movsAccio._PreuCompraParticipacioOrigen < 0.001)
                    {
                        continue;
                    }

                    Debug.WriteLine("Id: {0}. Prod: {1}. Data: {2}. Preu part: {3}. Preu orig: {4}",
                        movsAccio.Id, movsAccio.Prod._NomProducte, movsAccio.Data.ToShortDateString(), movsAccio.PreuParticipacio, movsAccio._PreuCompraParticipacioOrigen);

                    llistaErrors.Add(movsAccio);
                }
            }

            Debug.WriteLine("*** Fi Ok ***");
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


            var compres = sessio.Moviments.Single(w => w.Id == 91).TestCompresDeLaVenda(sessio).ToList();

            Assert.AreEqual(5, compres.Count(), "Número de files desgloç incorrecte");

            foreach (var mdc in compres)
            {
                if (mdc._DesglosCompra.MovCompraId == 26 && mdc._DesglosCompra.MovCompraOrigId == 17)
                {
                    //Id=24. MovId=26. MovOrigId=17. ParticipacionsDelMoviment=0.347200000000015. ParticipacionsDelMovimentOrigen=2.55975192993167
                    Assert.AreEqual(0.3472, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(2.5598, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.MovCompra.Id == 31 && mdc._DesglosCompra.MovCompraOrig.Id == 15)
                {
                    //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                    Assert.AreEqual(39.354, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(679.64, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.MovCompra.Id == 31 && mdc._DesglosCompra.MovCompraOrig.Id == 16)
                {
                    //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                    Assert.AreEqual(76.9523, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(547.6451, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.MovCompra.Id == 31 && mdc._DesglosCompra.MovCompraOrig.Id == 17)
                {
                    //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                    Assert.AreEqual(141.6462, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(1008.0512, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.MovCompra.Id == 31 && mdc._DesglosCompra.MovCompraOrig.Id == 27)
                {
                    //Id=44. MovId=31. MovOrigId=27. ParticipacionsDelMoviment=1.23029999999997. ParticipacionsDelMovimentOrigen=11.1955917196529
                    Assert.AreEqual(1.2303, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(11.1956, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else
                {
                    throw new AssertFailedException(String.Format("Aquesta fila no correspon{0}. ", mdc.ToString()));
                }
            }

            System.Diagnostics.Debug.WriteLine("*** Fi Ok ***");
        }


        /// <summary>
        /// No poden haver data hora minut segon duplicats.
        /// </summary>
        [TestMethod]
        public void ComprovaDatesMovDuplicades()
        {
            try
            {
                InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

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
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var thailand = sessio.ProdFons.Single(w => w.Id == 13);

            var pig = thailand.pig2TotalTest(inclouCartera:false);
            var pig2013 = thailand.pig2TotalTest(2013);

            double preuOrig;
            var mov = sessio.Moviments.Single(s => s.Id == 25);
            if (mov.TipusMoviment == TipusMoviment.Compra)
            {
                var venda = mov.MovimentRefVendaN;
                preuOrig = venda.Participacions * venda._PreuCompraParticipacioOrigen.Value / mov.Participacions;
                Assert.AreEqual(mov._PreuCompraParticipacioOrigen.GetValueOrDefault(), preuOrig, 0.001, "Preu origen no coincideix");
            }
            else
            {
                var venda = mov;

                var compresVenda = venda.compresAnteriors();
                preuOrig = compresVenda.Sum(movimentCompra => movimentCompra._ParticipacionsDisponibles * movimentCompra._PreuParticipacioOrigenTest);
                preuOrig = preuOrig / venda.Participacions;
                Assert.AreEqual(venda._PreuCompraParticipacioOrigen.GetValueOrDefault(), preuOrig, 0.001, "Preu origen no coincideix");
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
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            System.Diagnostics.Debug.WriteLine("********** Inici **********");

            int oks = 0, kos = 0;

            foreach (var compra in sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
            {
                var costTotalOrig = compra.DesglosCompres.Sum(s => s.ParticipacionsOrig * s._PreuParticipacioOrig);
                var preuUnitOrig = Math.Round(costTotalOrig / compra.Participacions, 4);
                var preuOrigAnt = Math.Round(compra._PreuCompraParticipacioOrigen.GetValueOrDefault(), 4);
                var dif = Math.Round(preuUnitOrig - preuOrigAnt, 2);

                if (dif > 0)
                {
                    System.Diagnostics.Debug.WriteLine("MovId = {0}\tDif = {3}\tpreuUnitOrig = {1}\tpreuOrigAnt = {2}"
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


        /// <summary>
        /// Comprova que el desgloç d'unes compres determinades son els esperats.
        /// </summary>
        [TestMethod]
        public void ProvesVaries()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);
            

            var compraT = sessio.Moviments.Single(w => w.Id == 41);
            var vendaT = sessio.Moviments.Single(w => w.Id == compraT.MovimentRefVendaId);
            var compra = sessio.Moviments.Single(w => w.Id == 1);
            var venda = sessio.Moviments.Single(w => w.Id == 4);

            var t0 = compraT.MovimentRefVenda1.Any();
            var t1 = vendaT.MovimentRefVenda1.Any();
            var t2 = compra.MovimentRefVenda1.Any();
            var t3 = venda.MovimentRefVenda1.Any();

            var x0 = compraT.MovimentRefVendaN;
            var x1 = vendaT.MovimentRefVendaN;
            var x2 = compra.MovimentRefVendaN;
            var x3 = venda.MovimentRefVendaN;

            System.Diagnostics.Debug.WriteLine("*** Fi Ok ***");
        }

        #endregion *** Test ***


        #region *** Mètodes ***

        /// <summary>
        /// Connecta la BD.
        /// </summary>
        /// <param name="usuari"></param>
        /// <returns></returns>
        internal static InversionsBDContext ConnectaBd(Usuari.Usuaris usuari)
        {
            InversionsBDContext sessio = new InversionsBDContext();
            sessio.Configuration.AutoDetectChangesEnabled = false; // Si poso true, dona error quan inserto una fila i l'esborro en la mateixa sessió.
            sessio.Configuration.LazyLoadingEnabled = true;

            Usuari.Seleccionat = sessio.Usuaris.Single(s => s.Id == (int)usuari);

            return sessio;
        }

        #endregion *** Mètodes ***
    }
}
