using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuanLyBar.Client.Services
{
    /// <summary>
    /// Tiện ích xử lý tự động thay đổi kích thước (Auto Resize) và nén ảnh (Image Compression)
    /// Giúp tối ưu hóa dung lượng lưu trữ trong Database (Firebird BLOB), tiết kiệm băng thông và đồng nhất giao diện.
    /// </summary>
    public static class ImageHelper
    {
        public const int DefaultMaxWidth = 500;
        public const int DefaultMaxHeight = 500;
        public const int DefaultJpegQuality = 80;

        /// <summary>
        /// Tối ưu hóa ảnh từ mảng byte: Auto Resize theo tỷ lệ và Nén thông minh (JPEG chất lượng cao hoặc PNG trong suốt)
        /// </summary>
        public static byte[] OptimizeImage(byte[]? rawBytes, int maxWidth = DefaultMaxWidth, int maxHeight = DefaultMaxHeight, int jpegQuality = DefaultJpegQuality)
        {
            if (rawBytes == null || rawBytes.Length == 0)
                return Array.Empty<byte>();

            try
            {
                using var inStream = new MemoryStream(rawBytes);
                var decoder = BitmapDecoder.Create(inStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                if (decoder.Frames.Count == 0)
                    return rawBytes;

                var frame = decoder.Frames[0];

                // 1. Kiểm tra góc xoay EXIF (thường gặp khi chụp từ điện thoại)
                int rotationAngle = GetExifRotation(frame);

                int origW = (rotationAngle == 90 || rotationAngle == 270) ? frame.PixelHeight : frame.PixelWidth;
                int origH = (rotationAngle == 90 || rotationAngle == 270) ? frame.PixelWidth : frame.PixelHeight;

                if (origW <= 0 || origH <= 0)
                    return rawBytes;

                // 2. Tính toán tỷ lệ co dãn (Scale)
                double scale = 1.0;
                if (origW > maxWidth || origH > maxHeight)
                {
                    double scaleX = (double)maxWidth / origW;
                    double scaleY = (double)maxHeight / origH;
                    scale = Math.Min(scaleX, scaleY);
                }

                // Nếu ảnh đã nhỏ hơn giới hạn, không cần xoay, và dung lượng đã nhỏ (< 80KB)
                if (scale >= 1.0 && rotationAngle == 0 && rawBytes.Length <= 80 * 1024)
                {
                    return rawBytes;
                }

                // 3. Thực hiện xoay và resize
                BitmapSource sourceToEncode = frame;
                if (rotationAngle != 0 || scale < 1.0)
                {
                    var group = new TransformGroup();
                    if (rotationAngle != 0)
                    {
                        group.Children.Add(new RotateTransform(rotationAngle));
                    }
                    if (scale < 1.0)
                    {
                        group.Children.Add(new ScaleTransform(scale, scale));
                    }

                    var transformed = new TransformedBitmap();
                    transformed.BeginInit();
                    transformed.Source = frame;
                    transformed.Transform = group;
                    transformed.EndInit();
                    transformed.Freeze();

                    sourceToEncode = transformed;
                }

                // 4. Kiểm tra xem ảnh có chứa độ trong suốt (Alpha transparency) hay không
                bool hasAlpha = CheckHasTransparency(sourceToEncode);

                // 5. Nén ảnh: Nếu có trong suốt thì nén PNG để giữ nền trong suốt, ngược lại nén JPEG chất lượng cao
                BitmapEncoder encoder;
                if (hasAlpha)
                {
                    encoder = new PngBitmapEncoder();
                }
                else
                {
                    encoder = new JpegBitmapEncoder
                    {
                        QualityLevel = Math.Clamp(jpegQuality, 10, 100)
                    };
                }

                encoder.Frames.Add(BitmapFrame.Create(sourceToEncode));

                using var outStream = new MemoryStream();
                encoder.Save(outStream);
                byte[] compressedBytes = outStream.ToArray();

                // Nếu kết quả nén lại lớn hơn ảnh gốc (trường hợp ảnh gốc đã rất tối ưu)
                if (compressedBytes.Length >= rawBytes.Length && scale >= 1.0 && rotationAngle == 0)
                {
                    return rawBytes;
                }

                return compressedBytes;
            }
            catch
            {
                // Nếu có lỗi giải mã (file đặc biệt), giữ nguyên dữ liệu gốc
                return rawBytes;
            }
        }

        /// <summary>
        /// Đọc file và tối ưu hóa ảnh
        /// </summary>
        public static byte[] OptimizeImageFromFile(string filePath, int maxWidth = DefaultMaxWidth, int maxHeight = DefaultMaxHeight, int jpegQuality = DefaultJpegQuality)
        {
            if (!File.Exists(filePath))
                return Array.Empty<byte>();

            byte[] rawBytes = File.ReadAllBytes(filePath);
            return OptimizeImage(rawBytes, maxWidth, maxHeight, jpegQuality);
        }

        /// <summary>
        /// Tạo BitmapImage từ byte[] an toàn cho hiển thị trên giao diện WPF
        /// </summary>
        public static BitmapImage? BytesToBitmapImage(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return null;

            try
            {
                using var ms = new MemoryStream(bytes);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Lấy góc xoay từ EXIF metadata của ảnh
        /// </summary>
        private static int GetExifRotation(BitmapFrame frame)
        {
            try
            {
                if (frame.Metadata is BitmapMetadata metadata && metadata.ContainsQuery("/app1/ifd/{ushort=274}"))
                {
                    var val = metadata.GetQuery("/app1/ifd/{ushort=274}");
                    if (val != null)
                    {
                        ushort orientation = Convert.ToUInt16(val);
                        return orientation switch
                        {
                            3 => 180,
                            6 => 90,
                            8 => 270,
                            _ => 0
                        };
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi đọc metadata
            }

            return 0;
        }

        /// <summary>
        /// Kiểm tra nhanh ảnh có pixel trong suốt hay không
        /// </summary>
        private static bool CheckHasTransparency(BitmapSource bitmap)
        {
            try
            {
                // Các định dạng chắc chắn không có kênh alpha
                if (bitmap.Format == PixelFormats.Bgr24 ||
                    bitmap.Format == PixelFormats.Rgb24 ||
                    bitmap.Format == PixelFormats.Bgr32 ||
                    bitmap.Format == PixelFormats.Gray8 ||
                    bitmap.Format == PixelFormats.Gray16 ||
                    bitmap.Format == PixelFormats.BlackWhite)
                {
                    return false;
                }

                // Nếu là định dạng có hỗ trợ alpha
                BitmapSource testBitmap = bitmap;
                if (bitmap.Format != PixelFormats.Bgra32 && bitmap.Format != PixelFormats.Pbgra32)
                {
                    if (bitmap.Format.BitsPerPixel < 32)
                        return false;

                    testBitmap = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
                }

                int w = testBitmap.PixelWidth;
                int h = testBitmap.PixelHeight;
                int stride = w * 4;
                byte[] pixels = new byte[h * stride];
                testBitmap.CopyPixels(pixels, stride, 0);

                // Quét mẫu pixel tìm kênh alpha < 250 (có độ trong suốt)
                // Bước nhảy pixel để quét nhanh các ảnh kích thước lớn
                int step = (pixels.Length > 100000) ? 8 : 4;
                for (int i = 3; i < pixels.Length; i += step)
                {
                    if (pixels[i] < 250)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Mặc định an toàn
            }

            return false;
        }
    }
}
