using System;
using System.Collections.Generic;
using System.Drawing;

namespace KMeansPictureDifference
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "C:/Users/your_user/Desktop/";
            Color [] colorsVector = KMeans.getTopKColorsArray(path, 1);

            // colors vector determines which colors will be considered by the algorithm during the classification
            List <ImageData> imageDatas = KMeans.extractColorVectorsFromDirectory(colorsVector, path);

            foreach (ImageData imageData in imageDatas)
                Console.WriteLine(imageData.getFilePath() + " " + string.Join(",", imageData.getVector().getValues()));

            KMeans.clustering(imageDatas, path, 2);
        }
    }
}
