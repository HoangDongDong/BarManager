import { Model, DataTypes } from 'sequelize';
import db from '../config/database';

class SQUICKNOTE extends Model {
    public id!: any;
    public status!: any;
    public usermodifiedid!: any;
    public timemodified!: any;
    public timecreated!: any;
    public sortorder!: any;
    public usercreatedid!: any;
    public notedata!: any;
}

SQUICKNOTE.init({
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
    SORTORDER: {
        type: DataTypes.INTEGER,
        
    },
    USERCREATEDID: {
        type: DataTypes.INTEGER,
        
    },
    NOTEDATA: {
        type: DataTypes.STRING,
        
    },
}, {
    sequelize: db.sequelize,
    modelName: 'SQUICKNOTE',
    tableName: 'SQUICKNOTE',
    timestamps: false
});

export default SQUICKNOTE;
