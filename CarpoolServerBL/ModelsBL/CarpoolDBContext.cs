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
        #region Login
        public User Login(string email, string pswd)
        {
            try
            {
                User user = this.Users
                .Include(us => us.Adult)
                .Include(us => us.Kid)
                .Where(u => (u.Email == email || u.UserName == email) && u.UserPswd == pswd).FirstOrDefault();
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region UpdateUser
        public User UpdateUser(User user, User updatedUser)
        {
            try
            {
                User currentUser = this.Users
                .Where(u => u.Id == user.Id).FirstOrDefault();

                currentUser.FirstName = updatedUser.FirstName;
                currentUser.LastName = updatedUser.LastName;
                currentUser.UserPswd = updatedUser.UserPswd;
                currentUser.BirthDate = updatedUser.BirthDate;
                currentUser.PhoneNum = updatedUser.PhoneNum;
                currentUser.City = updatedUser.City;
                currentUser.Street = updatedUser.Street;
                currentUser.HouseNum = updatedUser.HouseNum;

                this.SaveChanges();
                return currentUser;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion       

        #region AdultExist
        public Adult AdultExist(User user)
        {
            try
            {
                Adult adult = this.Adults
                .Where(a => a.IdNavigation == user).FirstOrDefault();
                return adult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region AdultSignUp
        public void AdultSignUp(Adult adult)
        {
            try
            {
                this.Adults.Add(adult);
                this.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region AddKid
        public void AddKid(Adult adult, Kid kid)
        {
            try
            {
                this.Kids.Add(kid);
                this.SaveChanges();

                //ילד שכבר קיים להורה
                KidsOfAdult existKidOfAdult = this.KidsOfAdults.Where(a => a.AdultId == adult.IdNavigation.Id).FirstOrDefault();
                if(existKidOfAdult != null)
                {
                    int existKidId = existKidOfAdult.KidId;
                    //אוסף כל ההורים השייכים לילד שקיים
                    IQueryable<KidsOfAdult> kidsOfAdults = this.KidsOfAdults.Where(k => k.KidId == existKidId);

                    if (kidsOfAdults != null)
                    {
                        //הוספת ההורים הקיימים לילד החדש
                        foreach (KidsOfAdult kidsOf in kidsOfAdults)
                        {
                            KidsOfAdult newKidsOfAdult = new KidsOfAdult()
                            {
                                AdultId = kidsOf.AdultId,
                                KidId = kid.Id
                            };
                            this.KidsOfAdults.Add(newKidsOfAdult);
                        }
                    }    
                }
                //אם לא קיימים עוד ילדים אז להוסיף את ההורה לילד החדש
                else
                {
                    KidsOfAdult kidsOfAdult = new KidsOfAdult()
                    {
                        AdultId = adult.IdNavigation.Id,
                        KidId = kid.Id
                    };
                    this.KidsOfAdults.Add(kidsOfAdult);
                }
                this.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region AddAdult
        public void AddAdult(Adult currentAdult, Adult adult)
        {
            try
            {
                this.Adults.Add(adult);
                this.SaveChanges();

                //אוסף כל הילדים השייכים להורה שקיים
                IQueryable<KidsOfAdult> kidsOfAdults = this.KidsOfAdults.Where(a => a.AdultId == currentAdult.IdNavigation.Id);

                if (kidsOfAdults != null)
                {
                    //הוספת הילדים הקיימים להורה החדש
                    foreach (KidsOfAdult kidsOf in kidsOfAdults)
                    {
                        KidsOfAdult newKidsOfAdult = new KidsOfAdult()
                        {
                            AdultId = adult.Id,
                            KidId = kidsOf.KidId
                        };
                        this.KidsOfAdults.Add(newKidsOfAdult);
                    }
                }
                this.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region AddActivity
        public void AddActivity(Activity activity)
        {
            try
            {
                this.Activities.Add(activity);
                this.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region JoinToActivity
        public void JoinToActivity(KidsInActivity kidsIn)
        {
            try
            {
                this.KidsInActivities.Add(kidsIn);
                this.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region GetAllKids
        public List<Kid> GetAllKids(Adult adult)
        {
            try
            {
                List<Kid> kids = new List<Kid>();
                List<int> kidsId = new List<int>();
                IQueryable<KidsOfAdult> kidsOfAdults = this.KidsOfAdults.Where(a => a.AdultId == adult.IdNavigation.Id);
                if (kidsOfAdults != null)
                {
                    foreach (KidsOfAdult kidsOf in kidsOfAdults)
                    {
                        kidsId.Add(kidsOf.KidId);
                    }
                    foreach (int kidId in kidsId)
                    {
                        User user =this.Users.Where(k => k.Id == kidId).FirstOrDefault();
                        Kid kid = this.Kids.Where(k => k.Id == kidId).FirstOrDefault();
                        kids.Add(kid);
                    }
                }                
                return kids;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region GetAllActivities
        public List<Activity> GetAllActivities(Kid kid)
        {
            try
            {
                List<Activity> activities = new List<Activity>();
                List<int> activitiesId = new List<int>();
                IQueryable<KidsInActivity> kidsInActivities = this.KidsInActivities.Where(k => k.KidId == kid.IdNavigation.Id);
                if (kidsInActivities != null)
                {
                    foreach (KidsInActivity kidsIn in kidsInActivities)
                    {
                        activitiesId.Add(kidsIn.ActivityId);
                    }
                    foreach (int activityId in activitiesId)
                    {
                        Activity activity = this.Activities.Where(a => a.Id == activityId).FirstOrDefault();
                        activities.Add(activity);
                    }
                }
                return activities;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region EmailExist
        public bool EmailExist(string email)
        {
            try
            {
                return this.Users.Any(u => u.Email == email);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
        }
        #endregion

        #region UserNameExist
        public bool UserNameExist(string userName)
        {
            try
            {
                return this.Users.Any(u => u.UserName == userName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
        }
        #endregion

        #region ActivityExist
        public bool ActivityExist(int activityId)
        {
            try
            {
                return this.Activities.Any(a => a.Id == activityId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return true;
            }
        }
        #endregion
    }
}
