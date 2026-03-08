using System.Net.Mail;
using API.DTOs;
using API.DTOs.AdminDto;
using API.DTOs.ClientDto;
using API.DTOs.ProviderDto;
using API.DTOs.ResetPasswordDto;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class AppAccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;

        private readonly IUnitOfWork _uow;

        private readonly IRepository<AppUserType> _appUserTypeRepo;
        private readonly IRepository<Subscribe> _subscribe;

        private readonly UserManager<AppUser> _userManager;

        private readonly SignInManager<AppUser> _signInManager;

        public AppAccountController(IUnitOfWork uow, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _uow = uow;
            _appUserTypeRepo = uow.Repository<AppUserType>();
            _subscribe = uow.Repository<Subscribe>();

            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        // Registering users who will use application mobile
        [AllowAnonymous]
        [HttpPost("clientRegister")]
        public async Task<ActionResult<ClientRegisterDto>> ClientRegister(ClientRegisterDto clientRegisterDto)
        {
            if (await UserExists(clientRegisterDto.Email)) return BadRequest("Email is Exist!");

            var user = _uow.Mapper.Map<Client>(clientRegisterDto);

            user.UserName = (clientRegisterDto.Email.Split('@')[0].ToString() + (_userManager.Users.Count() + 1)).ToLower().Replace(" ", "");

            user.AppUserTypeId = (await _appUserTypeRepo.GetByAsync(x => x.Name_en == "User")).Id;

            user.RegisterTime = DateTime.Now;


            user.IdentityPhotoUrl = await _uow.FileRepository
          .CreateFileFromBase64Async(clientRegisterDto.IdentityPhotoBase64, clientRegisterDto.Email);


            var result = await _userManager.CreateAsync(user, clientRegisterDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var userToReturn = _uow.Mapper.Map<ClientReturnDto>(user);

            userToReturn.Token = _tokenService.CreateToken(user);

            return Ok(userToReturn);
        }

        [AllowAnonymous]
        [HttpPost("socialClientRegister")]
        public async Task<ActionResult<SocialClientRegisterDto>> SocialClientRegister(SocialClientRegisterDto socialClientRegisterDto)
        {
            if (await UserExists(socialClientRegisterDto.Email)) return BadRequest("Email is Exist!");

            var user = _uow.Mapper.Map<Client>(socialClientRegisterDto);

            user.UserName = (socialClientRegisterDto.Email.Split('@')[0].ToString() + (_userManager.Users.Count() + 1)).ToLower().Replace(" ", "");

            user.AppUserTypeId = (await _appUserTypeRepo.GetByAsync(x => x.Name_en == "User")).Id;

            user.RegisterTime = DateTime.Now;
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);


            var userToReturn = _uow.Mapper.Map<SocialClientReturnDto>(user);

            userToReturn.Token = _tokenService.CreateToken(user);

            return Ok(userToReturn);
        }

        //login for user who will use application mobile
        [AllowAnonymous]
        [HttpPost("clientlogin")]
        public async Task<ActionResult<ClientReturnDto>> ClientLogin(LoginDto loginDto)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());

            if (user?.IsDeleted == true) { return BadRequest("This User Is Deleted "); }

            if (user == null) return Unauthorized("Invalid Email Or Password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid Email Or Password");

            var client = await _uow.ClientRepo.Map_GetBy<ClientReturnDto>(x => x.Id == user.Id);

            client.Token = _tokenService.CreateToken(user);

            return Ok(client);
        }

        [AllowAnonymous]
        [HttpPost("socialClientlogin")]
        public async Task<ActionResult<SocialClientReturnDto>> SocialClientLogin(SocialLoginDto loginDto)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());

            if (user == null) return Unauthorized("Invalid Email ");

            var client = await _uow.ClientRepo.Map_GetBy<SocialClientReturnDto>(x => x.Id == user.Id);

            client.Token = _tokenService.CreateToken(user);

            return Ok(client);
        }


        [HttpGet]

        public async Task<ActionResult<IEnumerable<ClientReturnDto>>> GetUsers()
        {
            var client = await _uow.ClientRepo.Map_GetAll<ClientReturnDto>();

            return Ok(client);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _uow.ClientRepo.Map_GetBy<ClientReturnDto>(x => x.Id == id);

            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> UpdateUserAdd(ClientUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var user = await _uow.UserRepo.GetById(dto.Id);

            var mapforUpdate = _uow.Mapper.Map(dto, user);

            var PhotoFolderName = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.IdentityPhotoBase64))
                mapforUpdate.IdentityPhotoUrl = await _uow.FileRepository.CreateFileFromBase64Async(dto.IdentityPhotoBase64, mapforUpdate.Email);


            _uow.UserRepo.Update(mapforUpdate);

            if (!await _uow.SaveAsync()) return BadRequest("Error Update");

            var map = _uow.Mapper.Map<ClientReturnDto>(mapforUpdate);
            _uow.UserRepo.Update(mapforUpdate);
            map.Token = dto.Token;

            return Ok(map);

        }

        [AllowAnonymous]
        [HttpPut("updateSubscribeCount/{subscribeId}")]
        public async Task<IActionResult> UpdateSubscribeCount(int subscribeId)
        {
            if (!ModelState.IsValid) return BadRequest();


            var user = await _uow.UserRepo.GetById(User.GetUserId());
            var Subscribe = await _subscribe.GetByIdAsync(subscribeId);

            user.SubscribeId = Subscribe.Id;
            user.SubscribeCount = user.SubscribeCount + Subscribe.AuctioningCount;
            _uow.UserRepo.Update(user);
            if (!await _uow.SaveAsync()) return BadRequest("Error Update");


            var map = _uow.Mapper.Map<ClientReturnDto>(user);

            return Ok(map);

        }


        [AllowAnonymous]
        [HttpPost("reset-password-token/{email}")]
        public async Task<IActionResult> ResetPasswordToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower());

            if (user == null) return Unauthorized("invalid Email");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = token;

            SendEmail(user.Email, callbackUrl);

            return Ok();
        }

        [AllowAnonymous]

        [HttpPost("SendEmail")]
        public void SendEmail(string email, string code)
        {

            // add from,to mailaddresses
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = new MailAddress("noreply@easacc.com");
            myMail.To.Add(email);

            // set subject and encoding
            myMail.Subject = "Reset Password";
            myMail.SubjectEncoding = System.Text.Encoding.UTF8;
            // set body-message and encoding
            myMail.Body = code;//محتوي الايميل
            myMail.BodyEncoding = System.Text.Encoding.UTF8;
            myMail.Priority = MailPriority.High;


            myMail.IsBodyHtml = true;

            using (var client = new SmtpClient())
            {
                client.Credentials = new System.Net.NetworkCredential("noreply@easacc.com", "Noreply5020#");
                client.Port = 587;
                client.Host = "mail.easacc.com";
                client.EnableSsl = true;

                client.Send(myMail);
            }


        }

        [AllowAnonymous]
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(string email, string newPassowrd, string token)
        {
            var user = await _userManager.FindByEmailAsync(email.ToLower());

            if (user == null) return Unauthorized("Invalid Email");

            await _userManager.ResetPasswordAsync(user, token, newPassowrd);

            return Ok();

        }

        [AllowAnonymous]
        [HttpPut("reset-passwordd")]
        public async Task<IActionResult> ResetPasswordd(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email.ToLower());

            if (user == null) return Unauthorized("Invalid Email");

            await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            return Ok();

        }


        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteClient(int id)
        // {
        //     var result = await _uow.ClientRepo.GetBy(x => x.Id == id);
        //     result.IsDeleted = !result.IsDeleted;
           
        //       _uow.ClientRepo.Update(result);

        //     if (await _uow.SaveAsync()) return Ok();

        //     return BadRequest("error");
          
        // }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _uow.ClientRepo.GetBy(x => x.Id == id);

            user.IsDeleted = !user.IsDeleted;

            _uow.ClientRepo.Update(user);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("error");
        }



        [HttpPut("RecoverAccount{email}")]
        public async Task<IActionResult> RecoverAccount(string email)
        {
            var user = await _uow.UserRepo.GetBy(x => x.Email == email);

            user.IsDeleted = !user.IsDeleted;

            _uow.UserRepo.Update(user);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("error");
        }

        //  User check Function

        private async Task<bool> UserExists(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }






        //    [HttpPut]
        // public async Task<IActionResult> UpdateUserAdd(ClientUpdateDto dto)
        // {
        //     if (!ModelState.IsValid) return BadRequest();

        //     var user = await _uow.UserRepo.GetById(dto.Id);

        //     var mapforUpdate = _uow.Mapper.Map(dto, user);

        //      var photos = new List<ClientPhoto>();

        //     foreach (var row in dto.clientPhotos)
        //     {
        //         var xx = new ClientPhoto();

        //         xx.IdentityPhotoUrl = await _uow.FileRepository
        //         .CreateFileFromBase64Async(row.FileBase64, User.GetUserName());

        //         photos.Add(xx);
        //     }

        //     user.clientPhotos = photos;

        //     _uow.UserRepo.Update(mapforUpdate);


        //     if (!await _uow.SaveAsync()) return BadRequest("Error Update");

        //     var map = _uow.Mapper.Map<ClientReturnDto>(mapforUpdate);

        //     _uow.UserRepo.Update(mapforUpdate);

        //     return Ok(map);

        // }
    }
}
