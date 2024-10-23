using ScannerQRCode.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerQRCode.Repositories
{   
    public class QRCodeRepository 
    {
        private QRContext _context = new();

        public void Add(QRCodeScan qRCodeScan)
        {
            _context.QRCodeScans.Add(qRCodeScan);
            _context.SaveChanges();
        }
    }
}
