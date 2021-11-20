//@author: Mateusz779
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace szyfrowanie_plikow
{
    public partial class Enc : Form
    {
        List<TextBox> textBoxes = new List<TextBox>();
        public string[] passwd = new string[8];
        int val = 1;
        bool err = false;
        public Enc()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            TextBox textBox = new TextBox();
            textBox.Name = "text_" + numericUpDown1.Value.ToString();
            textBox.PasswordChar = '*';
            textBoxes.Add(textBox);
            tableLayoutPanel1.Controls.Add(textBox);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            err = false;
            int a = 0;
            foreach(var i in textBoxes)
            {
                if (!string.IsNullOrWhiteSpace(i.Text)){
                    passwd[a] = i.Text;
                    a++;
                }
                if(i.Text.Length<8 && i.Text.Length!=0)
                {
                    err = true;
                    MessageBox.Show("Incorrect password! Password must be at least 8 characters long!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            if(!err)
            {
                this.DialogResult = DialogResult.OK;
                try
                {
                    this.Close();
                }
                catch { }
                err = false;
            }
                
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(val < ((NumericUpDown)sender).Value)
            {
                TextBox textBox = new TextBox();
                textBox.Name = "text_" + numericUpDown1.Value.ToString();
                textBox.PasswordChar = '*';
                textBoxes.Add(textBox);
                tableLayoutPanel1.Controls.Add(textBox);
                val++;
            }
            else
            {
                val--;
                tableLayoutPanel1.Controls.Remove(textBoxes.ToArray()[textBoxes.Count-1]);
                textBoxes.RemoveAt(textBoxes.Count-1);
            }

            
        }
    }
}
