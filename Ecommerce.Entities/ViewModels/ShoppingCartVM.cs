using Ecommerce.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Entities.ViewModels
{
	public class ShoppingCartVM
	{
        public IEnumerable<ShoppingCart> CartsList { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
