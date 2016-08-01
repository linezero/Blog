using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ASPNETCoreAPI.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ASPNETCoreAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private DataContext Context;
        public UsersController(DataContext _context)
        {
            Context = _context;
        }
        // GET: api/users
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(Context.Users.ToList());
        }

        // GET api/users/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var _user = Context.Users.FirstOrDefault(r => r.Id == id);
            if (_user == null)
                return NotFound();
            return Ok(_user);
        }

        // POST api/users
        [HttpPost]
        public IActionResult Post([FromBody]User user)
        {
            Context.Add(user);
            Context.SaveChanges();
            return Created($"api/users/{user.Id}",user);
        }

        // PUT api/users/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]User user)
        {
            var _user = Context.Users.FirstOrDefault(r => r.Id == id);
            if (_user == null)
                return NotFound();
            _user.UserName = user.UserName;
            _user.Password = user.Password;
            Context.Update(_user);
            Context.SaveChanges();
            return Created($"api/users/{_user.Id}", _user);
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var _user = Context.Users.FirstOrDefault(r => r.Id == id);
            if (_user == null)
                return NotFound();
            Context.Remove(_user);
            Context.SaveChanges();
            return NoContent();
        }
    }
}
