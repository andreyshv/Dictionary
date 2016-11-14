import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';

import { CollectionService } from './collection.service';
import { Collection } from './collection';

@Component({
    selector: 'my-collection-detail',
    templateUrl: './collection-detail.component.html',
    styleUrls: ['./collection-detail.component.css']
})
export class CollectionDetailComponent implements OnInit {
    collection: Collection;

    constructor(
        private service: CollectionService,
        private route: ActivatedRoute) {

    }

    ngOnInit(): void {
        this.route.params.forEach((params: Params) => {
            let id = +params['id'];
            this.service
                .get(id)
                .then(value => this.collection = value);
        })
    }

    goBack(): void {
        window.history.back();
    }

    save(): void {
        this.service.update(this.collection)
            .then(() => this.goBack());
    }
}

