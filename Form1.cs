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
        public Node Head;
        public List<LocationData> LocData;

        public Form1()
        {
            InitializeComponent();
            this.Data = PrepareInputs();
            this.Head = PrepareDecisionTree();
            this.LocData = PrepareLocationData();
            PrepareOutputs();
            TestUnlabeledData();

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

        private List<LocationData> PrepareLocationData()
        {
            StreamReader input = new StreamReader("LocationInfoData_Onder_0f74a0.csv");

            string[] header = input.ReadLine().Split(',');
            string line = input.ReadLine();
            List<string> readerData = new List<string>();

            while (line != null && line.Trim() != "")
            {
                if(!readerData.Contains(line))
                    readerData.Add(line);

                line = input.ReadLine();
            }

            readerData.Sort();

            line = readerData[0];

            string[] inputs = line.Split(',');

            List<LocationData> result = new List<LocationData>();
            LocationData lt = new LocationData();

            lt.MagnitudeList.Add(Math.Sqrt(Math.Pow(Convert.ToDouble(inputs[2]), 2) + Math.Pow(Convert.ToDouble(inputs[3]), 2) + Math.Pow(Convert.ToDouble(inputs[4]), 2)));
            double CumSpeed = Convert.ToDouble(inputs[5]);
            int count = 1;

            DateTime startDate = DateTime.ParseExact(inputs[0], "dd.MM.yyyy_HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime previous = startDate;

            for(int i=1;i<readerData.Count;i++)
            {
                line = readerData[i];
                inputs = line.Split(',');
                DateTime date = DateTime.ParseExact(inputs[0], "dd.MM.yyyy_HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                double currMag = Math.Sqrt(Math.Pow(Convert.ToDouble(inputs[2]), 2) + Math.Pow(Convert.ToDouble(inputs[3]), 2) + Math.Pow(Convert.ToDouble(inputs[4]), 2));
                double currSpeed = Convert.ToDouble(inputs[5]);

                if (Math.Abs((date - previous).TotalSeconds) > 60)
                {
                    lt.StartDate = startDate;
                    lt.EndDate = previous;
                    lt.AverageSpeed = CumSpeed / count;
                    result.Add(lt);

                    lt = new LocationData();
                    lt.MagnitudeList.Add(currMag);
                    CumSpeed = currSpeed;
                    count = 1;

                    startDate = date;
                    previous = date;
                }
                else
                {
                    lt.MagnitudeList.Add(currMag);
                    CumSpeed += currSpeed;
                    count++;

                    previous = date;
                }
            }

            lt.AverageSpeed = CumSpeed / count;
            lt.StartDate = startDate;
            lt.EndDate = previous;
            result.Add(lt);

            return result;
        }

        private void SetWriter(ref FileStream ostrm, ref StreamWriter writer, string text)
        {
            try
            {
                ostrm = new FileStream(string.Format("./{0}.csv", text), FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(e.Message);
                ostrm = null;
                writer = null;
                return;
            }
        }

        private void FreeWriter(FileStream ostrm, StreamWriter writer)
        {
            if (ostrm != null && writer != null)
            {
                writer.Close();
                ostrm.Close();
            }
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

        private void PrepareOutputs()
        {
            FileStream ostrm = null;
            StreamWriter writer = null;
            SetWriter(ref ostrm, ref writer, "PreProcessedData");

            writer.WriteLine("Time,Mean,sdX,sdY,sdZ,Var,FFT1,FFT2,FFT3,FFT4,PITCH,ROLL,Ori,Act");

            List<DateTime> attackList = new List<DateTime>() { new DateTime(2015, 05, 15), new DateTime(2015, 05, 25), new DateTime(2015, 05, 26) };
            List<DateTime> walkList = new List<DateTime>() { new DateTime(2015, 05, 17), new DateTime(2015, 05, 18), new DateTime(2015, 05, 21), new DateTime(2015, 05, 28), new DateTime(2015, 05, 29) };

            List<DataTable> filteredData = this.Data.FindAll(d => d.Rows.Count >= 40 && ((DateTime)d.Rows[0][0]) > new DateTime(2015, 05, 14) && ((DateTime)d.Rows[0][0]) < new DateTime(2015, 05, 30) && ((DateTime)d.Rows[0][0]).Hour >= 11 && ((DateTime)d.Rows[0][0]).Hour <= 20);

            int attackCount = 0;
            int walkingCount = 0;
            int accuracy = 0;

            foreach(DataTable dt in filteredData)
            {
                bool isAttack = false;
                if (attackList.Contains(((DateTime)dt.Rows[0][0]).Date))
                    isAttack = true;
                else if (!walkList.Contains(((DateTime)dt.Rows[0][0]).Date))
                    continue;
                
                for (int j = 3; j < dt.Rows.Count - 3;j++ )
                {
                    DataRow row = dt.Rows[j];

                    string type = ((string)row[13]).Trim();

                    if (type == "Walking" || (isAttack && type == "Trans"))
                    {
                        string entry = ((DateTime)row[0]).ToString("dd.MM.yyyy HH:mm:ss") + ",";

                        for (int i = 1; i < 13; i++)
                            entry += string.Format("{0},", row[i]);

                        entry += (isAttack ? "Attack" : "Walking");

                        writer.WriteLine(entry);

                        if (isAttack)
                            attackCount++;
                        else
                            walkingCount++;

                        if (isAttack == IsAttack(this.Head, row))
                            accuracy++;
                    }
                }
            }

            FreeWriter(ostrm, writer);

            Console.WriteLine("Attack Count: " + attackCount);
            Console.WriteLine("Walking Count: " + walkingCount);
            Console.WriteLine("True Count: " + accuracy);
            Console.WriteLine("Accuracy: " + ((double)accuracy) / ((double)(attackCount + walkingCount)));
        }

        private void TestUnlabeledData()
        {
            List<DataTable> filteredData = this.Data.FindAll(d => d.Rows.Count >= 40 && ((DateTime)d.Rows[0][0]) < new DateTime(2015, 05, 14));

            double cumAttack = 0;
            double countAttack = 0;
            double cumSpeedAttack = 0;
            double cumWalking = 0;
            double countWalking = 0;
            double cumSpeedWalking = 0;
            Dictionary<LocationData, int[]> dic = new Dictionary<LocationData, int[]>();

            foreach (DataTable dt in filteredData)
            {
                for (int j = 3; j < dt.Rows.Count - 3; j++)
                {
                    DataRow row = dt.Rows[j];
                    string type = ((string)row[13]).Trim();

                    if (type == "Walking" || type == "Trans")
                    {
                        DateTime date = (DateTime)row[0];
                        LocationData ld = this.LocData.Find(l => Math.Abs((l.StartDate - date).TotalSeconds) <= 10 || Math.Abs((l.EndDate - date).TotalSeconds) <= 10);

                        if (ld != null)
                        {
                            if (!dic.ContainsKey(ld))
                                dic.Add(ld, new int[2]);

                            if (IsAttack(this.Head, row))
                            {
                                cumAttack += ld.StandardDeviation;
                                cumSpeedAttack += ld.AverageSpeed;
                                countAttack++;

                                dic[ld][0]++;
                            }
                            else
                            {
                                cumWalking += ld.StandardDeviation;
                                cumSpeedWalking += ld.AverageSpeed;
                                countWalking++;

                                dic[ld][1]++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("----------------------------");
            Console.WriteLine("Unlabeled Data Analysis:");
            Console.WriteLine("Attack Count: " + ((int)countAttack));
            Console.WriteLine("Mean of SD for Attack: " + (cumAttack / countAttack));
            Console.WriteLine("Average Speed for Attack: " + (cumSpeedAttack / countAttack));
            Console.WriteLine("Walking Count: " + ((int)countWalking));
            Console.WriteLine("Mean of SD for Walking: " + (cumWalking / countWalking));
            Console.WriteLine("Average Speed for Walking: " + (cumSpeedWalking / countWalking));

            Console.WriteLine("----------------------------");

            int countValid = 0;
            foreach(var item in dic.Keys)
            {
                double percentage = (double)(dic[item][0]) / ((double)(dic[item][0] + dic[item][1]));

                if (percentage >= 0.9 || percentage <= 0.1)
                    countValid++;
            }

            Console.WriteLine("Location Data Count: " + dic.Keys.Count);
            Console.WriteLine("Number of Valid Zone: " + countValid);
        }

        private Node PrepareDecisionTree()
        {
            Stack<Node> stack = new Stack<Node>();

            Node current = new Node("FFT4", 55.200763);
            current.SmallerNode = new Node("ROLL", -124.773807);
            stack.Push(current);
            current = current.SmallerNode;

            current.SmallerNode = new Node("Var", 7.387613);
            stack.Push(current);
            current = current.SmallerNode;

            current.SmallerNode = new Node("ROLL", -148.333742);
            stack.Push(current);
            current = current.SmallerNode;

            current.SmallerNode = new Node("FFT1", 2.692375);
            stack.Push(current);
            current = current.SmallerNode;

            current.SmallerNode = new Node(false);
            current.BiggerNode = new Node("FFT2", 4.733851);
            stack.Push(current);
            current = current.BiggerNode;

            current.SmallerNode = new Node(false);
            current.BiggerNode = new Node(true);

            stack.Pop();
            current = stack.Pop();
            current.BiggerNode = new Node(true);

            current = stack.Pop();
            current.BiggerNode = new Node(true);

            current = stack.Pop();
            current.BiggerNode = new Node("PITCH", 11.410085);
            stack.Push(current);
            current = current.BiggerNode;

            current.SmallerNode = new Node(false);
            current.BiggerNode = new Node("sdZ", 1.774587);
            stack.Push(current);
            current = current.BiggerNode;

            current.SmallerNode = new Node(true);
            current.BiggerNode = new Node("sdX", 2.542402);
            stack.Push(current);
            current = current.BiggerNode;

            current.SmallerNode = new Node(false);
            current.BiggerNode = new Node(true);

            stack.Pop();
            stack.Pop();
            stack.Pop();
            current = stack.Pop();

            current.BiggerNode = new Node("PITCH", 11.410085);
            stack.Push(current);
            current = current.BiggerNode;

            current.SmallerNode = new Node(false);
            current.BiggerNode = new Node("FFT4", 107.390618);
            stack.Push(current);
            current = current.BiggerNode;

            current.SmallerNode = new Node("FFT2", 71.544493);
            stack.Push(current);
            current = current.SmallerNode;

            current.SmallerNode = new Node("ROLL", 72.970432);
            stack.Push(current);
            current = current.SmallerNode;

            current.SmallerNode = new Node(true);
            current.BiggerNode = new Node(false);

            current = stack.Pop();
            current.BiggerNode = new Node(false);

            current = stack.Pop();
            current.BiggerNode = new Node(false);

            stack.Pop();

            return stack.Pop();
        }

        private bool IsAttack(Node head, DataRow dr)
        {
            double current = (double)dr[head.Feature];

            if(current>head.Value)
            {
                if (string.IsNullOrEmpty(head.BiggerNode.Feature))
                    return head.BiggerNode.Attack;
                else
                    return IsAttack(head.BiggerNode, dr);
            }
            else
            {
                if (string.IsNullOrEmpty(head.SmallerNode.Feature))
                    return head.SmallerNode.Attack;
                else
                    return IsAttack(head.SmallerNode, dr);
            }
        }
    }

    public class Node
    {
        public string Feature;

        public bool Attack;

        public double Value;

        public Node BiggerNode;

        public Node SmallerNode;

        public Node(string p_Feature, double p_Value)
        {
            this.Feature = p_Feature;
            this.Value = p_Value;
        }

        public Node(bool p_Attack)
        {
            this.Attack = p_Attack;
        }
    }

    public class LocationData 
    {
        public DateTime StartDate;

        public DateTime EndDate;

        public List<double> MagnitudeList;

        public double AverageSpeed;

        public LocationData()
        {
            this.MagnitudeList = new List<double>();
        }

        private double? standardDeviation;
        public double StandardDeviation
        {
            get
            {
                if (!standardDeviation.HasValue)
                    standardDeviation = this.CalculateSD();

                return standardDeviation.Value;
            }
        }

        private double CalculateSD()
        {
            if(MagnitudeList.Count==0)
                return 0;

            double mean = 0;
            MagnitudeList.ForEach(m => mean += m);
            mean /= MagnitudeList.Count;

            double variance = 0;
            MagnitudeList.ForEach(m => variance += ((m-mean)*(m-mean)));
            variance /= MagnitudeList.Count;

            return Math.Sqrt(variance);
        }
    }
}
