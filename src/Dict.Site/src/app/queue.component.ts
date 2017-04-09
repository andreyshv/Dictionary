import { Component, OnInit } from '@angular/core';

import { Card } from './cards/card';
import { CardService } from './cards/card.service';
import { AppConfigService } from './app.config.service';

@Component({
    selector: 'app-queue',
    templateUrl: 'queue.component.html'
})
export class QueueComponent implements OnInit {
    cards: Card[];

    constructor(
        private service: CardService,
        private config: AppConfigService
    ) { }

    ngOnInit() {
        const collectionId = this.config.getCollectionId();
        if (collectionId) {
            this.service.getQueue(collectionId)
                .then(value => this.cards = value);
        }
    }
}
