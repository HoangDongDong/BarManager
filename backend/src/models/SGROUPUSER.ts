import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SGROUPUSER extends Model {
    public id!: any;
    public name!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public simageid!: any;
    public itemtype!: any;
    public parentdir!: any;
    public parentid!: any;
    public sortorder!: any;
    public autoid!: any;
}

SGROUPUSER.init({
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
    SIMAGEID: {
        type: DataTypes.INTEGER,
        
    },
    ITEMTYPE: {
        type: DataTypes.STRING,
        
    },
    PARENTDIR: {
        type: DataTypes.STRING,
        
    },
    PARENTID: {
        type: DataTypes.INTEGER,
        
    },
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    AUTOID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SGROUPUSER',
    tableName: 'SGROUPUSER',
    timestamps: false
});

export default SGROUPUSER;
