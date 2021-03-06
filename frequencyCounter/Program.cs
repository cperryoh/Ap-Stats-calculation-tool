﻿using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace frequencyCounter
{
    class Program
    {
        static MathNet.Numerics.Distributions.Normal result = new MathNet.Numerics.Distributions.Normal();
        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);
        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);
        class ListObject
        {

            //name of the object
            public string name;

            //number of occurrences of said object
            //start at one becuase when this object is created it has been determained that there is at least one occurrence of an object with this name
            public int count = 1;

            //set name to the one recived in the constructor
            public ListObject(string name)
            {
                this.name = name;
            }
        }

        //finds if a ListObject is in a list with a certain name
        static ListObject findObj(List<ListObject> objects, string searchItem)
        {

            //loop through every object in objects
            foreach (ListObject i in objects)
            {

                //if i's name is the one we are looking for return i
                if (i.name.Equals(searchItem))
                {
                    return i;
                }
            }

            //if an object with the name we are looking for is not found return null
            return null;
        }
        static double percentToZ(double percent)
        {
            double closest = double.MaxValue;
            for (double i = -6; i <= 6f; i += 0.01)
            {

                //convert i zscore to percentile
                double z = result.CumulativeDistribution(i);

                //if z is closer to our requested percentile than our previous one, use z
                if (Math.Abs(z - percent) < Math.Abs(result.CumulativeDistribution(closest) - percent) && z > percent)
                {

                    //set closest zscore to i
                    closest = i;
                }
            }
            return closest;
        }
        //parse list of strings in csv format
        static List<string> pareseStringCsv(string csv)
        {

            //remove any spaces
            csv = csv.Replace("  ", " ");
            csv = csv.Replace(", ", ",");
            csv = csv.Replace(" ,", ",");

            //list of strings data will be parsed into
            List<string> parsedCsv = new List<string>();

            //take out doulbe commas
            csv = csv.Replace(",,", ",");

            //loop while there are still commas in the string and its not the last char
            while (csv.IndexOf(",") != -1 && csv.IndexOf(",") != csv.Length - 1)
            {

                //string without commas
                string curStr = csv.Substring(0, csv.IndexOf(","));

                //add parsed string
                parsedCsv.Add(curStr);

                //remove from string
                csv = csv.Remove(0, csv.IndexOf(",") + 1);

            }

            //add last item to list
            parsedCsv.Add(csv);

            //retrun list
            return parsedCsv;
        }

        //parseIntCsv
        static List<double> parseNumCsv(string csv)
        {
            //remove any spaces
            csv = csv.Replace("  ", " ");
            csv = csv.Replace(", ", ",");
            csv = csv.Replace(" ,", ",");

            //list of strings data will be parsed into
            List<double> nums = new List<double>();

            //loop while there are still commas in the string and its not the last char
            while (csv.IndexOf(",") != -1 && csv.IndexOf(",") != csv.Length - 1)
            {

                //parse out possible double
                string curNum = csv.Substring(0, csv.IndexOf(","));

                //figure out if curNum is actually an int, if not eliminate if
                try
                {
                    double num = double.Parse(curNum);
                    nums.Add(num);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(curNum + " is invalid, eliminating from the data set");
                }

                //remove item from index
                csv = csv.Remove(0, csv.IndexOf(",") + 1);
            }

            //do the same thing but for the last item
            try
            {
                double num = double.Parse(csv);
                nums.Add(num);
            }
            catch (FormatException e)
            {
                Console.WriteLine(csv + " is invalid, eliminating from the data set");
            }

            //return list
            return nums;
        }
        static List<double> getDataSet()
        {
            //list of inputs 
            List<double> nums = new List<double>();

            //prompt user for intial input
            Console.Write("Enter a comma seperated data set, one data point or 'done':");
            string input = Console.ReadLine();
            do
            {

                //determain if input is singluar numbe or csv
                if (input.Contains(","))
                {

                    //add csv list to list
                    nums.AddRange(parseNumCsv(input));


                }
                else if (isNumber(input))
                {

                    //add singular number to list
                    nums.Add(double.Parse(input));
                }
                else if (input.Equals("print"))
                {
                    sortList(ref nums);
                    for (int i = 0; i < nums.Count - 1; i++)
                    {
                        Console.Write(nums[i] + ", ");
                    }
                    Console.Write(nums[nums.Count - 1] + "\n\n");
                }


                //get new input
                Console.Write("Enter a comma seperated data set, one data point or 'done': ");
                input = Console.ReadLine();

            } while (!input.Equals("done"));
            return nums;
        }
        static double getZScorePercent(double z)
        {
            return Math.Round(result.CumulativeDistribution(z), 4);
        }
        static Dictionary<string, double> getStats(List<double> nums)
        {
            Dictionary<string, double> stats = new Dictionary<string, double>();


            double mean, median, iqr, lq, uq;


            //get mean
            double sum = 0;
            foreach (int i in nums)
            {
                sum += i;
            }

            //get data on mean
            mean = sum / (double)nums.Count;
            double difTotal = 0;
            double range = nums[nums.Count - 1] - nums[0];
            double standerdDeviation = 0;
            foreach (double i in nums)
            {
                difTotal += (double)Math.Pow(i - mean, 2);
            }
            standerdDeviation = (double)Math.Sqrt(difTotal / ((double)nums.Count - 1));

            //get median
            median = getMedian(nums);

            //Lower quartile
            if (nums.Count % 2 == 0)
            {
                uq = getMedian(nums.GetRange(nums.Count / 2, nums.Count - (nums.Count / 2)));
                lq = getMedian(nums.GetRange(0, nums.Count / 2));
            }
            else
            {
                uq = getMedian(nums.GetRange(nums.Count / 2 + 1, nums.Count - (nums.Count / 2 + 1)));
                lq = getMedian(nums.GetRange(0, nums.Count / 2));
            }
            iqr = uq - lq;
            double outLierLowerRange = lq - (iqr * 1.5f);
            int outliers = 0;
            double outLierUpperRange = uq + (iqr * 1.5f);
            foreach (int i in nums)
            {
                if (i < outLierLowerRange || i > outLierUpperRange)
                {
                    outliers++;
                }
            }

            //calc ±1s
            double l1 = mean - standerdDeviation, u1 = mean + standerdDeviation;

            //calc ±2 s
            double l2 = mean - (standerdDeviation * 2), u2 = mean + (standerdDeviation * 2);

            //calc ±3 s
            double l3 = mean - (standerdDeviation * 3), u3 = mean + (standerdDeviation * 3);

            //add data to dictionary
            stats.Add("l1", l1);
            stats.Add("u1", u1);
            stats.Add("l2", l2);
            stats.Add("u2", u2);
            stats.Add("l3", l3);
            stats.Add("u3", u3);
            stats.Add("mean", mean);
            stats.Add("range", range);
            stats.Add("sd", standerdDeviation);
            stats.Add("median", median);
            stats.Add("iqr", iqr);
            stats.Add("uq", uq);
            stats.Add("lq", lq);
            stats.Add("lower range", outLierLowerRange);
            stats.Add("upper range", outLierUpperRange);
            stats.Add("outliers", outliers);
            return stats;

        }
        public static double factorial(double number)
        {
            double result = 1;
            while (number != 1)
            {
                result = result * number;
                number = number - 1;
            }
            return result;
        }
        //determains if a string can be converted to a string
        static bool isNumber(string input)
        {

            //try catch clause
            try
            {

                //convert input to a double
                double.Parse(input);

                //if the conversion goes through, it can be a number
                return true;
            }

            //if the conversion fails, string can not be an number
            catch (FormatException e)
            {
                return false;
            }
        }
        static double binomialDis(double x, double trials, double prob)
        {
            
            double numOfCombos = (factorial(trials)) / (factorial(x) * factorial(trials - x));
            double finalProb = numOfCombos * (Math.Pow(1 - prob, trials - x) * Math.Pow(prob, x));
            
            
            return finalProb;
        }
        static void Main(string[] args)
        {
            //list of objects to be counted
            List<ListObject> objects = new List<ListObject>();
            while (true)
            {
                //get mode
                Console.Write("Input a comand or a new element[b- calculate the probs of a binomial dis][c-changes in data][lr- get stats and line for scatter plot][z- convert a z-score to a probality] [f-frequency of a certain type of data][n-get mean, median, and mode from numeric data]: ");
                string calcType = Console.ReadLine().ToLower();

                //calc mode
                if (calcType.Equals("c"))
                {
                    //input var
                    String input = "";


                    //list of numbers for data set
                    List<double> nums = new List<double>();

                    //start of input loop
                    do
                    {

                        //get command or new number
                        Console.Write("[number] or [calc- get data][remove-remove last item added]: ");
                        input = Console.ReadLine().ToLower();

                        //input was calc, find min and max changes
                        if (input.Equals("calc"))
                        {

                            double minChange = double.MaxValue;
                            string fromToMin = "";

                            //loop through array starting at index 1
                            for (int i = 1; i < nums.Count; i++)
                            {
                                //-2.99999 -->2.9999-->2.9
                                //currchange
                                double currChange = Math.Round(Math.Abs(nums[i] - nums[i - 1]), 2);

                                //is cur change less than the min change so far
                                if (currChange < minChange)
                                {

                                    //set min change to curr change
                                    minChange = currChange;

                                    //set explination
                                    fromToMin = String.Format("{0} to {1}", nums[i - 1], nums[i]);
                                }
                            }


                            double maxChange = double.MinValue;
                            string fromToMax = "";

                            //loop through array starting at index 1
                            for (int i = 1; i < nums.Count; i++)
                            {
                                //-2.99999 -->2.9999-->2.9
                                //currChange=cur number - last number
                                double currChange = Math.Round(Math.Abs(nums[i] - nums[i - 1]), 2);

                                //is currChange bigger than last change
                                if (currChange > maxChange)
                                {

                                    //set max change to currChange
                                    maxChange = currChange;

                                    //set explination for change
                                    fromToMax = String.Format("{0} to {1}", nums[i - 1], nums[i]);
                                }
                            }
                            Console.WriteLine(String.Format("Biggest change was {0} and was {1}", maxChange, fromToMax));
                            Console.WriteLine(String.Format("Smallest change was {0} and was {1}", minChange, fromToMin));
                        }

                        //input was remove, remove last item given
                        else if (input.Equals("remove"))
                        {
                            Console.WriteLine(nums[nums.Count - 1] + " was removed.");
                            nums.RemoveAt(nums.Count - 1);
                        }

                        //assume its a number
                        else
                        {

                            //it is number, add to array
                            if (isNumber(input))
                            {
                                nums.Add(double.Parse(input));
                            }

                            //declar input invalid
                            else
                            {
                                Console.WriteLine("That is not a valid input");
                            }
                        }
                    }

                    //keep looping till user asks to calculate data
                    while (!input.Equals("calc"));
                }
                else if (calcType.Equals("b")){
                    Console.Write("q- quit back to main menue c- caclulate a binomial dis: ");
                    String input = Console.ReadLine().ToLower();
                    while (!input.Equals("q"))
                    {
                        if (input.Equals("c"))
                        {
                            Console.Write("Enter the type of binomial calc you would like to do [pdf][cdf]: ");
                            input = Console.ReadLine().ToLower();
                            if (input.Equals("pdf"))
                            {
                                double x = getNumber("Enter x: ");
                                double trials = getNumber("Enter trial count: ");
                                double prob = getNumber("Enter the probablilty of a success: ");

                                Console.WriteLine("P=" + binomialDis(x, trials, prob));
                            }else if (input.Equals("cdf"))
                            {
                                double bot = getNumber("Enter the lower range of x: ");
                                double top = getNumber("Enter the upper range of x: ");
                                double trials = getNumber("Enter the number of trials: ");
                                double prob = getNumber("Enter the probablilty of a success: ");
                                double finalProb = 0;
                                for(int x = (int)bot; x <= top; x++)
                                {
                                    double test = binomialDis(x, trials, prob);
                                    finalProb += Math.Round(binomialDis(x, trials, prob),4);
                                }
                                Console.WriteLine($"p({bot}<=x<={top})={Math.Round(finalProb,4)}");
                            }
                            
                        }

                        else
                        {
                            Console.WriteLine("That is not a valid input, please try again.");
                        }
                        Console.Write("q- quit back to main menue c- caclulate a binomial dis: ");
                        input = Console.ReadLine().ToLower();
                    }
                }
                //option to get data on numeric set
                else if (calcType.Equals("n"))
                {

                    var nums = getDataSet();
                    string copy = "";
                    sortList(ref nums);
                    for (int i = 0; i < nums.Count - 1; i++)
                    {
                        copy += nums[i] + ",";
                    }
                    copy += nums[nums.Count - 1];
                    OpenClipboard(IntPtr.Zero);
                    SetClipboardData(13, Marshal.StringToHGlobalUni(copy));

                    CloseClipboard();
                    var stats = getStats(nums);
                    //print out data
                    Console.WriteLine(String.Format("Range: {3} Mean: {0} Median: {1} Standerd Deviation: {2}", stats["mean"], stats["median"], stats["sd"], stats["range"]));
                    Console.WriteLine("Lq: {0} Uq: {1} IQR: {2} Lower outlier range: {3} Upper outlier range {4} Outlier count: {5}", stats["lq"], stats["uq"], stats["iqr"], stats["lower range"], stats["upper range"], stats["outliers"]);

                    //prints the following ranges of the data set ±1 s from the mean, ±2 s from the mean, and ±3 s from the mean
                    Console.WriteLine($"s±1=({Math.Round(stats["l1"], 2)},{Math.Round(stats["u1"], 2)})\ns±2=({Math.Round(stats["l2"], 2)},{Math.Round(stats["u2"], 2)})\ns±1=({Math.Round(stats["l3"], 2)},{Math.Round(stats["u3"], 2)})");
                }
                else if (calcType.Equals("z"))
                {
                    //get input
                    Console.Write("\n[z-score][calc- get a zscore][normal- enter data set and find out how close it is to the 68-95-99.7 rule][prv- convert percentile to a raw value][p- convert a z-score to a percentile][quit][sub]: ");
                    String input = Console.ReadLine();

                    //zscore=last score->percentile calculated zscore_= last last score->percentile calculated
                    double percent = 0, percent_ = 0;

                    //set up class to calculate zscore



                    //loop till user asks to quit
                    while (!input.Equals("quit"))
                    {

                        //convert zscore to percentile
                        if (isNumber(input))
                        {

                            //move calculated zscores over
                            percent_ = percent;
                            percent = getZScorePercent(double.Parse(input));

                            //print result
                            Console.WriteLine("The percentile for a z-score of {0} is {1}", double.Parse(input), percent);
                            Console.WriteLine(
@"Explination:
Uses zscore table to convert zscore to a percentile(http://www.z-table.com/). 
The tool in the url uses two seprate tables to calculate a score, one for 
positive zscores on for negitve ones.");
                        }
                        else if (input.ToLower().Equals("normal"))
                        {

                            //get data set from user
                            var nums = getDataSet();
                            sortList(ref nums);

                            //get all the stats on the data set
                            var stats = getStats(nums);

                            //pull the mean and standerd deviation
                            double mean = stats["mean"];
                            double sd = stats["sd"];

                            //set up varibles to count numbers withen range
                            double withenOneSd = 0, withenTwoSd = 0, withenThreeSd = 0;

                            //set up array for ranges
                            double[,] ranges = new double[3, 2];


                            //intialize ranges
                            //[0,1]= upper range withen 1 sd
                            for (int i = 0; i < 3; i++)
                            {
                                ranges[i, 0] = mean - (sd * (i + 1));
                                ranges[i, 1] = mean + (sd * (i + 1));
                            }

                            //loop through list and count numbers withen each range
                            for (int i = 0; i < nums.Count; i++)
                            {
                                double number = nums[i];
                                if (number < ranges[0, 1] && number > ranges[0, 0])
                                {
                                    withenOneSd++;
                                }
                                if (number < ranges[1, 1] && number > ranges[1, 0])
                                {
                                    withenTwoSd++;
                                }
                                if (number < ranges[2, 1] && number > ranges[2, 0])
                                {
                                    withenThreeSd++;
                                }
                            }

                            //convert counts to percentiles
                            withenOneSd = Math.Round((withenOneSd / nums.Count) * 100, 1);
                            withenTwoSd = Math.Round((withenTwoSd / nums.Count) * 100, 1);
                            withenThreeSd = Math.Round((withenThreeSd / nums.Count) * 100, 1);

                            //print
                            Console.WriteLine($"{withenOneSd}% are of observations withen 1 standerd deviation in either direction.");
                            Console.WriteLine($"{withenTwoSd}% are of observations withen 2 standerd deviations in either direction.");
                            Console.WriteLine($"{withenThreeSd}% are of observations withen 3 standerd deviations in either direction.");
                        }
                        else if (input.ToLower().Equals("calc"))
                        {

                            //get mean from user
                            double mean = getNumber("Enter the mean: ");

                            //get standerd deviation from user
                            double sd = getNumber("Enter the standerd deviation: ");

                            //get raw value from user
                            double dataPoint = getNumber("Enter the point that you would like the z-score of: ");

                            //calculate z score rounded to nearest 100th
                            double z = Math.Round((dataPoint - mean) / sd, 2);

                            //print
                            Console.WriteLine($"The z-score for {dataPoint} is {z}.");
                            Console.WriteLine(
@"Explination:
Uses the equation, 'zscore=(x-mean)/standerd deviation' to get zscore");


                            //log zscore to be used in sub function
                            percent_ = percent;
                            percent = getZScorePercent(z);

                        }

                        //sub track the last two calculated z scores
                        else if (input.ToLower().Equals("sub"))
                        {

                            //decide which way to subtract
                            if (percent > percent_)
                            {
                                Console.WriteLine("The differnece of the two previously calculated z-scores is {0}-{1}={2}", Math.Round(percent, 4), Math.Round(percent_, 4), Math.Round(percent - percent_, 4));
                            }
                            else
                            {
                                Console.WriteLine("The differnece of the two previously calculated z-scores is {0}-{1}={2}", Math.Round(percent_, 4), Math.Round(percent, 4), Math.Round(percent_ - percent, 4));
                            }
                            Console.WriteLine(
@"Explination:
Uses the last to calculated z scores and subtracts the larger one from the smaller one.");
                        }

                        //converts percentile to a raw value based off mean, and standerd deviation
                        else if (input.ToLower().Equals("prv"))
                        {
                            //get percentile from user
                            double p
                                = getNumber("Enter percentile: ");

                            //get mean from user
                            double mean = getNumber("Enter mean: ");

                            //get standerd deviation from user
                            double deviation = getNumber("Enter standerd deviation: ");

                            //convert percentile to a z score
                            double z = Math.Round(percentToZ(p / 100), 2);

                            //convert z to raw value by solving for x in the following equation
                            //zscore=(x-μ(mean))/σ(standerd deviation)
                            double raw = Math.Round((z * deviation) + mean, 2);

                            //print result
                            Console.WriteLine($"The raw value of a zscore {z} is {raw}");
                            Console.WriteLine(
@"Explination: 
1) Find closest z percent represented by z score on table
2) solve for x in zscore=(x-mean)/standerd deviation
");
                        }
                        //convert percentile to zscore(withen 1 100th)
                        else if (input.ToLower().Equals("p"))
                        {

                            //get percentile from user
                            double percentile = getNumber("Enter a percentile: ") / 100;

                            //loop through zscores -6 to 6 incrementing by 0.01
                            double z = percentToZ(percentile);

                            //print results
                            Console.WriteLine("The closest z score to {0}% is {1}", percentile * 100, Math.Round(z, 2));
                            Console.WriteLine(
@"Explination:
Uses zscore table(http://www.z-table.com/). Loops through entier table and finds the zscore that is closest to the requested percentile while still including it.
Ex: If you need a zscore for 70% choose 0.7019 instead of .6950. Even though 0.6950 is closer to 70% it does not capture it.");
                        }

                        //tell user that the given input was invalid
                        else
                        {
                            Console.WriteLine("That is not a valid command or zscore");
                        }

                        //get new input
                        Console.Write("\n[z-score][calc- get a zscore][normal- enter data set and find out how close it is to the 68-95-99.7 rule][prv- convert percentile to a raw value[p- convert a z-score to a percentile][quit][sub]: ");
                        input = Console.ReadLine();
                    }
                }
                else if (calcType.Equals("lr"))
                {
                    //print out for lr sub program
                    const String options = "Hit enter to continue or type quit then enter to go back to the begining";

                    //print out options
                    Console.WriteLine(options);

                    //get input
                    String input = Console.ReadLine().ToLower();
                    do
                    {

                        //ask for x values
                        Console.WriteLine("For the following data set, please input the x values");
                        List<double> x = getDataSet();

                        //ask for y values
                        Console.WriteLine("For the following data set, please input the y values. Ensure this set is the same length as the previously entered data set");
                        List<double> y = getDataSet();

                        //ensure they are the same length, if not, get them again
                        while (x.Count != y.Count)
                        {
                            Console.WriteLine("The amount of values in each data set are unequal, please try again!");
                            Console.WriteLine("For the following data set, please input the x values");
                            x = getDataSet();
                            Console.WriteLine("For the following data set, please input the y values. Ensure this set is the same length as the previously entered data set");
                            y = getDataSet();
                        }


                        //get stats for x and y
                        var XStats = getStats(x);
                        var YStats = getStats(y);

                        //r varible
                        double r = 1;

                        //sigma, used for part of the r value calcuation
                        double sigma = 0;

                        //loops for sigma calculation
                        for (int i = 0; i < x.Count; i++)
                        {
                            double x_ = (x[i] - XStats["mean"])/XStats["sd"];
                            double y_ = (y[i] - YStats["mean"]) / YStats["sd"];
                            sigma += x_ * y_;
                        }

                        //finish r value calc
                        r = sigma / (x.Count - 1);

                        //calculate slope for the line
                        double b1 = r * (YStats["sd"] / XStats["sd"]);

                        //y intercept for the line
                        double b0 = YStats["mean"] - (b1 * XStats["mean"]);

                        //print out info
                        Console.WriteLine($"Linear regression line is y^={Math.Round(b0,4)}+{Math.Round(b1, 4)}x\nr={Math.Round(r, 4)}\nr^2={Math.Round(Math.Pow(r, 2),2)}");

                        //start again
                        Console.WriteLine(options);
                        input = Console.ReadLine().ToLower();
                    } while (!input.Equals("quit"));
                }
                else if (calcType.Equals("p"))
                {
                    Console.Write("[quit][d-get mean/sd]: ");
                    String input = Console.ReadLine().ToLower();
                    while (!input.Equals("quit"))
                    {
                        if (input.Equals("d"))
                        {
                            Console.WriteLine("Enter all the possible outcomes");
                            var outComes = getDataSet();
                            Console.WriteLine("Enter all the probabilities for the possible outcomes");
                            var probs = getDataSet();
                            while (probs.Count != outComes.Count)
                            {
                                Console.WriteLine("Please re enter the data sets, they were not of equal lengths");
                                Console.WriteLine("Enter all the possible outcomes");
                                outComes = getDataSet();
                                Console.WriteLine("Enter all the probabilities for the possible outcomes");
                                probs = getDataSet();
                            }
                            double mean = 0;
                            for (int i = 0; i < outComes.Count(); i++)
                            {
                                mean += outComes[i] * probs[i];
                            }
                            double sd = 0;
                            for(int i = 0; i < outComes.Count(); i++)
                            {
                                sd += Math.Abs(outComes[i] - mean) * probs[i];
                            }
                            Console.WriteLine($"Sd: {Math.Round(sd, 3)} Variance: {Math.Round(Math.Pow(sd, 2), 3)} Mean: {Math.Round(mean, 3)}");
                        }
                    }
                }
                else if (calcType.Equals("f"))
                {
                    Console.WriteLine("[number][count - counts all the occurrences of each data point]");
                    String input = Console.ReadLine();
                    //if the input is not a comand start to count it
                    do
                    {
                        if (input.Equals("clear"))
                        {
                            Console.WriteLine("List cleared");
                            objects.Clear();
                        }
                        else if (input.Contains(","))
                        {
                            var csv = pareseStringCsv(input);
                            foreach (string i in csv)
                            {
                                ListObject obj = findObj(objects, i);

                                //if obj is not null it is already in the list
                                if (obj != null)
                                {

                                    //add one to the count of the obj
                                    obj.count++;
                                }
                                else
                                {
                                    //the object does not exist yet, create one with the input
                                    ListObject newObject = new ListObject(i);

                                    //add obj to list
                                    objects.Add(newObject);
                                }
                            }
                        }
                        else
                        {
                            //find out if the obj is part of the list
                            ListObject obj = findObj(objects, input);

                            //if obj is not null it is already in the list
                            if (obj != null)
                            {

                                //add one to the count of the obj
                                obj.count++;
                            }
                            else
                            {
                                //the object does not exist yet, create one with the input
                                ListObject newObject = new ListObject(input);

                                //add obj to list
                                objects.Add(newObject);
                            }
                        }
                        Console.WriteLine("Input a data point or command");
                        input = Console.ReadLine();

                    } while (!input.Equals("count"));

                    //input is a command
                    //input is clear command

                    //input is count command
                    if (input.Equals("count"))
                    {

                        //****start sorting code****

                        //varible that determains if sorting loop needs to run again
                        bool keepSorting = true;

                        //keeps running till entire array is sorted
                        while (keepSorting)
                        {
                            //records if any swaps were made this time around
                            bool anySwap = false;

                            //for loop entier array, starts at second item
                            for (int i = 1; i < objects.Count; i++)
                            {
                                //store current object at current index
                                ListObject curr = objects[i];

                                //store object before current object
                                ListObject prev = objects[i - 1];

                                //if current count is less than the previous object swap the two
                                if (curr.count < prev.count)
                                {

                                    //record swap was made
                                    anySwap = true;

                                    //move curr to prev location
                                    objects[i - 1] = curr;

                                    //move prev to curr location
                                    objects[i] = prev;
                                }
                            }

                            //if a swap was made this loop, keep sorting
                            keepSorting = anySwap;
                        }

                        //****end sorting code****

                        //loop through every object and print out it's name and count
                        foreach (ListObject i in objects)
                        {
                            Console.WriteLine(i.name + ": " + i.count);
                        }
                    }
                }
            }
        }

        //Code was gotten from https://www.codeproject.com/Articles/165320/Easy-Way-of-Converting-a-Decimal-to-a-Fraction
        //reused code starting here
        private static int occurence(string s, string check)
        {
            int i = 0;
            int d = s.Length;
            string ds = check;
            for (int n = (ds.Length / d); n > 0; n--)
            {
                if (ds.Contains(s))
                {
                    i++;
                    ds = ds.Remove(ds.IndexOf(s), d);
                }
            }
            return i;
        }
        static string Recur(string db)
        {
            if (db.Length < 13) return "0";
            var sb = new StringBuilder();
            for (int i = 0; i < 7; i++)
            {
                sb.Append(db[i]);
                int dlength = (db.Length / sb.ToString().Length);
                int occur = occurence(sb.ToString(), db);
                if (dlength == occur || dlength == occur - sb.ToString().Length)
                {
                    return sb.ToString();
                }
            }
            return "0";
        }
        private static void reduceNo(int i, ref double rD, ref double rN)
        {
            //keep reducing until divisibility ends
            while ((rD % i) == 0 && (rN % i) == 0)
            {
                rN = rN / i;
                rD = rD / i;
            }
        }
        static string Dec2Frac(double dbl)
        {
            char neg = ' ';
            double dblDecimal = dbl;
            if (dblDecimal < 0)
            {
                dblDecimal = Math.Abs(dblDecimal);
                neg = '-';
            }
            var whole = (int)Math.Truncate(dblDecimal);
            if (whole == dbl)
            {
                return String.Format("{0} because supplied value is not a fraction", dbl); //return no if it's not a decimal
            }
            string decpart = dblDecimal.ToString(CultureInfo.InvariantCulture).Replace(Math.Truncate(dblDecimal) + ".", "");
            double rN = Convert.ToDouble(decpart);
            double rD = Math.Pow(10, decpart.Length);

            string rd = Recur(decpart);
            int rel = Convert.ToInt32(rd);
            if (rel != 0)
            {
                rN = rel;
                rD = (int)Math.Pow(10, rd.Length) - 1;
            }
            //just a few prime factors for testing purposes
            var primes = new[] { 47, 43, 37, 31, 29, 23, 19, 17, 13, 11, 7, 5, 3, 2 };
            foreach (int i in primes) reduceNo(i, ref rD, ref rN);

            rN = rN + (whole * rD);
            return string.Format("{0}{1}/{2}", neg, rN, rD);
        }

        //ending here
        private static void sortList(ref List<double> nums)
        {
            bool keepSorting = true;
            //start sorting array
            while (keepSorting)
            {
                //records if any swaps were made this time around
                bool anySwap = false;

                //for loop entier array, starts at second item
                for (int i = 1; i < nums.Count; i++)
                {
                    //store current object at current index
                    double curr = nums[i];

                    //store object before current object
                    double prev = nums[i - 1];

                    //if current count is less than the previous object swap the two
                    if (curr < prev)
                    {

                        //record swap was made
                        anySwap = true;

                        //move curr to prev location
                        nums[i - 1] = curr;

                        //move prev to curr location
                        nums[i] = prev;
                    }
                }
                keepSorting = anySwap;
            }
        }
        private static double getNumber(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            while (!isNumber(input))
            {
                Console.Write(prompt);
                input = Console.ReadLine();
            }
            return double.Parse(input);
        }
        private static double getMedian(List<double> nums)
        {
            if (nums.Count % 2 == 0)
            {
                double lft = nums[(nums.Count / 2) - 1];
                double right = nums[(nums.Count / 2)];
                return (lft + right) / 2f;
            }
            else
            {
                int middleIndex = (int)(((double)nums.Count / 2f) + 0.5f);
                middleIndex--;
                return nums[middleIndex];
            }
        }
    }
}
