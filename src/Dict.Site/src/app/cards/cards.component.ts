import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { Card } from './card';
import { Collection } from '../collection';
import { CardService } from './card.service';
import { CollectionService } from '../collection.service';
import { AppConfigService } from '../app.config.service';
import { LoadingContainerComponent, LoadingPage } from '../../ext/loading-container.component';

@Component({
    //moduleId: module.id,
    selector: 'my-cards',
    templateUrl: 'cards.component.html',
    styleUrls: ['cards.component.scss']
})
export class CardsComponent extends LoadingPage implements OnInit {
    collection: Collection;
    cards: Card[];

    constructor(
        private service: CardService,
        private colService: CollectionService,
        private config: AppConfigService,
        private router: Router
    ) {
        super(true);
    }

    ngOnInit() {
        let collectionId = this.config.getCollectionId();
        if (collectionId) {
            this.colService.get(collectionId)
                .then(value => this.collection = value);

            this.service.getCards(collectionId, 0)
                .then(value => {
                    this.cards = value; 
                    this.ready();
                });
        }
    }

    onScrollDown() {
        if (this.collection !== null && !this.loading) {
            this.standby();
            this.service.getCards(this.collection.id, this.cards.length)
                .then(value => {
                    this.cards = this.cards.concat(value);
                    this.ready();
                });
        }
    }

    edit(cardId: number) {
        this.router.navigate(['/card-detail', cardId]);
    }

    add(word: string) {
        this.router.navigate(['/new-card', { collectionId: this.collection.id, word: word }]);
    }

    play(soundURL: string) {
        let audio = new Audio(soundURL);
        audio.play();
    }
}