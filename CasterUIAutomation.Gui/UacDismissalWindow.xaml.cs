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

// https://social.msdn.microsoft.com/Forums/en-US/225b4c48-cbf9-418c-9d76-9b0491520a96/uiaccess-and-elevation-without-using-the-secure-desktop?forum=windowsaccessibilityandautomation
// https://blogs.msdn.microsoft.com/asklar/2012/03/14/remote-assistance-and-uac-prompts/

namespace CasterUIAutomation
{
    /// <summary>
    /// Interaction logic for UacDismissalWindow.xaml
    /// </summary>
    public partial class UacDismissalWindow : Window
    {
        public UacDismissalWindow()
        {
            InitializeComponent();
        }
    }
}
