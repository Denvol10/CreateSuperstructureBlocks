using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CreateSuperstructureBlocks.Infrastructure;
using CreateSuperstructureBlocks.Models;

namespace CreateSuperstructureBlocks.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Создать блоки пролетного строения";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Id линий блоков
        private string _beamAxisIds;

        public string BeamAxisIds
        {
            get => _beamAxisIds;
            set => Set(ref _beamAxisIds, value);
        }
        #endregion

        #region Элементы оси трассы
        private string _roadAxisElemIds;

        public string RoadAxisElemIds
        {
            get => _roadAxisElemIds;
            set => Set(ref _roadAxisElemIds, value);
        }
        #endregion

        #region Элементы линии на поверхности 1
        private string _roadLineElemIds1;

        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => Set(ref _roadLineElemIds1, value);
        }
        #endregion

        #region Элементы линии на поверхности 2
        private string _roadLineElemIds2;

        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => Set(ref _roadLineElemIds2, value);
        }
        #endregion

        #region Список семейств и их типоразмеров
        private ObservableCollection<FamilySymbolSelector> _genericModelFamilySymbols = new ObservableCollection<FamilySymbolSelector>();
        public ObservableCollection<FamilySymbolSelector> GenericModelFamilySymbols
        {
            get => _genericModelFamilySymbols;
            set => Set(ref _genericModelFamilySymbols, value);
        }
        #endregion

        #region Выбранный типоразмер семейства
        private FamilySymbolSelector _familySymbolName;
        public FamilySymbolSelector FamilySymbolName
        {
            get => _familySymbolName;
            set => Set(ref _familySymbolName, value);
        }
        #endregion

        #region Индекс выбранного семейства
        private int _familySymbolIndex = Properties.Settings.Default.FamilySymbolIndex;
        #endregion

        #region Развернут ли блок
        private bool _isReversed = Properties.Settings.Default.IsReversed;
        public bool IsReversed
        {
            get => _isReversed;
            set => Set(ref _isReversed, value);
        }
        #endregion

        #region Команды

        #region Получение линий блоков
        public ICommand GetBeamAxisBySelectionCommand { get; }

        private void OnGetBeamAxisBySelectionCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetBeamAxisBySelection();
            BeamAxisIds = RevitModel.BeamAxisIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetBeamAxisBySelectionCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение оси трассы
        public ICommand GetRoadAxisCommand { get; }

        private void OnGetRoadAxisCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetPolyCurve();
            RoadAxisElemIds = RevitModel.RoadAxisElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadAxisCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии на поверхности дороги 1
        public ICommand GetRoadLines1Command { get; }

        private void OnGetRoadLines1CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine1();
            RoadLineElemIds1 = RevitModel.RoadLineElemIds1;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLines1CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии на поверхности дороги 2
        public ICommand GetRoadLines2Command { get; }

        private void OnGetRoadLines2CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine2();
            RoadLineElemIds2 = RevitModel.RoadLineElemIds2;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLines2CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Создание блоков
        public ICommand CreateBlocksCommand { get; }

        private void OnCreateBlocksCommandExecuted(object parameter)
        {
            RevitModel.CreateBlocks(FamilySymbolName, IsReversed);
            SaveSettings();
        }

        private bool CanCreateBlocksCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default.BeamAxisIds = BeamAxisIds;
            Properties.Settings.Default.RoadAxisElemIds = RoadAxisElemIds;
            Properties.Settings.Default.RoadLineElemIds1 = RoadLineElemIds1;
            Properties.Settings.Default.RoadLineElemIds2 = RoadLineElemIds2;
            Properties.Settings.Default.FamilySymbolIndex = GenericModelFamilySymbols.IndexOf(FamilySymbolName);
            Properties.Settings.Default.IsReversed = IsReversed;
            Properties.Settings.Default.Save();
        }

        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            GenericModelFamilySymbols = RevitModel.GetFamilySymbolNames();

            #region Инициализация свойств из Settings

            #region Инициализация значения осей блоков
            if (!(Properties.Settings.Default.BeamAxisIds is null))
            {
                string beamAxisIdsInSettings = Properties.Settings.Default.BeamAxisIds;
                if(RevitModel.IsBeamAxisExistInModel(beamAxisIdsInSettings) && !string.IsNullOrEmpty(beamAxisIdsInSettings))
                {
                    BeamAxisIds = beamAxisIdsInSettings;
                    RevitModel.GetBeamAxisBySettings(beamAxisIdsInSettings);
                }
            }
            #endregion

            #region Инициализация значения оси трассы
            if(!(Properties.Settings.Default.RoadAxisElemIds is null))
            {
                string roadAxisIdsInSettings = Properties.Settings.Default.RoadAxisElemIds;
                if(RevitModel.IsLinesExistInModel(roadAxisIdsInSettings) && !string.IsNullOrEmpty(roadAxisIdsInSettings))
                {
                    RoadAxisElemIds = roadAxisIdsInSettings;
                    RevitModel.GetAxisBySettings(roadAxisIdsInSettings);
                }
            }
            #endregion

            #region Инициализация значения элементам линии на поверхности 1
            if(!(Properties.Settings.Default.RoadLineElemIds1 is null))
            {
                string line1IdsInSettings = Properties.Settings.Default.RoadLineElemIds1;
                if (RevitModel.IsLinesExistInModel(line1IdsInSettings) && !string.IsNullOrEmpty(line1IdsInSettings))
                {
                    RoadLineElemIds1 = line1IdsInSettings;
                    RevitModel.GetRoadLines1BySettings(line1IdsInSettings);
                }
            }
            #endregion

            #region Инициализация значения элементам линии на поверхности 2
            if(!(Properties.Settings.Default.RoadLineElemIds2 is null))
            {
                string line2IdsInSettings = Properties.Settings.Default.RoadLineElemIds2;
                if(RevitModel.IsLinesExistInModel(line2IdsInSettings) && !string.IsNullOrEmpty(line2IdsInSettings))
                {
                    RoadLineElemIds2 = line2IdsInSettings;
                    RevitModel.GetRoadLines2BySettings(line2IdsInSettings);
                }
            }
            #endregion

            #region Инициализация значения типоразмера семейства
            if (_familySymbolIndex >= 0 && _familySymbolIndex <= GenericModelFamilySymbols.Count - 1)
            {
                FamilySymbolName = GenericModelFamilySymbols.ElementAt(_familySymbolIndex);
            }
            #endregion

            #endregion


            #region Команды
            GetBeamAxisBySelectionCommand = new LambdaCommand(OnGetBeamAxisBySelectionCommandExecuted, CanGetBeamAxisBySelectionCommandExecute);
            GetRoadAxisCommand = new LambdaCommand(OnGetRoadAxisCommandExecuted, CanGetRoadAxisCommandExecute);
            CreateBlocksCommand = new LambdaCommand(OnCreateBlocksCommandExecuted, CanCreateBlocksCommandExecute);
            GetRoadLines1Command = new LambdaCommand(OnGetRoadLines1CommandExecuted, CanGetRoadLines1CommandExecute);
            GetRoadLines2Command = new LambdaCommand(OnGetRoadLines2CommandExecuted, CanGetRoadLines2CommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
