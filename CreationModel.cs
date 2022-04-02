using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Level level1, level2;
            TakeLevels(doc, out level1, out level2);
            CreateWalls(doc, level1, level2);

            return Result.Succeeded;
        }

        // метод получения уровней
        private static void TakeLevels(Document doc, out Level level1, out Level level2)
        {
            List<Level> listLevel = new FilteredElementCollector(doc)
               .OfClass(typeof(Level))
               .OfType<Level>()
               .ToList();

            level1 = listLevel
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();
            level2 = listLevel
                .Where(x => x.Name.Equals("Уровень 2"))
                .FirstOrDefault();
        }

        private static void CreateWalls(Document doc, Level level1, Level level2)
        {
            // перевод из футов в мм, внутрення конвертация
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            // коллекцию с точками, за счет смещения
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            // массив в который будут добавлятьсозданные стены
            List<Wall> walls = new List<Wall>();

            // транцакция внутри которой цикл будет создавать стены
            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);//привязка верхнего уровня стены
            }
            transaction.Commit();
        }
    }
}