using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

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
        string year = DateTime.Today.Year.ToString();

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

        private void button_Click(object sender, RoutedEventArgs e) // supprimé l'item dans la listbox et liste
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
            int newOrdreDocNo = 0;

            try
            {
                var commandeAutofab = (from c in db.COMMANDEs
                                       where c.CONOTRANS == noProdval
                                       select new { c.COSEQ, c.CLIENT_CLNOM }).Single();

                sequenceCommande = commandeAutofab.COSEQ;
                string nonClienCommande = commandeAutofab.CLIENT_CLNOM;
                //MessageBox.Show(sequenceCommande.ToString() + "\n" + nonClienCommande);
            }
            catch
            {
                MessageBox.Show("No. de Commande AutoFAB non valid");
            }

            try
            {
                var ordredocument = (from d in db.DOCUMENTs
                                     where d.DOSEQ_REFERENCE == sequenceCommande && d.DONOM_TABLE == "COMMANDE"
                                     orderby d.DOORDRE descending
                                     select new { d.DOORDRE }).FirstOrDefault();

                newOrdreDocNo = ordredocument.DOORDRE + 1;

                //MessageBox.Show("Ordre " + newOrdreDocNo);    
            }
            catch (Exception)
            {
                newOrdreDocNo = 1; // aucun document dans commande Ordre = 1
            }

            if (distinct.Count > 0 && sequenceCommande != 0)
            {
                bool copyValid = false;

                foreach (var item in distinct)
                {
                    
                    string subPath = @"\\cabanons00013\documentsautofab6\documents\COMMANDE\" + year + @"\" + noProdval + @"\"; // \\cabanons00013\documentsautofab6\documents\COMMANDE\2015\CO-000024
                    Directory.CreateDirectory(subPath);

                    try
                    {
                        File.Copy(item, subPath + System.IO.Path.GetFileName(item));

                        DOCUMENT docNew = new DOCUMENT();

                        docNew.DONOM_TABLE = "COMMANDE";
                        docNew.DOSEQ_REFERENCE = sequenceCommande;
                        docNew.DOFICHIER = subPath + System.IO.Path.GetFileName(item);
                        docNew.DOORDRE = newOrdreDocNo;
                        docNew.DODESC_P = "PHOTO INSTALLATION";
                        docNew.DODESC_S = "PHOTO INSTALLATION";
                        docNew.DONOTE = "";
                        docNew.DODATE = DateTime.Now;
                        docNew.DOUSAGER = "PHOTO IMPORTER";

                        db.DOCUMENTs.InsertOnSubmit(docNew);
                        db.SubmitChanges();

                        newOrdreDocNo += 1; //Incrémente le compteur
                        copyValid = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(System.IO.Path.GetFileName(item) + " Fichier déja existant dans le repertoire AutoFAB Image non transféré");
                        copyValid = false;
                    }
                }

                if (copyValid)
                {
                    MessageBox.Show("Image Transféré a AutoFAB6");
                    _items.Clear();
                    items.Clear();
                    _distinct.Clear();
                    distinct.Clear();
                    noProd.Text = "CO-";
                    updateEcran();
                }
            }
            else 
            {
                MessageBox.Show("Aucune image transféré");
            }
        }
    
        private void updateEcran()
        {
            listPhoto.ItemsSource = null;
            listPhoto.ItemsSource = _distinct;

            totalImage = _distinct.Count();
            totalImageTextBox.Text = totalImage.ToString();
        }

        public static bool IsImageExtension(string ext)
        {
            return _validExtensions.Contains(ext);
        }

        private void listPhoto_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string imagefname = null;
            foreach (string item in distinct)
                if (listPhoto.SelectedItem != null)
                {
                    if (System.IO.Path.GetFileName(item) == listPhoto.SelectedItem.ToString())
                    {
                        imagefname = item;
                        break;
                    }
                    else
                    {
                        imagefname = null;
                    }
                }

            if (imagefname != null)
            {
                BitmapImage b = new BitmapImage();
                b.BeginInit();
                b.UriSource = new Uri(imagefname);
                b.EndInit();
                image.Source = b;
            }
            else
            {
                image.Source = null;
            }
        }
    }
}
