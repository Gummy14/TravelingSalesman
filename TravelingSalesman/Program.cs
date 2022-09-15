using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravelingSalesman
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] coordinates = ReadFile(@"C:\Users\alex_\source\repos\TravelingSalesman\TravelingSalesman\data\Data10.txt");
            List<float[]> xyCoordinates = GetCoordinates(coordinates);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(xyCoordinates));
        }
        private static string[] ReadFile(string file)
        {
            string[] lines = File.ReadAllLines(file);
            return lines;
        }
        private static List<float[]> GetCoordinates(string[] coordinates)
        {
            List<float[]> values = new List<float[]>();
            for (int a = 0; a < coordinates.Length; a++)
            {
                int commaPosition = coordinates[a].IndexOf(",");
                string yCoString = coordinates[a].Substring(0, commaPosition);
                string xCoString = coordinates[a].Substring(commaPosition + 2);
                float[] arrayToAddToList = new float[2] { float.Parse(xCoString), float.Parse(yCoString) };
                values.Add(arrayToAddToList);
            }
            return values;
        }
    }
}
