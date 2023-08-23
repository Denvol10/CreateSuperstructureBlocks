using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSuperstructureBlocks.Models
{
    public class SuperstructureBlock
    {
        public Line BlockAxis { get; set; }
        public XYZ StartAxisPoint { get; set; }
        public XYZ EndAxisPoint { get; set; }

        public SuperstructureBlock(Line axis)
        {
            BlockAxis = axis;
            StartAxisPoint = axis.GetEndPoint(0);
            EndAxisPoint = axis.GetEndPoint(1);
        }

        /// <summary>
        /// Метод возвращает вертикальную плоскость, под прямым углом к оси трассы и проходящую через одну из вершин
        /// </summary>
        /// <param name="roadAxis">Ось трассы</param>
        /// <returns>Возвращает вертикальную плоскость в начале линии</returns>
        public Plane GetStartPlane(PolyCurve roadAxis)
        {
            XYZ startPointProjectOnRoad = roadAxis.GetProjectPoint(StartAxisPoint);
            XYZ normalPoint = StartAxisPoint + XYZ.BasisZ;
            Plane plane = Plane.CreateByThreePoints(StartAxisPoint, startPointProjectOnRoad, normalPoint);

            return plane;
        }

        /// <summary>
        /// Метод возвращает вертикальную плоскость, под прямым углом к оси трассы и проходящую через одну из вершин
        /// </summary>
        /// <param name="roadAxis">Ось трассы</param>
        /// <returns>Возвращает вертикальную плоскость в конце линии</returns>
        public Plane GetEndPlane(PolyCurve roadAxis)
        {
            XYZ endPointProjectOnRoad = roadAxis.GetProjectPoint(EndAxisPoint);
            XYZ normalPoint = EndAxisPoint + XYZ.BasisZ;
            Plane plane = Plane.CreateByThreePoints(EndAxisPoint, endPointProjectOnRoad, normalPoint);

            return plane;
        }

    }
}
