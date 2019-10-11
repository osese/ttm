using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTM
{
    public partial class TaskYenilePopUp : Form
    {
        private int kere;
        private int interval;
        public TaskYenilePopUp()
        {
            InitializeComponent();
        }

        private void Yenile_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet.TimeIntervals_t' table. You can move, or remove it, as needed.
            this.timeIntervals_tTableAdapter.Fill(this.dataSet.TimeIntervals_t);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            kere = int.Parse(textBox2.Text);
            interval = (int)comboBox1.SelectedValue;
            this.Close();
        }

        public Tuple<int, int> Get()
        {
            return new Tuple<int, int>(kere, interval);
        }
    }
}
