using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    public class AccountOperations
    {
        public ResponseStatus LoginFieldsValid(LoginAccount account)
        {
            if (string.IsNullOrEmpty(account.Username) || string.IsNullOrEmpty(account.Password))
            {
                return new ErrorResponseStatus() { Message = "All inputs fields must have data!" };
            }
            else
            {
                return new SuccessResponseStatus() { Message = "Succesfully logged in!" };
            }
        }
        public ResponseStatus RegistrationFieldsValid(RegistrationAccount account)
        {
            // Checking if any are empty
            if (string.IsNullOrEmpty(account.Username) || string.IsNullOrEmpty(account.Password) || string.IsNullOrEmpty(account.ConfirmPassword) || string.IsNullOrEmpty(account.Email))
            {
                return new ErrorResponseStatus() { Message = "All inputs fields must have data!" };
            }
            else if (!(account.Password.Length > 5))
            {
                return new ErrorResponseStatus() { Message = "Password must be greater than 5!" };
            }
            else if (account.Password != account.ConfirmPassword)
            {
                return new ErrorResponseStatus() { Message = "Password must match Confirm password!" };
            } 
            else if (!Regex.Match(account.Email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success)
            {
                return new ErrorResponseStatus() { Message = "Email is incorrect!" };
            }
            else
            {
                return new SuccessResponseStatus() { Message = "Succesfully registered!" };
            }
        }
    }
}
