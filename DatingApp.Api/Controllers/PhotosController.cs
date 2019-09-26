 using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Helpers;
using DatingApp.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.Api.Controllers
{
   
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;

        public PhotosController(IDatingRepository repo, IMapper mapper,IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            // Cloudinary configuration 
            Account acc = new Account(

               _cloudinaryConfig.Value.CloudName,
               _cloudinaryConfig.Value.ApiKey,
               _cloudinaryConfig.Value.ApiSecret
               );

            _cloudinary = new Cloudinary(acc);
        }
    // Method to get the the list of photos from the database
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto (int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }
    // method to upload a picture
        [HttpPost]
        public async Task<IActionResult> AddPhotosForUser(int userId,[FromForm] PhotoForCreationDto photoForCreationDto)
        {
            // confirming if the logged-in user is the user that wants to update the profile.
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // This to get the user details from the data base
            var userFromRepo = await _repo.GetUser(userId, true);

            var file = photoForCreationDto.File;

            //The response expected from cloudinary
            var uploadResult = new ImageUploadResult();

            //this block of code checks if the file has been uploaded and transforms to a specific size
            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face") 
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            // getting the response from cloudinary
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicID = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            //checking if there is a main picture in the database
            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            userFromRepo.Photos.Add(photo);

          

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Could not add the Photo");
        }
    // Method to set a photo as the main photo
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id){

            // confirming if the logged-in user is the user that wants to update the profile.
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId, true);

            //checking if the user is the owner of the picture
            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            //checking if the photo is the main photo already
            var photoFromRepo = await _repo.GetPhoto(id);
            if(photoFromRepo.IsMain)
                return BadRequest("This is already the Main Photo");

            //getting and setting the current main photo to false.
                var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
                currentMainPhoto.IsMain = false; 

            //setting the photo as the main photo
            photoFromRepo.IsMain =true;

            if(await _repo.SaveAll())
                return NoContent();
            
            return BadRequest("Could not set Photo to Main");
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id){

        // confirming if the logged-in user is the user that wants to update the profile.
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId, true);

            //checking if the user is the owner of the picture
            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            //checking if the photo is the main photo already
            var photoFromRepo = await _repo.GetPhoto(id);
            if(photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo");
            //delete photos stored on cloudinary
           if(photoFromRepo.PublicID != null){
               
            
            var deleteParams = new DeletionParams(photoFromRepo.PublicID);
            
            var result = _cloudinary.Destroy(deleteParams);

            if(result.Result == "ok"){

                _repo.Delete(photoFromRepo);
            }

           }

           if(photoFromRepo.PublicID == null){

               _repo.Delete(photoFromRepo);
           }
         
            if(await _repo.SaveAll())
             return Ok();

             return BadRequest("Failed to delete Photo");
        }
    }
}