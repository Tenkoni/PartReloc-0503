using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PartReloc
{
    public partial class Form2 : Form
    {
        private Form1 mainform = null;

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form calling)
        {
            mainform = calling as Form1;
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) //método para evitar entrada de símbolos indeseados
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)  
        {
            int.TryParse(textBox1.Text, out int size);
            if (size < 500) size = 500;
            this.mainform.memorysz = size;
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
