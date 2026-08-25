import { SUSER } from '../models';

export interface BaseAccount {
    getProfile(): any;
}

export class AdminAccount implements BaseAccount {
    constructor(private data: any) {}
    getProfile() {
        return {
            id: this.data.id,
            ten_dang_nhap: this.data.ten_dang_nhap,
            vai_tro: 'Admin',
            permissions: ['ALL']
        };
    }
}

export class StaffAccount implements BaseAccount {
    constructor(private data: any) {}
    getProfile() {
        return {
            id: this.data.id,
            ten_dang_nhap: this.data.ten_dang_nhap,
            vai_tro: 'Staff',
            cua_hang_id: this.data.cua_hang_id,
            permissions: ['POS_VIEW', 'ORDER_CREATE']
        };
    }
}

export class AccountFactory {
    public static createAccount(data: any): BaseAccount {
        if (data.vai_tro === 1 || data.ISADMIN) {
            return new AdminAccount(data);
        } else {
            return new StaffAccount(data);
        }
    }
}
