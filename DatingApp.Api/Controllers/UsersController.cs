using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Helpers;
using DatingApp.Api.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Api.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

       
        //getting normal users
        // public async Task<IActionResult> GetUsers()
        // {
        //     var users = await _repo.GetUsers();

        //     var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

        //     return Ok(usersToReturn);
        // }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
          // getting the user of the logged in user
          var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
          //getting the user from the repo with the user Id
          var userFromRepo =await _repo.GetUser(currentUserId, true);

          userParams.UserId = currentUserId;

          if(string.IsNullOrEmpty(userParams.Gender)){
            userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
          }
            var users = await _repo.GetUsers(userParams);

            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(usersToReturn);
        }


        [HttpGet("{id}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
          //checking if the user is the current user
          var isCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) == id;

            var user = await _repo.GetUser(id, isCurrentUser);

            var userToReturn = _mapper.Map<UserForDetailDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserForUpdateDto userForUpdateDto)
      {
          // confirming if the logged-in user is the user that wants to update the profile.
          if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            return Unauthorized();
          
          // This to get the user details from the data base
          var userFromRepo = await _repo.GetUser(id, true);

          // Updating the users with the dto.
        _mapper.Map(userForUpdateDto, userFromRepo);
         
          if(await _repo.SaveAll())
              return NoContent();

            throw new Exception($"Updating user {id} failed on save");
      }

      [HttpPost("{id}/like/{recipientId}")]
      public async Task<IActionResult> LikeUser(int id, int recipientId){

        // confirming if the logged-in user is the user that wants to like the person
          if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            return Unauthorized();

        // checking if the person has already been like by the logged in user
        var like = await _repo.GetLike(id,recipientId);

        // if the like exists
        if(like != null)
              return BadRequest("You already liked the user");
        
      // check if the recipient exists....
        if(await _repo.GetUser(recipientId, false) == null)
            return NotFound();
        
        like = new Like{
          LikerId = id,
          LikeeId = recipientId
        };

          // Add the like to the table
        _repo.Add<Like>(like);

        if(await _repo.SaveAll())
          return Ok();

        return BadRequest("Failed to like User");
      }
      
    }
}