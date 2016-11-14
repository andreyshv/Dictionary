import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import { Card } from './card';

@Injectable()
export class CardService {
    private cardsUrl = 'api/cards';
    private collectionUrl = 'api/cards/collection';
    private queueUrl = 'api/queues';
    private headers: Headers = new Headers({ 'Content-Type': 'application/json' });

    constructor(
        private http: Http
    ) { }

    getQueue(collectionId: number): Promise<Card[]> {
        let url = `${this.queueUrl}/${collectionId}`;
        return this.http
            .get(url)
            .toPromise()
            .then(response => response.json() as Card[])
            .catch(this.handleError);
    }

    getCards(collectionId: number, first: number): Promise<Card[]> {
        let url = `${this.collectionUrl}/${collectionId}?first=${first}&count=20`;
        return this.http
            .get(url)
            .toPromise()
            .then(response => response.json() as Card[])
            .catch(this.handleError);
    }

    get(cardId: number): Promise<Card> {
        let url = `${this.cardsUrl}/${cardId}`;
        return this.http
            .get(url)
            .toPromise()
            .then(response => response.json() as Card)
            .catch(this.handleError);
    }

    add(card: Card): Promise<Card> {
        return this.http
            .post(this.cardsUrl, JSON.stringify(card), { headers: this.headers })
            .toPromise()
            .then(() => card)
            .catch(this.handleError);
    }
    
    update(card: Card): Promise<Card> {
        return this.http
            .put(`${this.cardsUrl}/${card.id}`, JSON.stringify(card), { headers: this.headers })
            .toPromise()
            .then(() => card)
            .catch(this.handleError);
    }

    //TODO implement error handling
    private handleError(error: any): Promise<any> {
        console.error('An error occured ', error);
        return Promise.reject(error.message || error);
    }
}