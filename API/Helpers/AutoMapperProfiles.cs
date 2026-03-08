using API.DTOs;
using API.DTOs.AdminDto;
using API.DTOs.AuctionDto;
using API.DTOs.AuctionRecordDto;
using API.DTOs.BannersDto;
using API.DTOs.BrandNameDto;
using API.DTOs.CarStatusDto;
using API.DTOs.CarTypeDto;
using API.DTOs.ClientDto;
using API.DTOs.ColorDto;
using API.DTOs.EndedAuctionsDto;
using API.DTOs.FavoriteAuction;
using API.DTOs.GenderDTO;
using API.DTOs.InsuranceDto;
using API.DTOs.ItemDto;
using API.DTOs.ItemPhotoDto;
using API.DTOs.ModelDto;
using API.DTOs.ProviderDto;
using API.DTOs.ProviderPhotoDto;
using API.DTOs.RepairDto;
using API.DTOs.TermsDto;
using API.DTOs.VehicleTypeDto;
using API.DTOs.WarrantyDto;
using API.Entities.Auctions;
using AutoMapper;

namespace API.Helpers
{

    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // CreateMap<BaseEntity, BaseDto>();
            CreateMap<Provider, ProviderUpdateDto>()
                .IncludeAllDerived()
                .IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

            // CreateMap<DateTime, DateTime>().ConvertUsing(x => DateTime.SpecifyKind(x, DateTimeKind.Utc ));



            CreateMap<ClientRegisterDto, Client>();
            CreateMap<Client, ClientReturnDto>();
            CreateMap<SocialClientRegisterDto, Client>();
            CreateMap<Client, SocialClientReturnDto>();
            CreateMap<ClientUpdateDto, Client>();



            CreateMap<ProviderRegisterDto, Provider>().ReverseMap();
            CreateMap<Provider, ProviderReturnDto>();
            CreateMap<ProviderPhoto, ProviderReturnDto>();
            CreateMap<ProviderUpdateDto, Provider>();

            CreateMap<ProviderTypeAddDto, ProviderType>().ReverseMap();
            CreateMap<ProviderType, ProviderTypeReturnDto>();


            CreateMap<SubscribeDto, Subscribe>().ReverseMap();


            CreateMap<AdminRegisterDto, Admin>();
            CreateMap<Admin, AdminReturnDto>();
            CreateMap<AdminUpdateDto, Admin>();

            CreateMap<KeyDto, Key>().ReverseMap();

            CreateMap<AuctionAddDto, Auction>();
            CreateMap<Auction, AuctionReturnDto>()
                .ForMember(x => x.IsFavorite, opt => opt.MapFrom(src => src.FavoriteAuctions.First() == null ? false : src.FavoriteAuctions.FirstOrDefault().Status))
                .ForMember(x => x.TotalRecord, opt => opt.MapFrom(src => src.AuctionRecords.Count))
                .ForMember(x => x.Price, opt => opt.MapFrom(src => src.AuctionRecords.Count > 0 ? src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Price : 0))
                .ForMember(x => x.ClientName, opt => opt.MapFrom(src => src.AuctionRecords.Count > 0 ? src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Client.FirstName : ""))
                .ForMember(x => x.ProviderName, opt => opt.MapFrom(src => src.ProviderId != null ? src.Provider.FirstName + " " + src.Provider.LastName : src.Admin.FirstName + " " + src.Admin.LastName));
  

            CreateMap<ItemPhotoAddDto, ItemPhoto>();
            CreateMap<ItemPhoto, ItemPhotoReturnDto>().ReverseMap();

            CreateMap<ProviderPhotoAddDto, ProviderPhoto>();
            CreateMap<AuctionRecord, AuctionListDto>();

            CreateMap<ProviderPhoto, ProviderPhotoReturnDto>().ReverseMap();


            CreateMap<AuctionRecordAddDto, AuctionRecord>();
            CreateMap<PlusAndMinusDto, AuctionRecord>();
        

            CreateMap<AuctionRecord, AuctionRecordReturnDto>()
             .ForMember(x => x.ClientName, opt => opt.MapFrom(src => src.Client.FirstName + " " + src.Client.LastName))
             .ForMember(x => x.ClientPhone, opt => opt.MapFrom(src => src.Client.MobileNumber));

            CreateMap<Auction, EndedAuctionReturnDto>()
                .ForMember(x => x.TotalRecord, opt => opt.MapFrom(src => src.AuctionRecords.Count))
                .ForMember(x => x.Price, opt => opt.MapFrom(src => src.AuctionRecords.Count > 0 ? src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Price : 0))
                .ForMember(x => x.itemPhotos, opt => opt.MapFrom(src => src.itemPhotos))
            
                .ForMember(x => x.SellerAmount, opt => opt.MapFrom((src => src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Price - (src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().AppPercent))))
                
                .ForMember(x => x.ProviderName, opt => opt.MapFrom(src => src.ProviderId != null ? src.Provider.FirstName + " " + src.Provider.LastName : src.Admin.FirstName + " " + src.Admin.LastName))
                .ForMember(x => x.ProviderType, opt => opt.MapFrom(src => src.ProviderId != null ? src.Provider.ProviderType.Name_en  : "Person" ))
                .ForMember(x => x.AppPercent, opt => opt.MapFrom(src =>  src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().AppPercent ))
                .ForMember(x => x.Percent, opt => opt.MapFrom(src =>  src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Percent))
                 .ForMember(x => x.Insurance, opt => opt.MapFrom(src =>  src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Client.Subscribe.Price))
                .ForMember(x => x.FinalTotal, opt => opt.MapFrom(src =>  src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().AppPercent + src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().PlusOrMinus))
                
                .ForMember(x => x.CarIdNumber, opt => opt.MapFrom(src => src.CarId))
                .ForMember(x => x.PlusOrMinus, opt => opt.MapFrom(src => src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().PlusOrMinus))
                .ForMember(x => x.AuctionId, opt => opt.MapFrom(src => src.AuctionRecords.OrderBy(x => x.Price).LastOrDefault().Id));
               

            CreateMap<AppUserType, AppUserTypeDto>();

            CreateMap<VehicleTypeAddDto, VehicleType>();
            CreateMap<VehicleType, VehicleTypeReturnDto>();
            CreateMap<VehicleTypeDto, VehicleType>();

            CreateMap<Gender, GenderDto>().ReverseMap();
            CreateMap<CarCommetion, CarCommesionDto>()
            .ReverseMap();

            CreateMap<RepairAddDto, Repair>();
            CreateMap<Repair, RepairReturnDto>();

            CreateMap<WarrantyAddDto, Warranty>();
            CreateMap<Warranty, WarrantyReturnDto>();


            CreateMap<InsuranceAddDto, Insurance>();
            CreateMap<Insurance, InsuranceReturnDto>();

            CreateMap<FavoriteAuctionAddDto, FavoriteAuction>();
            CreateMap<FavoriteAuction, FavoriteAuctionReturnDto>();

            CreateMap<ModelAddDto, Model>();
            CreateMap<Model, ModelReturnDto>();

            CreateMap<ItemAddDto, Item>().ReverseMap();
            CreateMap<Item, ItemReturnDto>();

            CreateMap<ColorAddDto, Color>();
            CreateMap<Color, ColorReturnDto>();

            CreateMap<CarTypeAddDto, CarType>();
            CreateMap<CarType, CarTypeReturnDto>();

            CreateMap<CarStatusAddDto, CarStatus>();
            CreateMap<CarStatus, CarStatusReturnDto>();

            CreateMap<BrandNameAddDto, BrandName>();
            CreateMap<BrandName, BrandNameReturnDto>();

            CreateMap<ItemPhotoAddDto, ItemPhoto>();
            CreateMap<ItemPhoto, ItemPhotoReturnDto>();

            CreateMap<TermsAndConditionsAddDto, TermsAndConditions>();
            CreateMap<TermsAndConditions, TermsAndConditionsReturnDto>();

            CreateMap<BannersAddDto, Banners>();
            CreateMap<Banners, BannersReturnDto>();


            CreateMap<AboutUsAddDto, AboutUs>();
            CreateMap<AboutUs, AboutUsReturnDto>();
        }
    }
}