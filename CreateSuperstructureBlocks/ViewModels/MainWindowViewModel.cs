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

        #region Тест проецирование точек на ось
        public ICommand CreateProjectPointsCommand { get; }

        private void OnCreateProjectPointsCommandExecuted(object parameter)
        {
            RevitModel.CreateProjectPoints();
        }

        private bool CanCreateProjectPointsCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            //SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Команды
            GetBeamAxisBySelectionCommand = new LambdaCommand(OnGetBeamAxisBySelectionCommandExecuted, CanGetBeamAxisBySelectionCommandExecute);
            GetRoadAxisCommand = new LambdaCommand(OnGetRoadAxisCommandExecuted, CanGetRoadAxisCommandExecute);
            CreateProjectPointsCommand = new LambdaCommand(OnCreateProjectPointsCommandExecuted, CanCreateProjectPointsCommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
