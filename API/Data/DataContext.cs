using API.Entities;
using API.Entities.Auctions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
     IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
     IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<AppUserType> appUserTypes { get; set; }
        public DbSet<Admin> admins { get; set; }
      
        public DbSet<Gender> genders { get; set; }
        public DbSet<VehicleType> vehicleTypes { get; set; }
        public DbSet<Repair> repairs { get; set; }
        public DbSet<Warranty> warranties { get; set; }
        public DbSet<Insurance> insurances { get; set; }
        public DbSet<BrandName> brandNames { get; set; }
        public DbSet<CarStatus> carStatuses { get; set; }
        public DbSet<CarType> carTypes { get; set; }
        public DbSet<Model> models { get; set; }
        public DbSet<Color> colors { get; set; }
        public DbSet<Item> items { get; set; }
        public DbSet<ItemPhoto> itemPhotos { get; set; }
        public DbSet<ProviderType> providerTypes { get; set; }
        public DbSet<TermsAndConditions> termsAndConditions { get; set; }
        public DbSet<AboutUs> aboutUs { get; set; }
        public DbSet<Auction> auctions { get; set; }
        public DbSet<ProviderPhoto> providerPhotos { get; set; }
  
        public DbSet<FavoriteAuction> FavoriteAuctions { get; set; }
        public DbSet<Banners> banners { get; set; }
        public DbSet<Key> keys { get; set; }
        public DbSet<CarCommetion> carCommetions { get; set; }
        public DbSet<Subscribe> Subscribes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

            builder.Entity<AppUserType>()
           .HasMany(x => x.Users)
           .WithOne(x => x.AppUserType)
           .HasForeignKey(x => x.AppUserTypeId)
           .IsRequired();

            builder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

            builder.Entity<Provider>()
            .HasMany(ur => ur.Auctions)
            .WithOne(ur => ur.Provider)
            .HasForeignKey(ur => ur.ProviderId);

        

            builder.Entity<Client>()
            .HasMany(ur => ur.AuctionRecords)
            .WithOne(u => u.Client)
            .HasForeignKey(ur => ur.ClientId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

            
           

            //  builder.Entity<Client>()
            // .HasOne(ur => ur.Subscribe)
            // .WithMany(u => u.Clients)
            // .HasForeignKey(ur => ur.SubscribeId)
            // .OnDelete(DeleteBehavior.NoAction)
            // .IsRequired();



           builder.Entity<Client>()
            .HasMany(ur => ur.FavoriteAuctions)
            .WithOne(u => u.Client)
            .HasForeignKey(ur => ur.ClientId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

             

            builder.Entity<Item>()
            .HasMany(ur => ur.itemPhotos)
            .WithOne(ur => ur.Item)
            .HasForeignKey(ur => ur.ItemId)
            .IsRequired();


            builder.Entity<BrandName>()
           .HasMany(ur => ur.Item)
           .WithOne(u => u.BrandName)
           .HasForeignKey(ur => ur.BrandNameId)
           .IsRequired();


            builder.Entity<ProviderType>()
           .HasMany(ur => ur.Provider)
           .WithOne(ur => ur.ProviderType)
           .HasForeignKey(ur => ur.ProviderTypeId)
           .IsRequired();

            builder.Entity<Auction>()
            .HasMany(ur => ur.AuctionRecords)
            .WithOne(ur => ur.Auction)
            .HasForeignKey(ur => ur.AuctionId)
            .IsRequired();

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){
            optionsBuilder.UseLazyLoadingProxies();
        }


    }
}