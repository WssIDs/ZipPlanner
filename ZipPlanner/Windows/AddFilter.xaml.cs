using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace ZipPlanner.Windows
{
    /// <summary>
    /// Interaction logic for AddFiler.xaml
    /// </summary>
    public partial class AddFilter : Window
    {
        ObservableCollection<string> filters;

        public AddFilter(ObservableCollection<string> filter)
        {
            InitializeComponent();

            filters = filter;
            DataContext = filters;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!filters.Contains(filter_txt.Text))
            {
                filters.Add(filter_txt.Text);
                DialogResult = true;
                Close();
            }
            else
            {
                DialogResult = false;
                MessageBox.Show("Невозможно добавить дважды одинаковый фильтр");
            }
        }
    }
}
