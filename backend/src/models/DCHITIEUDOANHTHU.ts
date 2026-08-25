import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DCHITIEUDOANHTHU extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public dnhomchitieudoanhthuid!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public nam!: any;
    public thang1!: any;
    public thang2!: any;
    public thang3!: any;
    public thang4!: any;
    public thang5!: any;
    public thang6!: any;
    public thang7!: any;
    public thang8!: any;
    public thang9!: any;
    public thang10!: any;
    public thang11!: any;
    public thang12!: any;
}

DCHITIEUDOANHTHU.init({
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
    DNHOMCHITIEUDOANHTHUID: {
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
    NAM: {
        type: DataTypes.STRING,
        
    },
    THANG1: {
        type: DataTypes.STRING,
        
    },
    THANG2: {
        type: DataTypes.STRING,
        
    },
    THANG3: {
        type: DataTypes.STRING,
        
    },
    THANG4: {
        type: DataTypes.STRING,
        
    },
    THANG5: {
        type: DataTypes.STRING,
        
    },
    THANG6: {
        type: DataTypes.STRING,
        
    },
    THANG7: {
        type: DataTypes.STRING,
        
    },
    THANG8: {
        type: DataTypes.STRING,
        
    },
    THANG9: {
        type: DataTypes.STRING,
        
    },
    THANG10: {
        type: DataTypes.STRING,
        
    },
    THANG11: {
        type: DataTypes.STRING,
        
    },
    THANG12: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DCHITIEUDOANHTHU',
    tableName: 'DCHITIEUDOANHTHU',
    timestamps: false
});

export default DCHITIEUDOANHTHU;
