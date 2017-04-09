import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import { Collection } from './collection';

@Injectable()
export class CollectionService {
    private apiUrl = 'api/collections';
    private headers: Headers = new Headers({ 'Content-Type': 'application/json' });

    constructor(
        private http: Http
    ) { }

    getAll(): Promise<Collection[]> {
        return this.http
            .get(this.apiUrl)
            .toPromise()
            .then(response => response.json() as Collection[])
            .catch(this.handleError);
    }

    get(id: number): Promise<Collection> {
        return this.http
            .get(`${this.apiUrl}/${id}`)
            .toPromise()
            .then(response => response.json() as Collection)
            .catch(this.handleError);
    }

    create(name: string, description: string): Promise<Collection> {
        const data = JSON.stringify({ name: name, description: description });
        return this.http
            .post(this.apiUrl, data, { headers: this.headers })
            .toPromise()
            .then(response => response.json() as Collection)
            .catch(this.handleError);
    }

    update(collection: Collection): Promise<Collection> {
        return this.http
            .put(`${this.apiUrl}/${collection.id}`, JSON.stringify(collection), { headers: this.headers })
            .toPromise()
            .then(() => collection)
            .catch(this.handleError);
    }

    delete(id: number): Promise<void> {
        return this.http
            .delete(`${this.apiUrl}/${id}`, { headers: this.headers })
            .toPromise()
            .then(() => null)
            .catch(this.handleError);
    }

    // TODO implement error handling
    private handleError(error: any): Promise<any> {
        console.error('An error occured ', error);
        return Promise.reject(error.message || error);
    }
}
