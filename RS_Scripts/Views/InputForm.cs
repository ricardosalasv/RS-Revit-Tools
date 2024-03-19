using System;
using System.Windows.Forms;

namespace RS_Scripts.Views
{
    public partial class InputForm : Form
    {
        public string UserData { get; private set; }

        public InputForm()
        {
            InitializeComponent();

            this.KeyDown += InputForm_KeyDown;
        }

        private void InputForm_Load(object sender, EventArgs e)
        {
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            ConfirmData();
        }

        private void InputForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if Enter key is pressed
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmData();
            }
        }

        private void ConfirmData()
        {
            // Retrieve data from the form
            UserData = textBox1.Text;
            this.Close();
        }
    }
}
