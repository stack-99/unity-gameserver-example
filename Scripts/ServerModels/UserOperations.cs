﻿using Assets.Scripts.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Shared
{
    // remember we ened to make a db code
    public class UserOperations
    {
        private Dictionary<string, User> Users = new Dictionary<string, User>();
        public UserOperations(Dictionary<string, User> users)
        {
            Users = users;
        }
        public User GetUser(string username)
        {
            return Users[username];
        }

        public bool CredentialsCorrect(string usernameOrEmail, string password)
        {
            if(DoesUserExist(usernameOrEmail))
            {
                var user = GetUser(usernameOrEmail);

                if(user.Password == password)
                {
                    return true;
                }

                return false;
            }

            throw new Exception("User does not exist!");
        }

        
        public bool DoesEmailExist(string email)
        {
            return false;
        }

        public bool DoesUserExist(string username)
        {
            if (Users.ContainsKey(username))
            {
                return true;
            }

            return false;
        }
    }
}
