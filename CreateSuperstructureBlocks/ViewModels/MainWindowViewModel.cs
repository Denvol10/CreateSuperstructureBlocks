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

        private string _title = "Комнаты";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        #endregion

        #region Список комнат

        private ObservableCollection<string> _rooms;

        public ObservableCollection<string> Rooms
        {
            get => _rooms;
            set => Set(ref _rooms, value);
        }

        #endregion

        #region Команды

        #region Команда получение всех комнат

        public ICommand GetRoomsCommand { get; }

        private void OnGetRoomsCommandExecuted(object parameter)
        {
            Rooms = new ObservableCollection<string>(RevitModel.GetAllRooms());
        }

        private bool CanGetRoomsCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #endregion


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel()
        {
            #region

            GetRoomsCommand = new LambdaCommand(OnGetRoomsCommandExecuted, CanGetRoomsCommandExecute);

            #endregion
        }
        #endregion
    }
}
