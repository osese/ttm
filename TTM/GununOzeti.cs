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
    public partial class GununOzeti : Form
    {
        /// <summary>
        /// Haftalık grafikler ve günlük durum raporlarını gösteren form.
        /// </summary>
        public GununOzeti()
        {
            InitializeComponent();
        }

        private void GununOzeti_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'raporlar.raporTarihXSure_ve_Calisma' table. You can move, or remove it, as needed.
            this.raporTarihXSure_ve_CalismaTableAdapter.Fill(this.raporlar.raporTarihXSure_ve_Calisma);
            ayrintilar(DateTime.Now);
            // başlangıçta 7 günlük raporları getir.
            charting(7);
        }

        private void ayrintilar(DateTime dt)
        {
            gunlukOzetTableAdapter1.Fill(raporlar.GunlukOzet, dt);
            var row = raporlar.GunlukOzet.First();

            label5.Text = row.IstoplamsuresnNull() ? "0" : new TimeSpan(0, 0, row.toplamsuresn).ToString();
            label6.Text = row.tamamlanan.ToString();
            label8.Text = row.toplamcalisma.ToString();
            listBox1.DataSource = row.IsgorevadlariNull() ? new string[] { }:row.gorevadlari.Split('|');
        }
        private void charting(int x)
        {   
            getToplamSureToplamCalismaWithGroupTableAdapter.Fill(raporlar.GetToplamSureToplamCalismaWithGroup, x);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int value = 7;
            if (int.TryParse(toolStripTextBox1.Text, out value))
            {
                charting(value);
                this.chart1.DataBind();
            }
            chart2.Series[0].Enabled = false;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ayrintilar(dateTimePicker1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = dateTimePicker1.Value.AddDays(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = dateTimePicker1.Value.AddDays(-1);
        }
    }
}
