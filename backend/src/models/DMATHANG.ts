import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DMATHANG extends Model {
    public note!: any;
    public name!: any;
    public code!: any;
    public gianhap!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public giaban!: any;
    public giaban2!: any;
    public giaban3!: any;
    public giaban4!: any;
    public tontoithieu!: any;
    public tontoida!: any;
    public baohanh!: any;
    public masanco!: any;
    public dhangsanxuatid!: any;
    public anh!: any;
    public hoahong!: any;
    public giavon!: any;
    public ddonvitinhchanid!: any;
    public giabanchan!: any;
    public ddoitackyguiid!: any;
    public macdinhgiamgia!: any;
    public macdinhgiamtien!: any;
    public dnhommathangid!: any;
    public ddonvitinhid!: any;
    public quydoi!: any;
    public theocan!: any;
    public cohansudung!: any;
    public size!: any;
    public tentienganh!: any;
    public dloaimathangid!: any;
    public tamkhoa!: any;
    public giatheothoigia!: any;
    public mausac!: any;
}

DMATHANG.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
    },
    CODE: {
        type: DataTypes.STRING,
        
    },
    GIANHAP: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
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
    GIABAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIABAN2: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIABAN3: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIABAN4: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TONTOITHIEU: {
        type: DataTypes.STRING,
        
    },
    TONTOIDA: {
        type: DataTypes.STRING,
        
    },
    BAOHANH: {
        type: DataTypes.STRING,
        
    },
    MASANCO: {
        type: DataTypes.STRING,
        
    },
    DHANGSANXUATID: {
        type: DataTypes.INTEGER,
        
    },
    ANH: {
        type: DataTypes.BLOB,
        
    },
    HOAHONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    GIAVON: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DDONVITINHCHANID: {
        type: DataTypes.INTEGER,
        
    },
    GIABANCHAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DDOITACKYGUIID: {
        type: DataTypes.INTEGER,
        
    },
    MACDINHGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    MACDINHGIAMTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DNHOMMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DDONVITINHID: {
        type: DataTypes.INTEGER,
        
    },
    QUYDOI: {
        type: DataTypes.STRING,
        
    },
    THEOCAN: {
        type: DataTypes.STRING,
        
    },
    COHANSUDUNG: {
        type: DataTypes.STRING,
        
    },
    SIZE: {
        type: DataTypes.STRING,
        
    },
    TENTIENGANH: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DLOAIMATHANGID: {
        type: DataTypes.INTEGER,
        
    },
    TAMKHOA: {
        type: DataTypes.STRING,
        
    },
    GIATHEOTHOIGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    MAUSAC: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DMATHANG',
    tableName: 'DMATHANG',
    timestamps: false
});

export default DMATHANG;
