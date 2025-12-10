using Ecommerce.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.Repositories
{
    public interface IShoppingCartRepository:IGenericRepository<ShoppingCart>
    {
        int IncreaseCount(ShoppingCart shoppingcart, int count);
        int DecreaseCount(ShoppingCart shoppingcart, int count);
      
    }
}
