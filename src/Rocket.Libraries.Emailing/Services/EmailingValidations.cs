﻿namespace Rocket.Libraries.Emailing.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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