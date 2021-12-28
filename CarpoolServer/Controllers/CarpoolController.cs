using CarpoolServerBL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CarpoolServer.Controllers
{
    [Route("CarpoolAPI")]
    [ApiController]
    public class CarpoolController : ControllerBase
    {
        #region Add connection to the db context using dependency injection
        CarpoolDBContext context;
        public CarpoolController(CarpoolDBContext context)
        {
            this.context = context;
        }
        #endregion

        //set the user default photo image name
        public const string DEFAULT_PHOTO = "defaultphoto.jpg";

        [Route("Login")]
        [HttpGet]
        public User Login([FromQuery] string email, [FromQuery] string pass)
        {
            User user = this.context.Login(email, pass);

            //Check user name and password
            if (user != null)
            {
                HttpContext.Session.SetObject("theUser", user);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;

                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return user;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }

        [Route("UpdateUser")]
        [HttpPost]
        public User UpdateUser([FromBody] User user)
        {
            //If user is null the request is bad
            if (user == null)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return null;
            }

            User currentUser = HttpContext.Session.GetObject<User>("theUser");
            //Check if user logged in and its ID is the same as the contact user ID
            if (currentUser != null && currentUser.Id == user.Id)
            {
                User updatedUser = context.UpdateUser(currentUser, user);

                if (updatedUser == null)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return null;
                }

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return updatedUser;

                ////Now check if an image exist for the contact (photo). If not, set the default image!
                //var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", DEFAULT_PHOTO);
                //var targetPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{user.Id}.jpg");
                //System.IO.File.Copy(sourcePath, targetPath);

                //return the contact with its new ID if that was a new contact
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }

        [Route("IsEmailExist")]
        [HttpGet]
        public bool IsEmailExist([FromQuery] string email)
        {
            bool isExist = this.context.EmailExist(email);
            if (isExist)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }

        [Route("IsUserNameExist")]
        [HttpGet]
        public bool IsUserNameExist([FromQuery] string userName)
        {
            bool isExist = this.context.UserNameExist(userName);
            if (isExist)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return true;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return false;
            }
        }

        [Route("AdultSignUp")]
        [HttpPost]
        public Adult AdultSignUp([FromBody] Adult adult)
        {
            //Check user name and password
            if (adult != null)
            {
                this.context.AdultSignUp(adult);

                //Copy defualt image for this adult
                var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                System.IO.File.Copy(pathFrom, pathTo);

                //adult.IdNavigation.Photo = $"{adult.Id}.jpg";
                //this.context.SaveChanges();

                HttpContext.Session.SetObject("theUser", adult);
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return adult;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }

        [Route("KidSignUp")]
        [HttpPost]
        public Kid KidSignUp([FromBody] KidsOfAdult kidsOfAdult)
        {
            //Check user name and password
            if (kidsOfAdult != null)
            {
                User a = HttpContext.Session.GetObject<User>("theUser");

                Adult adult = kidsOfAdult.Adult;
                Kid kid = kidsOfAdult.Kid;
                this.context.AddKid(adult, kid);

                //Copy defualt image for this adult
                var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{kid.Id}.jpg");
                System.IO.File.Copy(pathFrom, pathTo);

                //HttpContext.Session.SetObject("theUser", kid);
                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return kid;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }

        [Route("AddAdult")]
        [HttpPost]
        public Adult AddAdult([FromBody] Adult adult)
        {
            //Check user name and password
            if (adult != null)
            {
                User currentUser = HttpContext.Session.GetObject<User>("theUser");
                Adult currentAdult = new Adult()
                {
                    IdNavigation = currentUser
                };

                this.context.AddAdult(currentAdult, adult);

                //Copy defualt image for this adult
                var pathFrom = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", "defaultphoto.jpg");
                var pathTo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{adult.Id}.jpg");
                System.IO.File.Copy(pathFrom, pathTo);

                Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                //Important! Due to the Lazy Loading, the user will be returned with all of its contects!!
                return adult;
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                return null;
            }
        }

        [Route("UploadImage")]
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            User user = HttpContext.Session.GetObject<User>("theUser");
            //Check if user logged in and its ID is the same as the contact user ID
            if (user != null)
            {
                if (file == null)
                {
                    return BadRequest();
                }

                try
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", file.FileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }


                    return Ok(new { length = file.Length, name = file.FileName });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return BadRequest();
                }
            }
            return Forbid();
        }
    }
}