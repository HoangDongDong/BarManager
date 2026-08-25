import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DGIAODIENTHAOTAC extends Model {
    public note!: any;
    public name!: any;
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public sortorder!: any;
    public parentid!: any;
    public parentdir!: any;
    public itemtype!: any;
    public autoid!: any;
    public simageid!: any;
}

DGIAODIENTHAOTAC.init({
    NOTE: {
        type: DataTypes.STRING,
        
    },
    NAME: {
        type: DataTypes.STRING,
        
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
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
    PARENTDIR: {
        type: DataTypes.STRING,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    AUTOID: {
        type: DataTypes.INTEGER,
        
    },
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DGIAODIENTHAOTAC',
    tableName: 'DGIAODIENTHAOTAC',
    timestamps: false
});

export default DGIAODIENTHAOTAC;
