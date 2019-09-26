using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using DatingApp.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DatingApp.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
      
        private readonly IConfiguration  _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

      

        public AuthController(IConfiguration config, 
            IMapper mapper, 
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
          
            _config = config;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            //creating the user in the database with the username
             var userToCreate = _mapper.Map<User>(userForRegisterDto);

             //passing the user and password
             var result = await _userManager.CreateAsync(userToCreate,userForRegisterDto.Password);
           
             //user to return
             var userToReturn = _mapper.Map<UserForDetailDto>(userToCreate);

             if(result.Succeeded)
             {
                // The CreatedAtRoute method is intended to return a URL to the newly created resource when you invoke a POST method to store some new object.
                return CreatedAtRoute("GetUser", new {controller ="Users", id = userToReturn.Id}, userToReturn);
             }

             return BadRequest(result.Errors);

           
        }

         [HttpPost("login")]
         public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
         {
            //looking for the user through userManager
            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);
            if(user == null)
                return BadRequest("Unauthorised");

            //using the user to sign in
            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if (result.Succeeded)
            {
                //getting the user with their photos
                var appUser = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == userForLoginDto.Username.ToUpper());

                //mapping the appuser to the user to be returned
                var userToReturn = _mapper.Map<UserForListDto>(appUser);

                return Ok(new
                {
                    token = await GenerateJwtToken(appUser),
                    user = userToReturn
                });
            }

            return Unauthorized();

         }

        private async Task <string> GenerateJwtToken(User user)
        {
            //The token contains two claims
            var claims = new List<Claim>
            {
                //This is claims for the ID..
                new Claim (ClaimTypes.NameIdentifier, user.Id.ToString()),

                //This is claims for the Name..
                new Claim(ClaimTypes.Name, user.UserName)
            };

            //including the roles in the token
            var roles = await _userManager.GetRolesAsync(user);
        foreach(var role in roles)
            {
            claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //Creating a kry to ensure the token generated is secure
            var key = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //The token description
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            //creating the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }
    }
}