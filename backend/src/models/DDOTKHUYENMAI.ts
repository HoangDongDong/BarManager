import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DDOTKHUYENMAI extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dloaihinhkhuyenmaiid!: any;
    public tungay!: any;
    public denngay!: any;
    public ngungapdung!: any;
    public tilegiamgia!: any;
    public tilegiamgiatiengio!: any;
    public khuyenmaigiohat!: any;
    public tilegiamgiatong!: any;
    public tugio!: any;
    public dengio!: any;
    public tilegiamgiagiodau!: any;
}

DDOTKHUYENMAI.init({
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DLOAIHINHKHUYENMAIID: {
        type: DataTypes.INTEGER,
        
    },
    TUNGAY: {
        type: DataTypes.DATE,
        
    },
    DENNGAY: {
        type: DataTypes.DATE,
        
    },
    NGUNGAPDUNG: {
        type: DataTypes.STRING,
        
    },
    TILEGIAMGIA: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TILEGIAMGIATIENGIO: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    KHUYENMAIGIOHAT: {
        type: DataTypes.DATE,
        
    },
    TILEGIAMGIATONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    TUGIO: {
        type: DataTypes.DATE,
        
    },
    DENGIO: {
        type: DataTypes.DATE,
        
    },
    TILEGIAMGIAGIODAU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DDOTKHUYENMAI',
    tableName: 'DDOTKHUYENMAI',
    timestamps: false
});

export default DDOTKHUYENMAI;
