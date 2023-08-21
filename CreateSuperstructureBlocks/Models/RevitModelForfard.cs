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

namespace CreateSuperstructureBlocks
{
    public class RevitModelForfard
    {
        private UIApplication _uiapp = null;
        private Application _app = null;
        private UIDocument _uidoc = null;
        private Document _doc = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            _uiapp = uiapp;
            _app = uiapp.Application;
            _uidoc = uiapp.ActiveUIDocument;
            _doc = uiapp.ActiveUIDocument.Document;
        }

        public List<string> GetAllRooms()
        {
            var rooms = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Rooms)
                                                          .Cast<Room>()
                                                          .Select(r => r.Name)
                                                          .ToList();

            return rooms;
        }
    }
}
