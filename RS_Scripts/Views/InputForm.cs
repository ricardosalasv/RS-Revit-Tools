using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RS_Scripts.Views
{
    public partial class InputForm : Form
    {
        public string UserData { get; private set; }

        public InputForm()
        {
            InitializeComponent();
        }

        private void InputForm_Load(object sender, EventArgs e)
        {
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // Retrieve data from the form
            UserData = textBox1.Text;
            this.Close();
        }
    }
}
