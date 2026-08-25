import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TDONHANGCHITIET extends Model {
    public id!: any;
    public note!: any;
    public tdonhangid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dmathangid!: any;
    public baohanh!: any;
    public tilegiamgia!: any;
    public thanhtien!: any;
    public dongia!: any;
    public slnhap!: any;
    public slxuat!: any;
    public slthucte!: any;
    public slhethong!: any;
    public dkhohangid!: any;
    public khuyenmai!: any;
    public ddonvitinhid!: any;
    public slnhapchuaquydoi!: any;
    public slxuatchuaquydoi!: any;
    public giamtheotien!: any;
    public tiengiamgia!: any;
    public tdonhangtraid!: any;
    public xuatvattu!: any;
    public kichthuoc!: any;
    public hansudung!: any;
    public dnhanvien1id!: any;
    public dnhanvien2id!: any;
    public dnhanvien3id!: any;
    public tenhang!: any;
    public tugio!: any;
    public dengio!: any;
    public giavon!: any;
    public comboid!: any;
    public comboparentid!: any;
    public combosl!: any;
    public dtrangthaichebienid!: any;
    public giotinhluong!: any;
}

TDONHANGCHITIET.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    NOTE: {
        type: DataTypes.STRING,
        
    },
    TDONHANGID: {
        type: DataTypes.INTEGER,
        
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    BAOHANH: {
        type: DataTypes.STRING,
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    THANHTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DONGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SLNHAP: {
        type: DataTypes.STRING,
        
    },
    SLXUAT: {
        type: DataTypes.STRING,
        
    },
    SLTHUCTE: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    SLHETHONG: {
        type: DataTypes.STRING,
        
    },
    DKHOHANGID: {
        type: DataTypes.INTEGER,
        
    },
    KHUYENMAI: {
        type: DataTypes.STRING,
        
    },
    DDONVITINHID: {
        type: DataTypes.INTEGER,
        
    },
    SLNHAPCHUAQUYDOI: {
        type: DataTypes.STRING,
        
    },
    SLXUATCHUAQUYDOI: {
        type: DataTypes.STRING,
        
    },
    GIAMTHEOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TIENGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TDONHANGTRAID: {
        type: DataTypes.INTEGER,
        
    },
    XUATVATTU: {
        type: DataTypes.STRING,
        
    },
    KICHTHUOC: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    HANSUDUNG: {
        type: DataTypes.DATE,
        
    },
    DNHANVIEN1ID: {
        type: DataTypes.INTEGER,
        
    },
    DNHANVIEN2ID: {
        type: DataTypes.INTEGER,
        
    },
    DNHANVIEN3ID: {
        type: DataTypes.INTEGER,
        
    },
    TENHANG: {
        type: DataTypes.STRING,
        
    },
    TUGIO: {
        type: DataTypes.DATE,
        
    },
    DENGIO: {
        type: DataTypes.DATE,
        
    },
    GIAVON: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    COMBOID: {
        type: DataTypes.INTEGER,
        
    },
    COMBOPARENTID: {
        type: DataTypes.INTEGER,
        
    },
    COMBOSL: {
        type: DataTypes.STRING,
        
    },
    DTRANGTHAICHEBIENID: {
        type: DataTypes.INTEGER,
        
    },
    GIOTINHLUONG: {
        type: DataTypes.DATE,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TDONHANGCHITIET',
    tableName: 'TDONHANGCHITIET',
    timestamps: false
});

export default TDONHANGCHITIET;
