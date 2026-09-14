using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Dapper;
using QuanLyBar.Client.Models;
using QuanLyBar.Client.Services;

namespace QuanLyBar.Client.Views.TouchPOS
{
    public partial class TouchDieuChinhMatHangWindow : Window
    {
        public TouchCartItemVM Item { get; private set; }

        public TouchDieuChinhMatHangWindow(TouchCartItemVM item)
        {
            InitializeComponent();
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                TxtTitle.Text = $"ĐIỀU CHỈNH MẶT HÀNG: {(Item.TenMatHang ?? "").ToUpper()}";
                UpdateDisplayInfo();
            }
        }

        private void UpdateDisplayInfo()
        {
            if (Item != null)
            {
                TxtBanDau.Text = $"Số lượng ban đầu: {Item.SoLuong:0.#}";
            }
        }

        private void BtnPlusNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && decimal.TryParse(btn.Tag.ToString(), out decimal step))
            {
                Item.SoLuong += step;
                UpdateDisplayInfo();
            }
        }

        private void BtnMinusNumber_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null && decimal.TryParse(btn.Tag.ToString(), out decimal step))
            {
                if (Item.SoLuong > step)
                {
                    Item.SoLuong -= step;
                }
                else
                {
                    Item.SoLuong = 1;
                }
                UpdateDisplayInfo();
            }
        }

        private void BtnPlusKhac_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchNhapSoLuongWindow(Item.TenMatHang ?? "món", 1)
            {
                Owner = this
            };
            if (win.ShowDialog() == true && win.NewQuantity > 0)
            {
                Item.SoLuong += win.NewQuantity;
                UpdateDisplayInfo();
            }
        }

        private void BtnMinusKhac_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchNhapSoLuongWindow(Item.TenMatHang ?? "món", 1)
            {
                Owner = this
            };
            if (win.ShowDialog() == true && win.NewQuantity > 0)
            {
                if (Item.SoLuong > win.NewQuantity)
                {
                    Item.SoLuong -= win.NewQuantity;
                }
                else
                {
                    Item.SoLuong = 1;
                }
                UpdateDisplayInfo();
            }
        }

        private void BtnDoiGia_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchNhapSoLuongWindow($"Đổi giá cho: {Item.TenMatHang}", Item.DonGia)
            {
                Owner = this
            };
            if (win.ShowDialog() == true && win.NewQuantity >= 0)
            {
                Item.DonGia = win.NewQuantity;
                UpdateDisplayInfo();
            }
        }

        private async void BtnChuyenHoaDon_Click(object sender, RoutedEventArgs e)
        {
            TouchMainWindow? mainWin = Owner as TouchMainWindow;
            DBAN? currentBan = mainWin?.CurrentBan;

            if (currentBan == null || !currentBan.IsOpened || string.IsNullOrEmpty(currentBan.ActiveOrderId))
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "MỜI BẠN CHỌN BÀN ĐANG CÓ HÓA ĐƠN ĐỂ CHUYỂN", "CẢNH BÁO");
                return;
            }

            var chuyenMonWin = new TouchChuyenMonWindow(Item)
            {
                Owner = this
            };

            if (chuyenMonWin.ShowDialog() == true && chuyenMonWin.SelectedQuantity > 0)
            {
                decimal transferQty = chuyenMonWin.SelectedQuantity;

                var chonBanWin = new ChonBanChuyenGopTouchWindow(currentBan, isMergeMode: false, isMoveItemMode: true)
                {
                    Owner = this
                };

                if (chonBanWin.ShowDialog() == true && chonBanWin.SelectedTargetBan != null)
                {
                    var targetBan = chonBanWin.SelectedTargetBan;
                    await PerformItemTransferAsync(mainWin, currentBan, targetBan, Item, transferQty);

                    DialogResult = true;
                    Close();
                }
            }
        }

        private async Task PerformItemTransferAsync(TouchMainWindow? mainWin, DBAN sourceBan, DBAN targetBan, TouchCartItemVM item, decimal transferQty)
        {
            try
            {
                var service = new LocalSuDungDichVuService();
                string sourceOrderId = sourceBan.ActiveOrderId;

                // 1. Target order ID (must be an active order)
                string targetOrderId = targetBan.ActiveOrderId ?? "";
                if (!targetBan.IsOpened || string.IsNullOrEmpty(targetOrderId))
                {
                    QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, "MỜI BẠN CHỌN BÀN ĐANG CÓ HÓA ĐƠN ĐỂ CHUYỂN", "CẢNH BÁO");
                    return;
                }

                // 2. Load order details
                var sourceDetails = (await service.GetOrderDetailsAsync(sourceOrderId))?.ToList() ?? new List<PosDonHangChiTietViewModel>();
                var targetDetails = (await service.GetOrderDetailsAsync(targetOrderId))?.ToList() ?? new List<PosDonHangChiTietViewModel>();

                // 3. Find and update source item
                var sItem = sourceDetails.FirstOrDefault(x => x.MatHangId == item.MAMATHANG || x.MatHangName == item.TenMatHang);
                if (sItem != null)
                {
                    if (transferQty >= sItem.SoLuong)
                    {
                        sourceDetails.Remove(sItem);
                    }
                    else
                    {
                        sItem.SoLuong -= transferQty;
                        sItem.Recalculate();
                    }
                }

                // 4. Find and update target item
                var tItem = targetDetails.FirstOrDefault(x => x.MatHangId == (sItem?.MatHangId ?? item.MAMATHANG));
                if (tItem != null)
                {
                    tItem.SoLuong += transferQty;
                    tItem.Recalculate();
                }
                else
                {
                    targetDetails.Add(new PosDonHangChiTietViewModel
                    {
                        Id = Guid.NewGuid().ToString("N").Substring(0, 20),
                        MatHangId = sItem?.MatHangId ?? item.MAMATHANG,
                        MatHangName = sItem?.MatHangName ?? item.TenMatHang,
                        DonViTinh = sItem?.DonViTinh ?? "",
                        DonGiaGoc = item.DonGiaGoc,
                        DonGia = item.DonGia,
                        SoLuong = transferQty,
                        ThanhTien = transferQty * item.DonGia,
                        ChietKhauPhanTram = item.ChietKhauPhanTram,
                        GhiChu = item.GhiChu
                    });
                }

                // 5. Save source order
                decimal sourceTienHang = sourceDetails.Sum(x => x.ThanhTien);
                if (sourceDetails.Count > 0)
                {
                    await service.SaveOrderAsync(sourceOrderId, sourceDetails, sourceTienHang, 0, sourceTienHang, "", sourceBan.SoKhach > 0 ? sourceBan.SoKhach : 1);
                }
                else
                {
                    await service.DeleteOrderAsync(sourceOrderId);
                    try
                    {
                        using var conn = DbConnectionManager.GetConnection();
                        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                        await conn.ExecuteAsync("UPDATE DBAN SET ISOPENED = 0, ACTIVEORDERID = NULL WHERE CAST(ID AS VARCHAR(50)) = @BanId", new { BanId = sourceBan.Id });
                    }
                    catch { }

                    sourceBan.IsOpened = false;
                    sourceBan.ActiveOrderId = null;
                    sourceBan.SoPhieu = "";
                    sourceBan.TrangThai = "Trống";
                }

                // 6. Save target order
                decimal targetTienHang = targetDetails.Sum(x => x.ThanhTien);
                await service.SaveOrderAsync(targetOrderId, targetDetails, targetTienHang, 0, targetTienHang, "", targetBan.SoKhach > 0 ? targetBan.SoKhach : 1);

                // 7. Show success alert and refresh main window
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"ĐÃ CHUYỂN {transferQty:0.#} '{item.TenMatHang}' SANG BÀN '{targetBan.Name.ToUpper()}' THÀNH CÔNG!", "THÔNG BÁO");

                if (mainWin != null)
                {
                    mainWin.ReloadCurrentOrder();
                }
            }
            catch (Exception ex)
            {
                QuanLyBar.Views.TouchPOS.TouchConfirmWindow.ShowAlert(this, $"LỖI CHUYỂN MÓN: {ex.Message}", "LỖI");
            }
        }

        private void BtnDatSoLuong_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchNhapSoLuongWindow(Item.TenMatHang ?? "món", Item.SoLuong)
            {
                Owner = this
            };
            if (win.ShowDialog() == true && win.NewQuantity > 0)
            {
                Item.SoLuong = win.NewQuantity;
                UpdateDisplayInfo();
            }
        }

        private void BtnSuaGhiChu_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchKeyboardWindow(Item.GhiChu ?? "", false, $"SỬA GHI CHÚ MÓN: {Item.TenMatHang}")
            {
                Owner = this
            };
            if (win.ShowDialog() == true)
            {
                Item.GhiChu = win.ResultText;
                UpdateDisplayInfo();
            }
        }

        private void BtnGiamGia_Click(object sender, RoutedEventArgs e)
        {
            var win = new TouchNhapSoLuongWindow($"Giảm giá % cho {Item.TenMatHang}", Item.ChietKhauPhanTram)
            {
                Owner = this
            };
            if (win.ShowDialog() == true && win.NewQuantity >= 0 && win.NewQuantity <= 100)
            {
                Item.ChietKhauPhanTram = win.NewQuantity;
                UpdateDisplayInfo();
            }
        }

        private void BtnHuyBo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
