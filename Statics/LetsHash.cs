using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoachOnline.Statics
{
    public static class LetsHash
    {

        // Generates a random string with a given size.    
        public static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):   
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)rn.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }


        private static Random rn = new Random();
        private static string alphabed = "abcdefgijklmnopqrstuABCDEFGIJKLMNOPQRST";
        public static string ToSHA512(string plain)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(plain);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash).ToLower();
        }
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public static string RandomHash(string seed = "")
        {
            string preGeneratedHash = "";
            string generatedHash = "";
            var Date = DateTime.Now.AddDays(rn.Next(0, 1000));
            Date = Date.AddHours(rn.Next(0, 10000));
            Date = Date.AddMinutes(rn.Next(0, 10000));
            seed = $"{seed}{seed[rn.Next(0, seed.Length)]}{rn.Next(0, int.MaxValue)}";

            string generatedLettersHash = "";

            for (int x = 0; x <= rn.Next(50, 200); x++)
            {
                generatedLettersHash += $"{alphabed[rn.Next(0, alphabed.Length)]}";
            }

            preGeneratedHash = $"{generatedLettersHash}{rn.Next(0, int.MaxValue)}{generatedLettersHash}{Date}{generatedLettersHash}{rn.Next(0, int.MaxValue)}{seed}{rn.Next(0, int.MaxValue)}{rn.Next(0, int.MaxValue)}{rn.Next(0, int.MaxValue)}{rn.Next(0, int.MaxValue)}";

            generatedHash = ToSHA512(preGeneratedHash);

            return generatedHash;

        }



    }
}
