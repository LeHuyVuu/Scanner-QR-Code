using ScannerQRCode.Entities;
using ScannerQRCode.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerQRCode.Services
{
    
    public class ProductService
    {
        ProductRepository ProductRepository = new();
        public void AddProduct(Product product) 
        { 
             ProductRepository.Add(product);
        }

        public Product? GetProductByQRContent(string qrContent)
        {
            return ProductRepository.GetProduct(qrContent);
        }
    }
}
