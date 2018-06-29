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
using System.Windows.Shapes;

using ZipPlanner.Models;

namespace ZipPlanner.Windows
{
    /// <summary>
    /// Interaction logic for AddSchedule.xaml
    /// </summary>
    public partial class AddSchelude : Window
    {
        public AddSchelude(ArchiveSavedJob job)
        {
            InitializeComponent();
            Title = job.Name;

            DataContext = job;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}
