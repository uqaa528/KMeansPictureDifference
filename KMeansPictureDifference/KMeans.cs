using nQuant;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System;

namespace KMeansPictureDifference
{
    class KMeans
    {
        public static Color[] getTopKColorsArray(string directoryPath, int topColors)
        {
            // Setting up directory
            string[] array = Directory.GetFiles(directoryPath);

            var quantizer = new WuQuantizer();

            // Map used for storing total amount of occurences of colours in provided image set
            Dictionary<Color, int> totalColors = new Dictionary<Color, int>();

            foreach (string s in array)
            {
                // Processing input image
                // Resizing the image and quantifing it using nQuant library

                Bitmap bmp = new Bitmap(quantizer.QuantizeImage(
                    ImageProcessing.ResizeImage(new Bitmap(s), 640, 400))
                    );

                unsafe
                {
                    // Found on the Internet that it is the most efficient way of processing image pixel by pixel

                    // Lock bits
                    BitmapData bitmapData = bmp.LockBits(
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        ImageLockMode.ReadOnly,
                        bmp.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                    int bmpHeightInPixels = bitmapData.Height;
                    int bmpWidthInBytes = bitmapData.Width * bytesPerPixel;

                    byte* PtrToFirstPixel = (byte*)bitmapData.Scan0;

                    // Map used for storing total amount of occurenes of given colour in single image
                    Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

                    for (int y = 0; y < bmpHeightInPixels; y++)
                    {
                        byte* currentLine = PtrToFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < bmpWidthInBytes; x = x + bytesPerPixel)
                        {
                            int blue = currentLine[x];
                            int green = currentLine[x + 1];
                            int red = currentLine[x + 2];

                            Color newColor = Color.FromArgb(red, green, blue);

                            if (totalColors.ContainsKey(newColor))
                                totalColors[newColor] += 1;
                            else
                                totalColors.Add(newColor, 1);
                        }
                    }
                }
            }

            // Creating vector that will help us classify images to given classes
            Color[] mostPopularColors = new Color[topColors];

            var myList = totalColors.ToList();

            myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            for (int i = 0; i < topColors; i++)
            {
                mostPopularColors[i] = myList[i].Key;
                Console.WriteLine(myList[i].Key + " " + myList[i].Value);
            }

            return mostPopularColors;
        }

        public static List<ImageData> extractColorVectorsFromDirectory(Color[] inputVector, string directoryPath)
        {
            List<ImageData> imageDatas = new List<ImageData>();

            // Setting up directory
            string[] array = Directory.GetFiles(directoryPath);

            var quantizer = new WuQuantizer();

            foreach (string s in array)
            {
                // Processing input image
                // Resizing the image and quantifing it using nQuant library

                Bitmap bmp = new Bitmap(quantizer.QuantizeImage(
                    ImageProcessing.ResizeImage(new Bitmap(s), 640, 400))
                    );

                // Creating vector that will contain image color occurence counts
                int[] resultVector = new int[inputVector.Length];

                unsafe
                {
                    // Found on the Internet that it is the most efficient way of processing image pixel by pixel

                    // Lock bits
                    BitmapData bitmapData = bmp.LockBits(
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        ImageLockMode.ReadOnly,
                        bmp.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                    int bmpHeightInPixels = bitmapData.Height;
                    int bmpWidthInBytes = bitmapData.Width * bytesPerPixel;

                    byte* PtrToFirstPixel = (byte*)bitmapData.Scan0;

                    // Map used for storing total amount of occurenes of given colour in single image
                    Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

                    for (int y = 0; y < bmpHeightInPixels; y++)
                    {
                        byte* currentLine = PtrToFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < bmpWidthInBytes; x = x + bytesPerPixel)
                        {
                            int blue = currentLine[x];
                            int green = currentLine[x + 1];
                            int red = currentLine[x + 2];

                            Color newColor = Color.FromArgb(red, green, blue);

                            if (colorCount.ContainsKey(newColor))
                                colorCount[newColor] += 1;
                            else
                                colorCount.Add(newColor, 1);
                        }
                    }

                    // checking if inputVector contains colors from processed image. If it does we set vector value on coresponding index to given amount of occurences;
                    foreach (KeyValuePair<Color, int> entry in colorCount)
                    {
                        if (inputVector.Contains(entry.Key))
                        {
                            int index = Array.IndexOf(inputVector, entry.Key);
                            resultVector[index] = entry.Value;
                        }
                    }
                    ImageData imageData = new ImageData(new Vector(resultVector), s);
                    imageDatas.Add(new ImageData(new Vector(resultVector), s));
                }
            }

            return imageDatas;
        }

        public static void clustering(List<ImageData> imageDatas, string directoryPath, int k)
        {
            /* shuffling data */
            imageDatas = imageDatas.OrderBy(a => Guid.NewGuid()).ToList();
            
            /* creating directory for clusters */
            for (int i = 0; i < k; i++)
            {
                string path = directoryPath + "/" + i;
                try
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }
            }
            /* creating directory for clusters */

            /* map storing centroid vectors */
            Dictionary<int, int[]> centroidsMap = new Dictionary<int, int[]>();

            /* map storing vectors with their corresponding classes */
            Dictionary<ImageData, int> classificationMap = new Dictionary<ImageData, int>();

            /* map storing count of vectors of given type*/
            Dictionary<int, int> count = new Dictionary<int, int>();

            /* initializing maps */
            for (int i = 0; i < k; i++)
            {
                centroidsMap.Add(i, new int[imageDatas[0].getVector().getValues().Length]);

                count.Add(i, 0);
            }


            /* initial classification */

                /* adding vectors from input file to map values (first iteration is random) */
                for (int i = 0; i < imageDatas.Count(); i++)
                {

                    int[] tmpVector = centroidsMap[i % k];

                    for (int j = 0; j < tmpVector.Length; j++)
                    {
                        tmpVector[j] += imageDatas[i].getVector().getValues()[j];
                    }

                    centroidsMap[i % k] =  tmpVector;
                    classificationMap[imageDatas[i]] =  i % k;
                    count[i % k] =  count[i % k] + 1;
                }

                /* dividing centroid vector values by number of elements */
                divideVectorValues(centroidsMap, count);

            /* initial classification */


            /* k-means algorithm */

            bool valuesChanged = true;

            while (valuesChanged)
            {
                valuesChanged = false;

                foreach (ImageData imageData in imageDatas)
                {
                    double lowestDistance = -1;
                    int centroidIdentifier = -1;

                    /* calculating distance from every centroid */
                    for (int j = 0; j < centroidsMap.Count(); j++)
                    {
                        double newDistance = Vector.vectorDistance(imageData.getVector(), new Vector(centroidsMap[j]));

                        if (lowestDistance == -1)
                        {
                            lowestDistance = newDistance;
                            centroidIdentifier = j;
                        }
                        else if (newDistance < lowestDistance)
                        {
                            lowestDistance = newDistance;
                            centroidIdentifier = j;
                        }
                    }

                    if (centroidIdentifier != classificationMap[imageData])
                    {
                        classificationMap[imageData] = centroidIdentifier;
                        valuesChanged = true;
                    }
                }

                if (valuesChanged)
                {
                    /* resetting map values */
                    for (int i = 0; i < k; i++)
                    {
                        centroidsMap[i] = new int[imageDatas[0].getVector().getValues().Length];

                        count[i] = 0;
                    }

                    for (int i = 0; i < imageDatas.Count(); i++)
                    {
                        int identifier = classificationMap[imageDatas[i]];

                        for (int j = 0; j < centroidsMap[identifier].Length; j++)
                        {
                            centroidsMap[identifier][j] += imageDatas[i].getVector().getValues()[j];
                        }
                        count[identifier] = count[identifier] + 1;
                    }

                    divideVectorValues(centroidsMap, count);
                }
            }
            int o = 0;
            foreach (KeyValuePair<int, int[]> entry in centroidsMap)
            {
                Console.WriteLine(entry.Key + " " + string.Join(",", entry.Value) + "\nVectors assigned:");
                foreach (KeyValuePair<ImageData, int> entry2 in classificationMap)
                {
                    if (entry2.Value.Equals(entry.Key))
                    {
                        Console.WriteLine(string.Join(",", entry2.Key.getVector().getValues()) + " " + entry2.Key.getFilePath());
                        try
                        {
                            File.Move(entry2.Key.getFilePath(), directoryPath + "/" + entry.Key + "/" + entry.Key + "(" + o++ + ").png");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        private static void divideVectorValues(Dictionary<int, int[]> centroidsMap, Dictionary<int, int> count)
        {
            for (int i = 0; i < centroidsMap.Count(); i++)
            {
                int[] tmpVector = centroidsMap[i];

                for (int j = 0; j < tmpVector.Length; j++)
                {
                    if (count[i] == 0)
                    {
                        Console.WriteLine("Error in classification.");
                        Environment.Exit(-1);
                    }
                    else
                        tmpVector[j] = tmpVector[j] / count[i];
                }
            }
        }
    }
}
