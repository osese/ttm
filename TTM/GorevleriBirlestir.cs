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
    public partial class GorevleriBirlestir : Form
    {
        private string name = null;
        private int grup_id;
        public GorevleriBirlestir(List<String> list)
        {
            InitializeComponent();
            listBox1.DataSource = list;
            var text = "";
            foreach(var item in list)
            {
                text += item;
            }
            textBox1.Text = text;
        }

        public Tuple<string ,int> Get()
        {
            return new Tuple<string, int>(name, grup_id);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            name = textBox1.Text;
            grup_id = (int)comboBox1.SelectedValue;
            this.Close();
        }

        private void GorevleriBirlestir_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet.TaskGroup' table. You can move, or remove it, as needed.
            this.taskGroupTableAdapter.Fill(this.dataSet.TaskGroup);

        }
    }
}
