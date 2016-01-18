using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

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

        public static PhotoInstallations cPhoto = new PhotoInstallations();

        DataAutofab6DataContext db = new DataAutofab6DataContext();
        
        int sequenceCommande = 0;
        int newOrdreDocNo = 0;


        int totalImage = 0;
        string year = DateTime.Today.Year.ToString();
        bool noProdvalid = false;


        public MainWindow()
        {
            InitializeComponent();
            totalImageTextBox.Text = totalImage.ToString();
            this.DataContext = cPhoto;
            listPhoto.ItemsSource = cPhoto;
            deleteListItem.IsEnabled = false;
            transfertImages.IsEnabled = false;
        }

        /// <summary>
        /// Fonctione pour capté les fichier droppé dans la zone prévue
        /// </summary>
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
                        PhotoInstallation oPhoto = new PhotoInstallation(fname, @item);

                        var findObject = cPhoto.Any(p => p.Photo == item);
                        if (!findObject)
                        {
                            cPhoto.Add(oPhoto);
                        }
                        else
                        {
                            MessageBox.Show("Fichier: " + item + " déja existant");
                        }

                    }
                    else
                    {
                        MessageBox.Show("Erreur: "+ System.IO.Path.GetFileName(item) + " n'est pas un fichier valide!"); 
                    }
                }
                updateEcran();
                deleteListItem.IsEnabled = true;
            }
        }


        private void button_Click(object sender, RoutedEventArgs e) // supprimé l'item dans la listbox et liste 
        {
            List<PhotoInstallation> lstitems = new List<PhotoInstallation>();

            if (listPhoto.SelectedItem != null)
            {
                foreach(PhotoInstallation item in listPhoto.SelectedItems)
                {
                    lstitems.Add(item);
                }

                foreach (PhotoInstallation item in lstitems)
                {
                    cPhoto.Remove(item);
                }
                if (cPhoto.Count == 0)
                {
                    deleteListItem.IsEnabled = false;
                }
                updateEcran();
            }
            else MessageBox.Show("Veuillez selectionnez un item a supprimé");
        }

        /// <summary>
        /// Fonction pour effectué la copie des fichiers et inscrire la note dans les document Autofab
        /// </summary>
        private void transfertImages_Click(object sender, RoutedEventArgs e) 
        {
            string noProdval = noProd.Text;

            try
            {
                var ordredocument = (from d in db.DOCUMENTs
                                     where d.DOSEQ_REFERENCE == sequenceCommande && d.DONOM_TABLE == "COMMANDE"
                                     orderby d.DOORDRE descending
                                     select new { d.DOORDRE }).FirstOrDefault();

                newOrdreDocNo = ordredocument.DOORDRE + 1;

            }
            catch (Exception)
            {
                newOrdreDocNo = 1; // aucun document dans commande Ordre = 1
            }

            if (cPhoto.Count > 0 && sequenceCommande != 0)
            {
                bool copyValid = false;

                foreach (var item in cPhoto)
                {
                    
                    string subPath = @"\\cabanons00013\documentsautofab6\documents\COMMANDE\" + year + @"\" + noProdval + @"\"; // \\cabanons00013\documentsautofab6\documents\COMMANDE\2015\CO-000024
                    Directory.CreateDirectory(subPath);

                    try
                    {
                        File.Copy(item.Photo, subPath + System.IO.Path.GetFileName(item.Name));

                        DOCUMENT docNew = new DOCUMENT();

                        docNew.DONOM_TABLE = "COMMANDE";
                        docNew.DOSEQ_REFERENCE = sequenceCommande;
                        docNew.DOFICHIER = subPath + System.IO.Path.GetFileName(item.Name);
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
                        MessageBox.Show(System.IO.Path.GetFileName(item.Name) + " Fichier déja existant dans le repertoire AutoFAB Image non transféré");
                        copyValid = false;
                    }
                }

                if (copyValid)
                {
                    MessageBox.Show("Image Transféré a AutoFAB6");
                    // Vide la selection apres le transfert
                    cPhoto.Clear();
                    noProd.Text = "CO-";
                    labelNomClient.Content = null;
                    addl1.Content = null;
                    addl2.Content = null;
                    addl3.Content = null;
                    installateur.Items.Clear();
                    updateEcran();
                }
            }
            else 
            {
                MessageBox.Show("Aucune image transféré");
            }
        }
    
        private void updateEcran() // update le data a l'ecran
        {
            if (cPhoto.Count > 0 && noProdvalid)
            {
                transfertImages.IsEnabled = true;
            }
            else
            {
                transfertImages.IsEnabled = false;
            }

            totalImage = cPhoto.Count();
            totalImageTextBox.Text = totalImage.ToString();
        }

        public static bool IsImageExtension(string ext)
        {
            return _validExtensions.Contains(ext);
        }


        private void noProd_LostFocus(object sender, RoutedEventArgs e) // Load data commande et installateur
        {
            string noProdval = noProd.Text;
            installateur.Items.Clear();
            try
            {
                var commandeAutofab = (from c in db.COMMANDEs
                                       where c.CONOTRANS == noProdval
                                       select new { c.COSEQ, c.CLIENT_CLNOM, c.ADRFAC_ALADR1, c.ADRFAC_ALADR2, c.ADRFAC_ALADR4, c.ADRFAC_ALADR5 }).Single();

                sequenceCommande = commandeAutofab.COSEQ;
                string nonClienCommande = commandeAutofab.CLIENT_CLNOM;
                string add1 = commandeAutofab.ADRFAC_ALADR1;
                string add2 = commandeAutofab.ADRFAC_ALADR2;
                string add4 = commandeAutofab.ADRFAC_ALADR4;
                string add5 = commandeAutofab.ADRFAC_ALADR5;
                labelNomClient.Content = nonClienCommande;
                addl1.Content = add1;
                addl2.Content = add2;
                addl3.Content = add4.ToUpper();
                noProdvalid = true;
            }
            catch
            {
                MessageBox.Show("No. de Commande AutoFAB non valid");
                noProdvalid = false;
                labelNomClient.Content = null;
                addl1.Content = null;
                addl2.Content = null;
                addl3.Content = null;
            }

            try
            {
                var AutofabInstallateur = (from ds in db.DET_SUIVI_INSTs
                                                    join si in db.SUIVI_INSTs on ds.SUIVI_INST equals si.SISEQ
                                                    where si.SINOTRANS == noProdval
                                                    select new { ds.EMPLOYE_EMNOM }).ToList();




                foreach (var item in AutofabInstallateur)
                {
                    string iName = item.EMPLOYE_EMNOM;
                    installateur.Items.Add(iName);
                }
                


            }
            catch (Exception)
            {

                throw;
            }


            updateEcran();
        }
    }

}
