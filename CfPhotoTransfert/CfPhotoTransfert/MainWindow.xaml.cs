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
                    else
                    {
                        MessageBox.Show("Erreur: "+ System.IO.Path.GetFileName(item) + " n'est pas un fichier valide!"); 
                    }
                }


                _distinct = _items.Distinct().ToList();
                distinct = items.Distinct().ToList();

            }
            updateEcran();
            _items = _distinct;
            items = distinct;
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
            DataAutofab6DataContext db = new DataAutofab6DataContext();
            string noProdval = noProd.Text;
            int sequenceCommande = 0;

            try
            {
                var commandeAutofab = (from c in db.COMMANDEs
                                       where c.CONOTRANS == noProdval
                                       select new { c.COSEQ, c.CLIENT_CLNOM }).Single();

                sequenceCommande = commandeAutofab.COSEQ;
                string nonClienCommande = commandeAutofab.CLIENT_CLNOM;
                MessageBox.Show(sequenceCommande.ToString() + "\n" + nonClienCommande);
            }
            catch
            {
                MessageBox.Show("Commande non trouvé");
            }

            if (distinct.Count > 0)
            {
            DOCUMENT docNew = new DOCUMENT();

            docNew.DONOM_TABLE = "COMMANDE";
            docNew.DOSEQ_REFERENCE = sequenceCommande;
            docNew.DOORDRE = 99;
            docNew.DODESC_P = "test";
            docNew.DODESC_S = "test";
            docNew.DONOTE = "Note Test";
            docNew.DODATE = DateTime.Now;
            docNew.DOUSAGER = "CSharp";

            db.DOCUMENTs.InsertOnSubmit(docNew);
            db.SubmitChanges();
            MessageBox.Show("Data inséré");
            }
            else 
            {
                MessageBox.Show("Aucune image a transféré");
            }
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
