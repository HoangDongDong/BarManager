import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DBANGGIACHITIET extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public dbanggiaid!: any;
    public usercreatedid!: any;
    public tugio!: any;
    public dengio!: any;
    public sotien!: any;
    public ngayle!: any;
}

DBANGGIACHITIET.init({
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
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    TUGIO: {
        type: DataTypes.DATE,
        
    },
    DENGIO: {
        type: DataTypes.DATE,
        
    },
    SOTIEN: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    NGAYLE: {
        type: DataTypes.DATE,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DBANGGIACHITIET',
    tableName: 'DBANGGIACHITIET',
    timestamps: false
});

export default DBANGGIACHITIET;
