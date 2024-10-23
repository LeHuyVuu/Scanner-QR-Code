using Emgu.CV;
using Emgu.CV.Structure;
using ScannerQRCode;
using ScannerQRCode.Entities;
using ScannerQRCode.Services;
using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ZXing.Windows.Compatibility;

namespace BT3_Day2
{
    public partial class MainWindow : Window
    {
        private VideoCapture _capture;
        private DispatcherTimer _timer;
        private BarcodeReader _barcodeReader;
        private QRContext QRContext;
        private ProductService ProductService = new();

        public MainWindow()
        {
            InitializeComponent();
            // Cấu hình BarcodeReader để quét mã QR và mã vạch
            _barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new List<ZXing.BarcodeFormat>
                    {
                        ZXing.BarcodeFormat.QR_CODE,
                        ZXing.BarcodeFormat.CODE_128,  // Quét mã vạch 128
                        ZXing.BarcodeFormat.CODE_39,   // Quét mã vạch 39
                        ZXing.BarcodeFormat.EAN_13,    // Quét mã EAN 13
                        ZXing.BarcodeFormat.UPC_A,     // Quét mã UPC
                      
                    },
                    TryHarder = true
                }
            };
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _capture = new VideoCapture();
            _capture.ImageGrabbed += ProcessFrame;
            _capture.Start();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += (s, args) => { ProcessFrame(null, null); };
            _timer.Start();
        }



        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                using (var frame = _capture.QueryFrame().ToImage<Bgr, byte>())
                {
                    // Chuyển đổi hình ảnh trong luồng UI
                    Dispatcher.Invoke(() =>
                    {
                        var bitmap = ConvertToBitmap(frame);
                        var bitmapImage = BitmapToBitmapImage(bitmap);

                        // Cập nhật giao diện người dùng từ luồng UI
                        imgWebcam.Source = bitmapImage;

                        // Xử lý QR code
                        var result = _barcodeReader.Decode(bitmap);
                        if (result != null)
                        {
                         
                            string qrContent = result.Text;


                            // Kiểm tra nếu nội dung là một liên kết URL
                            if (Uri.IsWellFormedUriString(qrContent, UriKind.Absolute))
                            {
                                MessageBox.Show($"Liên kết URL được quét: {qrContent}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                                // Mở liên kết bằng trình duyệt
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = qrContent,
                                    UseShellExecute = true
                                });
                            }
                            else
                            {
                                Product? product = ProductService.GetProductByQRContent(qrContent);
                                // Nếu nội dung không phải là URL, lưu vào cơ sở dữ liệu
                                if (product != null)
                                {
                                    SaveQRCode(qrContent);
                                    MessageBox.Show($@"Thông tin sản phẩm:
- Tên sản phẩm: {product.ProductName}
- Giá: {product.Price:C}
- Số lượng: {product.Quantity}",
                                    "Thông báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                                }

                                else
                                {
                                    SaveQRCode(qrContent);
                                    MessageBox.Show($@"The QR code has been scanned successfully. Content:{qrContent}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                                }

                            }

                        }
                    });
                }
            }
        }

        private Bitmap ConvertToBitmap(Image<Bgr, byte> image)
        {
            // Đổi Image<Bgr, byte> thành Bitmap
            return image.ToBitmap();
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        private QRCodeService QRCodeService = new QRCodeService();
        // Lưu mã QR vào cơ sở dữ liệu
        private void SaveQRCode(string qrCodeText)
        {
            using (var context = new QRContext())
            {
                var qrCodeScan = new ScannerQRCode.Entities.QRCodeScan
                {
                    QRCodeText = qrCodeText,
                    ScanTime = DateTime.Now
                };
                QRCodeService.AddQRCode(qrCodeScan);
                context.SaveChanges();
            }
        }

        private void Button_Generate_Click(object sender, RoutedEventArgs e)
        {
            ProductApp productApp = new ProductApp();
            productApp.Show();
        }
    }
}
