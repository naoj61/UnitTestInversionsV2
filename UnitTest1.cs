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
        #region *** Modifiquen dades ***

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
                System.Diagnostics.Debug.WriteLine("********** Inici **********");

                foreach (var usu in sessio.Usuaris)
                {
                    // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
                    Usuari.Seleccionat = usu;
                    
                    using (var dbContextTransaction = conn.Database.BeginTransaction())
                    {
                        foreach (var co in conn.Moviments
                            .Where(w => w.UsuariId == usu.Id && w.TipusMoviment == TipusMoviment.Compra).OrderBy(o => o.Data).ToList())
                        {
                            System.Diagnostics.Debug.WriteLine("co.Id = {0}", co.Id);

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


        /// <summary>
        /// Fa un traspàs de fons.
        /// </summary>
        //[TestMethod]
        public void TraspasFons()
        {
            //return; // Per evitar executar accidentalment. Eliminar aquesta fila per regenerar l ataula.

            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            const int idDel = 2200;
            var files = sessio.Database.ExecuteSqlCommand("DELETE from [DesglosCompres] where [RefCompraId] > " + idDel);
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

        #endregion *** Modifiquen dades ***



        #region *** Test ***

        /// <summary>
        /// Comprova que el desgloç d'unes compres determinades son els esperats.
        /// </summary>
        [TestMethod]
        public void TestDesgloçCompres()
        {
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Joan);

            var compraX = sessio.Moviments.Single(w => w.Id == 41);

            Assert.AreEqual(1, compraX.DesglosCompres.Count(), "Número de files desgloç incorrecte");

            foreach (var desglosCompra in compraX.DesglosCompres)
            {
                if (desglosCompra.RefCompraOrigId == 7)
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
                if (desglosCompra.RefCompraOrigId == 15)
                {
                    //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                    Assert.AreEqual(443.134, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(679.64, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.RefCompraOrigId == 16)
                {
                    //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                    Assert.AreEqual(866.4984, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(547.6451, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.RefCompraOrigId == 17)
                {
                    //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                    Assert.AreEqual(1598.8742, Math.Round(desglosCompra.Participacions, 4));
                    Assert.AreEqual(1010.611, Math.Round(desglosCompra.ParticipacionsOrig, 4));
                }
                else if (desglosCompra.RefCompraOrigId == 27)
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
                if (mdc._DesglosCompra.RefCompraId == 26 && mdc._DesglosCompra.RefCompraOrigId == 17)
                {
                    //Id=24. MovId=26. MovOrigId=17. ParticipacionsDelMoviment=0.347200000000015. ParticipacionsDelMovimentOrigen=2.55975192993167
                    Assert.AreEqual(0.3472, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(2.5598, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.RefCompra.Id == 31 && mdc._DesglosCompra.RefCompraOrig.Id == 15)
                {
                    //Id=41. MovId=31. MovOrigId=15. ParticipacionsDelMoviment=39.354. ParticipacionsDelMovimentOrigen=679.64
                    Assert.AreEqual(39.354, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(679.64, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.RefCompra.Id == 31 && mdc._DesglosCompra.RefCompraOrig.Id == 16)
                {
                    //Id=42. MovId=31. MovOrigId=16. ParticipacionsDelMoviment=76.9523. ParticipacionsDelMovimentOrigen=547.6451
                    Assert.AreEqual(76.9523, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(547.6451, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.RefCompra.Id == 31 && mdc._DesglosCompra.RefCompraOrig.Id == 17)
                {
                    //Id=43. MovId=31. MovOrigId=17. ParticipacionsDelMoviment=141.6462. ParticipacionsDelMovimentOrigen=1008.0512
                    Assert.AreEqual(141.6462, Math.Round(mdc._ParticipacionsDelMoviment, 4));
                    Assert.AreEqual(1008.0512, Math.Round(mdc._ParticipacionsDelMovimentOrigen, 4));
                }
                else if (mdc._DesglosCompra.RefCompra.Id == 31 && mdc._DesglosCompra.RefCompraOrig.Id == 27)
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


        /// <summary>
        /// Compara els preu originals amb el sistema nou i amb l'antic  a nivell de producte.
        /// </summary>
        [TestMethod]
        public void ComparacioPreusCompraOrigAmbStockActual()
        {
            // *** Obligatori perquè funcioni "Usuari.Seleccionat.Id"
            InversionsBDContext sessio = ConnectaBd(Usuari.Usuaris.Carme);

            var data = DateTime.MaxValue;
            //var data = new DateTime(2015, 12, 5);
            double difTot = 0;

            int? prodId = null;
            //prodId = 6;

            if (prodId.HasValue)
            {
                // *** Només un producte.
                var prod = sessio.Productes.Single(w => w.Id == prodId.Value);
                var costAntic = prod.costEnCarteraMetodeAntic(data);
                var costnou = prod.costEnCartera(data);
                var dif = costAntic - costnou;

                System.Diagnostics.Debug.WriteLine("\nProducte: {0}-{1}", prod.Id, prod._NomProducte);
                System.Diagnostics.Debug.WriteLine("Mètode antic = {0}. \nMètode nou = {1}. \n\nDiferència = {2}"
                    , costAntic.ToString("#,##0.00€"), costnou.ToString("#,##0.00€"), dif.ToString("#,##0.00€"));

                difTot = dif;
            }
            else
            {
                // *** Tos els productes del usuari.
                foreach (var prod in sessio.Productes)
                {
                    if (prod.MovimentsProducteUsuari.Any())
                    {
                        var costAntic = prod.costEnCarteraMetodeAntic(data);
                        var costnou = prod.costEnCartera(data);
                        var dif = costAntic - costnou;

                        if (Math.Abs(dif) > .01)
                        {
                            difTot += dif;

                            System.Diagnostics.Debug.WriteLine("\nProducte: {0}-{1}", prod.Id, prod._NomProducte);
                            System.Diagnostics.Debug.WriteLine("Mètode antic = {0}. \tMètode nou = {1}. \tDiferència = {2}"
                                , costAntic.ToString("#,##0.00€"), costnou.ToString("#,##0.00€"), dif.ToString("#,##0.00€"));
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("\nDif total = {0}", difTot);

        }
        

        /// <summary>
        /// Compara els preu originals amb el sistema nou i amb l'antic  a nivell de moviment compra.
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


        #endregion *** Test ***


        #region *** Mètodes ***

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

            Usuari.Seleccionat = sessio.Usuaris.Single(s => s.Id == (int)usuari);

            return sessio;
        }

        #endregion *** Mètodes ***
    }
}
