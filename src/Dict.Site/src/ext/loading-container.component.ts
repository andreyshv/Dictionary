import { Component, Input } from '@angular/core';

@Component({
    selector: 'loading-container',
    templateUrl: 'loading-container.component.html'
})
export class LoadingContainerComponent {
    @Input() loading: boolean;
    constructor() { }
}

export class LoadingPage {
    public loading: boolean;
    constructor(val: boolean) {
        this.loading = val;
    }
    standby() {
        this.loading = true;
    }
    ready() {
        this.loading = false;
    }
}