using OPTANO.Modeling.Common;
using OPTANO.Modeling.Optimization;
using OPTANO.Modeling.Optimization.Configuration;
using OPTANO.Modeling.Optimization.Solver.Z3;
using Prototype.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prototype
{
    public partial class formDiet : Form
    {
        public double MaxCalorie { get; set; }
        public double MaxFat { get; set; }
        public double AdvProtein { get; set; }
        public double MaxSugar { get; set; }
        public double MaxCholesterol { get; set; }
        public double AdvFibers { get; set; }

        private List<DisplayName> displayNames;

        public formDiet()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string Compute()
        {
            List<FoodItem> foodItems = null;

            using (var db = new FoodContext())
            {
                foodItems = db.FoodItems.Take(20).ToList();
            }

            var config = new Configuration();
            config.NameHandling = NameHandlingStyle.UniqueLongNames;
            config.ComputeRemovedVariables = true;

            string html = "<html><head><title>Diet Suggestion</title>";
            using (var scope = new ModelScope())
            {
                var foodModel = new DietModel(foodItems,
                    MaxCalorie,
                    MaxFat,
                    AdvProtein,
                    MaxSugar,
                    MaxCholesterol,
                    AdvFibers
                    );

                using (var solver = new Z3Solver())
                {
                    var solution = solver.Solve(foodModel.Model);

                    foodModel
                        .Model
                        .VariableCollections
                        .ForEach(ValidateChildren => 
                            ValidateChildren.SetVariableValues(solution.VariableValues));

                   
                    html += @"<link rel='stylesheet' href='./Styles/dietstyle.css'></head><body>";
                    html += "<h2>Diet suggestion based on optimization</h2>";
                    html += "<p><table><thead><tr><th>Product</th><th>Amount</th></tr></thead>";
                    html += "<tbody>";

                    foreach (var stat in foodModel.Model.VariableStatistics)
                    {
                        if (stat.SolutionValue > 0)
                        {
                            var displayName = MatchDisplayName(stat.LongName);
                            html += $"<tr><td>{ displayName }</td><td>{ Math.Round(stat.SolutionValue) }</td></tr>";
                        }
                    }

                    html += "</tbody></table></p>";
                    html += $"<p>Total amount of calories: { MaxCalorie - solution.ObjectiveValues.Single().Value}</p>";
                    html += "<p><table><thead><th>Category</th><th>Realized value</th><th>Target value</th><th>Target</th></thead><tbody>";

                    foreach (var constraint in foodModel.Model.Constraints)
                    {
                        string label;
                        string idealValue;
                        string constrType;
                        switch (constraint.Name)
                        {
                            case "A":
                                label = "Calories";
                                idealValue = MaxCalorie.ToString("F0");
                                constrType = "Maximum";
                                break;
                            case "B":
                                label = "Fat";
                                idealValue = MaxFat.ToString("F0");
                                constrType = "Maximum";
                                break;
                            case "C":
                                label = "Protein";
                                idealValue = AdvProtein.ToString("F0");
                                constrType = "Advised";
                                break;
                            case "D":
                                label = "Sugar";
                                idealValue = MaxSugar.ToString("F0");
                                constrType = "Maximum";
                                break;
                            case "E":
                                label = "Cholesterol";
                                idealValue = MaxCholesterol.ToString("F3");
                                constrType = "Maximum";
                                break;
                            case "F":
                                label = "Fibers";
                                idealValue = AdvFibers.ToString("F0");
                                constrType = "Advised";
                                break;
                            default:
                                label = constraint.Name;
                                idealValue = "";
                                constrType = "";
                                break;
                        }
                        html += $"<tr><td>{ label }</td><td>{ constraint.Expression.Evaluate(solution.VariableValues) }</td><td>{ idealValue}</td><td>{ constrType }</td></tr>";
                    }

                    html += "</tbody></table></p></body></html>";                  
                }
            }

            return html;
        }

        private async void formDiet_Shown(object sender, EventArgs e)
        {
            var tokenSource = new CancellationTokenSource();
            var ct = tokenSource.Token;

            var progressBarRunningTask = new Task(() =>
              {
                  int counter = 0;
                  while (!ct.IsCancellationRequested)
                  {
                      progressBar1.Invoke(new Action(() => { progressBar1.Value = counter; }));
                      Thread.Sleep(500);
                      counter++;
                      if (counter > 100)
                      {
                          counter = 0;
                      }
                  }
              }, ct);
            progressBarRunningTask.Start();
            var computeTask = new Task<string>(() => Compute());
            computeTask.Start();

            var tasks = new Task[] { progressBarRunningTask, computeTask };

            var masterTask = new Task(() => Task.WaitAny(tasks));
            masterTask.Start();

            await masterTask;

            var html = computeTask.Result;

            using (var fileStream = new FileStream(@"Output\\result.html", FileMode.Create))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    await writer.WriteLineAsync(html);
                    await writer.FlushAsync();
                }
            }
            webBrowser1.Navigate("file:///D:/MyProjects/Demo/Prototype/Prototype/bin/Debug/Output/result.html");
            tokenSource.Cancel();
            label1.Visible = false;
            progressBar1.Visible = false;
            button1.Visible = true;
        }

        private void formDiet_Load(object sender, EventArgs e)
        {
            using (var db = new FoodContext())
            {
                displayNames = db.DisplayNames.ToList();
            }
        }

        private string MatchDisplayName(string name)
        {
            var splits = name.Split('_');
            name = splits[0];
            var dispName = (from d in displayNames
                            where d.Name == name
                            select d.NiceName).FirstOrDefault();
            return dispName;
        }
    }
}
