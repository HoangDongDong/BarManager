; Script Inno Setup cho phần mềm Quản Lý Bar, Nhà Hàng - Tân An Phát

#define MyAppName "QUẢN LÝ BAR, NHÀ HÀNG"
#define MyAppVersion "6.0.0.0"
#define MyAppPublisher "TÂN AN PHÁT"
#define MyAppURL "https://tananphat.com"
#define MyAppExeName "QuanLyBar.Client.exe"
#define TouchExeName "QuanLyBar.TouchPOS.exe"

[Setup]
AppId={{D37E88A1-4B9F-49E8-99D8-1429F03B2A1E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userdesktop}\QuanLyBar
UsePreviousAppDir=no
DisableDirPage=no
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=D:\QuanLyBar\Installer_Output
OutputBaseFilename=QuanLyBar_v6.0_Setup
Compression=lzma2/ultra64
SolidCompression=yes
SetupIconFile=D:\QuanLyBar\frontend\img\Icon.ico
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
english.WelcomeLabel1=Chào mừng tới trình cài đặt %1
english.WelcomeLabel2=Chương trình sẽ cài %1 phiên bản %2 trên máy tính của bạn.%n%nChúng tôi khuyên bạn đóng mọi chương trình khác lại trước khi cài đặt.%n%nNhấn Tiếp để tiếp tục, hoặc Hủy để thoát cài đặt.
english.SelectDirLabel3=Chương trình sẽ cài đặt %1 vào thư mục sau:
english.SelectDirBrowseLabel=Để tiếp tục, nhấn Tiếp. Nếu bạn muốn chọn một thư mục khác, nhấn Duyệt.

[Tasks]
Name: "desktopicon_client"; Description: "Tạo biểu tượng Quản Lý Bar, Nhà Hàng"; GroupDescription: "Biểu tượng Desktop:"
Name: "desktopicon_touch"; Description: "Tạo biểu tượng POS Cảm Ứng Bán Hàng"; GroupDescription: "Biểu tượng Desktop:"

[Files]
Source: "D:\QuanLyBar\Dist_Client\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\QuanLyBar\Dist_TouchPOS\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autodesktop}\Quản Lý Bar, Nhà Hàng"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon_client
Name: "{autodesktop}\POS Cảm Ứng Bán Hàng"; Filename: "{app}\{#TouchExeName}"; Tasks: desktopicon_touch
Name: "{group}\Quản Lý Bar, Nhà Hàng"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\POS Cảm Ứng Bán Hàng"; Filename: "{app}\{#TouchExeName}"
Name: "{group}\Gỡ bỏ ứng dụng"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Khởi chạy Quản Lý Bar, Nhà Hàng"; Flags: postinstall nowait skipifsilent
