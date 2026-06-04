using System;
using System.Collections.Generic;
using CrossbowOverlayWidget.Models;

namespace CrossbowOverlayWidget.Services
{
    public static class CalibrationEngine
    {
        public static List<DistanceMark> Calibrate(
            List<DistanceMark> marks, int idx1, int idx2,
            double scale, double spacing,
            int rangeMin, int rangeMax, int step)
        {
            if (idx1 < 0 || idx2 < 0 || idx1 == idx2 ||
                idx1 >= marks.Count || idx2 >= marks.Count)
                return marks;

            if (step <= 0 || rangeMin >= rangeMax)
                return marks;

            int centerIdx = marks.FindIndex(m => m.Distance == 200);
            if (centerIdx < 0) centerIdx = marks.Count / 2;

            var mark1 = marks[idx1];
            var mark2 = marks[idx2];

            double absY1 = (idx1 - centerIdx) * spacing + mark1.OffsetY * scale;
            double absY2 = (idx2 - centerIdx) * spacing + mark2.OffsetY * scale;
            int d1 = mark1.Distance, d2 = mark2.Distance;

            if (d1 == d2) return marks;

            double slope = (absY2 - absY1) / (double)(d2 - d1);

            var newMarks = new List<DistanceMark>();
            for (int d = rangeMin; d <= rangeMax; d += step)
            {
                double targetAbsY = absY1 + (d - d1) * slope;
                newMarks.Add(new DistanceMark
                {
                    Distance = d,
                    Label = $"{d}m",
                    IsMajor = (d % 100 == 0),
                    OffsetY = targetAbsY
                });
            }

            // Convert absolute Y back to relative offsetY
            int newCenterIdx = newMarks.FindIndex(m => m.Distance == 200);
            if (newCenterIdx < 0) newCenterIdx = newMarks.Count / 2;

            for (int i = 0; i < newMarks.Count; i++)
            {
                double targetAbsY = newMarks[i].OffsetY;
                double expectedAbsY = (i - newCenterIdx) * spacing;
                newMarks[i].OffsetY = (targetAbsY - expectedAbsY) / scale;
            }

            return newMarks;
        }
    }
}
