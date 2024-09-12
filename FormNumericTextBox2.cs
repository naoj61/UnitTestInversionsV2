using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestInversions
{
    public partial class FormNumericTextBox2 : Form
    {
        public FormNumericTextBox2()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button boto = (Button) sender;
            decimal valorDecimal;
            decimal.TryParse(boto.Text, out valorDecimal);
            ntb1.Valor = valorDecimal;
            ntb2.Valor = valorDecimal;
            ntb3.Valor = valorDecimal;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            decimal valorDecimal;
            decimal.TryParse(tb1.Text, out valorDecimal);
            ntb1.Valor = valorDecimal;
            ntb2.Valor = valorDecimal;
            ntb3.Valor = valorDecimal;
        }
    }
}
