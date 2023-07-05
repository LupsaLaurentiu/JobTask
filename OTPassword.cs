using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

public class OneTimePasswordGenerator
{
    private const int PasswordValidityDurationSeconds = 30;
    private const int RenewalIntervalMilliseconds = 1000;

    private string currentPassword;
    private DateTime currentPasswordExpiration;

    public void StartPasswordGenerator(string userId)
    {
        GenerateNewPassword(userId);

        UpdatePasswordAndRemainingSeconds(currentPassword, PasswordValidityDurationSeconds);

        // Start a new thread to automatically renew the password after the validity duration
        StartPasswordRenewalThread(userId);
    }

    private void GenerateNewPassword(string userId)
    {
        // Combine the User ID and current DateTime values into a single string
        string data = userId + DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        // Convert the string to bytes
        byte[] bytes = Encoding.UTF8.GetBytes(data);

        // Generate a cryptographic hash using SHA256
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(bytes);

            // Take the first 4 bytes of the hash
            byte[] truncatedHash = new byte[4];
            Array.Copy(hashBytes, truncatedHash, 4);

            // Convert the truncated hash to an integer value
            int passwordValue = BitConverter.ToInt32(truncatedHash, 0);

            // Take the absolute value of the password value
            int password = Math.Abs(passwordValue);

            // Pad the password with leading zeros if necessary
            currentPassword = password.ToString("D6");

            // Set the expiration time for the current password
            currentPasswordExpiration = DateTime.UtcNow.AddSeconds(PasswordValidityDurationSeconds);
        }
    }
    private double GetRemainingSeconds()
    {
        TimeSpan remainingTime = currentPasswordExpiration - DateTime.UtcNow;
        return Math.Max(0, remainingTime.TotalSeconds);
    }

    private void UpdatePasswordAndRemainingSeconds(string password, double seconds)
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write("Password: " + password.PadRight(6) + " | Remaining Time: " + seconds.ToString("0").PadLeft(2) + " seconds");
    }

    private void StartPasswordRenewalThread(string userId)
    {
        Thread renewalThread = new Thread(() =>
        {
            while (true)
            {
                double remainingSeconds = GetRemainingSeconds();

                if (remainingSeconds <= 0)
                {
                    GenerateNewPassword(userId);
                    remainingSeconds = PasswordValidityDurationSeconds;
                }

                UpdatePasswordAndRemainingSeconds(currentPassword, remainingSeconds);

                Thread.Sleep(RenewalIntervalMilliseconds);
            }
        });

        renewalThread.IsBackground = true;
        renewalThread.Start();
    }
}
