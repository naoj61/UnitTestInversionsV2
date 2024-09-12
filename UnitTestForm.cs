using System;
using System.Threading;
using System.Windows.Forms;
using Inversions.GUI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestInversions
{
    [TestClass]
    public class UnitTestForm
    {
        [TestMethod]
        public void TestFormOpen()
        {
            // Necessitem un fil separat per obrir el formulari i no bloquejar el fil principal del test
            Thread thread = new Thread(() =>
            {
                // Crea una instància del formulari que vols provar
                FormNumericTextBox2 form = new FormNumericTextBox2();

                // Mostra el formulari
                form.ShowDialog();

                // Asserts o verificacions (si cal)
                Assert.IsNotNull(form);
                Assert.AreEqual("UnitTestForm", form.Text);

                // Tanca el formulari després de les verificacions
                form.Close();
            });

            // Necessari per a l'execució de formularis en un fil diferent
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(); // Espera que el fil acabi
        }
    }
}