//@author: Mateusz779
using System;
using System.Windows.Forms;

namespace szyfrowanie_plikow
{
    public partial class Dec : Form
    {
        public string passwd { get; set; }
        public Dec()
        {
            InitializeComponent();
            textBox1.PasswordChar = '*';
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            if (!string.IsNullOrWhiteSpace(textBox1.Text)|| textBox1.Text.Length<8)
            {
                passwd = textBox1.Text;
                this.Close();
            }
            else
                MessageBox.Show("Incorrect password! Password must be at least 8 characters long!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
