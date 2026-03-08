using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.DTOs.AdminDto;
using API.DTOs.ResetPasswordDto;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{

 [AllowAnonymous]

    public class AdminController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AdminController(IUnitOfWork uow, ITokenService tokenService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, Data.DataContext context)
        {
            _tokenService = tokenService;

            _signInManager = signInManager;
            _userManager = userManager;
            _uow = uow;

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AdminReturnDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());

            if (user == null) return Unauthorized("Invalid Email Or Password");

            var result =await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid Email Or Password");

            var client = await _uow.AdminRepo.Map_GetBy<AdminReturnDto>(x => x.Id == user.Id);
            
            client.Token = _tokenService.CreateToken(user);

            return Ok(client);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminReturnDto>>> GetAdmins()
        {
            var admin = await _uow.AdminRepo.Map_GetAll<AdminReturnDto>();

            return Ok(admin);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var x = await _uow.AdminRepo.Map_GetBy<AdminReturnDto>(x => x.Id == id);

            return Ok(x);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateAdmin(AdminUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var Admin = await _uow.AdminRepo.GetBy(x => x.Id == dto.Id);

            var map = _uow.Mapper.Map(dto, Admin);

            _uow.AdminRepo.Update(map);

            if (await _uow.SaveAsync()) return Ok(dto);

            return BadRequest("error");
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _uow.AdminRepo.GetBy(x => x.Id == id);

            x.IsDeleted = true;

            _uow.AdminRepo.Update(x);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("error");
        }

        
        [AllowAnonymous]
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email.ToLower());

            if (user == null) return Unauthorized("Invalid Email");

            await _userManager.ResetPasswordAsync(user,dto.Token, dto.NewPassword);
        
            return Ok();

        }

    }

}