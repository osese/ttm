using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace TTM
{
    /// <summary>
    /// Main form. Kronometreyi, görev crudu gerçekleştirdiğimiz yer.
    /// </summary>
    public partial class Zamanlayıcı : Form
    {
        TimeSpan sayac = new TimeSpan(0, 0, 0);
        TimeSpan step = new TimeSpan(0, 0, 1);
        TimeSpan limit = new TimeSpan(0, 0, 0);
        //state, grup_aktif, grup_adi, yineleme, sinirlama
        string[] currentFilters = new string[6] {"state_id = 3", "grup_aktifmi = true", "", "", "", ""};

        DataSetTableAdapters.YinelemelerTableAdapter yinelemelerTableAdapter;

        SoundPlayer simpleSound = new SoundPlayer(Resource1.uyarises2);
        private TimeSpan? uyarisuregorev = null; // Seçili görevin tahmini süresi
        private TimeSpan uyarisure = new TimeSpan(0, 1, 0); // Seçili görevin tahmini süresi

        private DataSetTableAdapters.GenelDurumTableAdapter geneldurumtableadapter;
        string windowstate = "buyuk";
        int timerstate = 0;

        public Zamanlayıcı()
        {
            InitializeComponent();
            yinelemelerTableAdapter = new DataSetTableAdapters.YinelemelerTableAdapter();
            geneldurumtableadapter = new DataSetTableAdapters.GenelDurumTableAdapter();
            forTaskToplamSureAdapter1.yinelemeleriIsle();
            forTaskToplamSureAdapter1.gecikenleriIsle();
            dataSet.Tasks.TasksRowDeleting += Tasks_TasksRowDeleting;
            gotoState(0);
        }

        private void Tasks_TasksRowDeleting(object sender, DataSet.TasksRowChangeEvent e)
        {
            var row = dataSet.tasks_table_v.FindByid(e.Row.id);
            var sessionsjson = "";
            // TODO sessionun jsonini da koy.
            // var sessions = e.Row.GetSessionRows();
            // JsonConvert.SerializeObject(sessions); // deadline <dbnull> diye hata veriyor. ne alaka
            // arkadaşım session ın içinde deadline mı var yok. Taskla beraber serialize etmeye çalışıyor olabilir.


            DateTime? deadline = row.IsdeadlineNull() ? (DateTime?)null : row.deadline;
            int? tahminisuredk = row.IstahminisuredkNull() ? (int?)null : row.tahminisuredk;
            DateTime? sontarih = row.IssontarihNull() ? (DateTime?)null : row.sontarih;
            var done = (row.state_id == 1 ? true : false);
            taskdeletelogTableAdapter1.Insert(
               row.name, row.kayittarihi, row.notlar, deadline, row.sonsira,
               done, tahminisuredk, row.oncelik, row.saniyecalisma, row.kactane, sontarih, row.grup_adi, sessionsjson);

        }

        // Yeni task panel göster/gizle
        private void button3_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
        }
        // Görevleri göster/gizle
        private void button8_Click(object sender, EventArgs e)
        {
            panel7.Visible = !panel7.Visible;
        }

        /// <summary>
        /// Ana formu zamanlayıcının altındaki kısımdır.
        /// En genel bilgileri gösterir. Toplam grup sayisi toplam görev sayisi vb.
        /// Sadece program açılırken bir kere çalışır. 
        /// <see cref="Zamanlayıcı_Load(object, EventArgs)"/>
        /// </summary>
        private void setGenelDurum()
        {
            var row = this.dataSet.GenelDurum.First();

            label13.Text = row.grupsayisi.ToString() + " tane grup";
            label13.BackColor = Color.AntiqueWhite;

            label14.Text = row.gorevsayisi.ToString() + " tane toplam görev";
            label14.BackColor = Color.GhostWhite;

            label15.Text = row.calismasayisi.ToString() + " tane toplam calişma";
            label15.BackColor = Color.FloralWhite;

            label16.Text = row.tamamlanan.ToString() + " tane tamamlanan görev";
            label16.BackColor = Color.GreenYellow;

            label17.Text = row.tamamlanmayan.ToString() + " tane tamamlanmayan görev";
            label17.BackColor = Color.PaleVioletRed;

            label18.Text = (row.calismasuresi / 60).ToString() + " dakika" + "\nYani " + new TimeSpan(0, 0, row.calismasuresi).ToString();
            label18.BackColor = Color.DodgerBlue;

            label19.Text = "En çok üzerinde çalışılan grup: " + row.encokuzerindecalisilangrup.ToString() + ":" + new TimeSpan(0, 0, row.encokuzerindecalisilangrupsure);
            label19.BackColor = Color.LawnGreen;

            label20.Text = "En az üzerinde çalışılan grup: " + row.enazuzerindecalisilangrup.ToString() + ":" + new TimeSpan(0, 0, row.enazuzerindecalisilangrupsure);
            label20.BackColor = Color.IndianRed;
        }

        // Ana formu en altında bulunan ayrinti kısmı.
        // Grup adının, çalışma süresini ve son çalışma tarihini gösterir.
        private void grupAyrintilariniGoster()
        {
            var tasks = dataSet.tasks_table_v.Where(x => x.grup_adi == comboBox3.Text);
            int total = 0;
            DateTime lasttime = DateTime.MinValue;

            foreach (var task in tasks)
            {
                total += task.saniyecalisma;
                if (!task.IssontarihNull())
                {
                    if (lasttime < task.sontarih)
                    {
                        lasttime = task.sontarih;
                    }
                }
            }
            label22.Text = "Çalışma süresi: " + new TimeSpan(0, 0, total).ToString();
            label23.Text = "Son çalışma: " + lasttime.ToString();

            var tfirst = tasks.FirstOrDefault();
            if (tfirst != null)
            {
                label21.Text = tfirst.grup_adi;
            }
        }

        /// <summary>
        /// Form başlamadan önce, dataset tablolarını doldurur. 
        /// görevlerin listelendiği tabloya ilk filtreyi uygular.
        /// geneldurum raporunu işler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zamanlayıcı_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dataSet.TaskStates_t' table. You can move, or remove it, as needed.
            this.taskStates_tTableAdapter.Fill(this.dataSet.TaskStates_t);

            this.tasks_table_vTableAdapter.Fill(this.dataSet.tasks_table_v);
            this.taskGroupTableAdapter.Fill(this.dataSet.TaskGroup);
            this.sessionTableAdapter.Fill(this.dataSet.Session);
            this.tasksTableAdapter.Fill(this.dataSet.Tasks);
            this.yinelemelerTableAdapter.Fill(this.dataSet.Yinelemeler);
            geneldurumtableadapter.Fill(this.dataSet.GenelDurum);
            
            // Filtereyi uygula
            filtreyiIsle();

            setGenelDurum();
        }


        #region Yeni task paneli
        // kaydet
        private void button1_Click(object sender, EventArgs e)
        {

            var row = this.dataSet.Tasks.NewTasksRow();
            row.name = textBox1.Text;
            DateTime deadline = new DateTime();
            if (DateTime.TryParse(maskedTextBox1.Text, out deadline))
            {
                row.deadline = deadline;
            }
            int timedk = 0;
            if (int.TryParse(textBox4.Text, out timedk))
            {
                row.tahminisuredk = timedk;
            }
            row.notlar = textBox3.Text;
            row.grup_id = (int)comboBox1.SelectedValue;
            row.kayittarihi = DateTime.Now;
            row.sonsira = 1;
            row.state_id = 3;
            row.oncelik = trackBar1.Value;
            if (dateTimePicker1.Enabled)
            {
                row.tarih = dateTimePicker1.Value;
            }

            if (textBox2.Enabled)
            {
                row.limitsure = int.Parse(textBox2.Text);
            }

            if (checkBox1.Checked)
            {
                row.tekseferlik = true;
            }
            this.dataSet.Tasks.AddTasksRow(row);
            this.tasksTableAdapter.Update(row);

            //formu temizle 
            textBox1.Text = "";
            textBox3.Text = "";
            maskedTextBox1.Clear();
            textBox4.Clear();
        }

        // Kapat 
        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        // Yeni task grup ekle 
        private void button5_Click_1(object sender, EventArgs e)
        {
            //textBox2.Visible = true;
            TaskGrupForm tgf = new TaskGrupForm();
            tgf.Show();
        }

        // Öncelik ayar scrollu
        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            int on = trackBar1.Value;
            if (on < 4)
            {
                label12.BackColor = Color.White;
            }
            else if (on < 8)
            {
                label12.BackColor = Color.Yellow;
            }
            else
            {
                label12.BackColor = Color.Red;
            }
            label12.Text = on.ToString();
        }

        // tarihi aç kapa
        private void button9_Click_1(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = !dateTimePicker1.Enabled;
        }
        // limit sureyi aç kapa
        private void button13_Click(object sender, EventArgs e)
        {
            textBox2.Enabled = !textBox2.Enabled;
        }
        #endregion

        #region Timer kontrolleri
        // for each tick update sayac
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.sayac = sayac.Add(step);
            if (limit.TotalSeconds == 0) // Çalışma limiti yoksa normal sayaç.
            {
                textBox5.Text = this.sayac.ToString();
            }
            else // Çalışma limiti varsa ters sayaç.
            {
                textBox5.Text = ((TimeSpan)limit).Subtract(sayac).ToString();
                if (this.sayac.Equals(this.limit))
                {
                    simpleSound.Play();
                    //gotoState(3); // Durdur
                    //gotoState(4); // Kaydet
                }
            }

            if (this.sayac.TotalMinutes % this.uyarisure.TotalMinutes == 0)
            {
                simpleSound.Play();
            }
            else if (this.uyarisuregorev != null && this.sayac == this.uyarisuregorev)
            {
                simpleSound.Play();
                // uyari rengini göster
                textBox5.BackColor = Color.DarkRed;
                textBox5.ForeColor = Color.White;
            }

        }

        // başlat
        private void button5_Click(object sender, EventArgs e)
        {
            gotoState(-1);
        }

        // Bitir
        private void button6_Click(object sender, EventArgs e)
        {
            if (timerstate == 2)
            {
                gotoState(4);
            }
            else if (timerstate == 3)
            {
                gotoState(2);
            }
            else if (timerstate == 1)
            {
                gotoState(2);
            }
        }

        // Durdur/Devam Et
        private void button9_Click(object sender, EventArgs e)
        {
            if (timerstate == 1)
            {
                gotoState(3);
            }
            else if (timerstate == 3)
            {
                gotoState(1);
            }
        }

        // temizle 
        private void temizle()
        {
            //sayaci sıfırla
            this.sayac = new TimeSpan(0, 0, 0);
            //sayac textbox sifirla
            this.textBox5.Text = "00:00:00";

            // Disable buttons 
            durdurBtn.Enabled = false;
            gerialBtn.Enabled = false;
            bitirBtn.Enabled = false;

            // Enable görev/grup seç
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;

            // Çalişma adi ve notunu temizle 
            textBox6.Text = "";
            richTextBox1.Text = "";

            // Enable başlat button
            baslaBtn.Enabled = true;

            // kronometre eski haline dönsün
            textBox5.BackColor = DefaultBackColor;
            textBox5.ForeColor = DefaultForeColor;
        }

        // Geri Al
        private void button7_Click(object sender, EventArgs e)
        {
            gotoState(1);
        }

        #endregion

        #region Timer Küçük buttonlar

        // başlat button ve küçüğü
        private void baslaBtn_EnabledChanged(object sender, EventArgs e)
        {
            button14.Enabled = baslaBtn.Enabled;
        }

        // Küçük kapat butonu
        private void button11_Click(object sender, EventArgs e)
        {
            ActiveForm.Close();
        }
        // bitir buttonla onun küçüğünün eşlenmesi
        private void bitirBtn_TextChanged(object sender, EventArgs e)
        {
            if (bitirBtn.Text == "Bitir")
            {
                button12.Image = Resource1.basasar16;
            }
            else
            {
                button12.Image = Resource1.tik16;
            }
        }

        private void bitirBtn_EnabledChanged(object sender, EventArgs e)
        {
            button12.Enabled = bitirBtn.Enabled;
        }

        // durdur buttonla onun küçüğünün eşlenmesi
        private void durdurBtn_TextChanged(object sender, EventArgs e)
        {
            if (durdurBtn.Text == "Durdur")
            {
                button10.Image = Resource1.stop16;
            }
            else
            {
                button10.Image = Resource1.play16;
            }
        }

        private void durdurBtn_EnabledChanged(object sender, EventArgs e)
        {
            button10.Enabled = durdurBtn.Enabled;
        }
        #endregion

        // küçült/büyült
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.windowstate == "buyuk")
            {
                ActiveForm.WindowState = FormWindowState.Normal;
                ActiveForm.Width = 200;
                ActiveForm.Height = 100;
                ActiveForm.FormBorderStyle = FormBorderStyle.None;
                ActiveForm.TopMost = true;
                ActiveForm.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 200, Screen.PrimaryScreen.Bounds.Height - 100);

                panel1.Hide();
                panel2.Hide();
                panel3.Hide();
                //panel4.Hide();
                panel5.Hide();
                panel6.Hide();
                panel7.Hide();
                panel4.Parent = ActiveForm;
                this.windowstate = "kucuk";
            }
            else
            {
                ActiveForm.Width = 800;
                ActiveForm.Height = 600;
                ActiveForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                ActiveForm.TopMost = false;
                ActiveForm.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - 400, Screen.PrimaryScreen.Bounds.Height / 2 - 300);

                panel2.Show();
                panel3.Show();
                //panel4.Hide();
                panel5.Show();
                panel6.Show();

                panel4.Parent = panel3;
                this.windowstate = "buyuk";
            }
        }


        /// <summary>
        /// This function change the state of the zamanlayıcı. 
        /// </summary>
        /// <param name="i"></param>
        private void gotoState(int i)
        {
            Console.WriteLine("State: " + i);
            if (i == 0) // state 0 'initial'. sadece başlayabilir.
            {
                durdurBtn.Enabled = false;
                bitirBtn.Enabled = false;
                baslaBtn.Enabled = true;
                gerialBtn.Enabled = false;
                bitirBtn.Text = "Bitir";
                durdurBtn.Text = "Durdur";
                timerstate = 0;
            }
            else if (i == -1) // state -1 'başladı'. ara state 
            {

                if (comboBox2.SelectedIndex > -1)
                {
                    var id = comboBox2.SelectedValue;

                    // Görev timerı ve çalışma adı ayarla.
                    try
                    {
                        seciliGorevDegisti((int)id);
                    }
                    catch (GorevTamamlanmisException eex)
                    {
                        // Görev tamamlanmışsa hata ver. 
                        MessageBox.Show(eex.Message);
                        gotoState(0);
                        return;
                    }

                    //Create Session
                    var notlar = "";
                    var task = dataSet.Tasks.FindByid((int)comboBox2.SelectedValue);
                    var sira = task.sonsira;
                    task.sonsira += 1;
                    // Update tasks
                    tasksTableAdapter.Update(task);

                    //insert row
                    var row = dataSet.Session.NewSessionRow();
                    row.notlar = notlar;
                    row.task_id = (int)comboBox2.SelectedValue;
                    row.tarih = DateTime.Now;
                    row.sira = sira;
                    row.name = textBox6.Text;
                    row.sure = 0;
                    dataSet.Session.AddSessionRow(row);
                    sessionTableAdapter.Update(row);
                    // rowun idsini sayaç ın üzerine yapıştıralım. 
                    textBox5.Tag = row.id;

                    // Disable görev/grup seç
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;

                    // uyari suresini ayarla
                    if (!task.IstahminisuredkNull())
                    {
                        uyarisuregorev = new TimeSpan(0, task.tahminisuredk, 0);
                    }

                    gotoState(1);
                }

            }
            else if (i == 1) // state 1 'başladı'. durdurabilir ya da bitirebilir.
            {
                timer1.Start();

                bitirBtn.Enabled = true;
                durdurBtn.Enabled = true;
                baslaBtn.Enabled = false;
                gerialBtn.Enabled = false;
                bitirBtn.Text = "Bitir";
                durdurBtn.Text = "Durdur";
                timerstate = 1;
            }
            else if (i == 2) // state 2 'bitire basıldı'. geri alabilir ya da kaydetebilir.
            {
                timer1.Stop();
                bitirBtn.Text = "Kaydet";
                timerstate = 2;
                gerialBtn.Enabled = true;
                bitirBtn.Enabled = true;
                durdurBtn.Enabled = false;
            }
            else if (i == 3)   // state 3 'durduruldu'. devam eder ya da bitirebilir.
            {
                timer1.Stop();
                durdurBtn.Text = "Devam Et";

                durdurBtn.Enabled = true;
                bitirBtn.Enabled = true;
                baslaBtn.Enabled = false;
                gerialBtn.Enabled = false;
                timerstate = 3;

            }
            else if (i == 4) // state 4 'kaydete basıldı'.
            {
                // Update last session
                // sayacin uzerindeki rowun idsinden son çalışma bulunur.
                int id = (int)textBox5.Tag;
                var last = dataSet.Session.FindByid(id);
                last.sure = (int)this.sayac.TotalSeconds;
                last.name = textBox6.Text;
                last.notlar = richTextBox1.Text;
                last.btarih = DateTime.Now;
                sessionTableAdapter.Update(last);

                if (last.TasksRow.tekseferlik)
                {
                    last.TasksRow.state_id = 1;
                    tasksTableAdapter.Update(last.TasksRow);
                }
                // Etrafı temizle.
                temizle();
                bitirBtn.Text = "Bitir";

                timerstate = 4;
                gotoState(0); // initial state'e gidilir.
            }
        }



        // formu kapatmadan
        private void Zamanlayıcı_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timerstate != 0) // herhangi bir çalışma başlatıldıysa.
            {
                // sureyi kaydet ve çık.
                // Update last session
                var last = sessionTableAdapter.GetData().Last();
                last.sure = (int)this.sayac.TotalSeconds;
                last.name = textBox6.Text;
                last.notlar = richTextBox1.Text;
                last.btarih = DateTime.Now;
                sessionTableAdapter.Update(last);
            }
        }

        
        // Format sure column
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name.Equals("saniyecalisma"))
            {

                if (e.Value != null)
                {
                    if (e.Value.GetType() == typeof(Int32))
                    {

                        int totalsaniye = Convert.ToInt32(e.Value);
                        //Time span günlük olan da hata verirse bu kullanılabilir.
                        //int saat = totalsaniye / 3600;
                        //int dakika = (totalsaniye - (saat * 3600)) / 60;
                        //int saniye = totalsaniye - (saat * 3600) - (dakika * 60);
                        e.Value = new TimeSpan(0, 0, totalsaniye);
                    }
                    else
                    {
                        e.Value = "00:00:00";
                    }
                }
                else
                {
                    e.Value = "00:00:00";
                }
            }
        }

        #region Görevler tablosu context menu
        private void dataGridView1_RowContextMenuStripNeeded(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
        {
            //cms_row = (sender as DataGridView).Rows[e.RowIndex];
            (sender as DataGridView).Rows[e.RowIndex].Selected = true;
            contextMenuStrip1.Show(MousePosition);

        }
        private void gorevleriCombineLa()
        {
            var selectedRows = dataGridView1.SelectedRows;
            List<String> names = new List<string>();
            var notlar = "";


            foreach (DataGridViewRow sr in selectedRows)
            {
                var item = (DataSet.tasks_table_vRow)((DataRowView)sr.DataBoundItem).Row;
                names.Add(item.name);
                notlar += item.notlar + "\n";

            }


            var gb = new GorevleriBirlestir(names);
            gb.ShowDialog();
            
            var tuple = gb.Get();
            // Birleştirilmemişse çık. 
            if (tuple.Item1 == null || tuple.Item1 == "")
                return;
            var row = dataSet.Tasks.NewTasksRow();

            row.name = tuple.Item1;
            row.grup_id = tuple.Item2;
            row.notlar = notlar;
            row.oncelik = 0;
            row.state_id = 3;
            row.kayittarihi = DateTime.MaxValue;

            var sonsira = 0;
            var tahminisuredk = 0;
            foreach (DataGridViewRow sr in selectedRows)
            {
                var item = (DataSet.tasks_table_vRow)((DataRowView)sr.DataBoundItem).Row;

                if (item.oncelik > row.oncelik)
                {
                    row.oncelik = item.oncelik;
                    sonsira += item.sonsira;
                    tahminisuredk += item.IstahminisuredkNull() ? 0 : item.tahminisuredk;
                }
                if (item.kayittarihi < row.kayittarihi)
                {
                    row.kayittarihi = item.kayittarihi;
                }
            }
            row.sonsira = sonsira;
            row.tahminisuredk = tahminisuredk;

            dataSet.Tasks.AddTasksRow(row);
            tasksTableAdapter.Update(row);
            foreach (DataGridViewRow sr in selectedRows)
            {
                var item = (DataSet.tasks_table_vRow)((DataRowView)sr.DataBoundItem).Row;
                var asilitem = dataSet.Tasks.FindByid(item.id);
                asilitem.Delete();
                tasksTableAdapter.SessionlariBanaBagla(row.id, item.id);
            }

            
            // Update after delete
            tasksTableAdapter.Update(dataSet.Tasks);
            tasks_table_vTableAdapter.Fill(dataSet.tasks_table_v);
        }
        
        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var selectedRows = dataGridView1.SelectedRows;
            if (e.ClickedItem.Text == "Tamam")
            {
                foreach (DataGridViewRow row in selectedRows)
                {
                    var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                    task.state_id = 1;
                    task.bitistarihi = DateTime.Now;
                }
            }
            else if (e.ClickedItem.Text == "Beklemede")
            {
                foreach (DataGridViewRow row in selectedRows)
                {
                    var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                    task.state_id = 3;
                }
            }
            else if (e.ClickedItem.Text == "İptal")
            {
                foreach (DataGridViewRow sr in selectedRows)
                {
                    var task = dataSet.Tasks.FindByid((int)sr.Cells[0].Value);
                    task.state_id = 2;
                }
            }
            else if (e.ClickedItem.Text == "Sil")
            {
                contextMenuStrip1.Hide();


                var result = MessageBox.Show(selectedRows.Count + "", " tane görevi silmek istediğinize emin misiniz?",
                    MessageBoxButtons.OKCancel);

                if (result.HasFlag(DialogResult.OK))
                {
                    var bazirowlarincalismasivar = false;
                    foreach (DataGridViewRow row in selectedRows)
                    {
                        int count = (int)tasksTableAdapter.SessionCount((int)row.Cells[0].Value);
                        if (count > 0)
                        {
                            bazirowlarincalismasivar = true;
                        }
                    }

                    if (bazirowlarincalismasivar)
                    {
                        result = MessageBox.Show("Çalışması olduğu için bu görev silinemez. İlla silmek istiyorsan evete tıkla!!", "",
                           MessageBoxButtons.OKCancel);
                        if (!result.HasFlag(DialogResult.OK))
                            return;
                    }
                    foreach (DataGridViewRow row in selectedRows)
                    {
                        var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                        task.Delete();
                    }
                }
            }
            else if (e.ClickedItem.Text == "Yinele")
            {
                contextMenuStrip1.Hide();
                TaskYenilePopUp ty = new TaskYenilePopUp();
                ty.ShowDialog();
                var res = ty.Get();
                foreach (DataGridViewRow row in selectedRows)
                {
                    var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                    if (task.IstarihNull())
                    {
                        MessageBox.Show("Görevin atandığı bir tarih olmadığı için yinelenemez.");
                        dataSet.Yinelemeler.RejectChanges();
                        return;
                    }
                    if (task.yinelemevarmi)
                    {
                        MessageBox.Show("Görevin zaten bir yinelemesi mevcut.");
                        dataSet.Yinelemeler.RejectChanges();
                        return;
                    }
                    var yinelemerow = dataSet.Yinelemeler.NewYinelemelerRow();
                    yinelemerow.aralik = res.Item1;
                    yinelemerow.aralik_id = res.Item2;
                    yinelemerow.task_id = task.id;
                    yinelemerow.baslangic = DateTime.Now;
                    task.yinelemevarmi = true;
                    dataSet.Yinelemeler.AddYinelemelerRow(yinelemerow);
                }
                tasksTableAdapter.Update(dataSet.Tasks);
                yinelemelerTableAdapter.Update(dataSet.Yinelemeler);

            }
            else if (e.ClickedItem.Text == "Yinelemeyi Durdur")
            {
                
                foreach (DataGridViewRow row in selectedRows)
                {
                    var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                    var yinelemerow = dataSet.Yinelemeler.Where(x => x.task_id == task.id).FirstOrDefault();
                    yinelemerow.Delete();
                    task.yinelemevarmi = false;
                }
                tasksTableAdapter.Update(dataSet.Tasks);
                yinelemelerTableAdapter.Update(dataSet.Yinelemeler);
            }
            else if (e.ClickedItem.Text == "Sınırla")
            {
                TaskSınırlaPopUp ts = new TaskSınırlaPopUp();
                ts.ShowDialog();
                var res = ts.Get();
                foreach (DataGridViewRow row in selectedRows)
                {
                    var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                    task.limitsure = res;
                }
            }
            else if (e.ClickedItem.Text == "Birleştir")
            {
                gorevleriCombineLa();
                return;
            }else if(e.ClickedItem.Text == "Çalış")
            {
                if(selectedRows.Count == 1)
                {
                    DataGridViewRow row = selectedRows[0];
                    var task = dataSet.Tasks.FindByid((int)row.Cells[0].Value);
                    comboBox3.SelectedValue = task.grup_id;
                    //comboBox2.SelectedValue = task.id;
                }
                return;
            }
            tasksTableAdapter.Update(dataSet.Tasks);
            tasks_table_vTableAdapter.Fill(dataSet.tasks_table_v);
        }



        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            // En sondaki row, veri taşımadığı onu boyamıyoruz/boyayamıyoruz.
            if (e.IsLastVisibleRow) { return; }

            if ((int)dataGridView1.Rows[e.RowIndex].Cells["state_id"].Value == 1)
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;

            }
            else if ((bool)dataGridView1.Rows[e.RowIndex].Cells["yinelemevarmi"].Value == true)
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Gainsboro;
            }

            else if(dataGridView1.Rows[e.RowIndex].Cells["limitsure"].Value.GetType() != typeof(DBNull))
            {
                if ((int)dataGridView1.Rows[e.RowIndex].Cells["limitsure"].Value > 0)
                {
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Purple;
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.AntiqueWhite;
                }
            }
           

        }
        #endregion


        #region Görevler tablosu toolstrip buttonları
        // data table yenile butonu
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            tasks_table_vTableAdapter.Fill(dataSet.tasks_table_v);
        }


        // data table grup aktif/inaktif filter.
        private void datatable_filtercheckboxes_CheckedChanged(object sender, EventArgs e)
        {
            //currentFilters[1] = "grup_aktifmi = " + !checkBox2.Checked;
            var tag = (string)((ToolStripMenuItem)sender).Tag;
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
            if (tag == "yinelenen")
            {
                if (yinelenenlerToolStripMenuItem.Checked)
                {
                    currentFilters[3] = "yinelemevarmi = true";
                }else
                {
                    currentFilters[3] = "";
                }
            }
            else if(tag == "sinirlanan")
            {
                if (sınırlananlarToolStripMenuItem.Checked)
                {
                    currentFilters[4] = "limitsure > 0";
                }
                else
                {
                    currentFilters[4] = "";
                }
            }
            else if(tag == "pasifgrup")
            {
                currentFilters[2] = pasifGruplarToolStripMenuItem.Checked ? "grup_aktifmi = false" : "grup_aktifmi = true";
            }
            filtreyiIsle();
        }

        private void filtererToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var statefilter = "";

            var tag = (string)((ToolStripMenuItem)sender).Tag;
            var text = ((ToolStripMenuItem)sender).Text;

            if (tag == "tamam")
            {
                statefilter = "state_id = 1";
            }
            else if (tag == "iptal")
            {
                statefilter = "state_id = 2";
            }
            else if (tag == "gecik")
            {
                statefilter = "state_id = 4";
            }else if(tag == "bekle")
            {
                statefilter = "state_id = 3";
            }

            toolStripSplitButton1.Text = text;
            currentFilters[0] = statefilter;

            filtreyiIsle();
        }

        private void filtreyiIsle()
        {
            var filter = "";
            for(var i = 0; i < currentFilters.Length; i++)
            {
                if(currentFilters[i] != "")
                {
                    filter += currentFilters[i] + " and ";
                }
            }

            filter = filter.TrimEnd(new char[] {'a','n','d',' '});
            taskstablevBindingSource.Filter = filter;
        }
        #endregion

        #region Görev başlamadan önceki zamanlayıcı, ve çalışma adı ayarları
        // Görevin zamanlayıcı tavrını ve çalışma adını ayarlar.
        private void seciliGorevDegisti(int gorevid)
        {
            var t = dataSet.Tasks.FindByid(gorevid);

            string calisma_adi = calismaAdiUret(t);
            textBox6.Focus();
            textBox6.Text = calisma_adi;
            textBox6.SelectAll();
            limit = gorevTipineGoreLimitGetir(t);
        }
        private TimeSpan gorevTipineGoreLimitGetir(DataSet.TasksRow t)
        {
            if (!t.IslimitsureNull() && t.limitsure != 0)
            {
                var limit = new TimeSpan(0, t.limitsure, 0);
                var gecensaniye = tasksTableAdapter.ToplamSure(t.id);
                gecensaniye = gecensaniye != null ? gecensaniye : 0;
                var gecen = new TimeSpan(0, 0, (int)gecensaniye);
                limit = limit.Subtract(gecen);
                if (limit.TotalMilliseconds <= 0)
                {
                    throw new GorevTamamlanmisException("Bu görev verilen sınıra ulaşmıştır.");
                }
                return limit;

            }
            return new TimeSpan(0, 0, 0);
        }

        private string calismaAdiUret(DataSet.TasksRow t)
        {
            var kactane = tasksTableAdapter.SessionCount(t.id);
            return t.name + "-" + ((int)kactane + 1);
        }
        #endregion

        
        
        // Grup seçilince görevleri filtrele ve grup ayrıntılarını göster.
        private void comboBox3_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var grup_id = comboBox3.SelectedValue;
            tasksBindingSource1.Filter = "(state_id = 3 or state_id = 4) and grup_id = " + grup_id;
            panel7.Hide();
            grupAyrintilariniGoster();
        }


        // Görevin üzerine gelindiğinde notlar gözükecek.
        private void comboBox2_MouseHover(object sender, EventArgs e)
        {
            var id = comboBox2.SelectedValue;
            if (id != null)
            {
                var task = dataSet.Tasks.FindByid((int)id);
                toolTip1.SetToolTip(comboBox2, task.notlar);
            }
        }




        #region Başka pencerede açılan formlar
        // Günün Özeti formu
        private void button6_Click_1(object sender, EventArgs e)
        {
            GununOzeti go = new GununOzeti();
            go.Show();
        }


        // Ayarlar  formu
        private void button7_Click_1(object sender, EventArgs e)
        {
            Ayarlar a = new Ayarlar();
            a.Show();
        }

        // Taskı düzenle formu
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.GetRowCount(DataGridViewElementStates.Visible))
                return;
            if (dataGridView1.Rows[e.RowIndex].DataBoundItem == null)
            {
                return;
            }

            var id = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            GorevAyrintiForm gaf = new GorevAyrintiForm(id);
            gaf.ShowDialog();
            this.tasks_table_vTableAdapter.Fill(dataSet.tasks_table_v);
        }

        #endregion


        #region Görevler tablosu arama
        private string gelismisaramaFiltrecisi(string tag, string param)
        {
            var grupabbr = new string[] { "grup", "g", "gruplar", "gr", "grupadi" };
            var gorevabbr = new string[] { "gorev", "task", "t", "görev", "tasks", "name", "n"};
            var tarihabbr = new string[] { "tarih", "date" };

            string filter = "";
            if (grupabbr.Contains(tag.ToLower()))
            {
                filter = $"grup_adi LIKE '%{param.ToUpper().Trim(' ')}%'";
            }
            else if (gorevabbr.Contains(tag.ToLower()))
            {
                filter = $"name LIKE '%{param.ToUpper().Trim(' ')}%'";
            }else if (tarihabbr.Contains(tag.ToLower()))
            {
                DateTime o = new DateTime();
                param = param.Trim(' ');
                if (DateTime.TryParse(param, out o))
                {
                    filter = $"tarih = '{o.ToShortDateString()}'";
                }
                else if( param == "bugün")
                {
                    filter = $"tarih = '{DateTime.Now.ToShortDateString()}'";
                }else if(param == "yarın")
                {
                    filter = $"tarih = '{DateTime.Now.AddDays(1).ToShortDateString()}'";
                }else if(param == "dün")
                {
                    filter = $"tarih = '{DateTime.Now.AddDays(-1).ToShortDateString()}'";
                }

            }
            return filter;
        }

        private void aramaFilteresi_Bekleniyor()
        {
            var filter = "(";
            var text = toolStripTextBox1.Text;
            var arr = text.Split(':');

            if (arr.Length == 1)
            {
                filter += $"grup_adi LIKE '%{text}%' or name LIKE '%{text}%'";
            }
            else
            {
                for (int i = 0; i < arr.Length; i += 2)
                {
                    filter += gelismisaramaFiltrecisi(arr[0], arr[1]) + " or ";
                }
            }
            filter = filter.TrimEnd(new char[] { 'o', 'r', ' ' }) + ')';
            currentFilters[5] = filter;
            filtreyiIsle();
        }

        private void toolStripTextBox1_Validated(object sender, EventArgs e)
        {
            aramaFilteresi_Bekleniyor();
        }
        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // ding soundu kapa
                e.Handled = true;
                e.SuppressKeyPress = true;
                aramaFilteresi_Bekleniyor();
            }
        }
        #endregion


    }
}
