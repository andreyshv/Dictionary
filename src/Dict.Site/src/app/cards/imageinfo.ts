export class ImageInfo {
    url: string;
    tooltip: string;

    constructor (url: string, tooltip: string = '') {
        this.url = url;
        this.tooltip = tooltip;
    }
}
