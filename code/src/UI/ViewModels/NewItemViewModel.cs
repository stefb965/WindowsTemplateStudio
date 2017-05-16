using Microsoft.TemplateEngine.Abstractions;
using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Mvvm;
using Microsoft.Templates.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Templates.UI.ViewModels
{
    public class NewItemViewModel : Observable
    {
        private bool _isValid = true;

        private readonly NewItemView _newItemView;


        private string _contextFramework;

        public string ContextFramework
        {
            get { return _contextFramework; }
            set { SetProperty(ref _contextFramework, value); }
        }

        private string _contextProjectType;

        public string ContextProjectType
        {
            get { return _contextProjectType; }
            set { SetProperty(ref _contextProjectType, value); }
        }



        public NewItemViewModel(NewItemView newItemView, string contextProjectType, string contextFramework)
        {
            _newItemView = newItemView;
            _contextProjectType = contextProjectType;
            _contextFramework = contextFramework;
        }

        public ICommand OkCommand => new RelayCommand(SaveAndClose, IsValid);
        public ICommand CancelCommand => new RelayCommand(_newItemView.Close);

   
        public ObservableCollection<TemplateInfoViewModel> Templates { get; } = new ObservableCollection<TemplateInfoViewModel>();

        private TemplateInfoViewModel _templateSelected;
        public TemplateInfoViewModel TemplateSelected
        {
            get { return _templateSelected; }
            set
            {
                SetProperty(ref _templateSelected, value);
                if (value != null)
                {
                    ItemName = Naming.Infer(new List<string>(), value.Name);
                }
            }
        }

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                SetProperty(ref _itemName, value);

                Validate(value);

                OnPropertyChanged(nameof(OkCommand));
            }
        }

        public async Task InitializeAsync()
        {
            await GenContext.ToolBox.Repo.SynchronizeAsync();
            Templates.Clear();

            var pageTemplates = GenContext.ToolBox.Repo.Get(t => (t.GetTemplateType() == TemplateType.Page || t.GetTemplateType() == TemplateType.Feature)
                                                                && t.GetFrameworkList().Contains(_contextFramework)                                                               )
                                                            .Select(t => new TemplateInfoViewModel(t, GenComposer.GetAllDependencies(t, _contextFramework), _contextFramework));

            Templates.AddRange(pageTemplates);

            if (Templates.Any())
            {
                TemplateSelected = Templates.FirstOrDefault();
            }
            else
            {
                _isValid = false;
                OnPropertyChanged(nameof(OkCommand));
            }

            await Task.CompletedTask;
        }
        
        private void SaveAndClose()
        {
            _newItemView.DialogResult = true;
            _newItemView.Result = CreateUserSelection();

            _newItemView.Close();
        }

        private UserSelection CreateUserSelection()
        {
            var userSelection = new UserSelection()
            {
                ProjectType = _contextProjectType,
                Framework = _contextFramework
            };
            AddTemplate(userSelection, TemplateSelected.Template);
            var dependencies = GenComposer.GetAllDependencies(TemplateSelected.Template, _contextFramework);
            foreach (var dependency in dependencies)
            {
                AddTemplate(userSelection, dependency);
            }

            return userSelection;
        }

        private void AddTemplate(UserSelection userSelection, ITemplateInfo template)
        {
            if (template.GetTemplateType() == TemplateType.Page)
            {
                userSelection.Pages.Add((ItemName, template));
            }
            else
            {
                userSelection.Features.Add((ItemName, template));
            }
        }

        private void HandleValidation(ValidationResult validationResult)
        {
            _isValid = validationResult.IsValid;

            if (!validationResult.IsValid)
            {
                var message = "ValidationError";
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "UndefinedError";
                }
                throw new Exception(message);
            }
        }

        private void Validate(string value)
        {
            var validationResult = Naming.Validate(new List<string>(), value);

            HandleValidation(validationResult);
        }

        private bool IsValid() => _isValid;
    }
}
