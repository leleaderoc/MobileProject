using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MobileComputingUI
{
    public partial class Form1 : Form
    {
        public List<DataTable> Data;
        public Form1()
        {
            InitializeComponent();
            this.Data = PrepareInputs();

            foreach (var item in this.Data)
            {
                this.listBox1.Items.Add(item.TableName);
            }

            this.listBox2.Items.Add("X-Y-Z");

            for (int i = 1; i < this.Data[0].Columns.Count; i++)
            {
                this.listBox2.Items.Add(this.Data[0].Columns[i].ColumnName);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.PrepareChart();
        }

        private List<DataTable> PrepareInputs()
        {
            //Directory.SetCurrentDirectory("C:\\Users\\önder\\Documents\\Master II\\Mobile\\Project");
            StreamReader input = new StreamReader("ActFeaturesData_Onder_0f74a0.csv");

            string[] header = input.ReadLine().Split(',');
            string line = input.ReadLine();
            List<DataTable> result = new List<DataTable>();
            DataTable dt = null;

            DateTime? previous = null;

            while (line != null && line.Trim() != "")
            {
                string[] inputs = line.Split(',');
                DateTime date = DateTime.ParseExact(inputs[0], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                if (!previous.HasValue || Math.Abs((date - previous.Value).TotalSeconds) > 3)
                {
                    dt = new DataTable();
                    dt.TableName = String.Format("{0:s}", date);

                    dt.Columns.Add(header[0], typeof(DateTime));
                    for (int i = 1; i < header.Length - 3; i++)
                    {
                        dt.Columns.Add(header[i], typeof(Double));
                    }
                    dt.Columns.Add(header[12], typeof(string));
                    dt.Columns.Add(header[13], typeof(string));

                    result.Add(dt);
                }

                dt.Rows.Add(date, Convert.ToDouble(inputs[1]), Convert.ToDouble(inputs[2]), Convert.ToDouble(inputs[3]),
                    Convert.ToDouble(inputs[4]), Convert.ToDouble(inputs[5]), Convert.ToDouble(inputs[6]),
                    Convert.ToDouble(inputs[7]), Convert.ToDouble(inputs[8]), Convert.ToDouble(inputs[9]),
                    Convert.ToDouble(inputs[10]), Convert.ToDouble(inputs[11]), inputs[12], inputs[13]);

                previous = date;

                line = input.ReadLine();
            }

            result.Sort((dt1, dt2) => ((DateTime)dt1.Rows[0]["Time"]).CompareTo((DateTime)dt2.Rows[0]["Time"]));

            for (int i = 1; i < result.Count; i++)
            {
                if ((DateTime)result[i].Rows[0]["Time"] == (DateTime)result[i - 1].Rows[0]["Time"])
                {
                    result.RemoveAt(i);
                    i--;
                }
            }

            return result;
        }

        private void PrepareChart()
        {
            var series = this.chart1.Series[0];
            this.chart1.Series.Clear();
            this.chart1.Series.Add(series);

            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[0].XValueMember = "Time";

            if (this.listBox1.SelectedItem == null)
                this.listBox1.SelectedItem = this.listBox1.Items[0];

            if (this.listBox2.SelectedItem == null)
                this.listBox2.SelectedItem = this.listBox2.Items[0];

            DataTable current = this.Data.Find(dt => dt.TableName == (string)this.listBox1.SelectedItem);
            string column = (string)this.listBox2.SelectedItem;
            this.chart1.Series[0].Name = column;

            if (this.listBox2.SelectedIndex == 0)
            {
                this.chart1.Series.Add(new Series());
                this.chart1.Series.Add(new Series());

                this.chart1.Series[0].Name = "X";
                this.chart1.Series[0].ChartType = SeriesChartType.Spline;

                this.chart1.Series[1].Name = "Y";
                this.chart1.Series[1].Color = Color.Red;
                this.chart1.Series[1].ChartType = SeriesChartType.Spline;

                this.chart1.Series[2].Name = "Z";
                this.chart1.Series[2].Color = Color.Purple;
                this.chart1.Series[2].ChartType = SeriesChartType.Spline;

                DateTime startTime = (DateTime)current.Rows[0]["Time"];

                for (int i = 0; i < current.Rows.Count; i++)
                {
                    string timeS = ((DateTime)current.Rows[i]["Time"] - startTime).ToString(@"mm\:ss");
                    this.chart1.Series[0].Points.AddXY(timeS, (double)current.Rows[i]["sdX"]);
                    this.chart1.Series[1].Points.AddXY(timeS, (double)current.Rows[i]["sdY"]);
                    this.chart1.Series[2].Points.AddXY(timeS, (double)current.Rows[i]["sdZ"]);
                }
            }
            else if (this.listBox2.SelectedIndex < this.listBox2.Items.Count - 2)
            {
                this.chart1.Series[0].ChartType = SeriesChartType.Spline;
                DateTime startTime = (DateTime)current.Rows[0]["Time"];

                for (int i = 0; i < current.Rows.Count; i++)
                {
                    this.chart1.Series[0].Points.AddXY(((DateTime)current.Rows[i]["Time"] - startTime).ToString(@"mm\:ss"), (double)current.Rows[i][column]);
                }
            }
            else
            {
                this.chart1.Series[0].ChartType = SeriesChartType.Column;
                Dictionary<string, int> keyVals = new Dictionary<string, int>();

                for (int i = 0; i < current.Rows.Count; i++)
                {
                    string currentVal = (string)current.Rows[i][column];
                    int value = keyVals.ContainsKey(currentVal) ? keyVals[currentVal] : 0;
                    value++;
                    keyVals[currentVal] = value;
                }

                foreach (string key in keyVals.Keys)
                    this.chart1.Series[0].Points.AddXY(key, keyVals[key]);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.PrepareChart();
        }
    }
}
