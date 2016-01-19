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
        }
    }

}
