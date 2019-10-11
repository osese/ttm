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
    public partial class GorevAyrintiForm : Form
    {

        private int id;
        public GorevAyrintiForm(int id)
        {
            InitializeComponent();
            this.id = id;
        }



        private void GorevAyrintiForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet.TaskGroup' table. You can move, or remove it, as needed.
            this.taskGroupTableAdapter.Fill(this.dataSet.TaskGroup);
            // TODO: This line of code loads data into the 'dataSet.TaskStates_t' table. You can move, or remove it, as needed.
            this.taskStates_tTableAdapter.Fill(this.dataSet.TaskStates_t);
            this.tasksTableAdapter1.Fill(this.dataSet.Tasks);

            var task = dataSet.Tasks.FindByid(id);

            textBox1.Text = task.name;
            comboBox2.SelectedValue = task.state_id;
            comboBox1.SelectedValue = task.grup_id;
            trackBar1.Value = task.oncelik;
            richTextBox1.Text = task.notlar;
            maskedTextBox1.Text = task.IstarihNull() ? "": task.tarih.ToShortDateString();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var task = dataSet.Tasks.FindByid(id);
            task.name = textBox1.Text;
            task.state_id = (int)comboBox2.SelectedValue;
            task.grup_id = (int)comboBox1.SelectedValue;
            task.oncelik = trackBar1.Value;
            task.notlar = richTextBox1.Text;
            task.tarih = DateTime.Parse(maskedTextBox1.Text);
            tasksTableAdapter1.Update(task);
            this.Close();
        }
    }
}
