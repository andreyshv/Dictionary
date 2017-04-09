import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { Collection } from './collection';
import { CollectionService } from './collection.service';
import { AppConfigService } from './app.config.service';


@Component({
    selector: 'app-collections',
    templateUrl: 'collections.component.html',
    styleUrls: ['collections.component.css']
})
export class CollectionsComponent implements OnInit {
    private collections: Collection[];
    private activeItem: Collection;

    constructor(
        private service: CollectionService,
        private config: AppConfigService,
        private router: Router
        ) { }

    ngOnInit() {
        this.service.getAll()
            .then(data => {
                this.collections = data;

                const id = this.config.getCollectionId();
                this.activeItem = this.collections.find(c => c.id === id);
            });
    }

    onSelect(selected: Collection) {
        this.activeItem = selected;
        this.config.setCollectionId(selected.id);
    }

    add(name: string, description: string) {
        name = name.trim();
        if (!name) {
            return;
        }

        this.service.create(name, description.trim())
            .then(item => {
                this.collections.push(item);
                // TODO select active!
                this.activeItem = null;
            });
    }

    delete(item: Collection) {
        this.service
            .delete(item.id)
            .then(() => {
                this.collections = this.collections.filter(c => c !== item);
                if (this.activeItem === item) {
                    // TODO select active!
                    this.activeItem = null;
                }
            });
    }

    gotoDetail() {
        this.router.navigate(['/collection-detail', this.activeItem.id]);
    }
}
