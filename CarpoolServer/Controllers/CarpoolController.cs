using CarpoolServerBL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [Route("Login")]
        [HttpGet]
        public User Login([FromQuery] string email, [FromQuery] string pass)
        {
            User user = context.Login(email, pass);

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

        [Route("AdultSignUp")]
        [HttpPost]
        public Adult AdultSignUp([FromBody] Adult adult)
        {
            //Check user name and password
            if (adult != null)
            {
                this.context.AddAdult(adult);
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

        [Route("KidSignUp")]
        [HttpGet]
        public Kid KidSignUp([FromQuery] string email, [FromQuery] string userName, [FromQuery] string pass,
            [FromQuery] string fName, [FromQuery] string lName, [FromQuery] DateTime birthDate,
            [FromQuery] string phoneNumber, [FromQuery] string photo, [FromQuery] string city,
            [FromQuery] string neighborhood, [FromQuery] string street, [FromQuery] int houseNum)
        {
            User user = new User()
            {
                Email = email,
                UserName = userName,
                UserPswd = pass,
                FirstName = fName,
                LastName = lName,
                BirthDate = birthDate,
                PhoneNum = phoneNumber,
                Photo = photo,
                City = city,
                Neighborhood = neighborhood,
                Street = street,
                HouseNum = houseNum
            };

            Kid kid = new Kid()
            {
                IdNavigation = user
            };

            //Check user name and password
            if (kid != null)
            {
                this.context.AddKid(kid);
                HttpContext.Session.SetObject("theUser", user);
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

        [Route("GetString")]
        [HttpGet]
        public string GetString()
        {
            return "HELLO WORLD";
        }
    }
}