using OPTANO.Modeling.Optimization;
using OPTANO.Modeling.Optimization.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Prototype.Models
{
    public class DietModel
    {
        public DietModel(List<FoodItem> foodItems, double maxCalorie, double maxFat, double advProtein, double maxSugar, double maxCholesterol, double advFibers)
        {
            FoodItems = foodItems;
            Model = new Model();

            x = new VariableCollection<FoodItem>(
                Model,
                FoodItems,
                "x",
                foodItem => foodItem.Name,
                foodItem => 0,  // Lowerbound
                foodItem => foodItem.MaxServings, // Upperbound
                foodItem => VariableType.Integer
                );

            var energyConstraint = new Constraint(Expression.Sum(FoodItems.Select(f => x[f] * f.Energy)),
                "Energy",
                upperBound: maxCalorie);
            var fatConstraint = new Constraint(Expression.Sum(FoodItems.Select(f => x[f] * f.Fat)),
                "Fat",
                upperBound: maxFat);
            var proteinConstraint = new Constraint(Expression.Sum(FoodItems.Select(f => x[f] * f.Protein)),
                "Protein",
                lowerBound: 0.95 * advProtein,
                upperBound: 1.05 * advProtein);
            var sugarConstraint = new Constraint(Expression.Sum(FoodItems.Select(f => x[f] * f.Sugars)),
                "Sugars",
                upperBound: maxSugar);
            var cholesterolConstraint = new Constraint(Expression.Sum(FoodItems.Select(f => x[f] * f.Cholesterol)),
                "Cholesterol",
                upperBound: maxCholesterol);
            var fiberConstraint = new Constraint(Expression.Sum(FoodItems.Select(f => x[f] * f.Fiber)),
                "Fibers",
                lowerBound: 0.95 * advFibers,
                upperBound: 1.1 * advFibers);
            // Maximum # items in menu


            var allConstraints = new List<Constraint>() {
                energyConstraint,
                fatConstraint,
                proteinConstraint,
                sugarConstraint,
                cholesterolConstraint,
                fiberConstraint
            };

            Model.AddConstraints(allConstraints);
            Model.AddObjective(new Objective(maxCalorie - Expression.Sum(foodItems.Select(f => x[f] * f.Energy)),
                "sum of calories",
                ObjectiveSense.Minimize));
        }

        public Model Model { get; private set; }
        public List<FoodItem> FoodItems { get; }

        public VariableCollection<FoodItem> x { get; }

    }
}
