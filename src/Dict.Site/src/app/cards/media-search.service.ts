import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';

import { ImageInfo } from './imageinfo';
// google api keys
import { ApiKeys } from './api-keys';

@Injectable()
export class MediaSearchService {

    private urlTemplate = 'https://www.googleapis.com/customsearch/v1?fields=items&q={q}&cx={cx}&key={key}';

    constructor(private http: Http) { }

    public searchImages(query: string): Promise<ImageInfo[]> {
        if (!query || query == '')
            return null;

        let url = this.urlTemplate.replace('{cx}', ApiKeys.cx)
                .replace('{key}', ApiKeys.ApiKey)
                .replace('{q}', query);

        return this.http.get(url)
            .toPromise()
            .then(response => this.getUrls(response.json()))
            .catch(this.handleError);
    }

    private getUrls(obj: any): ImageInfo[] {
        let urls: ImageInfo[] = [];
        for (let i = 0; i < obj.items.length; i++) {
            let item = obj.items[i];
            let thumbs = item.pagemap.cse_thumbnail;
            if (thumbs) {
                urls.push(new ImageInfo(thumbs[0].src, item.htmlSnippet));
            } 
        }

        return urls;
    }

    private handleError(error: any): Promise<any> {
        console.error('An error occured ', error);
        return Promise.reject(error.message || error);
    }
}