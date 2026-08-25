import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DBANGGIATHEOBAN extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public dbanid!: any;
    public usercreatedid!: any;
    public dbanggiaid!: any;
}

DBANGGIATHEOBAN.init({
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
    DBANID: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DBANGGIAID: {
        type: DataTypes.INTEGER,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DBANGGIATHEOBAN',
    tableName: 'DBANGGIATHEOBAN',
    timestamps: false
});

export default DBANGGIATHEOBAN;
