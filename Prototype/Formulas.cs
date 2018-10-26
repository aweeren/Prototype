namespace Prototype
{
    public enum ActivityLevel
    {
        Light = 0,
        Medium = 1,
        Heavy = 2
    }

    public static class Formulas
    {
        public static double AdvisedCalories(double age, double weight, double height, string gender, ActivityLevel activity)
        {
            double activityMultiplier = 0;
            switch (activity)
            {
                case ActivityLevel.Light:
                    activityMultiplier = 1.53;
                    break;
                case ActivityLevel.Medium:
                    activityMultiplier = 1.76;
                    break;
                case ActivityLevel.Heavy:
                    activityMultiplier = 2.25;
                    break;
            }

            double bmr = 0; // Adjusted Harris-Benedict formula
            if (gender == "male")
            {
                bmr = 10 * weight + 6.25 * height - 5 * age + 5;
            }
            else
            {
                bmr = 10 * weight + 6.25 * height - 5 * age - 161;
            }

            return bmr * activityMultiplier;
        }

        public static double AdvicedFat(double calories)
        {
            return 50 * calories / 2000;
        }

        public static double AdvicedProtein(double weight)
        {
            return 0.6 * weight;
        }

        public static double AdvicedSugars(double calories)
        {
            return calories / 10;
        }

        public static double AdvicedFiber(double calories)
        {
            return 25 * calories / 2000;
        }
    }
}
