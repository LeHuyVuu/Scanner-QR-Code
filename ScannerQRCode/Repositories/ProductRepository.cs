using ScannerQRCode.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerQRCode.Repositories
{
   
    public class ProductRepository
    {
        private QRContext _context = new QRContext();

        public void Add(Product product)
        {
            _context.Products.Add(product); 
            _context.SaveChanges();
        }

        public Product? GetProduct(string qrContent)
        {
         return _context.Products.FirstOrDefault(product => product.Id == qrContent);
        }
    }
}
