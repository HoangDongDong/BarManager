import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SGROUPROLE extends Model {
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public sgroupuserid!: any;
    public sfunctionid!: any;
    public mode!: any;
}

SGROUPROLE.init({
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
    SGROUPUSERID: {
        type: DataTypes.INTEGER,
        
    },
    SFUNCTIONID: {
        type: DataTypes.INTEGER,
        
    },
    MODE: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SGROUPROLE',
    tableName: 'SGROUPROLE',
    timestamps: false
});

export default SGROUPROLE;
