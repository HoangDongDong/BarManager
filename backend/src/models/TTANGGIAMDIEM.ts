import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class TTANGGIAMDIEM extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public ngay!: any;
    public usercreatedid!: any;
    public dkhachhangid!: any;
    public diemtang!: any;
    public diemgiam!: any;
    public lydo!: any;
}

TTANGGIAMDIEM.init({
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
    DKHACHHANGID: {
        type: DataTypes.INTEGER,
        
    },
    DIEMTANG: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    DIEMGIAM: {
        type: DataTypes.DECIMAL(18,2),
        
    },
    LYDO: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'TTANGGIAMDIEM',
    tableName: 'TTANGGIAMDIEM',
    timestamps: false
});

export default TTANGGIAMDIEM;
