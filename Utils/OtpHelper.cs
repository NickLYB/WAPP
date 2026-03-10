using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace WAPP.Utils
{
    public static class OtpHelper
    {
        // 1. Generates a random 6-digit string (e.g., "492817")
        public static string GenerateNumericOtp()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        // 2. Hashes the OTP using HMACSHA256 and a secret key
        public static string HmacOtp(string otp, string secret)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
                return Convert.ToBase64String(hashBytes);
            }
        }

        // 3. Saves the Hashed OTP to the database using EMAIL instead of UserId
        public static void SaveOtpToDb(string email, string otpHash, DateTime expiryDate)
        {
            string connStr = ConfigurationManager.ConnectionStrings["MyDbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // We use DELETE first to remove any old OTPs for this email address
                string deleteOld = "DELETE FROM Emailotp WHERE Email = @email";
                string insertNew = "INSERT INTO Emailotp (Email, OtpHash, ExpiryDate) VALUES (@email, @hash, @expiry)";

                conn.Open();

                using (SqlCommand cmdDel = new SqlCommand(deleteOld, conn))
                {
                    cmdDel.Parameters.AddWithValue("@email", email);
                    cmdDel.ExecuteNonQuery();
                }

                using (SqlCommand cmdIns = new SqlCommand(insertNew, conn))
                {
                    cmdIns.Parameters.AddWithValue("@email", email);
                    cmdIns.Parameters.AddWithValue("@hash", otpHash);
                    cmdIns.Parameters.AddWithValue("@expiry", expiryDate);
                    cmdIns.ExecuteNonQuery();
                }
            }
        }
    }
}