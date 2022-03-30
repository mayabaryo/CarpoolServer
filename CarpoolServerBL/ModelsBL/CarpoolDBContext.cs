using System;
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
                .Include(us => us.Adult).Include(us => us.Adult.Activities).Include(us => us.Adult.Carpools).Include(us => us.Adult.KidsOfAdults)
                .Include(us => us.Kid).Include(us => us.Kid.KidsInActivities).Include(us => us.Kid.KidsInCarpools).Include(us => us.Kid.KidsOfAdults)
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

        #region AddCarpool
        public void AddCarpool(Carpool carpool)
        {
            try
            {
                this.Carpools.Add(carpool);
                this.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region JoinToCarpool
        public void JoinToCarpool(KidsInCarpool kidsIn)
        {
            try
            {
                this.KidsInCarpools.Add(kidsIn);
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
                        User user = this.Users.Where(k => k.Id == kidId).FirstOrDefault();
                        Kid kid = this.Kids
                            .Include(k => k.KidsInActivities)
                            .Include(k => k.KidsInCarpools)
                            .Include(k => k.KidsOfAdults)
                            .Where(k => k.Id == kidId).FirstOrDefault();
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

        #region GetKidActivities
        public List<Activity> GetKidActivities(Kid kid)
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
                        Activity activity = this.Activities
                            .Include(a => a.Adult)
                            .Include(a => a.Adult.IdNavigation)
                            .Include(a => a.Carpools)
                            .Include(a => a.KidsInActivities)
                            .Where(a => a.Id == activityId).FirstOrDefault();
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

        #region GetKidCarpools
        public List<Carpool> GetKidCarpools(Kid kid)
        {
            try
            {
                List<Carpool> carpools = new List<Carpool>();
                List<int> carpoolsId = new List<int>();
                IQueryable<KidsInCarpool> kidsInCarpools = this.KidsInCarpools.Where(k => k.KidId == kid.IdNavigation.Id);
                if (kidsInCarpools != null)
                {
                    foreach (KidsInCarpool kidsIn in kidsInCarpools)
                    {
                        carpoolsId.Add(kidsIn.CarpoolId);
                    }
                    foreach (int carpoolId in carpoolsId)
                    {
                        Carpool carpool = this.Carpools
                            .Include(a => a.Adult)
                            .Include(a => a.Adult.IdNavigation)
                            .Include(a => a.Activity)
                            .Include(a => a.KidsInCarpools)
                            .Include(a => a.CarpoolStatus)
                            .Where(a => a.Id == carpoolId).FirstOrDefault();

                        carpools.Add(carpool);
                    }
                }
                return carpools;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region GetAdultCarpools
        public List<Carpool> GetAdultCarpools(Adult adult)
        {
            try
            {
                IQueryable<Carpool> carpools = this.Carpools
                    .Include(a => a.Adult)
                    .Include(a => a.Adult.IdNavigation)
                    .Include(a => a.Activity)
                    .Include(a => a.KidsInCarpools)
                    .Include(a => a.CarpoolStatus)
                    .Where(a => a.AdultId == adult.IdNavigation.Id);

                List<Carpool> carpoolList = new List<Carpool>();

                foreach (Carpool c in carpools)
                {                   
                    carpoolList.Add(c);
                }
                
                return carpoolList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region GetCarpoolsInActivity
        public List<Carpool> GetCarpoolsInActivity(Activity activity)
        {
            try
            {
                List<Carpool> carpoolList = new List<Carpool>();

                IQueryable<Carpool> carpools = this.Carpools
                    .Include(a => a.Adult)
                    .Include(a => a.Adult.IdNavigation)
                    .Include(a => a.Activity)
                    .Include(a => a.KidsInCarpools)
                    .Include(a => a.CarpoolStatus)
                    .Where(a => a.ActivityId == activity.Id);
               
                if (carpools != null)
                {
                    foreach (Carpool c in carpools)
                    {
                        carpoolList.Add(c);
                    }                   
                }
                return carpoolList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        #endregion

        #region GetKidsInCarpool
        public List<Kid> GetKidsInCarpool(Carpool carpool)
        {
            try
            {
                List<Kid> kids = new List<Kid>();
                List<int> kidsId = new List<int>();
                IQueryable<KidsInCarpool> kidsInCarpool = this.KidsInCarpools.Where(a => a.CarpoolId == carpool.Id);
                if (kidsInCarpool != null)
                {
                    foreach (KidsInCarpool kidsIn in kidsInCarpool)
                    {
                        kidsId.Add(kidsIn.KidId);
                    }
                    foreach (int kidId in kidsId)
                    {
                        Kid kid = this.Kids
                            .Include(k => k.IdNavigation)
                            .Include(k => k.KidsInActivities)
                            .Include(k => k.KidsInCarpools)
                            .Include(k => k.KidsOfAdults)
                            .Where(k => k.Id == kidId).FirstOrDefault();
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

        #region AddRequestToJoinCarpool
        public bool AddRequestToJoinCarpool(RequestToJoinCarpool request)
        {
            try
            {
                request.RequestStatus = this.RequestCarpoolStatuses.Where(r => r.RequestId == 3).FirstOrDefault();
                this.RequestToJoinCarpools.Add(request);

                this.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
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
