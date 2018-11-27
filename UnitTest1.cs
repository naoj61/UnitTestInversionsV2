using System;
using System.Data.Entity.Core;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inversions;

namespace UnitTestInversions
{
    [TestClass]
    public class UnitTestInversions
    {
        /// <summary>
        /// Connecta la BD.
        /// </summary>
        /// <param name="usuari"></param>
        /// <returns></returns>
        private static InversionsBDContext ConnectaBd(Usuari.Usuaris usuari)
        {
            InversionsBDContext sessio = new InversionsBDContext();
            sessio.Configuration.AutoDetectChangesEnabled = false; // Si poso true, dona error quan inserto una fila i l'esborro en la mateixa sessió.
            sessio.Configuration.LazyLoadingEnabled = true;

            Usuari.Seleccionat = sessio.Usuaris.Single(s => s.Id == (int) usuari);

            return sessio;
        }

        #region *** Modifiquen dades ***

        /// <summary>
        /// Esborra la taula "DesglosCompres" i la crea de nou.
        /// </summary>
        [TestMethod]
        public void GeneraDesgloçCompres()
        {
            return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

            var sessio = new InversionsBDContext();

            sessio.Database.ExecuteSqlCommand("TRUNCATE TABLE [DesglosCompres]");

            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("********** Inici **********");

                        foreach (var usu in sessio.Usuaris)
                        {
                            foreach (var co in sessio.Moviments
                                .Where(w => w.UsuariId == usu.Id && w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
                            {
                               //if(co.Id == 170)
                                co.desgloçarCompra(conn);
                            }
                        }

                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        throw;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }

        /// <summary>
        /// Fa un traspàs de fons.
        /// </summary>
        [TestMethod]
        public void TraspasFons()
        {
            return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Carme);

            var files = sessio.Database.ExecuteSqlCommand("DELETE from [Moviments] where [UsuariId] = 2 AND [Id] > 168");

            using (var conn = new InversionsBDContext())
            {
                using (var dbContextTransaction = conn.Database.BeginTransaction())
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("********** Inici **********");

                        var prodVenda = conn.ProdFons.Single(w => w.Id == 15);
                        var prodCompra = conn.ProdFons.Single(w => w.Id == 16);

                        var dataVenda = new DateTime(2017, 11, 6, 11, 30, 00); // 06/11/2017 11:30:00

                        const double participacionsVenda = 2500;
                        const double preuParticipacioVenda = 10.08;
                        const string descripcio = "";
                        const double participacionsCompra = 57.3069;

                        prodVenda.desaTraspas(conn, dataVenda, participacionsVenda, preuParticipacioVenda, descripcio, dataVenda.AddSeconds(1)
                            , prodCompra, participacionsCompra);

                        conn.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        throw;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }

        #endregion *** Modifiquen dades ***



        #region *** Test ***

        [TestMethod]
        public void TestDesgloçCompres()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var compraX = sessio.Moviments.Single(w => w.Id == 41);

            Assert.AreEqual(1, compraX.DesglosCompres.Count(), "Número de files desgloç incorrecte");

            foreach (var desglosCompra in compraX.DesglosCompres)
            {
                if (desglosCompra.MovimentOrig.Id == 7)
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


            compraX = sessio.Moviments.Single(w => w.Id == 92);

            Assert.AreEqual(4, compraX.DesglosCompres.Count(), "Número de files desgloç incorrecte");

            foreach (var desglosCompra in compraX.DesglosCompres)
            {
                if (desglosCompra.MovimentOrig.Id == 15)
                {
                    //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                    Assert.AreEqual(443.134, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(679.64, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.MovimentOrig.Id == 16)
                {
                    //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                    Assert.AreEqual(866.4984, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(547.6451, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.MovimentOrig.Id == 17)
                {
                    //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                    Assert.AreEqual(1598.8742, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(1010.611, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.MovimentOrig.Id == 27)
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


            var compra = sessio.Moviments.Single(w => w.Id == 91).TestCompresDeLaVenda(sessio).ToList();

            Assert.AreEqual(5, compra.Count(), "Número de files desgloç incorrecte");

            foreach (var mdc in compra)
            {
                if (mdc._DesglosCompra.Moviment.Id == 26 && mdc._DesglosCompra.MovimentOrig.Id == 17)
                {
                    //Id=24. MovId=26. MovOrigId=17. ParticipacionsDelMoviment=0.347200000000015. ParticipacionsDelMovimentOrigen=2.55975192993167
                    Assert.AreEqual(0.3472, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(2.5598, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.Moviment.Id == 31 && mdc._DesglosCompra.MovimentOrig.Id == 15)
                {
                    //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                    Assert.AreEqual(39.354, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(679.64, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.Moviment.Id == 31 && mdc._DesglosCompra.MovimentOrig.Id == 16)
                {
                    //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                    Assert.AreEqual(76.9523, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(547.6451, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.Moviment.Id == 31 && mdc._DesglosCompra.MovimentOrig.Id == 17)
                {
                    //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                    Assert.AreEqual(141.6462, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(1008.0512, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.Moviment.Id == 31 && mdc._DesglosCompra.MovimentOrig.Id == 27)
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
        }


        /// <summary>
        /// Esborra la taula "DesglosCompres" i la crea de nou.
        /// </summary>
        [TestMethod]
        public void TestPreuOriginal()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            System.Diagnostics.Debug.WriteLine("********** Inici **********");

            int oks = 0, kos =0;

            foreach (var compra in sessio.Moviments.Where(w => w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
            {
                var costTotalOrig = compra.DesglosCompres.Sum(s => s.ParticipacionsOrig * s._PreuPartOrig);
                var preuUnitOrig = Math.Round(costTotalOrig / compra.Participacions, 4);
                var preuOrigAnt = Math.Round(compra.PreuParticipacioOrigen.GetValueOrDefault(), 4);
                var dif = Math.Round(preuUnitOrig - preuOrigAnt, 2);

                if (dif > 0)
                {
                    System.Diagnostics.Debug.WriteLine("MovId = {0}\tDif = {3}\tpreuUnitOrig = {1}\tpreuOrigAnt = {2}", compra.Id, preuUnitOrig, preuOrigAnt, dif);
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


        [TestMethod]
        public void CompresDunaVenda()
        {
            try
            {
                InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

                var venda = sessio.Moviments.Single(s => s.Id == 100);
                var prod = venda.Prod;
                System.Diagnostics.Debug.WriteLine("\nIdProducte: " + prod.Id);
                System.Diagnostics.Debug.WriteLine("Producte: " + prod._NomProducte);

                var compres = venda.compresAnteriors().ToArray();

                if (venda.Id == 100)
                {
                    Assert.AreEqual(2, compres.Count(), 0, "Count incorrecte");

                    comprovaMoviment(compres[0], 29, 1.827);
                    comprovaMoviment(compres[1], 92, 2877.415);
                }
            }
            //catch (AssertFailedException) {}
            catch (Exception es)
            {
                System.Diagnostics.Debug.WriteLine("\nError" + es.Message);
            }

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }


        /// <summary>
        /// Comprova el càlcul del preu origen en els traspassos de fons.
        /// </summary>
        [TestMethod]
        public void ComprovaCalculPreuOrigen()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            double preuOrig;
            var mov = sessio.Moviments.Single(s => s.Id == 25);
            if (mov.TipusMoviment == TipusMoviment.Compra)
            {
                var venda = mov.MovimentRefVenda;
                preuOrig = venda.Participacions * venda.PreuParticipacioOrigen.Value / mov.Participacions;
                Assert.AreEqual(mov.PreuParticipacioOrigen.GetValueOrDefault(), preuOrig, 0.001, "Preu origen no coincideix");
            }
            else
            {
                var venda = mov;

                var compresVenda = venda.compresAnteriors();
                preuOrig = compresVenda.Sum(movimentCompra => movimentCompra._ParticipacionsDisponibles * movimentCompra._PreuParticipacioOrigen);
                preuOrig = preuOrig / venda.Participacions;
                Assert.AreEqual(venda.PreuParticipacioOrigen.GetValueOrDefault(), preuOrig, 0.001, "Preu origen no coincideix");
            }

            //venda = sessio.Moviments.Single(s => s.Id == 25);
            //venda = sessio.Moviments.Single(s => s.Id == 28);
            //venda = sessio.Moviments.Single(s => s.Id == 30);

            System.Diagnostics.Debug.WriteLine("\nFinal");
        }
        
        #endregion *** Test ***

        private static void comprovaMoviment(MovimentCompra compra, int id, double part)
        {
            System.Diagnostics.Debug.WriteLine("\nIdMov={0}. _ParticipacionsDisponibles={1}",
                compra._Moviment.Id, compra._ParticipacionsDisponibles.ToString("0.000"));
            Assert.AreEqual(id, compra._Moviment.Id, 0, "Num Participacions Disponibles incorrecte");
            Assert.AreEqual(part, compra._ParticipacionsDisponibles, 0.001, "Num Participacions Disponibles incorrecte");
        }
    }
}
