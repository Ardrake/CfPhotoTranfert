using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CfPhotoTransfert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string[] _validExtensions = { ".jpg", ".bmp", ".gif", ".png" };

        public string setting = ConfigurationManager.AppSettings["setting1"];
        public string conn = ConfigurationManager.ConnectionStrings["prod"].ConnectionString;

        List<string> _items = new List<string>(); 
        List<string> _distinct = new List<string>();

        List<string> items = new List<string>();
        List<string> distinct = new List<string>();

        int totalImage = 0;

        public MainWindow()
        {
            InitializeComponent();
            totalImageTextBox.Text = totalImage.ToString();
                        
        }


        private void ImagePanel_drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string item in files)
                {
                    if (IsImageExtension(System.IO.Path.GetExtension(item)))
                    {
                        string fname = System.IO.Path.GetFileName(item);
                        _items.Add(fname);
                        items.Add(item);
                    }
                }
                _distinct = _items.Distinct().ToList();
                distinct = items.Distinct().ToList();
            }
            updateEcran();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (listPhoto.SelectedItem != null)
            {
                string fSelected = listPhoto.SelectedItem.ToString();
                foreach (string item in _distinct)
                {
                    if (item == fSelected)
                    {
                        _distinct.RemoveAll(s => s == item);
                        break;
                    }
                }

                foreach (string item in distinct)
                    if (System.IO.Path.GetFileName(item) == fSelected)
                    {
                    distinct.RemoveAll(s => s == item);
                    break;
                    }

                updateEcran();
            }
            else MessageBox.Show("Veuillez selectionnez un item a supprimé");
        }

        private void transfertImages_Click(object sender, RoutedEventArgs e)
        {

        }
    
        private void updateEcran()
        {
            listPhoto.ItemsSource = null;
            listPhoto.ItemsSource = _distinct;
            listBoxFullFile.ItemsSource = null;
            listBoxFullFile.ItemsSource = distinct;

            totalImage = _distinct.Count();
            totalImageTextBox.Text = totalImage.ToString();
        }

        public static bool IsImageExtension(string ext)
        {
            return _validExtensions.Contains(ext);
        }

    }
}
