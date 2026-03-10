using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BCrypt.Net;

namespace WAPP.Utils
{
    public class PasswordManager
    {
        // 1. Use this when the user REGISTERS
        public string HashPassword(string plainTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }

        // 2. Use this when the user LOGS IN
        public bool VerifyPassword(string enteredPassword, string storedHashFromDatabase)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHashFromDatabase);
            }
            catch (Exception)
            {
                // Failsafe in case the stored hash is empty or corrupted
                return false;
            }
        }
    }
}