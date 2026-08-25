import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SREPORTROLE extends Model {
    public status!: any;
    public mode!: any;
    public timemodified!: any;
    public timecreated!: any;
    public id!: any;
    public usermodifiedid!: any;
    public usercreatedid!: any;
    public sgroupuserid!: any;
    public sreportid!: any;
}

SREPORTROLE.init({
    STATUS: {
        type: DataTypes.BOOLEAN,
        
    },
    MODE: {
        type: DataTypes.STRING,
        
    },
    TIMEMODIFIED: {
        type: DataTypes.DATE,
        
    },
    TIMECREATED: {
        type: DataTypes.DATE,
        
    },
    ID: {
        type: DataTypes.INTEGER,
        primaryKey: true, autoIncrement: true,
    },
    USERMODIFIEDID: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    SGROUPUSERID: {
        type: DataTypes.INTEGER,
        
    },
    SREPORTID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SREPORTROLE',
    tableName: 'SREPORTROLE',
    timestamps: false
});

export default SREPORTROLE;
