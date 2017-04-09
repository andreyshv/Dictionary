import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

import { Card } from './card';
import { CardService } from './card.service';

@Component({
    selector: 'app-card-detail',
    templateUrl: './card-detail.component.html',
    styleUrls: ['./card-detail.component.css']
})
export class CardDetailComponent implements OnInit {
    public card: Card;

    constructor(
        private service: CardService,
        private route: ActivatedRoute
    ) { }

    ngOnInit() {
        this.route.params.forEach((params: Params) => {
            const id = +params['id'];
            if (id > 0) {
                // route = card-detail/:id
                this.service
                    .get(id)
                    .then(value => this.card = value);
            } else {
                // route = new-card
                this.card = new Card;
                this.card.word = params['word'];
                this.card.collectionId = +params['collectionId'];
            }
        });
    }

    save() {
        if (this.card.id) {
            this.service.update(this.card)
                .then(() => this.goBack());
        } else {
            this.service.add(this.card)
                .then(() => this.goBack());
        }
    }

    goBack() {
        window.history.back();
    }
}
