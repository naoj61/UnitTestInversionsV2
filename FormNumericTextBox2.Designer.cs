namespace UnitTestInversions
{
    partial class FormNumericTextBox2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ntb1 = new Controls.NumericTextBox2();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ntb3 = new Controls.NumericTextBox2();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ntb2 = new Controls.NumericTextBox2();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.tb1 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.dataGridView31 = new Controls.DataGridView3();
            this.Text = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Num = new Controls.NumericTextBoxColumn2();
            this.Num2 = new Controls.NumericTextBoxColumn2();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView31)).BeginInit();
            this.SuspendLayout();
            // 
            // ntb1
            // 
            this.ntb1._CapturaEscape = true;
            this.ntb1._Format = "0.# €";
            this.ntb1._NegatiusEnVermell = true;
            this.ntb1._PermetDecimals = true;
            this.ntb1._PermetNegatius = true;
            this.ntb1._PermetTextNull = false;
            this.ntb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ntb1.ForeColor = System.Drawing.Color.Black;
            this.ntb1.Location = new System.Drawing.Point(3, 22);
            this.ntb1.Name = "ntb1";
            this.ntb1.Size = new System.Drawing.Size(218, 26);
            this.ntb1.TabIndex = 0;
            this.ntb1.Text = "0 €";
            this.ntb1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ntb1.Valor = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ntb1);
            this.groupBox1.Location = new System.Drawing.Point(46, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(224, 55);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "€-Negatius-Vermell-Decimals";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ntb3);
            this.groupBox2.Location = new System.Drawing.Point(558, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(106, 55);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "€-Decimals";
            // 
            // ntb3
            // 
            this.ntb3._CapturaEscape = true;
            this.ntb3._Format = "0.# €";
            this.ntb3._NegatiusEnVermell = false;
            this.ntb3._PermetDecimals = true;
            this.ntb3._PermetNegatius = false;
            this.ntb3._PermetTextNull = false;
            this.ntb3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ntb3.ForeColor = System.Drawing.Color.Black;
            this.ntb3.Location = new System.Drawing.Point(3, 22);
            this.ntb3.Name = "ntb3";
            this.ntb3.Size = new System.Drawing.Size(100, 26);
            this.ntb3.TabIndex = 0;
            this.ntb3.Text = "0 €";
            this.ntb3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ntb3.Valor = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ntb2);
            this.groupBox3.Location = new System.Drawing.Point(302, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(224, 55);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "€-Negatius-Decimals";
            // 
            // ntb2
            // 
            this.ntb2._CapturaEscape = true;
            this.ntb2._Format = "0.# €";
            this.ntb2._NegatiusEnVermell = false;
            this.ntb2._PermetDecimals = true;
            this.ntb2._PermetNegatius = true;
            this.ntb2._PermetTextNull = false;
            this.ntb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ntb2.ForeColor = System.Drawing.Color.Black;
            this.ntb2.Location = new System.Drawing.Point(3, 22);
            this.ntb2.Name = "ntb2";
            this.ntb2.Size = new System.Drawing.Size(218, 26);
            this.ntb2.TabIndex = 0;
            this.ntb2.Text = "0 €";
            this.ntb2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ntb2.Valor = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(81, 104);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(116, 38);
            this.button1.TabIndex = 3;
            this.button1.Text = "123";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(219, 104);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 38);
            this.button2.TabIndex = 3;
            this.button2.Text = "123,485";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(360, 104);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(116, 38);
            this.button3.TabIndex = 3;
            this.button3.Text = "-12,654 ~€";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button_Click);
            // 
            // tb1
            // 
            this.tb1.Location = new System.Drawing.Point(81, 178);
            this.tb1.Name = "tb1";
            this.tb1.Size = new System.Drawing.Size(100, 26);
            this.tb1.TabIndex = 4;
            this.tb1.Text = "0";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(210, 172);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(116, 38);
            this.button4.TabIndex = 3;
            this.button4.Text = "<---";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // dataGridView31
            // 
            this.dataGridView31.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView31.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Text,
            this.Num,
            this.Num2});
            this.dataGridView31.Location = new System.Drawing.Point(49, 259);
            this.dataGridView31.Name = "dataGridView31";
            this.dataGridView31.RowTemplate.Height = 28;
            this.dataGridView31.Size = new System.Drawing.Size(339, 177);
            this.dataGridView31.TabIndex = 5;
            // 
            // Text
            // 
            this.Text.HeaderText = "Text";
            this.Text.Name = "Text";
            this.Text.ToolTipText = "qwe";
            // 
            // Num
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "C2";
            dataGridViewCellStyle1.NullValue = null;
            this.Num.DefaultCellStyle = dataGridViewCellStyle1;
            this.Num.HeaderText = "Num";
            this.Num.Name = "Num";
            // 
            // Num2
            // 
            this.Num2.HeaderText = "Num2";
            this.Num2.Name = "Num2";
            // 
            // FormNumericTextBox2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 498);
            this.Controls.Add(this.dataGridView31);
            this.Controls.Add(this.tb1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Name = "FormNumericTextBox2";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView31)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.NumericTextBox2 ntb1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private Controls.NumericTextBox2 ntb3;
        private System.Windows.Forms.GroupBox groupBox3;
        private Controls.NumericTextBox2 ntb2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox tb1;
        private System.Windows.Forms.Button button4;
        private Controls.DataGridView3 dataGridView31;
        private System.Windows.Forms.DataGridViewTextBoxColumn Text;
        private Controls.NumericTextBoxColumn2 Num;
        private Controls.NumericTextBoxColumn2 Num2;
    }
}