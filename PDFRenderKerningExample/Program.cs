using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using WebSupergoo;
using WebSupergoo.ABCpdf8;

namespace PDFRenderKerningExample
{
    class Program
    {
        static void Main(string[] args)
        {

            string filePath = @"desktop\TestResults\FontRenderingTestMethod1.pdf";
            string myFileName = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), filePath);

            Doc theDoc = new Doc();
            theDoc.MediaBox.String = "0 0 396 306";
            theDoc.Rect.String = theDoc.MediaBox.String;
            
            string fontURL = ReadFontFileNameFromRegistry("Arial", false);

            theDoc.Color.SetColor(new XColor() { Red = 0, Green = 0, Blue = 0 });

            theDoc.Font = theDoc.EmbedFont(fontURL, LanguageType.Unicode, false);

            theDoc.Rect.SetRect(0, 0, 100, 120);
            theDoc.FontSize = 72;
            theDoc.AddText("To");

            theDoc.Rect.SetRect(100, 0, 100, 120);

            fontURL = ReadFontFileNameFromRegistry("Courier New", false);
            theDoc.Font = theDoc.EmbedFont(fontURL, LanguageType.Unicode, false);

            theDoc.AddText("To");

            theDoc.Rect.SetRect(200, 0, 100, 120);

            fontURL = ReadFontFileNameFromRegistry("Times New Roman", false);
            theDoc.Font = theDoc.EmbedFont(fontURL, LanguageType.Unicode, false);

            theDoc.AddText("To");

            theDoc.Save(myFileName);
            theDoc.Clear();

            List<String> fontNames = new List<String>() { "Arial", "Courier New", "Times New Roman" };
            RenderText("To", fontNames, filePath);
        }

        private static string ReadFontFileNameFromRegistry(string FontName, bool includeCollectionIndex)
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

            RegistryKey regkeyFonts = Registry.LocalMachine.OpenSubKey(subKey, false);

            if (regkeyFonts == null)
                return null;

            try
            {
                const string trueType = "(TrueType)";
                List<string> valueNames = regkeyFonts.GetValueNames().Where(t => t.EndsWith(trueType) && t.Contains(FontName)).ToList();

                if (valueNames.Count() == 0)
                {
                    valueNames = regkeyFonts.GetValueNames().Where(t => t.EndsWith(trueType) && t.Contains(FontName.Replace(" ", ""))).ToList();
                    if (valueNames.Count() == 0)
                        return null;
                }

                string fontKey = valueNames[0];

                if (valueNames.Count() > 1)
                {
                    foreach (string s in valueNames)
                    {
                        List<string> nameList = s.Substring(0, s.Length - trueType.Length).Split('&').Select(t => t.Trim()).ToList();

                        if (nameList.Contains(FontName))
                        {
                            fontKey = s;
                            break;
                        }
                    }
                }

                string fontFileName = (string)regkeyFonts.GetValue(fontKey);
                string fontsfolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Fonts);

                return fontsfolder + "\\" + fontFileName;
            }
            catch
            {
                return null;
            }
        }

        private static void RenderText(string text, List<string> fontNames, string filePath)
        {
            int actualHeight = 2550;
            int actualWidth = 3300;

            Bitmap image = new Bitmap(actualWidth, actualHeight);

            image.SetResolution(300, 300);
            Graphics graphic = Graphics.FromImage(image);
            graphic.Clear(Color.White);
            graphic.DrawLine(new Pen(new SolidBrush(System.Drawing.Color.Red), 1), new Point(0, 0), new Point(800, 0));
            graphic.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            System.Drawing.Color color = System.Drawing.Color.Black;

            Brush mBrush = new SolidBrush(color);
            float drawX = 0;
            float drawY = 1000;
            StringFormat stringFormat = StringFormat.GenericTypographic;

            var drawPen = new Pen(new SolidBrush(System.Drawing.Color.Red), 1);

            graphic.DrawLine(drawPen, new Point(0, (int)drawY), new Point(800, (int)drawY));

            foreach (string fontName in fontNames)
            {
                System.Drawing.Font font = new Font(fontName, 216);
                FontFamily fontFamily = font.FontFamily;
                FontStyle fontStyle = font.Style;

                PointF p = new PointF(drawX, drawY);

                DrawOnBaseline(text, graphic, font, mBrush, p);

                drawX += 900;
            }
            string fileNamePNG = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), Path.ChangeExtension(filePath, ".png"));
            image.Save(fileNamePNG, ImageFormat.Png);

            graphic.Dispose();
        }

        private static void DrawOnBaseline(string s, Graphics g, Font f, Brush b, PointF pos)
        {
            float baselineOffset = f.SizeInPoints / f.FontFamily.GetEmHeight(f.Style) * f.FontFamily.GetCellAscent(f.Style);
            float baselineOffsetPixels = g.DpiY / 72f * baselineOffset;

            g.DrawString(s, f, b, new PointF(pos.X, pos.Y - (int)(baselineOffsetPixels + 0.5f)), StringFormat.GenericTypographic);
        }
    }
}