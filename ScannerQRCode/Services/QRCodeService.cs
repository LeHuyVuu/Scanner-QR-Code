using ScannerQRCode.Entities;
using ScannerQRCode.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerQRCode.Services
{
   
    public class QRCodeService
    {
        private QRCodeRepository QRcodeRepository = new QRCodeRepository();

        public void AddQRCode(QRCodeScan qRCodeScan)
        {
            QRcodeRepository.Add(qRCodeScan);
        }

    }
}
