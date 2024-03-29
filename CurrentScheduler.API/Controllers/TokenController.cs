﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ZKrakus.CurrentScheduler.API.Controllers
{
    public class TokenController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<AppUser> userManager;

        public TokenController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Route("/token")]
        [HttpPost]
        public async Task<IActionResult> Create(string username, string password)
        {
            if( await IsValidUsernameAndPassword(username, password))
            {
                return new ObjectResult(await GenerateToken(username));
            } else
            {
                return BadRequest();
            }
        }

        public async Task<bool> IsValidUsernameAndPassword(string username, string password)
        {
            var user = await userManager.FindByEmailAsync(username);
            return await userManager.CheckPasswordAsync(user, password);
        }

        public async Task<dynamic> GenerateToken(string username)
        {
            var user = await userManager.FindByEmailAsync(username);
            var roles = context.UserRoles.Join(context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, ur.RoleId, r.Name });

            // JWT uses a claim system
            // The security of JWT comes from the inability to externally manipulate the token without the token issuer (us) rejecting it.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySecretKey")), // If this secret gets out someone would be able to edit and forge the token.
                            SecurityAlgorithms.HmacSha256 // Signing Algorithm
                        )
                    ),
                    new JwtPayload(claims)
                );

            var output = new
            {
                Access_Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = username
            };

            return output;
        }
    }
}
