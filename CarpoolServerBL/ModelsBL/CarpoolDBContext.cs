﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CarpoolServerBL.Models
{
    public partial class CarpoolDBContext : DbContext
    {
        public User Login(string email, string userName, string pswd)
        {
            User user = this.Users
                //.Include(us => us.UserContacts)
                //.ThenInclude(uc => uc.ContactPhones)
                .Where(u => (u.Email == email || u.UserName == userName) && u.UserPswd == pswd).FirstOrDefault();
            return user;
        }

        //public void AddUser(User user)
        //{
        //    this.Users.Add(user);
        //    this.SaveChanges();
        //}
        public void AddAdult(Adult adult)
        {
            this.Adults.Add(adult);
            this.SaveChanges();
        }
        public void AddKid(Kid kid)
        {
            this.Kids.Add(kid);
            this.SaveChanges();
        }

        //public User SignUp(string email, string pswd, string fName, string lName)
        //{
        //    User user = new User()
        //    {
        //        Email=email,
        //        UserPswd=pswd,
        //        FirstName = fName,
        //        LastName = lName
        //    };
        //    return user;
        //}
    }
}
