using System;
using System.Collections;
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
using Quartz;
using Quartz.Impl;
using ZipPlanner.Models;

namespace ZipPlanner.Windows
{
    /// <summary>
    /// Interaction logic for AddSchedule.xaml
    /// </summary>
    public partial class AddSchedule : Window
    {
        ArchiveSavedJob job = new ArchiveSavedJob();

        public AddSchedule(ArchiveSavedJob job)
        {
            InitializeComponent();

            //DataContext = job;
            this.job = job;

            DataContext = this.job;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            //List<string> sb = new List<string>();
            //GetErrors(sb, mainGrid);
            //string message="";
            //foreach (var error in sb)
            //{
            //    message+= error;
            //}
            //if (message != "")
            //{
            //    MessageBox.Show(message);
            //}
            //else
            //{
                this.DialogResult = true;
                Close();
            //}
        }

        private void Addfilter_bt_Click(object sender, RoutedEventArgs e)
        {
            if(job != null)
            {
                AddFilter addfilter_dlg = new AddFilter(job.Filter);

                if (addfilter_dlg.ShowDialog() == true)
                {
                    
                }
            }
        }

        private void Removefilter_bt_Click(object sender, RoutedEventArgs e)
        {
            if (filter_lb.SelectedItem != null)
            {
                job.Filter.Remove((string)filter_lb.SelectedItem);
            }
        }


        private void GetErrors(List<string> sb, DependencyObject obj)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(obj))
            {
                TextBox element = child as TextBox;
                if (element == null) continue;

                if (Validation.GetHasError(element))
                {
                    sb.Add(element.Text + " найдена ошибка:\r\n");
                    foreach (ValidationError error in Validation.GetErrors(element))
                    {
                        sb.Add("  " + error.ErrorContent.ToString()+"\r\n");
                    }
                }

                GetErrors(sb, element);
            }
        }
    }
}
