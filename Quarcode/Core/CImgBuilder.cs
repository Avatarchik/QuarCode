﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Svg;
namespace Quarcode.Core
{
  static class CImgBuilder
  {
    public static Bitmap GenBMPQRfromMatrix(CPointsMatrix matrix, SViewState viewState)
    {
      Bitmap bmp = new Bitmap(matrix.Width, matrix.Heigt);

      using (Graphics gr = Graphics.FromImage(bmp))
      {

        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        gr.FillRectangle(new SolidBrush(CCoder.GetColorFor(PointType.Logo)), 0, 0, matrix.Width, matrix.Heigt);
        DrawBorderBackground(gr, matrix, viewState);

        // Логотип
        List<SGexPoint> currentType = matrix.DrawData.Where(x => x.pointType == PointType.Logo).ToList();

        for (int i = 0; i < currentType.Count; i++)
        {
          if (viewState.FillCells)
            gr.FillPolygon(
              new SolidBrush(CCoder.GetColorFor(currentType[i].pointType)),
              Vector.ToSystemPointsF(currentType[i].Cell.ToArray()));
          if (viewState.DrawCellBorder)
            gr.DrawPolygon(
              new Pen(new SolidBrush(Color.Gray), matrix.Heigt / (350f)),
              Vector.ToSystemPointsF(currentType[i].Cell.ToArray()));
        }

        // Значащие биты
        currentType = matrix.DrawData.Where(
          x => x.pointType == PointType.ByteTrue ||
            x.pointType == PointType.ByteFalse ||
            x.pointType == PointType.UndefinedByte).ToList();
        List<bool> databitsvalue = CCoder.EnCode(viewState.Message, currentType.Count);

        for (int i = 0; i < currentType.Count; i++)
        {
          currentType[i].pointType = databitsvalue[i] ? PointType.ByteTrue : PointType.ByteFalse;
          if (viewState.FillCells)
            gr.FillPolygon(new SolidBrush(CCoder.GetColorFor(matrix.DrawData[i].pointType)),
              Vector.ToSystemPointsF(currentType[i].Cell.ToArray()));
          if (viewState.DrawCellBorder)
            gr.DrawPolygon(new Pen(new SolidBrush(CCoder.GetColorFor(PointType.Border)), matrix.Heigt / (350f)),
              Vector.ToSystemPointsF(currentType[i].Cell.ToArray()));
        }


        //DrawBytes(gr, matrix, viewState);
        //DrawLogo(gr, matrix, viewState);
        DrawPoints(gr, matrix, viewState);
        DrawLogoPoints(gr, matrix, viewState);

        //#if DEBUG
        Brush blueline = new SolidBrush(Color.Red);
        Pen borderPointsPen = new Pen(blueline, 3);
        if (false)
          for (int i = 0; i < matrix.BorderPoints.Count; i++)
          {
            //Отрисовка центральных точек границы
            gr.DrawLine(borderPointsPen,
              (int)matrix.BorderPoints[i].x,
              (int)(int)matrix.BorderPoints[i].y,
              (int)matrix.BorderPoints[i].x + 2,
              (int)(int)matrix.BorderPoints[i].y + 2);
            gr.DrawString(i.ToString(),
              new Font("Sans Serif", 16f),
              new SolidBrush(Color.Red),
             (int)matrix.BorderPoints[i].x,
             (int)(int)matrix.BorderPoints[i].y);
          }

        //#endif
        //Отрисовка места под логотип
        //gr.FillPolygon(new SolidBrush(Color.WhiteSmoke), Vector.ToSystemPointsF(matrix.LogoBorderPoints.ToArray()));
#if DEBUG
        //DEBUG
        // вывод на экран окружения конкретной точки
        if (false)
          for (int i = 0; i < matrix.Points.Count; i++)
          {
            if (i != 5) continue;
            int[] points = matrix.sixNearest(i);
            for (int jj = 0; jj < points.Length; jj++)
            {
              try
              {
                gr.DrawLine(new Pen(new SolidBrush(Color.Green)),
                  (int)matrix.VectorAt(points[jj]).x + 2,
                  (int)(int)matrix.VectorAt(points[jj]).y + 2,
                  (int)matrix.VectorAt(points[jj]).x + 4,
                  (int)(int)matrix.VectorAt(points[jj]).y + 4);
                gr.DrawString(jj.ToString(),
                  new Font("Sans Serif", 16f),
                  new SolidBrush(Color.Green),
                 (int)matrix.VectorAt(points[jj]).x + 5,
                 (int)(int)matrix.VectorAt(points[jj]).y - 5);

              }
              catch (Exception e)
              {
                // do nothing
              }
            }
          }
        //END DEBUG
#endif
      }
      return bmp;
    }

    private static void DrawBytes(Graphics gr, CPointsMatrix matrix, SViewState viewState)
    {
      //Random rand = new Random();
      //Рисуем черный бордер
      if (viewState.DrawQRBorder)
        gr.FillPolygon(new SolidBrush(CCoder.GetColorFor(PointType.Border)), Vector.ToSystemPointsF(matrix.BorderPoints.ToArray()));
      for (int i = 0; i < matrix.Points.Count; i++)
      {
#if DEBUG
        List<int> drawlist = new List<int>();
        //drawlist.Add(0);
        //drawlist.Add(57);
        drawlist.Add(48);

        //if (!drawlist.Contains(i)) continue;
#endif

        //Получаем список окружающих точек
        Vector[] aroundgex = matrix.AroundVoronojGexAt(i);
        // Заливаем поле по окружающим точкам
        if (viewState.FillCells)
          gr.FillPolygon(new SolidBrush(CCoder.GetColorFor(PointType.ByteTrue)), Vector.ToSystemPointsF(aroundgex));
        // Отрисовываем границу по окружающим точкам
        if (aroundgex.Length > 2 && viewState.DrawCellBorder)
          gr.DrawPolygon(new Pen(new SolidBrush(CCoder.GetColorFor(PointType.Border)), matrix.Heigt / (350f)), Vector.ToSystemPointsF(aroundgex));


        if (false)
          for (int ii = 0; ii < matrix.LastSurround.Count; ii++)
          {
            gr.DrawLine(new Pen(new SolidBrush(Color.Red), 3),
              (int)matrix.LastSurround[ii].x,
              (int)matrix.LastSurround[ii].y,
              (int)matrix.LastSurround[ii].x + 2,
              (int)matrix.LastSurround[ii].y + 2);
          }
        for (int j = 0; j < aroundgex.Length; j++)
        {
          // Ставим точку

          if (false)
            gr.DrawLine(new Pen(new SolidBrush(Color.Green), 3),
              (int)aroundgex[j].x,
              (int)aroundgex[j].y,
              (int)aroundgex[j].x + 2,
              (int)aroundgex[j].y + 2);
          //номер в округе гекса
          if (false)
            gr.DrawString(j.ToString(),
             new Font("Sans Serif", 10f),
             new SolidBrush(Color.Black),
             (int)aroundgex[j].x + 2,
             (int)aroundgex[j].y + 2);
        }
      }
    }

    private static void DrawLogo(Graphics gr, CPointsMatrix matrix, SViewState viewState)
    {
      for (int i = matrix.Points.Count; i < matrix.Points.Count + matrix.LogoPoints.Count; i++)
      {
        if (i == 148) continue;
        //Получаем список окружающих точек
        Vector[] aroundgex = matrix.AroundVoronojGexAt(i);
        // Заливаем поле по окружающим точкам
        if (viewState.FillCells)
          gr.FillPolygon(new SolidBrush(CCoder.GetColorFor(PointType.Logo)), Vector.ToSystemPointsF(aroundgex));
        // Отрисовываем границу по окружающим точкам
        if (aroundgex.Length > 2 && viewState.DrawCellBorder)
          gr.DrawPolygon(new Pen(new SolidBrush(CCoder.GetColorFor(PointType.Border))), Vector.ToSystemPointsF(aroundgex));

        for (int j = 0; j < aroundgex.Length; j++)
        {
          // Ставим точку

          if (false)
            gr.DrawLine(new Pen(new SolidBrush(Color.Green), 3),
              (int)aroundgex[j].x,
              (int)aroundgex[j].y,
              (int)aroundgex[j].x + 2,
              (int)aroundgex[j].y + 2);
          //номер в округе гекса
          if (false)
            gr.DrawString(j.ToString(),
             new Font("Sans Serif", 10f),
             new SolidBrush(Color.Black),
             (int)aroundgex[j].x + 2,
             (int)aroundgex[j].y + 2);
        }
      }
    }

    private static void DrawBorderBackground(Graphics gr, CPointsMatrix matrix, SViewState viewState)
    {
      Random rand = new Random();
      //Рисуем черный бордер
      if (viewState.DrawQRBorder)
        gr.FillPolygon(new SolidBrush(CCoder.GetColorFor(PointType.Border)), Vector.ToSystemPointsF(matrix.BorderPoints.ToArray()));
    }

    private static void DrawPoints(Graphics gr, CPointsMatrix matrix, SViewState viewState)
    {
      Brush redline = new SolidBrush(Color.Red);
      Pen innerPointsPen = new Pen(redline, 3);
      if (viewState.DrawValNum)
        for (int i = 0; i < matrix.Points.Count; i++)
        {
          //Отрисовка центральных точек данных
          //gr.DrawLine(innerPointsPen,
          //  (int)matrix.Points[i].x,
          //  (int)matrix.Points[i].y,
          //  (int)matrix.Points[i].x + 2,
          //  (int)matrix.Points[i].y + 2);

          gr.DrawString(i.ToString(),
            new Font("Sans Serif", 10f),
            new SolidBrush(Color.Black),
           (int)matrix.Points[i].x,
           (int)(int)matrix.Points[i].y);
          // Отрисовка сдвинутых точек

          gr.DrawLine(innerPointsPen,
           (int)matrix.NoisedPoints[i].x,
           (int)matrix.NoisedPoints[i].y,
           (int)matrix.NoisedPoints[i].x + 2,
           (int)matrix.NoisedPoints[i].y + 2);
        }
    }

    private static void DrawLogoPoints(Graphics gr, CPointsMatrix matrix, SViewState viewState)
    {
      Brush redline = new SolidBrush(Color.Red);
      Pen innerPointsPen = new Pen(redline, 3);
      if (viewState.DrawValNum)
        for (int i = matrix.Points.Count; i < matrix.Points.Count + matrix.LogoPoints.Count; i++)
        {
          //Отрисовка центральных точек данных
          gr.DrawString(i.ToString(),
          new Font("Sans Serif", 10f),
          new SolidBrush(Color.Black),
         (int)matrix.NoisedPoints[i].x,
         (int)matrix.NoisedPoints[i].y);
          // Отрисовка сдвинутых точек

          gr.DrawLine(innerPointsPen,
           (int)matrix.NoisedPoints[i].x,
           (int)matrix.NoisedPoints[i].y,
           (int)matrix.NoisedPoints[i].x + 2,
           (int)matrix.NoisedPoints[i].y + 2);
        }
    }

    private static void DrawBorderPoints(Graphics gr, CPointsMatrix matrix, SViewState viewState)
    {
      Brush redline = new SolidBrush(Color.Red);
      Pen innerPointsPen = new Pen(redline, 3);
      if (viewState.DrawValNum)
        for (int i = matrix.Points.Count; i < matrix.Points.Count + matrix.LogoPoints.Count; i++)
        {
          //Отрисовка центральных точек данных
          gr.DrawString(i.ToString(),
          new Font("Sans Serif", 10f),
          new SolidBrush(Color.Black),
         (int)matrix.NoisedPoints[i].x,
         (int)matrix.NoisedPoints[i].y);
          // Отрисовка сдвинутых точек

          gr.DrawLine(innerPointsPen,
           (int)matrix.NoisedPoints[i].x,
           (int)matrix.NoisedPoints[i].y,
           (int)matrix.NoisedPoints[i].x + 2,
           (int)matrix.NoisedPoints[i].y + 2);
        }
    }

    public static void saveToFile(Bitmap img, string filepath)
    {
      using (Graphics gr = Graphics.FromImage(img))
      {
        gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        gr.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        img.Save(filepath, ImageFormat.Png);

      }
    }
  }

}
