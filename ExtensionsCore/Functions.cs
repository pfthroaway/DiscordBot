using System.IO;

namespace ExtensionsCore
{
    public static class Functions
    {
        /// <summary>Verifies that the requested file exists and that its file size is greater than zero. If not, it extracts the embedded file to the local output folder.</summary>
        /// <param name="resourceStream">Resource Stream from Assembly.GetExecutingAssembly().GetManifestResourceStream()</param>
        /// <param name="resourceName">Resource name</param>
        public static void VerifyFileIntegrity(Stream resourceStream, string resourceName)
        {
            FileInfo fileInfo = new FileInfo(resourceName);
            if (!File.Exists(resourceName) || fileInfo.Length == 0)
                ExtractEmbeddedResource(resourceStream, resourceName);
        }

        /// <summary>Extracts an embedded resource from a Stream.</summary>
        /// <param name="resourceStream">Resource Stream from Assembly.GetExecutingAssembly().GetManifestResourceStream()</param>
        /// <param name="resourceName">Resource name</param>
        public static void ExtractEmbeddedResource(Stream resourceStream, string resourceName)
        {
            if (resourceStream != null)
            {
                using (BinaryReader r = new BinaryReader(resourceStream))
                {
                    using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + resourceName, FileMode.OpenOrCreate))
                    {
                        using (BinaryWriter w = new BinaryWriter(fs))
                        {
                            w.Write(r.ReadBytes((int)resourceStream.Length));
                        }
                    }
                }
            }
        }

        #region Random Number Generation

        /// <summary>Generates a random number between min and max (inclusive).</summary>
        /// <param name="min">Inclusive minimum number</param>
        /// <param name="max">Inclusive maximum number</param>
        /// <param name="lowerLimit">Minimum limit for the method, regardless of min and max.</param>
        /// <param name="upperLimit">Maximum limit for the method, regardless of min and max.</param>
        /// <returns>Returns randomly generated integer between min and max with an upper limit of upperLimit.</returns>
        public static int GenerateRandomNumber(int min, int max, int lowerLimit = int.MinValue,
            int upperLimit = int.MaxValue)
        {
            if (min < lowerLimit)
                min = lowerLimit;
            if (max > upperLimit)
                max = upperLimit;
            int result = min < max
                ? ThreadSafeRandom.ThisThreadsRandom.Next(min, max + 1)
                : ThreadSafeRandom.ThisThreadsRandom.Next(max, min + 1);

            return result;
        }

        #endregion Random Number Generation
    }
}