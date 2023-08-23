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
using System.IO;

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

        #region Получение линии на поверхности 1 из Settings
        public void GetRoadLines1BySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            RoadLines1 = RevitGeometryUtils.GetCurvesById(Doc, elemIds).OfType<Line>().ToList();
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

        #region Получение линии на поверхности 2 из Settings
        public void GetRoadLines2BySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            RoadLines2 = RevitGeometryUtils.GetCurvesById(Doc, elemIds).OfType<Line>().ToList();
        }
        #endregion

        public void GetRoadLine2()
        {
            RoadLines2 = RevitGeometryUtils.GetRoadLines(Uiapp, out _roadLineElemIds2);
        }
        #endregion

        #region Список названий типоразмеров семейств
        public ObservableCollection<FamilySymbolSelector> GetFamilySymbolNames()
        {
            var familySymbolNames = new ObservableCollection<FamilySymbolSelector>();
            var allFamilies = new FilteredElementCollector(Doc).OfClass(typeof(Family)).OfType<Family>();
            var genericModelFamilies = allFamilies.Where(f => f.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel);
            if (genericModelFamilies.Count() == 0)
                return familySymbolNames;

            foreach (var family in genericModelFamilies)
            {
                foreach (var symbolId in family.GetFamilySymbolIds())
                {
                    var familySymbol = Doc.GetElement(symbolId);
                    familySymbolNames.Add(new FamilySymbolSelector(family.Name, familySymbol.Name));
                }
            }

            return familySymbolNames;
        }
        #endregion

        #region Создание блоков балок
        public void CreateBlocks(FamilySymbolSelector familyAndSymbolName,
                                 double coverageThickness,
                                 double plateThickness,
                                 double blockHeight,
                                 bool isReversed)
        {
            var blocks = new List<SuperstructureBlock>();

            if(isReversed)
            {
                foreach (var line in BeamAxis)
                {
                    blocks.Add(new SuperstructureBlock(line.CreateReversed() as Line));
                }
            }
            else
            {
                foreach(var line in BeamAxis)
                {
                    blocks.Add(new SuperstructureBlock(line));
                }
            }

            var testPoints = new List<XYZ>();

            FamilySymbol fSymbol = GetFamilySymbolByName(familyAndSymbolName);

            var creationDataList = new List<(Autodesk.Revit.Creation.FamilyInstanceCreationData CreationData, double Length)>();

            string resultPath = @"O:\Revit Infrastructure Tools\CreateSuperstructureBlocks\CreateSuperstructureBlocks\result.txt";

            using(StreamWriter sw = new StreamWriter(resultPath, false, Encoding.Default))
            {
                foreach (var block in blocks)
                {
                    var startPlane = block.GetStartPlane(RoadAxis);
                    var endPlane = block.GetEndPlane(RoadAxis);

                    Line startLineOnRoad1 = RevitGeometryUtils.GetIntersectCurve(RoadLines1, startPlane);
                    Line startLineOnRoad2 = RevitGeometryUtils.GetIntersectCurve(RoadLines2, startPlane);

                    Line endLineOnRoad1 = RevitGeometryUtils.GetIntersectCurve(RoadLines1, endPlane);
                    Line endLineOnRoad2 = RevitGeometryUtils.GetIntersectCurve(RoadLines2, endPlane);

                    XYZ startPointOnRoad1 = RevitGeometryUtils.LinePlaneIntersection(startLineOnRoad1, startPlane, out _);
                    XYZ startPointOnRoad2 = RevitGeometryUtils.LinePlaneIntersection(startLineOnRoad2, startPlane, out _);

                    XYZ endPointOnRoad1 = RevitGeometryUtils.LinePlaneIntersection(endLineOnRoad1, endPlane, out _);
                    XYZ endPointOnRoad2 = RevitGeometryUtils.LinePlaneIntersection(endLineOnRoad2, endPlane, out _);

                    XYZ startRoadSurfaceVector = startPointOnRoad1 - startPointOnRoad2;
                    XYZ endRoadSurfaceVector = endPointOnRoad1 - endPointOnRoad2;

                    XYZ startNormalOnRoadVector = startRoadSurfaceVector.CrossProduct(startPlane.Normal).Normalize();
                    if(startNormalOnRoadVector.Z > 0)
                    {
                        startNormalOnRoadVector = startNormalOnRoadVector.Negate();
                    }

                    XYZ endNormalOnRoadVector = endRoadSurfaceVector.CrossProduct(endPlane.Normal).Normalize();
                    if (endNormalOnRoadVector.Z > 0)
                    {
                        endNormalOnRoadVector = endNormalOnRoadVector.Negate();
                    }

                    double distanceBetweenRoadPlaneAndBlock = UnitUtils.ConvertToInternalUnits((coverageThickness
                                                                                                + plateThickness
                                                                                                + blockHeight), UnitTypeId.Millimeters);

                    double distanceBetweenPoints = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Meters);

                    XYZ offsetPlanePoint1 = startPointOnRoad1 + startNormalOnRoadVector * distanceBetweenRoadPlaneAndBlock;
                    XYZ offsetPlanePoint2 = startPointOnRoad2 + startNormalOnRoadVector * distanceBetweenRoadPlaneAndBlock;
                    XYZ offsetPlanePoint3 = endPointOnRoad1 + endNormalOnRoadVector * distanceBetweenRoadPlaneAndBlock;

                    Plane offsetPlane = Plane.CreateByThreePoints(offsetPlanePoint1, offsetPlanePoint2, offsetPlanePoint3);

                    Line startPointVerticalLine = Line.CreateBound(block.StartAxisPoint, block.StartAxisPoint + XYZ.BasisZ);

                    // Первая точка для адаптивного семейства блока
                    XYZ startOffsetPoint = RevitGeometryUtils.LinePlaneIntersection(startPointVerticalLine, offsetPlane, out _);

                    Line endPointVerticalLine = Line.CreateBound(block.EndAxisPoint, block.EndAxisPoint + XYZ.BasisZ);
                    XYZ endOffsetPoint = RevitGeometryUtils.LinePlaneIntersection(endPointVerticalLine, offsetPlane, out _);
                    XYZ vectorAlongBlock = endOffsetPoint - startOffsetPoint;

                    // Вторая точка для адаптивного семейства блока
                    XYZ secondPoint = startOffsetPoint + vectorAlongBlock.Normalize() * distanceBetweenPoints;

                    XYZ thirdPointVector = vectorAlongBlock.CrossProduct(startNormalOnRoadVector).Normalize();

                    // Третья точка для адаптивного семейства
                    XYZ thirdPoint = startOffsetPoint + thirdPointVector * distanceBetweenPoints;

                    var familyInstancePoints = new List<XYZ>() { startOffsetPoint, secondPoint, thirdPoint };
                    double blockLength = endOffsetPoint.DistanceTo(startOffsetPoint);


                    creationDataList.Add((new Autodesk.Revit.Creation.FamilyInstanceCreationData(fSymbol, familyInstancePoints), blockLength));

                    testPoints.Add(startPointOnRoad1);
                    testPoints.Add(startPointOnRoad2);

                }
            }

            using (Transaction trans = new Transaction(Doc, "Create Family Instances"))
            {
                trans.Start();
                if (!fSymbol.IsActive)
                {
                    fSymbol.Activate();
                }

                ICollection<ElementId> elementSet = null;

                if (Doc.IsFamilyDocument)
                {
                    elementSet = Doc.FamilyCreate.NewFamilyInstances2(creationDataList.Select(c => c.CreationData).ToList());
                }
                else
                {
                    elementSet = Doc.Create.NewFamilyInstances2(creationDataList.Select(c => c.CreationData).ToList());
                }

                for(int i = 0; i < elementSet.Count; i++)
                {
                    Element elem = Doc.GetElement(elementSet.ElementAt(i));
                    double length = creationDataList.Select(c => c.Length).ElementAt(i);

                    Parameter blockLengthParameter = elem.LookupParameter("Длина блока");
                    blockLengthParameter.Set(length);
                }

                foreach (var point in testPoints)
                {
                    var referPoint = Doc.FamilyCreate.NewReferencePoint(point);
                }
                trans.Commit();
            }

            //using (Transaction trans = new Transaction(Doc, "Created Blocks"))
            //{
            //    trans.Start();
            //    foreach (var point in testPoints)
            //    {
            //        var referPoint = Doc.FamilyCreate.NewReferencePoint(point);
            //    }
            //    trans.Commit();
            //}

        }
        #endregion

        #region Получение типоразмера по имени
        private FamilySymbol GetFamilySymbolByName(FamilySymbolSelector familyAndSymbolName)
        {
            var familyName = familyAndSymbolName.FamilyName;
            var symbolName = familyAndSymbolName.SymbolName;

            Family family = new FilteredElementCollector(Doc).OfClass(typeof(Family)).Where(f => f.Name == familyName).First() as Family;
            var symbolIds = family.GetFamilySymbolIds();
            foreach (var symbolId in symbolIds)
            {
                FamilySymbol fSymbol = (FamilySymbol)Doc.GetElement(symbolId);
                if (fSymbol.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == symbolName)
                {
                    return fSymbol;
                }
            }
            return null;
        }
        #endregion
    }
}
