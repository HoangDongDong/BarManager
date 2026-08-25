import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDONHANG extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public giamtheotien!: any;
    public loai!: any;
    public tongcong!: any;
    public phivanchuyen!: any;
    public tiengiamgia!: any;
    public tilegiamgia!: any;
    public tienthue!: any;
    public tilethue!: any;
    public tienhang!: any;
    public dnhacungcapid!: any;
    public dkhoxuatid!: any;
    public dkhonhapid!: any;
    public dnhanvienxuatid!: any;
    public dnhanviennhapid!: any;
    public diengiai!: any;
    public giothanhtoan!: any;
    public khachdua!: any;
    public tralai!: any;
    public nocu!: any;
    public loaigia!: any;
    public userthanhtoanid!: any;
    public tienthanhtoan!: any;
    public loaithanhtoan!: any;
    public tdathangid!: any;
    public giaohang!: any;
    public dathanhtoan!: any;
    public doitra!: any;
    public conno!: any;
    public diem!: any;
    public voucher!: any;
    public dnhanviengiaoid!: any;
    public trichnhanvien!: any;
    public dcuahangid!: any;
    public conlai!: any;
    public thanhtoan!: any;
    public dkhachhangid!: any;
    public dtaikhoannganhangid!: any;
    public dvoucherid!: any;
    public thetratruoc!: any;
    public dthetratruocid!: any;
    public trutichluy!: any;
    public diemgiam!: any;
    public tienmat!: any;
    public chuyenkhoan!: any;
    public the!: any;
    public dbanid!: any;
    public batdau!: any;
    public ketthuc!: any;
    public nhomguid!: any;
    public tiengio!: any;
    public tilegiamgiagio!: any;
    public tiengiamgiagio!: any;
    public sokhach!: any;
    public phidichvu!: any;
    public tilephidichvu!: any;
    public tilegiamgiatong!: any;
    public tiengiamgiatong!: any;
    public giamgiagiotheotien!: any;
    public phidichvutheotien!: any;
    public giamtongtheotien!: any;
    public soorder!: any;
    public tuthaydoigio!: any;
    public sohd!: any;
    public sott!: any;
    public solanintamtinh!: any;
    public dongia!: any;
    public dbanggiaid!: any;
    public tiengiophongcuoi!: any;
    public batdauphongcuoi!: any;
    public cachtinhgia!: any;
    public tienmoban!: any;
    public laninhoadon!: any;
    public intamtinhluc!: any;
    public dattruoc!: any;
    public congno!: any;
    public tienhangchuagiam!: any;
    public giamgiamathang!: any;
    public phutkhuyenmai!: any;
    public tilekhuyenmaiphutdau!: any;
    public passwifi!: any;
}

TDONHANG.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NAME: {
        type: DataTypes.STRING,
        
    },
    NOTE: {
        type: DataTypes.STRING,
        
    },
    STATUS: {
        type: DataTypes.BOOLEAN,
        
    },
    USERMODIFIEDID: {
        type: DataTypes.INTEGER,
        
    },
    TIMEMODIFIED: {
        type: DataTypes.DATE,
        
    },
    TIMECREATED: {
        type: DataTypes.DATE,
        
    },
    NGAY: {
        type: DataTypes.DATE,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    GIAMTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    LOAI: {
        type: DataTypes.STRING,
        
    },
    TONGCONG: {
        type: DataTypes.STRING,
        
    },
    PHIVANCHUYEN: {
        type: DataTypes.STRING,
        
    },
    TIENGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENTHUE: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILETHUE: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DNHACUNGCAPID: {
        type: DataTypes.INTEGER,
        
    },
    DKHOXUATID: {
        type: DataTypes.INTEGER,
        
    },
    DKHONHAPID: {
        type: DataTypes.INTEGER,
        
    },
    DNHANVIENXUATID: {
        type: DataTypes.INTEGER,
        
    },
    DNHANVIENNHAPID: {
        type: DataTypes.INTEGER,
        
    },
    DIENGIAI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIOTHANHTOAN: {
        type: DataTypes.DATE,
        
    },
    KHACHDUA: {
        type: DataTypes.STRING,
        
    },
    TRALAI: {
        type: DataTypes.STRING,
        
    },
    NOCU: {
        type: DataTypes.STRING,
        
    },
    LOAIGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    USERTHANHTOANID: {
        type: DataTypes.INTEGER,
        
    },
    TIENTHANHTOAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    LOAITHANHTOAN: {
        type: DataTypes.STRING,
        
    },
    TDATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    GIAOHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DATHANHTOAN: {
        type: DataTypes.STRING,
        
    },
    DOITRA: {
        type: DataTypes.STRING,
        
    },
    CONNO: {
        type: DataTypes.STRING,
        
    },
    DIEM: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    VOUCHER: {
        type: DataTypes.STRING,
        
    },
    DNHANVIENGIAOID: {
        type: DataTypes.INTEGER,
        
    },
    TRICHNHANVIEN: {
        type: DataTypes.STRING,
        
    },
    DCUAHANGID: {
        type: DataTypes.INTEGER,
        
    },
    CONLAI: {
        type: DataTypes.STRING,
        
    },
    THANHTOAN: {
        type: DataTypes.STRING,
        
    },
    DKHACHHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DTAIKHOANNGANHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DVOUCHERID: {
        type: DataTypes.INTEGER,
        
    },
    THETRATRUOC: {
        type: DataTypes.STRING,
        
    },
    DTHETRATRUOCID: {
        type: DataTypes.INTEGER,
        
    },
    TRUTICHLUY: {
        type: DataTypes.STRING,
        
    },
    DIEMGIAM: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENMAT: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    CHUYENKHOAN: {
        type: DataTypes.STRING,
        
    },
    THE: {
        type: DataTypes.STRING,
        
    },
    DBANID: {
        type: DataTypes.INTEGER,
        
    },
    BATDAU: {
        type: DataTypes.DATE,
        
    },
    KETTHUC: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    NHOMGUID: {
        type: DataTypes.INTEGER,
        
    },
    TIENGIO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIAGIO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENGIAMGIAGIO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SOKHACH: {
        type: DataTypes.STRING,
        
    },
    PHIDICHVU: {
        type: DataTypes.STRING,
        
    },
    TILEPHIDICHVU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIATONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENGIAMGIATONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIAMGIAGIOTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    PHIDICHVUTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIAMTONGTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SOORDER: {
        type: DataTypes.STRING,
        
    },
    TUTHAYDOIGIO: {
        type: DataTypes.DATE,
        
    },
    SOHD: {
        type: DataTypes.STRING,
        
    },
    SOTT: {
        type: DataTypes.STRING,
        
    },
    SOLANINTAMTINH: {
        type: DataTypes.STRING,
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
    TIENGIOPHONGCUOI: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    BATDAUPHONGCUOI: {
        type: DataTypes.STRING,
        
    },
    CACHTINHGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENMOBAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    LANINHOADON: {
        type: DataTypes.STRING,
        
    },
    INTAMTINHLUC: {
        type: DataTypes.STRING,
        
    },
    DATTRUOC: {
        type: DataTypes.STRING,
        
    },
    CONGNO: {
        type: DataTypes.STRING,
        
    },
    TIENHANGCHUAGIAM: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIAMGIAMATHANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    PHUTKHUYENMAI: {
        type: DataTypes.STRING,
        
    },
    TILEKHUYENMAIPHUTDAU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    PASSWIFI: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDONHANG',
    tableName: 'TDONHANG',
    timestamps: false
});

export default TDONHANG;
