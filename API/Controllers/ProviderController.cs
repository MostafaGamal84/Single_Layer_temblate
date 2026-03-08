using System.Net.Mail;
using API.DTOs;
using API.DTOs.ProviderDto;
using API.Entities.Auctions;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
   
    public class ProviderController : BaseApiController
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
   
        public ProviderController(IUnitOfWork uow, ITokenService tokenService, SignInManager<AppUser> signInManager, IMapper mapper, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;

            _uow = uow;
            _tokenService = tokenService;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ProviderReturnDto>> Login(LoginDto loginDto)
        {
            var provider = await _userManager.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email.ToLower());

            if (provider == null) return Unauthorized("Invalid Email Or Password");

            var result = await _signInManager.CheckPasswordSignInAsync(provider, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid Email Or Password");

            var client = await _uow.ProvidersRepo.Map_GetBy<ProviderReturnDto>(x => x.Id == provider.Id);

            client.Token = _tokenService.CreateToken(provider);

            return Ok(client);
        }


        [HttpGet]

        public async Task<ActionResult<IEnumerable<ProviderReturnDto>>> GetProviders()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAll<ProviderReturnDto>();

            return Ok(provider);
        }
         [HttpGet("getPerson")]

        public async Task<ActionResult<IEnumerable<ProviderReturnDto>>> GetPerson()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAllBy<ProviderReturnDto>(x=> x.ProviderType.Name_en == "Person");

            return Ok(provider);
        }

         [HttpGet("GetInsurance")]

        public async Task<ActionResult<IEnumerable<ProviderReturnDto>>> GetInsurance()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAllBy<ProviderReturnDto>(x => x.ProviderType.Name_en == "Insurance");

            return Ok(provider);
        }

         [HttpGet("GetRepair")]

        public async Task<ActionResult<IEnumerable<ProviderReturnDto>>> GetRepair()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAllBy<ProviderReturnDto>(x => x.ProviderType.Name_en == "Repair");

            return Ok(provider);
        }
[AllowAnonymous]
         [HttpGet("GetWarranty")]

        public async Task<ActionResult<IEnumerable<ProviderReturnDto>>> GetWarranty()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAllBy<ProviderReturnDto>(x => x.ProviderType.Name_en == "Warranty");

            return Ok(provider);
        }

         [HttpGet("GetAuctionPartner")]

        public async Task<ActionResult<IEnumerable<ProviderReturnDto>>> GetAuctionPartner()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAllBy<ProviderReturnDto>(x => x.ProviderType.Name_en == "Auction Partner");

            return Ok(provider);
        }
        [HttpGet("GetProviderCount")]

        public  async Task<IEnumerable<ProviderReturnDto>> GetProviderCount()
        {

            var provider = await _uow.ProvidersRepo.Map_GetAll<ProviderReturnDto>();

            return provider;
        }

      

        [HttpGet("getPendingRequests")]
        public async Task<IActionResult> getPendingRequests()
        {
            
            var result = await _uow.ProvidersRepo.Map_GetAllBy<ProviderReturnDto>(x => x.IsApproved == false && x.IsPending == true );

            return Ok(result);
        }
       


       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
          
            var result = await _uow.ProvidersRepo.Map_GetBy<ProviderReturnDto>(x => x.Id == id);

            return Ok(result);
        }



        [HttpPut("update")]
        public async Task<IActionResult> Update(ProviderUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            dto.Id = User.GetUserId();

            var provider = await _uow.ProvidersRepo.GetById(dto.Id);

            var xx = provider.ProviderPhotos.Id;

            var mapforUpdate = _uow.Mapper.Map(dto, provider);

            mapforUpdate.ProviderPhotos.Id = xx;

            var PhotoFolderName = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.providerPhotos.CommericalBase64))
                mapforUpdate.ProviderPhotos.CommercialRecordPhotoUrl = await _uow.FileRepository
                .CreateFileFromBase64Async(dto.providerPhotos.CommericalBase64, PhotoFolderName);

            if (!string.IsNullOrWhiteSpace(dto.providerPhotos.CompanyBase64))
                mapforUpdate.ProviderPhotos.CompanyPhotoUrl = await _uow.FileRepository.
                CreateFileFromBase64Async(dto.providerPhotos.CompanyBase64, PhotoFolderName);

            if (!string.IsNullOrWhiteSpace(dto.providerPhotos.IdentityBase64))
                mapforUpdate.ProviderPhotos.IdentityPhotoUrl = await _uow.FileRepository
                .CreateFileFromBase64Async(dto.providerPhotos.IdentityBase64, PhotoFolderName);

            _uow.ProvidersRepo.Update(mapforUpdate);


            if (!await _uow.SaveAsync()) return BadRequest("Error Update");

            var map = _uow.Mapper.Map<ProviderReturnDto>(mapforUpdate);

            _uow.ProvidersRepo.Update(mapforUpdate);

            return Ok(map);

        }


        [HttpPut("updateFromAdmin")]
        public async Task<IActionResult> UpdateFromAdmin(ProviderUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var provider = await _uow.ProvidersRepo.GetById(dto.Id);

            var mapforUpdate = _uow.Mapper.Map(dto, provider);

            _uow.ProvidersRepo.Update(mapforUpdate);

            if (!await _uow.SaveAsync()) return BadRequest("Error Update");

            var map = _uow.Mapper.Map<ProviderReturnDto>(mapforUpdate);

            _uow.ProvidersRepo.Update(mapforUpdate);

            return Ok(map);

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _uow.ProvidersRepo.GetBy(x => x.Id == id);

            provider.IsDeleted = true;

            _uow.ProvidersRepo.Update(provider);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("error");
        }
         [AllowAnonymous]

        [HttpPost("SendEmail")]
        public  void SendEmail(string email)
        {

            // add from,to mailaddresses
            MailMessage myMail = new System.Net.Mail.MailMessage();
            myMail.From = new MailAddress("noreply@easacc.com");
            myMail.To.Add(email);

            // set subject and encoding
            myMail.Subject = "C-Car Account Update ";
            myMail.SubjectEncoding = System.Text.Encoding.UTF8;
            // set body-message and encoding
            myMail.Body =  "تم قبول حسابك كمزود خدمة في منصة سي كار " ;//محتوي الايميل
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

        [HttpPut("Approved/{id}")]
        public async Task<IActionResult> Approved(int id)
        {
            var provider = await _uow.ProvidersRepo.GetBy(x => x.Id == id);

            provider.IsApproved = true;
            

            _uow.ProvidersRepo.Update(provider);
             SendEmail(provider.Email);
            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("error");
        }

        
        [HttpPut("Refused/{id}")]
        public async Task<IActionResult> Refused(int id)
        {
            var provider = await _uow.ProvidersRepo.GetBy(x => x.Id == id);

          
            provider.IsPending = false;

            _uow.ProvidersRepo.Update(provider);

            if (await _uow.SaveAsync()) return Ok();

            return BadRequest("error");
        }
    }
}

