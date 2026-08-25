import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TVAORA extends Model {
    public note!: any;
    public tbangluongid!: any;
    public ngay!: any;
    public dnhanvienid!: any;
    public giovao!: any;
    public giora!: any;
    public sogio!: any;
    public sogiotangca!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public giobatdau!: any;
    public gioketthuc!: any;
}

TVAORA.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    TBANGLUONGID: {
        type: DataTypes.INTEGER,
        
    },
    NGAY: {
        type: DataTypes.DATE,
        
    },
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    GIOVAO: {
        type: DataTypes.DATE,
        
    },
    GIORA: {
        type: DataTypes.DATE,
        
    },
    SOGIO: {
        type: DataTypes.DATE,
        
    },
    SOGIOTANGCA: {
        type: DataTypes.DATE,
        
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
    GIOBATDAU: {
        type: DataTypes.DATE,
        
    },
    GIOKETTHUC: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TVAORA',
    tableName: 'TVAORA',
    timestamps: false
});

export default TVAORA;
