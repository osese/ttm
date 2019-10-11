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
    public partial class TaskSınırlaPopUp : Form
    {

        private int sure;

        public TaskSınırlaPopUp()
        {
            InitializeComponent();
        }
        public int Get()
        {
            return sure;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            sure = int.Parse(textBox3.Text);
            this.Close();
        }
    }
}
