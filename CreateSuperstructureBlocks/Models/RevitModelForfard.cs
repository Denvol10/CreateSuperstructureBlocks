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
using CreateSuperstructureBlocks.Models;

namespace CreateSuperstructureBlocks
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Линии осей блоков в плане
        public List<Line> BeamAxis { get; set; }

        private string _beamAxisIds;
        public string BeamAxisIds
        {
            get => _beamAxisIds;
            set => _beamAxisIds = value;
        }

        public void GetBeamAxisBySelection()
        {
            BeamAxis = RevitGeometryUtils.GetCurvesByLines(Uiapp, out _beamAxisIds);
        }
        #endregion

        #region Проверка на то существуют линии осей блоков в моделе
        public bool IsBeamAxisExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(ModelLine));
        }
        #endregion

        #region Получение линий осей блоков в плане
        public void GetBeamAxisBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            BeamAxis = RevitGeometryUtils.GetBeamAxisById(Doc, elemIds);
        }
        #endregion

        #region Ось трассы
        public PolyCurve RoadAxis { get; set; }

        private string _roadAxisElemIds;
        public string RoadAxisElemIds
        {
            get => _roadAxisElemIds;
            set => _roadAxisElemIds = value;
        }

        public void GetPolyCurve()
        {
            var curves = RevitGeometryUtils.GetCurvesByRectangle(Uiapp, out _roadAxisElemIds);
            RoadAxis = new PolyCurve(curves);
        }
        #endregion

        #region Проверка на то существуют линии оси и линии на поверхности в модели
        public bool IsLinesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(DirectShape));
        }
        #endregion

        #region Получение оси трассы из Settings
        public void GetAxisBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            var lines = RevitGeometryUtils.GetCurvesById(Doc, elemIds);
            RoadAxis = new PolyCurve(lines);
        }
        #endregion

        #region Линия на поверхности 1
        public List<Line> RoadLines1 { get; set; }

        private string _roadLineElemIds1;
        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => _roadLineElemIds1 = value;
        }

        public void GetRoadLine1()
        {
            RoadLines1 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds1);
        }
        #endregion

        #region Линия на поверхности 2
        public List<Line> RoadLines2 { get; set; }

        private string _roadLineElemIds2;
        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => _roadLineElemIds2 = value;
        }

        public void GetRoadLine2()
        {
            RoadLines2 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds2);
        }
        #endregion

        #region Тест проекция точек на ось
        public void CreateProjectPoints()
        {
            var points = BeamAxis.Select(l => l.GetEndPoint(0));
            var projectPoints = new List<XYZ>();

            foreach(var point in points)
            {
                var projectPoint = RoadAxis.GetProjectPoint(point);
                if(!(projectPoint is null))
                {
                    projectPoints.Add(projectPoint);
                }
            }

            using(Transaction trans = new Transaction(Doc, "Created Project Points"))
            {
                trans.Start();
                foreach(XYZ point in projectPoints)
                {
                    var referencePoint = Doc.FamilyCreate.NewReferencePoint(point);
                }
                foreach (XYZ point in points)
                {
                    var referencePoint = Doc.FamilyCreate.NewReferencePoint(point);
                }
                trans.Commit();
            }
        }
        #endregion

    }
}
