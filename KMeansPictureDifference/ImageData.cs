using System;
using System.Collections.Generic;
using System.Text;

namespace KMeansPictureDifference
{
    class ImageData
    {
        Vector vector;
        string filepath;

        public ImageData(Vector vector, string filepath)
        {
            this.vector = vector;
            this.filepath = filepath;
        }

        public Vector getVector()
        {
            return vector;
        }

        public string getFilePath()
        {
            return filepath;
        }
    }
}
