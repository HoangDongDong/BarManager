import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TSOHOADON extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public so!: any;
}

TSOHOADON.init({
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
    SO: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TSOHOADON',
    tableName: 'TSOHOADON',
    timestamps: false
});

export default TSOHOADON;
