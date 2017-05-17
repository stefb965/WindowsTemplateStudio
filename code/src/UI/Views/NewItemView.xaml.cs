using Microsoft.TemplateEngine.Abstractions;
using Microsoft.Templates.UI.Services;
using Microsoft.Templates.UI.ViewModels;
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

namespace Microsoft.Templates.UI.Views
{
    /// <summary>
    /// Interaction logic for NewItem.xaml
    /// </summary>
    public partial class NewItemView : Window
    {
        public NewItemViewModel ViewModel { get; }

        private string _contextProjectType;
        private string _contextFramework;

        public UserSelection Result { get; set; }

        public ProjectTemplatesViewModel ProjectTemplates { get; private set; } = new ProjectTemplatesViewModel();

        public NewItemView(string contextProjectType, string contextFramework)
        {
            ViewModel = new NewItemViewModel(this);
            _contextProjectType = contextProjectType;
            _contextFramework = contextFramework;
            DataContext = ViewModel;
            Loaded += NewItemViewModel_Loaded;
            InitializeComponent();
        }

        private async void NewItemViewModel_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync(_contextProjectType, _contextFramework);
            
        }
    }
}
