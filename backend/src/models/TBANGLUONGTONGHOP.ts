import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TBANGLUONGTONGHOP extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dnhanvienid!: any;
    public luongca!: any;
    public tongluong!: any;
    public phat!: any;
    public tamung!: any;
    public thucnhan!: any;
    public thuong!: any;
    public luongthang!: any;
    public cachtinhluong!: any;
    public tbangluongid!: any;
    public sogiolam!: any;
    public sogiotangca!: any;
    public luongtheoca!: any;
}

TBANGLUONGTONGHOP.init({
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
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
    DNHANVIENID: {
        type: DataTypes.INTEGER,
        
    },
    LUONGCA: {
        type: DataTypes.STRING,
        
    },
    TONGLUONG: {
        type: DataTypes.STRING,
        
    },
    PHAT: {
        type: DataTypes.STRING,
        
    },
    TAMUNG: {
        type: DataTypes.STRING,
        
    },
    THUCNHAN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    THUONG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    LUONGTHANG: {
        type: DataTypes.STRING,
        
    },
    CACHTINHLUONG: {
        type: DataTypes.STRING,
        
    },
    TBANGLUONGID: {
        type: DataTypes.INTEGER,
        
    },
    SOGIOLAM: {
        type: DataTypes.DATE,
        
    },
    SOGIOTANGCA: {
        type: DataTypes.DATE,
        
    },
    LUONGTHEOCA: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TBANGLUONGTONGHOP',
    tableName: 'TBANGLUONGTONGHOP',
    timestamps: false
});

export default TBANGLUONGTONGHOP;
