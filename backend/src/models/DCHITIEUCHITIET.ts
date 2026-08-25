import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class DCHITIEUCHITIET extends Model {
    public id!: any;
    public note!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public usercreatedid!: any;
    public dchitieudoanhthuid!: any;
    public dtinhthanhid!: any;
    public chitieu!: any;
}

DCHITIEUCHITIET.init({
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
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    DCHITIEUDOANHTHUID: {
        type: DataTypes.INTEGER,
        
    },
    DTINHTHANHID: {
        type: DataTypes.INTEGER,
        
    },
    CHITIEU: {
        type: DataTypes.DECIMAL(18,2),
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'DCHITIEUCHITIET',
    tableName: 'DCHITIEUCHITIET',
    timestamps: false
});

export default DCHITIEUCHITIET;
