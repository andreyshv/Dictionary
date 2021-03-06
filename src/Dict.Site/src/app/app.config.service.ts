import { Injectable } from '@angular/core';

import { CookieService } from 'ngx-cookie';

@Injectable()
export class AppConfigService {
    private ACTIVE_COLLECTION = 'activeCollection';

    constructor(
        private cookieService: CookieService
    ) { }

    get(key: string): string {
        return this.cookieService.get(key);
    }

    // TODO check put args
    set(key: string, value: string) {
        this.cookieService.put(key, value);
    }

    getCollectionId(): number {
        const id = this.get(this.ACTIVE_COLLECTION);
        return (id) ? +id : null;
    }

    setCollectionId(collectionId: number) {
        return this.set(this.ACTIVE_COLLECTION, collectionId.toString());
    }
}
