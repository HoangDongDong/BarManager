import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TBANGLUONG extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public thang!: any;
    public nam!: any;
    public chitiet!: any;
}

TBANGLUONG.init({
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
    THANG: {
        type: DataTypes.STRING,
        
    },
    NAM: {
        type: DataTypes.STRING,
        
    },
    CHITIET: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TBANGLUONG',
    tableName: 'TBANGLUONG',
    timestamps: false
});

export default TBANGLUONG;
