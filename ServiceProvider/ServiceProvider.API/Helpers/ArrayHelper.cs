using System;

namespace ServiceProvider.API.Helpers
{
    /// <summary>
    /// The Array helper.
    /// </summary>
    public static class ArrayHelper
    {
        /// <summary>
        /// Trims all leading zero bytes from array.
        /// </summary>
        /// <param name="array">The byte array</param>
        /// <returns>The array with trimmed leading zero bytes.</returns>
        public static byte[] TrimStart(byte[] array)
        {
            var firstIndex = Array.FindIndex(array, b => b != 0);
            var newSize = array.Length - firstIndex;
            var trimmedArr = new byte[newSize];
            Array.Copy(array, firstIndex, trimmedArr, 0, newSize);

            return trimmedArr;
        }
    }
}
