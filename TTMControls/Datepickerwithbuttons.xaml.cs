using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TTMControls
{
    /// <summary>
    /// Interaction logic for Datepickerwithbuttons.xaml
    /// </summary>
    public partial class Datepickerwithbuttons : UserControl
    {
        public Datepickerwithbuttons()
        {
            InitializeComponent();
        }

        private void Ileri_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(datePicker.DisplayDate);
            datePicker.SelectedDate = datePicker.SelectedDate.Value.AddDays(1);
            
        }

        private void Geri_Click(object sender, RoutedEventArgs e)
        {
            datePicker.SelectedDate = datePicker.SelectedDate.Value.AddDays(-1);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            datePicker.SelectedDate = DateTime.Now;
        }
    }
}
