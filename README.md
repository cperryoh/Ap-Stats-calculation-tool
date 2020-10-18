# Ap-Stats-calculation-tool
## A tool that allows users to do calculations concerning statistics.
![Hi] (/images/calc.PNG)
* Count frequency of objects in a data set [f]
  * Count- counts all the occurrences of each data point (once ran, returns back to the beginning of the program)
  * number- adds a number to the array that will be counted
* Count changes in a data set. Log the smallest and largest changes between data points[c]
  * {number}: add to the data set of numbers
  * calc: calculate the changes in the data(once ran, returns back to the beginning of the program)
  * remove: remove the last item added
* z-Do calculations concerning z scores
  * z score: convert z score to percentile(z score is converted to percentile to be used on the [sub] function)
  * calc: convert raw data points to a z score(z score is converted to percentile to be used on the [sub] function)
  * p: convert percentile to a z score
  * sub: subtract the last two calculated z score percentiles will be subtracted to get a range
  * quit: return to the starting point of the program
* n- Get's statistics about the data set
  * allows for the entry of data to be in one of the two formats
    * 1,2,3,4,4321,1
     * 1
     * 2
     * 4
     * 2
     * 3
  * calc- finds the following stats  about the data set
    * range- the difference between the lowest and highest values
    * mean-the average of the data set
      * Calculated by adding all the data points together and dividing the sum by the number of data points.
    * Median- the middle of the data set
    * Standard deviation- the difference of all the data points from the mean added together and divided by the total amount of points
    * LQ or q1- calculated by finding the median of the lower half of the data set
    * UQ or q3- calculated by finding the median of the upper half of the data set.
    * IQR- q3-q1
    * lower outlier range- calculated by the following equation
      * LQ - (IQR * 1.5f)
      * any numbers bellow the lower outlier range are considered outliers
    * upper outlier range- calculated by the following equation
      * UQ - (IQR * 1.5f)
      * any numbers above the upper outlier range are considered outliers
    * Outlier count- The count of numbers either bellow the lower outlier range or above the upper outlier range 
