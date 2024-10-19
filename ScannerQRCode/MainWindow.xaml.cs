using Emgu.CV;
using Emgu.CV.Structure;
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

        public MainWindow()
        {
            InitializeComponent();
            _barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader();
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
                                // Nếu nội dung không phải là URL, lưu vào cơ sở dữ liệu
                                SaveQRCode(qrContent);
                                MessageBox.Show($"Nội dung mã QR được quét: {qrContent}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // Lưu mã QR vào cơ sở dữ liệu
        private void SaveQRCode(string qrCodeText)
        {
            using (var context = new QRContext())
            {
                var qrCodeScan = new Entities.QRCodeScan
                {
                    QRCodeText = qrCodeText,
                    ScanTime = DateTime.Now
                };
                context.QRCodeScans.Add(qrCodeScan);
                context.SaveChanges();
            }
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Mã QR quét được: {qrCodeText}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // Hàm để test quét mã vạch từ hình ảnh
        private void btnTestQRCode_Click(object sender, RoutedEventArgs e)
        {
            // Đường dẫn đến hình ảnh mã vạch
            string filePath = @"C:\Users\Dell\Desktop\barcode_image.png"; // Đặt đường dẫn đúng đến hình ảnh của bạn
            TestDecodeFromFile(filePath);
        }

        private void TestDecodeFromFile(string filePath)
        {
            // Tải hình ảnh từ tệp
            var bitmap = (Bitmap)Image.FromFile(filePath);

            // Xử lý QR code
            var result = _barcodeReader.Decode(bitmap);
            if (result != null)
            {
                string qrContent = result.Text;

                // Kiểm tra nếu nội dung là một liên kết URL
                if (Uri.IsWellFormedUriString(qrContent, UriKind.Absolute))
                {
                    MessageBox.Show($"Liên kết URL được quét: {qrContent}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = qrContent,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Nếu không phải URL, lưu vào cơ sở dữ liệu
                    using (var context = new QRContext())
                    {
                        var qrCodeScan = new Entities.QRCodeScan
                        {
                            QRCodeText = qrContent,
                            ScanTime = DateTime.Now
                        };
                        context.QRCodeScans.Add(qrCodeScan);
                        context.SaveChanges();
                    }
                    MessageBox.Show($"Nội dung mã QR được quét: {qrContent}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Không thể quét mã vạch!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
