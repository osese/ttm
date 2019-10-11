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
    public partial class TaskGrupForm : Form
    {
        public TaskGrupForm()
        {
            InitializeComponent();
        }

        private void TaskGrupForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet.TaskGroup' table. You can move, or remove it, as needed.
            this.taskGroupTableAdapter.Fill(this.dataSet.TaskGroup);
        }

        private void fillAddForm(DataSet.TaskGroupRow tgr)
        {
            textBox1.Text = tgr.name;
            textBox2.Text = tgr.uzunadi;
            textBox3.Text = tgr.kodu;
            textBox4.Text = tgr.notlar;
            textBox5.Text = tgr.id.ToString();
            checkBox1.Checked = tgr.aktif;
            panel1.Enabled = false;
        }

        private void getFromForm(ref DataSet.TaskGroupRow row)
        {
            row.aktif = true;
            row.kodu = textBox3.Text.Trim().ToUpper();
            row.notlar = textBox4.Text;
            row.uzunadi = textBox2.Text;
            row.ktarih = DateTime.Now;
            row.name = textBox1.Text.ToUpper();
            row.aktif = checkBox1.Checked;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel1.Enabled = true;
        }

        // Kaydet
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox5.Text == "") // new 
            {
                var row = dataSet.TaskGroup.NewTaskGroupRow();
                getFromForm(ref row);
                dataSet.TaskGroup.AddTaskGroupRow(row);
                taskGroupTableAdapter.Update(row);
            }
            else // update
            { 
                var row = dataSet.TaskGroup.FindByid(int.Parse(textBox5.Text));
                getFromForm(ref row);
                taskGroupTableAdapter.Update(row);
            }
            panel1.Enabled = false;
        }

        private void cleartheform()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
        }

        // Yeni Ekle
        private void button2_Click(object sender, EventArgs e)
        {
            cleartheform();
            panel1.Enabled = true;
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var id = comboBox1.SelectedValue;
            if (id != null)
            {
                var tg = dataSet.TaskGroup.FindByid((int)id);
                fillAddForm(tg);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox5.Text != "")
            {
                var result = MessageBox.Show("Silmek istediğinize emin misiniz?","",
                   MessageBoxButtons.OKCancel);
                if (result.HasFlag(DialogResult.OK))
                {
                    var row = dataSet.TaskGroup.FindByid(int.Parse(textBox5.Text));
                    row.Delete();
                    taskGroupTableAdapter.Update(row);
                    cleartheform();
                }
            }

        }
    }
}
