using System;
using System.Threading.Tasks;
using DatingApp.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Api.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<User> Login(string username, string password)
        {
            //finding a matching user
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == username);

            //If no user was found
            if(user == null)
                return null;

            //vreifying the password of the user.
            //if(!VerifyPasswordHash(password, user.PasswordHash,user.PasswordSalt))
            //return null;

            return user;

        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
           using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
               // looping over each components of the hash
                for(int i = 0; i< computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }
    

        public async Task<User> Register(User user, string password)
        {
            //creating a reference for the password hash and password salt
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            //user.PasswordHash = passwordHash;
            //user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        //This method helps to encrypt the password
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                // password salt is a randomly generated key
                passwordSalt = hmac.Key;

                // password hash is a hashing the password itself
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            
        }

        public async Task<bool> UserExists(string username)
        {
            //checking if there is a matching username in the database
           if(await _context.Users.AnyAsync(x => x.UserName == username))
                return true;

           return false;
        }
    }
}