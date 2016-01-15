using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;

namespace CfPhotoTransfert
{
    public class PhotoInstallation
    {
        public string Name { get; set; }
        public string Photo { get; set; }

        public PhotoInstallation(string name, string fName)
        {
            this.Name = name;
            this.Photo = fName;
        }
    }

    public class PhotoInstallations : ObservableCollection<PhotoInstallation>
    {
        public PhotoInstallations()
        {
            //Add(new PhotoInstallation("André", @"C:\Users\Public\Pictures\Sample Pictures\Desert.jpg"));
            //Add(new PhotoInstallation("Cooke", @"C:\Users\Public\Pictures\Sample Pictures\Jellyfish.jpg"));
            //Add(new PhotoInstallation("Nathalie", @"C:\Users\Public\Pictures\Sample Pictures\Lighthouse.jpg"));
        }
    }

}
