using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using API.Entities.Auctions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {

        public static async Task SeedUsers(DataContext context)
        {

            if (!await context.appUserTypes.AnyAsync())
            {
                var data = await ReadData<AppUserType>("UserType");
                await context.appUserTypes.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
            
             if (!await context.vehicleTypes.AnyAsync())
            {
                var data = await ReadData<VehicleType>("VehicleType");
                await context.vehicleTypes.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
             if (!await context.keys.AnyAsync())
            {
                var data = await ReadData<Key>("Key");
                await context.keys.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
             if (!await context.genders.AnyAsync())
            {
                var data = await ReadData<Gender>("Gender");
                await context.genders.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
              if (!await context.models.AnyAsync())
            {
                var data = await ReadData<Model>("Model");
                await context.models.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
             if (!await context.carStatuses.AnyAsync())
            {
                var data = await ReadData<CarStatus>("CarStatus");
                await context.carStatuses.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
              if (!await context.colors.AnyAsync())
            {
                var data = await ReadData<Color>("Color");
                await context.colors.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
            if (!await context.brandNames.AnyAsync())
            {
                var data = await ReadData<BrandName>("BrandName");
                await context.brandNames.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
             if (!await context.carTypes.AnyAsync())
            {
                var data = await ReadData<CarType>("CarType");
                await context.carTypes.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
             if (!await context.providerTypes.AnyAsync())
            {
                var data = await ReadData<ProviderType>("ProviderType");
                await context.providerTypes.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }

            if (!await context.termsAndConditions.AnyAsync())
            {
                var data = await ReadData<TermsAndConditions>("TermsAndConditions");
                await context.termsAndConditions.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }

            if (!await context.aboutUs.AnyAsync())
            {
                var data = await ReadData<AboutUs>("AboutUs");
                await context.aboutUs.AddRangeAsync(data);
                await context.SaveChangesAsync();
            }
            // if (!await context.Provider.AnyAsync())
            // {
            //     var data = await ReadData<AppUser>("Provider");
            //     await context.Users.AddRangeAsync(data);
            //     await context.SaveChangesAsync();
            // }
        }
        
        private static async Task<List<T>> ReadData<T>(string file)
        {
            var fileData = await System.IO.File.ReadAllTextAsync("Data/SeedData/" + file + ".json");
            var data = JsonSerializer.Deserialize<List<T>>(fileData);
            return data;
        }
    }



}










