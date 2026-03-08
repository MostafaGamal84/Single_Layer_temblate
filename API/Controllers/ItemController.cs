using System.Threading.Tasks;
using API.DTOs.ItemDto;
using API.Entities.Auctions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork;

namespace API.Controllers
{
   
    public class ItemController : BaseGenericApiController<Item, ItemAddDto, ItemReturnDto>
    {

        public ItemController(IUnitOfWork uow) : base(uow)
        {
        }
    
        
    }
}