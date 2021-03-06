﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using AlarmWorkflow.Parser.GenericParser.Parsing;
using AlarmWorkflow.Parser.GenericParser.Views;
using AlarmWorkflow.Shared.Core;
using AlarmWorkflow.Windows.UIContracts.ViewModels;

namespace AlarmWorkflow.Parser.GenericParser.ViewModels
{
    /// <summary>
    /// Defines the ViewModel that represents one section within a parser definition.
    /// </summary>
    class SectionDefinitionViewModel : ViewModelBase
    {
        #region Fields

        private ParserDefinitionViewModel _parent;
        private string _name;

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the name of the section.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        /// <summary>
        /// Gets/sets the collection of children areas.
        /// </summary>
        public ObservableCollection<AreaDefinitionViewModel> Areas { get; set; }
        /// <summary>
        /// Gets/sets the collection of section parsing aspects.
        /// </summary>
        public ObservableCollection<SectionParserDefinitionViewModel> Aspects { get; set; }

        #endregion

        #region Commands

        #region Command "AddAreaCommand"

        /// <summary>
        /// The AddAreaCommand command.
        /// </summary>
        public ICommand AddAreaCommand { get; private set; }

        private void AddAreaCommand_Execute(object parameter)
        {
            AreaDefinitionViewModel area = new AreaDefinitionViewModel(this);
            Areas.Add(area);
        }

        #endregion

        #region Command "RemoveSectionCommand"

        /// <summary>
        /// The RemoveSectionCommand command.
        /// </summary>
        public ICommand RemoveSectionCommand { get; private set; }

        private void RemoveSectionCommand_Execute(object parameter)
        {
            _parent.Sections.Remove(this);
        }

        #endregion

        #region Command "AddSectionAspectCommand"

        /// <summary>
        /// The AddSectionAspectCommand command.
        /// </summary>
        public ICommand AddSectionAspectCommand { get; private set; }

        private void AddSectionAspectCommand_Execute(object parameter)
        {
            AddSectionAspectWindow window = new AddSectionAspectWindow();
            if (window.ShowDialog() == true && window.SelectedType != null)
            {
                // Get real parent section here and add aspect to that
                // BUG?: It doesn't work reliably when you write "this.Aspects.Add()"... to be checked!
                SectionDefinitionViewModel section = (SectionDefinitionViewModel)_parent.SelectedNode;
                ISectionParser sectionParser = SectionParserCache.Create(window.SelectedType.Type);

                SectionParserDefinitionViewModel spdvm = new SectionParserDefinitionViewModel(section, sectionParser);
                section.Aspects.Add(spdvm);
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionDefinitionViewModel"/> class.
        /// </summary>
        private SectionDefinitionViewModel()
        {
            Name = Properties.Resources.SectionBlankName;

            Areas = new ObservableCollection<AreaDefinitionViewModel>();
            Aspects = new ObservableCollection<SectionParserDefinitionViewModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionDefinitionViewModel"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public SectionDefinitionViewModel(ParserDefinitionViewModel parent)
            : this()
        {
            Assertions.AssertNotNull(parent, "parent");
            _parent = parent;
        }

        #endregion

        #region Nested types

        #endregion
    }
}
