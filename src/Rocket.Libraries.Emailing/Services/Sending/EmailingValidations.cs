namespace Rocket.Libraries.Emailing.Services.Sending
{
    public static class EmailingValidations
    {
        public static bool IsInvalidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                var isValid = addr.Address == email;
                return isValid == false;
            }
            catch
            {
                return true;
            }
        }
    }
}