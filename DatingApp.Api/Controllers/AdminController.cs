using System.Threading.Tasks;
using DatingApp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatingApp.Api.Dtos;
using Microsoft.AspNetCore.Identity;
using DatingApp.Api.Model;
using Microsoft.Extensions.Options;
using DatingApp.Api.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DatingApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        private Cloudinary _cloudinary;

        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _context = context;
            _userManager = userManager;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );
            
            _cloudinary = new Cloudinary(acc);
        }
        [Authorize(Policy="RequireAdminRole")]
        [HttpGet("userswithroles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await (from user in _context.Users orderby user.UserName
                                    select new{
                                        Id = user.Id,
                                        UserName = user.UserName,
                                        Roles = (from userRole in user.UserRoles
                                        join role in _context.Roles on userRole.RoleId equals role.Id
                                        select role.Name).ToList()
                                    }).ToListAsync();
            return Ok(userList);

        }
        
        [Authorize(Policy="RequireAdminRole")]
        [HttpPost("editroles/{userName}")]
        public async Task<IActionResult> EditRoles ([FromRoute]string userName,[FromBody] RoleEditDto roleEditDto){

            //getting the user from the database 
            var user = await _userManager.FindByNameAsync(userName);
            //getting the roles attached to the user
            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDto.RoleNames;

            //selected = selectedRoles != null ? selectedRoles : new string[] {};
            selectedRoles = selectedRoles ?? new string[] {};

            //adding the roles to the user
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded)
                return BadRequest("Failed to add to the Roles");
            //removing the roles to the user
                result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if(!result.Succeeded)
                return  BadRequest("Failed to remove the Roles");
            
            return Ok(await _userManager.GetRolesAsync(user));
            
        }


        [Authorize(Policy= "ModeratePhotoRole")]
        [HttpGet("photosformoderation")]
        public async  Task<IActionResult> GetPhotosForModeration(){
          var photos = await _context.Photos
            .Include(u => u.User)
            .IgnoreQueryFilters()
            .Where(p => p.IsApproved == false)
            .Select(u => new {
                Id = u.Id,
                UserName = u.User.UserName,
                Url = u.Url,
                IsApproved = u.IsApproved
            }).ToListAsync();

            return Ok(photos);
        }

        [Authorize(Policy= "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]

        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            var photo = await _context.Photos
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.Id == photoId);
            
            photo.IsApproved = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
        
        [Authorize(Policy= "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            var photo = await _context.Photos
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.Id == photoId);
            
            if(photo.IsMain){
                return BadRequest("You cannot reject the main Photo");
            }

            if(photo.PublicID != null){
                var deleteParams = new DeletionParams(photo.PublicID);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok")
                {
                    _context.Photos.Remove(photo);
                }
            }

            if(photo.PublicID == null)
            {
                _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}