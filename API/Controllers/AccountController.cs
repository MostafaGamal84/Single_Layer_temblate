using API.DTOs;
using API.DTOs.AdminDto;
using API.DTOs.ProviderDto;
using API.Entities.Auctions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using UnitOfWork;

namespace API.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;

        private readonly IUnitOfWork _uow;

        private readonly IRepository<AppUserType> _appUserTypeRepo;

        private readonly UserManager<AppUser> _userManager;

        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(
            IUnitOfWork uow,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper
        )
        {
            _uow = uow;
            _appUserTypeRepo = uow.Repository<AppUserType>();
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
        }


        [HttpPost("adminRegister")]
        public async Task<ActionResult<AdminRegisterDto>> AdminRegister(AdminRegisterDto adminRegisterDto)
        {
            if (await UserExists(adminRegisterDto.Email)) return BadRequest("Email is Exist!");

            var user = _uow.Mapper.Map<Admin>(adminRegisterDto);

            user.UserName = adminRegisterDto.Email.ToLower();

            user.AppUserTypeId = (await _appUserTypeRepo.GetByAsync(x => x.Name_en == "Admin")).Id;

            user.RegisterTime = DateTime.Now;

            user.IsDeleted = false;

            var result = await _userManager.CreateAsync(user, adminRegisterDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var client = _uow.Mapper.Map<AdminReturnDto>(user);

            client.Token = _tokenService.CreateToken(user);

            return Ok(client);
        }


        [HttpPost("providerRegister")]
        public async Task<ActionResult<ProviderRegisterDto>> ProviderRegister(ProviderRegisterDto dto)
        {
            if (await UserExists(dto.Email)) return BadRequest("Email is Exist!");

            var provider = _uow.Mapper.Map<Provider>(dto);

            provider.UserName = dto.FirstName.ToLower();

            provider.AppUserTypeId = (await _appUserTypeRepo.GetByAsync(x => x.Name_en == "Provider")).Id;

            provider.RegisterTime = DateTime.Now;

            provider.IsApproved = false;
            provider.IsPending = true;

            var PhotoFolderName = dto.FirstName;

            provider.ProviderPhotos.CommercialRecordPhotoUrl = await _uow.FileRepository
            .CreateFileFromBase64Async(dto.providerPhotos.CommericalBase64, PhotoFolderName);

            provider.ProviderPhotos.CompanyPhotoUrl = await _uow.FileRepository.
            CreateFileFromBase64Async(dto.providerPhotos.CompanyBase64, PhotoFolderName);

            provider.ProviderPhotos.IdentityPhotoUrl = await _uow.FileRepository
            .CreateFileFromBase64Async(dto.providerPhotos.IdentityBase64, PhotoFolderName);

            var result = await _userManager.CreateAsync(provider, dto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            _uow.ProvidersRepo.AddAsync(provider);

            var providerToReturn = _uow.Mapper.Map<ProviderReturnDto>(provider);

            providerToReturn.Token = _tokenService.CreateToken(provider);

            return Ok(providerToReturn);
        }


        [HttpPost("providerRegisterFromAdmin")]
        public async Task<ActionResult<ProviderRegisterDto>> ProviderRegisterFromAdmin(ProviderRegisterDto dto)
        {
            if (await UserExists(dto.Email)) return BadRequest("Email is Exist!");

            var provider = _uow.Mapper.Map<Provider>(dto);

            provider.UserName = dto.Email.ToLower();

            provider.AppUserTypeId = (await _appUserTypeRepo.GetByAsync(x => x.Name_en == "Provider")).Id;

            provider.RegisterTime = DateTime.Now;

            provider.IsApproved = false;

            var PhotoFolderName = dto.FirstName;

            provider.ProviderPhotos.CommercialRecordPhotoUrl = await _uow.FileRepository
            .CreateFileFromBase64Async(dto.providerPhotos.CommericalBase64, PhotoFolderName);

            provider.ProviderPhotos.CompanyPhotoUrl = await _uow.FileRepository.
            CreateFileFromBase64Async(dto.providerPhotos.CompanyBase64, PhotoFolderName);

            provider.ProviderPhotos.IdentityPhotoUrl = await _uow.FileRepository
            .CreateFileFromBase64Async(dto.providerPhotos.IdentityBase64, PhotoFolderName);

            var result = await _userManager.CreateAsync(provider, dto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            _uow.ProvidersRepo.AddAsync(provider);

            var providerToReturn = _uow.Mapper.Map<ProviderReturnDto>(provider);

            return Ok(providerToReturn);
        }


        private async Task<bool> UserExists(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
    }
}
