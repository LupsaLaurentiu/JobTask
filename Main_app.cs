using System;

namespace PasswordGenerator
{
    class Main_app
    {
        static void Main(string[] args)
        {
            OneTimePasswordGenerator passwordGenerator = new OneTimePasswordGenerator();

            string userId = "your_user_id";
            DateTime dateTime = DateTime.Now;

            passwordGenerator.StartPasswordGenerator(userId);

            Console.ReadLine();
        }
    }
}
