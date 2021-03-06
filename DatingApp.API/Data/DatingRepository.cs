using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Controllers.Models.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            this._context = context;

        }
        public void Add<T>(T entity) where T : class
        {
           _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
           _context.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo =  await _context.Photos.FirstOrDefaultAsync(x => x.Id == id);
            
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
           return await _context.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
           var users = _context.Users.Include(x => x.Photos)
                        .OrderByDescending(x => x.LastActive).AsQueryable();

           users = users.Where(x => x.Id != userParams.UserId);

           users = users.Where(x => x.Gender == userParams.Gender);

           if(userParams.MinAge != 18 || userParams.MaxAge != 99)
           {
              var  minDob = DateTime.Today.AddYears(-userParams.MaxAge  - 1);
              var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

              users = users.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
           }
           if(!string.IsNullOrEmpty(userParams.OrderBy))
           {
               switch(userParams.OrderBy)
               {
                   case "created" : 
                     users = users.OrderByDescending(x => x.Created);
                     break;
                   default:
                     users = users.OrderByDescending(x => x.LastActive);
                     break;
               }
           }
           return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }
        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}