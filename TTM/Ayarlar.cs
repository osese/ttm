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
    public partial class Ayarlar : Form
    {
        DataSetTableAdapters.TasksTableAdapter tasks_adapter;
        DataSetTableAdapters.SessionTableAdapter session_adapter;
        DataSetTableAdapters.TaskGroupTableAdapter group_adapter;

        public Ayarlar()
        {
            InitializeComponent();
            tasks_adapter = new DataSetTableAdapters.TasksTableAdapter();
            group_adapter = new DataSetTableAdapters.TaskGroupTableAdapter();
            session_adapter = new DataSetTableAdapters.SessionTableAdapter();
        }

       
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbg= new FolderBrowserDialog();
            var result = fbg.ShowDialog();
            if(result == DialogResult.OK)
            {
                string xml = dataSet.GetXml();
                string schema = dataSet.GetXmlSchema();
                var path = System.IO.Path.Combine(fbg.SelectedPath, DateTime.Today.ToShortDateString() + ".xml");
                var pathschema = System.IO.Path.Combine(fbg.SelectedPath, DateTime.Today.ToShortDateString() + ".xsd");
                System.IO.File.WriteAllText(path, xml);
                System.IO.File.WriteAllText(pathschema, schema);
            }
        }

        private void Ayarlar_Load(object sender, EventArgs e)
        {
            tasks_adapter.Fill(dataSet.Tasks);
            group_adapter.Fill(dataSet.TaskGroup);
            session_adapter.Fill(dataSet.Session);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
