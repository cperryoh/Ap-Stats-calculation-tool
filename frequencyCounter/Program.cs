﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        static List<float> parseIntCsv(string csv)
        {
            //remove any spaces
            csv = csv.Replace("  ", " ");
            csv = csv.Replace(", ",",");
            csv=csv.Replace(" ,",",");

            //list of strings data will be parsed into
            List<float> nums = new List<float>();

            //loop while there are still commas in the string and its not the last char
            while (csv.IndexOf(",") != -1 && csv.IndexOf(",") != csv.Length - 1)
            {

                //parse out possible float
                string curNum = csv.Substring(0, csv.IndexOf(","));

                //figure out if curNum is actually an int, if not eliminate if
                try
                {
                    float num = float.Parse(curNum);
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
                float num = float.Parse(csv);
                nums.Add(num);
            }
            catch (FormatException e)
            {
                Console.WriteLine(csv + " is invalid, eliminating from the data set");
            }

            //return list
            return nums;
        }
        static double getZScorePercent(double z)
        {
            return Math.Round(result.CumulativeDistribution(z),4);
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
        static void Main(string[] args)
        {
            //list of objects to be counted
            List<ListObject> objects = new List<ListObject>();
            while (true)
            {
                //get mode
                Console.Write("Input a comand or a new element[c-changes in data][z- convert a z-score to a probality] [f-frequency of a certain type of data][n-get mean, median, and mode from numeric data]: ");
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

                //option to get data on numeric set
                else if (calcType.Equals("n"))
                {

                    //list of inputs 
                    List<float> nums = new List<float>();

                    //prompt user for intial input
                    Console.Write("Plese intput your data either in a csv format or one by one:");
                    string input = Console.ReadLine();
                    do
                    {

                        //determain if input is singluar numbe or csv
                        if (input.Contains(","))
                        {

                            //add csv list to list
                            nums.AddRange(parseIntCsv(input));


                        }
                        else if (isNumber(input))
                        {

                            //add singular number to list
                            nums.Add(float.Parse(input));
                        }else if (input.Equals("print")){
                            sortList(ref nums);
                            for(int i =0; i < nums.Count - 1; i++)
                            {
                                Console.Write(nums[i] + ", ");
                            }
                            Console.Write(nums[nums.Count - 1]+"\n\n");
                        }


                        //get new input
                        Console.Write("Plese intput your data either in a csv format or one by one: ");
                        input = Console.ReadLine();

                    } while (!input.Equals("calc"));
                    string copy = "";
                    sortList(ref nums);
                    for (int i = 0; i < nums.Count - 1; i++)
                    {
                        copy+=nums[i] + ",";
                    }
                    copy+=nums[nums.Count - 1];
                    OpenClipboard(IntPtr.Zero);
                    SetClipboardData(13, Marshal.StringToHGlobalUni(copy));

                    CloseClipboard();
                    //start sorting
                    sortList(ref nums);

                    
                    float mean, median,iqr,lq,uq;


                    //get mean
                    float sum = 0;
                    foreach (int i in nums)
                    {
                        sum += i;
                    }

                    //get data on mean
                    mean = sum / (float)nums.Count;
                    float difTotal = 0;
                    float range = nums[nums.Count - 1] - nums[0];
                    float standerdDeviation = 0;
                    foreach(float i in nums)
                    {
                        difTotal += (float)Math.Pow(i-mean,2);
                    }
                    standerdDeviation = (float) Math.Sqrt(difTotal / ((float)nums.Count-1));

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
                    float outLierLowerRange = lq - (iqr * 1.5f);
                    int outliers = 0;
                    float outLierUpperRange = uq + (iqr * 1.5f);
                    foreach(int i in nums)
                    {
                        if(i< outLierLowerRange ||i> outLierUpperRange)
                        {
                            outliers++;
                        }
                    }
                    //print out data
                    Console.WriteLine(String.Format("Range: {3} Mean: {0} Median: {1} Standerd Deviation: {2}", mean, median,standerdDeviation,range));
                    Console.WriteLine("Lq: {0} Uq: {1} IQR: {2} Lower outlier range: {3} Upper outlier range {4} Outlier count: {5}", lq, uq,iqr, outLierLowerRange, outLierUpperRange,outliers);
                }
                else if (calcType.Equals("z"))
                {
                    //get input
                    Console.Write("\n[z-score][calc- get a zscore][prv- convert percentile to a raw value][p- convert a z-score to a percentile][quit][sub]: ");
                    String input = Console.ReadLine();

                    //zscore=last score->percentile calculated zscore_= last last score->percentile calculated
                    double zscore =0, zscore_=0;

                    //set up class to calculate zscore
                    


                    //loop till user asks to quit
                    while (!input.Equals("quit"))
                    {

                        //convert zscore to percentile
                        if (isNumber(input))
                        {

                            //move calculated zscores over
                            zscore_ = zscore;
                            zscore = getZScorePercent(double.Parse(input));

                            //print result
                            Console.WriteLine("The percentile for a z-score of {0} is {1}", double.Parse(input), zscore);

                        }
                        else if (input.ToLower().Equals("calc"))
                        {
                            double mean = getNumber("Enter the mean: ");
                            double sd = getNumber("Enter the standerd deviation: ");
                            double dataPoint = getNumber("Enter the point that you would like the z-score of: ");
                            double z = Math.Round((dataPoint - mean) / sd, 2);
                            Console.WriteLine($"The z-score for {dataPoint} is {z}.");
                            zscore_ = zscore;
                            zscore = getZScorePercent(z);

                        }

                        //sub track the last two calculated z scores
                        else if (input.ToLower().Equals("sub"))
                        {

                            //decide which way to subtract
                            if (zscore > zscore_)
                            {
                                Console.WriteLine("The differnece of the two previously calculated z-scores is {0}-{1}={2}", Math.Round(zscore, 4), Math.Round(zscore_, 4), Math.Round(zscore - zscore_, 4));
                            }
                            else
                            {
                                Console.WriteLine("The differnece of the two previously calculated z-scores is {0}-{1}={2}", Math.Round(zscore_, 4), Math.Round(zscore, 4), Math.Round(zscore_ - zscore, 4));
                            }
                        }
                        else if (input.ToLower().Equals("prv"))
                        {
                            //get percentile from user
                            double percent = getNumber("Enter percentile: ");

                            //get mean from user
                            double mean = getNumber("Enter mean: ");

                            //get standerd deviation from user
                            double deviation = getNumber("Enter standerd deviation: ");

                            //convert percentile to a z score
                            double z = Math.Round(percentToZ(percent/100),2);

                            //convert z to raw value by solving for x in the following equation
                            //zscore=(x-μ(mean))/σ(standerd deviation)
                            double raw = Math.Round((z * deviation) + mean,2);

                            //print result
                            Console.WriteLine($"The raw value of a zscore {z} is {raw}");
                        }
                        //convert percentile to zscore(withen 1 100th)
                        else if (input.ToLower().Equals("p"))
                        {

                            //get percentile from user
                            double percentile =getNumber("Enter a percentile: ")/100;

                            //loop through zscores -6 to 6 incrementing by 0.01
                            double z = percentToZ(percentile);

                            //print results
                            Console.WriteLine("The closest z score to {0}% is {1}", percentile*100, Math.Round(z,2));
                        }

                        //tell user that the given input was invalid
                        else
                        {
                            Console.WriteLine("That is not a valid command or zscore");
                        }

                        //get new input
                        Console.Write("\n[z-score][calc- get a zscore][prv- convert percentile to a raw value[p- convert a z-score to a percentile][quit][sub]: ");
                        input = Console.ReadLine();
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
                            foreach(string i in csv)
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
        private static void sortList(ref List<float> nums)
        {
            bool keepSorting=true;
            //start sorting array
            while (keepSorting)
            {
                //records if any swaps were made this time around
                bool anySwap = false;

                //for loop entier array, starts at second item
                for (int i = 1; i < nums.Count; i++)
                {
                    //store current object at current index
                    float curr = nums[i];

                    //store object before current object
                    float prev = nums[i - 1];

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
        private static float getMedian(List<float> nums)
        {
            if (nums.Count % 2 == 0)
            {
                float lft = nums[(nums.Count / 2) - 1];
                float right = nums[(nums.Count / 2)];
                return (lft + right) / 2f;
            }
            else
            {
                int middleIndex = (int)(((float)nums.Count / 2f) + 0.5f);
                middleIndex--;
                return nums[middleIndex];
            }
        }
    }
}
