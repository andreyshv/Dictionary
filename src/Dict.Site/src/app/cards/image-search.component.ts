import { Component, OnInit, Input } from '@angular/core';

import { MediaSearchService } from './media-search.service';
import { Card } from './card';
import { ImageInfo } from './imageinfo';

@Component({
    selector: 'app-image-search',
    templateUrl: 'image-search.component.html',
    styleUrls: ['image-search.component.css']
})
export class ImageSearchComponent implements OnInit {
    @Input() card: Card;

    public imageInfos: ImageInfo[];

    constructor(private service: MediaSearchService) { }

    ngOnInit() {
    }

    search(query: string) {
        if (!query || query.length === 0) {
            return;
        }
        // this.service.searchImages(query)
        //     .then(value => { this.imageInfos = value });

        // -- test --

        const urls = [
            'https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcTWntCD6H8HD-JmxVTFvXyX7RUUOXw874NCtsNQ5th3PsfAxNvNPgZ-ky4'
        ];

        const infos: ImageInfo[] = [];
        for (let i = 0; i < urls.length; i++) {
            const info = new ImageInfo(urls[i], (i % 2 === 0) ? `URL: ${urls[i]}` : null);
            infos.push(info);
        }

        this.imageInfos = infos;
    }

    select(url: string) {
        this.card.imageURL = url;
    }
}
